# TMS coding conventions

Follow the reference application's Controllers / Interfaces / Services / Models / Views organization.
Use block-scoped C# namespaces, conventional constructors, four-space indentation and braces on separate lines.

## C#
- Keep one model class per file and use the Model suffix for domain models.
- Use PascalCase for classes, methods and model properties; camelCase for parameters and local variables.
- Keep JSON, model-binding and display annotation names in camelCase.
- Put each annotation on its own line, with a blank line between properties.
- Use private readonly fields named _camelCase for injected dependencies.
- Inject interfaces into controllers; keep business rules and persistence in services.
- Use descriptive variables and guard clauses. Avoid compressed methods and nested ternaries.
- Use shared constants for booking statuses. Keep mock data separate from booking rules.
- Comment why a lock, persistence step or fallback is needed rather than restating the code.

## Razor, JavaScript and CSS
- Indent nested markup and keep forms, controls and loops on separate lines.
- Use descriptive loop variables and named tuple fields instead of single letters or Item1.
- Keep scripts in wwwroot/js; separate theme, form and route-map behavior.
- Give functions clear names and split rendering, events and state updates into small functions.
- Keep one CSS declaration per line. Group base, component, theme and responsive rules clearly.
- Preserve accessible labels, keyboard controls, safe text insertion and existing camelCase form field names.

## Verification
Build with dotnet build TMS.slnx --no-restore.
Check booking creation, validation, cancellation and persistence when changing booking code.
Check the map, theme preference and booking pickup selection in the browser when changing front-end code.
Stop temporary preview servers after checks to avoid port conflicts with Visual Studio.
