using Microsoft.AspNetCore.Mvc;
using PostGenerator.Service.Services;
using PostGenerator.Shared.Request;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace PostGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OllamaController(OllamaService ollamaService) : ControllerBase
{

    // This method handles the entire streaming communication
    [HttpPost("topic")]
    public async Task<IActionResult> AskAi([FromBody] TopicRequest topic)
    {
        var result = await ollamaService.StreamChatAsync(topic);
        return Ok(result);
    }

    [HttpPost("generate-post")]
    public async Task<IActionResult> GeneratePost([FromBody] PostRequest postRequest)
    {
        var result = await ollamaService.GeneratePostAsync(postRequest);
        return Ok(result);
    }
}
