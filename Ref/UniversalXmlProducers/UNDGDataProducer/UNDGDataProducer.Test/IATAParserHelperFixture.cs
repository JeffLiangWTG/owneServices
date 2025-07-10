using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class IATAParserHelperFixture
	{
		[Test]
		public void LoadAttributes()
		{
			var dgIataRecord = new IATARecord
			{
				QualifyingDescriptiveText = "test",
				OtherNames = "name1;name2"
			};

			var result = IATAParserHelper.LoadAttributes(dgIataRecord);
			Assert.That(result, Has.Length.EqualTo(3));
			var firstAttr = result.First();
			Assert.That(firstAttr.DA_Index, Is.EqualTo("0"));
			Assert.That(firstAttr.DA_Type, Is.EqualTo(Constants.AttributeTypes.QualifyingDescriptive));
			Assert.That(firstAttr.DA_Descriptor, Is.EqualTo("test"));

			var secondAttr = result[1];
			Assert.That(secondAttr.DA_Index, Is.EqualTo("0"));
			Assert.That(secondAttr.DA_Type, Is.EqualTo(Constants.AttributeTypes.OtherNames));
			Assert.That(secondAttr.DA_Descriptor, Is.EqualTo("name1"));

			var thirdAttr = result.Last();
			Assert.That(thirdAttr.DA_Index, Is.EqualTo("1"));
			Assert.That(thirdAttr.DA_Type, Is.EqualTo(Constants.AttributeTypes.OtherNames));
			Assert.That(thirdAttr.DA_Descriptor, Is.EqualTo("name2"));
		}

		[Test]
		public void CreateIATAVariant()
		{
			var substances = new[]
			{
				new UNDGSubstance() { DG_UNNO = "1010", DG_UniqueRecordId = "1" },
				new UNDGSubstance() { DG_UNNO = "1010", DG_UniqueRecordId = "2" },
				new UNDGSubstance() { DG_UNNO = "1010", DG_UniqueRecordId = "3" },
				new UNDGSubstance() { DG_UNNO = "2020", DG_UniqueRecordId = "6" },
				new UNDGSubstance() { DG_UNNO = "2020", DG_UniqueRecordId = "4" },
				new UNDGSubstance() { DG_UNNO = "2020", DG_UniqueRecordId = "5" },
				new UNDGSubstance() { DG_UNNO = "3030", DG_UniqueRecordId = "6" },
				new UNDGSubstance() { DG_UNNO = "4040", DG_UniqueRecordId = "7" },
			};

			IATAParserHelper.CreateIATAVariant(substances);

			Assert.AreEqual("a", substances[0].DG_Variant);
			Assert.AreEqual("b", substances[1].DG_Variant);
			Assert.AreEqual("c", substances[2].DG_Variant);
			Assert.AreEqual("c", substances[3].DG_Variant);
			Assert.AreEqual("a", substances[4].DG_Variant);
			Assert.AreEqual("b", substances[5].DG_Variant);
			Assert.AreEqual("", substances[6].DG_Variant);
			Assert.AreEqual("", substances[7].DG_Variant);
		}
	}
}
