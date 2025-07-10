using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsPreviousDocumentUnionGoodsTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsPreviousDocumentUnionGoodsCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.PreviousDocumentUnionGoods));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.PreviousDocumentUnionGoods));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsPreviousDocumentUnionGoods;
		}
		NctsPreviousDocumentUnionGoods declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsPreviousDocumentUnionGoods();
	}
}
