using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffChangeRequestLookups))]
	public class GlbStaffChangeRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChangeRequestTemplates()
		{
			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			AssertNotNull("Change Request Templates should not be null", changeRequest.Lookups.ChangeRequestTemplates);
		}
	}
}
