using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobRoadSailingFilterBusinessObject))]
	sealed class JobRoadSailingFilterBusinessObjectTest : JobSailingFilterBusinessObjectTest
	{
		#region TestTruckRegFilter

		public void TestTruckRegFilter()
		{
			JobSailing[] sailings;
			ModuleTextFilter filter = (ModuleTextFilter)Strip[JobRoadSailingFilterBusinessObject.Descriptions.TruckRef];

			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains("Expect sailing 1 to be in collection", Sailing1, sailings);
			AssertCollectionContains("Expect sailing 2 to be in collection", Sailing2, sailings);

			filter.Property = Voyage1.JV_VoyageFlight;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains("Expect sailing 1 to be in collection", Sailing1, sailings);
			AssertCollectionNotContains("Expect sailing 2 not to be in collection", Sailing2, sailings);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains("Expect sailing 1 to be in collection", Sailing1, sailings);
			AssertCollectionNotContains("Expect sailing 2 not to be in collection", Sailing2, sailings);

			filter.Property = ZString.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionNotContains("Expect sailing 1 not to be in collection", Sailing1, sailings);
			AssertCollectionNotContains("Expect sailing 2 not to be in collection", Sailing2, sailings);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains("Expect sailing 1 to be in collection", Sailing1, sailings);
			AssertCollectionContains("Expect sailing 2 to be in collection", Sailing2, sailings);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobRoadSailingFilterBusinessObject();
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		#endregion
	}
}
