#region Purpose
// The rule for a plugin id: kebab-case, so it is a valid single route segment.
#endregion

#region Design
// Lowercase letters and digits in hyphen-separated words. No leading, trailing, or double hyphen.
// That excludes every reserved prefix the shell uses (_devtools, _framework, _content).
#endregion

namespace TimeWarp.DevTools;

/// <summary>Validates <see cref="DevToolsPlugin.Id"/>.</summary>
public static partial class DevToolsPluginId
{
  /// <summary>Regular expression a plugin id must match.</summary>
  public const string Pattern = "^[a-z0-9]+(-[a-z0-9]+)*$";

  /// <summary>True when <paramref name="id"/> is a kebab-case plugin id.</summary>
  /// <param name="id">Candidate id.</param>
  public static bool IsValid(string? id) => id is not null && IdRegex().IsMatch(id);

  [GeneratedRegex(Pattern, RegexOptions.CultureInvariant)]
  private static partial Regex IdRegex();
}
