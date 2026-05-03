using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace PostGenerator.Shared.Model;

public record class PostIdea(string Angle,
    string KeyPoints,
    string Hook,
    string CallToAction);