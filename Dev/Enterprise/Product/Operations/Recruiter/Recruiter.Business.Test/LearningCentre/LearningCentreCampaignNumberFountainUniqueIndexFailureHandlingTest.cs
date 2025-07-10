using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LearningCentreCampaignNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => typeof(LearningCentreCampaign);

		protected override SchemaColumn ColumnThatUsesNumberFountain => GlbCompanyCampaignSchema.G0_CampaignID;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.CertificateExamCampaignID;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			var campaign = testBizO as LearningCentreCampaign;
			campaign.G0_CampaignName = "Some campaign";
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var result = base.AdditionalInsertValues;
				result.Add(GlbCompanyCampaignSchema.Constants.G0_CampaignName, $"'Some other campaign'");
				result.Add(GlbCompanyCampaignSchema.Constants.G0_GC, $"'{GlbCompany.CurrentCompany.PK}'");
				return result;
			}
		}
	}
}
