using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CusMAWBValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCM_GB()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "different company");
			var otherBranch = cusMawb.Branch.Company.Branches.AddNew();
			cusMawb.CM_GB = otherBranch.PK;
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "different company");
			cusMawb.CM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;
			AssertHasErrorContaining(cusMawb.CM_GBInfo, "different company");
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "enter");
			cusMawb.CM_GB = ZGuid.Empty;
			AssertHasErrorContaining(cusMawb.CM_GBInfo, "enter");
		}

		public void TestValidateCM_GBForSavedCusMAWB()
		{
			var cusMawb = Factory.New<CusMAWB>();

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "~C1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "~B1";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var cusMAWBLoaded = new BusinessObjectFactory().Load<CusMAWB>(cusMawb.PK);

				cusMAWBLoaded.CM_GB = cusMAWBLoaded.CM_GB;//not changing
				AssertNoErrors(cusMAWBLoaded.CM_GBInfo);

				cusMAWBLoaded.CM_GB = branch.PK;
				AssertHasErrorContaining(cusMAWBLoaded.CM_GBInfo, "different company");
			}
		}

		public void TestCheckFilter()
		{
			var cusMAWB = GetNewMAWB();

			cusMAWB.CustomsCargoStatusFilter = "~~~";
			AssertHasMessageErrorContaining(cusMAWB.CustomsCargoStatusFilterInfo, ListValidation.InvalidCodeMessageError);

			cusMAWB.CustomsCargoStatusFilter = GetValidCustomsCargoStatus(cusMAWB);
			AssertNoMessageErrorContaining(cusMAWB.CustomsCargoStatusFilterInfo, ListValidation.InvalidCodeMessageError);

			cusMAWB.CustomsMessageStatusFilter = "~~~";
			AssertHasMessageErrorContaining(cusMAWB.CustomsMessageStatusFilterInfo, ListValidation.InvalidCodeMessageError);

			cusMAWB.CustomsMessageStatusFilter = GetValidCustomsMessageStatus(cusMAWB);
			AssertNoMessageErrorContaining(cusMAWB.CustomsMessageStatusFilterInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation
		protected virtual CusMAWB GetNewMAWB()
		{
			return Factory.New<CusMAWB>();
		}

		protected virtual string GetValidCustomsCargoStatus(CusMAWB mawb)
		{
			var list = mawb.Lookups.CustomsCargoStatusList;
			return list.Count > 0 ? list[0].Code : "";
		}

		protected virtual string GetValidCustomsMessageStatus(CusMAWB mawb)
		{
			var list = mawb.Lookups.CustomsMessageStatusList;
			return list.Count > 0 ? list[0].Code : "";
		}

		#endregion
	}
}
