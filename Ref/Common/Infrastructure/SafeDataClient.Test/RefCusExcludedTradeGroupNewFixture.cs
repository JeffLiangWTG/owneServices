using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	public class RefCusExcludedTradeGroupNewFixture
	{
		[Test]
		public void Create()
		{
			var ex = new RefCusExcludedTradeGroup
			{
				ZZC_ZZA_TradeGroup = Guid.NewGuid(),
				ZZC_PK = Guid.NewGuid()
			};
			var newEx = new RefCusExcludedTradeGroupNew(ex, new RefCusRateApplicability(safeRepository));
			Assert.AreEqual(ex, newEx.RefCusExcludedTradeGroup);
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(newEx));
			Assert.AreEqual(ex.ZZC_ZZA_TradeGroup, newEx.S03_ZZA_TradeGroup);
		}

		[Test]
		public void Link()
		{
			var ex = new RefCusExcludedTradeGroup
			{
				ZZC_ZZA_TradeGroup = Guid.NewGuid(),
				ZZC_PK = Guid.NewGuid()
			};
			var newEx = new RefCusExcludedTradeGroupNew(new RefCusRateApplicability(safeRepository));
			newEx.Link(ex);
			Assert.AreEqual(ex, newEx.RefCusExcludedTradeGroup);
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(newEx));
		}

		[Test]
		public void Update()
		{
			var ex = new RefCusExcludedTradeGroup
			{
				ZZC_ZZA_TradeGroup = Guid.NewGuid(),
				ZZC_PK = Guid.NewGuid()
			};
			var newEx = new RefCusExcludedTradeGroupNew(new RefCusRateApplicability(safeRepository)) { S03_ZZA_TradeGroup = Guid.NewGuid() };
			newEx.Link(ex);
			Assert.AreNotEqual(ex.ZZC_ZZA_TradeGroup, newEx.S03_ZZA_TradeGroup);
			newEx.Update();
			Assert.AreEqual(ex.ZZC_ZZA_TradeGroup, newEx.S03_ZZA_TradeGroup);
		}

		[Test]
		public void UnLink()
		{
			var ex = new RefCusExcludedTradeGroup
			{
				ZZC_ZZA_TradeGroup = Guid.NewGuid(),
				ZZC_PK = Guid.NewGuid()
			};
			var newEx = new RefCusExcludedTradeGroupNew(ex, new RefCusRateApplicability(safeRepository));
			var result = newEx.Unlink();
			Assert.That(result, Is.Not.Empty);
			Assert.AreEqual(ex, result.First());
			Assert.IsNull(newEx.RefCusExcludedTradeGroup);
			Assert.IsFalse(ex.RefCusExcludedTradeGroupNews.Contains(newEx));
		}

		[Test]
		public void TopLevelObject()
		{
			var rateApp = new RefCusRateApplicability(safeRepository) { S01_PK = Guid.NewGuid() };
			var ex = new RefCusExcludedTradeGroup
			{
				ZZC_ZZA_TradeGroup = Guid.NewGuid(),
				ZZC_PK = Guid.NewGuid()
			};
			var newEx = new RefCusExcludedTradeGroupNew(ex, rateApp);
			Assert.That(rateApp, Is.EqualTo(newEx.TopLevelNonPersistentObjects.First()));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
