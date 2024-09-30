using System.IO.Ports;
using HJHTools.Event;
using HJHTools.Ports;
using NLog;

namespace HJHTools;

/// <summary>
/// 通用配置
/// </summary>
public class Global
{
    /// <summary>
    /// 基础初始化
    /// </summary>
    public static void Init(IEventAggregator? eventAggregator = null, SerialPort? serialPort = null) {
        LogManager.Setup().LoadConfigurationFromFile("NLog.config");
        if (eventAggregator != null) InitEventBus(eventAggregator);
        if (serialPort != null) InitSerialPortUtil(serialPort);
    }

    /// <summary>
    /// 初始化 EventBus
    /// </summary>
    /// <param name="eventAggregator">事件聚合器</param>
    public static void InitEventBus(IEventAggregator eventAggregator) {
        EventBus.Init(eventAggregator);
    }

    /// <summary>
    /// <p>初始化 SerialPortUtil</p> 
    /// var container = new Container();
    /// _serialPort = new SerialPort(container);
    /// </summary>
    /// <param name="serialPort">SerialPort 对象</param>
    public static void InitSerialPortUtil(SerialPort serialPort) {
        SerialPortUtil.Init(serialPort);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public static void Release() {
        EventBus.UnAllSubscriptions();
        SerialPortUtil.Release();
    }
}