namespace sample.gateway.Models;

public struct EntitlementId : IEquatable<EntitlementId>, IComparable<EntitlementId>
{
    private string _value;

    public static readonly EntitlementId Empty = (EntitlementId)string.Empty;

    public EntitlementId(string id)
    {
        _value = id;
    }

    public override bool Equals(object obj)
    {
        if (obj is EntitlementId)
        {
            return Equals((EntitlementId)obj);
        }
        if (obj is string)
        {
            return Equals((string)obj);
        }
        if (obj == null)
        {
            return _value == null;
        }
        return false;
    }
    public bool Equals(string x)
    {
        return _value == x;
    }
    public bool Equals(EntitlementId x)
    {
        return _value == x._value;
    }
    public override int GetHashCode()
    {
        if (_value == null)
        {
            return 0;
        }
        return _value.GetHashCode();
    }
    public override string ToString()
    {
        return _value;
    }
    public string ToLowerString()
    {
        return _value.ToLowerInvariant();
    }
    public static bool TryParse(string input, out EntitlementId result)
    {
        result = new EntitlementId
        {
            _value = input
        };
        return true;
    }
    public static bool operator ==(EntitlementId x, EntitlementId y)
    {
        return x._value == y._value;
    }
    public static bool operator !=(EntitlementId x, EntitlementId y)
    {
        return x._value != y._value;
    }
    public static bool operator >=(EntitlementId x, EntitlementId y)
    {
        return x._value.CompareTo(y._value) >= 0;
    }
    public static bool operator <=(EntitlementId x, EntitlementId y)
    {
        return x._value.CompareTo(y._value) <= 0;
    }
    public static implicit operator string(EntitlementId x)
    {
        return x._value;
    }
    public static explicit operator EntitlementId(string x)
    {
        return new EntitlementId
        {
            _value = x
        };
    }

    public readonly int CompareTo(EntitlementId other)
    {
        if (string.IsNullOrWhiteSpace(other))
        {
            return 1;
        }

        return _value.CompareTo(other._value);
    }
}