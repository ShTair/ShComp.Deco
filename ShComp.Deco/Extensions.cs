using System.Text;

namespace ShComp.Deco;

internal static class Extensions
{
    public static string ToXString(this in Span<byte> buffer)
    {
        var sb = new StringBuilder(buffer.Length * 2);
        foreach (byte b in buffer) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static string ToXString(this byte[] buffer) => ToXString(buffer.AsSpan());

    public static byte[] FromXString(this string s)
    {
        static int ToInt(int c) => c switch { <= 57 => c - 48, <= 70 => c - 55, _ => c - 87 };

        var buffer = new byte[s.Length / 2];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)((ToInt(s[i * 2]) << 4) + ToInt(s[i * 2 + 1]));
        }

        return buffer;
    }
}
