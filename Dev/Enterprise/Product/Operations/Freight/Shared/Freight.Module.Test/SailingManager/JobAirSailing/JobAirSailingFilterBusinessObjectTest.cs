using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobAirSailingFilterBusinessObject))]
	sealed class JobAirSailingFilterBusinessObjectTest : JobSailingFilterBusinessObjectTest
	{
		#region TestFlightNoFilter

		public void TestFlightNoFilter()
		{
			JobAirSailingFilterBusinessObject filter = new JobAirSailingFilterBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			((ModuleTextFilter)filter["Flight No."]).Property = "";
			((ModuleTextFilter)filter["Flight No."]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Flight No."]).Property = Voyage1.JV_VoyageFlight;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Flight No."]).Property = Voyage1.JV_VoyageFlight.Left(1);

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Flight No."]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Flight No."]).Property = Voyage2.JV_VoyageFlight;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Flight No."]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			((ModuleTextFilter)filter["Flight No."]).Property = ZString.Empty;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Flight No."]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));
		}

		#endregion

		#region Test Flight Status

		public void TestFlightStatus()
		{
			Sailing1.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched;

			Sailing2.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;

			Factory.Save();

			JobAirSailingFilterBusinessObject filter = new JobAirSailingFilterBusinessObject();

			((ModuleTextFilter)filter["Flight Status"]).Property = Constants.FlightScheduleStatus.Matched;
			((ModuleTextFilter)filter["Flight Status"]).IsActive = true;
			sailings.Load(filter.Filter);

			CombineAssertions("Filtering JobSailings for 'Flight Status' = Matched", () =>
			{
				Assert("Sailing1 should be in collection as it has a Matched ('MTD') Flight Status.", sailings.Contains(Sailing1.PK));
				Assert("Sailing2 should not be in collection as it does not have a Matched ('MTD') Flight Status.", !sailings.Contains(Sailing2.PK));
			});

			((ModuleTextFilter)filter["Flight Status"]).Property = Constants.FlightScheduleStatus.PartiallyMatched;
			((ModuleTextFilter)filter["Flight Status"]).IsActive = true;
			sailings.Load(filter.Filter);

			CombineAssertions("Filtering JobSailings for 'Flight Status' = PMD", () =>
			{
				Assert("Sailing1 should not be in collection as it does not have a PartiallyMatched ('PMD') Flight Status.", !sailings.Contains(Sailing1.PK));
				Assert("Sailing2 should be in collection as it has a PartiallyMatched ('PMD') Flight Status.", sailings.Contains(Sailing2.PK));
			});
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobAirSailingFilterBusinessObject();
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		#endregion
	}
}
