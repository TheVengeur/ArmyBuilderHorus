namespace ArmyBuilderHorus.Services;

public static class KeyStore
{
    private const string KeyName = "club-passphrase";
    public static Task SaveAsync(string pass) => SecureStorage.SetAsync(KeyName, pass);
    public static Task<string?> LoadAsync() => SecureStorage.GetAsync(KeyName);
}
