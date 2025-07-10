using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignContactController))]
	public class TestGlbCompanyCampaignContactController : ZControllerBasherTest
	{
		#region TestViewForm

		public override void TestViewForm()
		{
			Assert("TestViewForm needs to be tested for each CampaignContact type instead of just one", true);
		}

		[SnailTest]
		public void TestViewForm_OrgContact()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetOrgContactInDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.Organisation, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_InquiryContact()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetInquiryContactInDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.SalesEnquiry, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		#endregion

		#region TestEditForm

		public override void TestEditForm()
		{
			Assert("TestEditForm needs to be tested for each CampaignContact type instead of just one", true);
		}

		[SnailTest]
		public void TestEditForm_OrgContact()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetOrgContactInDatabase());
				AssertEquals(ControllerIDs.Organisation, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{ ((ZForm)testForm).Close(); }
			}
		}

		[SnailTest]
		public void TestEditForm_InquiryContact()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetInquiryContactInDatabase());
				AssertEquals(ControllerIDs.SalesEnquiry, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{ ((ZForm)testForm).Close(); }
			}
		}

		#endregion

		public override void TestNewForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show new form", null, Controller.ShowNewForm());
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override void TestDeleteForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show delete form", null, Controller.ShowDeleteForm(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
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
			return typeof(CampaignContact);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompanyCampaignContact;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new NotSupportedException("This controller can contain two different business objects. All tests that use this method should be overrided, and tested for each business object type");
		}

		CampaignContact GetOrgContactInDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();

			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(campaign);
			collection.Load(new ZQuery(ViewCampaignContactSchema.PK, contact.PK));
			return collection[0];
		}

		CampaignContact GetInquiryContactInDatabase()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(campaign);
			collection.Load(new ZQuery(ViewCampaignContactSchema.PK, inquiry.PK));
			return collection[0];
		}

		#endregion
	}
}
