using Masuit.Tools;

namespace HJHTools.Entity.Global;

/// <summary>
/// 可比较数组
/// </summary>
public class ArrayComparer<T> : IEqualityComparer<T[]>
{
    public bool Equals(T[]? x, T[]? y) {
        if (x == null || y == null) return x == y;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(T[]? obj) {
        return obj == null
            ? 0
            : obj.Aggregate(17,
                (current, b) => current * 31 + (b == null ? 0 : EqualityComparer<T>.Default.GetHashCode(b)));
    }

    public override string ToString() {
        return this.ToJsonString();
    }
}