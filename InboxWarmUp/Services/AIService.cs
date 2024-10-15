using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace InboxWarmUp.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // Consider storing this in a secure way.

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "AIzaSyCUem-nu48j5PRRfeK3hMT9VEo6Mdxz9D0"; // Replace with your actual API key from configuration.
        }

        public async Task<string> GenerateEmailTemplateAsync(string prompt)
        {
            // Check if the prompt is empty
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return "Prompt is empty.";
            }

            // Prepare request for Gemini AI service
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            // Log the request body
            Console.WriteLine($"Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Prepare the request URL
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_apiKey}";

            // Make the POST request
            try
            {
                var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                    // Log the entire response for debugging
                    Console.WriteLine($"API Response: {JsonSerializer.Serialize(result)}");

                    // Check if the response has candidates and get the first candidate's content
                    if (result?.Candidates != null && result.Candidates.Count > 0)
                    {
                        return result.Candidates[0].Content.Parts[0].Text; // Access the new structure
                    }
                    else
                    {
                        return "AI response is empty.";
                    }
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorMessage}");
                    return $"Error generating email template: {errorMessage}";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
                return "Error making request to AI service.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return "An unexpected error occurred.";
            }
        }

        // Gemini AI response model
        public class GeminiResponse
        {
            public List<Candidate> Candidates { get; set; }
        }

        public class Candidate
        {
            public Content Content { get; set; }
            public string FinishReason { get; set; }
            public int Index { get; set; }
            public List<SafetyRating> SafetyRatings { get; set; }
        }

        public class Content
        {
            public List<Part> Parts { get; set; }
        }

        public class Part
        {
            public string Text { get; set; }
        }

        public class SafetyRating
        {
            public string Category { get; set; }
            public string Probability { get; set; }
        }
    }
}
