using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ChatGptCarBot.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiKey = "<PUT YOUR KEY HERE>";

        public ChatController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "Prompt")]
        public async Task<string> Get()
        {
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            string apiUrl = "https://api.openai.com/v1/chat/completions";
            string jsonContent = $"{{ \"prompt\": \"Hello\", \"max_tokens\": 5, \"model\": \"gpt-3.5-turbo\" }}";

            var response = await httpClient.PostAsync(apiUrl, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
    }
}
