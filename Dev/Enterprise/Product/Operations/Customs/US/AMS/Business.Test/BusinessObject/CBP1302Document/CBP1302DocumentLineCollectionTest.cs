using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CBP1302DocumentLineCollection))]
	public class CBP1302DocumentLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CBP1302DocumentLineCollection>
	{
		protected override CBP1302DocumentLineCollection GetCollectionToTest()
		{
			return new CBP1302DocumentLineCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CBP1302DocumentLine(Bill);
		}

		CusInBondHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<CusInBondHeader>();
				}
				return header;
			}
		}
		CusInBondHeader header;

		CusInBondBill Bill
		{
			get
			{
				if (bill == null)
				{
					bill = Header.Bills.AddNew();
				}
				return bill;
			}
		}
		CusInBondBill bill;

		public void Test1302Properties()
		{
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "69", "69 DESC", startDate, endDate);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "A1", "A1 DESC", startDate, endDate);
			var cusCode3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "A2", "A2 DESC", startDate, endDate);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1";
			bill.B0_Weight = 1.0m;
			bill.B0_WeightUQ = "KG";
			bill.B0_RL_NKLastForeignPort = "ABCDE";
			bill.B0_LastForeignPortKCode = "12345";
			bill.B0_RL_NKForeignPortOfContract = "FGHIJ";
			bill.B0_ForeignPortOfContractKCode = "67890";
			bill.B0_IssuerCode = "XXXW";

			var foreignShipper = bill.DocAddresses.AddNew();
			foreignShipper.DocAddressType = DocAddressType.ForeignShipperDocumentaryAddress;
			foreignShipper.E2_Address1 = "123 FAKE ROAD";
			var consigneeAddress = bill.DocAddresses.AddNew();
			consigneeAddress.DocAddressType = DocAddressType.ConsigneeAddress;
			consigneeAddress.E2_Address1 = "12 CHERRY TREE LANE";
			var notifyParty = bill.DocAddresses.AddNew();
			notifyParty.DocAddressType = DocAddressType.NotifyParty;
			notifyParty.E2_Address1 = "10 DOWNING STREET";

			var container1 = bill.MovementDetail.Containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			container1.BC_Seal1 = "SEAL1";
			var commodity11 = container1.Commodities.AddNew();
			commodity11.BY_MarksAndNumbers = "MARK1";
			commodity11.BY_PieceCount = 1;
			commodity11.BY_ManifestUnitCode = "PAL";
			commodity11.BY_Description = "RED";
			commodity11.BY_GrossWeight = 2.0m;
			commodity11.BY_GrossWeightUnit = "KG";
			var commodity12 = container1.Commodities.AddNew();
			commodity12.BY_MarksAndNumbers = "MARK2";
			commodity12.BY_PieceCount = 2;
			commodity12.BY_ManifestUnitCode = "PAL";
			commodity12.BY_Description = "YELLOW";
			commodity12.BY_GrossWeight = 3.0m;
			commodity12.BY_GrossWeightUnit = "KG";
			var undg1 = container1.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3333", "", "IMO").First().PK;

			var container2 = bill.MovementDetail.Containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			container2.BC_Seal1 = "SEAL2";
			container2.BC_Seal2 = "SEAL3";
			var commodity21 = container2.Commodities.AddNew();
			commodity21.BY_MarksAndNumbers = "MARK1";
			commodity21.BY_PieceCount = 3;
			commodity21.BY_ManifestUnitCode = "PAL";
			commodity21.BY_Description = "BLUE";
			commodity21.BY_GrossWeight = 4.0m;
			commodity21.BY_GrossWeightUnit = "KG";
			var commodity22 = container2.Commodities.AddNew();
			commodity22.BY_MarksAndNumbers = "MARK2";
			commodity22.BY_PieceCount = 4;
			commodity22.BY_ManifestUnitCode = "PAL";
			commodity22.BY_Description = "GREEN";
			commodity22.BY_GrossWeight = 5.0m;
			commodity22.BY_GrossWeightUnit = "KG";
			var undg2 = container2.UNDGs.AddNew();
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3333", "", "IMO").First().PK;

			var container3 = bill.MovementDetail.Containers.AddNew();
			container3.BC_ContainerNum = "CONT3";
			container3.BC_Seal2 = "SEAL4";
			var commodity31 = container3.Commodities.AddNew();
			commodity31.BY_MarksAndNumbers = "MARK1";
			commodity31.BY_PieceCount = 5;
			commodity31.BY_ManifestUnitCode = "PAL";
			commodity31.BY_Description = "ORANGE";
			commodity31.BY_GrossWeight = 6.0m;
			commodity31.BY_GrossWeightUnit = "KG";
			var commodity32 = container3.Commodities.AddNew();
			commodity32.BY_MarksAndNumbers = "MARK2";
			commodity32.BY_PieceCount = 6;
			commodity32.BY_ManifestUnitCode = "PAL";
			commodity32.BY_Description = "BLACK";
			commodity32.BY_GrossWeight = 7.0m;
			commodity32.BY_GrossWeightUnit = "KG";
			var undg3 = container3.UNDGs.AddNew();
			undg3.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3333", "", "IMO").First().PK;
			var undg4 = container3.UNDGs.AddNew();
			undg4.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3334", "", "IMO").First().PK;

			var disp1 = bill.DispositionCodes.AddNew();
			disp1.US_Code = "A1";
			disp1.US_DispositionDate = new ZDateTime(2014, 3, 26, 9, 34, 0);
			var disp2 = bill.DispositionCodes.AddNew();
			disp2.US_Code = "69";
			disp2.US_DispositionDate = new ZDateTime(2014, 3, 26, 9, 33, 0);
			var disp3 = bill.DispositionCodes.AddNew();
			disp3.US_Code = "A2";
			disp3.US_DispositionDate = new ZDateTime(2014, 3, 26, 17, 34, 0);

			ZString dispositionOutput = @"XXXWMB1

