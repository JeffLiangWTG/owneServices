using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobRailSailingFilterBusinessObject))]
	sealed class JobRailSailingFilterBusinessObjectTest : JobSailingFilterBusinessObjectTest
	{
		#region TestJourneyNameFilter

		public void TestJourneyNameFilter()
		{
			JobSailing[] sailings;
			ModuleTextFilter filter = (ModuleTextFilter)Strip[JobRailSailingFilterBusinessObject.Descriptions.JourneyName];

			filter.Property = "";
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionContains(Sailing2, sailings);

			filter.Property = Voyage1.JV_RV_NKVessel;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionNotContains(Sailing2, sailings);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionNotContains(Sailing2, sailings);

			filter.Property = ZString.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionNotContains(Sailing1, sailings);
			AssertCollectionNotContains(Sailing2, sailings);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionContains(Sailing2, sailings);
		}

		#endregion

		#region TestJourneyNoFilter

		public void TestJourneyNoFilter()
		{
			JobSailing[] sailings;
			ModuleTextFilter filter = (ModuleTextFilter)Strip[JobRailSailingFilterBusinessObject.Descriptions.JourneyNo];

			filter.Property = "";
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionContains(Sailing2, sailings);

			filter.Property = Voyage1.JV_VoyageFlight;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionNotContains(Sailing2, sailings);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionNotContains(Sailing2, sailings);

			filter.Property = ZString.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionNotContains(Sailing1, sailings);
			AssertCollectionNotContains(Sailing2, sailings);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			sailings = Factory.Load<JobSailing>(GetSubGroupQuery(filter));
			AssertCollectionContains(Sailing1, sailings);
			AssertCollectionContains(Sailing2, sailings);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobRailSailingFilterBusinessObject();
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Rail; }
		}

		#endregion
	}
}
