# Devlog workflow

This project is both the website and the tutorial for building the website.

When Codex explains or performs a meaningful build step, add a new chapter to
`devlog.html` so the site documents its own construction.

## Chapter rules

- Use the next number: `Devlog 004`, `Devlog 005`, and so on.
- Give the section a matching id: `devlog-004`, `devlog-005`.
- Explain the goal before listing commands or code.
- Break the chapter into small steps with `h3` headings.
- Give each `h3` a unique id so the table of contents can link to it.
- Put commands and code snippets inside `pre` and `code` tags.
- Include a verification step that tells the reader what to check.
- Update the table of contents in the aside.

## Suggested chapter shape

```html
<section id="devlog-004">
    <h2>Devlog 004: Chapter title</h2>

    <p>Short summary of what this chapter builds or explains.</p>

    <h3 id="step-id">Step 1: First action</h3>
    <p>Explain why this step matters.</p>

    <pre><code>Commands or code go here</code></pre>

    <h3 id="verify-step">Step 2: Verify the result</h3>
    <p>Explain what the reader should see when it works.</p>
</section>
```

## Publishing workflow

```powershell
git status
git add .
git commit -m "Describe the devlog update"
git push
```
