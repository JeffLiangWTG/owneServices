using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	internal class RefCusRateApplicabilityUOMFixture
	{
		[Test]
		public void Create()
		{
			var uom = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_UOM = "KG"
			};
			var result = new RefCusRateApplicabilityUOM(uom, new RefCusRateApplicability(safeRepository));
			Assert.IsTrue(uom.RefCusRateApplicabilityUOMs.Contains(result));
			Assert.AreEqual(uom, result.RefCusRateUOM);
			Assert.AreEqual("KG", result.S02_UOM);
		}

		[Test]
		public void Link()
		{
			var uom = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_UOM = "KG"
			};
			var result = new RefCusRateApplicabilityUOM(new RefCusRateApplicability(safeRepository));
			result.Link(uom);
			Assert.AreEqual(uom, result.RefCusRateUOM);
			Assert.IsTrue(uom.RefCusRateApplicabilityUOMs.Contains(result));
		}

		[Test]
		public void Update()
		{
			var uom = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid()
			};
			var result = new RefCusRateApplicabilityUOM(new RefCusRateApplicability(safeRepository)) { S02_UOM = "KG" };
			result.Link(uom);
			Assert.AreNotEqual("KG", uom.ZXG_UOM);
			result.Update();
			Assert.AreEqual("KG", uom.ZXG_UOM);
		}

		[Test]
		public void Unlink()
		{
			var uom = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_UOM = "KG"
			};
			var result = new RefCusRateApplicabilityUOM(uom, new RefCusRateApplicability(safeRepository));
			var uomResult = result.Unlink();
			Assert.That(uomResult, Is.Not.Empty);
			Assert.AreEqual(uom, uomResult.First());
			Assert.IsNull(result.RefCusRateUOM);
			Assert.IsFalse(uom.RefCusRateApplicabilityUOMs.Contains(result));
		}

		[Test]
		public void TopLevelObject()
		{
			var rateApp = new RefCusRateApplicability(safeRepository) { S01_PK = Guid.NewGuid() };
			var uom = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_UOM = "KG"
			};
			var result = new RefCusRateApplicabilityUOM(uom, rateApp);
			Assert.That(rateApp, Is.EqualTo(result.TopLevelNonPersistentObjects.First()));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
