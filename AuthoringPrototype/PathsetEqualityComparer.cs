using Kibali;

namespace AuthoringPrototype;

public class PathsetEqualityComparer : IEqualityComparer<PathSet>
{
    public bool Equals(PathSet x, PathSet y)
    {
        if (x == null || y == null)
            return false;
        var matched = x.Methods.OrderBy(s => s).SequenceEqual(y.Methods.OrderBy(s => s)) && x.SchemeKeys.OrderBy(s => s).SequenceEqual(y.SchemeKeys.OrderBy(s => s));
        return matched;
    }

    public int GetHashCode(PathSet obj)
    {
        if (obj == null)
            return 0;

        return obj.Methods.GetHashCode() ^ obj.SchemeKeys.GetHashCode();
    }
}
