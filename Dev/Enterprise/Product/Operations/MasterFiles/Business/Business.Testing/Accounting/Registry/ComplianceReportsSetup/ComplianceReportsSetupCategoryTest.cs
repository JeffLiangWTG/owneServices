using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportsSetupCategory))]
	class ComplianceReportsSetupCategoryTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateCategory()
		{
			AssertNoErrors("Precondition: Category should not have errors.", BizObj.CategoryInfo);

			BizObj.Category = "";
			AssertHasError(BizObj.CategoryInfo, "Please enter a Category Code.");
			BizObj.Category = "ABC";
			AssertNoErrors(BizObj.CategoryInfo);
		}

		public void TestSequence()
		{
			AssertEquals(0m, BizObj.Sequence);
			BizObj.Sequence = -1m;
			AssertEquals(0m, BizObj.Sequence);

			BizObj.Sequence = 11m;
			AssertEquals(11m, BizObj.Sequence);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Category = "";
			BizObj.ClearAllNotifications();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.CategoryInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ComplianceReportsSetupCategory result = new ComplianceReportsSetupCategory
			{
				Category = "A01",
				CategoryDescription = "Test Desc",
				Sequence = 1m,
				AllowDuplicate = true
			};
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceReportsSetupCategory();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ComplianceReportsSetupCategory BizObj
		{
			get { return (ComplianceReportsSetupCategory)base.BizObj; }
		}

		#endregion

	}
}
