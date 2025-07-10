using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class FHLTest : TestCaseWithFactory
	{
		public void TestBasicFHL4()
		{
			const string expectedFHL4 = AWBTestDataCreator.HAWBSampleFHL1;

			var dataCreator = new AWBTestDataCreator(Factory);
			dataCreator.SetupHAWBHeader1();

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(dataCreator.HAWBHeader1);

			var actualFHL4 = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertMultilineASCIIEquals("FWB Message as Version 16", expectedFHL4.Trim(), actualFHL4.Trim());
		}

		public void TestNatureOfGoods()
		{
			var defaultAllowShortGoodsDescriptionOverrideforFHL = Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL;
			try
			{
				var dataCreator = new AWBTestDataCreator(Factory);
				dataCreator.SetupHAWBHeader1();
				dataCreator.HAWBHeader1.EH_ManifestDescriptionOfGoods = "Test Short Decr";

				var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
				var fhlDetailsProvider = new FHLMessageDetails(dataCreator.HAWBHeader1);

				Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = false;
				var outString1 = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertMultilineASCIIEquals("NOT populate EH_ManifestDescriptionOfGoods when registry is NOT enabled", AWBTestDataCreator.HAWBSampleFHL1.Trim(), outString1.Trim());

				Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = true;
				var outString2 = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertMultilineASCIIEquals("Populate EH_ManifestDescriptionOfGoods when registry is enabled", AWBTestDataCreator.HAWBSampleFHLForNatureOfGoods.Trim(), outString2.Trim());

				dataCreator.HAWBHeader1.EH_ManifestDescriptionOfGoods = string.Empty;
				var outString3 = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
				AssertMultilineASCIIEquals("NOT populate EH_ManifestDescriptionOfGoods when empty", AWBTestDataCreator.HAWBSampleFHL1.Trim(), outString3.Trim());
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = defaultAllowShortGoodsDescriptionOverrideforFHL;
			}
		}

		public void TestHarmonisedCodes()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.AvailableHarmonisedCodes = new StringCollectionX(new[] { "1234", "22222", "333888", "444444", "555555", "666666", "777777", "888888", "999999", "101010" });

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(dataCreator.HAWBHeader1);

			var outString = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertMultilineASCIIEquals("Max of 9 HC codes are included. Codes are padded to 6 digits.", AWBTestDataCreator.HAWBSampleFHLForHarmonisedCodes.Trim(), outString.Trim());

			header.AvailableHarmonisedCodes = new StringCollectionX();

			outString = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertMultilineASCIIEquals("HC Section is omitted.", AWBTestDataCreator.HAWBSampleFHL1.Trim(), outString.Trim());
		}

		public void TestFreeTextDescriptionOfGoods()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.DetailedGoodsDescription = string.Empty;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(dataCreator.HAWBHeader1);

			var outString = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertMultilineASCIIEquals(" Use NatureAndQtyOfGoods For TXT if DetailedGoodsDescription is empty", AWBTestDataCreator.HAWBSampleFHL1.Trim(), outString.Trim());
		}

		#region ACID Numbers

		public void TestAcidNumbers_MultipleNumbers()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_HandlingInformation = "ACID Number:6AU123456789D0VAHK11N,6AU123456789D0VAHK11O\r\n";
			header.EH_ConsigneeCountryCode = Core.Constants.CountryCodes.Egypt;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			const string expected = @"/EG/IMP/M/6AU123456789D0VAHK11N
/EG/IMP/M/6AU123456789D0VAHK11O";
			AssertContains(expected, fhlMessage);
		}

		public void TestAcidNumbers_SingleNumber()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();
			header.EH_HandlingInformation = "ACID Number:6AU123456789D0VAHK11N\r\n";
			header.EH_ConsigneeCountryCode = Core.Constants.CountryCodes.Egypt;

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);

			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			const string expected = @"/EG/IMP/M/6AU123456789D0VAHK11N";
			AssertContains(expected, fhlMessage);
		}

		#endregion

		#region Goods Declaration Reference Number

		public void TestGoodsDeclarationReferenceNumbers()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_ShipperCountryCode = Core.Constants.CountryCodes.Switzerland;

			header.GoodsDeclarationReferenceNumbersExposed = new List<GoodsDeclarationReferenceNumber>
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

			var expected = @"OCI/CH/EXP/M/13CH9876AB88901235
