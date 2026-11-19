#region Purpose
// Advances the theme: system, dark, light, then system again, and applies it through FluentUI.
#endregion

#region Design
// The theme switch dispatches this with no argument, so the order lives here and not in markup.
// IThemeService stores the mode and sets body[data-theme]; the shell's theme script reads the
// stored mode on the next first paint.
#endregion

namespace TimeWarp.DevTools.Shell.Features.Shell;

partial class ShellState
{
  /// <summary>Moves <see cref="Theme"/> to the next mode.</summary>
  public static class CycleThemeActionSet
  {
    /// <summary>The cycle-theme action.</summary>
    public sealed class Action : IAction;

    /// <summary>Writes the next mode and applies it.</summary>
    public sealed class Handler(IStore store, IThemeService themeService) : StateActionHandler<Action>(store)
    {
      private ShellState ShellState => Store.GetState<ShellState>();

      /// <inheritdoc />
      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        ThemeMode next = NextTheme(ShellState.Theme);
        ShellState.Theme = next;
        await themeService.SetThemeAsync(next);
      }
    }
  }
}
