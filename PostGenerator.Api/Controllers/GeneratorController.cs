using Microsoft.AspNetCore.Mvc;
using PostGenerator.Service.IServices;
using PostGenerator.Shared.Model;
using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;

namespace PostGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeneratorController(IIdeaAgentService ideaAgent, IWriterAgentService writerAgent) : ControllerBase
{
    [HttpPost("generate-and-post")]
    public async Task<IActionResult> GenerateAndPost([FromBody] PostRequest request)
    {
        // Step 1: Idea Agent brainstorms the concept
        var idea = await ideaAgent.GenerateIdeaAsync(
            request.Topic, request.Tone, request.Audience);

        // Step 2: Writer Agent crafts the post
        //var postContent = await writerAgent.WritePostAsync(
        //    idea, request.Tone, request.Audience);

        // Step 3: Post to LinkedIn
        //var posted = await linkedIn.PostAsync(request.AccessToken, postContent);

        //if (!posted)
        //    return BadRequest("Post was generated but LinkedIn rejected it.");

        //return Ok(new GeneratedPost(postContent, idea.Angle));
        return Ok(idea);
    }

    // Preview only — no LinkedIn posting
    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] PostRequest request)
    {
        var idea = await ideaAgent.GenerateIdeaAsync(
            request.Topic, request.Tone, request.Audience);

        var postContent = await writerAgent.WritePostAsync(
            idea, request.Tone, request.Audience);

        return Ok(new
        {
            Idea = idea,
            Post = postContent
        });
    }
}
