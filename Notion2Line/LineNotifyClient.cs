using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notion2Line
{
    public class LineNotifyClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _lineToken;

        public LineNotifyClient(string lineToken)
        {
            _lineToken = lineToken;
            _httpClient = new HttpClient();
        }

        public async Task NotifyAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("message", message)
            });

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _lineToken);

            var response = await _httpClient.PostAsync("https://notify-api.line.me/api/notify", requestContent);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}