DISPOSITIONS:
69 69 DESC 03/26/2014 9:33:00 AM

A1 A1 DESC 03/26/2014 9:34:00 AM

A2 A2 DESC 03/26/2014 5:34:00 PM

";

			ZString weightAndUnit0 = @"2 KG
3 KG";

			ZString weightAndUnit1 = @"4 KG
5 KG";

			ZString weightAndUnit2 = @"6 KG
7 KG";

			ZString cont0 = @"MN: MARK1
MARK2
CN: CONT1
SN: SEAL1";

			ZString cont1 = @"MN: MARK1
MARK2
CN: CONT2
SN: SEAL2
SEAL3";

			ZString cont2 = @"MN: MARK1
MARK2
CN: CONT3
SN: SEAL4";

			ZString pack0 = @"1 PAL RED
2 PAL YELLOW
UN: 3333 ";

			ZString pack1 = @"3 PAL BLUE
4 PAL GREEN
UN: 3333 ";

			ZString pack2 = @"5 PAL ORANGE
6 PAL BLACK
UN: 3333 3334";

			var lines = header.DocumentLines;
			lines.Initialise1302();

			AssertEquals(3, lines.Count);
			Assert(lines[0].FirstContainerInBill);
			Assert(!lines[1].FirstContainerInBill);
			Assert(!lines[2].FirstContainerInBill);

			AssertContains(weightAndUnit0, lines[0].WeightAndUnit);
			AssertContains(weightAndUnit1, lines[1].WeightAndUnit);
			AssertContains(weightAndUnit2, lines[2].WeightAndUnit);

			AssertEquals("ABCDE 12345", lines[0].LastForeignPortCombined);
			AssertEquals("FGHIJ 67890", lines[0].ForeignPortOfContractCombined);
			AssertEquals(ZString.Empty, lines[1].LastForeignPortCombined);
			AssertEquals(ZString.Empty, lines[1].ForeignPortOfContractCombined);
			AssertEquals(ZString.Empty, lines[2].LastForeignPortCombined);
			AssertEquals(ZString.Empty, lines[2].ForeignPortOfContractCombined);

			AssertContains("SH: 123 FAKE ROAD", lines[0].Addresses);
			AssertNotContains("CO: 12 CHERRY TREE LANE", lines[0].Addresses);
			AssertNotContains("NF: 10 DOWNING STREET", lines[0].Addresses);

			AssertNotContains("SH: 123 FAKE ROAD", lines[1].Addresses);
			AssertContains("CO: 12 CHERRY TREE LANE", lines[1].Addresses);
			AssertNotContains("NF: 10 DOWNING STREET", lines[1].Addresses);

			AssertNotContains("SH: 123 FAKE ROAD", lines[2].Addresses);
			AssertNotContains("CO: 12 CHERRY TREE LANE", lines[2].Addresses);
			AssertContains("NF: 10 DOWNING STREET", lines[2].Addresses);

			AssertContains(cont0, lines[0].ContainersMarksSeals);
			AssertContains(cont1, lines[1].ContainersMarksSeals);
			AssertContains(cont2, lines[2].ContainersMarksSeals);

			AssertContains(pack0, lines[0].PackagesAndDescriptions);
			AssertContains(pack1, lines[1].PackagesAndDescriptions);
			AssertContains(pack2, lines[2].PackagesAndDescriptions);

			AssertEquals("XXXWMB1", lines[0].BillNumber);
			AssertEquals(ZString.Empty, lines[1].BillNumber);
			AssertEquals(ZString.Empty, lines[2].BillNumber);

			AssertEquals(dispositionOutput, lines[0].Dispositions);
			AssertEquals(ZString.Empty, lines[1].Dispositions);
			AssertEquals(ZString.Empty, lines[2].Dispositions);
		}
	}
}
