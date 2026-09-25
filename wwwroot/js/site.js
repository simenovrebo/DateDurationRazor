// Helpers for the calculator form: "Today" / "Now" / preset links and the show-hide sections.
(function () {
    var pad = function (n) { return String(n).padStart(2, "0"); };

    function setValue(id, value) {
        var input = document.getElementById(id);
        if (!input) return;
        input.value = value;
        input.dispatchEvent(new Event("change", { bubbles: true }));
    }

    document.addEventListener("click", function (e) {
        var link = e.target.closest("a.js-today, a.js-now, a.js-set, a.js-toggle");
        if (!link) return;
        e.preventDefault();

        var now = new Date();

        if (link.classList.contains("js-today")) {
            setValue(link.dataset.target, now.getFullYear() + "-" + pad(now.getMonth() + 1) + "-" + pad(now.getDate()));
        }
        else if (link.classList.contains("js-now")) {
            setValue(link.dataset.target, pad(now.getHours()) + ":" + pad(now.getMinutes()) + ":" + pad(now.getSeconds()));
        }
        else if (link.classList.contains("js-set")) {
            setValue(link.dataset.target, link.dataset.value);
        }
        else if (link.classList.contains("js-toggle")) {
            var sections = document.querySelectorAll(".js-" + link.dataset.section);
            var flag = document.getElementById(link.dataset.flag);
            var open = flag && flag.value.toLowerCase() === "true";
            open = !open;

            sections.forEach(function (el) { el.hidden = !open; });
            if (flag) flag.value = open ? "true" : "false";
            link.textContent = open ? link.dataset.remove : link.dataset.add;
        }
    });
})();
