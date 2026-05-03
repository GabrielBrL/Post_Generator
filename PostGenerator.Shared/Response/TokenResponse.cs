using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Response;

public record class TokenResponse(string access_token, int expires_in);
