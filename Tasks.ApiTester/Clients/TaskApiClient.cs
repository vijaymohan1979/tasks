using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Models;

namespace Tasks.ApiTester.Clients;

/// <summary>
/// HTTP client wrapper for Task API operations with built-in logging.
/// </summary>
public class TaskApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public TaskApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Verifies the API is reachable.
    /// </summary>
    public async Task HealthCheckAsync()
    {
        ConsoleLogger.LogDebug($"GET {_baseUrl}?PageSize=1");
        var response = await _httpClient.GetAsync($"{_baseUrl}?PageSize=1");
        response.EnsureSuccessStatusCode();
    }

    #region GET Operations

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    public async Task<TaskResponse?> GetTaskByIdAsync(int id)
    {
        var url = $"{_baseUrl}/{id}";
        ConsoleLogger.LogDebug($"GET {url}");

        var response = await _httpClient.GetAsync(url);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        ConsoleLogger.LogDebug($"Retrieved: {result}");
        return result;
    }

    /// <summary>
    /// Gets a task by ID and returns the raw response for status code inspection.
    /// </summary>
    public async Task<HttpResponseMessage> GetTaskByIdRawAsync(int id)
    {
        var url = $"{_baseUrl}/{id}";
        ConsoleLogger.LogDebug($"GET {url}");

        var response = await _httpClient.GetAsync(url);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    /// <summary>
    /// Gets tasks with optional filtering, sorting, and pagination.
    /// </summary>
    public async Task<PaginatedResponse<TaskResponse>?> GetTasksAsync(string queryString = "")
    {
        var url = $"{_baseUrl}{queryString}";
        ConsoleLogger.LogDebug($"GET {url}");

        var response = await _httpClient.GetAsync(url);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskResponse>>(_jsonOptions);

        ConsoleLogger.LogDebug($"Retrieved: {result}");
        return result;
    }

    /// <summary>
    /// Gets tasks and returns the raw response for status code inspection.
    /// </summary>
    public async Task<HttpResponseMessage> GetTasksRawAsync(string queryString = "")
    {
        var url = $"{_baseUrl}{queryString}";
        ConsoleLogger.LogDebug($"GET {url}");

        var response = await _httpClient.GetAsync(url);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    #endregion

    #region POST Operations

    /// <summary>
    /// Creates a new task.
    /// </summary>
    public async Task<TaskResponse?> CreateTaskAsync<T>(T request)
    {
        ConsoleLogger.LogDebug($"POST {_baseUrl}");
        ConsoleLogger.LogDebug($"Request Body: {JsonSerializer.Serialize(request, _jsonOptions)}");

        var response = await _httpClient.PostAsJsonAsync(_baseUrl, request, _jsonOptions);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        ConsoleLogger.LogDebug($"Created: {result}");
        return result;
    }

    /// <summary>
    /// Creates a task and returns the raw response for status code inspection.
    /// </summary>
    public async Task<HttpResponseMessage> CreateTaskRawAsync<T>(T request)
    {
        ConsoleLogger.LogDebug($"POST {_baseUrl}");
        ConsoleLogger.LogDebug($"Request Body: {JsonSerializer.Serialize(request, _jsonOptions)}");

        var response = await _httpClient.PostAsJsonAsync(_baseUrl, request, _jsonOptions);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    #endregion

    #region PUT Operations

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    public async Task<TaskResponse?> UpdateTaskAsync<T>(int id, T request)
    {
        var url = $"{_baseUrl}/{id}";
        ConsoleLogger.LogDebug($"PUT {url}");
        ConsoleLogger.LogDebug($"Request Body: {JsonSerializer.Serialize(request, _jsonOptions)}");

        var response = await _httpClient.PutAsJsonAsync(url, request, _jsonOptions);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        ConsoleLogger.LogDebug($"Updated: {result}");
        return result;
    }

    /// <summary>
    /// Updates a task and returns the raw response for status code inspection.
    /// </summary>
    public async Task<HttpResponseMessage> UpdateTaskRawAsync<T>(int id, T request)
    {
        var url = $"{_baseUrl}/{id}";
        ConsoleLogger.LogDebug($"PUT {url}");
        ConsoleLogger.LogDebug($"Request Body: {JsonSerializer.Serialize(request, _jsonOptions)}");

        var response = await _httpClient.PutAsJsonAsync(url, request, _jsonOptions);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    #endregion

    #region PATCH Operations

    /// <summary>
    /// Updates only the status of a task.
    /// </summary>
    public async Task<TaskResponse?> UpdateTaskStatusAsync<T>(int id, T request)
    {
        var url = $"{_baseUrl}/{id}/status";
        ConsoleLogger.LogDebug($"PATCH {url}");
        ConsoleLogger.LogDebug($"Request Body: {JsonSerializer.Serialize(request, _jsonOptions)}");

        var response = await _httpClient.PatchAsJsonAsync(url, request, _jsonOptions);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        ConsoleLogger.LogDebug($"Status Updated: {result}");
        return result;
    }

    /// <summary>
    /// Updates task status and returns the raw response for status code inspection.
    /// </summary>
    public async Task<HttpResponseMessage> UpdateTaskStatusRawAsync<T>(int id, T request)
    {
        var url = $"{_baseUrl}/{id}/status";
        ConsoleLogger.LogDebug($"PATCH {url}");
        ConsoleLogger.LogDebug($"Request Body: {JsonSerializer.Serialize(request, _jsonOptions)}");

        var response = await _httpClient.PatchAsJsonAsync(url, request, _jsonOptions);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    #endregion

    #region DELETE Operations

    /// <summary>
    /// Deletes a task by ID.
    /// </summary>
    public async Task<HttpResponseMessage> DeleteTaskAsync(int id)
    {
        var url = $"{_baseUrl}/{id}";
        ConsoleLogger.LogDebug($"DELETE {url}");

        var response = await _httpClient.DeleteAsync(url);
        ConsoleLogger.LogDebug($"Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    #endregion
}