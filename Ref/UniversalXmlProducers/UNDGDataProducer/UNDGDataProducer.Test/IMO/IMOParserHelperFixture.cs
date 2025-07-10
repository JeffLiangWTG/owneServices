using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class IMOParserHelperFixture
	{
		[Test]
		public void SetEQData()
		{
			var attributes = new List<UNDGAttribute>();
			var result = new UNDGSubstance();
			result.SetEQData("See SP340", attributes);

			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0].DA_Descriptor, Is.EqualTo(""));
			Assert.That(attributes[0].DA_Index, Is.EqualTo("340"));
			Assert.That(attributes[0].DA_Type, Is.EqualTo("SPP"));
			Assert.That(result.DG_ExceptedQuantityCode, Is.EqualTo(null));
			attributes = new List<UNDGAttribute>();
			result.SetEQData("E0", attributes);
			Assert.That(result.DG_ExceptedQuantityCode, Is.EqualTo("E0"));
			Assert.That(attributes, Is.Empty);
		}

		[Test]
		public void SetLQData()
		{
			var attributes = new List<UNDGAttribute>();
			var result = new UNDGSubstance();
			result.SetLQData("See SP340", attributes);

			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0].DA_Descriptor, Is.EqualTo(""));
			Assert.That(attributes[0].DA_Index, Is.EqualTo("340"));
			Assert.That(attributes[0].DA_Type, Is.EqualTo("SPP"));
			Assert.That(result.DG_LQ2OrPaxMaxAmt, Is.EqualTo(0));
			Assert.That(result.DG_LQ2OrPaxMaxAmtType, Is.EqualTo(null));
			Assert.That(result.DG_LQ2OrPaxMaxAmtUQ, Is.EqualTo(null));
			Assert.That(result.DG_LQMaxAmt, Is.EqualTo(0));
			Assert.That(result.DG_LQMaxAmtType, Is.EqualTo(null));
			Assert.That(result.DG_LQMaxAmtUQ, Is.EqualTo(null));

			attributes = new List<UNDGAttribute>();
			result.SetLQData("10 kg or 5 L", attributes);
			Assert.That(attributes, Is.Empty);
			Assert.That(result.DG_LQ2OrPaxMaxAmt, Is.EqualTo(5));
			Assert.That(result.DG_LQ2OrPaxMaxAmtType, Is.EqualTo(Constants.AmtTypes.NetWeightLimitAmtType));
			Assert.That(result.DG_LQ2OrPaxMaxAmtUQ, Is.EqualTo("L"));
			Assert.That(result.DG_LQMaxAmt, Is.EqualTo(10));
			Assert.That(result.DG_LQMaxAmtType, Is.EqualTo(Constants.AmtTypes.NetWeightLimitAmtType));
			Assert.That(result.DG_LQMaxAmtUQ, Is.EqualTo("kg"));

			result = new UNDGSubstance();
			result.SetLQData("10 kg", attributes);
			Assert.That(attributes, Is.Empty);
			Assert.That(result.DG_LQ2OrPaxMaxAmt, Is.EqualTo(0));
			Assert.That(result.DG_LQ2OrPaxMaxAmtType, Is.EqualTo(null));
			Assert.That(result.DG_LQ2OrPaxMaxAmtUQ, Is.EqualTo(null));
			Assert.That(result.DG_LQMaxAmt, Is.EqualTo(10));
			Assert.That(result.DG_LQMaxAmtType, Is.EqualTo(Constants.AmtTypes.NetWeightLimitAmtType));
			Assert.That(result.DG_LQMaxAmtUQ, Is.EqualTo("kg"));
		}

		[Test]
		public void CreateAttributesStowSegSpecProv_Stow()
		{
			var stow = "SW10 SGG20 NOT";
			var expectedResult = new List<UNDGAttribute>()
			{
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "10",
					DA_Type = "CPV"
				},
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "SGG20",
					DA_Type = "SGG"
				},
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "NOT",
					DA_Type = "NVL"
				},
			};

			var attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesStowSegSpecProv(stowSegRecords, specProvRecords, stow, string.Empty, string.Empty, attributes);
			Assert.That(attributes, Has.Count.EqualTo(3));

			for (var x = 0; x < 3; x++)
			{
				Assert.That(attributes[x].DA_Descriptor, Is.EqualTo(expectedResult[x].DA_Descriptor));
				Assert.That(attributes[x].DA_Index, Is.EqualTo(expectedResult[x].DA_Index));
				Assert.That(attributes[x].DA_Type, Is.EqualTo(expectedResult[x].DA_Type));
			}
		}

		[Test]
		public void CreateAttributesStowSegSpecProv_Seg()
		{
			var seg = "H20 SGG20 SP30";
			var expectedResult = new List<UNDGAttribute>()
			{
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "20",
					DA_Type = "DLG"
				},
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "SGG20",
					DA_Type = "SGG"
				},
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "30",
					DA_Type = "DLG"
				},
			};

			var attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesStowSegSpecProv(stowSegRecords, specProvRecords, string.Empty, seg, string.Empty, attributes);
			Assert.That(attributes, Has.Count.EqualTo(3));

			for (var x = 0; x < 3; x++)
			{
				Assert.That(attributes[x].DA_Descriptor, Is.EqualTo(expectedResult[x].DA_Descriptor));
				Assert.That(attributes[x].DA_Index, Is.EqualTo(expectedResult[x].DA_Index));
				Assert.That(attributes[x].DA_Type, Is.EqualTo(expectedResult[x].DA_Type));
			}
		}

		[Test]
		public void CreateAttributesStowSegSpecProv_SpecProv()
		{
			var specProv = "20 30";
			var expectedResult = new List<UNDGAttribute>()
			{
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "20",
					DA_Type = "SPP"
				},
				new UNDGAttribute
				{
					DA_Descriptor = "",
					DA_Index = "30",
					DA_Type = "SPP"
				},
			};

			var attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesStowSegSpecProv(stowSegRecords, specProvRecords, string.Empty, string.Empty, specProv, attributes);
			Assert.That(attributes, Has.Count.EqualTo(2));

			for (var x = 0; x < 2; x++)
			{
				Assert.That(attributes[x].DA_Descriptor, Is.EqualTo(expectedResult[x].DA_Descriptor));
				Assert.That(attributes[x].DA_Index, Is.EqualTo(expectedResult[x].DA_Index));
				Assert.That(attributes[x].DA_Language, Is.EqualTo(string.IsNullOrEmpty(expectedResult[x].DA_Descriptor) ? string.Empty : "EN"));
				Assert.That(attributes[x].DA_Type, Is.EqualTo(expectedResult[x].DA_Type));
			}
		}

		[Test]
		public void CreateAttributesProperty()
		{
			var attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesProperty(propertyRecords, "0004", "a", attributes);
			Assert.That(attributes, Has.Count.EqualTo(1));

			var expectedResult = new UNDGAttribute
			{
				DA_Descriptor = "0004a Substance.",
				DA_Index = "0",
				DA_Type = "PRP"
			};
			Assert.That(attributes[0].DA_Descriptor, Is.EqualTo(expectedResult.DA_Descriptor));
			Assert.That(attributes[0].DA_Index, Is.EqualTo(expectedResult.DA_Index));
			Assert.That(attributes[0].DA_Type, Is.EqualTo(expectedResult.DA_Type));

			attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesProperty(propertyRecords, "0005", "a", attributes);
			Assert.That(attributes, Has.Count.EqualTo(0));
		}

		[Test]
		public void CreateAttributesQualifyingDescriptiveText()
		{
			var attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesQualifyingDescriptiveText(qualifyingDescriptiveTextRecords, "0004a", attributes);
			Assert.That(attributes, Has.Count.EqualTo(1));

			var expectedResult = new UNDGAttribute
			{
				DA_Descriptor = "0004a Substance.",
				DA_Index = "0",
				DA_Type = "QDT"
			};
			Assert.That(attributes[0].DA_Descriptor, Is.EqualTo(expectedResult.DA_Descriptor));
			Assert.That(attributes[0].DA_Index, Is.EqualTo(expectedResult.DA_Index));
			Assert.That(attributes[0].DA_Type, Is.EqualTo(expectedResult.DA_Type));

			attributes = new List<UNDGAttribute>();
			IMOParserHelper.CreateAttributesQualifyingDescriptiveText(qualifyingDescriptiveTextRecords, "0005a", attributes);
			Assert.That(attributes, Has.Count.EqualTo(0));
		}

		IEnumerable<StowSegRecord> stowSegRecords;
		IEnumerable<SpecProvRecord> specProvRecords;
		IEnumerable<PropertyRecord> propertyRecords;
		IEnumerable<QualifyingDescriptiveTextRecord> qualifyingDescriptiveTextRecords;

		[SetUp]
		public void SetUp()
		{
			stowSegRecords = new List<StowSegRecord>() {
					new StowSegRecord { DGLPhrase = "9", DGLText = "SW101 Should not be a match for SW10" },
					new StowSegRecord { DGLPhrase = "10", DGLText = "SW10 Ten" },
					new StowSegRecord { DGLPhrase = "20", DGLText = "H20 Twenty" },
					new StowSegRecord { DGLPhrase = "30", DGLText = "SP30 Thirty" },
			};

			specProvRecords = new List<SpecProvRecord>() {
					new SpecProvRecord { SPNo = "10", SPText = "SW10 Ten" },
					new SpecProvRecord { SPNo = "20", SPText = "H20 Twenty" },
					new SpecProvRecord { SPNo = "30", SPText = "SP30 Thirty" },
			};

			propertyRecords = new List<PropertyRecord>() {
					new PropertyRecord { UNNO = "0004", Variant = "a", PropObs = "0004a Substance." },
					new PropertyRecord { UNNO = "0004", Variant = "b", PropObs = "0004b Substance." },
					new PropertyRecord { UNNO = "0005", Variant = "", PropObs = "0005 Substance." },
			};

			qualifyingDescriptiveTextRecords = new List<QualifyingDescriptiveTextRecord>() {
					new QualifyingDescriptiveTextRecord { UnId = "0004a", QualifyingDescriptiveText = "0004a Substance." },
					new QualifyingDescriptiveTextRecord { UnId = "0004b", QualifyingDescriptiveText = "0004b Substance." },
					new QualifyingDescriptiveTextRecord { UnId = "0005", QualifyingDescriptiveText = "0005 Substance." },
			};
		}
	}
}
