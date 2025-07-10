using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.AWB.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Business.AWB.Testing.FWBTest;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class FHLTest : FBaseTest
	{
		public void TestFHL4()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				const string expectedFHL4 = @"
FHL/4
MBI/006-96667465DFWFRA/T4K245
HBS/CVGA00176739/DFWFRA/4/K245/4/WILD CHERRY BAR
/NSC/EAW/PEF/SCO
TXT/WILD CHERRY BAR DIMS 13X12X15 IN X 2 DIMS 48X40X40 IN X 1 DIMS 12
/X11X16 IN X 1        4 SLAC
OCI/US/SHP/T/USC7788
/US/SHP/CT/13175910000
/DE/CNE/T/XYZ8899
/DE/CNE/CT/49665279340
/KR/NFY/T/9900
/US/DNR/D/UN1234
/US/DNR/D/UN2345
SHP/HARRIS   FORD LLC
/9307 EAST 56TH STREET
/INDIANAPOLIS/IN
/US/46216/TE/13175910000
CNE/WELLA MANUFACTURING GMBH
/WELLASTRASE 2-4
/HUENFELD
/DE/36088/TE/49665279340
CVD/USD/CC/NVD/4289.68/XXX
";

				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				var actualFHL4 = new FHL(new FWBMessageDetails(dataCreator.ConsolAWBHeader), new FHLMessageDetails(dataCreator.ShipmentAWBHeader), FHL.Version.No4).ToString();

				AssertMultilineASCIIEquals("FHL Message as Version 4", expectedFHL4.Trim(), actualFHL4.Trim());
			}
		}

		public void TestToStringAllData()
		{
			AssertMessageCorrect();
		}

		public void TestCVDIfBothPPDandCOLareChecked()
		{
			ShipmentExportAWBHeader aWBHeader = CreateTemplateShipmentAWBHeader();
			CVD = "CVD/ZAR/CC/123.54/NCV/XXX\r\n";
			aWBHeader.EH_OtherPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			aWBHeader.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			FHL fHL = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(aWBHeader), FHL.Version.No2);

			AssertMultilineEquals("Check The Strings", Expected, fHL.ToString(), '\r');
		}

		public void TestCVDIfOnlyPPDIsChecked()
		{
			ShipmentExportAWBHeader aWBHeader = CreateTemplateShipmentAWBHeader();
			CVD = "CVD/ZAR/PP/123.54/NCV/XXX\r\n";
			aWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			aWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			FHL fHL = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(aWBHeader), FHL.Version.No2);

			AssertMultilineEquals("Check The Strings", Expected, fHL.ToString(), '\r');
		}

		public void TestCVDIfOnlyCOLIsChecked()
		{
			ShipmentExportAWBHeader aWBHeader = CreateTemplateShipmentAWBHeader();
			CVD = "CVD/ZAR/CC/123.54/NCV/XXX\r\n";
			aWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			aWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			FHL fHL = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(aWBHeader), FHL.Version.No2);

			AssertMultilineEquals("Check The Strings", Expected, fHL.ToString(), '\r');
		}

		public void TestSLACOnlyShowsWhenNotZero()
		{
			ShipmentExportAWBHeader awbHeader = CreateTemplateShipmentAWBHeader();
			FHL fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No4);
			VNO = "4";
			HBS = "HBS/AEI12345678/SINSYD/4/K400/7/COMPUTER PARTS\r\n";
			AssertMultilineEquals("SLAC is greater than 0, should appear on FHL", Expected, fhl.ToString(), '\r');

			awbHeader.EH_ShippingLoadAndCount = 0;
			fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No4);
			HBS = "HBS/AEI12345678/SINSYD/4/K400//COMPUTER PARTS\r\n";
			AssertMultilineEquals("SLAC is 0, should not appear on FHL", Expected, fhl.ToString(), '\r');
		}

		public void TestPostcode()
		{
			ShipmentExportAWBHeader awbHeader = CreateTemplateShipmentAWBHeader();
			FHL fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No2);
			awbHeader.EH_ShipperPostCode = "XYZ123";
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/XYZ123/FAX/12345678\r\n";
			AssertMultilineEquals("Valid postcode is shown on FHL", Expected, fhl.ToString(), '\r');

			awbHeader.EH_ShipperPostCode = "XY**12";
			fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No2);
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/XY  12/FAX/12345678\r\n";
			AssertMultilineEquals("Invalid characters are replaced with spaces", Expected, fhl.ToString(), '\r');

			awbHeader.EH_ShipperPostCode = "***";
			fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No2);
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG//FAX/12345678\r\n";
			AssertMultilineEquals("Postcode is omitted but its separator is included as contact details exist", Expected, fhl.ToString(), '\r');

			awbHeader.EH_ShipperContactCode = "";
			awbHeader.EH_ShipperContactDetail = "";
			awbHeader.EH_ShipperPostCode = "XYZ123";
			fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No2);
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/XYZ123\r\n";
			AssertMultilineEquals("Valid postcode is shown when no contact details", Expected, fhl.ToString(), '\r');

			awbHeader.EH_ShipperPostCode = "***";
			fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No2);
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG\r\n";
			AssertMultilineEquals("Postcode and separator are omitted when postcode is invalid and contact details are omitted", Expected, fhl.ToString(), '\r');

			awbHeader.EH_ShipperPostCode = "";
			fhl = new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(awbHeader), FHL.Version.No2);
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG\r\n";
			AssertMultilineEquals("Postcode and separator are omitted for empty postcode", Expected, fhl.ToString(), '\r');
		}

		public void TestFHL_ConsigneeTradeTradeNo_ForImportToChina()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_ConsigneeTraderNo = "1234567";
			header.EH_ConsigneeCountryCode = CountryCodes.China;
			header.EH_ConsigneeTraderNoCountryCode = CountryCodes.China;
			header.EH_ConsigneeTraderNoType = "USCI";
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/USCI1234567", message);
		}

		public void TestFHL_AlsoNotifyTradeTradeNo_ForImportToChina()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_AlsoNotifyTraderNo = "1234567";
			header.EH_AlsoNotifyCountryCode = CountryCodes.China;
			header.EH_AlsoNotifyTraderNoCountryCode = CountryCodes.China;
			header.EH_AlsoNotifyTraderNoType = "USCI";
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("NFY/T/USCI1234567", message);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactNameNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsNotInChina_OCIIdNotEmpty_ConsigneeContactNameNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.SetupHAWBHeader1();
			var header = dataCreator.HAWBHeader1;
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactNameNotEmpty_OCIPresent()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/CN/CNE/AB/IGOR";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactDetailNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsNotInChina_OCIIdNotEmpty_ConsigneeContactDetailNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.SetupHAWBHeader1();
			var header = dataCreator.HAWBHeader1;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactDetailNotEmpty_ConsigneeContactCodeFax_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.FAX;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/CNE", fhlMessage);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactDetailNotEmpty_OCIPresent()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/CN/CNE/AB/111
