(function () {
    const languageAliases = {
        "c#": "csharp",
        css: "css",
        gitignore: "gitignore",
        html: "markup",
        javascript: "javascript",
        js: "javascript",
        markdown: "markdown",
        powershell: "powershell"
    };

    function normalizeCodeBlock(code) {
        const label = code.querySelector("strong");

        if (!label) {
            return "";
        }

        const labelText = label.textContent.replace("</>", "").trim();
        label.remove();

        if (code.firstChild && code.firstChild.nodeType === Node.TEXT_NODE) {
            code.firstChild.textContent = code.firstChild.textContent.replace(/^\s*\n/, "");
        }

        return labelText;
    }

    function inferLanguage(code, labelText) {
        const className = Array.from(code.classList).find((name) => name.startsWith("language-"));

        if (className) {
            return className.replace("language-", "");
        }

        const labelLanguage = languageAliases[labelText.toLowerCase()];

        if (labelLanguage) {
            return labelLanguage;
        }

        const text = code.textContent.trimStart();

        if (text.startsWith("<!DOCTYPE") || text.startsWith("<")) {
            return "markup";
        }

        if (text.startsWith("# ")) {
            return "markdown";
        }

        if (/^[.#\w:[\]-]+\s*\{/.test(text)) {
            return "css";
        }

        if (/^(git|mkdir|cd)\b/.test(text)) {
            return "powershell";
        }

        return "none";
    }

    async function copyText(text) {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return;
        }

        const textarea = document.createElement("textarea");
        textarea.value = text;
        textarea.setAttribute("readonly", "");
        textarea.style.position = "fixed";
        textarea.style.top = "-999px";
        document.body.appendChild(textarea);
        textarea.select();

        const copied = document.execCommand("copy");
        textarea.remove();

        if (!copied) {
            throw new Error("Copy command failed");
        }
    }

    function selectCode(code) {
        const selection = window.getSelection();
        const range = document.createRange();

        range.selectNodeContents(code);
        selection.removeAllRanges();
        selection.addRange(range);
    }

    function addCopyButton(pre, code) {
        const button = document.createElement("button");
        button.className = "copy-code-button";
        button.type = "button";
        button.textContent = "Copy";

        button.addEventListener("click", async () => {
            try {
                await copyText(code.textContent);
                button.textContent = "Copied";
            } catch (error) {
                selectCode(code);
                button.textContent = "Selected";
            }

            window.setTimeout(() => {
                button.textContent = "Copy";
            }, 1400);
        });

        pre.appendChild(button);
    }

    document.querySelectorAll("pre code").forEach((code) => {
        const pre = code.parentElement;
        const labelText = normalizeCodeBlock(code);
        const language = inferLanguage(code, labelText);
        const displayLanguage = labelText || language;

        code.classList.add(`language-${language}`);
        pre.classList.add(`language-${language}`);
        pre.dataset.codeLanguage = displayLanguage;

        addCopyButton(pre, code);
    });

    if (window.Prism && typeof window.Prism.highlightAll === "function") {
        if (window.Prism.plugins && window.Prism.plugins.autoloader) {
            window.Prism.plugins.autoloader.languages_path =
                "https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/components/";
        }

        window.Prism.highlightAll();
    }
})();
