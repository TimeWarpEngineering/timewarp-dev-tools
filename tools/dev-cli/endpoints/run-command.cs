#region Purpose
// Starts the standalone DevTools host.
#endregion
#region Design
// `dev run` runs `dotnet run --project source/timewarp-dev-tools-host` from the repository root
// through TimeWarp.Amuru and streams that process to the terminal. Git.FindRoot() resolves the
// root, so the command works from any directory inside the repo. The host's launch profile pins
// http://localhost:5290; the command prints that URL before the build starts.
// Ctrl+C cancels the command token; Amuru then stops the child. A cancel, or an exit from that
// signal (130, 143), is a stop, not a failure. Same shape as timewarp-ramani's run command.
#endregion

namespace DevCli.Commands;

[NuruRoute("run", Description = "Build and run the standalone DevTools host (http://localhost:5290)")]
public sealed class RunCommand : ICommand<Unit>
{
  internal const string HostUrl = "http://localhost:5290";

  internal sealed class Handler : IRequestHandler<RunCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(RunCommand command, CancellationToken cancellationToken)
    {
      string? root = Git.FindRoot();
      if (root is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string project = Path.Combine("source", "timewarp-dev-tools-host");
      if (!Directory.Exists(Path.Combine(root, project)))
      {
        Terminal.WriteErrorLine($"Error: host project not found: {Path.Combine(root, project)}");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      Terminal.WriteLine($"Running the standalone DevTools host from {root}...");
      Terminal.WriteLine($"DevTools: {HostUrl} (plugins: {HostUrl}/_devtools/plugins). Press Ctrl+C to stop.".Green());

      CommandResult host = DotNet.Run()
        .WithProject(project)
        .WithWorkingDirectory(root)
        .WithNoValidation()
        .Build();

      try
      {
        int exitCode = await host.RunAsync(cancellationToken);
        if (exitCode is 0 or 130 or 143)
        {
          Terminal.WriteLine("DevTools host stopped.");
          return Unit.Value;
        }

        Terminal.WriteErrorLine($"DevTools host exited with code {exitCode}.".Red());
        Environment.ExitCode = exitCode;
        return Unit.Value;
      }
      catch (OperationCanceledException)
      {
        Terminal.WriteLine("DevTools host stopped.");
        return Unit.Value;
      }
    }
  }
}
