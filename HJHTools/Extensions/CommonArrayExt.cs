using System.Collections.ObjectModel;

namespace HJHTools.Extensions;

public static class CommonArrayExt
{
    /// <summary>
    /// 将源字节数组 <paramref name="source"/> 复制到目标字节数组 <paramref name="destination"/>，
    /// 并从指定偏移量 <paramref name="offset"/> 开始。
    /// </summary>
    /// <param name="source">要复制的源字节数组。如果为 <c>null</c>，则不执行复制操作。</param>
    /// <param name="destination">目标字节数组。如果为 <c>null</c>，则不执行复制操作。</param>
    /// <param name="offset">目标数组中开始复制的偏移量。</param>
    /// <returns>返回新的偏移量，它是 <paramref name="offset"/> 加上源数组的长度。</returns>
    public static int CopyArraySegment(this byte[]? source, byte[]? destination, int offset) {
        if (source == null) return offset;
        if (destination != null) Buffer.BlockCopy(source, 0, destination, offset, source.Length);
        return offset + source.Length; // 返回新的偏移量
    }
    
    /// <summary>
    /// 泛型方法，用于查找指定元素的所有索引
    /// </summary>
    /// <param name="array">原数组</param>
    /// <param name="value">查找值</param>
    /// <returns>索引集合</returns>
    public static List<int> FindIndices<T>(this T[] array, T value) {
        var indices = new List<int>();
        for (var i = 0; i < array.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(array[i], value))
            {
                indices.Add(i);
            }
        }

        return indices;
    }
    
    /// <summary>
    /// 根据开始索引和结束索引截取数组（不包含结束索引）。
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="array">原数组</param>
    /// <param name="startIndex">开始索引（包含）</param>
    /// <param name="endIndex">结束索引（不包含）</param>
    /// <returns>截取后的数组</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果索引不合法</exception>
    public static T[] SubArray<T>(this T[] array, int startIndex, int? endIndex = null)
    {
        // 检查数组是否为 null
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");
        }

        int end;
        // 为空或者大于数组总长度就截取开始至数组末数据
        if (endIndex == null || endIndex > array.Length)
        {
            end = array.Length;
        }
        else
        {
            end = endIndex.Value;
        }

        // 检查索引范围
        if (startIndex < 0 || startIndex >= endIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex),
                $"Invalid start or end index: startIndex = {startIndex}, endIndex = {endIndex}, array length = {array.Length}.");
        }

        // 计算截取的长度
        var length = end - startIndex;
        var result = new T[length];
        // 复制数组中的元素
        Array.Copy(array, startIndex, result, 0, length);
        return result;
    }
    
    /// <summary>
    /// 获取数组长度，null 或空时为0
    /// </summary>
    /// <param name="array">原数组</param>
    /// <returns>数组长度</returns>
    public static int ToLengthOrDefault<T>(this T[]? array) {
        return array?.Length ?? 0;
    }
    
    /// <summary>
    /// 转集合 List
    /// </summary>
    /// <param name="array">原数组</param>
    /// <returns>集合</returns>
    public static List<T> ToList<T>(this T[] array) {
        return new List<T>(array);
    }
    
    /// <summary>
    /// 转可观察集合 ObservableCollection
    /// </summary>
    /// <param name="array">原数组</param>
    /// <returns>可观察集合</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this T[] array) {
        return new ObservableCollection<T>(array);
    }
    
    /// <summary>
    /// 获取字符串，以分隔符 delimiter 隔开
    /// </summary>
    /// <param name="arr">原数组</param>
    /// <param name="delimiter">分隔符 默认：','</param>
    /// <returns>字符串</returns>
    public static string ToDelimiterString<T>(this T[] arr, string delimiter = ",") {
        return string.Join(delimiter, arr);
    }
}