/CH/EXP/M/13CH9876AB88901236
/CH/IMP/M/14CH45612354752
/CH/SHP/CT/13175910000
/DE/CNE/CT/49665279340
SHP/HARRIS   FORD LLC
/9307 EAST 56TH STREET ORANGE COUNTY
/LOS ANGELES/CA
/CH/46216/TE/13175910000
CNE/WELLA MANUFACTURING GMBH
/WELLASTRASE 2-4 SCHLESWIG-HOLSTEIN
/HAMBURG
/DE/36088/TE/49665279340
CVD/EUR/CC/2000/4277.68/4800";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			AssertSegment(fhlMessage, "OCI", expected);
		}

		#endregion

		#region Movement Reference Numbers

		public void TestMovementReferenceNumbers()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_ShipperCountryCode = Core.Constants.CountryCodes.Germany;

			header.MovementReferenceNumbersExposed = new List<MovementReferenceNumber>();

			var mrn1 = new MovementReferenceNumber
			{
				CountryOfIssue = "IT",
				MovementCode = MovementReferenceCode.Codes.CustomsExport
			};
			mrn1.Numbers.AddRange(new List<ZString> { "13IT9876ab88901235", "13IT9876ab88901236" });
			header.MovementReferenceNumbersExposed.Add(mrn1);

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
			header.MovementReferenceNumbersExposed.Add(mrn2);

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
			header.MovementReferenceNumbersExposed.Add(mrn3);

			header.MovementReferenceNumbersExposed.Add(new MovementReferenceNumber());

			var expected = @"OCI/IT/EXP/M/13IT9876AB88901235
