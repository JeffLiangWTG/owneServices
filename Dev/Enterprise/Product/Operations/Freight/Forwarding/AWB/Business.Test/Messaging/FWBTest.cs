using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class FWBTest : TestCaseWithFactory
	{
		public void TestAddChargeSumary_Prepaid()
		{
			using (ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedSegment = "PPD\r\n/CT0";

				var header = Factory.New<ExportAWBHeader>();
				header.EH_WeightPrepaidCollect = Constants.AWB.PPDCollect.Prepaid;
				var detailsProvider = new FWBMessageDetails(header);

				var actualText = new FWB(detailsProvider, FWB.Version.No16).ToString();
				AssertContains("FWB Message should contain the segment of Prepaid", expectedSegment, actualText);

				header.EH_WeightPrepaidCollect = Constants.AWB.PPDCollect.Collect;

				actualText = new FWB(detailsProvider, FWB.Version.No16).ToString();
				AssertNotContains("Should not contain the segment as the header is not Prepaid", expectedSegment, actualText);

				header.EH_WeightPrepaidCollect = Constants.AWB.PPDCollect.Prepaid;

				actualText = new FWB(detailsProvider, FWB.Version.No10).ToString();
				AssertNotContains("Should not contain the segment as the FWB version is not No16", expectedSegment, actualText);

				using (ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					detailsProvider = new FWBMessageDetails(header);
					actualText = new FWB(detailsProvider, FWB.Version.No10).ToString();
					AssertContains("ShouldSa contain the segment as the registry item's value is true", expectedSegment, actualText);
				}
			}
		}

		public void TestAddChargeSumary_Collect()
		{
			using (ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedSegment = "COL\r\n/CT0";

				var header = Factory.New<ExportAWBHeader>();
				header.EH_WeightPrepaidCollect = Constants.AWB.PPDCollect.Collect;
				var detailsProvider = new FWBMessageDetails(header);

				var actualText = new FWB(detailsProvider, FWB.Version.No16).ToString();
				AssertContains("FWB Message should contain the segment of Collect", expectedSegment, actualText);

				header.EH_WeightPrepaidCollect = Constants.AWB.PPDCollect.Prepaid;

				actualText = new FWB(detailsProvider, FWB.Version.No16).ToString();
				AssertNotContains("Should not contain the segment as the header is not Collect", expectedSegment, actualText);

				header.EH_WeightPrepaidCollect = Constants.AWB.PPDCollect.Collect;

				actualText = new FWB(detailsProvider, FWB.Version.No10).ToString();
				AssertNotContains("Should not contain the segment as the FWB version is not No16", expectedSegment, actualText);

				using (ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					detailsProvider = new FWBMessageDetails(header);
					actualText = new FWB(detailsProvider, FWB.Version.No10).ToString();
					AssertContains("Should contain the segment as the registry item's value is true", expectedSegment, actualText);
				}
			}
		}

		public void TestCTIsAlwaysShown()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			var detailsProvider = new FWBMessageDetails(awbHeader);
			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("PPD\r\n/CT0", fwbMessage);
		}

		public void TestDimensionDetails()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.AWBRateLines[0].ER_RateClass = Constants.AWB.RateClass.QuantityRate;
			awbHeader.AWBRateLines[0].ER_CommodityItemNumber = "1";
			awbHeader.AWBRateLines[1].ER_RateClass = Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation;
			awbHeader.AWBRateLines[1].ER_CommodityItemNumber = "2";
			var detailsProvider = new FWBMessageDetails(awbHeader);
			string expectedDimensionsLine = "/3/ND//NDA";

			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertNotContains("ULD rate line on AWB. ND//NDA shouldn't print", expectedDimensionsLine, fwbMessage);

			awbHeader.AWBRateLines[1].ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;

			fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("No ULD rate line on AWB. ND//NDA should print", expectedDimensionsLine, fwbMessage);

			fwbMessage = new FWB(detailsProvider, FWB.Version.No10).ToString();
			AssertNotContains("Dimension info should not appear on version 10.", expectedDimensionsLine, fwbMessage);
		}

		public void TestBasicFWB16()
		{
			string expectedFWB16 = AWBTestDataCreator.MAWBSampleFWB;

			var dataCreator = new AWBTestDataCreator(Factory);
			var detailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertMultilineASCIIEquals("FWB Message as Version 16", expectedFWB16.Trim(), actualFWB16.Trim());
		}

		public void TestSendFWBWithoutAgentDetailsSegment()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.MAWBHeader.EH_AgentIATACodeFormatted = ZString.Empty;
			var detailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertNotContains("FWB Message should not have AGT Segment", "AGT/", actualFWB16);

			dataCreator.MAWBHeader.EH_AgentIATACodeFormatted = "1234567";

			actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertContains("FWB Message should now have AGT Segment", "AGT/", actualFWB16);
		}

		public void TestEH_WeightVPPDCOLValid()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_WeightVPPDCOL = ZString.Empty;
			ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var detailsProvider = new FWBMessageDetails(awbHeader);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			Assert(actualFWB16.Contains(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid));

			ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			Assert(actualFWB16.Contains(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid));
		}

		public void TestREFCityCode()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var detailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USRTT";
			GlbBranch.CurrentBranch.HomePort.RL_IATA = "RTT";
			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			Assert("FWB message should contain 'REF//MWB00696667465/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/RTT'",
				actualFWB16.Contains(@"REF//MWB00696667465/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/RTT"));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USIA9";
			GlbBranch.CurrentBranch.HomePort.RL_IATA = "IAH";
			actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			Assert("FWB message should contain 'REF//MWB00696667465/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/IAH'",
				actualFWB16.Contains(@"REF//MWB00696667465/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/IAH"));
		}

		public void TestDimensions()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var header = dataCreator.MAWBHeader;
			var dimensionsRateLine = header.AWBRateLines[2];
			dimensionsRateLine.ER_NatureAndQtyOfGoodsType = "D";
			var detailsProvider = new FWBMessageDetails(header);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("FWB Message should now have Dimensions", "4/ND//INH13-12-15/2", actualFWB16);
		}

		public void TestVolume()
		{
			var dataCreator = new AWBTestDataCreator(Factory);
			var header = dataCreator.MAWBHeader;
			var volumeRateLine = header.AWBRateLines[2];
			volumeRateLine.ER_NatureAndQtyOfGoodsType = "V";
			volumeRateLine.NatureAndQtyOfGoods.Text = "VOL 21.98 L";
			var detailsProvider = new FWBMessageDetails(header);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("FWB Message should now have Volume in cm3", "4/NV/CC21980", actualFWB16);

			volumeRateLine.NatureAndQtyOfGoods.Text = "VOL 100000.13 L";
			actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("FWB Message should now have fallback on M3", "4/NV/MC100.00013", actualFWB16);
		}

		public void TestNatureAndQtyOfGoodsText()
		{
			var header = Factory.New<ExportAWBHeader>();

			header.AWBRateLines[4].ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine1.NatureAndQtyOfGoods.Text = "                ***AGO***          ";
			header.AWBRateLine2.NatureAndQtyOfGoods.Text = "#$%%%^*&^$";
			header.AWBRateLine3.NatureAndQtyOfGoods.Text = "   ****MUST&&GO AS BOOKED****    ";
			header.AWBRateLine4.NatureAndQtyOfGoods.Text = "  ";

			var detailsProvider = new FWBMessageDetails(header);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertMultilineASCIIEquals("FWB Message should have AGO",
				@"FWB/16
-/T00
RTG/
SHP
/
/
/
/
CNE
/
/
/
/
CVD///CC/NVD/NCV/XXX
RTD/1/NG/AGO MUST  GO AS BOOK
/2/NG/ED
/3/ND//NDA
PPD
/CT0
ISU//
REF//MWB/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/BNE", actualFWB16);
		}

		public void TestNatureAndQtyOfGoodsText_SkipOnlyInvalidCharsLine()
		{
			var header = Factory.New<ExportAWBHeader>();

			header.AWBRateLines[0].ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLines[1].ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLines[2].ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine1.NatureAndQtyOfGoods.Text = "Consolidation as per";
			header.AWBRateLine2.NatureAndQtyOfGoods.Text = "=========";
			header.AWBRateLine3.NatureAndQtyOfGoods.Text = "7/04/2015 4:37:12 AM";

			var detailsProvider = new FWBMessageDetails(header);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertMultilineASCIIEquals("FWB Message should skip lines which has only invalid chars",
				@"FWB/16
-/T00
RTG/
SHP
/
/
/
/
CNE
/
/
/
/
CVD///CC/NVD/NCV/XXX
RTD/1/NG/CONSOLIDATION AS PER
/2/NG/7 04 2015 4 37 12 AM
/3/ND//NDA
PPD
/CT0
ISU//
REF//MWB/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/BNE", actualFWB16);
		}

		public void TestNatureAndQtyOfGoodsText_LithiumBatteryDetailsToAWBMessage()
		{
			var header = Factory.New<ExportAWBHeader>();

			header.AWBRateLines[0].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLines[1].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			header.AWBRateLines[2].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLines[3].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLines[4].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLines[5].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLines[6].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLines[7].ER_NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;

			header.AWBRateLine1.NatureAndQtyOfGoods.Text = "Consolidation as per attached list";
			header.AWBRateLine2.NatureAndQtyOfGoodsVolume.Volume = 6;
			header.AWBRateLine2.NatureAndQtyOfGoodsVolume.Unit = Constants.Volume.CubicMetres;
			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI965;
			header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI967;
			header.AWBRateLine7.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI970;

			var detailsProvider = new FWBMessageDetails(header);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertMultilineASCIIEquals("FWB Message should show Lithium Battery details in the NG segment",
				@"FWB/16
-/T00
RTG/
SHP
/
/
/
/
CNE
/
/
/
/
CVD///CC/NVD/NCV/XXX
RTD/1/NG/CONSOLIDATION AS PER
/2/NG/ATTACHED LIST
/3/NV/MC6
/4/NG/LI BATT PI965
/5/NG/LI BATT PI967
/6/NG/LI BATT PI970
PPD
/CT0
ISU//
REF//MWB/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/BNE", actualFWB16);
		}

		public void TestNDA()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.AWBRateLines[0].ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			awbHeader.AWBRateLines[0].ER_CommodityItemNumber = "1";
			var detailsProvider = new FWBMessageDetails(awbHeader);

			const string expectedDimensionsLine = "/2/ND//NDA";

			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("no dims and no volume - expected NDA", expectedDimensionsLine, fwbMessage);

			awbHeader.AWBRateLines[0].ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsDimensions.Text = "VOL 1.234 M3";

			fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertNotContains("has vol - no NDA expected", expectedDimensionsLine, fwbMessage);

			awbHeader.AWBRateLines[0].ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsDimensions.Text = "DIMS 1x2x3 FT x 4";

			fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertNotContains("has dims - no NDA expected", expectedDimensionsLine, fwbMessage);

			awbHeader.AWBRateLines[0].ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("no dims and no volume - expected NDA", expectedDimensionsLine, fwbMessage);
		}

		[ExpectNoExceptions]
		public void TestOtherServiceInformation_Wrapping()
		{
			#region Setup

			var awbHeader = Factory.New<ExportAWBHeader>();

			const string sixtyFiveChars = "SIXTY-FIVE-CHARS.................................SIXTY-FIVE-CHARS";
			const string expectSixtyFiveChars = "OSI/SIXTY-FIVE-CHARS.................................SIXTY-FIVE-CHARS\r\n";

			const string sixtySixChars = "SIXTY-SIX-CHARS....................................SIXTY-SIX-CHARS";
			const string expectSixtySixChars = "OSI/SIXTY-SIX-CHARS....................................SIXTY-SIX-CHAR\r\n" +
													  "/S";

			const string oneThirtyChars = "ONE-THIRTY-CHARS..................................................................................................ONE-THIRTY-CHARS";
			const string expectOneThirtyChars = "OSI/ONE-THIRTY-CHARS.................................................\r\n" +
													  "/.................................................ONE-THIRTY-CHARS\r\n";

			const string oneThirtyOneChars = "ONE-THIRTY-ONE-CHARS...........................................................................................ONE-THIRTY-ONE-CHARS";
			const string expectOneThirtyOneChars = "OSI/ONE-THIRTY-ONE-CHARS.............................................\r\n" +
													  "/..............................................ONE-THIRTY-ONE-CHAR\r\n" +
													  "/S";

			const string oneNinetyFiveChars = "ONE-NINETY-FIVE-CHARS.........................................................................................................................................................ONE-NINETY-FIVE-CHARS";
			const string expectOneNinetyFiveChars = "OSI/ONE-NINETY-FIVE-CHARS............................................\r\n" +
													   "/.................................................................\r\n" +
													   "/............................................ONE-NINETY-FIVE-CHARS\r\n";

			const string oneNinetySixChars = "ONE-NINETY-SIX-CHARS............................................................................................................................................................ONE-NINETY-SIX-CHARS";

			#endregion

			var detailsProvider = new FWBMessageDetails(awbHeader);

			awbHeader.EH_HandlingInformation = sixtyFiveChars;
			AssertContains("Expected 65 chars to not wrap", expectSixtyFiveChars, new FWB(detailsProvider, FWB.Version.No16).ToString());

			awbHeader.EH_HandlingInformation = sixtySixChars;
			AssertContains("Expected 66 chars to wrap one char", expectSixtySixChars, new FWB(detailsProvider, FWB.Version.No16).ToString());

			awbHeader.EH_HandlingInformation = oneThirtyChars;
			AssertContains("Expected 130 chars to wrap one full line", expectOneThirtyChars, new FWB(detailsProvider, FWB.Version.No16).ToString());

			awbHeader.EH_HandlingInformation = oneThirtyOneChars;
			AssertContains("Expected 131 chars to wrap one full line + one char", expectOneThirtyOneChars, new FWB(detailsProvider, FWB.Version.No16).ToString());

			awbHeader.EH_HandlingInformation = oneNinetyFiveChars;
			AssertContains("Expected 195 chars to wrap two full lines", expectOneNinetyFiveChars, new FWB(detailsProvider, FWB.Version.No16).ToString());

			AssertExceptionThrown("More than 195 chars is not supported in Db", typeof(MaxLengthExceededException), () => awbHeader.EH_HandlingInformation = oneNinetySixChars);
			try
			{
				awbHeader.EH_HandlingInformation = oneNinetySixChars;
				Fail("More than 195 chars is not supported in Db");
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				Assert("Exception was epected", true);
			}
		}

		public void TestOtherServiceInformation_InvalidLine()
		{
			#region Setup

			var awbHeader = Factory.New<ExportAWBHeader>();

			const string invalidSecondLine = "PLEASE NOTIFY CONSIGNEE IMMEDIATLEY UPON ARRIVAL\r\n**KNOWN SHIPPER**";
			const string expectedIncludingOnlyOneLine = "OSI/PLEASE NOTIFY CONSIGNEE IMMEDIATLEY UPON ARRIVAL    KNOWN SHIPPER\r\n";
			const string expectedNotIncludingSecondLine = "OSI/PLEASE NOTIFY CONSIGNEE IMMEDIATLEY UPON ARRIVAL    KNOWN SHIPPER\r\n/  \r\n";

			const string allInvalidLine = "****** ****** ********* *********** **** *******\r\n******* ***********************";
			const string expectedNotIncludingOSI = "OSI";

			#endregion

			var detailsProvider = new FWBMessageDetails(awbHeader);

			awbHeader.EH_HandlingInformation = invalidSecondLine;
			AssertContains("Expected including only one line", expectedIncludingOnlyOneLine, new FWB(detailsProvider, FWB.Version.No16).ToString());
			AssertNotContains("Expected not including second empty line", expectedNotIncludingSecondLine, new FWB(detailsProvider, FWB.Version.No16).ToString());

			awbHeader.EH_HandlingInformation = allInvalidLine;
			AssertNotContains("Expected not including other service information", expectedNotIncludingOSI, new FWB(detailsProvider, FWB.Version.No16).ToString());
		}

		public void TestAddAccountingInformationSkipsCorrectRecords()
		{
			var dataCreator = new AWBTestDataCreator(Factory);

			var nonSkippableInformation = Factory.New<ExportAWBAccountingInformation>();
			nonSkippableInformation.EA_InformationID = "GEN";
			nonSkippableInformation.EA_Information = "NON VALE A FINI IVA";
			dataCreator.MAWBHeader.AWBAccountingInformations.Add(nonSkippableInformation);

			var skippableInformation = Factory.New<SkippableExportAWBAccountingInformation>();
			skippableInformation.EA_InformationID = "SIV";
			skippableInformation.EA_Information = "11803680153";
			dataCreator.MAWBHeader.AWBAccountingInformations.Add(skippableInformation);

			var detailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("GEN Record should not have been skipped", "GEN/NON VALE A FINI IVA", actualFWB16);
			AssertNotContains("SIV Record should have been skipped", "SIV/11803680153", actualFWB16);
		}

		public void TestAccountingInformationFormat()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			var accountingInfo = awbHeader.AWBAccountingInformations.AddNew();
			accountingInfo.EA_Information = "!@#$%";

			var detailsProvider = new FWBMessageDetails(awbHeader);

			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertNotContains("AccountingInfo can only be Text", "/GEN/     ", fwbMessage);
		}

		public void TestPartyAccountFormat()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_ShipperAccount = "!@#$%";

			var detailsProvider = new FWBMessageDetails(awbHeader);

			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertNotContains("Party account can only be Text", "SHP/     ", fwbMessage);
		}

		public void TestPartyContactDetailsFormat()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_ShipperContactCode = "$%^";
			awbHeader.EH_ShipperContactDetail = "!@#$";

			var detailsProvider = new FWBMessageDetails(awbHeader);

			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertNotContains("Party contact information can only be AlphaNumeric", "/   /   ", fwbMessage);
		}

		public void TestPartyContactDetailsPhoneNumberStartsWithPlus()
		{
			var awbHeader = Factory.New<ExportAWBHeader>();
			awbHeader.EH_ShipperContactCode = "TE";
			awbHeader.EH_ShipperContactDetail = "+123456789";

			var detailsProvider = new FWBMessageDetails(awbHeader);

			string fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertContains("Party contact information can only be AlphaNumeric", "OCI//SHP/CT/123456789", fwbMessage);
		}

		#region Customs Entry Numbers

		public void TestCustomsEntryNumbers_NoNumbers()
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();
			var detailsProvider = new FWBMessageDetails(awbHeader);
			var fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertNotContains("ACC segment is not included", "ACC/", fwbMessage);
		}

		public void TestCustomsEntryNumbers_MultipleNumbers()
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();

			awbHeader.CustomsEntryNumbersExposed = new List<EntryNumber>
			{
				new EntryNumber
				{
					Type = CusEntryNumberTypes.Standard.MovementReferenceNumber,
					Number = "1111"
				},
				new EntryNumber
				{
					Type = CusEntryNumberTypes.Standard.ClearancePermitNumber,
					Number = "2222"
				},
				new EntryNumber
				{
					Type = CusEntryNumberTypes.Standard.UniqueConsignementReference,
					Number = "3333"
				},
				new EntryNumber()
			};
			var detailsProvider = new FWBMessageDetails(awbHeader);

			var fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertContains("ACC contains all numbers except MRN",
@"ACC/GEN/PMT 2222
/GEN/UCR 3333
",
			fwbMessage);
		}

		#endregion

		#region ACID Numbers

		public void TestAcidNumbers_MultipleNumbers()
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();
			awbHeader.EH_HandlingInformation = "ACID Number:6AU123456789D0VAHK11N,6AU123456789D0VAHK11O\r\n";
			awbHeader.EH_ConsigneeCountryCode = Constants.CountryCodes.Egypt;

			var detailsProvider = new FWBMessageDetails(awbHeader);

			var fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertSegment(awbHeader, FWB.Version.No16, "OCI",
				@"OCI/EG/IMP/M/6AU123456789D0VAHK11N
/EG/IMP/M/6AU123456789D0VAHK11O");
		}

		public void TestAcidNumbers_SingleNumber()
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();
			awbHeader.EH_HandlingInformation = "ACID Number:6AU123456789D0VAHK11N\r\n";
			awbHeader.EH_ConsigneeCountryCode = Constants.CountryCodes.Egypt;

			var detailsProvider = new FWBMessageDetails(awbHeader);

			var fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertSegment(awbHeader, FWB.Version.No16, "OCI",
				@"OCI/EG/IMP/M/6AU123456789D0VAHK11N");
		}

		#endregion

		#region Goods Declaration Reference Number

		public void TestGoodsDeclarationReferenceNumbers_MultipleNumbers_Version10()
		{
			AssertGoodsDeclarationReferenceNumbers_MultipleNumbers(string.Empty, FWB.Version.No10);
		}

		public void TestGoodsDeclarationReferenceNumbers_MultipleNumbers_Version16()
		{
			const string expected =
@"OCI/CH/EXP/M/13CH9876AB88901235
/CH/EXP/M/13CH9876AB88901236
/CH/IMP/M/14CH45612354752
";

			AssertGoodsDeclarationReferenceNumbers_MultipleNumbers(expected, FWB.Version.No16);
		}

		void AssertGoodsDeclarationReferenceNumbers_MultipleNumbers(string expected, FWB.Version version)
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();

			awbHeader.GoodsDeclarationReferenceNumbersExposed = new List<GoodsDeclarationReferenceNumber>
			{
				new GoodsDeclarationReferenceNumber
				{
					Numbers = new List<ZString>() { "13CH9876ab88901235", "13CH9876ab88901236" },
					CountryOfIssue = "CH",
					MovementCode = MovementReferenceCode.Codes.CustomsExport
				},
				new GoodsDeclarationReferenceNumber
				{
					Numbers = new List<ZString>() { "14CH45612354752" },
					CountryOfIssue = "CH",
					MovementCode = MovementReferenceCode.Codes.CustomsImport,
				},
				new GoodsDeclarationReferenceNumber()
			};

			AssertSegment(awbHeader, version, "OCI", expected);
		}

		#endregion

		#region Movement Reference Numbers

		public void TestMovementReferenceNumbers_NoNumbers()
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();
			var detailsProvider = new FWBMessageDetails(awbHeader);
			var fwbMessage = new FWB(detailsProvider, FWB.Version.No16).ToString();

			AssertNotContains("ACC segment is not included", "OCI/", fwbMessage);
		}

		public void TestMovementReferenceNumbers_MultipleNumbers_Version10()
		{
			AssertMovementReferenceNumbers_MultipleNumbers(string.Empty, FWB.Version.No10);
		}

		public void TestMovementReferenceNumbers_MultipleNumbers_Version16()
		{
			const string expected =
@"OCI/IT/EXP/M/13IT9876AB88901235
/IT/EXP/M/13IT9876AB88901236
/PL/IMP/M/14PL45612354752
//HWB/I/HBL1111
/GB/TRA/M/15GB78888555448
//HWB/I/HBL222
//ULD/I/AAAA123456
//ULD/I/BBBB123456
//MAL/I/XXXXXX";

			AssertMovementReferenceNumbers_MultipleNumbers(expected, FWB.Version.No16);
		}

		void AssertMovementReferenceNumbers_MultipleNumbers(string expected, FWB.Version version)
		{
			var awbHeader = Factory.New<ExportAWBHeaderTest>();

			awbHeader.MovementReferenceNumbersExposed = new List<MovementReferenceNumber>();

			var mrn1 = new MovementReferenceNumber
			{
				CountryOfIssue = "IT",
				MovementCode = MovementReferenceCode.Codes.CustomsExport
			};
			mrn1.Numbers.AddRange(new List<ZString> { "13IT9876ab88901235", "13IT9876ab88901236" });
			awbHeader.MovementReferenceNumbersExposed.Add(mrn1);

			var mrn2 = new MovementReferenceNumber
			{
				CountryOfIssue = "PL",
				MovementCode = MovementReferenceCode.Codes.CustomsImport
			};
			mrn2.Numbers.Add("14PL45612354752");
			mrn2.RelatedNumbers.Add(new EntryNumber
			{
				Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
				Number = "HBL1111"
			});
			awbHeader.MovementReferenceNumbersExposed.Add(mrn2);

			var mrn3 = new MovementReferenceNumber
			{
				CountryOfIssue = "GB",
				MovementCode = MovementReferenceCode.Codes.CustomsTransit
			};
			mrn3.Numbers.Add("15GB78888555448");
			mrn3.RelatedNumbers.AddRange(new List<EntryNumber>
			{
				new EntryNumber
				{
					Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
					Number = "HBL222"
				},
				new EntryNumber
				{
					Type = RelatedMovementReferenceNumberType.Codes.ULDIdentifier,
					Number = "AAAA123456"
				},
				new EntryNumber
				{
					Type = RelatedMovementReferenceNumberType.Codes.ULDIdentifier,
					Number = "BBBB123456"
				},
				new EntryNumber
				{
					Type = RelatedMovementReferenceNumberType.Codes.MailReceptacleNumber,
					Number = "XXXXXX"
				}
			});
			awbHeader.MovementReferenceNumbersExposed.Add(mrn3);

			awbHeader.MovementReferenceNumbersExposed.Add(new MovementReferenceNumber());

			AssertSegment(awbHeader, version, "OCI", expected);
		}

		#endregion

		#region Dangerous Goods

		public void TestDangerousGoods_Version10()
		{
			AssertDangerousGoods(string.Empty, FWB.Version.No10);
		}

		public void TestDangerousGoods_Version16()
		{
			const string expected =
@"OCI/AU/DNR/D/AAAA
/AU/DNR/D/BBBB";

			AssertDangerousGoods(expected, FWB.Version.No16);
		}

		public void AssertDangerousGoods(string expected, FWB.Version version)
		{
			var header = Factory.New<ExportAWBHeaderTest>();

			header.DGCodesExposed = new List<ZString>
			{
				"AAAA",
				"BBBB"
			};

			AssertSegment(header, version, "OCI", expected);
		}

		#endregion

		#region Security Statement

		public void TestOCISegmentSecurityStatusLines()
		{
			var header = Factory.New<ExportAWBHeaderTest>();
			header.EH_RN_NKAgentApprovalCountryCode = "AU";
			header.EH_AgentApprovalNumber = "12345";
			header.EH_AgentApprovalExpiryDate = new ZDate(2019, 6, 1);

			var line1 = header.ExportAWBSecurityStatusLines.AddNew();
			line1.EAS_ApprovalCategory = "AC";
			line1.EAS_RN_NKCountryCode = "NZ";
			line1.EAS_ApprovalNumber = "22222";
			line1.EAS_ApprovalExpiryDate = new ZDate(2019, 7, 1);

			var line2 = header.ExportAWBSecurityStatusLines.AddNew();
			line2.EAS_ApprovalCategory = "RA";
			line2.EAS_RN_NKCountryCode = "SG";
			line2.EAS_ApprovalNumber = "33333";
			line2.EAS_ApprovalExpiryDate = new ZDate(2019, 8, 1);

			var line3 = header.ExportAWBSecurityStatusLines.AddNew();
			line3.EAS_ApprovalCategory = "KC";
			line3.EAS_RN_NKCountryCode = "HK";
			line3.EAS_ApprovalNumber = "44444";

			var expected = @"OCI/AU/ISS/RA/12345
///ED/0619
///AC/22222
/HK//KC/44444
///ED/1299
/SG/OSS/RA/33333
///ED/0819
///SN/
///SD/";
			AssertSegment(header, FWB.Version.No16, "OCI", expected, true, "Country code is included for RA, KC but not AC. Expiry date is included for RA, KC but not AC and defaults to 1299 if not supplied.");
		}

		public void TestOCISegmentSecurityStatusLines_ShouldAddOSSToMessage_WhenCategoryIsRegulatedAgent()
		{
			var header = Factory.New<ExportAWBHeaderTest>();
			header.EH_AgentApprovalNumber = "TEST123";

			var line = header.ExportAWBSecurityStatusLines.AddNew();
			line.EAS_ApprovalCategory = "RA";
			line.EAS_RN_NKCountryCode = "SG";
			line.EAS_ApprovalNumber = "33333";
			line.EAS_ApprovalExpiryDate = new ZDate(2019, 8, 1);

			var expected = @"OCI//ISS/RA/TEST123
///ED/1299
/SG/OSS/RA/33333
///ED/0819
///SN/
///SD/";
			AssertSegment(header, FWB.Version.No16, "OCI", expected, true);
		}

		#endregion

		public void TestZeroWeightRateLinesAreOmitted()
		{
			var header = Factory.New<ExportAWBHeader>();

			header.AWBRateLine1.ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine1.NatureAndQtyOfGoods.Text = "Consolidation as per";
			header.AWBRateLine2.ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine2.NatureAndQtyOfGoods.Text = "manifest";
			header.AWBRateLine3.ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine3.ER_WeightInLBsOrKGs = "K";
			header.AWBRateLine3.ER_GrossWeight = 0;
			header.AWBRateLine4.ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine4.ER_WeightInLBsOrKGs = "L";
			header.AWBRateLine4.ER_GrossWeight = 0;
			header.AWBRateLine5.ER_NatureAndQtyOfGoodsType = "G";
			header.AWBRateLine5.NatureAndQtyOfGoods.Text = "Goods description";
			header.AWBRateLine6.ER_NatureAndQtyOfGoodsType = "X";
			header.AWBRateLine6.NatureAndQtyOfGoods.Text = "ULD12345";
			header.AWBRateLine6.ER_RateClass = "U";
			header.AWBRateLine6.ER_GrossWeight = 0;
			header.AWBRateLine6.ER_WeightInLBsOrKGs = "K";
			header.AWBRateLine7.ER_NatureAndQtyOfGoodsType = "X";
			header.AWBRateLine7.NatureAndQtyOfGoods.Text = "ULD54321";
			header.AWBRateLine7.ER_RateClass = "U";
			header.AWBRateLine7.ER_GrossWeight = 10;
			header.AWBRateLine7.ER_WeightInLBsOrKGs = "K";

			var detailsProvider = new FWBMessageDetails(header);

			string actualFWB16 = new FWB(detailsProvider, FWB.Version.No16).ToString();
			AssertMultilineASCIIEquals("FWB Message should skip lines which have zero weight",
				@"FWB/16
-/T010
RTG/
SHP
/
/
/
/
CNE
/
/
/
/
CVD///CC/NVD/NCV/XXX
RTD/1/NG/CONSOLIDATION AS PER
/2/NG/MANIFEST
/3/NG/GOODS DESCRIPTION
/4/CU
/NX/ULD12345
/5/K10/CU
/NX/ULD54321
/6/ND//NDA
PPD
/CT0
ISU//
REF//MWB/FFW/CWIDEDIEDIDAT/BNE", actualFWB16);
		}

		public void TestFWBToString_TraderCodeShouldBePrefixedWithIssuingCountryCode()
		{
			var header = Factory.New<ExportAWBHeaderTest>();

			header.EH_ConsigneeTraderNo = "GB123";
			header.EH_ConsigneeTraderNoCountryCode = "GB";
			header.EH_ConsigneeTraderNoType = "EOR";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_ConsigneeTraderNoType = "EORI NO.";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_ShipperTraderNo = "GB123";
			header.EH_ShipperTraderNoCountryCode = "GB";
			header.EH_ShipperTraderNoType = "EOR";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_ShipperTraderNoType = "EORI NO.";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_AlsoNotifyTraderNo = "GB123";
			header.EH_AlsoNotifyTraderNoCountryCode = "GB";
			header.EH_AlsoNotifyTraderNoType = "EOR";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_AlsoNotifyTraderNoType = "EORI NO.";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/GB123", message);
		}

		public void TestFWB_ConsigneeTradeTradeNo_ForEFTA()
		{
			var header = Factory.New<ExportAWBHeaderTest>();

			header.EH_ConsigneeTraderNo = "1234567";
			header.EH_ConsigneeCountryCode = Constants.CountryCodes.Norway;
			header.EH_ConsigneeTraderNoCountryCode = Constants.CountryCodes.Norway;
			header.EH_ConsigneeTraderNoType = OrgCusCode.NorwayCodeTypes.MVA;
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/1234567", message);

			header.EH_ConsigneeTraderNo = "123456789";
			header.EH_ConsigneeCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_ConsigneeTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_ConsigneeTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/123456789", message);

			header.EH_ConsigneeTraderNo = "23456789";
			header.EH_ConsigneeCountryCode = Constants.CountryCodes.Germany;
			header.EH_ConsigneeTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_ConsigneeTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/23456789", message);
		}

		public void TestFWB_AlsoNotifyTradeTradeNo_ForEFTA()
		{
			var header = Factory.New<ExportAWBHeaderTest>();

			header.EH_AlsoNotifyTraderNo = "1234567";
			header.EH_AlsoNotifyCountryCode = Constants.CountryCodes.Norway;
			header.EH_AlsoNotifyTraderNoCountryCode = Constants.CountryCodes.Norway;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.NorwayCodeTypes.MVA;
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("NFY/T/1234567", message);

			header.EH_AlsoNotifyTraderNo = "123456789";
			header.EH_AlsoNotifyCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_AlsoNotifyTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("NFY/T/123456789", message);

			header.EH_AlsoNotifyTraderNo = "23456789";
			header.EH_AlsoNotifyCountryCode = Constants.CountryCodes.Germany;
			header.EH_AlsoNotifyTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("NFY/T/23456789", message);
		}

		#region Implementation

		void AssertSegment(ExportAWBHeader header, FWB.Version version, string segmentIdentifier, string expected, bool includeSecurityDeclaration = false, string message = "", int maxLength = -1)
		{
			var detailsProvider = new FWBMessageDetails(header);
			var fwbMessage = new FWB(detailsProvider, version, includeSecurityDeclaration).ToString();

			var locationOfOCI = fwbMessage.IndexOf(string.Concat(segmentIdentifier, "/"), StringComparison.InvariantCultureIgnoreCase);

			var actual = locationOfOCI > 0
				? fwbMessage.Substring(locationOfOCI, fwbMessage.Length - locationOfOCI - 1)
				: string.Empty;

			if (maxLength >= 0)
			{
				var actualTrimmed = actual.Trim('\r', '\n');
				AssertLessThanOrEqualTo($"Length of {segmentIdentifier} segment should not exceed {maxLength}.", actualTrimmed.Length, maxLength);
			}

			AssertMultilineASCIIEquals(string.Format("{0} segment. {1}", segmentIdentifier, message),
				expected, actual);
		}

		class ExportAWBHeaderTest : ExportAWBHeader
		{
			public ExportAWBHeaderTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public List<EntryNumber> CustomsEntryNumbersExposed { get; set; }

			protected override IEnumerable<EntryNumber> GetCustomsEntryNumbers()
			{
				return CustomsEntryNumbersExposed;
			}

			public List<MovementReferenceNumber> MovementReferenceNumbersExposed { get; set; }

			protected override IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbers()
			{
				return MovementReferenceNumbersExposed;
			}

			public List<GoodsDeclarationReferenceNumber> GoodsDeclarationReferenceNumbersExposed { get; set; }

			protected override IEnumerable<GoodsDeclarationReferenceNumber> GetGoodsDeclarationReferenceNumbers()
			{
				return GoodsDeclarationReferenceNumbersExposed;
			}

			public List<ZString> DGCodesExposed { get; set; }

			protected override IEnumerable<ZString> GetDGCodes()
			{
				return DGCodesExposed;
			}

			protected override IEnumerable<ZString> GetDGUNNOValues()
			{
				return DGCodesExposed;
			}

			public override ZString TSASecurityStatement
			{
				get { return tsaSecurityStatement; }
			}

			public void SetTSASecurityStatement(ZString statement)
			{
				tsaSecurityStatement = statement;
			}

			ZString tsaSecurityStatement;

			public override RefCountry DestinationCountry => DestinationCountryForTest;
			public RefCountry DestinationCountryForTest { get; set; }

			public override RefCountry OriginCountry => OriginCountryForTest;
			public RefCountry OriginCountryForTest { get; set; }

			public override ZString FreightForwarderOrCarrierCode => FreightForwarderOrCarrierCodeForTest;
			public ZString FreightForwarderOrCarrierCodeForTest { get; set; }

			public override ZString DestinationShipperComment => DestinationShipperCommentForTest;
			public ZString DestinationShipperCommentForTest { get; set; }
		}

		/// <summary>
		/// Dummy class to enable testing of skippable accounting information records
		/// </summary>
		internal class SkippableExportAWBAccountingInformation : ExportAWBAccountingInformation
		{
			public SkippableExportAWBAccountingInformation(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool IsSkippedOnMessaging
			{
				get { return true; }
			}
		}

		#endregion
	}
}
