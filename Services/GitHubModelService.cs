// MeaslesAIChatApp/Services/GitHubModelService.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // 加入 Logging
using System;
using System.Linq; // For Choices.FirstOrDefault()
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeaslesAIChatApp.Services
{
    public class GitHubModelService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GitHubModelService> _logger; // 加入 Logger
        private readonly HttpClient _httpClient;
        private readonly string _githubToken;
        private readonly string _apiEndpoint;
        private readonly string _modelName;

        public GitHubModelService(IConfiguration configuration, ILogger<GitHubModelService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _githubToken = _configuration["GitHubModels:Token"]
                           ?? throw new InvalidOperationException("GitHub Token not configured. Check User Secrets (local) or Environment Variables (Vercel).");

            _apiEndpoint = _configuration["GitHubModels:ApiEndpoint"] ?? "https://models.github.ai/inference";
            _modelName = _configuration["GitHubModels:ModelName"] ?? "openai/o3";

            // 使用 HttpClient 來呼叫 GitHub API
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _githubToken);
            _httpClient.BaseAddress = new Uri(_apiEndpoint);
        }

        public async Task<string?> GetModelResponseAsync(string userPrompt)
        {
            _logger.LogInformation("Attempting to get model response for prompt: {Prompt}", userPrompt);

            // 設定模型參數
            const float temperature = 0.7f;
            const int maxTokens = 500;

            var requestData = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                },
                temperature = temperature,
                max_tokens = maxTokens
            };

            try
            {
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("v1/chat/completions", content);
                response.EnsureSuccessStatusCode();
                
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = System.Text.Json.JsonSerializer.Deserialize<ChatCompletionResponse>(responseString);
                
                if (responseObject?.Choices?.Count > 0)
                {
                    string? messageContent = responseObject.Choices[0].Message?.Content;
                    if (!string.IsNullOrWhiteSpace(messageContent))
                    {
                        _logger.LogInformation("Successfully received model response.");
                        return messageContent;
                    }
                }
                
                _logger.LogWarning("Received no valid choices from the model.");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed. Status: {Status}, Message: {Message}",
                    ex.StatusCode, ex.Message);
                return $"Error: API request failed ({ex.StatusCode}). Please check logs.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while communicating with the model.");
                return "Error: An unexpected error occurred. Please try again later.";
            }
        }

        // 需要新增一個用於反序列化的類別
        private class ChatCompletionResponse
        {
            public List<Choice> Choices { get; set; } = new List<Choice>();
            
            public class Choice
            {
                public Message? Message { get; set; }
            }
            
            public class Message
            {
                public string? Role { get; set; }
                public string? Content { get; set; }
            }
        }
    }
}