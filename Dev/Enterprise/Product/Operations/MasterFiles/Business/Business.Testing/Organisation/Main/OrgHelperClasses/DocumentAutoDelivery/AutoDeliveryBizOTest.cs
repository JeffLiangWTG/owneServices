using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoDeliveryBizO))]
	sealed class AutoDeliveryBizOTest : DocumentSupporterTest
	{
		// BG - There was no testing for this.

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "~CODEX";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_IsForwarder = true;

			var newAppAgPorts = orgHeader.AppointedAgentPorts.AddNew();
			newAppAgPorts.O5_PortOrCountry = "AUSYD";
			newAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			newAppAgPorts.O5_OA_AgentOfficeAddress = orgHeader.MainAddress.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "~CODEY";
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUSYD";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "~CODEZ";
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUSYD";

			var supplierLink = orgHeader.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;
			supplierLink.SelectedForPrinting = true;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";

			var buyerLink = orgHeader.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = consignee.PK;
			buyerLink.SelectedForPrinting = true;
			buyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";

			Factory.Save();

			return orgHeader;
		}

		#endregion
	}
}
