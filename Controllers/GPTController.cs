using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prog3_WebApi_Javascript.DTOs;


namespace Prog3_WebApi_Javascript.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GPTController : ControllerBase
    {
        //היכולת לבצע קריאות http
        private readonly HttpClient _client;
        public GPTController(IConfiguration config)
        {
            // Initialize the private HttpClient instance
            _client = new HttpClient();

            // Retrieve the API key from the configuration settings
            string api_key = config.GetValue<string>("OpenAI:Key");

            // Create the authorization header using the API key
            string auth = "Bearer " + api_key;

            // Add the authorization header to the default request headers of the HttpClient instance
            _client.DefaultRequestHeaders.Add("Authorization", auth);
        }


        [HttpPost("GPTChat")]
        public async Task<IActionResult> GPTChat(Prompt promptFromUser)
        {
            //נקודת קצה API עבור OpenAI GPT
            string endpoint = "https://api.openai.com/v1/chat/completions";
            // המודל, במקרה הזה צאט 3.5
            string model = "gpt-3.5-turbo-0125";
            // מספר מקסימום של טוקנים
            int max_tokens = 2000;
            //הגדרת טמפרטורה למפרומפט
            double temperature = 0.8;



            // בנה את ההנחיה לשלוח לדגם
            // במקרה הזה, אנחנו שולחים לצ׳אט בקשה ליצור שאלות בנושא מסוים, המשתמש מזין נושא ואז מקבל תשובה של השאלות, אז יש שרשור של הפרומפט והנושא שהוזן
            string promptToSend = $"Please generate 5 questions related to the subject of {promptFromUser.Subject}." +
            $"you are going to play the role and speak on behalf of {promptFromUser.Charecter} and in the Language of {promptFromUser.Language} ." +
            $"The question should be at the level of elemenatry school" +
            $"The question should be clear, concise, and designed to assess someone's knowledge, Keep your question under 2000 characters." +
            $"Give me 4 multiple choice questions, each on the subject of {promptFromUser.Subject} +" +
            $"make it in json format: " +
            "{Questions:[ { Question1  {Qtext: string ans1: string: ans2: string: ans3: string: ans4: string correct ans: string\n\tcorrectRes: string\n\twrongResp: string\n    }\n} ]\n\nIn response to an incorrect answer, give constructive feedback";


            GPTRequest request = new GPTRequest()
            {
                response_format = new { type = "json_object" },
                //max_tokens = max_tokens,
                temperature = temperature,
                model = model,
                messages = new List<Message>() {
                   new Message{
                    role = "system",
                    content = "Whenever you receive a request, you must reply in the following JSON format: " +
                    "{" +
                    "Questions:[ " +
                    "{" +
                    "Qtext: string " +
                    "ans1: string: " +
                    "ans2: string: " +
                    "ans3: string: " +
                    "ans4: string: " +
                    "correct ans: string:" +
                    "correctRes: string:" +
                    "wrongResp: string:" +
                    "}" +
                    "]" +
                    "}" +
                    "In response to an incorrect answer, give constructive feedback"
                    },
                   
                   new Message{
                    role = "user",
                    content = $"Please generate an opinion related to the subject of history in the level of elementary school students." +
                    $"The opinion should be clear, concise, and designed to assess someone's knowledge " +
                    $"or understanding of the topic. Keep your opinion under 300 characters. Return the opinion in Hebrew language."
                   },
                   new Message{
                    role = "assistant",
                    content = "{ Questions:" +
                    "[" +
                    "{Qtext: What sport involves hitting a small ball into a series of holes with as few strokes as possible?," +
                    "ans1: Tennis," +
                    "ans2: Golf," +
                    "ans3: Soccer, " +
                    "ans4:Basketball, " +
                    "correct_ans:Golf, " +
                    "correctRes:Correct! In golf, players aim to complete the course with the fewest strokes possible.," +
                    "wrongResp: Incorrect. Golf involves hitting a small ball into a series of holes with as few strokes as possible. }," +
                    "{Qtext:Which sport involves using a net and hitting a shuttlecock over it?," +
                    "ans1:Baseball, " +
                    "ans2:Football," +
                    "ans3:Tennis," +
                    "ans4:Badminton, " +
                    "correct_ans:Badminton, " +
                    "correctRes:Well done! Badminton is played by hitting a shuttlecock over a net.," +
                    "wrongResp: Not quite. Badminton involves using a net and hitting a shuttlecock over it.}," +
                    "{ Qtext: In which sport do players use a paddle to hit a ball?," +
                    "ans1:Baseball," +
                    "ans2:Football," +
                    "ans3:Tennis," +
                    "ans4:Badminton," +
                    "correct_ans:Football," +
                    "correctRes: Well done! Football is played by hitting a shuttlecock over a net.," +
                    "wrongResp: Not quite. Football involves using a net and hitting a shuttlecock over it.}" +
                    "{ Qtext: how have an oreng ball?," +
                    "ans1:Baseball," +
                    "ans2:basketball," +
                    "ans3:Tennis," +
                    "ans4:Badminton," +
                    "correct_ans:basketball," +
                    "correctRes: Well done! basketball have an oreng ball," +
                    "wrongResp: Not quite. basketball have an oreng ball.}," +
                    "{ Which sport involves hitting a small ball into a series of holes with as few strokes as possible?," +
                    "ans1:Tennis," +
                    "ans2:Golf," +
                    "ans3:Soccer," +
                    "ans4:Basketball," +
                    "correct_ans:Golf," +
                    "correctRes: Correct! In golf, players aim to complete the course with the fewest strokes possible.," +
                    "wrongResp: Incorrect. Golf involves hitting a small ball into a series of holes with as few strokes as possible.}" +
                    "{ Qtext: What color usually is basketball ball?," +
                    "ans1:Orange," +
                    "ans2:Black," +
                    "ans3:White," +
                    "ans4:Green," +
                    "correct_ans:Orange," +
                    "correctRes: Well done! basketball usually orange color," +
                    "wrongResp: Not quite. basketball usually in a different color.}," +
                    "]}"
                    },
                   new Message {
                    role = "user",
                   content = promptToSend
                    }
                }
            };
        
            // Send the GPTRequest object to the OpenAI API
            var res = await _client.PostAsJsonAsync(endpoint, request);

            // בדיקה האם התקבלה תשובה תקינה 
            if (!res.IsSuccessStatusCode)
                return BadRequest("problem: " + res.Content.ReadAsStringAsync());

            // Read the JSON response from the API
            JsonObject? jsonFromGPT = res.Content.ReadFromJsonAsync<JsonObject>().Result;
            if (jsonFromGPT == null)
                return BadRequest("empty");

            // Extract the generated content from the JSON response
            // choices - ככה זה חוזר לפי מה שמוצג באתר
            string content = jsonFromGPT["choices"][0]["message"]["content"].ToString();

            // Return the generated content
            return Ok(content);



        }






        [HttpGet("bored")]
        public async Task<IActionResult> GetActivity()
        {
            //HTTP: 
            string endpoint = "http://www.boredapi.com/api/activity/";
            //Save response.
            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                //הכנסת התשובת לתוך משתנה
                JsonObject responseContent = response.Content.ReadFromJsonAsync<JsonObject>().Result;

                bored bored = new bored();
                bored.activity = responseContent["activity"].ToString();
                bored.type = responseContent["type"].ToString();
                bored.participants = responseContent["participants"].ToString();

                return Ok(bored);
            }

            return BadRequest("API Call Failed");
        }










    }


}

