using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class LocalCartageJobOrgValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOrgTypeAndUsageCommentAreUnique()
		{
			CommonCartageType jobType = Factory.New<CommonCartageType>();
			CommonCartageOrg org1 = jobType.CommonCartageOrganisations.AddNew();
			CommonCartageOrg org2 = jobType.CommonCartageOrganisations.AddNew();

			org1.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CFS;
			org2.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CFS;
			org1.E5_UsageComment = "";
			org2.E5_UsageComment = "";

			AssertHasError(org2.E5_OrgTypeInfo, LocalCartageJobOrgValidation.UniqueOrgTypeAndUsageCommentError);
			AssertHasError(org1.E5_UsageCommentInfo, LocalCartageJobOrgValidation.UniqueOrgTypeAndUsageCommentError);
			AssertNoErrors(org1.E5_OrgTypeInfo);
			AssertNoErrors(org2.E5_UsageCommentInfo);

			org2.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CNR;
			org1.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CTO;
			org2.E5_UsageComment = "xtir1";
			org1.E5_UsageComment = "xtir2";

			AssertNoErrors(org1.E5_OrgTypeInfo);
			AssertNoErrors(org2.E5_OrgTypeInfo);
			AssertNoErrors(org1.E5_UsageCommentInfo);
			AssertNoErrors(org2.E5_UsageCommentInfo);
		}
	}
}
