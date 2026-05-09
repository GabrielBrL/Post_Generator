using Microsoft.AspNetCore.Mvc;
using PostGenerator.Shared.IServices;
using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;

namespace PostGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeneratorController(ITopicAgentService topicAgent, IIdeaAgentService ideaAgent, IWriterAgentService writerAgent) : ControllerBase
{
    [HttpPost("generate-topic")]
    public async Task<IActionResult> GenerateTopic([FromBody] TopicRequest topicRequest)
    {
        var topic = topicAgent.GenerateAsync(topicRequest, HttpContext.RequestAborted);

        return Ok(topic);
    }

    [HttpPost("generate-and-post")]
    public async Task<IActionResult> GenerateAndPost([FromBody] PostRequest request)
    {
        // Step 1: Idea Agent brainstorms the concept
        PostIdeaResponse? idea = await ideaAgent.GenerateIdeaAsync(
            request, HttpContext.RequestAborted);

        // Step 2: Writer Agent crafts the post        
        if (idea != null)
        {
            var postContent = await writerAgent.WritePostAsync(
                idea, HttpContext.RequestAborted);
            return Ok(postContent);
        }

        // Step 3: Post to LinkedIn
        //var posted = await linkedIn.PostAsync(request.AccessToken, postContent);

        return BadRequest("Post was generated but LinkedIn rejected it.");
    }

    // Preview only — no LinkedIn posting
    //[HttpPost("preview")]
    //public async Task<IActionResult> Preview([FromBody] PostRequest request)
    //{
    //    var idea = await ideaAgent.GenerateIdeaAsync(
    //        request.Topic, request.Tone, request.Audience);

    //    var postContent = await writerAgent.WritePostAsync(
    //        idea, request.Tone, request.Audience);

    //    return Ok(new
    //    {
    //        Idea = idea,
    //        Post = postContent
    //    });
    //}
}