";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInChina_ConsigneeTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeCountryCode = CountryCodes.China;
			header.EH_ConsigneeTraderNo = "111";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/CN/CNE/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInIndonesia_ConsigneeTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Indonesia;
			header.EH_ConsigneeTraderNo = "111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Indonesia);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/ID/CNE/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestShipperIsInIndonesia_ShipperTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ShipperCountryCode = CountryCodes.Indonesia;
			header.EH_ShipperTraderNo = "111";
			header.OriginCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Indonesia);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/ID/SHP/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInKenya_ConsigneeTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Kenya;
			header.EH_ConsigneeTraderNo = "111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/KE/CNE/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInKenya_OCIModifiesConsigneeTraderNoTypeIfPIN()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Kenya;
			header.EH_ConsigneeTraderNo = "111";
			header.EH_ConsigneeTraderNoType = "PIN";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/KE/CNE/T/P111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInKenya_OCIIncludesConsigneeTraderNoTypeIfNotPIN()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Kenya;
			header.EH_ConsigneeTraderNo = "111";
			header.EH_ConsigneeTraderNoType = "XYZ";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/KE/CNE/T/XYZ111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInBrazil_ConsigneeTraderNoDoesNotContainSpecialCharacters()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Brazil;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Brazil);
			header.EH_ConsigneeTraderNoType = "CNPJ";
			header.EH_ConsigneeTraderNo = "13.339. 532/0001-08";

			const string expected = @"OCI/US/SHP/CT/13175910000
