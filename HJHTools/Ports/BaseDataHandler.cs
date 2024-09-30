using HJHTools.Extensions;
using NLog;

namespace HJHTools.Ports;

public static class BaseDataHandler
{
    // ReSharper disable once UnusedMember.Local
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly List<byte[]> ReceivedBuffer = new();
    private static readonly object Lock = new();
    
    // 头尾、检验
    public static readonly byte[] HeaderByte = { 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x68 };
    public static readonly byte[] CheckByte = { 0xD0 };
    public static readonly byte[] TailByte = { 0x16 };
    private const byte DefaultReceiveTailByte = 0x16;

    public static byte[] DecodeData(byte[] partialData) {
        lock (Lock)
        {
            // 将新接收到的分包数据添加到缓冲区
            ReceivedBuffer.Add(partialData);

            // 将所有分包拼接成一个大的 byte[]
            var combinedData = ReceivedBuffer.SelectMany(b => b).ToArray();

            // 检查是否接收到完整的数据包（假设以 0xFE 或者 0x68 开头，以 0x16 结束）
            if (combinedData.IsNotNullOrEmpty() && (combinedData[0] == 0xFE || combinedData[0] == 0x68))
            {
                combinedData = RemoveLeadingFe(combinedData);
                var indices = combinedData.FindIndices((byte)0x16);
                if (indices.IsEmpty()) return Array.Empty<byte>();
                foreach (var data in indices.Select(t => combinedData.Take(t + 1).ToArray()))
                {
                    // 判断是否合法数据
                    if (!IsReceive(data)) continue;
                    // 清空缓冲区
                    ReceivedBuffer.Clear();
                    return data;
                }
            }
            else
            {
                // 清空缓冲区
                ReceivedBuffer.Clear();
            }

            return Array.Empty<byte>();
        }
    }

    private static byte[] RemoveLeadingFe(byte[] data) {
        // 找到第一个不是 0xFE 的索引
        var firstNonFeIndex = Array.FindIndex(data, b => b != 0xFE);

        // 如果数组中全是 0xFE，返回一个空数组，否则截取剩余部分
        return firstNonFeIndex == -1 ? Array.Empty<byte>() : data.Skip(firstNonFeIndex).ToArray();
    }

    private static bool IsReceive(byte[] data) {
        return data.IsNotNull() && data.Length > 11 && data.Last() == DefaultReceiveTailByte
               && data.Take(8).ToArray().SequenceEqual(HeaderByte)
               && data[9].ToInt32() == data.Length - 12;
    }
}