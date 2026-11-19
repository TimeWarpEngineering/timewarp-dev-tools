#region Purpose
// Product name and version strings shown by the shell.
#endregion

#region Design
// Version is the assembly's informational version with any "+commit" build metadata removed,
// falling back to the assembly version. The same helper reads plugin assembly versions for About.
#endregion

namespace TimeWarp.DevTools.Shell;

/// <summary>Names and versions the shell displays.</summary>
public static class DevToolsShellInfo
{
  /// <summary>Product name shown in the shell header.</summary>
  public const string ProductName = "TimeWarp DevTools";

  /// <summary>Version of the shell assembly.</summary>
  public static string ShellVersion => VersionOf(typeof(DevToolsShellInfo).Assembly);

  /// <summary>Informational version of <paramref name="assembly"/> without build metadata.</summary>
  /// <param name="assembly">The assembly to read.</param>
  public static string VersionOf(Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);
    string? informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (!string.IsNullOrWhiteSpace(informational))
    {
      int plus = informational.IndexOf('+', StringComparison.Ordinal);
      return plus < 0 ? informational : informational[..plus];
    }

    return assembly.GetName().Version?.ToString() ?? "0.0.0";
  }
}
