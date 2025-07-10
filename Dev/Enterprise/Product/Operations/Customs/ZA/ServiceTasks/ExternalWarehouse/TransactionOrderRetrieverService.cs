using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ZA.ServiceTasks.TransactionOrderRetrieverService.Code,
	Enterprise.Customs.ZA.ServiceTasks.TransactionOrderRetrieverService.FriendlyName,
	Enterprise.Customs.ZA.ServiceTasks.TransactionOrderRetrieverService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ZA.ServiceTasks.TransactionOrderRetrieverService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ZA.ServiceTasks.TransactionOrderRetrieverService.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GenericMessageDelivery },
	"ZA Transaction Orders interchanges inbound"
	)]

namespace Enterprise.Customs.ZA.ServiceTasks
{
	class TransactionOrderRetrieverService : Customs.ServiceTasks.GMDCustomsMessagingService
	{
		public const string Code = "ZTR";
		public const string FriendlyName = "ZA Transaction Orders Message Retriever";
		public const string MessageServiceTaskCategory = "ZAC";

		protected override IEnumerable<ZString> InterchangeTypes => new ZString[] { GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse };

		protected override GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor() => new ExternalWarehouseInboundInterchangeProcessor();
	}
}
