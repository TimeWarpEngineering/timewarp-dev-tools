#!/usr/bin/env -S dotnet --
#:package TimeWarp.Jaribu
#:package Shouldly
#:project $(SourceDirectory)timewarp-dev-tools-abstractions/timewarp-dev-tools-abstractions.csproj
#:property PublishAot=false

#region Purpose
// The plugin contract assembly references no TimeWarp.State package and no shell or plugin assembly.
#endregion

#region Design
// Two views of the same rule. The compiled assembly's references catch a type the contract uses.
// The restore graph (obj/project.assets.json) catches a package the contract pulls in without
// using it, including a transitive one. Paths come from this file's [CallerFilePath]; the tests
// props turn ContinuousIntegrationBuild off so that path is real.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DevToolsContract_
{
  using System.Reflection;
  using System.Text.Json;
  using TimeWarp.DevTools;

  [TestTag("Contract")]
  public class References_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<References_Given_>();

    public static async Task CompiledAssembly_Should_ReferenceNoTimeWarpAssembly()
    {
      string[] names = [.. typeof(DevToolsPlugin).Assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? "")];

      names.ShouldNotBeEmpty();
      names.ShouldAllBe(name => !name.StartsWith("TimeWarp.", StringComparison.Ordinal));
      await Task.CompletedTask;
    }

    public static async Task RestoreGraph_Should_HaveNoTimeWarpStatePackage()
    {
      string assets = Path.Combine(RepoRoot(), "source", "timewarp-dev-tools-abstractions", "obj", "project.assets.json");
      File.Exists(assets).ShouldBeTrue($"Restore output not found: {assets}");

      using JsonDocument document = JsonDocument.Parse(await File.ReadAllTextAsync(assets));
      string[] libraries = [.. document.RootElement.GetProperty("libraries").EnumerateObject().Select(library => library.Name)];

      libraries.ShouldAllBe(library => !library.StartsWith("TimeWarp.State", StringComparison.OrdinalIgnoreCase));
      libraries.ShouldAllBe(library => !library.StartsWith("TimeWarp.DevTools", StringComparison.OrdinalIgnoreCase));
    }

    private static string RepoRoot([CallerFilePath] string callerPath = "")
    {
      DirectoryInfo? directory = new FileInfo(callerPath).Directory;
      while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "timewarp-dev-tools.slnx")))
      {
        directory = directory.Parent;
      }

      return directory?.FullName ?? throw new InvalidOperationException($"No timewarp-dev-tools.slnx above {callerPath}.");
    }
  }
}
