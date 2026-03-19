using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CoursedogImporter.Authentication;

public class CoursedogAuthClient
{
  private readonly HttpClient _httpClient;
  private readonly string _email;
  private readonly string _password;
  private readonly string _baseUrl;

  // Matches the JSON shape Coursedog sessions endpoint expects.
  // PlaintextPassword must be one word so CamelCase serialization
  // produces 'plaintextPassword' - API docs incorrectly show 'password'.
  private class AuthRequest
  {
    public string Email {get; set;}
    public string PlaintextPassword {get; set;}
  }

  // Only Token field is needed from response.
  // The rest of the object is ignored by deserializer.
  private class AuthResponse
  {
    public string Token {get; set;}
  }

  // Constructor reads all required values from environment at instantiation.
  // Throws immediately if any variable is missing rather than failing
  // later with less obvious error.
  public CoursedogAuthClient()
  {
    _httpClient = new HttpClient();

    _email = Environment.GetEnvironmentVariable("COURSEDOG_EMAIL")
             ?? throw new InvalidOperationException(
                 "COURSEDOG_EMAIL environment variable is not set.");

    _password = Environment.GetEnvironmentVariable("COURSEDOG_PASSWORD")
                ?? throw new InvalidOperationException(
                    "COURSEDOG_PASSWORD environment variable is not set.");
    
    _baseUrl = Environment.GetEnvironmentVariable("COURSEDOG_BASE_URL")
               ?? throw new InvalidOperationException(
                   "COURSEDOG_BASE_URL environment variable is not set.");
  }

  public async Task<string> GetTokenAsync()
  {
    // Build request body - CamelCase policy ensures PascalCase properties
    // serialize to the camelCase field names the API expects.
    var requestBody = new AuthRequest
    {
      Email = _email,
      PlaintextPassword = _password
    };

    var json = JsonSerializer.Serialize(requestBody,
                new JsonSerializerOptions
                {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
    
    // Wrap JSON string in HTTP content with correct encoding and content type header.
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    // POST to sessions endpoint - server returns 201 Created on success.
    var response = await _httpClient.PostAsync($"{_baseUrl}/sessions", content);

    // Throws HttpRequestException if response is outside 200-299 range.
    response.EnsureSuccessStatusCode();

    var responseJson = await response.Content.ReadAsStringAsync();

    // CaseInsensitive match handles 'token' (API) mapping to 'Token' (C# property).
    var authResponse = JsonSerializer.Deserialize<AuthResponse>(
        responseJson,
        new JsonSerializerOptions {PropertyNameCaseInsensitive = true}
        );

    return authResponse.Token;
  }
}
