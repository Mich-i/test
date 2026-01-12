using Projects;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> server = builder.AddProject<ChatTool_Server>("server");
IResourceBuilder<ProjectResource> client = builder.AddProject<ChatTool_Client>("client")
    .WithReference(server);

builder.Build().Run();