using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module
{
	[TestedType(typeof(HRGlbCompanyCampaignFilterBusinessObject))]
	public class HRGlbCompanyCampaignFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCategoryFilter_Description()
		{
			var filterBizo1 = new HRGlbCompanyCampaignFilterBusinessObject();
			AssertEquals(OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.DefaultValue, filterBizo1["Category 1"].MultilingualDescription);
			AssertEquals(OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.DefaultValue, filterBizo1["Category 2"].MultilingualDescription);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My HR Campaign Category 1");
			OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My HR Campaign Category 2");
			var filterBizo2 = new HRGlbCompanyCampaignFilterBusinessObject();
			AssertEquals("My HR Campaign Category 1", filterBizo2["Category 1"].MultilingualDescription);
			AssertEquals("My HR Campaign Category 2", filterBizo2["Category 2"].MultilingualDescription);
		}

		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRGlbCompanyCampaignFilterBusinessObject();
		}
		#endregion
	}
}
