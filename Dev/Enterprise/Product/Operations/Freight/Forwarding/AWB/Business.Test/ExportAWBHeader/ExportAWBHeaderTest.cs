using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBHeader))]
	public class ExportAWBHeaderTest : EnterpriseBusinessObjectTestCaseWithListChecking<ExportAWBHeader>
	{
		public void TestEH_TotalGrossWeight()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();

			decimal lineWeight = 0m;
			decimal expectedTotal = 0m;
			for (int index = 0; index < 12; index++)
			{
				lineWeight += 0.1m;
				awbHeader.AWBRateLines[index].ER_GrossWeight = lineWeight;

				expectedTotal += lineWeight;
			}

			AssertEquals("EH_TotalGrossWeight has sum of all lines weights", expectedTotal, awbHeader.EH_TotalGrossWeight);
		}

		public void TestEH_TotalGrossWeight_DecimalPlaces()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			AssertEquals("EH_TotalGrossWeight should show 1 decimal", 1, awbHeader.TotalGrossWeightDecimalPlaces);
		}

		public void TestNatureAndQtyOfGoods()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "Line 1";
			awbHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "Line 2";
			awbHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "Line 4";
			awbHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription = "Line 12";

			ZString expectedNatureAndQtyOfGoods =
@"Line 1
Line 2

Line 4







Line 12".Replace("\r", "");
			AssertEquals("NatureAndQtyOfGoods should return all rate lines delimited by a new line", expectedNatureAndQtyOfGoods, awbHeader.NatureAndQtyOfGoods);

			awbHeader.NatureAndQtyOfGoods =
@"Line 1

Line 3


Line 6




