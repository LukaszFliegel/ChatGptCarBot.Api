using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace ChatGptCarBot.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly string apiKey = "";

        public ChatController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        [EnableCors("AllowReactApp")]
        [HttpPost(Name = "Prompt")]
        public async Task<string> Prompt(Message message)
        {
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            string apiUrl = "https://api.openai.com/v1/chat/completions";

            if (!_memoryCache.TryGetValue(message.Guid, out AiModel aiModel))
            {
                aiModel = new AiModel();
                aiModel.AddPrompt("You are a salesman. You sell cars. You are talking to a customer. You goal is to make the best possible advice to the customer on how to pick their most suitable car. All further promopts are from the customer. Answer as a salesman. Answer with a maximum of 64 characters.");
                _memoryCache.Set(message.Guid, aiModel);
            }

            aiModel.AddPrompt(message.Prompt);

            var response = await httpClient.PostAsync(apiUrl, new StringContent(JsonSerializer.Serialize(aiModel), Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            var responseModel = JsonSerializer.Deserialize<AiModelResponse>(responseBody);
            var responseContent = responseModel.choices.First().message.content;

            aiModel.AddResponse(responseContent);

            return responseContent;
        }

        public record Message
        {
            public string Prompt { get; set; }
            public string Guid { get; set; }
        }

        public record AiModel
        {
            public string model { get; set; } = "gpt-3.5-turbo";
            public int max_tokens { get; set; } = 64;
            public List<AiMessage> messages { get; set; } = new List<AiMessage>();

            public void AddPrompt(string message)
            {
                messages.Add(new AiMessage() { content = message });
            }

            public void AddResponse(string message)
            {
                messages.Add(new AiMessage() { content = message, role = "assistant" });
            }
        }

        public record AiMessage
        {
            public string role { get; set; } = "user";
            public string content { get; set; }
        }



        public class AiModelResponse
        {
            public string id { get; set; }
            public string _object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public Choice[] choices { get; set; }
            public Usage usage { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        public class Choice
        {
            public int index { get; set; }
            public ResponseMessage message { get; set; }
            public string finish_reason { get; set; }
        }

        public class ResponseMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

    }
}
