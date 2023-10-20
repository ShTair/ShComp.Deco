using System.Text;

namespace ShComp.Deco;

public static class Utils
{
    public static string? DecodeName(string? name) => name is null ? null : DecodeName(Convert.FromBase64String(name));

    public static string? DecodeName(byte[]? nameData) => nameData is null ? null : Encoding.UTF8.GetString(nameData);
}
