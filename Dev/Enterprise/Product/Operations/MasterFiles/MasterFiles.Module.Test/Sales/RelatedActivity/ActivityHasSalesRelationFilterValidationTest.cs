using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ActivityHasSalesRelationFilterValidationTest : HasSalesRelationFilterValidationTest
	{
		public void TestValidateTypeProperty()
		{
			var filter = new ActivityHasSalesRelationFilter(Factory, "Dummy", typeof(OrgOpportunity));
			var validation = new ActivityHasSalesRelationFilterValidation(filter);

			filter.TypeProperty = "";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);

			filter.TypeProperty = "XXX";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, true);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);
		}

		public void TestValidateBizObjPK()
		{
			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var filter = new ActivityHasSalesRelationFilter(Factory, "Dummy", typeof(OrgOpportunity));
			var validation = new ActivityHasSalesRelationFilterValidation(filter);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			filter.BizObjPK = campaign.PK;
			validation.ValidateBizObjPK();
			AssertListValidationInvalidCodeError(filter.BizObjPKInfo, false);

			filter.TypeProperty = RelatableActivityTypeList.Codes.CampaignManagement;
			filter.BizObjPK = Guid.NewGuid();
			validation.ValidateBizObjPK();
			AssertListValidationInvalidCodeError(filter.BizObjPKInfo, true);

			filter.TypeProperty = RelatableActivityTypeList.Codes.CampaignManagement;
			filter.BizObjPK = campaign.PK;
			validation.ValidateBizObjPK();
			AssertListValidationInvalidCodeError(filter.BizObjPKInfo, false);

			filter.TypeProperty = RelatableActivityTypeList.Codes.OpportunityManager;
			filter.BizObjPK = opportunity.PK;
			validation.ValidateBizObjPK();
			AssertListValidationInvalidCodeError(filter.BizObjPKInfo, false);

			filter.TypeProperty = "ZJD";
			filter.BizObjPK = Guid.NewGuid();
			validation.ValidateBizObjPK();
			AssertListValidationInvalidCodeError(filter.BizObjPKInfo, false);

			filter.TypeProperty = ZString.Empty;
			filter.BizObjPK = Guid.NewGuid();
			validation.ValidateBizObjPK();
			AssertListValidationInvalidCodeError(filter.BizObjPKInfo, false);
		}
	}
}
