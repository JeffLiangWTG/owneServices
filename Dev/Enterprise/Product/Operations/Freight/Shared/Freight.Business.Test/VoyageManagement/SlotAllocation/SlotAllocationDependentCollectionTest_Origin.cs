using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SlotAllocationDependentCollection))]
	sealed class SlotAllocationDependentCollectionTest_Origin : SlotAllocationDependentCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Origins.AddNew().SlotAllocations;
		}

		protected override string TableCode
		{
			get { return JobVoyOriginSchema.Constants.Prefix; }
		}

		#endregion
	}
}
