using System;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountWaveProcessTaskCollection))]
	public class WhsCycleCountWaveProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsCycleCountWaveProcessTaskCollection>
	{
		#region Overrides

		protected override WhsCycleCountWaveProcessTaskCollection GetCollectionToTestCore()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			return new WhsCycleCountWaveProcessTaskCollection(wave);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsCycleCountWaveProcessTaskCollection);
		}

		#endregion

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;
	}
}
