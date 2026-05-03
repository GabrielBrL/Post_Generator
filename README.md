# PostAgent — Agent API

An internal API responsible exclusively for communicating with AI agents and returning generated LinkedIn post content. It has no knowledge of users, scheduling, LinkedIn, or email — that responsibility belongs to a separate API.

---

## Architecture Overview

```
                        ┌─────────────────────────┐
                        │     User-Facing API      │  ← handles users, LinkedIn,
                        │   (built separately)     │     scheduling, email, approval
                        └────────────┬────────────┘
                                     │ internal HTTP calls
                                     ▼
                        ┌─────────────────────────┐
                        │       Agent API          │  ← this project
                        │   (this repository)      │     only talks to Claude
                        └────────────┬────────────┘
                                     │
                                     ▼
                        ┌─────────────────────────┐
                        │     Anthropic Claude     │
                        └─────────────────────────┘
```

The Agent API is intentionally isolated. It receives a request, runs the AI pipeline, and returns a result. It does not store data, send emails, manage sessions, or post to LinkedIn.

---

## Agent Pipeline

```
Input (topic or stacks)
        │
        ▼
[Topic Agent]     ← selects the best topic from a list of tech stacks
        │           (skipped when a topic is provided directly)
        ▼
[Idea Agent]      ← builds angle, hook, key points, and CTA from the topic
        │
        ▼
[Writer Agent]    ← writes the final LinkedIn post from the idea
        │
        ▼
Result returned to caller
```

---

## Tech Stack

| | |
|---|---|
| Runtime | .NET 10 |
| Framework | ASP.NET Core Web API |
| AI | Anthropic Claude (`claude-sonnet-4-20250514`) |

No database. No external services. No state.

---

## Project Structure

```
AgentAPI/
├── Controllers/
│   └── AgentController.cs       # POST /api/agent/from-topic
│                                # POST /api/agent/from-stacks
├── Services/
│   ├── TopicAgentService.cs     # Agent 1: picks best topic from stacks
│   ├── IdeaAgentService.cs      # Agent 2: builds post concept
│   └── WriterAgentService.cs    # Agent 3: writes the final post
├── Middleware/
│   ├── AgentMiddleware.cs       # API key · payload validation · exception handling
│   └── ExceptionMiddleware.cs   # Global exception handler
├── Models/
│   └── AgentModels.cs           # Request and response models
├── Extensions/
│   └── MiddlewareExtensions.cs
└── appsettings.json
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An [Anthropic API key](https://console.anthropic.com)

### Installation

```bash
git clone https://github.com/your-username/postagent-agent-api.git
cd postagent-agent-api
dotnet restore
```

### Configuration

Edit `appsettings.json`:

```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-your-key-here"
  },
  "Agents": {
    "ApiKey": "your-internal-secret-key"
  }
}
```

`Agents:ApiKey` is the secret the User-Facing API must include in every request via the `X-Agent-Key` header.

### Run

```bash
dotnet run
```

Available at `http://localhost:5100`.
Swagger UI at `http://localhost:5100/swagger`.

---

## Endpoints

### `POST /api/agent/from-topic`

Runs the Idea Agent and Writer Agent from a given topic and returns the generated post.

**Headers**

```
Content-Type: application/json
X-Agent-Key: your-internal-secret-key
```

**Request body**

```json
{
  "topic": "What building a SaaS taught me about backend architecture",
  "tone": "inspiring",
  "audience": "Software Engineers"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `topic` | string | ✅ | The post subject (10–500 chars) |
| `tone` | string | ❌ | `professional` · `inspiring` · `casual` · `educational` |
| `audience` | string | ❌ | Target LinkedIn audience |

**Response**

```json
{
  "topic": "What building a SaaS taught me about backend architecture",
  "post": "Most developers design their backend for the product they have today.\n\nI used to do the same..."
}
```

---

### `POST /api/agent/from-stacks`

Runs all three agents — Topic, Idea, and Writer — from a list of tech stacks and returns both the generated topic and the final post.

**Request body**

```json
{
  "stacks": [".NET", "Angular", "SQL Server", "Azure", "Docker"],
  "tone": "inspiring",
  "audience": "Software Engineers"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `stacks` | string[] | ✅ | List of technologies (min 1) |
| `tone` | string | ❌ | Defaults to `professional` |
| `audience` | string | ❌ | Defaults to `General Professionals` |

**Response**

```json
{
  "topic": "What building a full-stack SaaS with .NET and Angular taught me about system design",
  "post": "Most developers learn system design from books.\n\nI learned it by breaking production at 2am..."
}
```

---

## The Three Agents

### Agent 1 — Topic Agent

**Used by:** `/from-stacks` only.

Receives a list of tech stacks and selects the single best LinkedIn post topic. The topic is framed as a personal lesson, a strong opinion, or a developer insight. Maximum 150 characters.

**Input:** `stacks[]`, `tone`, `audience`
**Output:** topic string

---

### Agent 2 — Idea Agent

**Used by:** both endpoints.

Receives the topic and returns a structured post concept with four fields:

| Field | Description |
|---|---|
| `Angle` | The unique perspective or narrative direction |
| `KeyPoints` | 3–4 main points to cover, separated by semicolons |
| `Hook` | The attention-grabbing opening line |
| `CallToAction` | The closing question or CTA |

**Input:** `topic`, `tone`, `audience`
**Output:** `PostIdea` object

---

### Agent 3 — Writer Agent

**Used by:** both endpoints.

Receives the idea and writes the final LinkedIn post. Rules enforced via system prompt: short paragraphs, sparse emoji, no hashtags, maximum 1300 characters.

**Input:** `PostIdea`, `tone`, `audience`
**Output:** post string

---

## Middleware

Every request passes through the following steps before reaching the controller:

```
Request
   │
   ├── 1. API Key check      validates X-Agent-Key header
   └── 2. Payload validation  topic length, allowed tone/audience values,
                               prompt injection pattern scan
```

Blocked requests return a structured error:

```json
{
  "error": "VALIDATION_ERROR",
  "message": "Topic is too short. Minimum 10 characters.",
  "timestamp": "2026-05-03T14:22:00Z"
}
```

---

## Exception Handling

All unhandled exceptions are caught globally and returned as structured JSON:

```json
{
  "status": 500,
  "error": "InvalidOperationException",
  "message": "Idea agent returned an empty result.",
  "timestamp": "2026-05-03T14:22:00Z"
}
```

| Exception | HTTP status |
|---|---|
| `KeyNotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| `ArgumentException` | 400 |
| `InvalidOperationException` | 400 |
| Everything else | 500 |

---

## Allowed Values

**Tones**
```
professional · inspiring · casual · educational
```

**Audiences**
```
Software Engineers · Tech Leads · Engineering Managers
Product Managers · Entrepreneurs · General Professionals
Backend Developers · Frontend Developers · DevOps Engineers
```

---

## What this API does NOT do

- Does not store anything in a database
- Does not authenticate users
- Does not send emails
- Does not post to LinkedIn
- Does not manage schedules or approvals

All of that is handled by the User-Facing API, which calls this API as an internal service.

---

## Environment Variables (production)

```bash
ANTHROPIC__APIKEY=sk-ant-...
AGENTS__APIKEY=your-internal-secret-key
```

---

## Related

| Repository | Responsibility |
|---|---|
| `postagent-agent-api` ← this | AI agent pipeline, post generation |
| `postagent-user-api` | Users, LinkedIn, scheduling, approval, email |
| `postagent-frontend` | Angular dashboard |