/BR/CNE/T/CNPJ13339532000108";
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludesConsigneeTraderNoType()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.EH_ConsigneeTraderNo = "111";
			header.EH_ConsigneeTraderNoType = "PIN";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/EG/CNE/T/PIN111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			AssertContains(expected, fhlMessage);
		}

		public void TestOCI_IncludeEmailAndIP_ByDestination()
		{
			TestCase("USLAX", true);
			TestCase("AUMEL", false);
			TestCase("PRCAG", true);
			TestCase("VIAGL", true);
			TestCase("GUDED", true);
			TestCase("MPROP", true);
			TestCase("ASAPI", true);

			void TestCase(string destPort, bool shouldIncludeEmail)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;

				var header = Factory.New<ShipmentExportAWBHeaderTest>();
				header.EH_ParentID = shipment.PK;
				header.Populate();
				header.EH_ShipperCountryCode = Core.Constants.CountryCodes.Australia;
				header.EH_ShipperContactEmail = "shipper@abc.com.au";
				header.EH_ConsigneeCountryCode = "US";
				header.EH_ConsigneeContactEmail = "consignee@bcd.com";

				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.Add(shipment);
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = destPort;

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = consol.PK;
				awbHeader.Populate();

				var expectedEmails = @"OCI/US/SHP/MU/SHIPPER
/US/SHP/MD/ABC.COM.AU
/US/CNE/MU/CONSIGNEE
/US/CNE/MD/BCD.COM";
				var expectedIPs = @"/US/CUS/IA/127.0.0.1
/US/CUS/IR/127.0.0.1";

				var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4).ToString();
				if (shouldIncludeEmail)
				{
					AssertContains(expectedEmails, fhlMessage);
					AssertContains(expectedIPs, fhlMessage);
				}
				else
				{
					AssertNotContains(expectedEmails, fhlMessage);
					AssertNotContains(expectedIPs, fhlMessage);
				}
			}
		}

		public void TestOCI_IncludeEmailAndIP_ByTransport()
		{
			TestCase(new string[] { "AUSYD", "USLAX", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "NLABC" }, false);
			TestCase(new string[] { "AUSYD", "HKHKG", "USATL", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "PRCAG", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "VIAGL", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "GUDED", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "MPROP", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "ASAPI", "AUMEL", "NLABC" }, true);

			void TestCase(string[] ports, bool shouldIncludeEmail)
			{
				var consol = Factory.New<ForwardingConsol>();
				for (var i = 0; i < ports.Length - 1; i++)
				{
					var transport = consol.Transports.AddNew();
					transport.JW_RL_NKLoadPort = ports[i];
					transport.JW_RL_NKDiscPort = ports[i + 1];
				}

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = ports.First();
				shipment.JS_RL_NKDestination = ports.Last();

				var header = Factory.New<ShipmentExportAWBHeaderTest>();
				header.EH_ParentID = shipment.PK;
				header.Populate();
				header.EH_ShipperCountryCode = "AU";
				header.EH_ShipperContactEmail = "shipper@abc.com.au";
				header.EH_ConsigneeCountryCode = "US";
				header.EH_ConsigneeContactEmail = "consignee@bcd.com";

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = consol.PK;
				awbHeader.Populate();

				var expectedEmails = @"OCI/US/SHP/MU/SHIPPER
/US/SHP/MD/ABC.COM.AU
/US/CNE/MU/CONSIGNEE
/US/CNE/MD/BCD.COM";
				var expectedIPs = @"/US/CUS/IA/127.0.0.1
/US/CUS/IR/127.0.0.1";

				var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4).ToString();
				if (shouldIncludeEmail)
				{
					AssertContains(expectedEmails, fhlMessage);
					AssertContains(expectedIPs, fhlMessage);
				}
				else
				{
					AssertNotContains(expectedEmails, fhlMessage);
					AssertNotContains(expectedIPs, fhlMessage);
				}
			}
		}

		public void TestOCI_VerifiedKnownConsignor()
		{
			TestCase("AUSYD", "APP", "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: true, shouldIncludeVKC: false, "The route does not pass through the United States, so there will be no indicators in the OCI segment.");
			TestCase("USLAX", "APP", "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is APP, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", "PHS", "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is not APP, but Consignor has a valid Known status, so the indicator appears in the OCI segment and is Y");
			TestCase("USLAX", "PHS", "N", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is not APP, but neither Consignor nor Local Client has a valid Known status, so the indicator appears in the OCI segment and is N.");
			TestCase("USLAX", "PHS", "Y", shouldOnlyCheckLocalClient: true, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is not APP, but Local Client has valid Known status, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", "PHS", "N", shouldOnlyCheckLocalClient: true, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is not APP, but Local Client doesn't have a valid Known status, so the indicator appears in the OCI segment and is N.");

			void TestCase(string destPortCode, string inspectionType, string knownConsignorCode, bool shouldOnlyCheckLocalClient, bool hasValidKnownStatus, bool shouldIncludeVKC, string reason)
			{
				var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;
				if (shouldOnlyCheckLocalClient)
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
				}
				else
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
				}

				using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
				{
					var consol = Factory.New<ForwardingConsol>();
					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					consol.JK_RL_NKDischargePort = destPortCode;
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var shipment = consol.Shipments.AddNew();
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.JS_RL_NKOrigin = "GBLON";
					shipment.JS_RL_NKDestination = destPortCode;
					
					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CON";
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

					var localClient = Factory.NewWithValidTestData<OrgHeader>();
					localClient.OH_Code = "LOC";
					shipment.CreateShipmentJobHeaderWithMutex();
					shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = localClient.PK;
					shipment.JS_InspectionTypeCode = inspectionType;

					if (hasValidKnownStatus)
					{
						consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;

						var addressCountryData1 = consignor.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData1.OV_OH_OrgHeader = consignor.PK;
						addressCountryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

						localClient.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;

						var addressCountryData2 = localClient.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData2.OV_OH_OrgHeader = localClient.PK;
						addressCountryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					}

					var header = Factory.New<ShipmentExportAWBHeaderTest>();
					header.EH_ParentID = shipment.PK;
					header.Populate();
					header.EH_ShipperCountryCode = "GB";
					header.EH_ConsigneeCountryCode = "US";

					var expected = @"/US/CUS/KP/";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4).ToString();

					if (shouldIncludeVKC)
					{
						expected += knownConsignorCode;
						AssertContains(expected, fhlMessage);
					}
					else
					{
						AssertNotContains(expected, fhlMessage);
					}
					shipment.Job.Dispose();
				}
			}
		}
		public void TestOCI_VerifiedKnownConsignorWithWarning()
		{
			TestCase("USLAX", "APP", "Y", hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is APP, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", "PHS", "Y", hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is not APP, but Local Client has valid Known status, so the indicator appears in the OCI segment and is Y.");

			void TestCase(string destPortCode, string inspectionType, string knownConsignorCode, bool hasValidKnownStatus, bool shouldIncludeVKC, string reason)
			{
				var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;
				((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;

				using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
				{
					var consol = Factory.New<ForwardingConsol>();
					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					consol.JK_RL_NKDischargePort = destPortCode;
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var shipment = consol.Shipments.AddNew();
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.JS_RL_NKOrigin = "GBLON";
					shipment.JS_RL_NKDestination = destPortCode;

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CON";
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

					var localClient = Factory.NewWithValidTestData<OrgHeader>();
					localClient.OH_Code = "LOC";
					shipment.CreateShipmentJobHeaderWithMutex();
					shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = localClient.PK;
					shipment.JS_InspectionTypeCode = inspectionType;

					if (hasValidKnownStatus)
					{
						consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;

						var addressCountryData1 = consignor.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData1.OV_OH_OrgHeader = consignor.PK;
						addressCountryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

						localClient.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;

						var addressCountryData2 = localClient.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData2.OV_OH_OrgHeader = localClient.PK;
						addressCountryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					}

					var header = Factory.New<ShipmentExportAWBHeaderTest>();
					header.EH_ParentID = shipment.PK;
					header.Populate();
					header.EH_ShipperCountryCode = "GB";
					header.EH_ConsigneeCountryCode = "US";

					var expected = @"/US/CUS/KP/";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4).ToString();

					if (shouldIncludeVKC)
					{
						expected += knownConsignorCode;
						AssertContains(expected, fhlMessage);
					}
					else
					{
						AssertNotContains(expected, fhlMessage);
					}
					shipment.Job.Dispose();
				}
			}
		}

		public void TestOCI_VerifiedKnownConsignorAndLocalClientWithWarning()
		{
			TestCase("USLAX", "PHS", "Y", consignorWithWarning: true, consignorWithYes: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is PHS and Consignor and Local Client are set to Yes/Warning, but Consignor has valid Known status, so the indicator appears in the OCI segment and is Y");
			TestCase("USLAX", "PHS", "N", consignorWithWarning: true, consignorWithYes: false, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is PHS and Consignor and Local Client are set set to Yes/Warning, but Consignor doesn't have valid Known status so the indicator appears in the OCI segment and is N.");

			TestCase("USLAX", "PHS", "Y", consignorWithWarning: false, consignorWithYes: true, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is PHS and Consignor and Local Client are set to Yes/Warning, but Consignor has valid Known status, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", "PHS", "N", consignorWithWarning: false, consignorWithYes: true, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is PHS and Consignor and Local Client are set to Yes/Warning, but Consignor doesn't have valid Known status so the indicator appears in the OCI segment and is N.");

			TestCase("USLAX", "APP", "Y", consignorWithWarning: false, consignorWithYes: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is APP and Consignor is set as No and Local Client is set as WARN, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", "PHS", "Y", consignorWithWarning: false, consignorWithYes: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is PHS and Consignor is set as No and Local Client is set as WARN, but Consignor has valid Known status, so the indicator appears in the OCI segment and is Y.");

			void TestCase(string destPortCode, string inspectionType, string knownConsignorCode, bool consignorWithWarning, bool consignorWithYes, bool hasValidKnownStatus, bool shouldIncludeVKC, string reason)
			{
				var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;
				((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
				if (consignorWithWarning)
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
				}
				else if (consignorWithYes)
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
				}
				
				using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
				{
					var consol = Factory.New<ForwardingConsol>();
					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					consol.JK_RL_NKDischargePort = destPortCode;
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var shipment = consol.Shipments.AddNew();
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.JS_RL_NKOrigin = "GBLON";
					shipment.JS_RL_NKDestination = destPortCode;

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CON";
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

					var localClient = Factory.NewWithValidTestData<OrgHeader>();
					localClient.OH_Code = "LOC";
					shipment.CreateShipmentJobHeaderWithMutex();
					shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = localClient.PK;
					shipment.JS_InspectionTypeCode = inspectionType;

					if (hasValidKnownStatus)
					{
						consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;

						var addressCountryData1 = consignor.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData1.OV_OH_OrgHeader = consignor.PK;
						addressCountryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					}

					localClient.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;
					var addressCountryData2 = localClient.MainAddress.KnownShipperDetails.AddNew();
					addressCountryData2.OV_OH_OrgHeader = localClient.PK;
					addressCountryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					addressCountryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					
					var header = Factory.New<ShipmentExportAWBHeaderTest>();
					header.EH_ParentID = shipment.PK;
					header.Populate();
					header.EH_ShipperCountryCode = "GB";
					header.EH_ConsigneeCountryCode = "US";

					var expected = @"/US/CUS/KP/";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4).ToString();

					if (shouldIncludeVKC)
					{
						expected += knownConsignorCode;
						AssertContains(expected, fhlMessage);
					}
					else
					{
						AssertNotContains(expected, fhlMessage);
					}
					shipment.Job.Dispose();
				}
			}
		}

		public void TestOCI_CustomerAccountHolderAndName()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			string expected, message;

			shipment.JS_RL_NKOrigin = "AUSYD";
			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "AGT";
			shipmentHeader.EH_ShipperName = "Shipment Shipper Name";
			shipmentHeader.EH_ConsigneeName = "Shipment Consignee Name";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_FullName = "Controlling Customer Org Name";
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			expected = @"/US/CUS/AH/3
/US/CUS/AN/CONTROLLING CUSTOMER ORG NAME";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			shipmentHeader.EH_WeightVPPDCOL = "PPD";
			expected = @"/US/CUS/AH/S
/US/CUS/AN/SHIPMENT SHIPPER NAME";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AH/C
/US/CUS/AN/SHIPMENT CONSIGNEE NAME";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "BTH";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			expected = @"/US/CUS/AH/C
/US/CUS/AN/SHIPMENT CONSIGNEE NAME";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			expected = @"/US/CUS/AH/S
/US/CUS/AN/SHIPMENT SHIPPER NAME";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);
		}

		public void TestOCI_CustomerAccountIssuerAndNumber()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			string expected, message;

			shipment.JS_RL_NKOrigin = "AUSYD";
			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "AGT";
			shipmentHeader.EH_ShipperAccount = "Shipper Acc";
			shipmentHeader.EH_ConsigneeAccount = "Consignee Acc";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			controllingCustomer.OH_Code = "CtrlCustCode";

			var proxyOrgCusCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			proxyOrgCusCode.OK_RN_NKCodeCountry = "US";
			proxyOrgCusCode.OK_CodeType = "CCA";
			proxyOrgCusCode.SecuredCustomsRegNo = "USCCAREGNO";

			expected = @"/US/CUS/AI/USCCAREGNO
/US/CUS/AR/CTRLCUSTCODE";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			proxyOrgCusCode.OK_CodeType = "CCP";
			shipmentHeader.EH_WeightVPPDCOL = "PPD";
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/SHIPPERACC";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/CONSIGNEEACC";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "BTH";
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/SHIPPERACC";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/CONSIGNEEACC";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);
		}

		public void TestOCI_CustomerAccountShippingFrequency()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			// 1 shipment, calculate by controlling customer
			var testConsolHeader = CreateData(1, DateTime.Now.AddDays(-10));
			var testShipmentHeader = testConsolHeader.Consol.Shipments[0].AWBHeader;
			var message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testShipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// Add 30 shipments with future ETD (Should not affect the result), calculate by controlling customer. 
			testConsolHeader = CreateData(30, DateTime.Now.AddDays(10));
			testShipmentHeader = testConsolHeader.Consol.Shipments[0].AWBHeader;
			message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testShipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// Add 30 shipments, calculate by consignee
			testConsolHeader = CreateData(30, DateTime.Now.AddDays(-10));
			testConsolHeader.Consol.Shipments[0].ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			testShipmentHeader = testConsolHeader.Consol.Shipments[0].AWBHeader;
			message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testShipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// Add 30 shipments with future ETD (Should not affect the result), calculate by consignee
			testConsolHeader = CreateData(30, DateTime.Now.AddDays(10));
			testConsolHeader.Consol.Shipments[0].ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			testShipmentHeader = testConsolHeader.Consol.Shipments[0].AWBHeader;
			message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testShipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// calculate by shipper. Add 30 shipments with future ETD (Should not affect the result)
			testConsolHeader.Consol.Shipments[0].ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			testShipmentHeader = testConsolHeader.Consol.Shipments[0].AWBHeader;
			testShipmentHeader.EH_WeightVPPDCOL = "PPD";
			message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testShipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// Add 30 shipments, calculate by shipper
			testConsolHeader = CreateData(30, DateTime.Now.AddDays(-10));
			testConsolHeader.Consol.Shipments[0].ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			testShipmentHeader = testConsolHeader.Consol.Shipments[0].AWBHeader;
			testShipmentHeader.EH_WeightVPPDCOL = "PPD";
			message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testShipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/R", message);

			// Immediate Transaction
			shipper.OH_IsTempAccount = true;
			message = new FHL(new FWBMessageDetails(testConsolHeader), new FHLMessageDetails(testConsolHeader.Consol.Shipments[0].AWBHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/AF/I", message);

			ConsolExportAWBHeader CreateData(int numOfShipment, ZDateTime shipmentETD)
			{
				var consol = Factory.New<ForwardingConsol>();
				var consolHeader = Factory.New<ConsolExportAWBHeader>();
				consolHeader.EH_ParentID = consol.PK;
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USSEA";

				for (var i = 0; i < numOfShipment; i++)
				{
					var shipment = consol.Shipments.AddNew();
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
					shipment.ConsignorPK = shipper.PK;
					shipment.ConsigneePK = consignee.PK;
					shipment.JS_E_DEP = shipmentETD;

					var shipmentHeader = Factory.New<ShipmentExportAWBHeaderTest>();
					shipmentHeader.EH_ParentID = shipment.PK;
				}
				Factory.Save();
				return consolHeader;
			}
		}

		public void TestOCI_CustomerAccountEstablishmentDateAndBillingType()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			string expected, message;

			shipment.JS_RL_NKOrigin = "AUSYD";
			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "AGT";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			controllingCustomer.OH_Code = "CtrlCustCode";
			controllingCustomer.OH_IsCreditor = false;
			controllingCustomer.CompanyData.OB_APCreditAgreedPaymentMethod = "CCD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_Code = "Consignor";
			consignor.OH_IsCreditor = false;
			consignor.CompanyData.OB_APCreditAgreedPaymentMethod = "CHK";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_Code = "Consignee";
			consignee.OH_IsCreditor = false;
			consignee.CompanyData.OB_APCreditAgreedPaymentMethod = "TRF";

			var proxyOrgCusCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			proxyOrgCusCode.OK_RN_NKCodeCountry = "US";
			proxyOrgCusCode.OK_CodeType = "CCA";
			proxyOrgCusCode.SecuredCustomsRegNo = "USCCAREGNO";

			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertNotContains("/US/CUS/AE", message);
			AssertNotContains("/US/CUS/BT", message);

			controllingCustomer.OH_IsCreditor = true;
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertNotContains("/US/CUS/AE", message);

			controllingCustomer.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 12, 12, 30, 00);
			consignor.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 13, 12, 30, 00);
			consignee.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 14, 12, 30, 00);
			expected = @"/US/CUS/AE/12JUN24
