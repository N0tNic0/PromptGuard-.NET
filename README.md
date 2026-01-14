# PromptGuard.NET

PromptGuard is a CLI-first toolkit to version, validate and diff LLM prompts as production-grade assets.

## Example workflow

```bash
pg init
pg validate prompts
pg diff invoice.extractor@1.0.0 invoice.extractor@1.1.0
```

## Overview

PromptGuard was born from a simple observation:  
when prompts are used in production, they behave like code.

They evolve, break, introduce regressions and require traceability.
Yet, most teams still manage them as unversioned strings.

PromptGuard provides a minimal but robust workflow to treat prompts as first-class assets:
versioned, validated and diffable — just like software.

## Design principles

PromptGuard follows a few intentional design choices:

- **CLI-first**: prompts should be validated and inspected close to developers and CI pipelines
- **Provider-agnostic**: no coupling to a specific LLM vendor
- **Filesystem-based**: prompts are plain files, easy to version and review
- **Explicit versioning**: every prompt change is deliberate and traceable

The goal is not abstraction, but control.

## Why not store prompts in code?

Embedding prompts directly in application code makes them:

- hard to version independently
- difficult to review changes
- impossible to diff meaningfully
- opaque when debugging production issues

PromptGuard decouples prompt evolution from application releases,
making changes explicit, reviewable and reproducible.

## Real-world use cases

PromptGuard is designed for scenarios such as:

- document and invoice extraction pipelines
- classification and routing systems
- AI-assisted decision support
- regulated or auditable AI workflows

Any system where a prompt change can affect downstream logic
benefits from explicit prompt lifecycle management.

## What PromptGuard deliberately avoids

PromptGuard intentionally does not aim to be:

- a prompt marketplace
- a prompt editor or UI builder
- a no-code AI platform
- an orchestration framework

Its scope is intentionally narrow:
making prompt behavior explicit, inspectable and reliable.

## Project status

PromptGuard is currently in an **early but functional** stage.

Core features (init, validate, diff) are stable and CI-tested.
Future work will focus on prompt testing and regression detection.

# Getting Started

PromptGuard is a CLI tool to validate, version and compare LLM prompts stored as files.
You can use it locally in any repository without external dependencies.

## Prerequisites

.NET 10 SDK
https://dotnet.microsoft.com/download

## Clone the repository

```bash
git clone https://github.com/<N0tNic0>/PromptGuard.NET.git
cd PromptGuard.NET
```

## Initialize PromptGuard

```bash
dotnet run --project src/PromptGuard.Cli -- init
```

This will create:

.promtguard/
└─ config.yaml

prompts/
└─ _examples/
   └─ invoice_extraction/
      └─ 1.0.0.yaml

The _examples folder contains sample prompts for demo and experimentation.

## Validate prompts

Validate all prompts in the prompts directory:

```bash
dotnet run --project src/PromptGuard.Cli -- validate prompts
```

Expected output:

```bash
Validating prompts/_examples/invoice_extraction/1.0.0.yaml
OK
```

## Validate prompts

Create a new version of a prompt, for example:

```bash
prompts/_examples/invoice_extraction/1.1.0.yaml
```

Then compare versions:

```bash
dotnet run --project src/PromptGuard.Cli -- diff _examples/invoice_extraction@1.0.0 _examples/invoice_extraction@1.1.0
```

PromptGuard will show a colorized diff highlighting changes in:

- template
- parameters
- policy

## How prompts are organized

PromptGuard expects prompts to be stored as:

prompts/
└─ <prompt-name>/
   └─ <version>.yaml

Example:

prompts/
└─ invoice_extraction/
   ├─ 1.0.0.yaml
   └─ 1.1.0.yaml

Prompt references follow the format:

<prompt-name>@<version>

## Examples vs real prompts

prompts/_examples/ → demo and sample prompts (tracked in Git)

prompts/* → your real prompts (recommended to be gitignored)

Recommended .gitignore:

prompts/*
!prompts/_examples/**
