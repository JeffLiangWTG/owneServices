using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ShippingInstruction
{
	sealed class ShippingInstructionDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AssertDocumentContentForUSExportConsol();
			}
		}

		void AssertDocumentContentForUSExportConsol()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_Code = "SSS";
			sendingForwarder.OH_FullName = "Sending Agent";
			sendingForwarder.MainAddress.CompanyName = "Sending Agent Company";
			sendingForwarder.MainAddress.OA_Address1 = "Sending Agent Address 1";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_Code = "RRR";
			receivingForwarder.OH_FullName = "Receiving Agent";
			receivingForwarder.MainAddress.CompanyName = "Receiving Agent Company";
			receivingForwarder.MainAddress.OA_Address1 = "Receiving Agent Address 1";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "SG";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.MainAddress.CompanyName = "Carrier Company";
			carrier.MainAddress.OA_Address1 = "Carrier Address 1";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			carrier.CustomsCodes.AddNew("CCC", "NUM1", "US");

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "CCC";
			creditor.OH_FullName = "CREDITOR";
			creditor.MainAddress.CompanyName = "Creditor Company";
			creditor.MainAddress.OA_Address1 = "Creditor Address 1";
			creditor.MainAddress.OA_RN_NKCountryCode = "CN";
			creditor.OH_IsCreditor = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_AgentType = "AGT";
			consol.JK_ConsolMode = "FCL";
			consol.JK_RL_NKLoadPort = "USSEA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();

			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";
			consolCost[JobConsolCostSchema.E6_OH_Creditor] = creditor.PK;

			var container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PIXU2145650";
			container.JC_RC = container40GP.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USSEA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.CustomsEntryNumberType = "ITN";
			shipment.CustomsEntryNumber = "A12345";
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			shipment.JS_GoodsValue = 101.00m;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 890;
			packline.JL_ActualWeightUQ = "KG";
			packline.JL_ActualVolume = 3;
			packline.JL_ActualVolumeUQ = "M3";
			packline.SetContainer(consol, container);

			Factory.Save();

			var entryNumbers = shipment.CusEntryNumbers;

			AssertContainsExactElementsInAnyOrder("Prerequisite; ITN number is set", new[]
			{
				"ITN|A12345"
			},
			entryNumbers.Select(n => $"{n[CusEntryNumSchema.CE_EntryType]}|{n[CusEntryNumSchema.CE_EntryNum]}"));

			var carrierRefShippingLine = Factory.New<RefShippingLine>();
			carrier.OH_RSL_ShippingLine = carrierRefShippingLine.PK;

			AssertNull(carrierRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider));

			var pivotPK = new ZGuid("f2831195-c1f7-4026-a80b-cd44de7aaeec");

			var notShowBOLDocumentationContent = CreateContent(false);
			AssertContents(consol, pivotPK, notShowBOLDocumentationContent);

			var billOfLadingProviderMessagingRequirement = carrierRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			billOfLadingProviderMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			billOfLadingProviderMessagingRequirement.RSR_IsShippingInstruction = true;

			var eblProvider = carrierRefShippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider.RSE_IsAvailable = true;
			eblProvider.RSE_IsDefault = true;
			eblProvider.RSE_Name = EBLProviderConstants.Codes.Bolero;

			Assert(carrierRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsShippingInstruction);
			AssertContents(consol, pivotPK, CreateContent(true));

			billOfLadingProviderMessagingRequirement.RSR_IsShippingInstruction = false;

			Assert(!carrierRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsShippingInstruction);
			AssertContents(consol, pivotPK, notShowBOLDocumentationContent);
		}

		string CreateContent(bool isShowBOLDocumentation)
		{
			var bolDocumentationContent = isShowBOLDocumentation ? @"
[84,39] eBL Provider
[85,39] Not Listed" : "";

			return
$@"[2,5] Shipper
[2,17] Carrier
[2,28] SHIPPING INSTRUCTION
[3,5] SENDING AGENT COMPANY
[3,17] CARRIER COMPANY
[4,5] SENDING AGENT ADDRESS 1
[4,17] CARRIER ADDRESS 1
[7,5] AUSTRALIA
[7,17] AUSTRALIA
[13,5] Consignee
[13,17] Notify Party
[13,29] Carrier Booking References
[14,5] RECEIVING AGENT COMPANY
[15,5] RECEIVING AGENT ADDRESS 1
[17,29] Release Type
[17,41] No of Originals
[17,47] No of Copies
[18,5] SINGAPORE
[18,29] BOL Original
[18,41] 3
[18,47] 3
[19,29] Requested Date of Issue
[19,41] Place of Issue
[24,5] Vessel
[24,17] Lloyds/IMO
[24,29] Voyage
[24,41] Mode
[24,45] Pickup/Delivery
[25,41] FCL
[25,45] ☐ Door Pickup
[26,45] ☐ Door Delivery
[27,5] Port of Load
[27,17] Port of Discharge
[27,29] Origin
[27,41] Destination
[28,5] USSEA - SEATTLE, WA, UNITED STATES
[28,17] AUSYD - SYDNEY, AUSTRALIA
[28,29] USSEA - SEATTLE, WA, UNITED STATES
[28,41] AUSYD - SYDNEY, AUSTRALIA
[29,5] Place of Receipt
[29,17] Place of Delivery
[29,29] Goods Value
[29,41] BOL Currency
[30,5] USSEA - SEATTLE, WA, UNITED STATES
[30,17] AUSYD - SYDNEY, AUSTRALIA
[30,29] 101.00
[30,41] USD
[32,5] Goods and Equipment Details 
[33,5] Container
[33,17] ISO Type
[33,21] Cargo Wt (KG)
[33,29] Tare (KG)
[33,37] Packs
[33,45] Volume (M3)
[34,5] PIXU2145650 
[34,17] 42G0 
[34,21] 890.000
[34,29] 3830.000
[34,37] 10
[34,45] 3.000
[35,5]    Container Weight Verified By:
[35,29] Date: 
[35,41] Method: Not Verified
[36,5]    Packs:
[36,9] 10
[36,15] PLT
[36,17] Marks: 
[36,35] Goods Description: 
[37,9] 890.000
[37,15] KG
[38,9] 3.000
[38,15] M3
[42,17] Height: 0.000M    Length: 0.000M    Width: 0.000M
[43,5] ITN:
[43,17] A12345
[45,5] POF/Exemption Statement:
[47,5] Additional References
[48,5] BOL Number
[48,29] Freight Forwarder Ref
[48,41] C00001000
[49,5] Shipper Ref
[50,5] Letter of Credit No
[50,29] Carrier Contract No
[51,5] HBL Number
[51,29] Contract Named Account
[55,43] Created By
[57,5] Additional Parties
[58,5] Notify Party 2
[58,17] Notify Party 3
[58,29] Forwarder
[58,41] Buyer
[59,29] SENDING AGENT COMPANY
[60,29] SENDING AGENT ADDRESS 1
[63,29] AUSTRALIA
[66,5] Freight Payer
[67,5] SENDING AGENT COMPANY
[68,5] SENDING AGENT ADDRESS 1
[71,5] AUSTRALIA
[75,5] Additional Instructions/Clauses
[76,5] Charge Payment Instructions
[76,39] Freight Payable At
[77,7] Freight Charges
[77,39] USSEA - Seattle, WA, United States
[78,15] ☑ Prepaid
[78,23] ☐ Collect
[78,31] ☐ Payable Elsewhere
[78,39] Show Charges on Bill of Lading
[79,7] Optional Charge Payment Instruction by Category
[79,39] ☐ Issue Freighted Bill of Lading
[80,11] Origin Port Charge
[80,23] ☐ Prepaid
[80,31] ☐ Collect
[80,39] Charge Clauses
[81,11] Origin Haulage
[81,23] ☐ Prepaid
[81,31] ☐ Collect
[81,41] ☑ Freight Prepaid
[82,11] Destination Port Charge
[82,23] ☐ Prepaid
[82,31] ☐ Collect
[82,41] ☐ Freight Collect
[83,11] Destination Haulage
[83,23] ☐ Prepaid
[83,31] ☐ Collect
[83,41] ☐ Freight As Agreed{bolDocumentationContent}
[86,5] Bill of Lading Clauses
[87,7] ☐ Received for Shipment
[87,21] ☐ Laden on Board Vessel
[87,37] ☐ Shipper's Load and Count
[88,7] ☐ Laden on Board
[88,21] ☐ On Board Vessel
[88,37] ☐ Shipper's Load, Stowage and Count
[89,7] ☐ On Board Rail
[89,21] ☐ Laden on Board Named Vessel
[89,37] ☐ No Shipper's Export Declaration Required
[90,5] Other Clauses
[93,5] Forwarding Instruction Notes
[96,5] Goods Handling Instruction
[101,5] Special Instruction
[106,43] Created By";
		}
	}
}
