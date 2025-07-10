using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class NX5105Commodity_WineAbstractTest<TCommodityWine> : TestCaseWithFactory
		where TCommodityWine : NX5105CommodityWine
	{
		[ExpectNoExceptions]
		public void TestAlcoholContentNumeric()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_AlcoholPercentage = 12;
			IWine wine = (TCommodityWine)Activator.CreateInstance(typeof(TCommodityWine), invoiceLine);
			NUnit.Framework.Assert.That(wine.AlcoholContentNumeric, NUnit.Framework.Is.EqualTo(12m).Using(CustomComparers.TypeComparison), "Wine.AlcoholContentNumeric should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			var exception = AssertExceptionThrown<TargetInvocationException>(() =>
			{
				IWine wine = (TCommodityWine)Activator.CreateInstance(typeof(TCommodityWine), (JobComInvoiceLine)null);
			});
			NUnit.Framework.Assert.That(exception.InnerException, NUnit.Framework.Is.TypeOf<ArgumentNullException>());

			AssertNoExceptionThrown(() =>
			{
				var line = Factory.New<JobComInvoiceLine>();
				IWine wine = (TCommodityWine)Activator.CreateInstance(typeof(TCommodityWine), line);
			}

			);
		}

		[ExpectNoExceptions]
		public virtual void TestCheckNotApplicableProperties()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			IWine wine = (TCommodityWine)Activator.CreateInstance(typeof(TCommodityWine), invoiceLine);
			NUnit.Framework.Assert.That(wine.AgeNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wine.BottledDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(wine.CoverLotNumberAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wine.GeographicRegion, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wine.OriginalNonLotNumberAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wine.ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(wine.ProductExpiryDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(wine.RemoveLotNumberAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wine.YearNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));
		}
	}
}
