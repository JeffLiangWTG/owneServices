using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbTimeAllocation))]
	sealed class GlbTimeAllocationTest : GlbStaffResourceTimeTest
	{
		public void TestRelatedItemTypeDescription()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			GlbTimeAllocation time = Factory.New<GlbTimeAllocation>();
			AssertEquals("", time.RelatedItemTypeDescription);

			time.GA_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals(DataBoundResourceStrings.GetTableDescriptiveName(OrgHeaderSchema.Constants.TableName), time.RelatedItemTypeDescription);
		}

		public void TestLookups()
		{
			GlbTimeAllocation time = Factory.New<GlbTimeAllocation>();
			AssertEquals(typeof(GlbTimeAllocationLookups), time.Lookups.GetType());
		}
	}
}
