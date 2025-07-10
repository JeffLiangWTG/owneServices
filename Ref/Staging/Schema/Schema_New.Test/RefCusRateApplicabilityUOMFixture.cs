using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusRateApplicabilityUOMFixture
	{
		[Test]
		public void Constructor()
		{
			var uom = new RefCusRateUOM()
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_ZZ2_Rate = Guid.NewGuid(),
				ZXG_UOM = "AAA",
			};

			var rateAppUOM = new RefCusRateApplicabilityUOM(uom);

			Assert.That(rateAppUOM.S02_UOM == uom.ZXG_UOM);
		}
	}
}
