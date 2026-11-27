// #region Purpose
// TimeWarp DevTools first-paint theme. Loaded as the first child of <body>, before any content.
// #endregion
// #region Design
// Sets body[data-theme] to the mode FluentUI stored (light or dark), else to the browser
// preference. It never stores a mode, so with nothing stored FluentUI keeps following the OS.
// #endregion
(function () {
  var mode = null;
  try {
    var stored = window.localStorage.getItem("fluentui-blazor:theme-settings");
    if (stored) {
      var parsed = JSON.parse(stored);
      var value = String(parsed && (parsed.mode !== undefined ? parsed.mode : parsed.Mode)).toLowerCase();
      if (value === "light" || value === "dark") {
        mode = value;
      }
    }
  } catch (e) {
    mode = null;
  }

  if (mode === null) {
    var dark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
    mode = dark ? "dark" : "light";
  }

  document.body.setAttribute("data-theme", mode);
})();
