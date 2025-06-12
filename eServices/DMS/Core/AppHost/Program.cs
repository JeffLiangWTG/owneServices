var builder = DistributedApplication.CreateBuilder(args);

var configapi = builder.AddProject<Projects.eServices_Dms_Core_ConfigApi>("configapi");

var storageapi = builder.AddProject<Projects.eServices_Dms_Core_StorageApi>("storageapi");

var messagesapi = builder.AddProject<Projects.eServices_Dms_Core_MessagesApi>("messagesapi");

builder.AddProject<Projects.eServices_Dms_Core_OpsPortal>("opsportal")
	.WithReference(messagesapi);

builder.Build().Run();
