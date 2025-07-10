using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRCampaignEmailTemplateEditor))]
	sealed class HRCampaignEmailTemplateEditorTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			return new HRCampaignEmailTemplateEditor(campaign);
		}
	}
}
