using System.Linq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportNonCondensedDeclarationGovernmentAgencyGoodsItemTests : GovernmentAgencyGoodsItemAbstractTests<ExportNonCondensedDeclarationGovernmentAgencyGoodsItem>
	{
		[ExpectNoExceptions]
		public override void TestSequenceNumeric()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public new void TestEntryLineGroup()
		{
			InvoiceLine.JI_Group = "Bicycle Parts";
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.EntryLineGroupForDocument, NUnit.Framework.Is.EqualTo("Bicycle Parts").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalDocuments()
		{
			var assignedJobComInvLineRef = InvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedJobComInvLineRef.JG_ReferenceNumber = "A2111";
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public override void TestCommodity()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.Commodity.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationCommodity)));
		}

		[ExpectNoExceptions]
		public override void TestGoodsMeasure()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.GoodsMeasure.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationGoodsMeasure)));
		}

		[ExpectNoExceptions]
		public override void TestGoodsStatisticalMeasure()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.GoodsStatisticalMeasure.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationGoodsStatisticalMeasure)));
		}

		protected override ExportNonCondensedDeclarationGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => new ExportNonCondensedDeclarationGovernmentAgencyGoodsItem(EntryLine, InvoiceLine, 1);
	}
}
