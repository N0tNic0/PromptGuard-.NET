![CI](https://github.com/N0tNic0/PromptGuard.NET/actions/workflows/ci.yml/badge.svg)
# PromptGuard.NET

PromptGuard is a CLI-first toolkit to version, validate and diff LLM prompts as production-grade assets.

## Example workflow

```bash
pg init
pg validate prompts
pg diff invoice.extractor@1.0.0 invoice.extractor@1.1.0
---

## STEP 8 — Aggiungi LICENSE MIT
Crea file:

```powershell
notepad LICENSE

MIT License

Copyright (c) 2026 Nicholas Notaro

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
