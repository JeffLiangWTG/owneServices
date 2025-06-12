var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.eServices_Dms_Usc_Outbound>("outbound");

builder.AddProject<Projects.eServices_Dms_Usc_Inbound>("inbound");

builder.AddProject<Projects.eServices_Dms_Usc_Inbound_MQ>("inbound-mq");

builder.AddProject<Projects.eServices_Dms_Usc_RefFiles>("reffiles");

builder.Build().Run();
