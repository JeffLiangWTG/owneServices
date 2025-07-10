using System;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class DeclarableGoodsNomenclatureFilterFixture
	{
		[Test]
		public void IsValid()
		{
			var dec1 = new declarableGoodsNomenclature();
			var dec2 = new declarableGoodsNomenclature { dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(10) };
			var dec3 = new declarableGoodsNomenclature { dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(-10) };

			var filter = new DeclarableGoodsNomenclatureFilter(new string[] {});
			Assert.IsTrue(filter.IsValid(dec1, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(dec2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(dec3, DateTime.UtcNow));
		}

		[Test]
		public void IsValid_FilterGoodsNomeclature()
		{
			var dec1 = new declarableGoodsNomenclature { goodsNomenclatureCode = "0111204512" };
			var dec2 = new declarableGoodsNomenclature { goodsNomenclatureCode = "0112304500" };
			var filter_GoodsNomenclature = new[] { "0111" };
			var filter = new DeclarableGoodsNomenclatureFilter(filter_GoodsNomenclature);
			Assert.IsTrue(filter.IsValid(dec1, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(dec2, DateTime.UtcNow));
		}
	}
}
