using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BranchManagementCodeDescriptionBool))]
	sealed class BranchManagementCodeDescriptionBoolTest : CodeDescriptionBoolTest
	{
		public void TestIsReadOnly()
		{
			var code1 = (BranchManagementCodeDescriptionBool)GetNewBusinessObject();
			code1.CodeMaxLength = 3;
			code1.Code = "ABC";
			code1.Description = (NoResString)"DescABC";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			AssertEquals("Code is not in use", false, code1.IsReadOnly);

			var code2 = (BranchManagementCodeDescriptionBool)GetNewBusinessObject();
			code2.CodeMaxLength = 3;
			code2.Code = "XYZ";
			code2.Description = (NoResString)"DescXYZ";

			branch.GB_AccountingGroupCode = "XYZ";
			Factory.Save();
			AssertEquals("Code is in use", true, code2.IsReadOnly);
		}

		public new void TestReadOnlyStates()
		{
			TestIsReadOnly();
		}

		public override void TestCanDelete()
		{
			var code1 = (BranchManagementCodeDescriptionBool)GetNewBusinessObject();
			code1.CodeMaxLength = 3;
			code1.Code = "ABC";
			code1.Description = (NoResString)"DescABC";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			var canDelete = (ICanDelete)code1;
			AssertEquals("CanDelete", true, canDelete.CanDelete);

			var code2 = (BranchManagementCodeDescriptionBool)GetNewBusinessObject();
			code2.CodeMaxLength = 3;
			code2.Code = "XYZ";
			code2.Description = (NoResString)"DescXYZ";

			branch.GB_AccountingGroupCode = "XYZ";
			Factory.Save();
			canDelete = code2;
			AssertEquals("CanDelete", false, canDelete.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This group code is in use on some branches and cannot be deleted.", canDelete.ReasonForNotAbleToDelete);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			var clone = clone1 as BranchManagementCodeDescriptionBool;
			AssertEquals("Code", "COD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 3, clone.CodeMaxLength);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (BranchManagementCodeDescriptionBool)GetNewBusinessObject();

			result.CodeMaxLength = 3;
			result.Code = "COD";
			result.Description = (NoResString)"DescriptionA";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
