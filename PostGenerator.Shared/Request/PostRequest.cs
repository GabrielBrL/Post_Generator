using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Request;

public record class PostRequest(
    TopicResult topic,
    string SessionId
    );
