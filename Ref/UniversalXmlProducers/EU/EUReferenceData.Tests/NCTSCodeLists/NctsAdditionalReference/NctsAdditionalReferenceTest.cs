using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsAdditionalReferenceTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(additionalReference.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsAdditionalReferenceCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(additionalReference.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.AdditionalReference));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(additionalReference.DataSource, Is.EqualTo(Constants.UccDataSources.AdditionalReference));
		}

		protected override List<(string, string)> ExpectedAttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.N),
			};

		[SetUp]
		public void Setup()
		{
			additionalReference = GetNctsCodeListDetails() as NctsAdditionalReference;
		}
		NctsAdditionalReference additionalReference;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsAdditionalReference();
	}
}
