using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class GatewayChargeDefaultDebtorConfigurationLookups : ZLookups
	{
		public GatewayChargeDefaultDebtorConfigurationLookups(GatewayChargeDefaultDebtorConfiguration parent)
			: base(parent)
		{
		}

		public new GatewayChargeDefaultDebtorConfiguration Parent => (GatewayChargeDefaultDebtorConfiguration)base.Parent;

		public const string All = "ALL";
		MultilingualString AllChargeGroups => ResString.GetMultilingualString("A3A5C5DD-424C-429B-8577-62E9444534EA", "All Charge Groups");
		MultilingualString AllPaymentTypes => ResString.GetMultilingualString("CF8E2162-94FD-47D0-8FDB-3DF680631FA0", "All Payment Types");
		MultilingualString AllTransportModes => ResString.GetMultilingualString("5D20318F-BD59-4E4E-94E7-1D35736E11CA", "All Transport Modes");

		public CodeDescriptionPairList ConsolDirectionList
		{
			get
			{
				if (fConsolDirectionList == null)
				{
					fConsolDirectionList = new CodeDescriptionPairList();
					fConsolDirectionList.AddPair(FreightShipmentDirection.Code.All, FreightShipmentDirection.Description.All);
					fConsolDirectionList.AddPair(FreightShipmentDirection.Code.Export, FreightShipmentDirection.Description.Export);
					fConsolDirectionList.AddPair(FreightShipmentDirection.Code.Import, FreightShipmentDirection.Description.Import);
					fConsolDirectionList.AddPair(FreightShipmentDirection.Code.Domestic, FreightShipmentDirection.Description.Domestic);
					fConsolDirectionList.AddPair(FreightShipmentDirection.Code.Other, FreightShipmentDirection.Description.Other);
				}
				return fConsolDirectionList;
			}
		}
		CodeDescriptionPairList fConsolDirectionList;

		public CodeDescriptionPairList ConsolTransportModeList
		{
			get
			{
				if (fConsolTransportModeList == null)
				{
					fConsolTransportModeList = new CodeDescriptionPairList();
					fConsolTransportModeList.AddPair(All, AllTransportModes);
					fConsolTransportModeList.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
					fConsolTransportModeList.RemoveCode(TransportModes.SeaAir);
					fConsolTransportModeList.RemoveCode(TransportModes.AirSea);
					fConsolTransportModeList.RemoveCode(TransportModes.Courier);
				}
				return fConsolTransportModeList;
			}
		}
		CodeDescriptionPairList fConsolTransportModeList;

		public CodeDescriptionPairList ChargeGroupList
		{
			get
			{
				if (fChargeGroupList == null)
				{
					fChargeGroupList = new CodeDescriptionPairList();
					{
						fChargeGroupList.AddPair(All, AllChargeGroups);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Descriptions.Origin);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Descriptions.Loading);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Descriptions.Freight);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Insurance, ChargeCodeGroupList.Descriptions.Insurance);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Descriptions.Unloading);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Descriptions.Destination);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Descriptions.OriginBrokerage);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Descriptions.Brokerage);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.CustomsDuty, ChargeCodeGroupList.Descriptions.CustomsDuty);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargeCodeGroupList.Descriptions.OriginBrokerageOnly);
						fChargeGroupList.AddPair(ChargeCodeGroupList.Codes.BrokerageOnly, ChargeCodeGroupList.Descriptions.BrokerageOnly);
					}
				}
				return fChargeGroupList;
			}
		}
		CodeDescriptionPairList fChargeGroupList;

		public CodeDescriptionPairList ConsolPaymentTermList
		{
			get
			{
				if (fConsolPaymentTermList == null)
				{
					fConsolPaymentTermList = new CodeDescriptionPairList();
					fConsolPaymentTermList.AddPair(All, AllPaymentTypes);
					fConsolPaymentTermList.AddPair(PaymentType.Prepaid, ResString.GetMultilingualString("6DCB65F2-3348-41AB-AA07-C6355CDF5F57", "Prepaid Agent"));
					fConsolPaymentTermList.AddPair(PaymentType.Collect, ResString.GetMultilingualString("7D3F56FE-F45D-4607-9D55-CF749A717B7F", "Collect Agent"));
				}
				return fConsolPaymentTermList;
			}
		}
		CodeDescriptionPairList fConsolPaymentTermList;

		public CodeDescriptionPairList RelatedJobList
		{
			get
			{
				if (relatedJobList == null)
				{
					relatedJobList = new CodeDescriptionPairList();
					relatedJobList.AddPair(GatewayRelatedJob.Codes.All, GatewayRelatedJob.Descriptions.AllRelatedJob);
					relatedJobList.AddPair(GatewayRelatedJob.Codes.RelatedToShipment, GatewayRelatedJob.Descriptions.RelatedToShipment);
					relatedJobList.AddPair(GatewayRelatedJob.Codes.NotRelatedToJob, GatewayRelatedJob.Descriptions.NotRelatedToJob);
				}

				return relatedJobList;
			}
		}
		CodeDescriptionPairList relatedJobList;

		public CodeDescriptionPairList PreviousSendingAgentList
		{
			get
			{
				if (Parent.RelatedJob == GatewayRelatedJob.Codes.RelatedToShipment)
				{
					if (psaListSHP == null)
					{
						psaListSHP = new CodeDescriptionPairList();
						psaListSHP.AddPair(GatewayPreviousSendingAgent.Codes.All, GatewayPreviousSendingAgent.Descriptions.All);
						psaListSHP.AddPair(GatewayPreviousSendingAgent.Codes.SendingAgent, GatewayPreviousSendingAgent.Descriptions.SendingAgent);
						psaListSHP.AddPair(GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent, GatewayPreviousSendingAgent.Descriptions.NoPrevSendingAgent);
						psaListSHP.AddPair(GatewayPreviousSendingAgent.Codes.GatewayAgent, GatewayPreviousSendingAgent.Descriptions.GatewayAgent);
						psaListSHP.AddPair(GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT, GatewayPreviousSendingAgent.Descriptions.GatewayAgentWithFT);
					}

					return psaListSHP;
				}
				else
				{
					if (psaListNonSHP == null)
					{
						psaListNonSHP = new CodeDescriptionPairList();
						psaListNonSHP.AddPair(GatewayPreviousSendingAgent.Codes.All, GatewayPreviousSendingAgent.Descriptions.All);
					}

					return psaListNonSHP;
				}
			}
		}
		CodeDescriptionPairList psaListSHP, psaListNonSHP;

		public CodeDescriptionPairList DebtorList
		{
			get
			{
				if (Parent.RelatedJob == GatewayRelatedJob.Codes.RelatedToShipment)
				{
					if (debtorListSHP == null)
					{
						debtorListSHP = new CodeDescriptionPairList();
						debtorListSHP.AddPair(GatewayDebtor.Codes.SendingAgent, GatewayDebtor.Descriptions.SendingAgent);
						debtorListSHP.AddPair(GatewayDebtor.Codes.ReceivingAgent, GatewayDebtor.Descriptions.ReceivingAgent);
						debtorListSHP.AddPair(GatewayDebtor.Codes.ShipmentPickupAgent, GatewayDebtor.Descriptions.ShipmentPickupAgent);
						debtorListSHP.AddPair(GatewayDebtor.Codes.ShipmentDeliveryAgent, GatewayDebtor.Descriptions.ShipmentDeliveryAgent);
						debtorListSHP.AddPair(GatewayDebtor.Codes.PreviousSendingAgent, GatewayDebtor.Descriptions.PreviousSendingAgent);
					}

					return debtorListSHP;
				}
				else
				{
					if (debtorListNonSHP == null)
					{
						debtorListNonSHP = new CodeDescriptionPairList();
						debtorListNonSHP.AddPair(GatewayDebtor.Codes.SendingAgent, GatewayDebtor.Descriptions.SendingAgent);
						debtorListNonSHP.AddPair(GatewayDebtor.Codes.ReceivingAgent, GatewayDebtor.Descriptions.ReceivingAgent);
					}

					return debtorListNonSHP;
				}
			}
		}
		CodeDescriptionPairList debtorListSHP, debtorListNonSHP;
	}
}
