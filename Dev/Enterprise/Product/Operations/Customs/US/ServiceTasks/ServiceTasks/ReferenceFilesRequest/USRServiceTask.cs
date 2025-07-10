using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.ReferenceFileUpdate,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.USRServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates + "," + Enterprise.Core.Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs AC messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs CS messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs IT messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs FR messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs FO messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs VR messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs WR messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.QueryQuotaResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs UR messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs QB messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs HZ messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs HY messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs NR messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.DailyStatement,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs PF messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs PZ messages reference file update")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	}, "US Customs MS messages reference file update")]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class USRServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		protected override string CurrentServiceTaskCode => ServiceTaskApplicationCodeList.Codes.ReferenceFileUpdate;
		protected override void RunMainTask(CancellationToken token)
		{
			if (!ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now))
			{
				foreach (var company in GlbCompany.GetActiveCompanies(company => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(company.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates))
				{
					using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
					{
						var processor = new USRIncomingMessageProcessor() { Logger = this.Logger };
						processor.ExecuteBatch(token);
					}
				}
			}
		}
	}
}
