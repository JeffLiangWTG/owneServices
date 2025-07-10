using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsSupportingDocumentTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(supportingDocument.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsSupportingDocumentCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(supportingDocument.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.SupportingDocumentType));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(supportingDocument.DataSource, Is.EqualTo(Constants.UccDataSources.SupportingDocuments));
		}

		protected override List<(string, string)> ExpectedAttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.Y),
			(Constants.AttributeNames.ItemNumber, Constants.AttributeValues.N),
			(Constants.AttributeNames.Complement, Constants.AttributeValues.N),
			};

		[SetUp]
		public void Setup()
		{
			supportingDocument = GetNctsCodeListDetails() as NctsSupportingDocument;
		}
		NctsSupportingDocument supportingDocument;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsSupportingDocument();
	}
}
