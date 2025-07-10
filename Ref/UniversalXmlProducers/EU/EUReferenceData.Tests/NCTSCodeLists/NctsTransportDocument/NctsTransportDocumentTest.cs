using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsTransportDocumentTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(transportDocument.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsTransportDocumentCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(transportDocument.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.TransportDocumentType));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(transportDocument.DataSource, Is.EqualTo(Constants.UccDataSources.TransportDocuments));
		}

		protected override List<(string, string)> ExpectedAttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.Y),
			};

		[SetUp]
		public void Setup()
		{
			transportDocument = GetNctsCodeListDetails() as NctsTransportDocument;
		}
		NctsTransportDocument transportDocument;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsTransportDocument();
	}
}
