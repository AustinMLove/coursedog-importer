using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using CoursedogImporter.Models;

namespace CoursedogImporter.Services;

public class ProgramDataService
{
  private readonly HttpClient _httpClient;
  private readonly string _baseUrl;
  private readonly string _schoolId;

  // Constructor reads required evironment variables.
  // Throws if any variable is missing.
  public ProgramDataService()
  {
    _httpClient = new HttpClient();

    _baseUrl = Environment.GetEnvironmentVariable("COURSEDOG_BASE_URL")
               ?? throw new InvalidOperationException(
                   "COURSEDOG_BASE_URL environment variable is not set.");
    
    _schoolId = Environment.GetEnvironmentVariable("COURSEDOG_SCHOOL_ID")
                ?? throw new InvalidOperationException(
                    "COURSEDOG_SCHOOL_ID environment variable is not set.");
  }

  // Note: API returns programs keyed by a timestamped revision ID (e.g. "AA.ARTS-2025-08-18").
  // The 'code' field represents the stable program code for lookup.
  // The 'sisId' field contains the UUID required for PUT endpoint path.
  // Timestamped ID is revision specific and coannot be used for updates.
  public async Task<Dictionary<string, string>> BuildProgramMapAsync(string token)
  {
    // Attach authorization bearer token to HTTP header.
    _httpClient.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", token);

    // Send GET request to programs endpoint.
    var response = await _httpClient.GetAsync($"{_baseUrl}/cm/{_schoolId}/programs?limit=0");

    // Throws an HttpRequestException if response is outside acceptable range.
    response.EnsureSuccessStatusCode();

    var responseJson = await response.Content.ReadAsStringAsync();

    // Deserialize JSON response and map to ProgramRecord model fields
    var programObjects = JsonSerializer.Deserialize<Dictionary<string, ProgramRecord>>(
        responseJson, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

    // Build and return dictionary of programs.
    return programObjects.Values.ToDictionary(
        program => program.Code,
        program => program.SisId
    );
  }
}
