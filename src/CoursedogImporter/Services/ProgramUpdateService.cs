using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoursedogImporter.Models;

namespace CoursedogImporter.Services;

public class ProgramUpdateService
{
  private readonly HttpClient _httpClient;
  private readonly string _baseUrl;
  private readonly string _schoolId;

  public ProgramUpdateService()
  {
    _httpClient = new HttpClient();

    _baseUrl = Environment.GetEnvironmentVariable("COURSEDOG_BASE_URL")
      ?? throw new InvalidOperationException("COURSEDOG_BASE_URL environment variable is not set.");

    _schoolId = Environment.GetEnvironmentVariable("COURSEDOG_SCHOOL_ID")
      ?? throw new InvalidOperationException("COURSEDOG_SCHOOL_ID environment variable is not set.");
  }

  public async Task UpdateProgramRequirementsAsync(
      string token,
      string sisId,
      string freeformHtml)
  {
    // Attach bearer token to all requests
    _httpClient.DefaultRequestHeaders.Authorization = 
      new AuthenticationHeaderValue("Bearer", token);

    // Build request body
    var requestBody = new ProgramUpdateRequest
    {
      Requisites = new RequisitesUpdate
      {
        RequisitesFreeform = new RequisitesFreeform
        {
          ShowInCatalog = true,
          Value = freeformHtml
        }
      }
    };

    // Serialize with CamelCase policy to match API expectations
    var json = JsonSerializer.Serialize(requestBody,
        new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

    var content = new StringContent(json, Encoding.UTF8, "application/json");

    // PUT to the program endpoint
    var response = await _httpClient.PutAsync(
        $"{_baseUrl}/cm/{_schoolId}/programs/{sisId}?doIntegration=true", content);

    response.EnsureSuccessStatusCode();

    Console.WriteLine($"Program {sisId} updated successfully.");
  }
}
