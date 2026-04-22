using CoursedogImporter.Authentication;
using CoursedogImporter.Services;
using CoursedogImporter.Models;

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
    // .env lives in project root, two levels up from src/CoursedogImporter.
    LoadEnvFile("../../.env");

    // Validate command line argument and set path
    if (args.Length == 0)
      throw new InvalidOperationException("Usage: dotnet run <path-to-catalog-html>");

    var catalogFilePath = args[0];

    // Retrieve bearer token for API requests and report status.
    var authClient = new CoursedogAuthClient();
    Console.WriteLine("Authenticating with Coursedog API...");
    var token = await authClient.GetTokenAsync();
    Console.WriteLine("Authentication successful. Token received.");

    // GET all programs, build programMap dictionary, and report status.
    var programDataService = new ProgramDataService();
    Console.WriteLine("Fetching program data...");
    var programMap = await programDataService.BuildProgramMapAsync(token);
    Console.WriteLine($"Program dictionary built. {programMap.Count} programs loaded.");

    // Parse catalog HTML file
    Console.WriteLine($"Parsing catalog file: {catalogFilePath}");
    var parser = new CatalogParserService();
    var requirements = parser.ParseFromFile(catalogFilePath);

    // Print extracted program details for verification
    Console.WriteLine($"\nProgram: {requirements.ProgramTitle}");
    Console.WriteLine($"Sections found: {requirements.Sections.Count}");

    // Generate freeform HTML block
    var builder = new RequirementBlockBuilder();
    var freeformHtml = builder.BuildFreeformHtml(requirements);

    // Print generated HTML for verification before sending to API
    Console.WriteLine("\n--- Generated HTML ---");
    Console.WriteLine(freeformHtml);
    Console.WriteLine("--- End HTML ---\n");

    // Submit HTML PUT request with confirmation
    // For now, prompt user to confirm and manually enter parsed program code
    Console.WriteLine("Send PUT request to Coursedog API? (yes/no): ");
    var confirm = Console.ReadLine();

    if (confirm?.ToLower() == "yes" || confirm?.ToLower() == "y")
    {
      // Look up sisId by program code
      Console.WriteLine("Enter program code (e.g. AAS.NUR): ");
      var programCode = Console.ReadLine();

      if (programMap.TryGetValue(programCode, out var sisId))
      {
        var updateService = new ProgramUpdateService();
        await updateService.UpdateProgramRequirementsAsync(token, sisId, freeformHtml);
      }
      else
      {
        Console.WriteLine($"Program code '{programCode}' not found in dictionary.");
        Console.WriteLine("Available codes sample:");
        foreach (var kvp in programMap.Take(5))
          Console.WriteLine($"  {kvp.Key}");
      }
    }
  }
}
