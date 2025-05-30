var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.IdempotencyDemo_API>("idempotencydemo-api")
	.WithReference(cache);

builder.Build().Run();
