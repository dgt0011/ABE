using Microsoft.Extensions.Options;
using System.Text.Json;


namespace AnotherBlogEngine.Web.Shared.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiUrl;

        public HttpService(HttpClient httpClient, IOptions<BaseUrlConfiguration> baseUrlConfiguration)
        {
            _httpClient = httpClient;
            _apiUrl = baseUrlConfiguration.Value.ApiBase;
        }

        public async Task<T?> HttpGet<T>(string uri) where T : class
        {
            var result = await _httpClient.GetAsync($"{_apiUrl}{uri}");
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }

            return await FromHttpResponseMessage<T>(result);
        }

        private static async Task<T?> FromHttpResponseMessage<T>(HttpResponseMessage result)
        {
            var contentsString = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(contentsString, CaseInsensitiveOptions);
        }

        private static readonly JsonSerializerOptions CaseInsensitiveOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
