using CoursedogImporter.Authentication;

class Program
{

  // Environment Load Helper

  static void LoadEnvFile(string filePath)
  {
    if (!File.Exists(filePath))
      return;

    foreach (var line in File.ReadAllLines(filePath))
    {
      if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
      {
        continue;
      }

      // Split at first '=' - passwords may contain '='.
      var parts = line.Split('=', 2);
      if (parts.Length != 2)
        continue;

      var key = parts[0].Trim();
      var value = parts[1].Trim();

      // Only set environment variables if they aren't already defined.
      // This will allow pipeline secret injection later without .env overriding them.
      if (Environment.GetEnvironmentVariable(key) == null)
        Environment.SetEnvironmentVariable(key, value);
    }
  }

  // Main

  static async Task Main(string[] args)
  {
    // .env lives in prject root, two levels up from src/CoursedogImporter.
    LoadEnvFile("../../.env");

    var authClient = new CoursedogAuthClient();

    Console.WriteLine("Authenticating with Coursedog API...");
    var token = await authClient.GetTokenAsync();
    Console.WriteLine("Authentication successful. Token received.");

  }
}