/US/CUS/BT/CC";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			proxyOrgCusCode.OK_CodeType = "CCP";

			shipmentHeader.EH_WeightVPPDCOL = "PPD";

			expected = @"/US/CUS/AE/13JUN24";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);
			AssertNotContains("/US/CUS/BT", message);

			consignor.OH_IsCreditor = true;
			expected = @"/US/CUS/AE/13JUN24
/US/CUS/BT/CHQ";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AE/14JUN24";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);
			AssertNotContains("/US/CUS/BT", message);

			consignee.OH_IsCreditor = true;
			expected = @"/US/CUS/AE/14JUN24
/US/CUS/BT/EFT";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "BTH";
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			expected = @"/US/CUS/AE/13JUN24
/US/CUS/BT/CHQ";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			expected = @"/US/CUS/AE/14JUN24
/US/CUS/BT/EFT";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			consignee.OH_SystemCreateTimeUtc = new ZDateTime(2023, 07, 09, 12, 30, 00);
			expected = @"/US/CUS/AE/09JUL23
/US/CUS/BT/EFT";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains(expected, message);

			consignee.OH_SystemCreateTimeUtc = ZDateTime.Empty;
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertNotContains("/US/CUS/AE", message);
		}

		public void Test_ValidateAccountHolderAndNameForFHL_ShouldNotThrow()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			var acasHandler = new UsaACASCountryHandler(consolHeader);

			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "AGT";
			shipmentHeader.EH_ShipperName = "Shipment Shipper Name";
			shipmentHeader.EH_ConsigneeName = "Shipment Consignee Name";

			AssertNoExceptionThrown("Should not have Null Ref Exception during validation", () =>
			{
				var validateResult = acasHandler.ValidateAccountHolderAndNameForFHL(shipmentHeader);
				AssertEquals(true, validateResult);

				shipment = null;
				validateResult = acasHandler.ValidateAccountHolderAndNameForFHL(shipmentHeader);
				AssertEquals(true, validateResult);

				shipmentHeader = null;
				validateResult = acasHandler.ValidateAccountHolderAndNameForFHL(shipmentHeader);
				AssertEquals(true, validateResult);
			});
		}

		public void TestOCI_BiographicData()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Category = shipper.OH_Category = consignee.OH_Category = "NAT";

			var cusCode = controllingCustomer.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.SecuredCustomsRegNo = "UsPassport123";

			cusCode = shipper.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.SecuredCustomsRegNo = "AuPassport456";

			cusCode = consignee.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "NZ";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DriverLicenceID;
			cusCode.SecuredCustomsRegNo = "NzDriverLicense000";

			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			consol.JK_OverrideWaybillDefaults = true;
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKDischargePort = "USLAX";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			shipment.ConsignorPK = shipper.PK;
			shipment.ConsigneePK = consignee.PK;

			var shipmentHeader = Factory.New<ShipmentExportAWBHeaderTest>();
			shipmentHeader.EH_ParentID = shipment.PK;

			// Bio data from ctrl customer
			var message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains("/US/CUS/PI/PPT-US-USPASSPORT123", message);

			// Bio data from shipper
			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			shipmentHeader.EH_WeightVPPDCOL = "PPD";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains("US/CUS/PI/PPT-AU-AUPASSPORT456", message);

			// Bio data from consignee
			shipmentHeader.EH_WeightVPPDCOL = "COL";
			message = new FHL(new FWBMessageDetails(consolHeader), new FHLMessageDetails(shipmentHeader), FHL.Version.No4).ToString();
			AssertContains("US/CUS/PI/DL-NZ-NZDRIVERLICENSE000", message);
		}

		public void TestOCILineConsigneeTraderNo_DoesNotIncludeBINPrefix_Bangladesh()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Bangladesh;
			header.EH_ConsigneeTraderNoType = OrgCusCode.BangladeshCodeTypes.BIN;
			header.EH_ConsigneeTraderNo = "111111111111111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bangladesh);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/BD/CNE/T/111111111111111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCILineAlsoNotifyTraderNo_DoesNotIncludeBINPrefix_Bangladesh()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);

			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyTraderNo = "111111111111111";
			header.EH_AlsoNotifyCountryCode = CountryCodes.Bangladesh;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.BangladeshCodeTypes.BIN;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/BD/NFY/T/111111111111111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCILineConsigneeTraderNo_IncludeAINPrefix_Bangladesh()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Bangladesh;
			header.EH_ConsigneeTraderNoType = OrgCusCode.BangladeshCodeTypes.AIN;
			header.EH_ConsigneeTraderNo = "111111111111111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bangladesh);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/BD/CNE/T/AIN111111111111111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCILineAlsoNotifyTraderNo_IncludeAINPrefix_Bangladesh()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);

			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyTraderNo = "111111111111111";
			header.EH_AlsoNotifyCountryCode = CountryCodes.Bangladesh;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.BangladeshCodeTypes.AIN;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/BD/NFY/T/AIN111111111111111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_ICEasConsigneeTraderNoType_Morocco()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Morocco;
			header.EH_ConsigneeTraderNoType = "ICE";
			header.EH_ConsigneeTraderNo = "111111111111111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Morocco);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/CNE/T/111111111111111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_ICEisMissing_ConsigneeTraderNo_HasDefaultValue_Morocco()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Morocco;
			header.EH_ConsigneeTraderNoType = "ICE";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Morocco);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/CNE/T/000000000000000";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_AlsoNotify_ICEasConsigneeTraderNoType_Morocco()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);

			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyTraderNo = "111111111111111";
			header.EH_AlsoNotifyCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoType = "ICE";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/MA/NFY/T/111111111111111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_AlsoNotify_ICEisMissing_ConsigneeTraderNo_HasDefaultValue_Morocco()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);

			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoType = "ICE";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/MA/NFY/T/000000000000000";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_NITasConsigneeTraderNoType_Bolivia()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Bolivia;
			header.EH_ConsigneeTraderNoType = "NIT";
			header.EH_ConsigneeTraderNo = "886644220";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bolivia);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/BO/CNE/T/NIT886644220";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_NITasShipperTraderNoType_Bolivia()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ShipperCountryCode = CountryCodes.Bolivia;
			header.EH_ShipperTraderNoType = "NIT";
			header.EH_ShipperTraderNo = "886644220";
			header.OriginCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bolivia);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/BO/SHP/T/NIT886644220";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIIncludes_RTNasConsigneeTraderNoType_Honduras()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Honduras;
			header.EH_ConsigneeTraderNoType = "RTN";
			header.EH_ConsigneeTraderNo = "5678999";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Honduras);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/HN/CNE/T/RTN5678999";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCIInformationIdentifierWhenIsICS2SelfFilling()
		{
			var hawbHeader = Factory.New<ShipmentExportAWBHeaderTest>();
			hawbHeader.EH_ConsigneeCountryCode = CountryCodes.Netherlands;
			hawbHeader.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Netherlands);
			hawbHeader.EH_ConsigneeTraderNoType = "PIN";
			hawbHeader.EH_ConsigneeTraderNo = "111";

			hawbHeader.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			hawbHeader.Shipment.JS_TransportMode = TransportModes.Air;
			hawbHeader.Shipment.JS_RL_NKLoadPort = "AUSYD";
			hawbHeader.Shipment.JS_RL_NKDischargePort = "NLABC";

			var mawbHeader = Factory.New<ConsolExportAWBHeader>();
			var consol = hawbHeader.Shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			mawbHeader.EH_ParentID = consol.PK;
			mawbHeader.Populate();

			var transport = hawbHeader.Shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NLABC";

			var expected = @"OCI/NL/CNE/T/PIN111";
			var fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
			var fhlDetailsProvider = new FHLMessageDetails(hawbHeader);
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var misc = receivingForwarder.MiscServ;
			misc.OM_FWAdvanceCargoReportingSelfFiler = true;

			hawbHeader.Populate();
			Assert(hawbHeader.HasInboundToICS2Zone);
			Assert(hawbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			var notExpected = @"OCI/NL/DCL/T/PIN111";
			hawbHeader.EH_ConsigneeCountryCode = CountryCodes.Netherlands;
			hawbHeader.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Netherlands);
			hawbHeader.EH_ConsigneeTraderNoType = "PIN";
			hawbHeader.EH_ConsigneeTraderNo = "111";
			fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
			fhlDetailsProvider = new FHLMessageDetails(hawbHeader);
			fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			Assert("Should always be false for FHL", !fhlDetailsProvider.IsDeclarantForAdvancedCargoReporting);
			AssertContains(expected, fhlMessage);
			AssertNotContains(notExpected, fhlMessage);
		}

		public void TestShipperTrader_EnableChinaCustomsTaxNumberTable()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ShipperCountryCode = CountryCodes.Kenya;
			header.EH_ShipperTraderNoType = "TRADE REGISTER NUMBER";
			header.EH_ShipperTraderNo = "111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/KE/SHP/T/TRADEREGISTERNUMBER111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeTrader_EnableChinaCustomsTaxNumberTable()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.EH_ConsigneeTraderNoType = "TRADE REGISTER NUMBER";
			header.EH_ConsigneeTraderNo = "111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/EG/CNE/T/TRADEREGISTERNUMBER111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestAlsoNotifyTrader_EnableChinaCustomsTaxNumberTable()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Kenya;
			header.EH_AlsoNotifyTraderNoType = "TRADE REGISTER NUMBER";
			header.EH_AlsoNotifyTraderNo = "111";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/DE/CNE/CT/49665279340
