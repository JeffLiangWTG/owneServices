using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TypeSafePackingGroupTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			AssertEquals(typeof(PackingGroupValidation), packingGroup.Validation.GetType());
		}

		public void TestPackage()
		{
			AssertEquals(typeof(PackingGroupLookups), packingGroup.Lookups.GetType());
		}

		JobDeclaration declaration;
		PackingGroup packingGroup;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			packingGroup = declaration.PackingGroups.AddNew();
		}
	}
}
