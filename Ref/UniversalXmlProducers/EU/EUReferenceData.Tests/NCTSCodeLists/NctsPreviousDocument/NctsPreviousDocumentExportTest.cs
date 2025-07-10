using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsPreviousDocumentExportTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(PreviousDocument.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsPreviousDocumentCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(PreviousDocument.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.PreviousDocumentExportType));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(PreviousDocument.DataSource, Is.EqualTo("EUN DC40N Previous Document"));
		}

		protected override List<(string, string)> ExpectedAttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			};


		protected override string ExpectedExtraType => Constants.UccConstants.ExportExtraType;

		[SetUp]
		public void Setup()
		{
			PreviousDocument = GetNctsCodeListDetails() as NctsPreviousDocumentExportType;
		}
		NctsPreviousDocumentExportType PreviousDocument;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsPreviousDocumentExportType();
	}
}
