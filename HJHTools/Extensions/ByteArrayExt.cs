using System.Text;

namespace HJHTools.Extensions;

public static class ByteArrayExt
{
    /// <summary>
    /// byte 数组转 ASCII 码值
    /// </summary>
    /// <param name="bytes">Byte数组</param>
    /// <returns>ASCII 码字符串</returns>
    public static string ToAscii(this byte[] bytes) {
        return Encoding.ASCII.GetString(bytes);
    }

    /// <summary>
    /// byte 数组转 int（十进制）
    /// </summary>
    /// <param name="bytes">原数组</param>
    /// <returns>int 类型的值</returns>
    public static int ToInt(this byte[] bytes) {
        return bytes.Aggregate(0, (current, b) => (current << 8) + b);
    }

    /// <summary>
    /// byte 数组转 int 数组（十进制）
    /// </summary>
    /// <param name="bytes">原数组</param>
    /// <returns>int 数组</returns>
    public static int[] ToIntArray(this byte[] bytes) {
        return bytes.Select(b => (int)b).ToArray();
    }
}