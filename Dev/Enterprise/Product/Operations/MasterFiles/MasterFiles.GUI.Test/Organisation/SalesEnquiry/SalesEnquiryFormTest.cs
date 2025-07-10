using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesEnquiryForm))]
	sealed class SalesEnquiryFormTest : ZFormBasherTest
	{
		[GuiTest]
		public override void TestMinimumSizeNotTooBig()
		{
			using (Form testForm = GetFormToBash())
			{
				int minScreenWidthSupported = ControlDpiScalingHelper.ScaleToCurrentDpiX(1260);
				int minScreenHeightSupported = ControlDpiScalingHelper.ScaleToCurrentDpiY(768);
				int typicalTaskbarHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

				int maxSizeWidth = minScreenWidthSupported;
				int maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert(
					"Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(),
testForm.MinimumSize.Width <= maxSizeWidth);
				Assert(
					"Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(),
					testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		public void TestPromptUserForSave_ConcurrencyErrorOnSave()
		{
			using (var testForm = (SalesEnquiryForm)GetFormToBashCore())
			{
				Factory.RefreshEnabled = false;

				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;

				var enquiryFactory2 = factory2.Load<SalesEnquiry>(Enquiry.PK);

				AssertNotNull("Pre-condition", enquiryFactory2);

				enquiryFactory2.O1_CompanyName = "Test Name1";
				factory2.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				Enquiry.O1_CompanyName = "Test Name2";
				var result = testForm.PromptUserForSave();

				AssertContains("Merge Warning Message should be shown on ZSaveConcurrencyException.", "While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("PromptUserForSave() should return DialogResult.Cancel on ZSaveConcurrencyException.", DialogResult.Cancel, result);
				AssertEquals("Name was changed in factory2 first, so factory1 should take those changes.", "Test Name1", Enquiry.O1_CompanyName);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				Enquiry.O1_CompanyName = "Test Name2";
				result = testForm.PromptUserForSave();

				AssertContains("Changes only made in 1 factory, Merge Warning Message shouldn't be shown", "You cannot proceed until the inquiry has been saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("PromptUserForSave() should return DialogResult.OK when theres no ZSaveConcurrencyException.", DialogResult.OK, result);
				AssertEquals("Name should be changed normally.", "Test Name2", Enquiry.O1_CompanyName);
			}
		}

		#region Implementation

		OrgHeader Org;
		SalesEnquiry Enquiry;

		protected override Form GetFormToBashCore()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.OH_FullName = "Some SalesEnquiry Org";
			Org.MainAddress.OA_Address1 = "1 Street";
			Org.OH_RL_NKClosestPort = "AUSYD";

			Enquiry = Factory.New<SalesEnquiry>();
			Enquiry.OrgPk = Org.PK;
			Enquiry.O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
			Enquiry.O1_ContactName = "Test Contact";
			Enquiry.O1_CompanyName = "Test Name";

			Factory.Save();

			var form = new SalesEnquiryForm(Enquiry);
			form.ControllerID = ControllerIDs.SalesEnquiry;
			return form;
		}

		#endregion
	}
}
