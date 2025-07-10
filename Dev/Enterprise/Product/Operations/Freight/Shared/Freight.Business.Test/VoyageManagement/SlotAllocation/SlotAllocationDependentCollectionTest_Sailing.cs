using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SlotAllocationDependentCollection))]
	sealed class SlotAllocationDependentCollectionTest_Sailing : SlotAllocationDependentCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			return voyage.Sailings[0].SlotAllocations;
		}

		protected override string TableCode
		{
			get { return JobSailingSchema.Constants.Prefix; }
		}

		#endregion
	}
}