/IT/EXP/M/13IT9876AB88901236
/PL/IMP/M/14PL45612354752
//HWB/I/HBL1111
/GB/TRA/M/15GB78888555448
//HWB/I/HBL222
//ULD/I/AAAA123456
//ULD/I/BBBB123456
//MAL/I/XXXXXX
/DE/SHP/CT/13175910000
/DE/CNE/CT/49665279340
SHP/HARRIS   FORD LLC
/9307 EAST 56TH STREET ORANGE COUNTY
/LOS ANGELES/CA
/DE/46216/TE/13175910000
CNE/WELLA MANUFACTURING GMBH
/WELLASTRASE 2-4 SCHLESWIG-HOLSTEIN
/HAMBURG
/DE/36088/TE/49665279340
CVD/EUR/CC/2000/4277.68/4800";

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			AssertSegment(fhlMessage, "OCI", expected);
		}

		void AssertSegment(string fhlMessage, string segmentIdentifier, string expected, string message = "", int maxLength = -1)
		{
			var locationOfOCI = fhlMessage.IndexOf(string.Concat(segmentIdentifier, "/"), StringComparison.InvariantCultureIgnoreCase);

			var actual = locationOfOCI > 0
				? fhlMessage.Substring(locationOfOCI, fhlMessage.Length - locationOfOCI - 1)
				: string.Empty;

			if (maxLength >= 0)
			{
				var actualTrimmed = actual.Trim('\r', '\n');
				AssertLessThanOrEqualTo($"Length of {segmentIdentifier} segment should not exceed {maxLength}.", actualTrimmed.Length, maxLength);
			}

			AssertMultilineASCIIEquals(string.Format("{0} segment. {1}", segmentIdentifier, message),
				expected, actual);
		}

		#endregion

		public void TestFHLToString_TraderCodeShouldBePrefixedWithIssuingCountryCode()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_ConsigneeTraderNo = "GB123";
			header.EH_ConsigneeTraderNoCountryCode = "GB";
			header.EH_ConsigneeTraderNoType = "EOR";
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_ConsigneeTraderNoType = "EORI NO.";
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_ShipperTraderNo = "GB123";
			header.EH_ShipperTraderNoCountryCode = "GB";
			header.EH_ShipperTraderNoType = "EOR";
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_ShipperTraderNoType = "EORI NO.";
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_AlsoNotifyTraderNo = "GB123";
			header.EH_AlsoNotifyTraderNoCountryCode = "GB";
			header.EH_AlsoNotifyTraderNoType = "EOR";
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/GB123", message);

			header.EH_AlsoNotifyTraderNoType = "EORI NO.";
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/GB123", message);
		}

		public void TestFHL_ConsigneeTradeTradeNo_ForEFTA()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_ConsigneeTraderNo = "1234567";
			header.EH_ConsigneeCountryCode = Constants.CountryCodes.Norway;
			header.EH_ConsigneeTraderNoCountryCode = Constants.CountryCodes.Norway;
			header.EH_ConsigneeTraderNoType = OrgCusCode.NorwayCodeTypes.MVA;
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/1234567", message);

			header.EH_ConsigneeTraderNo = "123456789";
			header.EH_ConsigneeCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_ConsigneeTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_ConsigneeTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/123456789", message);

			header.EH_ConsigneeTraderNo = "23456789";
			header.EH_ConsigneeCountryCode = Constants.CountryCodes.Germany;
			header.EH_ConsigneeTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_ConsigneeTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("CNE/T/23456789", message);
		}

		public void TestFHL_AlsoNotifyTradeTradeNo_ForEFTA()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_AlsoNotifyTraderNo = "1234567";
			header.EH_AlsoNotifyCountryCode = Constants.CountryCodes.Norway;
			header.EH_AlsoNotifyTraderNoCountryCode = Constants.CountryCodes.Norway;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.NorwayCodeTypes.MVA;
			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("NFY/T/1234567", message);

			header.EH_AlsoNotifyTraderNo = "123456789";
			header.EH_AlsoNotifyCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_AlsoNotifyTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("NFY/T/123456789", message);

			header.EH_AlsoNotifyTraderNo = "23456789";
			header.EH_AlsoNotifyCountryCode = Constants.CountryCodes.Germany;
			header.EH_AlsoNotifyTraderNoCountryCode = Constants.CountryCodes.Switzerland;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.SwissCodeTypes.UID;
			fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			fhlDetailsProvider = new FHLMessageDetails(header);
			message = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();
			AssertContains("NFY/T/23456789", message);
		}

		#region Kenya Custom Entry Number

		public void TestKenyaCustomEntryNumber()
		{
			var dataCreator = new AWBTestDataCreator<ExportAWBHeaderForTest>(Factory);
			var header = dataCreator.SetupHAWBHeader1();

			header.EH_ShipperCountryCode = Core.Constants.CountryCodes.Kenya;
			header.CustomsEntryNumbersExposed = new List<EntryNumber>
			{
				new EntryNumber
				{
					Type = CusEntryNumberTypes.Standard.ClearancePermitNumber,
					Number = "123456789"
				}
				,
				new EntryNumber
				{
					Type = CusEntryNumberTypes.Standard.ClearancePermitNumber,
					Number = "987654321"
				},
				new EntryNumber
				{
					Type = CusEntryNumberTypes.Standard.ClearancePermitNumber,
					Number = string.Empty
				}
			};

			var fwbDetailsProvider = new FWBMessageDetails(dataCreator.MAWBHeader);
			var fhlDetailsProvider = new FHLMessageDetails(header);
			var fhlMessage = new FHL(fwbDetailsProvider, fhlDetailsProvider, FHL.Version.No4).ToString();

			AssertContains("/KE/EXP/M/123456789 987654321", fhlMessage);
		}

		#endregion
	}
}