/KE/NFY/T/TRADEREGISTERNUMBER111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdEmpty_AlsoNotifyContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactName = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdEmpty_AlsoNotifyContactNameNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactName = "IGOR";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsNotInChina_OCIIdNotEmpty_AlsoNotifyContactNameNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.SetupHAWBHeader1();
			var header = dataCreator.HAWBHeader1;
			header.EH_AlsoNotifyContactName = "IGOR";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactName = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactNameNotEmpty_OCIPresent()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactName = "IGOR";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/CN/NFY/AB/IGOR
";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdEmpty_AlsoNotifyContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdEmpty_AlsoNotifyContactDetailNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = "111";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsNotInChina_OCIIdNotEmpty_AlsoNotifyContactDetailNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.SetupHAWBHeader1();
			var header = dataCreator.HAWBHeader1;
			header.EH_AlsoNotifyContactDetail = "111";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactDetailNotEmpty_AlsoNotifyContactCodeFax_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = "111";
			header.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.FAX;
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertNotContains("/CN/NFY", fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactDetailNotEmpty_OCIPresent()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = "111";
			header.EH_By1st = "XX";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"/CN/NFY/AB/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestAlsoNotifyIsInChina_AlsoNotifyTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyCountryCode = CountryCodes.China;
			header.EH_AlsoNotifyTraderNo = "111";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/CN/NFY/T/111
";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestAlsoNotifyIsInBrazil_AlsoNotifyTraderNoDoesNotContainSpecialCharacters()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Brazil;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Brazil);
			header.EH_AlsoNotifyTraderNoType = "CNPJ";
			header.EH_AlsoNotifyTraderNo = "13.339. 532/0001-08";

			const string expected = @"OCI/US/SHP/CT/13175910000
/DE/CNE/CT/49665279340
/BR/NFY/T/CNPJ13339532000108";
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestDestinationIsInChina_ShipperTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.IsImportToChinaForTest = true;
			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperCountryCode = CountryCodes.Australia;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/AU/SHP/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestShipperIsInChina_ShipperTraderNoUsesTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperCountryCode = CountryCodes.China;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/CN/SHP/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestShipperIsNotInChina_ShipperTraderNoDoesntUseTIdentifier()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperCountryCode = CountryCodes.Australia;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/AU/SHP/T/111";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestOCISegment_CorrectCNEAndNFYOrder()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);

			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";
			airline.RM_ContactPhoneOCIIdentifier = "PH";

			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";
			header.EH_ShipperTraderNo = "ze number lelel";
			header.EH_ConsigneeTraderNo = "ze cons number pepehands";
			header.EH_ConsigneeContactDetail = "1300655506";
			header.EH_AlsoNotifyTraderNo = "Y33T XD W00T UwU*";
			header.EH_AlsoNotifyContactName = "Clubber lang";
			header.EH_AlsoNotifyContactCode = "CL";
			header.EH_AlsoNotifyContactDetail = "H3110";
			header.IsImportToChinaForTest = true;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			string expected = @"OCI/US/SHP/T/ZE NUMBER LELEL
/US/SHP/CT/13175910000
/DE/CNE/T/ZE CONS NUMBER PEPEHANDS
/CN/CNE/AB/IGOR
/CN/CNE/PH/1300655506
//NFY/T/Y33T XD W00T UWU 
/CN/NFY/AB/CLUBBER LANG
";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);

			header.IsImportToChinaForTest = false;
			header.IsTransitingThroughChinaForTest = true;

			expected = @"OCI/US/SHP/T/ZE NUMBER LELEL
/CN/SHP/PH/13175910000
/DE/CNE/T/ZE CONS NUMBER PEPEHANDS
/CN/CNE/AB/IGOR
/CN/CNE/PH/1300655506
//NFY/T/Y33T XD W00T UWU 
/CN/NFY/AB/CLUBBER LANG
";
			fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);

			header.IsTransitingThroughChinaForTest = false;

			expected = @"OCI/US/SHP/T/ZE NUMBER LELEL
