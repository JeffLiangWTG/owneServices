using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Documents.Templates.Testing
{
	public class BillOfLadingTemplateTest : DocumentVisualizer.Testing.BillOfLadingContentTest
	{
		public override void TestDocumentContent()
		{
			var menuItemPK = new ZGuid("d50d9b8c-5f5f-4c3c-8f0e-2429c59ce43c");
			var templatePK = new ZGuid("2128d917-fe77-4a2d-b127-631faba0fa68");

			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_Code = "UNDG1";

			var billOfLading = GetNewBillOfLading();
			var header = billOfLading.Job;

			for (var i = 0; i < 50; ++i)
			{
				var packingLine = CreatePackingLine(billOfLading);

				packingLine.UNDGs.AddNew().DI_DG = undgSubstance1.PK;
			}

			var loosePackingLine = CreatePackingLine(billOfLading);
			loosePackingLine.JL_JC = ZGuid.Empty;

			CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500m, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750m, "DLAB", "USD");

			for (var i = 0; i < 50; ++i)
			{
				CreateLineCharge(header, header.AgentCollectPK, 750m, "FRT", "USD");
			}

			var billOfLadingImageCollection = new BillOfLadingImageCollection();
			var billOfLadingImage = billOfLadingImageCollection.AddNew();
			billOfLadingImage.PrincipalPK = billOfLading.Principal.PK;
			billOfLadingImage.Description = (NoResString)"XYZ";
			billOfLadingImage.Image = new Bitmap(10, 10);
			billOfLadingImage.Enabled = true;

			using (AgencyRegistry.Instance.BillOfLadingLogosImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1"))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(billOfLading, menuItemPK, "Original", 0, FCLContent, "Agency Bill Of Lading CargoWise", templatePK);
			}

			foreach (BillOfLadingPackLine outerPackLine in billOfLading.OuterPackLines)
			{
				outerPackLine.JL_JC = ZGuid.Empty;
			}

			using (AgencyRegistry.Instance.BillOfLadingLogosImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1"))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertContents(billOfLading, menuItemPK, "Original", 0, FCLContentWithoutPacklineDetails, "Agency Bill Of Lading CargoWise", templatePK);
			}

			billOfLading.JS_PackingMode = ContainerModes.RollOnRollOff;
			billOfLading.ShippingContainers.RemoveAndDeleteAll();

			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_GrossWeight = 10000;
			container.JC_GrossWeightUQ = Weight.Grams;
			container.JC_GrossVolume = 10000;
			container.JC_GrossVolumeUQ = Volume.MegaLitre;
			container.JC_TotalHeight = 1;
			container.JC_Description = "Container Description";
			container.JC_HarmonisedCode = "0012";

			using (AgencyRegistry.Instance.BillOfLadingLogosImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1"))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(billOfLading, menuItemPK, "Original", 0, RollOnRollOffContent, "Agency Bill Of Lading CargoWise", templatePK);
			}

			billOfLading.JS_PackingMode = ContainerModes.BreakBulk;
			billOfLading.ShippingContainers.RemoveAndDeleteAll();

			container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_GrossWeight = 10000;
			container.JC_GrossWeightUQ = Weight.Grams;
			container.JC_GrossVolume = 10000;
			container.JC_GrossVolumeUQ = Volume.MegaLitre;
			container.JC_TotalHeight = 1;
			container.JC_Description = "Container Description";
			container.JC_HarmonisedCode = "0012";

			container.UNDGs.AddNew().DI_DG = undgSubstance1.PK;

			using (AgencyRegistry.Instance.BillOfLadingLogosImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1"))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(billOfLading, menuItemPK, "Original", 0, BreakBulkContent, "Agency Bill Of Lading CargoWise", templatePK);
			}

			for (var i = 0; i < 100; ++i)
			{
				container.UNDGs.AddNew().DI_DG = undgSubstance1.PK;
			}

			using (AgencyRegistry.Instance.BillOfLadingLogosImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1"))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertContents(billOfLading, menuItemPK, "Original", 0, BreakBulkContentWithLotsOfUNDGs, "Agency Bill Of Lading CargoWise", templatePK);
			}
		}

		const string FCLContent =
@"[2,4] Shipper
[2,20] OCEAN SEA WAYBILL
[2,39] Ocean Bill of Lading
[5,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[5,39] HOUSEBILL001
[9,4] Consignee
[10,4] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[12,4] Notify Party
[14,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[16,4] Vessel
[16,11] Voyage
[16,20] Port of Loading
[16,32] Excess Value Declaration
[18,4] MAIN.Vessel
[18,11] MAINVoyage
[18,20] AUSYD - SYDNEY
[18,32] Refer to clause on reverse side
[21,4] Port of Discharge
[21,11] Destination (if on-carriage)
[21,20] Freight Payable At
[21,32] No. of Original B/L
[22,4] NZAKL - AUCKLAND
[22,11] AUCKLAND - NEW ZEALAND
[22,20] SYDNEY - AUSTRALIA
[22,32] 2 (TWO)
[23,4] Marks and Numbers
[23,11] Number and Kind of Packages / Description of Goods
[23,32] Gross Weight
Kgs.


[23,41] Mesaurement
M3


[25,4] marks & numbers
[25,11] goods description
[25,32] 5765.000 KG
[25,41] 920.600 M3
[27,4] Ctnr. No
[27,6] Seal
[27,14] Type
[27,17] Net (kg)
[27,20] Tare (kg)
[27,26] Gross (Kg)
[27,30] Volume (m3)
[27,36] Packs
[28,4] AAAA0000007
[28,6] -
[28,17] 2750.000
[28,20] 0.000
[28,26] 2750.000
[28,30] 901.300
[28,36] 151 PKG
[29,4] goods description GEN
[29,17] 2000.000
[29,30] 1.300
[29,36] 1 BOT
[30,4] DDD GEN
[30,17] 15.000
[30,21] UNDG1
[30,30] 18.000
[30,36] 3 PTL
[31,4] DDD GEN
[31,17] 15.000
[31,21] UNDG1
[31,30] 18.000
[31,36] 3 PTL
[32,4] DDD GEN
[32,17] 15.000
[32,21] UNDG1
[32,30] 18.000
[32,36] 3 PTL
[33,4] DDD GEN
[33,17] 15.000
[33,21] UNDG1
[33,30] 18.000
[33,36] 3 PTL
[34,4] DDD GEN
[34,17] 15.000
[34,21] UNDG1
[34,30] 18.000
[34,36] 3 PTL
[35,4] DDD GEN
[35,17] 15.000
[35,21] UNDG1
[35,30] 18.000
[35,36] 3 PTL
[36,4] DDD GEN
[36,17] 15.000
[36,21] UNDG1
[36,30] 18.000
[36,36] 3 PTL
[37,4] DDD GEN
[37,17] 15.000
[37,21] UNDG1
[37,30] 18.000
[37,36] 3 PTL
[38,4] DDD GEN
[38,17] 15.000
[38,21] UNDG1
[38,30] 18.000
[38,36] 3 PTL
[39,4] DDD GEN
[39,17] 15.000
[39,21] UNDG1
[39,30] 18.000
[39,36] 3 PTL
[40,4] DDD GEN
[40,17] 15.000
[40,21] UNDG1
[40,30] 18.000
[40,36] 3 PTL
[41,4] DDD GEN
[41,17] 15.000
[41,21] UNDG1
[41,30] 18.000
[41,36] 3 PTL
[42,4] DDD GEN
[42,17] 15.000
[42,21] UNDG1
[42,30] 18.000
[42,36] 3 PTL
[43,4] DDD GEN
[43,17] 15.000
[43,21] UNDG1
[43,30] 18.000
[43,36] 3 PTL
[44,4] DDD GEN
[44,17] 15.000
[44,21] UNDG1
[44,30] 18.000
[44,36] 3 PTL
[45,4] DDD GEN
[45,17] 15.000
[45,21] UNDG1
[45,30] 18.000
[45,36] 3 PTL
[47,4] DDD GEN
[47,17] 15.000
[47,21] UNDG1
[47,30] 18.000
[47,36] 3 PTL
[49,4] DDD GEN
[49,17] 15.000
[49,21] UNDG1
[49,30] 18.000
[49,36] 3 PTL
[51,4] DDD GEN
[51,17] 15.000
[51,21] UNDG1
[51,30] 18.000
[51,36] 3 PTL
[53,4] DDD GEN
[53,17] 15.000
[53,21] UNDG1
[53,30] 18.000
[53,36] 3 PTL
[58,18] Blaticus1
[61,4] Shipped
[61,13] 01-Jan-24
[61,28] Payment Term:
[61,33] Prepaid
[63,24] Freight Details, Charges
[64,24] Charge Description
[64,34] Collect
[64,41] Prepaid
[66,24] International Freight
[66,41] 1000.00 AUD
[69,24] Origin Labour Charges
[69,34] 500.00 AUD
[71,24] Destination Labour Charges
[71,41] 750.00 USD
[73,4] Place and Date of Issue
[74,24] International Freight
[74,34] 750.00 USD
[75,14] 01-Jan-24
[76,24] International Freight
[76,34] 750.00 USD
[77,4] MELBOURNE - AUSTRALIA
[79,24] International Freight
[79,34] 750.00 USD
[81,24] International Freight
[81,34] 750.00 USD
[83,24] International Freight
[83,34] 750.00 USD
[86,24] International Freight
[86,34] 750.00 USD
[89,24] International Freight
[89,34] 750.00 USD
[91,24] International Freight
[91,34] 750.00 USD
[92,4] Place of Receipt
[92,14] Place of Delivery
[93,24] International Freight
[93,34] 750.00 USD
[94,24] International Freight
[94,34] 750.00 USD
[99,4] SYDNEY - AUSTRALIA
[99,14] AUCKLAND - NEW ZEALAND
[99,24] Total No. of Packages
[102,24] FIVE CONTAINER(S)
[103,21] Note/Signature:
[107,4] Page 1 *** Continues Next Page ***
[172,4] Page 2 *** Continues Next Page ***
[173,3] CONTINUATION PAGE
[173,19] Sea Waybill - HOUSEBILL001
[175,4] Consignor
[175,20] Consignee
[176,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[177,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[181,4] Notify Party
[181,20] Goods Collected From
[181,32] ETD
[181,36] 02-Jan-24
[182,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[182,20] SYDNEY, AUSTRALIA
[183,20] Goods Delivered To
[183,32] ETA
[183,36] 03-Jan-24
[184,20] AUCKLAND, NEW ZEALAND
[185,20] Weight
[185,32] Volume
[186,20] 5765.000 KG
[186,32] 920.600 M3
[187,4] Phone: 
[187,20] Package Quantity
[188,4] Fax: 
[188,20] 10 BOT (OUTER)
[191,4] marks & numbers
[191,11] goods description
[191,32] 5765.000 KG
[191,41] 920.600 M3
[194,4] Ctnr. No
[194,6] Seal
[194,14] Type
[194,17] Net (kg)
[194,20] Tare (kg)
[194,26] Gross (Kg)
[194,30] Volume (m3)
[194,36] Packs
[196,4] DDD GEN
[196,17] 15.000
[196,21] UNDG1
[196,30] 18.000
[196,36] 3 PTL
[197,4] DDD GEN
[197,17] 15.000
[197,21] UNDG1
[197,30] 18.000
[197,36] 3 PTL
[199,4] DDD GEN
[199,17] 15.000
[199,21] UNDG1
[199,30] 18.000
[199,36] 3 PTL
[201,4] DDD GEN
[201,17] 15.000
[201,21] UNDG1
[201,30] 18.000
[201,36] 3 PTL
[203,4] DDD GEN
[203,17] 15.000
[203,21] UNDG1
[203,30] 18.000
[203,36] 3 PTL
[205,4] DDD GEN
[205,17] 15.000
[205,21] UNDG1
[205,30] 18.000
[205,36] 3 PTL
[207,4] DDD GEN
[207,17] 15.000
[207,21] UNDG1
[207,30] 18.000
[207,36] 3 PTL
[209,4] DDD GEN
[209,17] 15.000
[209,21] UNDG1
[209,30] 18.000
[209,36] 3 PTL
[210,4] DDD GEN
[210,17] 15.000
[210,21] UNDG1
[210,30] 18.000
[210,36] 3 PTL
[212,4] DDD GEN
[212,17] 15.000
[212,21] UNDG1
[212,30] 18.000
[212,36] 3 PTL
[214,4] DDD GEN
[214,17] 15.000
[214,21] UNDG1
[214,30] 18.000
[214,36] 3 PTL
[216,4] DDD GEN
[216,17] 15.000
[216,21] UNDG1
[216,30] 18.000
[216,36] 3 PTL
[218,4] DDD GEN
[218,17] 15.000
[218,21] UNDG1
[218,30] 18.000
[218,36] 3 PTL
[220,4] DDD GEN
[220,17] 15.000
[220,21] UNDG1
[220,30] 18.000
[220,36] 3 PTL
[222,4] DDD GEN
[222,17] 15.000
[222,21] UNDG1
[222,30] 18.000
[222,36] 3 PTL
[223,4] DDD GEN
[223,17] 15.000
[223,21] UNDG1
[223,30] 18.000
[223,36] 3 PTL
[225,4] DDD GEN
[225,17] 15.000
[225,21] UNDG1
[225,30] 18.000
[225,36] 3 PTL
[227,4] DDD GEN
[227,17] 15.000
[227,21] UNDG1
[227,30] 18.000
[227,36] 3 PTL
[229,4] DDD GEN
[229,17] 15.000
[229,21] UNDG1
[229,30] 18.000
[229,36] 3 PTL
[231,4] DDD GEN
[231,17] 15.000
[231,21] UNDG1
[231,30] 18.000
[231,36] 3 PTL
[233,4] DDD GEN
[233,17] 15.000
[233,21] UNDG1
[233,30] 18.000
[233,36] 3 PTL
[235,4] DDD GEN
[235,17] 15.000
[235,21] UNDG1
[235,30] 18.000
[235,36] 3 PTL
[236,4] DDD GEN
[236,17] 15.000
[236,21] UNDG1
[236,30] 18.000
[236,36] 3 PTL
[238,4] DDD GEN
[238,17] 15.000
[238,21] UNDG1
[238,30] 18.000
[238,36] 3 PTL
[240,4] DDD GEN
[240,17] 15.000
[240,21] UNDG1
[240,30] 18.000
[240,36] 3 PTL
[242,4] DDD GEN
[242,17] 15.000
[242,21] UNDG1
[242,30] 18.000
[242,36] 3 PTL
[244,4] DDD GEN
[244,17] 15.000
[244,21] UNDG1
[244,30] 18.000
[244,36] 3 PTL
[246,4] DDD GEN
[246,17] 15.000
[246,21] UNDG1
[246,30] 18.000
[246,36] 3 PTL
[248,4] DDD GEN
[248,17] 15.000
[248,21] UNDG1
[248,30] 18.000
[248,36] 3 PTL
[250,4] DDD GEN
[250,17] 15.000
[250,21] UNDG1
[250,30] 18.000
[250,36] 3 PTL
[251,4] BBBB0000007
[251,6] -
[251,17] 3000.000
[251,20] 0.000
[251,26] 3000.000
[251,30] 1.300
[251,36] 1 BOT
[253,4] goods description GEN
[253,17] 3000.000
[253,30] 1.300
[253,36] 1 BOT
[255,4] -
[255,6] -
[255,14] -
[255,17] 15.000
[255,20] -
[255,26] 15.000
[255,30] 18.000
[255,36] 3 PTL
[257,4] DDD GEN
[257,17] 15.000
[257,30] 18.000
[257,36] 3 PTL
[259,4] Charge Description
[259,11] Collect
[259,17] Prepaid
[261,4] International Freight
[261,11] 750.00 USD
[263,4] International Freight
[263,11] 750.00 USD
[264,4] International Freight
[264,11] 750.00 USD
[266,4] International Freight
[266,11] 750.00 USD
[268,4] International Freight
[268,11] 750.00 USD
[270,4] International Freight
[270,11] 750.00 USD
[272,4] International Freight
[272,11] 750.00 USD
[274,4] International Freight
[274,11] 750.00 USD
[276,4] International Freight
[276,11] 750.00 USD
[277,4] International Freight
[277,11] 750.00 USD
[279,4] International Freight
[279,11] 750.00 USD
[281,4] International Freight
[281,11] 750.00 USD
[283,4] International Freight
[283,11] 750.00 USD
[285,4] International Freight
[285,11] 750.00 USD
[287,4] International Freight
[287,11] 750.00 USD
[289,4] International Freight
[289,11] 750.00 USD
[290,4] International Freight
[290,11] 750.00 USD
[292,4] International Freight
[292,11] 750.00 USD
[294,4] International Freight
[294,11] 750.00 USD
[296,4] International Freight
[296,11] 750.00 USD
[299,4] Page 3 *** Continues Next Page ***
[300,3] CONTINUATION PAGE
[300,19] Sea Waybill - HOUSEBILL001
[302,4] Consignor
[302,20] Consignee
[303,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[304,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[308,4] Notify Party
[308,20] Goods Collected From
[308,32] ETD
[308,36] 02-Jan-24
[309,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[309,20] SYDNEY, AUSTRALIA
[310,20] Goods Delivered To
[310,32] ETA
[310,36] 03-Jan-24
[311,20] AUCKLAND, NEW ZEALAND
[312,20] Weight
[312,32] Volume
[313,20] 5765.000 KG
[313,32] 920.600 M3
[314,4] Phone: 
[314,20] Package Quantity
[315,4] Fax: 
[315,20] 10 BOT (OUTER)
[318,4] Charge Description
[318,11] Collect
[318,17] Prepaid
[319,4] International Freight
[319,11] 750.00 USD
[321,4] International Freight
[321,11] 750.00 USD
[323,4] International Freight
[323,11] 750.00 USD
[325,4] International Freight
[325,11] 750.00 USD
[327,4] International Freight
[327,11] 750.00 USD
[329,4] International Freight
[329,11] 750.00 USD
[331,4] International Freight
[331,11] 750.00 USD
[332,4] International Freight
[332,11] 750.00 USD
[334,4] International Freight
[334,11] 750.00 USD
[336,4] International Freight
[336,11] 750.00 USD
[338,4] International Freight
[338,11] 750.00 USD
[340,4] International Freight
[340,11] 750.00 USD
[342,4] International Freight
[342,11] 750.00 USD
[344,4] International Freight
[344,11] 750.00 USD
[345,4] International Freight
[345,11] 750.00 USD
[347,4] International Freight
[347,11] 750.00 USD
[349,4] International Freight
[349,11] 750.00 USD
[351,4] International Freight
[351,11] 750.00 USD
[353,4] International Freight
[353,11] 750.00 USD
[355,4] International Freight
[355,11] 750.00 USD
[389,4] Page 4";

		const string FCLContentWithoutPacklineDetails =
@"[2,4] Shipper
[2,20] OCEAN SEA WAYBILL
[2,39] Ocean Bill of Lading
[5,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[5,39] HOUSEBILL001
[9,4] Consignee
[10,4] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[12,4] Notify Party
[14,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[16,4] Vessel
[16,11] Voyage
[16,20] Port of Loading
[16,32] Excess Value Declaration
[18,4] MAIN.Vessel
[18,11] MAINVoyage
[18,20] AUSYD - SYDNEY
[18,32] Refer to clause on reverse side
[21,4] Port of Discharge
[21,11] Destination (if on-carriage)
[21,20] Freight Payable At
[21,32] No. of Original B/L
[22,4] NZAKL - AUCKLAND
[22,11] AUCKLAND - NEW ZEALAND
[22,20] SYDNEY - AUSTRALIA
[22,32] 2 (TWO)
[23,4] Marks and Numbers
[23,11] Number and Kind of Packages / Description of Goods
[23,32] Gross Weight
Kgs.


[23,41] Mesaurement
M3


[25,4] marks & numbers
[25,11] goods description
[25,32] 5765.000 KG
[25,41] 920.600 M3
[27,4] Ctnr. No
[27,6] Seal
[27,14] Type
[27,17] Net (kg)
[27,20] Tare (kg)
[27,26] Gross (Kg)
[27,30] Volume (m3)
[27,36] Packs
[28,4] AAAA0000007
[28,6] -
[28,17] 0.000
[28,20] 0.000
[28,26] 0.000
[28,30] 0.000
[29,4] BBBB0000007
[29,6] -
[29,17] 0.000
[29,20] 0.000
[29,26] 0.000
[29,30] 0.000
[30,4] -
[30,6] -
[30,14] -
[30,17] 2000.000
[30,20] -
[30,26] 2000.000
[30,30] 1.300
[30,36] 1 BOT
[31,4] -
[31,6] -
[31,14] -
[31,17] 3000.000
[31,20] -
[31,26] 3000.000
[31,30] 1.300
[31,36] 1 BOT
[32,4] -
[32,6] -
[32,14] -
[32,17] 15.000
[32,20] -
[32,26] 15.000
[32,30] 18.000
[32,36] 3 PTL
[33,4] -
[33,6] -
[33,14] -
[33,17] 15.000
[33,20] -
[33,26] 15.000
[33,30] 18.000
[33,36] 3 PTL
[34,4] -
[34,6] -
[34,14] -
[34,17] 15.000
[34,20] -
[34,26] 15.000
[34,30] 18.000
[34,36] 3 PTL
[35,4] -
[35,6] -
[35,14] -
[35,17] 15.000
[35,20] -
[35,26] 15.000
[35,30] 18.000
[35,36] 3 PTL
[36,4] -
[36,6] -
[36,14] -
[36,17] 15.000
[36,20] -
[36,26] 15.000
[36,30] 18.000
[36,36] 3 PTL
[37,4] -
[37,6] -
[37,14] -
[37,17] 15.000
[37,20] -
[37,26] 15.000
[37,30] 18.000
[37,36] 3 PTL
[38,4] -
[38,6] -
[38,14] -
[38,17] 15.000
[38,20] -
[38,26] 15.000
[38,30] 18.000
[38,36] 3 PTL
[39,4] -
[39,6] -
[39,14] -
[39,17] 15.000
[39,20] -
[39,26] 15.000
[39,30] 18.000
[39,36] 3 PTL
[40,4] -
[40,6] -
[40,14] -
[40,17] 15.000
[40,20] -
[40,26] 15.000
[40,30] 18.000
[40,36] 3 PTL
[41,4] -
[41,6] -
[41,14] -
[41,17] 15.000
[41,20] -
[41,26] 15.000
[41,30] 18.000
[41,36] 3 PTL
[42,4] -
[42,6] -
[42,14] -
[42,17] 15.000
[42,20] -
[42,26] 15.000
[42,30] 18.000
[42,36] 3 PTL
[43,4] -
[43,6] -
[43,14] -
[43,17] 15.000
[43,20] -
[43,26] 15.000
[43,30] 18.000
[43,36] 3 PTL
[44,4] -
[44,6] -
[44,14] -
[44,17] 15.000
[44,20] -
[44,26] 15.000
[44,30] 18.000
[44,36] 3 PTL
[45,4] -
[45,6] -
[45,14] -
[45,17] 15.000
[45,20] -
[45,26] 15.000
[45,30] 18.000
[45,36] 3 PTL
[47,4] -
[47,6] -
[47,14] -
[47,17] 15.000
[47,20] -
[47,26] 15.000
[47,30] 18.000
[47,36] 3 PTL
[49,4] -
[49,6] -
[49,14] -
[49,17] 15.000
[49,20] -
[49,26] 15.000
[49,30] 18.000
[49,36] 3 PTL
[51,4] -
[51,6] -
[51,14] -
[51,17] 15.000
[51,20] -
[51,26] 15.000
[51,30] 18.000
[51,36] 3 PTL
[53,4] -
[53,6] -
[53,14] -
[53,17] 15.000
[53,20] -
[53,26] 15.000
[53,30] 18.000
[53,36] 3 PTL
[58,18] Blaticus1
[61,4] Shipped
[61,13] 01-Jan-24
[61,28] Payment Term:
[61,33] Prepaid
[63,24] Freight Details, Charges
[64,24] Charge Description
[64,34] Collect
[64,41] Prepaid
[66,24] International Freight
[66,41] 1000.00 AUD
[69,24] Origin Labour Charges
[69,34] 500.00 AUD
[71,24] Destination Labour Charges
[71,41] 750.00 USD
[73,4] Place and Date of Issue
[74,24] International Freight
[74,34] 750.00 USD
[75,14] 01-Jan-24
[76,24] International Freight
[76,34] 750.00 USD
[77,4] MELBOURNE - AUSTRALIA
[79,24] International Freight
[79,34] 750.00 USD
[81,24] International Freight
[81,34] 750.00 USD
[83,24] International Freight
[83,34] 750.00 USD
[86,24] International Freight
[86,34] 750.00 USD
[89,24] International Freight
[89,34] 750.00 USD
[91,24] International Freight
[91,34] 750.00 USD
[92,4] Place of Receipt
[92,14] Place of Delivery
[93,24] International Freight
[93,34] 750.00 USD
[94,24] International Freight
[94,34] 750.00 USD
[99,4] SYDNEY - AUSTRALIA
[99,14] AUCKLAND - NEW ZEALAND
[99,24] Total No. of Packages
[102,24] ONE HUNDRED AND FIFTY SEVEN CONTAINER(S)
[103,21] Note/Signature:
[107,4] Page 1 *** Continues Next Page ***
[172,4] Page 2 *** Continues Next Page ***
[173,3] CONTINUATION PAGE
[173,19] Sea Waybill - HOUSEBILL001
[175,4] Consignor
[175,20] Consignee
[176,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[177,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[181,4] Notify Party
[181,20] Goods Collected From
[181,32] ETD
[181,36] 02-Jan-24
[182,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[182,20] SYDNEY, AUSTRALIA
[183,20] Goods Delivered To
[183,32] ETA
[183,36] 03-Jan-24
[184,20] AUCKLAND, NEW ZEALAND
[185,20] Weight
[185,32] Volume
[186,20] 5765.000 KG
[186,32] 920.600 M3
[187,4] Phone: 
[187,20] Package Quantity
[188,4] Fax: 
[188,20] 10 BOT (OUTER)
[191,4] marks & numbers
[191,11] goods description
[191,32] 5765.000 KG
[191,41] 920.600 M3
[194,4] Ctnr. No
[194,6] Seal
[194,14] Type
[194,17] Net (kg)
[194,20] Tare (kg)
[194,26] Gross (Kg)
[194,30] Volume (m3)
[194,36] Packs
[196,4] -
[196,6] -
[196,14] -
[196,17] 15.000
[196,20] -
[196,26] 15.000
[196,30] 18.000
[196,36] 3 PTL
[197,4] -
[197,6] -
[197,14] -
[197,17] 15.000
[197,20] -
[197,26] 15.000
[197,30] 18.000
[197,36] 3 PTL
[199,4] -
[199,6] -
[199,14] -
[199,17] 15.000
[199,20] -
[199,26] 15.000
[199,30] 18.000
[199,36] 3 PTL
[201,4] -
[201,6] -
[201,14] -
[201,17] 15.000
[201,20] -
[201,26] 15.000
[201,30] 18.000
[201,36] 3 PTL
[203,4] -
[203,6] -
[203,14] -
[203,17] 15.000
[203,20] -
[203,26] 15.000
[203,30] 18.000
[203,36] 3 PTL
[205,4] -
[205,6] -
[205,14] -
[205,17] 15.000
[205,20] -
[205,26] 15.000
[205,30] 18.000
[205,36] 3 PTL
[207,4] -
[207,6] -
[207,14] -
[207,17] 15.000
[207,20] -
[207,26] 15.000
[207,30] 18.000
[207,36] 3 PTL
[209,4] -
[209,6] -
[209,14] -
[209,17] 15.000
[209,20] -
[209,26] 15.000
[209,30] 18.000
[209,36] 3 PTL
[210,4] -
[210,6] -
[210,14] -
[210,17] 15.000
[210,20] -
[210,26] 15.000
[210,30] 18.000
[210,36] 3 PTL
[212,4] -
[212,6] -
[212,14] -
[212,17] 15.000
[212,20] -
[212,26] 15.000
[212,30] 18.000
[212,36] 3 PTL
[214,4] -
[214,6] -
[214,14] -
[214,17] 15.000
[214,20] -
[214,26] 15.000
[214,30] 18.000
[214,36] 3 PTL
[216,4] -
[216,6] -
[216,14] -
[216,17] 15.000
[216,20] -
[216,26] 15.000
[216,30] 18.000
[216,36] 3 PTL
[218,4] -
[218,6] -
[218,14] -
[218,17] 15.000
[218,20] -
[218,26] 15.000
[218,30] 18.000
[218,36] 3 PTL
[220,4] -
[220,6] -
[220,14] -
[220,17] 15.000
[220,20] -
[220,26] 15.000
[220,30] 18.000
[220,36] 3 PTL
[222,4] -
[222,6] -
[222,14] -
[222,17] 15.000
[222,20] -
[222,26] 15.000
[222,30] 18.000
[222,36] 3 PTL
[223,4] -
[223,6] -
[223,14] -
[223,17] 15.000
[223,20] -
[223,26] 15.000
[223,30] 18.000
[223,36] 3 PTL
[225,4] -
[225,6] -
[225,14] -
[225,17] 15.000
[225,20] -
[225,26] 15.000
[225,30] 18.000
[225,36] 3 PTL
[227,4] -
[227,6] -
[227,14] -
[227,17] 15.000
[227,20] -
[227,26] 15.000
[227,30] 18.000
[227,36] 3 PTL
[229,4] -
[229,6] -
[229,14] -
[229,17] 15.000
[229,20] -
[229,26] 15.000
[229,30] 18.000
[229,36] 3 PTL
[231,4] -
[231,6] -
[231,14] -
[231,17] 15.000
[231,20] -
[231,26] 15.000
[231,30] 18.000
[231,36] 3 PTL
[233,4] -
[233,6] -
[233,14] -
[233,17] 15.000
[233,20] -
[233,26] 15.000
[233,30] 18.000
[233,36] 3 PTL
[235,4] -
[235,6] -
[235,14] -
[235,17] 15.000
[235,20] -
[235,26] 15.000
[235,30] 18.000
[235,36] 3 PTL
[236,4] -
[236,6] -
[236,14] -
[236,17] 15.000
[236,20] -
[236,26] 15.000
[236,30] 18.000
[236,36] 3 PTL
[238,4] -
[238,6] -
[238,14] -
[238,17] 15.000
[238,20] -
[238,26] 15.000
[238,30] 18.000
[238,36] 3 PTL
[240,4] -
[240,6] -
[240,14] -
[240,17] 15.000
[240,20] -
[240,26] 15.000
[240,30] 18.000
[240,36] 3 PTL
[242,4] -
[242,6] -
[242,14] -
[242,17] 15.000
[242,20] -
[242,26] 15.000
[242,30] 18.000
[242,36] 3 PTL
[244,4] -
[244,6] -
[244,14] -
[244,17] 15.000
[244,20] -
[244,26] 15.000
[244,30] 18.000
[244,36] 3 PTL
[246,4] -
[246,6] -
[246,14] -
[246,17] 15.000
[246,20] -
[246,26] 15.000
[246,30] 18.000
[246,36] 3 PTL
[248,4] -
[248,6] -
[248,14] -
[248,17] 15.000
[248,20] -
[248,26] 15.000
[248,30] 18.000
[248,36] 3 PTL
[250,4] -
[250,6] -
[250,14] -
[250,17] 15.000
[250,20] -
[250,26] 15.000
[250,30] 18.000
[250,36] 3 PTL
[251,4] -
[251,6] -
[251,14] -
[251,17] 15.000
[251,20] -
[251,26] 15.000
[251,30] 18.000
[251,36] 3 PTL
[253,4] -
[253,6] -
[253,14] -
[253,17] 15.000
[253,20] -
[253,26] 15.000
[253,30] 18.000
[253,36] 3 PTL
[255,4] -
[255,6] -
[255,14] -
[255,17] 15.000
[255,20] -
[255,26] 15.000
[255,30] 18.000
[255,36] 3 PTL
[257,21] UNDG1
[259,21] UNDG1
[261,21] UNDG1
[263,21] UNDG1
[264,21] UNDG1
[266,21] UNDG1
[268,21] UNDG1
[270,21] UNDG1
[272,21] UNDG1
[274,21] UNDG1
[276,21] UNDG1
[277,21] UNDG1
[279,21] UNDG1
[281,21] UNDG1
[283,21] UNDG1
[285,21] UNDG1
[287,21] UNDG1
[289,21] UNDG1
[290,21] UNDG1
[292,21] UNDG1
[294,21] UNDG1
[296,21] UNDG1
[299,4] Page 3 *** Continues Next Page ***
[300,3] CONTINUATION PAGE
[300,19] Sea Waybill - HOUSEBILL001
[302,4] Consignor
[302,20] Consignee
[303,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[304,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[308,4] Notify Party
[308,20] Goods Collected From
[308,32] ETD
[308,36] 02-Jan-24
[309,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[309,20] SYDNEY, AUSTRALIA
[310,20] Goods Delivered To
[310,32] ETA
[310,36] 03-Jan-24
[311,20] AUCKLAND, NEW ZEALAND
[312,20] Weight
[312,32] Volume
[313,20] 5765.000 KG
[313,32] 920.600 M3
[314,4] Phone: 
[314,20] Package Quantity
[315,4] Fax: 
[315,20] 10 BOT (OUTER)
[318,21] UNDG1
[319,21] UNDG1
[321,21] UNDG1
[323,21] UNDG1
[325,21] UNDG1
[327,21] UNDG1
[329,21] UNDG1
[331,21] UNDG1
[332,21] UNDG1
[334,21] UNDG1
[336,21] UNDG1
[338,21] UNDG1
[340,21] UNDG1
[342,21] UNDG1
[344,21] UNDG1
[345,21] UNDG1
[347,21] UNDG1
[349,21] UNDG1
[351,21] UNDG1
[353,21] UNDG1
[355,21] UNDG1
[357,21] UNDG1
[358,21] UNDG1
[360,21] UNDG1
[362,21] UNDG1
[364,21] UNDG1
[366,21] UNDG1
[368,21] UNDG1
[370,4] Charge Description
[370,11] Collect
[370,17] Prepaid
[371,4] International Freight
[371,11] 750.00 USD
[373,4] International Freight
[373,11] 750.00 USD
[375,4] International Freight
[375,11] 750.00 USD
[377,4] International Freight
[377,11] 750.00 USD
[379,4] International Freight
[379,11] 750.00 USD
[381,4] International Freight
[381,11] 750.00 USD
[383,4] International Freight
[383,11] 750.00 USD
[385,4] International Freight
[385,11] 750.00 USD
[386,4] International Freight
[386,11] 750.00 USD
[388,4] International Freight
[388,11] 750.00 USD
[390,4] International Freight
[390,11] 750.00 USD
[392,4] International Freight
[392,11] 750.00 USD
[394,4] International Freight
[394,11] 750.00 USD
[396,4] International Freight
[396,11] 750.00 USD
[398,4] International Freight
[398,11] 750.00 USD
[399,4] International Freight
[399,11] 750.00 USD
[401,4] International Freight
[401,11] 750.00 USD
[403,4] International Freight
[403,11] 750.00 USD
[405,4] International Freight
[405,11] 750.00 USD
[407,4] International Freight
[407,11] 750.00 USD
[409,4] International Freight
[409,11] 750.00 USD
[411,4] International Freight
[411,11] 750.00 USD
[412,4] International Freight
[412,11] 750.00 USD
[414,4] International Freight
[414,11] 750.00 USD
[416,4] International Freight
[416,11] 750.00 USD
[418,4] International Freight
[418,11] 750.00 USD
[420,4] International Freight
[420,11] 750.00 USD
[422,4] International Freight
[422,11] 750.00 USD
[424,4] International Freight
[424,11] 750.00 USD
[426,4] Page 4 *** Continues Next Page ***
[427,3] CONTINUATION PAGE
[427,19] Sea Waybill - HOUSEBILL001
[429,4] Consignor
[429,20] Consignee
[430,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[431,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[435,4] Notify Party
[435,20] Goods Collected From
[435,32] ETD
[435,36] 02-Jan-24
[436,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[436,20] SYDNEY, AUSTRALIA
[437,20] Goods Delivered To
[437,32] ETA
[437,36] 03-Jan-24
[438,20] AUCKLAND, NEW ZEALAND
[439,20] Weight
[439,32] Volume
[440,20] 5765.000 KG
[440,32] 920.600 M3
[441,4] Phone: 
[441,20] Package Quantity
[442,4] Fax: 
[442,20] 10 BOT (OUTER)
[445,4] Charge Description
[445,11] Collect
[445,17] Prepaid
[447,4] International Freight
[447,11] 750.00 USD
[449,4] International Freight
[449,11] 750.00 USD
[451,4] International Freight
[451,11] 750.00 USD
[453,4] International Freight
[453,11] 750.00 USD
[455,4] International Freight
[455,11] 750.00 USD
[457,4] International Freight
[457,11] 750.00 USD
[459,4] International Freight
[459,11] 750.00 USD
[460,4] International Freight
[460,11] 750.00 USD
[462,4] International Freight
[462,11] 750.00 USD
[464,4] International Freight
[464,11] 750.00 USD
[466,4] International Freight
[466,11] 750.00 USD
[508,4] Page 5";

		const string RollOnRollOffContent =
@"[2,4] Shipper
[2,20] OCEAN SEA WAYBILL
[2,39] Ocean Bill of Lading
[5,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[5,39] HOUSEBILL001
[9,4] Consignee
[10,4] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[12,4] Notify Party
[14,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[16,4] Vessel
[16,11] Voyage
[16,20] Port of Loading
[16,32] Excess Value Declaration
[18,4] MAIN.Vessel
[18,11] MAINVoyage
[18,20] AUSYD - SYDNEY
[18,32] Refer to clause on reverse side
[21,4] Port of Discharge
[21,11] Destination (if on-carriage)
[21,20] Freight Payable At
[21,32] No. of Original B/L
[22,4] NZAKL - AUCKLAND
[22,11] AUCKLAND - NEW ZEALAND
[22,20] SYDNEY - AUSTRALIA
[22,32] 3 (THREE)
[23,4] Marks and Numbers
[23,11] Number and Kind of Packages / Description of Goods
[23,32] Gross Weight
Kgs.


[23,41] Mesaurement
M3


[25,4] marks & numbers
[25,11] goods description
[25,32] 10.000 KG
[25,41] 10000000.000 M3
[27,4] VIN/Serial
[27,8] Count
[27,34] Weight (Kg)
[27,41] Volume (m3)
[28,4] AAAA0000007
[28,8] 1
[28,34] 10.000
[28,41] 10000000.000
[29,13] Height: 1.000M  Length: 0.000M  Width: 0.000M
[30,13] Container Description
[32,13] HC: 0012
[42,18] Blaticus1
[45,4] Shipped
[45,13] 01-Jan-24
[45,28] Payment Term:
[45,33] Prepaid
[47,24] Freight Details, Charges
[48,24] Charge Description
[48,34] Collect
[48,41] Prepaid
[50,24] International Freight
[50,41] 1000.00 AUD
[53,24] Origin Labour Charges
[53,34] 500.00 AUD
[55,24] Destination Labour Charges
[55,41] 750.00 USD
[57,4] Place and Date of Issue
[58,24] International Freight
[58,34] 750.00 USD
[59,14] 01-Jan-24
[60,24] International Freight
[60,34] 750.00 USD
[61,4] MELBOURNE - AUSTRALIA
[63,24] International Freight
[63,34] 750.00 USD
[65,24] International Freight
[65,34] 750.00 USD
[67,24] International Freight
[67,34] 750.00 USD
[70,24] International Freight
[70,34] 750.00 USD
[73,24] International Freight
[73,34] 750.00 USD
[75,24] International Freight
[75,34] 750.00 USD
[76,4] Place of Receipt
[76,14] Place of Delivery
[77,24] International Freight
[77,34] 750.00 USD
[78,24] International Freight
[78,34] 750.00 USD
[83,4] SYDNEY - AUSTRALIA
[83,14] AUCKLAND - NEW ZEALAND
[83,24] Total No. of Packages
[86,24] ONE Vehicle(s)
[87,21] Note/Signature:
[91,4] Page 1 *** Continues Next Page ***
[156,4] Page 2 *** Continues Next Page ***
[157,3] CONTINUATION PAGE
[157,19] Sea Waybill - HOUSEBILL001
[159,4] Consignor
[159,20] Consignee
[160,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[161,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[165,4] Notify Party
[165,20] Goods Collected From
[165,32] ETD
[165,36] 02-Jan-24
[166,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[166,20] SYDNEY, AUSTRALIA
[167,20] Goods Delivered To
[167,32] ETA
[167,36] 03-Jan-24
[168,20] AUCKLAND, NEW ZEALAND
[169,20] Weight
[169,32] Volume
[170,20] 10.000 KG
[170,32] 10000000.000 M3
[171,4] Phone: 
[171,20] Package Quantity
[172,4] Fax: 
[172,20] 10 BOT (OUTER)
[175,4] marks & numbers
[175,11] goods description
[175,32] 10.000 KG
[175,41] 10000000.000 M3
[178,4] Charge Description
[178,11] Collect
[178,17] Prepaid
[180,4] International Freight
[180,11] 750.00 USD
[182,4] International Freight
[182,11] 750.00 USD
[184,4] International Freight
[184,11] 750.00 USD
[186,4] International Freight
[186,11] 750.00 USD
[188,4] International Freight
[188,11] 750.00 USD
[190,4] International Freight
[190,11] 750.00 USD
[191,4] International Freight
[191,11] 750.00 USD
[193,4] International Freight
[193,11] 750.00 USD
[195,4] International Freight
[195,11] 750.00 USD
[197,4] International Freight
[197,11] 750.00 USD
[199,4] International Freight
[199,11] 750.00 USD
[201,4] International Freight
[201,11] 750.00 USD
[203,4] International Freight
[203,11] 750.00 USD
[205,4] International Freight
[205,11] 750.00 USD
[206,4] International Freight
[206,11] 750.00 USD
[208,4] International Freight
[208,11] 750.00 USD
[210,4] International Freight
[210,11] 750.00 USD
[212,4] International Freight
[212,11] 750.00 USD
[214,4] International Freight
[214,11] 750.00 USD
[216,4] International Freight
[216,11] 750.00 USD
[218,4] International Freight
[218,11] 750.00 USD
[219,4] International Freight
[219,11] 750.00 USD
[221,4] International Freight
[221,11] 750.00 USD
[223,4] International Freight
[223,11] 750.00 USD
[225,4] International Freight
[225,11] 750.00 USD
[227,4] International Freight
[227,11] 750.00 USD
[229,4] International Freight
[229,11] 750.00 USD
[231,4] International Freight
[231,11] 750.00 USD
[232,4] International Freight
[232,11] 750.00 USD
[234,4] International Freight
[234,11] 750.00 USD
[236,4] International Freight
[236,11] 750.00 USD
[238,4] International Freight
[238,11] 750.00 USD
[240,4] International Freight
[240,11] 750.00 USD
[242,4] International Freight
[242,11] 750.00 USD
[244,4] International Freight
[244,11] 750.00 USD
[245,4] International Freight
[245,11] 750.00 USD
[247,4] International Freight
[247,11] 750.00 USD
[249,4] International Freight
[249,11] 750.00 USD
[251,4] International Freight
[251,11] 750.00 USD
[253,4] International Freight
[253,11] 750.00 USD
[268,4] Page 3";

		const string BreakBulkContent =
@"[2,4] Shipper
[2,20] OCEAN SEA WAYBILL
[2,39] Ocean Bill of Lading
[5,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[5,39] HOUSEBILL001
[9,4] Consignee
[10,4] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[12,4] Notify Party
[14,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[16,4] Vessel
[16,11] Voyage
[16,20] Port of Loading
[16,32] Excess Value Declaration
[18,4] MAIN.Vessel
[18,11] MAINVoyage
[18,20] AUSYD - SYDNEY
[18,32] Refer to clause on reverse side
[21,4] Port of Discharge
[21,11] Destination (if on-carriage)
[21,20] Freight Payable At
[21,32] No. of Original B/L
[22,4] NZAKL - AUCKLAND
[22,11] AUCKLAND - NEW ZEALAND
[22,20] SYDNEY - AUSTRALIA
[22,32] 3 (THREE)
[23,4] Marks and Numbers
[23,11] Number and Kind of Packages / Description of Goods
[23,32] Gross Weight
Kgs.


[23,41] Mesaurement
M3


[25,4] marks & numbers
[25,11] goods description
[25,32] 10.000 KG
[25,41] 10000000.000 M3
[27,4] Reference Number
[27,8] Packs
[27,13] Dimensions
[27,34] Weight (Kg)
[27,41] Volume (m3)
[28,4] AAAA0000007
[28,8] 1 BOT
[28,13] Height: 1.000M  Length: 0.000M  Width: 0.000M
[28,34] 10.000
[28,41] 10000000.000
[29,13] Container Description
[31,13] HC: 0012
[32,13] UNDG1
[41,18] Blaticus1
[44,4] Shipped
[44,13] 01-Jan-24
[44,28] Payment Term:
[44,33] Prepaid
[46,24] Freight Details, Charges
[47,24] Charge Description
[47,34] Collect
[47,41] Prepaid
[49,24] International Freight
[49,41] 1000.00 AUD
[52,24] Origin Labour Charges
[52,34] 500.00 AUD
[54,24] Destination Labour Charges
[54,41] 750.00 USD
[56,4] Place and Date of Issue
[57,24] International Freight
[57,34] 750.00 USD
[58,14] 01-Jan-24
[59,24] International Freight
[59,34] 750.00 USD
[60,4] MELBOURNE - AUSTRALIA
[62,24] International Freight
[62,34] 750.00 USD
[64,24] International Freight
[64,34] 750.00 USD
[66,24] International Freight
[66,34] 750.00 USD
[69,24] International Freight
[69,34] 750.00 USD
[72,24] International Freight
[72,34] 750.00 USD
[74,24] International Freight
[74,34] 750.00 USD
[75,4] Place of Receipt
[75,14] Place of Delivery
[76,24] International Freight
[76,34] 750.00 USD
[77,24] International Freight
[77,34] 750.00 USD
[82,4] SYDNEY - AUSTRALIA
[82,14] AUCKLAND - NEW ZEALAND
[82,24] Total No. of Packages
[85,24] ONE Unit(s)
[86,21] Note/Signature:
[90,4] Page 1 *** Continues Next Page ***
[155,4] Page 2 *** Continues Next Page ***
[156,3] CONTINUATION PAGE
[156,19] Sea Waybill - HOUSEBILL001
[158,4] Consignor
[158,20] Consignee
[159,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[160,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[164,4] Notify Party
[164,20] Goods Collected From
[164,32] ETD
[164,36] 02-Jan-24
[165,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[165,20] SYDNEY, AUSTRALIA
[166,20] Goods Delivered To
[166,32] ETA
[166,36] 03-Jan-24
[167,20] AUCKLAND, NEW ZEALAND
[168,20] Weight
[168,32] Volume
[169,20] 10.000 KG
[169,32] 10000000.000 M3
[170,4] Phone: 
[170,20] Package Quantity
[171,4] Fax: 
[171,20] 10 BOT (OUTER)
[174,4] marks & numbers
[174,11] goods description
[174,32] 10.000 KG
[174,41] 10000000.000 M3
[177,4] Charge Description
[177,11] Collect
[177,17] Prepaid
[179,4] International Freight
[179,11] 750.00 USD
[181,4] International Freight
[181,11] 750.00 USD
[183,4] International Freight
[183,11] 750.00 USD
[185,4] International Freight
[185,11] 750.00 USD
[187,4] International Freight
[187,11] 750.00 USD
[189,4] International Freight
[189,11] 750.00 USD
[190,4] International Freight
[190,11] 750.00 USD
[192,4] International Freight
[192,11] 750.00 USD
[194,4] International Freight
[194,11] 750.00 USD
[196,4] International Freight
[196,11] 750.00 USD
[198,4] International Freight
[198,11] 750.00 USD
[200,4] International Freight
[200,11] 750.00 USD
[202,4] International Freight
[202,11] 750.00 USD
[204,4] International Freight
[204,11] 750.00 USD
[205,4] International Freight
[205,11] 750.00 USD
[207,4] International Freight
[207,11] 750.00 USD
[209,4] International Freight
[209,11] 750.00 USD
[211,4] International Freight
[211,11] 750.00 USD
[213,4] International Freight
[213,11] 750.00 USD
[215,4] International Freight
[215,11] 750.00 USD
[217,4] International Freight
[217,11] 750.00 USD
[218,4] International Freight
[218,11] 750.00 USD
[220,4] International Freight
[220,11] 750.00 USD
[222,4] International Freight
[222,11] 750.00 USD
[224,4] International Freight
[224,11] 750.00 USD
[226,4] International Freight
[226,11] 750.00 USD
[228,4] International Freight
[228,11] 750.00 USD
[230,4] International Freight
[230,11] 750.00 USD
[231,4] International Freight
[231,11] 750.00 USD
[233,4] International Freight
[233,11] 750.00 USD
[235,4] International Freight
[235,11] 750.00 USD
[237,4] International Freight
[237,11] 750.00 USD
[239,4] International Freight
[239,11] 750.00 USD
[241,4] International Freight
[241,11] 750.00 USD
[243,4] International Freight
[243,11] 750.00 USD
[244,4] International Freight
[244,11] 750.00 USD
[246,4] International Freight
[246,11] 750.00 USD
[248,4] International Freight
[248,11] 750.00 USD
[250,4] International Freight
[250,11] 750.00 USD
[252,4] International Freight
[252,11] 750.00 USD
[267,4] Page 3";

		const string BreakBulkContentWithLotsOfUNDGs =
@"[2,4] Shipper
[2,20] OCEAN SEA WAYBILL
[2,39] Ocean Bill of Lading
[5,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[5,39] HOUSEBILL001
[9,4] Consignee
[10,4] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[12,4] Notify Party
[14,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[16,4] Vessel
[16,11] Voyage
[16,20] Port of Loading
[16,32] Excess Value Declaration
[18,4] MAIN.Vessel
[18,11] MAINVoyage
[18,20] AUSYD - SYDNEY
[18,32] Refer to clause on reverse side
[21,4] Port of Discharge
[21,11] Destination (if on-carriage)
[21,20] Freight Payable At
[21,32] No. of Original B/L
[22,4] NZAKL - AUCKLAND
[22,11] AUCKLAND - NEW ZEALAND
[22,20] SYDNEY - AUSTRALIA
[22,32] 3 (THREE)
[23,4] Marks and Numbers
[23,11] Number and Kind of Packages / Description of Goods
[23,32] Gross Weight
Kgs.


[23,41] Mesaurement
M3


[25,4] marks & numbers
[25,11] goods description
[25,32] 10.000 KG
[25,41] 10000000.000 M3
[27,4] Reference Number
[27,8] Packs
[27,13] Dimensions
[27,34] Weight (Kg)
[27,41] Volume (m3)
[28,4] AAAA0000007
[28,8] 1 BOT
[28,13] Height: 1.000M  Length: 0.000M  Width: 0.000M
[28,34] 10.000
[28,41] 10000000.000
[29,13] Container Description
[31,13] HC: 0012
[32,13] UNDG1
[33,13] UNDG1
[34,13] UNDG1
[35,13] UNDG1
[36,13] UNDG1
[37,13] UNDG1
[38,13] UNDG1
[39,13] UNDG1
[40,13] UNDG1
[41,13] UNDG1
[42,13] UNDG1
[43,13] UNDG1
[44,13] UNDG1
[45,13] UNDG1
[47,13] UNDG1
[49,13] UNDG1
[51,13] UNDG1
[53,13] UNDG1
[58,18] Blaticus1
[61,4] Shipped
[61,13] 01-Jan-24
[61,28] Payment Term:
[61,33] Prepaid
[63,24] Freight Details, Charges
[64,24] Charge Description
[64,34] Collect
[64,41] Prepaid
[66,24] International Freight
[66,41] 1000.00 AUD
[69,24] Origin Labour Charges
[69,34] 500.00 AUD
[71,24] Destination Labour Charges
[71,41] 750.00 USD
[73,4] Place and Date of Issue
[74,24] International Freight
[74,34] 750.00 USD
[75,14] 01-Jan-24
[76,24] International Freight
[76,34] 750.00 USD
[77,4] MELBOURNE - AUSTRALIA
[79,24] International Freight
[79,34] 750.00 USD
[81,24] International Freight
[81,34] 750.00 USD
[83,24] International Freight
[83,34] 750.00 USD
[86,24] International Freight
[86,34] 750.00 USD
[89,24] International Freight
[89,34] 750.00 USD
[91,24] International Freight
[91,34] 750.00 USD
[92,4] Place of Receipt
[92,14] Place of Delivery
[93,24] International Freight
[93,34] 750.00 USD
[94,24] International Freight
[94,34] 750.00 USD
[99,4] SYDNEY - AUSTRALIA
[99,14] AUCKLAND - NEW ZEALAND
[99,24] Total No. of Packages
[102,24] ONE Unit(s)
[103,21] Note/Signature:
[107,4] Page 1 *** Continues Next Page ***
[172,4] Page 2 *** Continues Next Page ***
[173,3] CONTINUATION PAGE
[173,19] Sea Waybill - HOUSEBILL001
[175,4] Consignor
[175,20] Consignee
[176,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[177,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[181,4] Notify Party
[181,20] Goods Collected From
[181,32] ETD
[181,36] 02-Jan-24
[182,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[182,20] SYDNEY, AUSTRALIA
[183,20] Goods Delivered To
[183,32] ETA
[183,36] 03-Jan-24
[184,20] AUCKLAND, NEW ZEALAND
[185,20] Weight
[185,32] Volume
[186,20] 10.000 KG
[186,32] 10000000.000 M3
[187,4] Phone: 
[187,20] Package Quantity
[188,4] Fax: 
[188,20] 10 BOT (OUTER)
[191,4] marks & numbers
[191,11] goods description
[191,32] 10.000 KG
[191,41] 10000000.000 M3
[194,13] UNDG1
[196,13] UNDG1
[198,13] UNDG1
[200,13] UNDG1
[202,13] UNDG1
[204,13] UNDG1
[206,13] UNDG1
[207,13] UNDG1
[209,13] UNDG1
[211,13] UNDG1
[213,13] UNDG1
[215,13] UNDG1
[217,13] UNDG1
[219,13] UNDG1
[221,13] UNDG1
[222,13] UNDG1
[224,13] UNDG1
[226,13] UNDG1
[228,13] UNDG1
[230,13] UNDG1
[232,13] UNDG1
[234,13] UNDG1
[235,13] UNDG1
[237,13] UNDG1
[239,13] UNDG1
[241,13] UNDG1
[243,13] UNDG1
[245,13] UNDG1
[247,13] UNDG1
[248,13] UNDG1
[250,13] UNDG1
[252,13] UNDG1
[254,13] UNDG1
[256,13] UNDG1
[258,13] UNDG1
[260,13] UNDG1
[261,13] UNDG1
[263,13] UNDG1
[265,13] UNDG1
[267,13] UNDG1
[269,13] UNDG1
[271,13] UNDG1
[273,13] UNDG1
[275,13] UNDG1
[276,13] UNDG1
[278,13] UNDG1
[280,13] UNDG1
[282,13] UNDG1
[284,13] UNDG1
[286,13] UNDG1
[288,13] UNDG1
[289,13] UNDG1
[291,13] UNDG1
[293,13] UNDG1
[297,4] Page 3 *** Continues Next Page ***
[298,3] CONTINUATION PAGE
[298,19] Sea Waybill - HOUSEBILL001
[300,4] Consignor
[300,20] Consignee
[301,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[302,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[306,4] Notify Party
[306,20] Goods Collected From
[306,32] ETD
[306,36] 02-Jan-24
[307,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[307,20] SYDNEY, AUSTRALIA
[308,20] Goods Delivered To
[308,32] ETA
[308,36] 03-Jan-24
[309,20] AUCKLAND, NEW ZEALAND
[310,20] Weight
[310,32] Volume
[311,20] 10.000 KG
[311,32] 10000000.000 M3
[312,4] Phone: 
[312,20] Package Quantity
[313,4] Fax: 
[313,20] 10 BOT (OUTER)
[316,13] UNDG1
[317,13] UNDG1
[319,13] UNDG1
[321,13] UNDG1
[323,13] UNDG1
[325,13] UNDG1
[327,13] UNDG1
[329,13] UNDG1
[330,13] UNDG1
[332,13] UNDG1
[334,13] UNDG1
[336,13] UNDG1
[338,13] UNDG1
[340,13] UNDG1
[342,13] UNDG1
[343,13] UNDG1
[345,13] UNDG1
[347,13] UNDG1
[349,13] UNDG1
[351,13] UNDG1
[353,13] UNDG1
[355,13] UNDG1
[356,13] UNDG1
[358,13] UNDG1
[360,13] UNDG1
[362,13] UNDG1
[364,13] UNDG1
[366,13] UNDG1
[368,13] UNDG1
[369,4] Charge Description
[369,11] Collect
[369,17] Prepaid
[371,4] International Freight
[371,11] 750.00 USD
[373,4] International Freight
[373,11] 750.00 USD
[375,4] International Freight
[375,11] 750.00 USD
[377,4] International Freight
[377,11] 750.00 USD
[379,4] International Freight
[379,11] 750.00 USD
[381,4] International Freight
[381,11] 750.00 USD
[383,4] International Freight
[383,11] 750.00 USD
[384,4] International Freight
[384,11] 750.00 USD
[386,4] International Freight
[386,11] 750.00 USD
[388,4] International Freight
[388,11] 750.00 USD
[390,4] International Freight
[390,11] 750.00 USD
[392,4] International Freight
[392,11] 750.00 USD
[394,4] International Freight
[394,11] 750.00 USD
[396,4] International Freight
[396,11] 750.00 USD
[397,4] International Freight
[397,11] 750.00 USD
[399,4] International Freight
[399,11] 750.00 USD
[401,4] International Freight
[401,11] 750.00 USD
[403,4] International Freight
[403,11] 750.00 USD
[405,4] International Freight
[405,11] 750.00 USD
[407,4] International Freight
[407,11] 750.00 USD
[409,4] International Freight
[409,11] 750.00 USD
[410,4] International Freight
[410,11] 750.00 USD
[412,4] International Freight
[412,11] 750.00 USD
[414,4] International Freight
[414,11] 750.00 USD
[416,4] International Freight
[416,11] 750.00 USD
[418,4] International Freight
[418,11] 750.00 USD
[420,4] International Freight
[420,11] 750.00 USD
[422,4] International Freight
[422,11] 750.00 USD
[424,4] Page 4 *** Continues Next Page ***
[425,3] CONTINUATION PAGE
[425,19] Sea Waybill - HOUSEBILL001
[427,4] Consignor
[427,20] Consignee
[428,20] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[429,4] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[433,4] Notify Party
[433,20] Goods Collected From
[433,32] ETD
[433,36] 02-Jan-24
[434,4] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[434,20] SYDNEY, AUSTRALIA
[435,20] Goods Delivered To
[435,32] ETA
[435,36] 03-Jan-24
[436,20] AUCKLAND, NEW ZEALAND
[437,20] Weight
[437,32] Volume
[438,20] 10.000 KG
[438,32] 10000000.000 M3
[439,4] Phone: 
[439,20] Package Quantity
[440,4] Fax: 
[440,20] 10 BOT (OUTER)
[443,4] Charge Description
[443,11] Collect
[443,17] Prepaid
[445,4] International Freight
[445,11] 750.00 USD
[447,4] International Freight
[447,11] 750.00 USD
[449,4] International Freight
[449,11] 750.00 USD
[451,4] International Freight
[451,11] 750.00 USD
[453,4] International Freight
[453,11] 750.00 USD
[455,4] International Freight
[455,11] 750.00 USD
[457,4] International Freight
[457,11] 750.00 USD
[458,4] International Freight
[458,11] 750.00 USD
[460,4] International Freight
[460,11] 750.00 USD
[462,4] International Freight
[462,11] 750.00 USD
[464,4] International Freight
[464,11] 750.00 USD
[466,4] International Freight
[466,11] 750.00 USD
[507,4] Page 5";
		public BillOfLading GetNewBillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V0001";
			billOfLading.JS_HouseBill = "HOUSEBILL001";
			billOfLading.JS_PackingMode = ContainerModes.FCL;
			billOfLading.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "NZAKL";
			billOfLading.JS_HouseBillIssueDate = new ZDateTime(2024, 1, 1);
			billOfLading.JS_RL_NKHouseBillIssuePlace = "AUMEL";
			billOfLading.JS_HBLContainerPackModeOverride = HBLDeliveryModes.Codes.CY_CY;
			billOfLading.JS_INCO = DomesticPaymentTerms.Prepaid;
			billOfLading.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;
			billOfLading.JS_ShippedOnBoardDate = new ZDateTime(2024, 1, 1);
			billOfLading.JS_E_DEP = new ZDateTime(2024, 1, 2);
			billOfLading.JS_E_ARV = new ZDateTime(2024, 1, 3);
			billOfLading.JS_GoodsDescription = "goods description";
			billOfLading.JS_MarksAndNumbers = "marks & numbers";
			billOfLading.JS_BookingReference = "BKG000001";
			billOfLading.JS_HouseBillOfLadingType = "FIA";

			billOfLading.CustomsEntryNumber = "T7HRTXGXT";
			billOfLading.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var mainTransport = billOfLading.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";
			mainTransport.JW_Vessel = "MAIN.Vessel";
			mainTransport.JW_VoyageFlight = "MAINVoyage";

			var otherTransport = billOfLading.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "NZAKL";
			otherTransport.JW_RL_NKDiscPort = "NZALR";

			var container1 = billOfLading.FCLContainers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_ContainerMode = ContainerModes.FCL;

			var container2 = billOfLading.FCLContainers.AddNew();
			container2.JC_ContainerNum = "BBBB0000007";
			container2.JC_ContainerMode = ContainerModes.FCL;

			billOfLading.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = billOfLading.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 2000;
			packline1.JL_ActualWeightUQ = Weight.Kilograms;
			packline1.JL_ActualVolume = 1.3;
			packline1.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline1.JL_JS = billOfLading.PK;
			container1.PackLines.Add(packline1);

			var packline2 = billOfLading.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 3000;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;
			packline2.JL_ActualVolume = 1.3;
			packline2.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline2.JL_JS = billOfLading.PK;
			container2.PackLines.Add(packline2);

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			billOfLading.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "MANY";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "TOO MUCH";
			notifyParty3.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3.MainAddress.Address1 = "Unit 686";
			notifyParty3.MainAddress.Address2 = "99 How Lane";
			notifyParty3.MainAddress.City = "Auckland";
			notifyParty3.MainAddress.Postcode = "5038";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			var companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			billOfLading.JS_OH_DeliveryAgent = principal.PK;

			billOfLading.JS_NoCopyBills = 1;
			billOfLading.JS_NoOriginalBills = 2;
			billOfLading.JS_OuterPacks = 10;
			billOfLading.JS_F3_NKPackType = PkgUnit.Bottle;
			billOfLading.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_FullName = "Ziggy Z";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.MainAddress.Address1 = "Unit 13";
			localClient.MainAddress.Address2 = "4 Lost Lane";
			localClient.MainAddress.City = "Sydney";
			localClient.MainAddress.Postcode = "2000";
			localClient.MainAddress.OA_RN_NKCountryCode = "AU";

			var agentCollect = Factory.New<OrgHeader>();
			agentCollect.OH_FullName = "Airmarine Inc.";
			agentCollect.OH_RL_NKClosestPort = "USCHI";
			agentCollect.MainAddress.Address1 = "5638 S Central Ave";
			agentCollect.MainAddress.City = "Chicago";
			agentCollect.MainAddress.Postcode = "60638";
			agentCollect.MainAddress.OA_RN_NKCountryCode = "US";

			var loader = new JobHeader.Loader(billOfLading);
			var header = loader.TryLoadOrCreate();

			header.LocalChargesPK = localClient.PK;
			header.AgentCollectPK = agentCollect.PK;

			Factory.Save();

			return billOfLading;
		}

		BillOfLadingPackLine CreatePackingLine(BillOfLading billOfLading)
		{
			var packLine = billOfLading.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = CargoTypes.General;
			packLine.JL_RN_NKOrigin = "AU";
			packLine.JL_ItemNo = 12;
			packLine.JL_EndItemNo = 13;
			packLine.JL_ContainerPackingOrder = 1;
			packLine.JL_PackLineId = "2";
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_PackageCount = 3;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualWeightUQ = Weight.Kilograms;
			packLine.JL_ActualVolume = 2000;
			packLine.JL_ActualVolumeUQ = Volume.CubicDecimetres;
			packLine.JL_Length = 1;
			packLine.JL_Width = 2;
			packLine.JL_Height = 3;
			packLine.JL_UnitOfDimension = Length.Metres;
			packLine.JL_ExportRefNumber = "SLDNO001";
			packLine.JL_ImportRefNumber = "SLDNO002";
			packLine.JL_Description = "Goods 1";
			packLine.JL_MarksAndNumbers = "Marks 1";
			packLine.JL_F3_NKPackType = "PTL";
			packLine.JL_DetailedDescription = "DDD";
			packLine.JL_HarmonisedCode = "HSCodeData";
			packLine.JL_RefNumber = "JL_RefNumber";
			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = 5;
			packLine.JL_RequiredTemperatureMaximum = 6;
			packLine.JL_RequiredTemperatureUnit = Temperature.Centigrade;

			billOfLading.FCLContainers.Cast<CommonContainer>().First(x => x.JC_ContainerNum == "AAAA0000007").PackLines.Add(packLine);

			return packLine;
		}

		void CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;
		}
	}
}
