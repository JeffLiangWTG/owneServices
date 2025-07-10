using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Telematics.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffTelematicsBusinessObjectLookupProviderTest : TestCaseWithFactory
	{
		public void TestCode()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, Env.CurrentUser.PK);
			AssertEquals("Code should match current user initials", Env.CurrentUser.Initials, telematicsBizo.Code);
		}

		public void TestDescriptionForInterface()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, Env.CurrentUser.PK);
			AssertEquals("Description (for interface) should match current user full name", Env.CurrentUser.FullName, telematicsBizo.DescriptionForInterface);
		}

		public void TestDescriptionInEnglish()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, Env.CurrentUser.PK);
			AssertEquals("Description (in English) should match current user full name", Env.CurrentUser.FullName, telematicsBizo.DescriptionInEnglish);
		}

		public void TestTypeIdentifier()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, Env.CurrentUser.PK);
			AssertEquals("Staff", telematicsBizo.TypeIdentifier);
		}

		public void TestAssignmentOfNonexistentStaff()
		{
			var telematicsBizo = telematicsProvider.GetBusinessObject(Factory, ZGuid.Missing);
			AssertEquals(null, telematicsBizo);
		}

		ITelematicsBusinessObjectLookupProvider telematicsProvider;

		protected override void SetUp()
		{
			base.SetUp();

			telematicsProvider = new GlbStaffTelematicsBusinessObjectLookupProvider();
		}
	}
}
