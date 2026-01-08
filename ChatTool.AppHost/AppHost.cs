using Projects;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> server = builder.AddProject<ChatTool_Server>("server")
    .WithEndpoint("https", e => { e.Port = 7033; e.IsExternal = true; })
    .WithEndpoint("http", e => { e.Port = 5021; e.IsExternal = true; });

IResourceBuilder<ProjectResource> client = builder.AddProject<ChatTool_Client>("client")
    .WithReference(server)
    .WithEndpoint("https", e => { e.Port = 7000; e.IsExternal = true; })
    .WithEndpoint("http", e => { e.Port = 5000; e.IsExternal = true; });

builder.Build().Run();