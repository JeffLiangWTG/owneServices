using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: AssemblyTitle("Telematics Service Tasks")]
[assembly: AssemblyDescription("Telematics Service Tasks")]
[assembly: AssemblyConfiguration("")]

[assembly: HostedService(
	TelematicsDataProcessingServiceTask.Code,
	"Process incoming Telematics data",
	"TEL",
	typeof(TelematicsDataProcessingServiceTask),
	MinimumPeriod = "1second",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(
	TelematicsDataProcessingServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.Telematics,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + TelematicsMessageList.Codes.ProtobufData,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y"
	},
	"Protobuf messages inbound")]

[assembly: HostedServiceBusinessObjectBinding(
	TelematicsDataProcessingServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.Telematics,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + TelematicsMessageList.Codes.TelematicsXmlData,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y"
	},
	"Telematics XML messages inbound")]

[assembly: HostedService(
	TelematicsCleanObsoleteDeviceDataServiceTask.Code,
	"Clean obsolete device data from the database",
	"TEL",
	typeof(TelematicsCleanObsoleteDeviceDataServiceTask),
	MinimumPeriod = "1day",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Saturday },
	DefaultScheduleStartAtLocal = "23hours"
	)]

[assembly: HostedService(
	TelematicsRimDataSendingServiceTask.Code,
	"Telematics RIM data sending task",
	"TEL",
	typeof(TelematicsRimDataSendingServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtLocal = "0seconds"
	)]

[assembly: HostedService(
	TelematicsGpsLocationRoadTypeServiceTask.Code,
	"Mark GPS locations as public or private",
	"TEL",
	typeof(TelematicsGpsLocationRoadTypeServiceTask),
	MinimumPeriod = "1second",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(
	TelematicsGpsLocationRoadTypeServiceTask.Code,
	GlbDeviceLocationSchema.Constants.TableName,
	new[]
	{
		GlbDeviceLocationSchema.Constants.V2_RoadType + "=" + GlbDeviceLocationRoadTypes.Codes.Unknown
	},
	"Gps Locations with unknown road type")]

[assembly: HostedService(
	TelematicsPreDriveChecklistNotificationTask.Code,
	"Alert users for failed Pre-Drive checklist",
	"TEL",
	typeof(TelematicsPreDriveChecklistNotificationTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(
	TelematicsPreDriveChecklistNotificationTask.Code,
	TelPreDriveChecklistHeaderSchema.Constants.TableName,
	new[]
	{
		TelPreDriveChecklistHeaderSchema.Constants.TPH_IsProcessed + "=0"
	},
	"Unprocessed Pre-Drive checklists")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Telematics.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
#endif