/US/SHP/CT/13175910000
/DE/CNE/T/ZE CONS NUMBER PEPEHANDS
/DE/CNE/CP/IGOR
/DE/CNE/CT/1300655506
//NFY/T/Y33T XD W00T UWU 
//NFY/CP/CLUBBER LANG
";
			fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInArgentina_ConsigneeTraderNoHasSpecialCharsRemoved()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Argentina;
			header.EH_ConsigneeTraderNo = "_11@1-1,A%1-";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Argentina);

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			const string expected = @"OCI/US/SHP/CT/13175910000
/AR/CNE/T/1111A1";
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains(expected, fhlMessage);
		}

		public void TestConsigneeIsInEgypt_OCIModifiesConsigneeTraderNoTypeIsVAT()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_ConsigneeTraderNoType = OrgCusCode.CodeTypes.VATCode;
			header.EH_ConsigneeTraderNo = "88995566";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/88995566", message);
		}

		public void TestNotifyPartyIsInEgypt_OCIModifiesNotifyPartyTypeIsVAT()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.CodeTypes.VATCode;
			header.EH_AlsoNotifyTraderNo = "88995566";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("NFY/T/88995566", message);
		}

		public void TestShipperTradeIsInEgypt_OCIModifiesShipperTradeTypeIsVAT()
		{
			var dataCreator = new AWBTestDataCreator<ShipmentExportAWBHeaderTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_ShipperCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_ShipperTraderNoType = OrgCusCode.CodeTypes.VATCode;
			header.EH_ShipperTraderNo = "88995566";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			AssertContains("SHP/T/88995566", message);
		}

		public void TestFHLDestinationCode_MultiRoutes()
		{
			var awbShipmentHeader = Factory.New<ShipmentExportAWBHeader>();
			awbShipmentHeader.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			awbShipmentHeader.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			awbShipmentHeader.Shipment.JS_HouseBill = "CVGA00176739";
			awbShipmentHeader.Shipment.JS_OverrideWaybillDefaults = ZBool.True;
			awbShipmentHeader.Populate();

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Consol.JK_OverrideWaybillDefaults = ZBool.True;
			awbHeader.Consol.JK_MasterBillNum = "00696667465";
			awbHeader.Populate();

			var message = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(awbShipmentHeader), FHL.Version.No4).ToString();
			var expectedContainingString = $@"MBI/006-96667465/T0K0";
			AssertContains(expectedContainingString, message);

			awbShipmentHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
			awbShipmentHeader.Shipment.JS_RL_NKDestination = "USLAX";
			awbShipmentHeader.Populate();

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			awbHeader.Populate();

			message = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(awbShipmentHeader), FHL.Version.No4).ToString();
			expectedContainingString = $@"MBI/006-96667465SYDLAX/T0K0";
			AssertContains(expectedContainingString, message);

			var flight1 = consol.Transports[0];
			flight1.JW_RL_NKLoadPort = "AUSYD";
			flight1.JW_RL_NKDiscPort = "NZAKL";
			flight1.JW_TransportMode = Core.Constants.TransportModes.Air;

			var flight2 = consol.Transports.AddNew();
			flight2.JW_RL_NKLoadPort = "NZAKL";
			flight2.JW_RL_NKDiscPort = "USLAX";
			flight2.JW_TransportMode = Core.Constants.TransportModes.Road;

			awbShipmentHeader.Populate();
			awbHeader.Populate();

			message = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(awbShipmentHeader), FHL.Version.No4).ToString();
			expectedContainingString = $@"MBI/006-96667465SYDAKL/T0K0";
			AssertContains(expectedContainingString, message);
		}

		public void TestUSTerritories_PRF_ExportStatementConfigSet_FHL()
		{
			TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_PRF_ExportStatementConfigSet_FHL_OCI_Contains_PRFdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
						"AES", "AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true,
						true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = Factory.New<ShipmentExportAWBHeader>();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;
					shipment.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
					cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber1.CE_EntryNum = "X20100101987654";
					header.EH_ParentID = shipment.PK;
					header.Populate();

					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					var consol = Factory.New<ForwardingConsol>();
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var expected = $@"OCI/{countryCode}/EXP/M/AES X20100101987654";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4);
					AssertContains(expected, fhlMessage.ToString());
				}
			}
		}

		public void TestUSTerritories_PDU_ExportStatementConfigSet_FHL()
		{
			TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_PDU_ExportStatementConfigSet_FHL_OCI_Contains_PDUdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PDU",
						"AESPOST", "Postdeparture Citation-USPPI", "SHP", "DOE", "UDF", true, true, true, true, true,
						true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = Factory.New<ShipmentExportAWBHeader>();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);
					shipment.DocsAndCartage.JP_ExportStatement = "PDU";
					header.EH_ParentID = shipment.PK;

					var consignor = Factory.New<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678912");
					shipment.ConsignorPK = consignor.PK;

					header.Populate();

					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					var consol = Factory.New<ForwardingConsol>();
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var expected = $@"OCI/{countryCode}/EXP/M/PDF 12345678912 20101001";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4);
					AssertContains(expected, fhlMessage.ToString());
				}
			}
		}

		public void TestUSTerritories_DWN_ExportStatementConfigSet_EntryFilerIdSetInRegistry_FHL()
		{
			TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			void TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var filer = new ExportEntryFilerID();
					filer.EntryFilerID = "111111111";
					filer.EntryFilerIDType = "D";

					ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

					var defaultValue = new CountryExportStatementSettingCollection();
					var exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

					var header = Factory.New<ShipmentExportAWBHeader>();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);
					shipment.DocsAndCartage.JP_ExportStatement = "DWN";

					header.EH_ParentID = shipment.PK;
					header.Populate();

					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					var consol = Factory.New<ForwardingConsol>();
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var expected = $@"OCI/{countryCode}/EXP/M/AED 111111111 20101001";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4);
					AssertContains(expected, fhlMessage.ToString());
				}
			}
		}

		public void TestUSTerritories_DWN_ExportStatementConfigSet_EntryFilerId_NotSetInRegistry_FHL()
		{
			TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_DWN_ExportStatementConfigSet_FHL_OCI_Contains_DWNdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = Factory.New<ShipmentExportAWBHeader>();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);
					shipment.DocsAndCartage.JP_ExportStatement = "DWN";

					header.EH_ParentID = shipment.PK;
					header.Populate();

					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					var consol = Factory.New<ForwardingConsol>();
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var expected = $@"OCI/{countryCode}/EXP/M/AED 20101001";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4);
					AssertContains(expected, fhlMessage.ToString());
				}
			}
		}

		public void TestUSTerritories_LOW_ExportStatementConfigSet_FHL()
		{
			TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");
			void TestUSExport_LOW_ExportStatementConfigSet_FHL_OCI_Contains_LOWdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW", "NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = Factory.New<ShipmentExportAWBHeader>();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;
					shipment.DocsAndCartage.JP_ExportStatement = "LOW";

					header.EH_ParentID = shipment.PK;
					header.Populate();

					var awbHeader = Factory.New<ConsolExportAWBHeader>();
					var consol = Factory.New<ForwardingConsol>();
					awbHeader.EH_ParentID = consol.PK;
					awbHeader.Populate();

					var expected = $@"OCI/{countryCode}/EXP/M/AES NOEEI EXC";
					var fhlMessage = new FHL(new FWBMessageDetails(awbHeader), new FHLMessageDetails(header), FHL.Version.No4);
					AssertContains(expected, fhlMessage.ToString());
				}
			}
		}

		public void TestUSExport_Multiple_ExportStatementConfigSet_FHL_OCI_Contains_AllDetails()
		{
			var countryCode = CountryCodes.UnitedStates;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();
				CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
				exportStatementSetting.CountryCode = CountryCodes.UnitedStates;
				exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW", "NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true, true, true, true));
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

				var hawbHeader = Factory.New<ShipmentExportAWBHeader>();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.DocsAndCartage.JP_ExportStatement = "LOW";

				hawbHeader.EH_ParentID = shipment.PK;

				var mawbHeader = Factory.New<ConsolExportAWBHeader>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				mawbHeader.EH_ParentID = consol.PK;
				mawbHeader.Populate();

				var agentTypes = new List<string> { "DRT", "CLD", "AGT" };
				foreach (var agentType in agentTypes)
				{
					mawbHeader.Consol.JK_AgentType = agentType;
					mawbHeader.Populate();
					hawbHeader.Populate();

					var fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
					var fhlDetailsProvider = new FHLMessageDetails(hawbHeader);

					const string expected = @"OCI/US/EXP/M/AES NOEEI";
					var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
					AssertContains(expected, fhlMessage);
				}
			}
		}

		public void TestDGVariantNotShown()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				var message = new FHL(new FWBMessageDetails(dataCreator.ConsolAWBHeader), new FHLMessageDetails(dataCreator.ShipmentAWBHeader), FHL.Version.No4).ToString();
				AssertMatchingPrefix(message, "/US/DNR/D/", "UN1234", "UN2345");
			}
		}

		public void TestItalyAirExport_OCI_Contains_MRN_CTStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var testMRN = "0001234567";
				var testCTStatus = "X";
				var hawbHeader = Factory.New<ShipmentExportAWBHeader>();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITROM";
				shipment.JS_RL_NKDestination = "AUSYD";

				shipment.CustomsEntryNumberType = "MRN";
				shipment.CustomsEntryNumberForBinding = testMRN;
				shipment.JS_CommunityTransitStatus = testCTStatus;

				hawbHeader.EH_ParentID = shipment.PK;

				var mawbHeader = Factory.New<ConsolExportAWBHeader>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				mawbHeader.EH_ParentID = consol.PK;
				mawbHeader.Populate();

				mawbHeader.Consol.JK_AgentType = "AGT";
				mawbHeader.Populate();
				hawbHeader.Populate();

				var fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
				var fhlDetailsProvider = new FHLMessageDetails(hawbHeader);

				var expected = @$"OCI/IT/EXP/M/{testMRN}
