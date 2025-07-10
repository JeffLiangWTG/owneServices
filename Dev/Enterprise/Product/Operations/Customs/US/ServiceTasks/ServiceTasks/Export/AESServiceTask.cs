using System.Threading;
using CargoWise.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.AES,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.AES,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.AESServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates + "," + Enterprise.Core.Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.AES,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsExport },
	"US Customs AES export interchanges inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.AES,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Transmit,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsExport,
				EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.AES.CommodityShipment,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs AES export messages outbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.AES,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsExport,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs AES export messages inbound"
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class AESServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		protected override string CurrentServiceTaskCode => Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.AES;
		protected override void RunMainTask(CancellationToken token)
		{
			DisposableEnvironment.GetActiveCompanies(USCustomsJurisdiction.Countries).ForEach(companyCode =>
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForCompany(companyCode))
				{
					new AESTIROutgoingMessageProcessor(Logger).ProcessMessage(token);
					new AESTIRIncomingMessageProcessor() { Logger = this.Logger }.ExecuteBatch(token);
				}
			});
		}
	}
}
