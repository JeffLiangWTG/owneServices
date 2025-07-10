using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaPackPackedItemPivotCollection))]
	sealed class AsycudaPackPackedItemPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackPackedItemPivotCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var pack = Factory.New<AsycudaPack>();
			return new AsycudaPackPackedItemPivotCollection(pack);
		}
	}
}