//COR//{testCTStatus}";
				var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertContains(expected, fhlMessage);
			}
		}

		public void TestItalyAirExport_OCI_No_MRN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var testCTStatus = "X";
				var hawbHeader = Factory.New<ShipmentExportAWBHeader>();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITROM";
				shipment.JS_RL_NKDestination = "AUSYD";

				shipment.JS_CommunityTransitStatus = testCTStatus;

				hawbHeader.EH_ParentID = shipment.PK;

				var mawbHeader = Factory.New<ConsolExportAWBHeader>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				mawbHeader.EH_ParentID = consol.PK;
				mawbHeader.Populate();

				mawbHeader.Consol.JK_AgentType = "AGT";
				mawbHeader.Populate();
				hawbHeader.Populate();

				var fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
				var fhlDetailsProvider = new FHLMessageDetails(hawbHeader);

				var unexpected = @$"OCI/IT/EXP/M/";
				var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertNotContains(unexpected, fhlMessage);
			}
		}

		public void TestItalyAirExport_OCI_Contains_MRN_No_CTStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var testMRN = "0001234567";
				var hawbHeader = Factory.New<ShipmentExportAWBHeader>();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITROM";
				shipment.JS_RL_NKDestination = "AUSYD";

				shipment.CustomsEntryNumberType = "MRN";
				shipment.CustomsEntryNumberForBinding = testMRN;

				hawbHeader.EH_ParentID = shipment.PK;

				var mawbHeader = Factory.New<ConsolExportAWBHeader>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				mawbHeader.EH_ParentID = consol.PK;
				mawbHeader.Populate();

				mawbHeader.Consol.JK_AgentType = "AGT";
				mawbHeader.Populate();
				hawbHeader.Populate();

				var fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
				var fhlDetailsProvider = new FHLMessageDetails(hawbHeader);

				var expected = @$"OCI/IT/EXP/M/{testMRN}";
				var unexpected = "//COR//";
				var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertContains(expected, fhlMessage);
				AssertNotContains(unexpected, fhlMessage);
			}
		}

		public void TestItalyAirExport_OCI_ExcessMaxLength()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var testCTStatus = "X";
				var testMRN = "01234567890";
				var hawbHeader = Factory.New<ShipmentExportAWBHeader>();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITROM";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_CommunityTransitStatus = testCTStatus;

				for (var i = 0; i < 46; i++)
				{
					CusEntryNumber cusEntryNum = shipment.CusEntryNumbers.AddNew();
					cusEntryNum.CE_EntryNum = testMRN + $"{i}";
					cusEntryNum.CE_EntryType = "MRN";
				}

				hawbHeader.EH_ParentID = shipment.PK;

				var mawbHeader = Factory.New<ConsolExportAWBHeader>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				mawbHeader.EH_ParentID = consol.PK;
				mawbHeader.Populate();

				mawbHeader.Consol.JK_AgentType = "AGT";
				mawbHeader.Populate();
				hawbHeader.Populate();

				var fwbDetailsProvider = new FWBMessageDetails(mawbHeader);
				var fhlDetailsProvider = new FHLMessageDetails(hawbHeader);

				var expected = @$"/IT/EXP/M/{testMRN}44
