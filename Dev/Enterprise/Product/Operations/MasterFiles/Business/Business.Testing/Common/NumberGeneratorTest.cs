using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NumberGeneratorTest : TestCaseWithFactory
	{
		public void TestNullAdditionalNumberCustomisation()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var primary = new DummyTarget(customisation1);
			var additional = new DummyTarget(null);

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = Env.NumberFountains.JobShipmentNumber;
			generator.FountainGetter = FountainGetter;
			generator.AdditionalTargets.Add(additional);

			AssertNoExceptionThrown("generator.Generate()", () => generator.Generate());
		}

		public void TestFountainSelection()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var customisation2 = new BillOfLadingNumberCustomisation();
			customisation2.Categories = NumberCustomisationElementCategories.Standard;

			var baseFountain = Env.NumberFountains.GetForwardingGeneratorFountain("::");
			var nextNumber = 100;
			var secondFountain = Env.NumberFountains.GetForwardingGeneratorFountain("XXX-Blah");
			secondFountain.SetNext(Factory, 200);
			var thirdFountain = Env.NumberFountains.GetForwardingGeneratorFountain("XXX-BlahEDI");
			thirdFountain.SetNext(Factory, 300);

			customisation1.UseShipmentSequenceNumber = false;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Blah", fountain: false, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 2, "", fountain: false, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");

			customisation2.UseShipmentSequenceNumber = true;
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Blah", fountain: false, checkDigit: true);
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 2, "", fountain: false, checkDigit: true);
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");

			AssertGenerated(customisation1, customisation2, "Should use the base fountain", "BlahEDI100", "BlahEDI100", baseFountain, "BlahEDI", "100", "",
				baseFountain, "BlahEDI", "100", "", baseFountain, nextNumber);

			customisation1.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Fountain = true;
			AssertGenerated(customisation1, customisation2, "Should use the \"Blah\" fountain", "BlahEDI200", "BlahEDI200", secondFountain, "BlahEDI", "200", "",
				secondFountain, "BlahEDI", "200", "", baseFountain, nextNumber);

			customisation1.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Fountain = true;
			AssertGenerated(customisation1, customisation2, "Should use the \"BlahEDI\" fountain", "BlahEDI300", "BlahEDI300", thirdFountain, "BlahEDI", "300", "",
				thirdFountain, "BlahEDI", "300", "", baseFountain, nextNumber);

			customisation1.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Order = 3;
			AssertGenerated(customisation1, customisation2, "Should ignore the order for the purpose of selecting a fountain", "EDIBlah301", "BlahEDI301", thirdFountain, "EDIBlah", "301", "",
				thirdFountain, "BlahEDI", "301", "", baseFountain, nextNumber);

			customisation2.UseShipmentSequenceNumber = false;
			AssertGenerated(customisation1, customisation2, "Bill of lading should now use the basic fountain", "EDIBlah302", "BlahEDI100", thirdFountain, "EDIBlah", "302", "",
				null, "BlahEDI", "100", "", baseFountain, nextNumber);

			customisation2.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Fountain = true;
			AssertGenerated(customisation1, customisation2, "Bill of lading should now use its own number fountain", "EDIBlah303", "BlahEDI201", thirdFountain, "EDIBlah", "303", "",
				null, "BlahEDI", "201", "", baseFountain, nextNumber);

			customisation2.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Fountain = true;
			AssertGenerated(customisation1, customisation2, "Bill of lading should use the same number if the fountain key is the same", "EDIBlah304", "BlahEDI304", thirdFountain, "EDIBlah", "304", "",
				thirdFountain, "BlahEDI", "304", "", baseFountain, nextNumber);
		}

		public void TestRemoveFountainPrefix()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var customisation2 = new BillOfLadingNumberCustomisation();
			customisation2.Categories = NumberCustomisationElementCategories.Standard;

			customisation1.RemoveFountainPrefix = false;
			customisation2.RemoveFountainPrefix = false;

			AssertGenerated(customisation1, customisation2, "Dont remove the 'S' prefix by default", "S00001000", "S00001000", Env.NumberFountains.JobShipmentNumber, "S", "00001000", "", Env.NumberFountains.JobShipmentNumber);

			customisation1.RemoveFountainPrefix = true;
			customisation2.RemoveFountainPrefix = true;

			AssertGenerated(customisation1, customisation2, "Remove the 'S' prefix", "00001000", "00001000", Env.NumberFountains.JobShipmentNumber, "", "00001000", "", Env.NumberFountains.JobShipmentNumber);
		}

		public void TestTrimNumberToLength()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var nextNumber = 2001000;

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "8");
			AssertGenerated(customisation1, "Trim number to 8 digits", "S02001000", Env.NumberFountains.JobShipmentNumber, "S", "02001000", "", Env.NumberFountains.JobShipmentNumber, nextNumber);

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "7");
			AssertGenerated(customisation1, "Trim number to 7 digits", "S2001000", Env.NumberFountains.JobShipmentNumber, "S", "2001000", "", Env.NumberFountains.JobShipmentNumber, nextNumber);

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "6");
			AssertGenerated(customisation1, "Trim number to 5 digits", "S2001000", Env.NumberFountains.JobShipmentNumber, "S", "2001000", "", Env.NumberFountains.JobShipmentNumber, nextNumber);
		}

		public void TestSequenceNumber()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 2, "Blah");
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 3, "8");
			AssertGenerated(customisation1, "Sequence number at the end", "SBlah00001000", Env.NumberFountains.JobShipmentNumber, "SBlah", "00001000", "", Env.NumberFountains.JobShipmentNumber);

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "8");
			AssertGenerated(customisation1, "SequenceNumber at the beginning", "S00001000Blah", Env.NumberFountains.JobShipmentNumber, "S", "00001000", "Blah", Env.NumberFountains.JobShipmentNumber);
		}

		public void TestEnforceMaxLength()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var customisation2 = new BillOfLadingNumberCustomisation();
			customisation2.Categories = NumberCustomisationElementCategories.Standard;

			var primaryTarget = new DummyTarget(customisation1, 1);
			var aditionalTarget = new DummyTarget(customisation2, 2);

			var generator = new NumberGenerator();
			generator.PrimaryTarget = primaryTarget;
			generator.AdditionalTargets.Add(aditionalTarget);

			primaryTarget.Value = "12345678901234567890";
			aditionalTarget.Value = "12345678901234567890";

			generator.EnforceMaxLengths();

			primaryTarget.Value = "123456789012345678901";
			try
			{
				generator.EnforceMaxLengths();
				Fail("should have thrown a GeneratedOverLengthCodeException.");
			}
			catch (GeneratedOverLengthCodeException ex)
			{
				const string expectedMessage =
					"Generated a <insert name1 here> that is too big to fit in the available space.\r\n" +
					"('123456789012345678901')\r\n" +
					"\r\n" +
					"To fix this you need to change the following registry option to generate shorter numbers.\r\n" +
					"<insert location1 here>" +
					"";

				AssertEquals(expectedMessage, ex.Message);
			}

			primaryTarget.Value = "12345678901234567890";
			aditionalTarget.Value = "123456789012345678901";
			try
			{
				generator.EnforceMaxLengths();
				Fail("should have thrown a GeneratedOverLengthCodeException.");
			}
			catch (GeneratedOverLengthCodeException ex)
			{
				const string expectedMessage =
					"Generated a <insert name2 here> that is too big to fit in the available space.\r\n" +
					"('123456789012345678901')\r\n" +
					"\r\n" +
					"To fix this you need to change the following registry option to generate shorter numbers.\r\n" +
					"<insert location2 here>" +
					"";

				AssertEquals(expectedMessage, ex.Message);
			}
		}

		public void TestCheckDigitAlgorithm_R31()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var baseFountain = Env.NumberFountains.GetForwardingGeneratorFountain("::");
			var nextNumber = 100;

			customisation1.RemoveFountainPrefix = true;
			customisation1.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "2");

			for (int i = 0; i < 31; i++)
			{
				var prefix = "1234567890ABCDEFGHIJKLMNOPQRSTU"[i];
				var check = "BCDEFGHIJKLMNOPQRSTU0123456789A"[i];

				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, prefix.ToString() + "###");
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, prefix.ToString(), fountain: false, checkDigit: false);

				AssertGenerated(
					customisation1,
					string.Format("{0}###{0}100 = {1} // ignore non-alphanumeric", prefix, check),
					string.Format("{0}###{0}100{1}", prefix, check),
					baseFountain, prefix.ToString() + "###" + prefix.ToString(), "100", "", baseFountain, nextNumber);

				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, prefix.ToString());
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, prefix.ToString(), fountain: false, checkDigit: false);

				AssertGenerated(
					customisation1,
					string.Format("{0}{0}100 = {1}", prefix, check),
					string.Format("{0}{0}100{1}", prefix, check),
					baseFountain, prefix.ToString() + prefix.ToString(), "100", "", baseFountain, nextNumber);
			}

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, "1", fountain: false, checkDigit: true);
			AssertGenerated(
					customisation1,
				string.Format("{0}100 = {1}", "U1", "2"),
				string.Format("{0}100{1}", "U1", "2"),
				baseFountain, "U1", "100", "", baseFountain, nextNumber);
		}

		public void TestCheckDigitAlgorithm_R07()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var baseFountain = Env.NumberFountains.GetForwardingGeneratorFountain("::");
			var nextNumber = 100;

			customisation1.RemoveFountainPrefix = true;
			customisation1.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.MAWB;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "2");

			for (int i = 0; i < 36; i++)
			{
				var prefix = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[i];
				var check = "210654321065432106543210654321065432"[i];

				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, prefix.ToString() + "[]");
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, prefix.ToString(), fountain: false, checkDigit: false);
				AssertGenerated(
					customisation1,
					string.Format("{0}[]{0}100 = {1} // ignore non-alphanumeric", prefix, check),
					string.Format("{0}[]{0}100{1}", prefix, check),
					baseFountain, prefix.ToString() + "[]" + prefix.ToString(), "100", "", baseFountain, nextNumber);

				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, prefix.ToString());
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, prefix.ToString(), fountain: false, checkDigit: false);
				AssertGenerated(
					customisation1,
					string.Format("{0}{0}100 = {1}", prefix, check),
					string.Format("{0}{0}100{1}", prefix, check),
					baseFountain, prefix.ToString() + prefix.ToString(), "100", "", baseFountain, nextNumber);
			}

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, "1", fountain: false, checkDigit: true);
			AssertGenerated(
				customisation1,
				string.Format("{0}100 = {1}", "Z1", "1"),
				string.Format("{0}100{1}", "Z1", "1"),
				baseFountain, "Z1", "100", "", baseFountain, nextNumber);
		}

		public void TestCheckDigitAlgorithm_RCC()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var baseFountain = Env.NumberFountains.GetForwardingGeneratorFountain("::");
			var nextNumber = 100;

			customisation1.RemoveFountainPrefix = true;
			customisation1.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.CanadaCustoms;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "2");

			for (int i = 0; i < 36; i++)
			{
				var prefix = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[i];
				var check = "234567890111111111111111111111111111"[i];

				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, prefix.ToString() + "[]");
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, prefix.ToString(), fountain: false, checkDigit: false);
				AssertGenerated(
					customisation1,
					string.Format("{0}[]{0}100 = {1} // ignore non-alphanumeric", prefix, check),
					string.Format("{0}[]{0}100{1}", prefix, check),
					baseFountain, prefix.ToString() + "[]" + prefix.ToString(), "100", "", baseFountain, nextNumber);

				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, prefix.ToString());
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, prefix.ToString(), fountain: false, checkDigit: false);
				AssertGenerated(
					customisation1,
					string.Format("{0}{0}100 = {1}", prefix, check),
					string.Format("{0}{0}100{1}", prefix, check),
					baseFountain, prefix.ToString() + prefix.ToString(), "100", "", baseFountain, nextNumber);
			}

			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, "1", fountain: false, checkDigit: true);
			AssertGenerated(
				customisation1,
				string.Format("{0}100 = {1}", "Z1", "3"),
				string.Format("{0}100{1}", "Z1", "3"),
				baseFountain, "Z1", "100", "", baseFountain, nextNumber);
		}

		[TestDate(2010, 09, 06)]
		public void TestSuffix()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var customisation2 = new BillOfLadingNumberCustomisation();
			customisation2.Categories = NumberCustomisationElementCategories.Standard;

			var fountain = Env.NumberFountains.GetForwardingGeneratorFountain("XXX-BlahEDI");
			fountain.SetNext(Factory, 300);

			customisation1.UseShipmentSequenceNumber = false;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Blah", true, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 2, "", true, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 51, "2", fountain: false, checkDigit: true);

			customisation2.UseShipmentSequenceNumber = true;
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Blah", fountain: false, checkDigit: true);
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 2, "", fountain: false, checkDigit: true);
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			SetElement(customisation2, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 51, "1", fountain: false, checkDigit: true);

			AssertGenerated(customisation1, customisation2, "Should use the \"BlahEDI\" fountain", "SBlahEDI30010", "SBlahEDI300I", fountain, "SBlahEDI", "300", "10",
				fountain, "SBlahEDI", "300", "I", Env.NumberFountains.JobShipmentNumber, 1000);

			customisation1.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			customisation2.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			AssertGenerated(customisation1, customisation2, "Should use the \"BlahEDI\" fountain", "SBlahEDI30110Q", "SBlahEDI301IB", fountain, "SBlahEDI", "301", "10",
				fountain, "SBlahEDI", "301", "I", Env.NumberFountains.JobShipmentNumber, 1000);
		}

		public void TestHandleInvalidGeneration()
		{
			var baseFountain = Env.NumberFountains.JobShipmentNumber;
			baseFountain.SetNext(Factory, 1000);

			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.SundryCharges;
			var primary = new DummyTarget(customisation1, 1);
			primary.NumberCustomisation.Elements.Cast<BillOfLadingNumberCustomisationElement>().ToList().ForEach(x => x.Include = false);
			primary.FountainValue = "HELLO";
			primary.ValuePrefix = "J";
			primary.ValueSuffix = "K";

			var customisation2 = new BillOfLadingNumberCustomisation();
			customisation2.Categories = NumberCustomisationElementCategories.PackageID;
			var additional = new DummyTarget(customisation2, 2);
			additional.NumberCustomisation.Elements.Cast<BillOfLadingNumberCustomisationElement>().ToList().ForEach(x => x.Include = false);
			additional.FountainValue = "BYE";
			additional.ValuePrefix = "Q";
			additional.ValueSuffix = "R";

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			CombineAssertions(() =>
			{
				AssertExceptionThrown<InvalidOperationException>("primary not setup", "PrimaryTarget.Value was set to prefix 'S' only", generator.Generate);
				AssertEquals("LastKeyReported", "Invalid NumberGeneratorTarget (<insert name1 here>, 20, )", ErrorReporter.LastKeyReported);
				AssertMultilineASCIIEquals("LastMessageReported", $@"Context:
Company=[EDI='{GlbCompany.CurrentCompany.PK}']
Branch=[BNE='{GlbBranch.CurrentBranch.PK}']
Department=[BRN='{GlbDepartment.CurrentDepartment.PK}']

Value='S'
Name='<insert name1 here>'
MaxLength='20'
NumberCustomisationLocation='<insert location1 here>'
FountainUsedForGeneration='Enterprise.ZArchitecture.Environment.NumberFountainProxy'
Prefix='S'
Seed='1000'
ValuePrefix='S'
ValueSuffix=''
FountainValue=''
Elements.Categories='SundryCharges'
", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				primary.NumberCustomisation.Elements.SetCategories(NumberCustomisationElementCategories.Standard);
				SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
				generator.AdditionalTargets.Add(additional);
				AssertNoExceptionThrown(generator.Generate);
				AssertEquals("LastKeyReported", "Invalid NumberGeneratorTarget (<insert name2 here>, 20, )", ErrorReporter.LastKeyReported);
				AssertMultilineASCIIEquals("LastMessageReported", $@"Context:
Company=[EDI='{GlbCompany.CurrentCompany.PK}']
Branch=[BNE='{GlbBranch.CurrentBranch.PK}']
Department=[BRN='{GlbDepartment.CurrentDepartment.PK}']

Value='S'
Name='<insert name2 here>'
MaxLength='20'
NumberCustomisationLocation='<insert location2 here>'
FountainUsedForGeneration='Enterprise.ZArchitecture.Environment.NumberFountainProxy'
Prefix='S'
Seed='1001'
ValuePrefix='S'
ValueSuffix=''
FountainValue=''
Elements.Categories='PackageID'
", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestGenerateNumbers()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var baseFountain = Env.NumberFountains.JobShipmentNumber;
			baseFountain.SetNext(Factory, 1001);

			var primary = new DummyTarget(customisation1);
			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertSequencesEqual("Correct numbers and quantity are generated.",
				new ZString[] { "S00001001", "S00001002", "S00001003", "S00001004", "S00001005" },
				generator.GenerateNumbers(primary, 5));

			AssertSequencesEqual("Correct numbers and quantity are generated on next run.",
				new ZString[] { "S00001006", "S00001007", "S00001008", "S00001009", "S00001010", "S00001011" },
				generator.GenerateNumbers(primary, 6));
		}

		public void TestGenerateNumbers_RemoveFountainPrefix()
		{
			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var baseFountain = Env.NumberFountains.JobShipmentNumber;
			baseFountain.SetNext(Factory, 1001);

			customisation1.RemoveFountainPrefix = true;
			var primary = new DummyTarget(customisation1);
			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertSequencesEqual("Correct numbers and quantity are generated.",
				new ZString[] { "00001001", "00001002", "00001003", "00001004", "00001005" },
				generator.GenerateNumbers(primary, 5));
		}

		[TestDate(2010, 09, 06)]
		public void TestGenerateNumbers_WithSuffix()
		{
			var fountain = Env.NumberFountains.GetForwardingGeneratorFountain("XXX-BlahEDI");
			fountain.SetNext(Factory, 300);

			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			customisation1.UseShipmentSequenceNumber = false;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Blah", fountain: true, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 2, "", fountain: true, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 51, "2", fountain: false, checkDigit: true);

			var primary = new DummyTarget(customisation1);
			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = Env.NumberFountains.JobShipmentNumber;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertSequencesEqual("Correct numbers and quantity are generated.",
				new ZString[] { "SBlahEDI30010", "SBlahEDI30110", "SBlahEDI30210", "SBlahEDI30310", "SBlahEDI30410" },
				generator.GenerateNumbers(primary, 5));
		}

		[TestDate(2010, 09, 06)]
		public void TestGenerateNumbers_WithSuffix_WithCheckDigit()
		{
			var fountain = Env.NumberFountains.GetForwardingGeneratorFountain("XXX-BlahEDI");
			fountain.SetNext(Factory, 300);

			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			customisation1.UseShipmentSequenceNumber = false;
			customisation1.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Blah", fountain: true, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 2, "", fountain: true, checkDigit: true);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 51, "2", fountain: false, checkDigit: true);

			var primary = new DummyTarget(customisation1);
			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = Env.NumberFountains.JobShipmentNumber;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertSequencesEqual("Correct numbers and quantity are generated.",
				new ZString[] { "SBlahEDI30010N", "SBlahEDI30110Q", "SBlahEDI302105", "SBlahEDI30310F", "SBlahEDI30410P" },
				generator.GenerateNumbers(primary, 5));
		}

		public void TestGenerateNumbers_NullTarget()
		{
			var baseFountain = Env.NumberFountains.JobShipmentNumber;
			baseFountain.SetNext(Factory, 1001);

			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var primary = new DummyTarget(customisation1);
			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertExceptionThrown(typeof(ArgumentNullException), () => generator.GenerateNumbers(null, 10));
		}

		public void TestGenerateNumbers_InvalidGeneration()
		{
			var baseFountain = Env.NumberFountains.JobShipmentNumber;
			baseFountain.SetNext(Factory, 1001);

			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.SundryCharges;
			var primary = new DummyTarget(customisation1, 1);
			primary.NumberCustomisation.Elements.Cast<BillOfLadingNumberCustomisationElement>().ToList().ForEach(x => x.Include = false);

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertExceptionThrown(typeof(InvalidOperationException), "Generated number is empty.", () => generator.GenerateNumbers(primary, 10));
		}

		public void TestGenerateNumbers_EnforceMaxLength()
		{
			var baseFountain = Env.NumberFountains.JobShipmentNumber;
			baseFountain.SetNext(Factory, 1001);

			var customisation1 = new BillOfLadingNumberCustomisation();
			customisation1.Categories = NumberCustomisationElementCategories.Standard;

			var primary = new DummyTarget(customisation1);
			SetElement(customisation1, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "20");

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			AssertExceptionThrown(typeof(GeneratedOverLengthCodeException),
				@"Generated a <insert name here> that is too big to fit in the available space.
('S00000000000000001001')

To fix this you need to change the following registry option to generate shorter numbers.
<insert location here>",
				() => generator.GenerateNumbers(primary, 10, enforceMaxLength: true));
		}

		void AssertGenerated(BillOfLadingNumberCustomisation customisation1, string message, string expectedPrimary, INumberFountainProxy expectedFountain, string expectedPrefix,
			string expectedSequenceNumber, string expectedSuffix, INumberFountainProxy baseFountain, int nextNumber = 1000)
		{
			baseFountain.SetNext(Factory, nextNumber);

			var primary = new DummyTarget(customisation1);

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.ValueProviders.AddRange(new StandardValueSource());

			generator.Generate();

			AssertEquals(message + ": Primary", expectedPrimary, primary.Value);
			if (expectedFountain != null)
			{
				AssertNotNull(message + ": Fountain", primary.FountainUsedForGeneration);
				AssertEquals(message + ": Fountain",
						expectedFountain.PeekPreliminaryFormatted(new Connected()),
						primary.FountainUsedForGeneration.PeekPreliminaryFormatted(new Connected()));
			}

			AssertEquals(message + ": Prefix", expectedPrefix, primary.ValuePrefix);
			AssertEquals(message + ": Sequence Number", expectedSequenceNumber, primary.FountainValue);
			AssertEquals(message + ": Suffix", expectedSuffix, primary.ValueSuffix);
		}

		void AssertGenerated(BillOfLadingNumberCustomisation customisation1, BillOfLadingNumberCustomisation customisation2, string message, string expectedPrimary, string expectedAditional,
				INumberFountainProxy expectedPrimaryFountain, string expectedPrimaryPrefix, string expectedPrimarySequenceNumber, string expectedPrimarySuffix, INumberFountainProxy baseFountain, int nextNumber = 1000)
		{
			AssertGenerated(customisation1, customisation2, message, expectedPrimary, expectedAditional, expectedPrimaryFountain, expectedPrimaryPrefix, expectedPrimarySequenceNumber, expectedPrimarySuffix,
					null, null, null, null, baseFountain, nextNumber);
		}

		void AssertGenerated(BillOfLadingNumberCustomisation customisation1, BillOfLadingNumberCustomisation customisation2, string message, string expectedPrimary, string expectedAditional,
				INumberFountainProxy expectedPrimaryFountain, string expectedPrimaryPrefix, string expectedPrimarySequenceNumber, string expectedPrimarySuffix,
				INumberFountainProxy expectedAdditionalFountain, string expectedAdditionalPrefix, string expectedAdditionalSequenceNumber, string expectedAdditionalSuffix, INumberFountainProxy baseFountain, int nextNumber = 1000)
		{
			baseFountain.SetNext(Factory, nextNumber);

			var primary = new DummyTarget(customisation1);
			var additional = new DummyTarget(customisation2);

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.PrimaryTarget = primary;
			generator.Context = new NumberGeneratorContext(CompanyPK, BranchPK, DepartmentPK);
			generator.BaseFountain = baseFountain;
			generator.FountainGetter = FountainGetter;
			generator.AdditionalTargets.Add(additional);
			generator.ValueProviders.AddRange(new StandardValueSource());

			generator.Generate();

			AssertEquals(message + ": Primary", expectedPrimary, primary.Value);
			AssertEquals(message + ": Additional", expectedAditional, additional.Value);
			if (expectedPrimaryFountain != null)
			{
				AssertNotNull(message + ": Primary Fountain", primary.FountainUsedForGeneration);
				AssertEquals(message + ": Primary Fountain",
						expectedPrimaryFountain.PeekPreliminaryFormatted(new Connected()),
						primary.FountainUsedForGeneration.PeekPreliminaryFormatted(new Connected()));
			}

			AssertEquals(message + ": Primary Prefix", expectedPrimaryPrefix, primary.ValuePrefix);
			AssertEquals(message + ": Primary Sequence Number", expectedPrimarySequenceNumber, primary.FountainValue);
			AssertEquals(message + ": Primary Suffix", expectedPrimarySuffix, primary.ValueSuffix);
			if (expectedAdditionalFountain != null)
			{
				AssertNotNull(message + ": Additional Fountain", additional.FountainUsedForGeneration);
				AssertEquals(message + ": Additional Fountain",
						expectedAdditionalFountain.PeekPreliminaryFormatted(new Connected()),
						additional.FountainUsedForGeneration.PeekPreliminaryFormatted(new Connected()));
			}

			if (expectedAdditionalPrefix != null)
			{
				AssertEquals(message + ": Additional Prefix", expectedAdditionalPrefix, additional.ValuePrefix);
			}

			if (expectedAdditionalSequenceNumber != null)
			{
				AssertEquals(message + ": Additional Sequence Number", expectedAdditionalSequenceNumber, additional.FountainValue);
			}

			if (expectedAdditionalSuffix != null)
			{
				AssertEquals(message + ": Additional Suffix", expectedAdditionalSuffix, additional.ValueSuffix);
			}
		}

		void SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail = "", bool fountain = false, bool checkDigit = true)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			element.Fountain = fountain;
			element.CheckDigit = checkDigit;
		}

		readonly ZGuid CompanyPK = GlbCompany.CurrentCompany.PK;
		readonly ZGuid BranchPK = GlbBranch.CurrentBranch.PK;
		readonly ZGuid DepartmentPK = GlbDepartment.CurrentDepartment.PK;

		INumberFountainProxy FountainGetter(string fountainKey) => Env.NumberFountains.GetForwardingGeneratorFountain("XXX-" + fountainKey);

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AllowManualShipmentEntry = false;
		}

		class DummyTarget : NumberGeneratorTarget
		{
			public DummyTarget(BillOfLadingNumberCustomisation customisation)
				: this(customisation, null) { }

			public DummyTarget(BillOfLadingNumberCustomisation customisation, int? num)
			{
				this.customisation = customisation;
				this.num = num;
			}

			public override string NumberCustomisationLocation
			{
				get => string.Format("<insert location{0} here>", num);
			}

			protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => customisation;

			protected override int GetMaxLengthCore() => 20;

			protected override ZString GetNameCore() => string.Format("<insert name{0} here>", num);

			readonly BillOfLadingNumberCustomisation customisation;
			readonly int? num;
		}

		class Connected : IDbConnected
		{
			public DbConnection Connection
			{
				get => Db.Connection;
			}
		}
	}
}
