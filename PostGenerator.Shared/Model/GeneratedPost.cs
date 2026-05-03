using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Model;

public record class GeneratedPost(
string Content,
string Idea
);