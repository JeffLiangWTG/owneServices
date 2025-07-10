using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GatewayChargeDefaultDebtorConfigurationRegistryItem : StronglyTypedRegistryItem<GatewayChargeDefaultDebtorConfigurationCollection>
	{
		public GatewayChargeDefaultDebtorConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new GatewayChargeDefaultDebtorConfigurationRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		public class GatewayChargeDefaultDebtorConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public GatewayChargeDefaultDebtorConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new GatewayChargeDefaultDebtorConfigurationRegistryDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultCollection = new GatewayChargeDefaultDebtorConfigurationCollection();

				defaultCollection.SuspendValidation();
				GetDefaultValues(defaultCollection);
				defaultCollection.ResumeValidation();
				return defaultCollection;
			}

			static void GetDefaultValues(GatewayChargeDefaultDebtorConfigurationCollection defaultCollection)
			{
				var configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = "ALL";
				configuration.ConsolPaymentTerm = Core.Constants.PaymentType.Prepaid;
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.SendingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = "ALL";
				configuration.ConsolPaymentTerm = Core.Constants.PaymentType.Collect;
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.ReceivingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.Loading;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.SendingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.Origin;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.SendingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerage;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.SendingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerageOnly;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.SendingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.Destination;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.ReceivingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.Unloading;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.ReceivingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.ReceivingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.BrokerageOnly;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.ReceivingAgent;

				configuration = defaultCollection.AddNew();
				configuration.ConsolDirection = "ALL";
				configuration.ConsolTransportMode = "ALL";
				configuration.ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
				configuration.ConsolPaymentTerm = "ALL";
				configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.All;
				configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.All;
				configuration.Debtor = Core.Constants.GatewayDebtor.Codes.ReceivingAgent;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.GatewayChargeDefaultDebtorRegistryItemEditor, Enterprise.Accounting.GUI")]
	class GatewayChargeDefaultDebtorConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GatewayChargeDefaultDebtorConfigurationCollection>
	{
		public GatewayChargeDefaultDebtorConfigurationRegistryDataType()
		{
		}
	}
}
