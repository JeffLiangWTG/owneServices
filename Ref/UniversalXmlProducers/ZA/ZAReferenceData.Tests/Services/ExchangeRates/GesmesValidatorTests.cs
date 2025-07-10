using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using Enterprise.Edifact.D96B.Messages.GESMES;
using Enterprise.Edifact.D96B.Segments;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates
{
	[TestFixture]
	sealed class GesmesValidatorTests
	{
		[Test]
		public void ValidateMessage()
		{
			var validGroup0 = new List<string>
			{
				"UNH+1+GESMES:D:96B:UN:ZZZ01",
				"BGM+190+0+9",
				"DTM+7:20220103:102"
			};

			var validGroup11 = new List<string>
			{
				"DSI+ZAR",
				"ARR+BRL+0.241124",
				"ARR+HKD+0.517350",
				"ARR+ZWD+25.587192"
			};

			var validUNT = "UNT+8+1";

			var data = new List<string>();
			data.AddRange(validGroup0);

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.ValidateMessage(EdifactValidationTestHelper.PrepareSecGroup<GESMESMessage>(segData, errorCollector), errorCollector);

			var valid = runValidation(null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("GESMES message is NULL"));

			valid = runValidation(data.ToArray());
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected at least 1 occurrence of 'Group11' segment group"));

			data.AddRange(validGroup11);

			valid = runValidation(data.ToArray());
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'UNT' message section"));

			data.Add(validUNT);

			valid = runValidation(data.ToArray());
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			data.AddRange(validGroup11);
			valid = runValidation(data.ToArray());
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			data.RemoveAt(0); // Remove UNH segment
			valid = runValidation(data.ToArray());
			Assert.That(valid, Is.EqualTo(false));
		}

		[Test]
		public void Validate_UNH()
		{
			var validSeg = "UNH+1+GESMES:D:96B:UN:ZZZ01";
			var invalidSeg = "UNH+1+GESMES:C:96A:UX:ABC02";

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.Validate_UNH(EdifactValidationTestHelper.PrepareMsgSection(segData, new UNHSegmentMessageSection(9999), errorCollector), errorCollector);

			var valid = runValidation(null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'UNH' message section missing"));

			valid = runValidation(new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'UNH' message section"));

			valid = runValidation(new[] { invalidSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Message identifier not supported"));

			valid = runValidation(new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_BGM()
		{
			var validSeg = "BGM+190++2";
			var invalidSeg = "BGM+1++9";
			var invalidSegDel = "BGM+190++3";

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.Validate_BGM(EdifactValidationTestHelper.PrepareMsgSection(segData, new BGMSegmentMessageSection(9999), errorCollector), errorCollector);

			var valid = runValidation(null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'BGM' message section missing"));

			valid = runValidation(new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'BGM' message section"));

			valid = runValidation(new[] { invalidSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected DocumentMessageNameCoded to be 190 - Statistical and other administrative internal documents"));

			valid = runValidation(new[] { invalidSegDel });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Exchange rate deletions are not supported"));

			valid = runValidation(new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_DTM()
		{
			var validSeg = "DTM+7:20220103:102";
			var invalidSeg_Period = "DTM+291:20220103:102";
			var invalidSeg_Format = "DTM+7:20220103:108";

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.Validate_DTM(EdifactValidationTestHelper.PrepareMsgSection(segData, new DTMSegmentMessageSection(9999), errorCollector), errorCollector);

			var valid = runValidation(null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'DTM' message section missing"));

			valid = runValidation(new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'DTM' message section"));

			valid = runValidation(new[] { invalidSeg_Period });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected Effective date in DTM"));

			valid = runValidation(new[] { invalidSeg_Format });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected CCYYMMDD format for effective date"));

			valid = runValidation(new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group11()
		{
			var validDSI = "DSI+ZAR";
			var validARR1 = "ARR+AUD+0.12345";
			var validARR2 = "ARR+GBP+0.0056";

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.Validate_Group11(EdifactValidationTestHelper.PrepareSecGroup<SegmentGroup11>(segData, errorCollector), errorCollector);

			var valid = runValidation(new[] { validDSI, validARR1, validARR2 });
			Assert.That(valid, Is.EqualTo(true));

			valid = runValidation(new[] { validARR1, validARR2 });
			Assert.That(valid, Is.EqualTo(false));
		}

		[Test]
		public void Validate_Group11_DSI()
		{
			var validSeg = "DSI+ZAR";
			var invalidSeg = "DSI+GBP";

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.Validate_Group11_DSI(EdifactValidationTestHelper.PrepareMsgSection(segData, new DSISegmentMessageSection(9999), errorCollector), errorCollector);

			var valid = runValidation(null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'DSI' message section missing"));

			valid = runValidation(new[] { validSeg, validSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'DSI' message section"));

			valid = runValidation(new[] { invalidSeg });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("DataSetIdentifier should be 'ZAR'"));

			valid = runValidation(new[] { validSeg });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_Group11_ARR()
		{
			var validSeg = "ARR+AUD+0.12345";
			var validSeg2 = "ARR+JPY+7.45676";
			var invalidSeg_NoCurrency = "ARR++15.8765";
			var invalidSeg_NoRate = "ARR+GBP+";
			var invalidSeg_InvalidRate = "ARR+USD+BadRate";

			Func<string[], bool> runValidation = (string[] segData) => GesmesValidator.Validate_Group11_ARR(EdifactValidationTestHelper.PrepareMsgSection(segData, new ARRSegmentMessageSection(9999), errorCollector), errorCollector);

			var valid = runValidation(null);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'ARR' message section missing"));

			valid = runValidation(new[] { invalidSeg_NoCurrency });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Exchange rate currency is required"));

			valid = runValidation(new[] { invalidSeg_NoRate });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Could not convert rate '' to decimal for 'GBP'"));

			valid = runValidation(new[] { invalidSeg_InvalidRate });
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Could not convert rate 'BadRate' to decimal for 'USD'"));

			valid = runValidation(new[] { validSeg, validSeg2 });
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Validate_UNT()
		{
			var validSeg = "UNT+23+1";
			var invalidSeg = "UNT++9";

			Func<string[], int, bool> runValidation = (string[] segData, int grp11Count) => GesmesValidator.Validate_UNT(EdifactValidationTestHelper.PrepareMsgSection(segData, new UNTSegmentMessageSection(9999), errorCollector), errorCollector, grp11Count);

			var valid = runValidation(null, 0);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 'UNT' message section missing"));

			valid = runValidation(new[] { validSeg, validSeg }, 0);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Expected 1 occurrence(s) of 'UNT' message section"));

			valid = runValidation(new[] { invalidSeg }, 0);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Number of segments in message could not be read"));

			valid = runValidation(new[] { validSeg }, 12);
			Assert.That(valid, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Contains.Substring("Message is expected to have 18 exchange rates but 12 were identified"));

			valid = runValidation(new[] { validSeg }, 18);
			Assert.That(valid, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			errorCollector = new StringBuilder();
		}
		StringBuilder errorCollector;
	}
}
