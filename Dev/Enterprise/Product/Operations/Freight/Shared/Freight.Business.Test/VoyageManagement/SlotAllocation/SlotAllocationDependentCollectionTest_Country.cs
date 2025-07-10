using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SlotAllocationDependentCollection))]
	sealed class SlotAllocationDependentCollectionTest_Country : SlotAllocationDependentCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.CurrentCountry.SlotAllocations;
		}

		protected override string TableCode
		{
			get { return JobVoyCountrySchema.Constants.Prefix; }
		}

		#endregion
	}
}
