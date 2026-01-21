using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<ChatTool_Server>("server");

builder.Build().Run();