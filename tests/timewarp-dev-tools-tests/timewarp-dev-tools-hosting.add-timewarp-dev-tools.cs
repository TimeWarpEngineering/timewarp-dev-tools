#!/usr/bin/env -S dotnet --
#:package TimeWarp.Jaribu
#:package Shouldly
#:project $(SourceDirectory)timewarp-dev-tools-hosting/timewarp-dev-tools-hosting.csproj
#:property PublishAot=false

#region Purpose
// Aspire model test for AddTimeWarpDevTools: run mode adds the DevTools host with a labelled http
// endpoint, Development environment, and no manifest entry; publish mode adds nothing.
#endregion

#region Design
// Model only, like timewarp-architecture's state-dev-tools-model-tests.cs: the builder is created
// with the operation's arguments and never built, so nothing starts. Environment callbacks run
// against a run-mode context. The host project is this repo's standalone host; both overloads
// (csproj path and IProjectMetadata) are checked. Literals, not the package constants, so a
// changed constant fails here.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TimeWarpDevToolsHosting_
{
  using Aspire.Hosting;
  using Aspire.Hosting.ApplicationModel;
  using Aspire.Hosting.Dotnet;
  using TimeWarp.DevTools.Hosting;

  [TestTag("Hosting")]
  public class AddTimeWarpDevTools_Given_
  {
    private const string ResourceName = "timewarp-dev-tools";

    [ModuleInitializer]
    internal static void Register() => RegisterTests<AddTimeWarpDevTools_Given_>();

    public static async Task RunMode_Should_AddDevToolsWithLabelledHttpEndpoint()
    {
      IDistributedApplicationBuilder builder = NewBuilder([]);
      builder.ExecutionContext.IsRunMode.ShouldBeTrue();

      IResourceBuilder<DotnetProjectResource> devtools = builder.AddTimeWarpDevTools(HostProjectPath());

      DotnetProjectResource resource = builder.Resources.OfType<DotnetProjectResource>().Single(r => r.Name == ResourceName);
      resource.ShouldBeSameAs(devtools.Resource);
      resource.Annotations.OfType<EndpointAnnotation>().Select(endpoint => endpoint.Name).ShouldBe(["http"]);
      resource.Annotations.OfType<EndpointAnnotation>().Single().UriScheme.ShouldBe("http");
      resource.Annotations.OfType<ResourceUrlsCallbackAnnotation>().ShouldNotBeEmpty();
      resource.Annotations.OfType<ManifestPublishingCallbackAnnotation>()
        .ShouldContain(annotation => annotation == ManifestPublishingCallbackAnnotation.Ignore);

      Dictionary<string, object> environment = await RunModeEnvironmentAsync(resource);
      environment["ASPNETCORE_ENVIRONMENT"].ShouldBe("Development");
    }

    public static async Task ProjectMetadata_Should_AddTheSameResource()
    {
      IDistributedApplicationBuilder builder = NewBuilder([]);

      builder.AddTimeWarpDevTools<StandaloneHostProject>("devtools");

      DotnetProjectResource resource = builder.Resources.OfType<DotnetProjectResource>().Single(r => r.Name == "devtools");
      resource.Annotations.OfType<EndpointAnnotation>().Select(endpoint => endpoint.Name).ShouldBe(["http"]);
      await Task.CompletedTask;
    }

    public static async Task PublishMode_Should_AddNothing()
    {
      IDistributedApplicationBuilder builder = NewBuilder(["--operation", "publish"]);
      builder.ExecutionContext.IsPublishMode.ShouldBeTrue();

      IResourceBuilder<DotnetProjectResource> devtools = builder.AddTimeWarpDevTools(HostProjectPath())
        .WithEnvironment("CHAINED", "still-works");

      devtools.Resource.Name.ShouldBe(ResourceName);
      builder.Resources.ShouldNotContain(resource => resource.Name == ResourceName);
      builder.Resources.ShouldNotContain(resource => resource == devtools.Resource);
      await Task.CompletedTask;
    }

    private static IDistributedApplicationBuilder NewBuilder(string[] args) =>
      DistributedApplication.CreateBuilder(new DistributedApplicationOptions { Args = args, DisableDashboard = true });

    private static async Task<Dictionary<string, object>> RunModeEnvironmentAsync(IResource resource)
    {
      EnvironmentCallbackContext context = new(new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run), resource);
      foreach (EnvironmentCallbackAnnotation callback in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
      {
        await callback.Callback(context);
      }

      return context.EnvironmentVariables.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    internal static string HostProjectPath([CallerFilePath] string callerPath = "")
    {
      DirectoryInfo? directory = new FileInfo(callerPath).Directory;
      while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "timewarp-dev-tools.slnx")))
      {
        directory = directory.Parent;
      }

      string root = directory?.FullName ?? throw new InvalidOperationException($"No timewarp-dev-tools.slnx above {callerPath}.");
      return Path.Combine(root, "source", "timewarp-dev-tools-host", "timewarp-dev-tools-host.csproj");
    }
  }

  public sealed class StandaloneHostProject : IProjectMetadata
  {
    public string ProjectPath => AddTimeWarpDevTools_Given_.HostProjectPath();
  }
}