Line 11
Line 12".Replace("\r", "");
			AssertEquals("Line 1", awbHeader.AWBRateLine1.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertEquals("Line 3", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine5.NatureAndQtyOfGoods.Text);
			AssertEquals("Line 6", awbHeader.AWBRateLine6.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine7.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine8.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine9.NatureAndQtyOfGoods.Text);
			AssertEquals("", awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text);
			AssertEquals("Line 11", awbHeader.AWBRateLine11.NatureAndQtyOfGoods.Text);
			AssertEquals("Line 12", awbHeader.AWBRateLine12.NatureAndQtyOfGoods.Text);
		}

		public void TestTextToNatureAndQtyOfGoodsLines()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			var lines = awbHeader.TextToNatureAndQtyOfGoodsLines("A very long detailed description of the goods which takes up more than one line");
			AssertMultilineASCIIEquals("Wraps to 20 characters", "A very long detailed description of\nthe goods which takes up more than\none line", lines);
		}

		public void TestSecurityStatus()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			ExportAWBSpecialHandling handling1 = awbHeader.AWBSpecialHandlingItems.AddNew();
			handling1.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CarbonDioxideSolidDryIce;
			ExportAWBSpecialHandling handling2 = awbHeader.AWBSpecialHandlingItems.AddNew();
			handling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

			Assert("Precondition: handling1 is not security status", !handling1.IsSecurityStatus);
			Assert("Precondition: handling2 is security status", handling2.IsSecurityStatus);
			AssertEquals("Only handling with security status expected", AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft, awbHeader.SecurityStatus.EP_SpecialHandling);
		}

		public void TestBranch()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			AssertEquals("awbHeader.Branch.GB_BranchName", GlbBranch.CurrentBranch.GB_BranchName, awbHeader.Branch.GB_BranchName);
			AssertEquals("awbHeader.Branch.PK", GlbBranch.CurrentBranch.PK, awbHeader.Branch.PK);
		}

		public void TestAgentIATACodeAllowsUpTo14CharactersOfEntry()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			AssertEquals("awbHeader.EH_AgentIATACodeInfo.MaxLength", 14, awbHeader.EH_AgentIATACodeFormattedInfo.MaxLength);
		}

		public void TestEditableAgentCodeFieldsForNewRecordsWithBlankAGTRegistryOff()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			AssertAgentCodeFieldsCanAllBeModifiedAndDefaultFromTheRegistryAndPersistToTheDatabase();
		}

		public void TestEditableAgentCodeFieldsForNewRecordsWithBlankAGTRegistryOn()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			AssertAgentCodeFieldsCanAllBeModifiedAndDefaultFromTheRegistryAndPersistToTheDatabase();
		}

		public void TestEditableAgentCodeFieldsForExistingRecordsWithBlankAGTRegistryOff()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			AssertAgentCodeFieldsAreReadOnlyAndDefaultFromTheRegistryOnExistingRecordsIeWithoutBranch();
		}

		public void TestEditableAgentCodeFieldsForExistingRecordsWithBlankAGTRegistryOn()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			AssertAgentCodeFieldsAreReadOnlyAndDefaultFromTheRegistryOnExistingRecordsIeWithoutBranch();
		}

		public void TestAWBTypeFunctionality()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();

			AssertEquals("Default: awbHeader.EH_AWBType", AWBTypeList.Codes.AgentMaster, awbHeader.EH_AWBType);
			AssertEquals("Default: awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.AgentMaster, awbHeader.AWBType);
			AssertEquals("Default: awbHeader.IsHouseAirWayBill", false, awbHeader.IsHouseAirWayBill);

			awbHeader.EH_AWBType = AWBTypeList.Codes.DirectMaster;
			AssertEquals("DirectMaster: awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.DirectMaster, awbHeader.AWBType);
			AssertEquals("DirectMaster: awbHeader.IsHouseAirWayBill", false, awbHeader.IsHouseAirWayBill);

			awbHeader.EH_AWBType = AWBTypeList.Codes.House;
			AssertEquals("House: awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.House, awbHeader.AWBType);
			AssertEquals("House: awbHeader.IsHouseAirWayBill", true, awbHeader.IsHouseAirWayBill);

			awbHeader.EH_AWBType = AWBTypeList.Codes.MasterHouse;
			AssertEquals("MasterHouse: awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.MasterHouse, awbHeader.AWBType);
			AssertEquals("MasterHouse: awbHeader.IsHouseAirWayBill", false, awbHeader.IsHouseAirWayBill);

			awbHeader.EH_AWBType = AWBTypeList.Codes.UndefinedMaster;
			AssertEquals("UndefinedMaster: awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.UndefinedMaster, awbHeader.AWBType);
			AssertEquals("UndefinedMaster: awbHeader.IsHouseAirWayBill", false, awbHeader.IsHouseAirWayBill);

			awbHeader.EH_AWBType = "";
			AssertEquals("'': awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.UndefinedMaster, awbHeader.AWBType);
			AssertEquals("'': awbHeader.IsHouseAirWayBill", false, awbHeader.IsHouseAirWayBill);

			awbHeader.EH_AWBType = "X_X";
			AssertEquals("X_X: awbHeader.AWBType", ExportAWBHeader.TypeOfAWB.UndefinedMaster, awbHeader.AWBType);
			AssertEquals("X_X: awbHeader.IsHouseAirWayBill", false, awbHeader.IsHouseAirWayBill);
		}

		public void TestSuspendSettingHasChanges()
		{
			ExportAWBHeader header = Factory.New<ExportAWBHeader>();

			using (header.SuspendSettingHasChanges())
			{
				header.EH_AWBType = AWBTypeList.Codes.House;

				header.AWBRateLine1.NatureAndQtyOfGoodsDescription = "One";
				header.AWBRateLine2.NatureAndQtyOfGoodsDescription = "Two";
				header.AWBRateLine3.NatureAndQtyOfGoodsDescription = "Three";
				header.AWBRateLine4.NatureAndQtyOfGoodsDescription = "Four";
				header.AWBRateLine5.NatureAndQtyOfGoodsDescription = "Five";
				header.AWBRateLine6.NatureAndQtyOfGoodsDescription = "Six";
				header.AWBRateLine7.NatureAndQtyOfGoodsDescription = "Seven";
				header.AWBRateLine8.NatureAndQtyOfGoodsDescription = "Eight";
				header.AWBRateLine9.NatureAndQtyOfGoodsDescription = "Nine";
				header.AWBRateLine10.NatureAndQtyOfGoodsDescription = "Ten";
				header.AWBRateLine11.NatureAndQtyOfGoodsDescription = "Eleven";
				header.AWBRateLine12.NatureAndQtyOfGoodsDescription = "Twelve";

				header.AWBRateLine1.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine2.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine3.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine4.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine5.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine6.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine7.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine8.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine9.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine10.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine11.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
				header.AWBRateLine12.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			}

			AssertEquals(false, header.HasChanges);
		}

		public void TestNatureAndQtyOfGoodsProperties()
		{
			var header = Factory.New<ExportAWBHeader>();

			header.AWBRateLine1.NatureAndQtyOfGoodsDescription = "One";
			header.AWBRateLine2.NatureAndQtyOfGoodsDescription = "Two";
			header.AWBRateLine3.NatureAndQtyOfGoodsDescription = "Three";
			header.AWBRateLine4.NatureAndQtyOfGoodsDescription = "Four";
			header.AWBRateLine5.NatureAndQtyOfGoodsDescription = "Five";
			header.AWBRateLine6.NatureAndQtyOfGoodsDescription = "Six";
			header.AWBRateLine7.NatureAndQtyOfGoodsDescription = "Seven";
			header.AWBRateLine8.NatureAndQtyOfGoodsDescription = "Eight";
			header.AWBRateLine9.NatureAndQtyOfGoodsDescription = "Nine";
			header.AWBRateLine10.NatureAndQtyOfGoodsDescription = "Ten";
			header.AWBRateLine11.NatureAndQtyOfGoodsDescription = "Eleven";
			header.AWBRateLine12.NatureAndQtyOfGoodsDescription = "Twelve";

			Factory.Save();

			var reloadedHeader = new BusinessObjectFactory().Load<ExportAWBHeader>(header.PK);

			AssertEquals("reloadedHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription", "One", reloadedHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription", "Two", reloadedHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription", "Three", reloadedHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription", "Four", reloadedHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription", "Five", reloadedHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription", "Six", reloadedHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription", "Seven", reloadedHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription", "Eight", reloadedHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription", "Nine", reloadedHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription", "Ten", reloadedHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription", "Eleven", reloadedHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("reloadedHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription", "Twelve", reloadedHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AssertEquals("reloadedHeader.HasChanges", false, reloadedHeader.HasChanges);
		}

		public void TestAWBRateLines()
		{
			ExportAWBHeader header = Factory.New<ExportAWBHeader>();

			AssertEquals(1, (int)header.AWBRateLines[0].ER_LineCount);
			AssertEquals(2, (int)header.AWBRateLines[1].ER_LineCount);
			AssertEquals(3, (int)header.AWBRateLines[2].ER_LineCount);
			AssertEquals(4, (int)header.AWBRateLines[3].ER_LineCount);
			AssertEquals(5, (int)header.AWBRateLines[4].ER_LineCount);
			AssertEquals(6, (int)header.AWBRateLines[5].ER_LineCount);
			AssertEquals(7, (int)header.AWBRateLines[6].ER_LineCount);
			AssertEquals(8, (int)header.AWBRateLines[7].ER_LineCount);
			AssertEquals(9, (int)header.AWBRateLines[8].ER_LineCount);
			AssertEquals(10, (int)header.AWBRateLines[9].ER_LineCount);
			AssertEquals(11, (int)header.AWBRateLines[10].ER_LineCount);
			AssertEquals(12, (int)header.AWBRateLines[11].ER_LineCount);

			AssertEquals(header.AWBRateLines[0], header.AWBRateLine1);
			AssertEquals(header.AWBRateLines[1], header.AWBRateLine2);
			AssertEquals(header.AWBRateLines[2], header.AWBRateLine3);
			AssertEquals(header.AWBRateLines[3], header.AWBRateLine4);
			AssertEquals(header.AWBRateLines[4], header.AWBRateLine5);
			AssertEquals(header.AWBRateLines[5], header.AWBRateLine6);
			AssertEquals(header.AWBRateLines[6], header.AWBRateLine7);
			AssertEquals(header.AWBRateLines[7], header.AWBRateLine8);
			AssertEquals(header.AWBRateLines[8], header.AWBRateLine9);
			AssertEquals(header.AWBRateLines[9], header.AWBRateLine10);
			AssertEquals(header.AWBRateLines[10], header.AWBRateLine11);
			AssertEquals(header.AWBRateLines[11], header.AWBRateLine12);
		}

		public void TestAgentNameAndPlaceLength()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "THESE ARE FIFTY CHARACTERS THESE ARE FIFTY CHARACT";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "THIS CITY IS FIFTY CHARACETRS THIS CITY IS FIFTY C";

			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;

			AssertEquals("THESE ARE FIFTY CHARACTERS THESE AR", awbHeader.EH_AgentName);
			AssertEquals("THIS CITY IS FIFT", awbHeader.EH_AgentPlace);

			AssertEquals("AgentName will only have the characters allowed by schema maxlength", 35, awbHeader.EH_AgentNameInfo.MaxLength);
			AssertEquals("AgentPlace will only have the characters allowed by schema maxlength", 17, awbHeader.EH_AgentPlaceInfo.MaxLength);

			var awbHeader1 = Factory.New<ExportAWBHeader>();
			awbHeader1.EH_GB_UserBranch = ZGuid.Empty;

			Factory.Save();

			AssertEquals("THESE ARE FIFTY CHARACTERS THESE ARE FIFTY CHARACT", awbHeader1.EH_AgentName);
			AssertEquals("THIS CITY IS FIFTY CHARACETRS THIS CITY IS FIFTY C", awbHeader1.EH_AgentPlace);
		}

		public void TestEH_By1stAirlineName()
		{
			var header = Factory.New<ExportAWBHeader>();
			header.EH_By1st = "US";
			AssertEquals("Multiple airlines share the US code, so result should be empty", ZString.Empty, header.EH_By1stAirlineName);

			header.EH_By1st = "QF";
			AssertEquals("QF uniquely identifies Qantas", "Qantas Airways Limited", header.EH_By1stAirlineName);
		}

		public void TestGetDisplayOption()
		{
			CombineAssertions(() =>
			{
				AssertGetDisplayOption(AWBTypeList.Codes.House, Core.Constants.AWB.EntitlementCode.Agent,
					ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption, ExportAWBRegistry.Instance.HAWBCollectDisplayOption);

				AssertGetDisplayOption(AWBTypeList.Codes.AgentMaster, Core.Constants.AWB.EntitlementCode.Carrier,
					ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption, ExportAWBRegistry.Instance.MAWBCollectDisplayOption);

				AssertGetDisplayOption(AWBTypeList.Codes.DirectMaster, Core.Constants.AWB.EntitlementCode.Carrier,
					ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption, ExportAWBRegistry.Instance.MAWBCollectDisplayOption);

				AssertGetDisplayOption(AWBTypeList.Codes.MasterHouse, Core.Constants.AWB.EntitlementCode.Carrier,
					ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption, ExportAWBRegistry.Instance.MAWBCollectDisplayOption);

				AssertGetDisplayOption(AWBTypeList.Codes.UndefinedMaster, Core.Constants.AWB.EntitlementCode.Carrier,
					ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption, ExportAWBRegistry.Instance.MAWBCollectDisplayOption);
			});
		}

		void AssertGetDisplayOption(ZString awbType, ZString defaultEntitlement, AWBDisplayOptionRegistryItem prepaidRegistryItem, AWBDisplayOptionRegistryItem collectRegistryItem)
		{
			ExportAWBHeader header = Factory.New<ExportAWBHeader>();
			header.EH_AWBType = awbType;
			AssertEquals("MSC|Blank IATA Code Mapping|" + defaultEntitlement, FormatAWBDisplayOption(header.GetDisplayOption(ZString.Empty, ZString.Empty)));
			AssertEquals("MSC|Blank IATA Code Mapping|" + defaultEntitlement, FormatAWBDisplayOption(header.GetDisplayOption(ZString.Empty, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid)));
			AssertEquals("MSC|Blank IATA Code Mapping|" + defaultEntitlement, FormatAWBDisplayOption(header.GetDisplayOption(ZString.Empty, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect)));

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(header.IsHouseAirWayBill ? AWBDisplayOptionType.HAWB : AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].IATADescription = (NoResString)"test mawb collect";
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			collectRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals("AS|test mawb collect|C", FormatAWBDisplayOption(header.GetDisplayOption(Core.Constants.AWB.ChargeCodes.AS, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect)));
			AssertEquals("AS|Assembly|" + defaultEntitlement, FormatAWBDisplayOption(header.GetDisplayOption(Core.Constants.AWB.ChargeCodes.AS, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid)));

			collection = AWBDisplayOptionCollection.GetDefault(header.IsHouseAirWayBill ? AWBDisplayOptionType.HAWB : AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].IATADescription = (NoResString)"test mawb prepaid";
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Split;
			prepaidRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals("AS|test mawb collect|C", FormatAWBDisplayOption(header.GetDisplayOption(Core.Constants.AWB.ChargeCodes.AS, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect)));
			AssertEquals("AS|test mawb prepaid|S", FormatAWBDisplayOption(header.GetDisplayOption(Core.Constants.AWB.ChargeCodes.AS, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid)));

			collectRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collectRegistryItem.DefaultValue);
			prepaidRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, prepaidRegistryItem.DefaultValue);
		}

		public void TestEH_SecurityStatus()
		{
			var header = Factory.New<ExportAWBHeader>();
			AssertEquals("Pre-condition", ZString.Empty, header.EH_SecurityStatus);

			var notASecurityStatus = header.AWBSpecialHandlingItems.AddNew();
			notASecurityStatus.EP_SpecialHandling = "ABC";
			AssertEquals("No security status items", ZString.Empty, header.EH_SecurityStatus);

			var securityStatus = header.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = "SCO";
			AssertEquals("Security Status SCO", "SCO", header.EH_SecurityStatus);

			securityStatus.EP_SpecialHandling = "NSC";
			AssertEquals("Security Status NSC", "NSC", header.EH_SecurityStatus);
		}

		public void TestEH_SecurityStatus_NotSecuredWhenAtLeastOneUnknowScreeningMethod()
		{
			var header = Factory.New<ExportAWBHeader>();
			AssertEquals("Pre-condition", ZString.Empty, header.EH_SecurityStatus);

			var notASecurityStatus = header.AWBSpecialHandlingItems.AddNew();
			notASecurityStatus.EP_SpecialHandling = "SCO";

			AssertEquals("Security Status", "SCO", header.EH_SecurityStatus);

			var line = header.ExportAWBSecurityStatusLines.AddNew();
			line.EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;

			AssertEquals("Security Status", "SCO", header.EH_SecurityStatus);

			line.EAS_ScreeningMethod = FreightDataRegistry.AviationSecurity_Unknown_Code;

			AssertEquals("Security Status", "NSC", header.EH_SecurityStatus);
		}

		public void TestExportAWBSecurityStatusLines()
		{
			var header = Factory.New<ExportAWBHeader>();

			var line = Factory.New<ExportAWBSecurityStatusLine>();
			line.EAS_EH = header.PK;

			AssertContainsExactElementsInAnyOrder(new[] { line },
				header.ExportAWBSecurityStatusLines);
		}

		public void TestCargoSecurityKnownShippers()
		{
			var header = Factory.New<ExportAWBHeader>();

			var line1 = Factory.New<ExportAWBSecurityStatusLine>();
			line1.EAS_EH = header.PK;
			line1.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			var line2 = Factory.New<ExportAWBSecurityStatusLine>();
			line2.EAS_EH = header.PK;
			line2.EAS_ScreeningMethod = ScreeningMethods.Codes.XRayEquipment;

			var line3 = Factory.New<ExportAWBSecurityStatusLine>();
			line3.EAS_EH = header.PK;
			line3.EAS_ExemptionGround = ExemptionCodes.Codes.NuclearMaterial;

			AssertContainsExactElementsInAnyOrder(new[] { line1 },
				header.CargoSecurityKnownShippers);
		}

		public void TestCargoSecurityScreeningMethods()
		{
			var header = Factory.New<ExportAWBHeader>();

			var line1 = Factory.New<ExportAWBSecurityStatusLine>();
			line1.EAS_EH = header.PK;
			line1.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			var line2 = Factory.New<ExportAWBSecurityStatusLine>();
			line2.EAS_EH = header.PK;
			line2.EAS_ScreeningMethod = ScreeningMethods.Codes.XRayEquipment;

			var line3 = Factory.New<ExportAWBSecurityStatusLine>();
			line3.EAS_EH = header.PK;
			line3.EAS_ExemptionGround = ExemptionCodes.Codes.NuclearMaterial;

			AssertContainsExactElementsInAnyOrder(new[] { line2 },
				header.CargoSecurityScreeningMethods);
		}

		public void TestCargoSecurityExemptionGrounds()
		{
			var header = Factory.New<ExportAWBHeader>();

			var line1 = Factory.New<ExportAWBSecurityStatusLine>();
			line1.EAS_EH = header.PK;
			line1.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			var line2 = Factory.New<ExportAWBSecurityStatusLine>();
			line2.EAS_EH = header.PK;
			line2.EAS_ScreeningMethod = ScreeningMethods.Codes.XRayEquipment;

			var line3 = Factory.New<ExportAWBSecurityStatusLine>();
			line3.EAS_EH = header.PK;
			line3.EAS_ExemptionGround = ExemptionCodes.Codes.NuclearMaterial;

			AssertContainsExactElementsInAnyOrder(new[] { line3 },
				header.CargoSecurityExemptionGrounds);
		}

		#region Implementation

		void AssertAgentCodeFieldsAreReadOnlyAndDefaultFromTheRegistryOnExistingRecordsIeWithoutBranch()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "7654321";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "FRED FLINTSTONE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "BEDROCK";

			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_GB_UserBranch = ZGuid.Empty;
			awbHeader.EH_AgentIATACodeFormatted = ZString.Empty;
			awbHeader.EH_AgentAccountNo = ZString.Empty;
			awbHeader.EH_AgentName = ZString.Empty;
			awbHeader.EH_AgentPlace = ZString.Empty;

			CombineAssertions(delegate
			{
				AssertEquals("awbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", awbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("awbHeader.EH_AgentAccountNo", "7654321", awbHeader.EH_AgentAccountNo);
				AssertEquals("awbHeader.EH_AgentName", "FRED FLINTSTONE", awbHeader.EH_AgentName);
				AssertEquals("awbHeader.EH_AgentPlace", "BEDROCK", awbHeader.EH_AgentPlace);

				AssertEquals("awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", true, awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentAccountNoInfo.ReadOnly", true, awbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentNameInfo.ReadOnly", true, awbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentPlaceInfo.ReadOnly", true, awbHeader.EH_AgentPlaceInfo.ReadOnly);
			});
		}

		void AssertAgentCodeFieldsCanAllBeModifiedAndDefaultFromTheRegistryAndPersistToTheDatabase()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "7654321";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "FRED FLINTSTONE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "BEDROCK";

			var awbHeader = Factory.New<ExportAWBHeader>();

			CombineAssertions(delegate
			{
				AssertEquals("awbHeader.EH_GB_UserBranch should be filled in by default", GlbBranch.CurrentBranch.PK, awbHeader.EH_GB_UserBranch);
				AssertEquals("awbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", awbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("awbHeader.EH_AgentAccountNo", "7654321", awbHeader.EH_AgentAccountNo);
				AssertEquals("awbHeader.EH_AgentName", "FRED FLINTSTONE", awbHeader.EH_AgentName);
				AssertEquals("awbHeader.EH_AgentPlace", "BEDROCK", awbHeader.EH_AgentPlace);

				AssertEquals("awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", false, awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentAccountNoInfo.ReadOnly", false, awbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentNameInfo.ReadOnly", false, awbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentPlaceInfo.ReadOnly", false, awbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			Factory.Save();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "";

			var reloadedAwbHeader = new BusinessObjectFactory().Load<ExportAWBHeader>(awbHeader.PK);
			CombineAssertions(delegate
			{
				AssertEquals("reloadedAwbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", reloadedAwbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("reloadedAwbHeader.EH_AgentAccountNo", "7654321", reloadedAwbHeader.EH_AgentAccountNo);
				AssertEquals("reloadedAwbHeader.EH_AgentName", "FRED FLINTSTONE", reloadedAwbHeader.EH_AgentName);
				AssertEquals("reloadedAwbHeader.EH_AgentPlace", "BEDROCK", reloadedAwbHeader.EH_AgentPlace);

				AssertEquals("awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", false, awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentAccountNoInfo.ReadOnly", false, awbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentNameInfo.ReadOnly", false, awbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentPlaceInfo.ReadOnly", false, awbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			reloadedAwbHeader.EH_AgentIATACodeFormatted = "765 4328";
			reloadedAwbHeader.EH_AgentAccountNo = "";
			reloadedAwbHeader.EH_AgentName = "ROAD RUNNER";
			reloadedAwbHeader.EH_AgentPlace = "THROUGH";

			CombineAssertions(delegate
			{
				AssertEquals("reloadedAwbHeader.EH_AgentIATACodeFormatted", "76-5 4328", reloadedAwbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("reloadedAwbHeader.EH_AgentAccountNo", "", reloadedAwbHeader.EH_AgentAccountNo);
				AssertEquals("reloadedAwbHeader.EH_AgentName", "ROAD RUNNER", reloadedAwbHeader.EH_AgentName);
				AssertEquals("reloadedAwbHeader.EH_AgentPlace", "THROUGH", reloadedAwbHeader.EH_AgentPlace);
			});
		}

		string FormatAWBDisplayOption(AWBDisplayOption displayOption)
		{
			return displayOption != null ? string.Format("{0}|{1}|{2}", displayOption.IATACode, displayOption.IATADescription, displayOption.Entitlement) : string.Empty;
		}

		#endregion
	}
}
