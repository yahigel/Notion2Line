using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;


namespace Notion2Line
{

    public class NotionApiClient
    {
        private readonly string _notionSecret;
        private readonly HttpClient _httpClient;

        public NotionApiClient(string notionSecret)
        {
            _notionSecret = notionSecret;
            _httpClient = new HttpClient();
        }

        public async Task<string> FetchDataFromNotion(string databaseId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://api.notion.com/v1/databases/{databaseId}/query"),
                Headers = {
                { "Authorization", $"Bearer {_notionSecret}" },
                { "Notion-Version", "2021-05-13" }
            },
                Content = new StringContent("{\"page_size\": 100}", System.Text.Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        public async Task<string> FetchAllDataFromNotion(string databaseId)
        {
            bool hasMore = true;
            string nextCursor = null;
            List<JsonElement> allResults = new List<JsonElement>();

            while (hasMore)
            {
                // リクエストボディの作成
                var requestBody = new Dictionary<string, object>
                {
                    {"page_size", 100}
                };

                // start_cursorが存在する場合のみ、リクエストボディに追加
                if (!string.IsNullOrEmpty(nextCursor))
                {
                    requestBody.Add("start_cursor", nextCursor);
                }

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://api.notion.com/v1/databases/{databaseId}/query"),
                    Headers = {
                { "Authorization", $"Bearer {_notionSecret}" },
                { "Notion-Version", "2021-05-13" }
            },
                    Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Notion API returned {response.StatusCode}: {errorResponse}");
                }
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(jsonResponse);

                allResults.AddRange(data.RootElement.GetProperty("results").EnumerateArray());

                hasMore = data.RootElement.TryGetProperty("has_more", out JsonElement hasMoreElement) && hasMoreElement.GetBoolean();
                nextCursor = hasMore ? data.RootElement.GetProperty("next_cursor").GetString() : null;
            }

            var finalResults = JsonDocument.Parse(JsonSerializer.Serialize(new { results = allResults }));
            return finalResults.RootElement.GetRawText();
        }
    }
}
