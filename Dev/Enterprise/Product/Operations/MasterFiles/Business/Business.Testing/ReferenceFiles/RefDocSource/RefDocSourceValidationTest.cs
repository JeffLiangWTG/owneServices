using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefDocSourceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRDS_Desc()
		{
			DocSource.RDS_Desc = ZString.Empty;
			Assert("Expecting RDS_Desc to be empty and have errors.", DocSource.RDS_DescInfo.HasErrors());
			DocSource.RDS_Desc = "asd";
			Assert("Expecting RDS_Desc to have too few characters and have errors.", DocSource.RDS_DescInfo.HasErrors());
			DocSource.RDS_Desc = "Australia, Dollars";
			Assert("RDS_Desc should be correct, not expecting errors.", !DocSource.RDS_DescInfo.HasNotifications());
		}

		public void TestCheckRDS_Code()
		{
			DocSource.RDS_Code = ZString.Empty;
			Assert("Expecting RDS_Code to be empty and have errors.", DocSource.RDS_CodeInfo.HasErrors());
			DocSource.RDS_Code = "ET";
			Assert("Expecting RDS_Code to have too few characters and have errors.", DocSource.RDS_CodeInfo.HasErrors());
			DocSource.RDS_Code = "MSC";
			Assert("RDS_Code should be correct, not expecting errors.", !DocSource.RDS_CodeInfo.HasErrors());
			DocSource.RDS_Code = "ETO";
			Assert("RDS_Code should be correct, not expecting errors.", !DocSource.RDS_CodeInfo.HasNotifications());
			DocSource.RDS_Code = "A:B";
			Assert("Expecting DocSource to have errors because it contains a :.", DocSource.RDS_CodeInfo.HasErrors());
		}

		public void TestIsUniqueAndNotInAllCategory()
		{
			DocSource.RDS_Code = "MSC";
			Assert(DocSource.Validation.IsUnique());

			DocSource.RDS_Code = "ZZZ";
			Assert(DocSource.Validation.IsUnique());

			RefDocSource newDocSource = Factory.New(typeof(RefDocSource)) as RefDocSource;
			newDocSource.RDS_Code = "AAA";
			Assert(newDocSource.Validation.IsUnique());

			RefDocSource anotherNewDocSource = Factory.New(typeof(RefDocSource)) as RefDocSource;
			anotherNewDocSource.RDS_Code = "AAA";
			Assert(!anotherNewDocSource.Validation.IsUnique());
		}

		#region Implementation

		RefDocSource DocSource;

		protected override void SetUp()
		{
			base.SetUp();
			DocSource = Factory.New(typeof(RefDocSource)) as RefDocSource;
		}

		#endregion
	}
}
