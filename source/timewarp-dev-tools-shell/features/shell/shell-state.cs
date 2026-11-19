#region Purpose
// The shell's own UI state: the theme mode chosen with the theme switch.
#endregion

#region Design
// TimeWarp.State store owned by the shell. It is not part of the plugin contract and is separate
// from any plugin's state. Theme is the FluentUI ThemeMode: System follows the browser.
#endregion

namespace TimeWarp.DevTools.Shell.Features.Shell;

/// <summary>Shell UI state.</summary>
public sealed partial class ShellState : State<ShellState>
{
  /// <summary>Theme mode the shell asked FluentUI to apply.</summary>
  public ThemeMode Theme { get; private set; } = ThemeMode.System;

  /// <inheritdoc />
  public override void Initialize() => Theme = ThemeMode.System;

  /// <summary>Theme after <paramref name="theme"/> in the system, dark, light cycle.</summary>
  /// <param name="theme">Current mode.</param>
  public static ThemeMode NextTheme(ThemeMode theme) =>
    theme switch
    {
      ThemeMode.System => ThemeMode.Dark,
      ThemeMode.Dark => ThemeMode.Light,
      _ => ThemeMode.System
    };

  /// <summary>Label the theme switch shows for <paramref name="theme"/>.</summary>
  /// <param name="theme">Mode to describe.</param>
  public static string ThemeLabel(ThemeMode theme) =>
    theme switch
    {
      ThemeMode.Dark => "Dark",
      ThemeMode.Light => "Light",
      _ => "System"
    };
}
