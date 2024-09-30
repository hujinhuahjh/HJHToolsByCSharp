using System.Collections.ObjectModel;
using System.IO.Ports;
using HJHTools.Entity.Enums;
using HJHTools.Event;
using Masuit.Tools;
using Nethereum.Hex.HexConvertors.Extensions;
using NLog;

namespace HJHTools.Ports;

#pragma warning disable CS8602 // 解引用可能出现空引用。
public static class SerialPortUtil
{
    public delegate void ErrorHandler(string message);

    /// <summary>
    /// <p>example:</p>
    ///     SerialPortUtil.OnError += message => MessageBox.Show($"HandlerA received: {message}");
    /// </summary>
    public static event ErrorHandler? OnError;

    private static byte[] _currCmd = Array.Empty<byte>();
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly ManualResetEvent ManualResetEvent = new(false);
    private static int _waitTime = 1 * 1000; // 超时时间，单位毫秒

    private static SerialPort? _serialPort;
    private static int _retryCount;

    private static string? SelectSerialPortName { get; set; }
    private static bool IsOpenPort { get; set; }

    /// <summary>
    /// 设置串口实例
    /// </summary>
    public static void Init(SerialPort serialPort) {
        _serialPort = serialPort;
        EventBus.Subscribe<CmdExecuteResultEnum>(OnReSendExecuted);
    }

    /// <summary>
    /// 刷新串口列表
    /// </summary>
    public static void RefreshSerialPortList(ObservableCollection<string> serialPortNames) {
        var ports = SerialPort.GetPortNames();
        serialPortNames.Clear();
        foreach (var port in ports)
        {
            serialPortNames.Add(port); // 逐个添加新的端口名
        }

        if (SelectSerialPortName != null && ports.Contains(SelectSerialPortName)) return;
        IsOpenPort = false;
        Reset();
    }

    /// <summary>
    /// 打开串口
    /// </summary>
    public static void OpenPort(string portName, int baudRate, int dataBit, string parity,
        SerialDataReceivedEventHandler dataReceivedHandler) {
        if (_serialPort == null)
        {
            throw new InvalidOperationException("SerialPort is not initialized.");
        }

        try
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBit;
            _serialPort.Parity = GetParity(parity);
            _serialPort.Open();
            _serialPort.DataReceived += dataReceivedHandler;
            IsOpenPort = true;
            SelectSerialPortName = portName;
        }
        catch (Exception e)
        {
            IsOpenPort = false;
            SelectSerialPortName = null;
            NotifyError($"串口打开失败：{e.Message}");
            Reset();
        }
    }

    /// <summary>
    /// 关闭串口
    /// </summary>
    public static void ClosePort(SerialDataReceivedEventHandler dataReceivedHandler) {
        if (_serialPort == null || !IsWorking()) return;
        _serialPort.DataReceived -= dataReceivedHandler;
        _serialPort.Close();
        IsOpenPort = false;
    }

    /// <summary>
    /// 异步接收数据
    /// </summary>
    public static async Task<byte[]> Receive(Func<byte[], byte[]> decodeDataFunc) {
        if (!IsWorking()) return Array.Empty<byte>();

        return await Task.Run(() =>
        {
            if (_serialPort == null) return Array.Empty<byte>();

            var byteLen = _serialPort.BytesToRead;
            var byteArray = new byte[byteLen];
            _serialPort.Read(byteArray, 0, byteArray.Length);

            // 使用传入的解码函数
            var bytes = decodeDataFunc(byteArray);
            ManualResetEvent.Reset();
            return bytes.Length == 0 ? Array.Empty<byte>() : bytes;
        });
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    public static void Send(byte[] bytes) {
        _currCmd = bytes;
        if (bytes.IsNullOrEmpty()) return;
        if (!IsWorking())
        {
            Reset();
            NotifyError("Serial port is not open.");
            return;
        }

        Logger.Info($"Send command => {bytes.ToHex()}");
        ManualResetEvent.Reset();
        _serialPort?.Write(bytes, 0, bytes.Length);

        if (ManualResetEvent.WaitOne(_waitTime))
        {
            Logger.Error($"Execution not timed out --- {_currCmd.ToHex()}");
            _retryCount = 0;
        }
        else
        {
            Logger.Error($"Execution has timed out --- {_currCmd.ToHex()}");
            HandleRetry(); // 只有当超时标记时才处理重试
        }
    }

    /// <summary>
    /// 发送字符串数据
    /// </summary>
    public static void Send(string text) {
        if (IsWorking())
        {
            Logger.Info($"Send command ==> {text}");
            _serialPort?.Write(text);
        }
        else
        {
            Reset();
            NotifyError("Serial port is not open.");
        }
    }

    /// <summary>
    /// 释放串口资源，并解除事件订阅
    /// </summary>
    public static void Release() {
        // 如果串口对象已经打开，关闭它
        if (IsWorking())
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }

        // 重置串口对象
        _serialPort = null;
        // 重置打开标志
        IsOpenPort = false;
        SelectSerialPortName = null;
        // 解除事件订阅
        OnError = null;
        // 重置状态
        Reset();
        Logger.Info("Serial port has been released.");
    }

    /// <summary>
    /// 重置状态
    /// </summary>
    public static void Reset() {
        _waitTime = 1 * 1000;
        _retryCount = 0;
        ManualResetEvent.Reset();
        _currCmd = Array.Empty<byte>();
    }

    /// <summary>
    /// 判断 SerialPort 对象是否为空，且串口是否打开
    /// </summary>
    public static bool IsWorking() {
        return _serialPort is { IsOpen: true } && IsOpenPort;
    }

    /// <summary>
    /// 接收到数据响应是调用此方法，用以解除 Send 方法超时检测
    /// </summary>
    public static void HasReceivedData() {
        ManualResetEvent.Set();
    }

    /// <summary>
    /// 设置方法超时时间, 默认 1000 ms
    /// </summary>
    public static void SetWaitTime(int waitTime) {
        _waitTime = waitTime;
    }

    /// <summary>
    /// 获取串口校验位
    /// </summary>
    private static Parity GetParity(string parity) {
        return parity switch
        {
            "Odd" => Parity.Odd,
            "Even" => Parity.Even,
            "Mark" => Parity.Mark,
            "Space" => Parity.Space,
            _ => Parity.None
        };
    }

    /// <summary>
    /// 处理重试机制
    /// </summary>
    private static void HandleRetry() {
        _retryCount++;
        if (_retryCount >= 3)
        {
            Logger.Error("Execution failed after 3 retries.");
            EventBus.Publish(CmdExecuteResultEnum.Timeout);
            return;
        }

        if (_currCmd.Length <= 0) return;

        Logger.Warn($"Retrying Send command ==> {_currCmd.ToHex()}");
        EventBus.Publish(CmdExecuteResultEnum.Retry);
    }

    /// <summary>
    /// 超时重发
    /// </summary>
    private static void OnReSendExecuted(CmdExecuteResultEnum result) {
        if (result == CmdExecuteResultEnum.Retry)
            Send(_currCmd);
    }

    /// <summary>
    /// 替代 MessageBox 的方法，用事件触发
    /// </summary>
    private static void NotifyError(string message) {
        OnError?.Invoke(message);
        Logger.Error(message); // 保持日志记录
    }
}