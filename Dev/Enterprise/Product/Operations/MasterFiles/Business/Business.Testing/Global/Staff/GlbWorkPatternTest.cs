using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbWorkPattern))]
	public class GlbWorkPatternTest : EnterpriseBusinessObjectTestCase
	{
		public void TestWorkTimes()
		{
			var pattern = Factory.New<GlbWorkPattern>();

			var worktime1 = Factory.New<GlbWorkTime>();
			worktime1.GW_ParentID = pattern.PK;
			worktime1.GW_ParentTableCode = GlbWorkPatternSchema.Constants.Prefix;

			var worktime2 = Factory.New<GlbWorkTime>();
			worktime2.GW_ParentID = pattern.PK;
			worktime2.GW_ParentTableCode = GlbWorkPatternSchema.Constants.Prefix;

			var worktime3 = Factory.New<GlbWorkTime>();
			worktime3.GW_ParentID = ZGuid.NewZGuid();
			worktime3.GW_ParentTableCode = GlbWorkPatternSchema.Constants.Prefix;

			var worktime4 = Factory.New<GlbWorkTime>();
			worktime4.GW_ParentID = ZGuid.NewZGuid();
			worktime4.GW_ParentTableCode = "AA";

			AssertNotNull(pattern.WorkTimes);
			AssertEquals(pattern.WorkTimes.Count, 2);
			AssertEquals(pattern.WorkTimes[0], worktime1);
			AssertEquals(pattern.WorkTimes[1], worktime2);
		}

		public void TestWorkTimes_Empty()
		{
			var pattern = Factory.New<GlbWorkPattern>();
			AssertNotNull(pattern.WorkTimes);
			AssertEquals(pattern.WorkTimes.Count, 0);
		}
	}
}
