using PromptGuard.Core.Models;

namespace PromptGuard.Core.Validation;

public sealed class PromptValidator
{
    public ValidationResult Validate(PromptDefinition p)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(p.Name))
            result.AddError("Missing required field: name");

        if (string.IsNullOrWhiteSpace(p.Version))
            result.AddError("Missing required field: version");

        if (string.IsNullOrWhiteSpace(p.Template))
            result.AddError("Missing required field: template");

        // Check variables are actually used in template: {{var}}
        foreach (var v in p.Variables ?? new())
        {
            var token = "{{" + v + "}}";
            if (!p.Template.Contains(token, StringComparison.Ordinal))
                result.AddWarning($"Variable '{v}' declared but not used in template (expected token: {token}).");
        }

        // Policy checks
        if (p.Policy.RequireJson)
        {
            // simple heuristic: ensure template hints JSON-only
            var t = p.Template.ToLowerInvariant();
            if (!t.Contains("json"))
                result.AddWarning("Policy require_json=true but template doesn't mention JSON output.");
        }

        if (p.Policy.MaxOutputTokens is <= 0)
            result.AddError("policy.max_output_tokens must be > 0 if provided.");

        // Forbidden phrases should not appear in template
        foreach (var phrase in p.Policy.ForbiddenPhrases ?? new())
        {
            if (!string.IsNullOrWhiteSpace(phrase) &&
                p.Template.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                result.AddError($"Template contains forbidden phrase: '{phrase}'");
        }

        return result;
    }
}