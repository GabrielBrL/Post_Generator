using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Request;

public record TopicRequest(
string[] Stacks,
int Quantity = 6,
string Platform = "mixed",
string Level = "mixed",
string Language = "English"
);
