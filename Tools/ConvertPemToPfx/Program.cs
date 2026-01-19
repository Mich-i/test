using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

if (args.Length < 3)
{
    Console.WriteLine("Usage: dotnet run --project Tools/ConvertPemToPfx <cert.pem> <key.pem> <out.pfx> [password]");
    return 1;
}

string certPath = args[0];
string keyPath = args[1];
string outPfx = args[2];
string password = args.Length >= 4 ? args[3] : string.Empty;

if (!File.Exists(certPath))
{
    Console.WriteLine($"Certificate file not found: {certPath}");
    return 2;
}

if (!File.Exists(keyPath))
{
    Console.WriteLine($"Key file not found: {keyPath}");
    return 3;
}

string certPem = File.ReadAllText(certPath);
string keyPem = File.ReadAllText(keyPath);

try
{
    // Create certificate object from PEM (no private key yet)
    var cert = X509Certificate2.CreateFromPem(certPem);

    // Determine key type and import
    X509Certificate2 certWithKey;
    if (keyPem.Contains("RSA PRIVATE KEY") || keyPem.Contains("BEGIN PRIVATE KEY") || keyPem.Contains("BEGIN RSA"))
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(keyPem.ToCharArray());
        certWithKey = cert.CopyWithPrivateKey(rsa);
    }
    else if (keyPem.Contains("EC PRIVATE KEY") || keyPem.Contains("BEGIN EC"))
    {
        using var ec = ECDsa.Create();
        ec.ImportFromPem(keyPem.ToCharArray());
        certWithKey = cert.CopyWithPrivateKey(ec);
    }
    else
    {
        Console.WriteLine("Unsupported private key format. Key must be RSA or EC in PEM.");
        return 4;
    }

    byte[] pfx = certWithKey.Export(X509ContentType.Pfx, password);
    File.WriteAllBytes(outPfx, pfx);
    Console.WriteLine($"Wrote PFX to: {outPfx}");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.GetType().Name}: {ex.Message}");
    return 10;
}