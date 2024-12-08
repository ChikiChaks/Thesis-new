using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Prog3_WebApi_Javascript.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyntaxCorrectionController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly ILogger<SyntaxCorrectionController> _logger;

        public SyntaxCorrectionController(IConfiguration config, ILogger<SyntaxCorrectionController> logger)
        {
            _client = new HttpClient();
            _logger = logger;
            string api_key = config.GetValue<string>("OpenAI:Key");
            string auth = "Bearer " + api_key;
            _client.DefaultRequestHeaders.Add("Authorization", auth);
        }

        [HttpPost("FixPythonSyntax")]
        public async Task<IActionResult> FixPythonSyntax([FromBody] SyntaxCorrectionRequest request)
        {
            try
            {
                string endpoint = "https://api.openai.com/v1/chat/completions";
                string model = "gpt-3.5-turbo";
                int maxTokens = 1500;
                double temperature = 0.5;

                var gptRequest = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful assistant that responds in Hebrew." },
                        new { role = "user", content = $"תקן את תחביר קוד הפייתון הבא:\n\n{request.Code}\n\n" }
                    },
                    max_tokens = maxTokens,
                    temperature = temperature
                };

                var response = await _client.PostAsJsonAsync(endpoint, gptRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, Content: {errorContent}");
                    return BadRequest($"API Error: {response.StatusCode}, Content: {errorContent}");
                }

                JsonObject jsonResponse = await response.Content.ReadFromJsonAsync<JsonObject>();
                if (jsonResponse == null)
                {
                    _logger.LogError("Received empty response from API.");
                    return BadRequest("Received empty response from API.");
                }

                string correctedCode = jsonResponse["choices"][0]["message"]["content"].ToString();

                return Ok(new { CorrectedCode = correctedCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return StatusCode(500, "Internal server error");
            }
        }

        public class SyntaxCorrectionRequest
        {
            public string Code { get; set; }
        }
    }
}
