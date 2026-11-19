#region Purpose
// Joins the shell assembly to the host's ClientPipeline mediator graph.
#endregion

#region Design
// The shell is a library: it does not reference TimeWarp.Mediator.Generators. The host does, and
// its generated AddGeneratedMediator<ClientPipeline>() registers the shell's state handlers
// because this assembly opts in with MediatorAssembly. MediatorScope puts those handlers on
// ClientPipeline, the TimeWarp.State store pipeline.
#endregion

[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(ClientPipeline))]
