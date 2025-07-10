using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignClickController))]
	public class GlbCompanyCampaignClickControllerTest : ZControllerBasherTest
	{
		public override void TestViewForm()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetCampaignClickInDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.GlbCompanyCampaignClick, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestTemplateCopyForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show template copy form", null, Controller.ShowTemplateCopyForm(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(GlbCompanyCampaignClick);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompanyCampaignClick;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new NotSupportedException("This controller can contain two different business objects. All tests that use this method should be overrided, and tested for each business object type");
		}

		GlbCompanyCampaignClick GetCampaignClickInDatabase()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();

			var link = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link.GCL_G0_Campaign = campaign.PK;
			link.GCL_Context = "New Context";
			link.GCL_URL = "http://org.net";

			var campaignItemClickStatModel = new CampaignItemClickStatModel(campaignItem);
			var campaignClick = campaignItemClickStatModel.CampaignClickCollection.AddNew();
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;
			campaignClick.GCC_GCL = link.PK;

			Factory.Save();

			return campaignClick;
		}

		#endregion
	}
}