//COR//{testCTStatus}";
				var unexpected = @$"/IT/EXP/M/{testMRN}45";
				var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertContains(expected, fhlMessage);
				AssertNotContains(unexpected, fhlMessage);
			}
		}
		
		#region Implementation

		class ShipmentExportAWBHeaderTest : ShipmentExportAWBHeader
		{
			public ShipmentExportAWBHeaderTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString EH_ConsigneeContactEmail { get; set; }

			public override ZString EH_ShipperContactEmail { get; set; }

			public override RefCountry DestinationCountry => DestinationCountryForTest;
			public RefCountry DestinationCountryForTest { get; set; }

			public override RefCountry OriginCountry => OriginCountryForTest;
			public RefCountry OriginCountryForTest { get; set; }

			public override bool IsImportToChina => IsImportToChinaForTest;
			public bool IsImportToChinaForTest { get; set; }

			public override bool IsTransitingThroughChina => IsTransitingThroughChinaForTest;
			public bool IsTransitingThroughChinaForTest { get; set; }
		}

		protected override string Expected
		{
			get { return string.Format(ExpectedFormatString, new object[] { VNO, MBI, HBS, TXT, SHP, CNE, CVD }); }
		}

		protected override CargoIMP CargoIMPMessage
		{
			get { return new FHL(new FWBMessageDetails(CreateTemplateAWBHeader()), new FHLMessageDetails(CreateTemplateShipmentAWBHeader()), FHL.Version.No2); }
		}

		ShipmentExportAWBHeader CreateTemplateShipmentAWBHeader()
		{
			ShipmentExportAWBHeader aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			aWBHeader.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			aWBHeader.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			aWBHeader.Shipment.JS_HouseBill = "AEI12345678";
			aWBHeader.Shipment.JS_RL_NKOrigin = "SGSIN";
			aWBHeader.Shipment.JS_RL_NKDestination = "AUSYD";
			aWBHeader.Shipment.JS_OuterPacks = 4;
			aWBHeader.Shipment.JS_ActualWeight = 400.00M;
			aWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			aWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "computer parts these computer parts are the short description for memory chips and other parts like sound cards and usb ports and whatever other parts come to mind like cables and keyboards and mouses and the mother of all boards I think that there is enough here for a simple test";
			aWBHeader.Populate();
			aWBHeader.Shipment.JS_OverrideWaybillDefaults = ZBool.True;

			aWBHeader.EH_ShipperAccount = "BRECPT";
			aWBHeader.EH_ShipperName = "Mr Shipper";
			aWBHeader.EH_ShipperAddress = "Shipper Address";
			aWBHeader.EH_ShipperPlace = "Place";
			aWBHeader.EH_ShipperState = "State";
			aWBHeader.EH_ShipperCountryCode = "SG";
			aWBHeader.EH_ShipperPostCode = "1234";
			aWBHeader.EH_ShipperContactCode = "FAX";
			aWBHeader.EH_ShipperContactDetail = "+1 (23456) (78)";

			aWBHeader.EH_ConsigneeAccount = "BREUSA";
			aWBHeader.EH_ConsigneeName = "Mr Consignee";
			aWBHeader.EH_ConsigneeAddress = "Consignee Address";
			aWBHeader.EH_ConsigneePlace = "C Place";
			aWBHeader.EH_ConsigneeState = "C State";
			aWBHeader.EH_ConsigneeCountryCode = "AU";
			aWBHeader.EH_ConsigneePostCode = "4321";
			aWBHeader.EH_ConsigneeContactCode = "TLX";
			aWBHeader.EH_ConsigneeContactDetail = "+9 (876) 54321";

			aWBHeader.EH_Currency = "ZAR";
			aWBHeader.EH_ChargesCode = "TE";
			aWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			aWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			aWBHeader.EH_DeclaredValue = 123.54M;
			aWBHeader.EH_CustomsValue = 0M;
			aWBHeader.EH_InsuranceValue = 0M;

			aWBHeader.EH_ShippingLoadAndCount = 7;

			VNO = "2";
			HBS = "HBS/AEI12345678/SINSYD/4/K400/COMPUTER PARTS\r\n";
			TXT = "TXT/COMPUTER PARTS THESE COMPUTER PARTS ARE THE SHORT DESCRIPTION FOR\r\n/ MEMORY CHIPS AND OTHER PARTS LIKE SOUND CARDS AND USB PORTS AND \r\n/WHATEVER OTHER PARTS COME TO MIND LIKE CABLES AND KEYBOARDS AND M\r\n/OUSES AND THE MOTHER OF ALL BOARDS I THINK THAT THERE IS ENOUGH H\r\n/ERE FOR A SIMPLE TEST\r\n";
			SHP = "SHP/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/1234/FAX/12345678\r\n";
			CNE = "CNE/MR CONSIGNEE\r\n/CONSIGNEE ADDRESS\r\n/C PLACE/C STATE\r\n/AU/4321/TLX/987654321\r\n";
			CVD = "CVD/ZAR/CP/123.54/NCV/XXX\r\n";

			return aWBHeader;
		}

		protected override Forwarding.AWB.Business.ExportAWBHeader CreateTemplateAWBHeader()
		{
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			aWBHeader.EH_ParentID = consol.PK;

			aWBHeader.Consol.JK_OverrideWaybillDefaults = ZBool.True;
			aWBHeader.Consol.JK_MasterBillNum = "61812345675";
			aWBHeader.Populate();

			aWBHeader.EH_AWBOriginCode = "SIN";
			aWBHeader.EH_AirportOfDestinationCode = "JFK";
			aWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "10";
			aWBHeader.AWBRateLines[0].ER_GrossWeight = 1000M;
			aWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;

			aWBHeader.EH_ShippingLoadAndCount = 5;

			MBI = "MBI/618-12345675SINJFK/T10K1000\r\n";

			return aWBHeader;
		}

		protected override Forwarding.AWB.Business.ExportAWBHeader GetNewFWBHeader() => Factory.New<ConsolExportAWBHeader>();

		#region Expected Values

		string VNO = "";
		string MBI = "";
		string HBS = "";
		string TXT = "";
		string SHP = "";
		string CNE = "";
		string CVD = "";
		const string ExpectedFormatString =
			"FHL/{0}\r\n" + //VNO
			"{1}" + //MBI
			"{2}" + //HBS
			"{3}" + //TXT
			"{4}" + //SHP
			"{5}" + //CNE
			"{6}";//CVD

		#endregion

		#endregion
	}
}
