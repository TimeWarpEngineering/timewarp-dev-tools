#region Purpose
// Reads the mode FluentUI has stored so the theme switch label matches what the page shows.
#endregion

#region Design
// A new circuit starts with ShellState.Theme = System while the browser may have a stored Dark or
// Light mode from an earlier visit. The shell dispatches this once, after its first interactive
// render. No stored settings leaves System.
#endregion

namespace TimeWarp.DevTools.Shell.Features.Shell;

partial class ShellState
{
  /// <summary>Copies FluentUI's stored theme mode into <see cref="Theme"/>.</summary>
  public static class SyncThemeActionSet
  {
    /// <summary>The sync-theme action.</summary>
    public sealed class Action : IAction;

    /// <summary>Reads the stored mode.</summary>
    public sealed class Handler(IStore store, IThemeService themeService) : StateActionHandler<Action>(store)
    {
      private ShellState ShellState => Store.GetState<ShellState>();

      /// <inheritdoc />
      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        ThemeSettings? settings = await themeService.GetThemeSettingsAsync();
        ShellState.Theme = settings?.Mode ?? ThemeMode.System;
      }
    }
  }
}
