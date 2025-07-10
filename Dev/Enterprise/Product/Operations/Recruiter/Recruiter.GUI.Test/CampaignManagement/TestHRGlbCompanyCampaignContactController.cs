using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaignContactController))]
	public class TestHRGlbCompanyCampaignContactController : ZControllerBasherTest
	{
		#region TestViewForm
		public override void TestViewForm()
		{
			Assert("TestViewForm needs to be tested for each CampaignContact type instead of just one", true);
		}

		[SnailTest]
		public void TestViewForm_GlbStaff()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetGlbStaffInDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.GlbStaff, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_HRJobApplicant()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetHRJobApplicantInDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.HRJobApplicant, form.ControllerID);
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
		public void TestEditForm_GlbStaff()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetGlbStaffInDatabase());
				AssertEquals(ControllerIDs.GlbStaff, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{
					((ZForm)testForm).Close();
				}
			}
		}

		[SnailTest]
		public void TestEditForm_HRJobApplicant()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetHRJobApplicantInDatabase());
				AssertEquals(ControllerIDs.HRJobApplicant, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{
					((ZForm)testForm).Close();
				}
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
			return ControllerIDs.HRGlbCompanyCampaignContact;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new NotSupportedException("This controller can contain two different business objects. All tests that use this method should be overrided, and tested for each business object type");
		}

		CampaignContact GetGlbStaffInDatabase()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			var collection = new GlbCampaignContactCollection(campaign);
			collection.Load(new ZQuery(ViewCampaignContactSchema.PK, staff.PK));
			return collection[0];
		}

		CampaignContact GetHRJobApplicantInDatabase()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			var collection = new GlbCampaignContactCollection(campaign);
			collection.Load(new ZQuery(ViewCampaignContactSchema.PK, applicant.PK));
			return collection[0];
		}
		#endregion
	}
}
