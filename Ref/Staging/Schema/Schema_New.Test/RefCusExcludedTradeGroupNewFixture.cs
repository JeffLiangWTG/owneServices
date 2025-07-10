using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusExcludedTradeGroupNewFixture
	{
		[Test]
		public void Constructor()
		{
			var excludedTradeGroup = new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZT_Applicability = Guid.NewGuid(),
				ZZC_ZZA_NKTradeGroup = "AAA",
				ZZC_ZZA_ZZZ_NKDataGrouping = "BBB"
			};
			var excludedTradeGroupNew = new RefCusExcludedTradeGroupNew(excludedTradeGroup);

			Assert.That(excludedTradeGroupNew.S03_ZZA_NKTradeGroup == excludedTradeGroup.ZZC_ZZA_NKTradeGroup);
			Assert.That(excludedTradeGroupNew.S03_ZZA_ZZZ_NKDataGrouping == excludedTradeGroup.ZZC_ZZA_ZZZ_NKDataGrouping);
		}
	}
}
