# PromptGuard.NET

PromptGuard is a CLI-first toolkit to version, validate and diff LLM prompts as production-grade assets.

## Example workflow

```bash
pg init
pg validate prompts
pg diff invoice.extractor@1.0.0 invoice.extractor@1.1.0
