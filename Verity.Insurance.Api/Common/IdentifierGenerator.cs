namespace Verity.Insurance.Api.Common;

internal static class IdentifierGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string New() => string.Concat(
        Enumerable.Range(0, 16).Select(_ => Alphabet[Random.Shared.Next(Alphabet.Length)]));
}
