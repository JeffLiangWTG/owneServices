using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class ProdatValidatorTests
	{
		[Test]
		public void Validate_Group0_UNH()
		{
			var validSeg = "UNH+1+PRODAT:D:96B:UN:ZZZ01";
			var invalidSeg = "UNH+1+PRODAT:C:96A:UX:ABC02";
			var valid = ProdatValidatorForTest.Validate_Group0_UNH_Exposed(errorCollector, null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'UNH' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group0_UNH_Exposed(errorCollector, new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'UNH' message section"));

			valid = ProdatValidatorForTest.Validate_Group0_UNH_Exposed(errorCollector, new[] { invalidSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Message identifier not supported"));

			valid = ProdatValidatorForTest.Validate_Group0_UNH_Exposed(errorCollector, new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group0_BGM()
		{
			var validSeg = "BGM+6+0+2";
			var invalidSeg = "BGM+1+0+2";
			var valid = ProdatValidatorForTest.Validate_Group0_BGM_Exposed(errorCollector, null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'BGM' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group0_BGM_Exposed(errorCollector, new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'BGM' message section"));

			valid = ProdatValidatorForTest.Validate_Group0_BGM_Exposed(errorCollector, new[] { invalidSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected DocumentMessageNameCoded to be 6 - Product Specification Report"));

			valid = ProdatValidatorForTest.Validate_Group0_BGM_Exposed(errorCollector, new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group0_DTM()
		{
			var validSeg = "DTM+302:20220103:102";
			var invalidSeg_Period = "DTM+291:20220103:102";
			var invalidSeg_Format = "DTM+302:20220103:108";

			var valid = ProdatValidatorForTest.Validate_Group0_DTM_Exposed(errorCollector, null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'DTM' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group0_DTM_Exposed(errorCollector, new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'DTM' message section"));

			valid = ProdatValidatorForTest.Validate_Group0_DTM_Exposed(errorCollector, new[] { invalidSeg_Period });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected Publication date in DTM"));

			valid = ProdatValidatorForTest.Validate_Group0_DTM_Exposed(errorCollector, new[] { invalidSeg_Format });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected CCYYMMDD format for publication date"));

			valid = ProdatValidatorForTest.Validate_Group0_DTM_Exposed(errorCollector, new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group0()
		{
			var validUNH = "UNH+1+PRODAT:D:96B:UN:ZZZ01";
			var validBGM = "BGM+6+0+2";
			var validDTM = "DTM+302:20220103:102";
			var validFTX = "FTX+AAI+++TERMS AND: CONDITIONS APPLY";

			var valid = ProdatValidatorForTest.Validate_Group0_Exposed(errorCollector, new[] { validUNH, validBGM, validDTM, validFTX });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			valid = ProdatValidatorForTest.Validate_Group0_Exposed(errorCollector, new[] { validFTX, validBGM, validDTM });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group8_LIN()
		{
			var validSeg = "LIN+1++ ";
			string lineNumber;
			var valid = ProdatValidatorForTest.Validate_Group8_LIN_Exposed(errorCollector, null, out lineNumber);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'LIN' message section missing"));
			Assert.That(lineNumber, Is.EqualTo(string.Empty));

			valid = ProdatValidatorForTest.Validate_Group8_LIN_Exposed(errorCollector, new[] { validSeg, validSeg }, out lineNumber);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'LIN' message section"));
			Assert.That(lineNumber, Is.EqualTo(string.Empty));

			valid = ProdatValidatorForTest.Validate_Group8_LIN_Exposed(errorCollector, new[] { validSeg }, out lineNumber);
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
			Assert.That(lineNumber, Is.EqualTo("1"));
		}

		[Test]
		public void Validate_Group8_PIA()
		{
			var validSeg = "PIA+5+1+01.01+ +0101.21+1";
			var invalidSeg_Function = "PIA+9+1+01.01+ +0101.21+1";
			var invalidSeg_ItemNumber = "PIA+5+9+01.01+ +0101.21+1";
			var valid = ProdatValidatorForTest.Validate_Group8_PIA_Exposed(errorCollector, null, "1");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'PIA' message section missing. LineNumber: 1"));

			valid = ProdatValidatorForTest.Validate_Group8_PIA_Exposed(errorCollector, new[] { validSeg, validSeg }, "2");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'PIA' message section. LineNumber: 2"));

			valid = ProdatValidatorForTest.Validate_Group8_PIA_Exposed(errorCollector, new[] { invalidSeg_Function }, "2");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected ProductIdFunctionQualifier to be 5 - ProductIdentification. LineNumber: 2"));

			valid = ProdatValidatorForTest.Validate_Group8_PIA_Exposed(errorCollector, new[] { invalidSeg_ItemNumber }, "3");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected ItemNumberIdentification1.ItemNumber to be 1. LineNumber: 3"));

			valid = ProdatValidatorForTest.Validate_Group8_PIA_Exposed(errorCollector, new[] { validSeg }, "");
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group8_DTM()
		{
			var validSeg_1 = "DTM+7:20120101:102";
			var validSeg_2 = "DTM+206:99991231000000:204";
			var invalidSeg_Format = "DTM+206:99991231000000:3";
			var invalidSeg_Period = "DTM+92:99991231000000:204";
			var valid = ProdatValidatorForTest.Validate_Group8_DTM_Exposed(errorCollector, null, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'DTM' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group8_DTM_Exposed(errorCollector, new[] { validSeg_1 }, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 2 occurrence(s) of 'DTM' message section"));

			valid = ProdatValidatorForTest.Validate_Group8_DTM_Exposed(errorCollector, new[] { validSeg_1, validSeg_2, validSeg_2 }, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 2 occurrence(s) of 'DTM' message section"));

			valid = ProdatValidatorForTest.Validate_Group8_DTM_Exposed(errorCollector, new[] { validSeg_1, invalidSeg_Format }, "4");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected DateTimePeriodFormat to be 102 - CCYYMMDD or 204 - CCYYMMDDHHMMSS. LineNumber: 4"));

			valid = ProdatValidatorForTest.Validate_Group8_DTM_Exposed(errorCollector, new[] { validSeg_1, invalidSeg_Period }, "5");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected DateTimePeriod to be 7 - EffectiveDateTime or 206 - EndDateTime. LineNumber: 5"));

			valid = ProdatValidatorForTest.Validate_Group8_DTM_Exposed(errorCollector, new[] { validSeg_1, validSeg_2 }, "");
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group8_MEA()
		{
			var validSeg = "MEA+AAE+:::UNIT";
			var invalidSeg = "MEA+AAH+:::UNIT";
			var valid = ProdatValidatorForTest.Validate_Group8_MEA_Exposed(errorCollector, null, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'MEA' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group8_MEA_Exposed(errorCollector, new[] { validSeg, validSeg }, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'MEA' message section"));

			valid = ProdatValidatorForTest.Validate_Group8_MEA_Exposed(errorCollector, new[] { invalidSeg }, "6");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected MeasurementApplicationQualifier to be AAE - Measurement. LineNumber: 6"));

			valid = ProdatValidatorForTest.Validate_Group8_MEA_Exposed(errorCollector, new[] { validSeg }, "");
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group8_FTX()
		{
			var validSeg_1 = "FTX+A10+++99";
			var validSeg_2 = "FTX+AAA+++PURE-BRED BREEDING ANIMALS: : : : ";

			var valid = ProdatValidatorForTest.Validate_Group8_FTX_Exposed(errorCollector, null, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'FTX' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group8_FTX_Exposed(errorCollector, new[] { "" }, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected at least 1 occurrence(s) of 'FTX' message section"), "");

			valid = ProdatValidatorForTest.Validate_Group8_FTX_Exposed(errorCollector, new[] { validSeg_1 }, "");
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			valid = ProdatValidatorForTest.Validate_Group8_FTX_Exposed(errorCollector, new[] { validSeg_1, validSeg_2 }, "");
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group8_PGI()
		{
			var validSeg = "PGI+11+1P1";
			var invalidSeg = "PGI+3+1P1";
			var valid = ProdatValidatorForTest.Validate_Group8_PGI_Exposed(errorCollector, null, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'PGI' message section missing"));

			valid = ProdatValidatorForTest.Validate_Group8_PGI_Exposed(errorCollector, new[] { validSeg, validSeg }, "");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'PGI' message section"), "");

			valid = ProdatValidatorForTest.Validate_Group8_PGI_Exposed(errorCollector, new[] { invalidSeg }, "7");
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected ProductGroupTypeCoded to be 11 - ProductGroup. LineNumber: 7"));

			valid = ProdatValidatorForTest.Validate_Group8_PGI_Exposed(errorCollector, new[] { validSeg }, "");
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group8()
		{
			var validLIN = "LIN+1++ ";
			var validPIA = "PIA+5+1+01.01+ +0101.21+1";
			var validDTM1 = "DTM+7:20120101:102";
			var validDTM2 = "DTM+206:99991231000000:204";
			var validMEA = "MEA+AAE+:::UNIT";
			var validFTX = "FTX+A10+++99";
			var validPGI = "PGI+11+1P1";

			var valid = ProdatValidatorForTest.Validate_Group8_Exposed(errorCollector, new[] { validLIN, validPIA, validDTM1, validDTM2, validMEA, validFTX, validPGI });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			valid = ProdatValidatorForTest.Validate_Group8_Exposed(errorCollector, new[] { validLIN, validPIA, validDTM1, validMEA, validFTX, validPGI });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.Not.EqualTo(string.Empty));
		}

		[Test]
		public void ValidateMessage()
		{
			var validGroup0 = new List<string>
			{
				"UNH+1+PRODAT:D:96B:UN:ZZZ01",
				"BGM+6+0+2",
				"DTM+302:20220103:102",
				"FTX+AAI+++TERMS AND: CONDITIONS APPLY"
			};

			var validGroup8 = new List<string>
			{
				"LIN+1++ ",
				"PIA+5+1+01.01+ +0101.21+1",
				"DTM+7:20120101:102",
				"DTM+206:99991231000000:204",
				"MEA+AAE+:::UNIT",
				"FTX+A10+++99",
				"PGI+11+1P1"
			};

			var data = new List<string>();
			data.AddRange(validGroup0);

			var valid = ProdatValidatorForTest.ValidateMessage_Exposed(errorCollector, null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("PRODAT message is NULL"));

			valid = ProdatValidatorForTest.ValidateMessage_Exposed(errorCollector, data.ToArray());
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected at least 1 occurrence of 'Group8' segment group"));

			data.AddRange(validGroup8);
			valid = ProdatValidatorForTest.ValidateMessage_Exposed(errorCollector, data.ToArray());
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			data.AddRange(validGroup8);
			valid = ProdatValidatorForTest.ValidateMessage_Exposed(errorCollector, data.ToArray());
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			data.RemoveAt(0); // Remove UNH segment
			valid = ProdatValidatorForTest.ValidateMessage_Exposed(errorCollector, data.ToArray());
			Assert.That(valid, Is.EqualTo(false));
		}

		[Test]
		public void ValidateFiles()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Valid_01.txt");
			var msg = EdifactLoader.LoadProdatMessage(msgContent);

			errorCollector.Clear();

			var valid = ProdatValidator.ValidateMessage(msg, errorCollector);
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			msgContent = "This is not an Edifact Message";
			msg = EdifactLoader.LoadProdatMessage(msgContent);
			valid = ProdatValidator.ValidateMessage(msg, errorCollector);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("PRODAT message is NULL"));

			errorCollector.Clear();
			msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.FullPRODAT.txt");
			msg = EdifactLoader.LoadProdatMessage(msgContent);
			valid = ProdatValidator.ValidateMessage(msg, errorCollector);
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Invalid.txt");
			msg = EdifactLoader.LoadProdatMessage(msgContent);
			valid = ProdatValidator.ValidateMessage(msg, errorCollector);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'PIA' message section"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			errorCollector = new StringBuilder();
		}

		StringBuilder errorCollector;
	}
}
