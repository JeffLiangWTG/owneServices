using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsPreviousDocumentTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(previousDocument.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsPreviousDocumentCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(previousDocument.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.PreviousDocumentType));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(previousDocument.DataSource, Is.EqualTo("EUN DC40N Previous Document"));
		}

		protected override List<(string, string)> ExpectedAttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.Y),
			(Constants.AttributeNames.ItemNumber, Constants.AttributeValues.N),
			(Constants.AttributeNames.Complement, Constants.AttributeValues.N),
			};

		[SetUp]
		public void Setup()
		{
			previousDocument = GetNctsCodeListDetails() as NctsPreviousDocumentType;
		}
		NctsPreviousDocumentType previousDocument;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsPreviousDocumentType();
	}
}
