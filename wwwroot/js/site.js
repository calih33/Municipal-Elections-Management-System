// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// dark mode toggle (bonus)
// _Layout sets data-bs-theme in <head> before the page draws; this switches it and remembers the choice
(function () {
    var button = document.getElementById("theme-toggle");
    if (!button) {
        return;
    }

    function updateLabel() {
        var dark = document.documentElement.getAttribute("data-bs-theme") === "dark";
        var label = dark ? "Switch to light mode" : "Switch to dark mode";
        button.setAttribute("aria-label", label);
        button.setAttribute("title", label);
    }

    button.addEventListener("click", function () {
        var dark = document.documentElement.getAttribute("data-bs-theme") === "dark";
        var theme = dark ? "light" : "dark";
        document.documentElement.setAttribute("data-bs-theme", theme);

        // storage can be blocked (private windows), the toggle still works for this page
        try {
            localStorage.setItem("theme", theme);
        } catch (e) { }

        updateLabel();
    });

    updateLabel();
})();
