using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DataReaderExtensions
{
    public static int GetInt32Value(this DbDataReader reader, string name) => Convert.ToInt32(reader[name]);
    public static string GetStringValue(this DbDataReader reader, string name) => Convert.ToString(reader[name])!;
    public static string GetNullableString(this DbDataReader reader, string name) => reader[name] is DBNull ? string.Empty : Convert.ToString(reader[name])!;
    public static byte[] GetBytesValue(this DbDataReader reader, string name) => (byte[])reader[name];
    public static byte[]? GetNullableBytes(this DbDataReader reader, string name) => reader[name] is DBNull ? null : (byte[])reader[name];
    public static DateOnly GetDateOnlyValue(this DbDataReader reader, string name) => DateOnly.FromDateTime(Convert.ToDateTime(reader[name]));
}
