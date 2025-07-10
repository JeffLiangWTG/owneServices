using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsAdditionalInformationTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(additionalInformation.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsAdditionalInfoCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(additionalInformation.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.AdditionalInformation));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(additionalInformation.DataSource, Is.EqualTo(Constants.UccDataSources.AdditionalInformation));
		}

		protected override List<(string, string)> ExpectedAttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Description, Constants.AttributeValues.N),
			};

		[SetUp]
		public void Setup()
		{
			additionalInformation = GetNctsCodeListDetails() as NctsAdditionalInformation;
		}
		NctsAdditionalInformation additionalInformation;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsAdditionalInformation();
	}
}
