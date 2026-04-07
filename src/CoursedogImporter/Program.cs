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

    // Print extracted structure for verification
    Console.WriteLine($"\nProgram: {requirements.ProgramTitle}");
    Console.WriteLine($"Sections found: {requirements.Sections.Count}");
    Console.WriteLine();

    foreach (var section in requirements.Sections)
    {
      Console.WriteLine($"--- {section.SectionName} ---");
      foreach (var entry in section.Entries)
      {
        // Type prefix for each entry to verify categorization
        var prefix = entry.Type switch
        {
          EntryType.Course => " [COURSE] ",
          EntryType.Placeholder => " [PLACEHOLDER] ",
          EntryType.SubHeading => " [SUBHEADING] ",
          EntryType.Subtotal => " [SUBTOTAL] ",
          EntryType.Narrative => " [NARRATIVE] ",
          _ => " [UNKNOWN] "
        };
        Console.WriteLine($"{prefix} {entry.Text}");
      }
      Console.WriteLine();
    }
  }
}
