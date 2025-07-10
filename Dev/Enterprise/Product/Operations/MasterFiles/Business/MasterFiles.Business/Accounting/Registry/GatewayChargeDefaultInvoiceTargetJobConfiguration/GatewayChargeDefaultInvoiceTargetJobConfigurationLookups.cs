using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class GatewayChargeDefaultInvoiceTargetJobConfigurationLookups : ZLookups
	{
		public GatewayChargeDefaultInvoiceTargetJobConfigurationLookups(GatewayChargeDefaultInvoiceTargetJobConfiguration parent)
			: base(parent)
		{
		}

		public const string All = "ALL";
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

		public CodeDescriptionPairList PreviousSendingAgentTypeList
		{
			get
			{
				if (fPreviousSendingAgentTypeList == null)
				{
					fPreviousSendingAgentTypeList = new CodeDescriptionPairList();
					{
						fPreviousSendingAgentTypeList.AddPair(GatewayPreviousSendingAgent.Codes.All, GatewayPreviousSendingAgent.Descriptions.All);
						fPreviousSendingAgentTypeList.AddPair(GatewayPreviousSendingAgent.Codes.SendingAgent, GatewayPreviousSendingAgent.Descriptions.SendingAgent);
						fPreviousSendingAgentTypeList.AddPair(GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent, GatewayPreviousSendingAgent.Descriptions.NoPrevSendingAgent);
						fPreviousSendingAgentTypeList.AddPair(GatewayPreviousSendingAgent.Codes.GatewayAgent, GatewayPreviousSendingAgent.Descriptions.GatewayAgent);
						fPreviousSendingAgentTypeList.AddPair(GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT, GatewayPreviousSendingAgent.Descriptions.GatewayAgentWithFT);
					}
				}
				return fPreviousSendingAgentTypeList;
			}
		}
		CodeDescriptionPairList fPreviousSendingAgentTypeList;

		public CodeDescriptionPairList InvoiceTargetJobTypeList
		{
			get
			{
				if (fInvoiceTargetJobTypeList == null)
				{
					fInvoiceTargetJobTypeList = new TargetJobDefaultingOptions();
				}
				return fInvoiceTargetJobTypeList;
			}
		}
		CodeDescriptionPairList fInvoiceTargetJobTypeList;
	}
}
