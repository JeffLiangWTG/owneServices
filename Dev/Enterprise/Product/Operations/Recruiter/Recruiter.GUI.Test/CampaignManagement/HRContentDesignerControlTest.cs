using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI.Testing
{
	sealed class HRContentDesignerControlTest : TestCaseWithFactory
	{
		public void TestContactGuidFindBoxVisibility()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Contact Campaign";
			Factory.Save();
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				AssertEquals("Find box shouldn't appear when no data source is selected", false, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				form.ContentControl.DataSourceDropEditExposed.Text = HRContactDataSourceList.Codes.CampaignTracking;
				AssertEquals("Find box shouldn't appear when an invalid data source is selected", false, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				form.ContentControl.DataSourceDropEditExposed.Text = HRContactDataSourceList.Codes.JobApplicant;
				AssertEquals("Find box should appear", true, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				AssertEquals(typeof(HRJobApplicantCollection), form.ContentControl.ContactGuidFindBoxExposed.List.GetType());
				form.ContentControl.DataSourceDropEditExposed.Text = new ZString("blah");
				AssertEquals("Find box should disappear when invalid data source is selected", false, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				form.ContentControl.DataSourceDropEditExposed.Text = HRContactDataSourceList.Codes.Staff;
				AssertEquals("Find box should appear", true, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				AssertEquals(typeof(GlbStaffCollection), form.ContentControl.ContactGuidFindBoxExposed.List.GetType());
			}
		}

		public void TestSimulationContactHRJobApplicant()
		{
			var applicantContact = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			applicantContact.HA_FullName = "Contact AUT";
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Contact Campaign";
			Factory.Save();
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				campaign.TemplateEditor.TemplateHtmlText = form.ContentControl.EmailContentTextBoxExposed.Text = @"<html><head></head><body><p><a href=""http://www.test.gov"" target=""_blank"" tid=""Tester"">Tester</a></p><p>(*ContactName*)</p></body></html>";
				AssertEqualsIgnoreLineBreaks("No macro preview", @"<p><a href=""http://www.test.gov"" target=""_blank"" tid=""Tester"">Tester</a></p>
<p>(*ContactName*)</p>", form.ContentControl.SelectPreviewContentBodyHtml());
				form.ContentControl.DataSourceDropEditExposed.Text = HRContactDataSourceList.Codes.JobApplicant;
				AssertEquals(true, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				form.ContentControl.ContactGuidFindBoxExposed.CodeBox.Text = applicantContact.HA_FullName;
				AssertEquals(applicantContact.PK, form.ContentControl.ContactGuidFindBoxExposed.Guid);
				AssertEquals("TemplateEditor.ApplicantSimulationContactPK", form.ContentControl.ContactGuidFindBoxExposed.GetBindingMember());
				campaign.TemplateEditor.ApplicantSimulationContactPK = applicantContact.PK;
				campaign.TemplateEditor.EnableMacroDataPreview = true;
				AssertEqualsIgnoreLineBreaks("With macro preview", @"<p><a href=""http://www.test.gov"" target=""_blank"" tid=""Tester"">Tester</a></p>
<p>Contact AUT</p>", form.ContentControl.SelectPreviewContentBodyHtml());
			}
		}

		public void TestSimulationContactGlbStaff()
		{
			var staffContact = Factory.NewWithValidTestData<GlbStaff>();
			staffContact.GS_FullName = "Contact AUS";
			staffContact.GS_Code = "CAS";
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Contact Campaign";
			Factory.Save();
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.EmailContentTextBoxExposed.Text = @"<html><head></head><body><p><a href=""http://www.test.gov"" target=""_blank"" tid=""Tester"">Tester</a></p><p>(*ContactName*)</p></body></html>";
				form.ContentControl.EmailContentTextBoxExposed.DataBindings["Text"].WriteValue();
				AssertEqualsIgnoreLineBreaks("No macro preview", @"<p><a href=""http://www.test.gov"" target=""_blank"" tid=""Tester"">Tester</a></p>
<p>(*ContactName*)</p>", form.ContentControl.SelectPreviewContentBodyHtml());
				form.ContentControl.DataSourceDropEditExposed.Text = HRContactDataSourceList.Codes.Staff;
				AssertEquals(true, form.ContentControl.ContactGuidFindBoxExposed.Visible);
				form.ContentControl.ContactGuidFindBoxExposed.CodeBox.Text = staffContact.GS_Code;
				AssertEquals(staffContact.PK, form.ContentControl.ContactGuidFindBoxExposed.Guid);
				AssertEquals("TemplateEditor.StaffSimulationContactPK", form.ContentControl.ContactGuidFindBoxExposed.GetBindingMember());
				campaign.TemplateEditor.StaffSimulationContactPK = staffContact.PK;
				campaign.TemplateEditor.EnableMacroDataPreview = true;
				AssertEqualsIgnoreLineBreaks("With macro preview", @"<p><a href=""http://www.test.gov"" target=""_blank"" tid=""Tester"">Tester</a></p>
<p>Contact AUS</p>", form.ContentControl.SelectPreviewContentBodyHtml());
			}
		}

		HRDummyCampaignForm GetFormForTest(GlbCompanyCampaign campaign)
		{
			return new HRDummyCampaignForm(campaign);
		}

		public class HRDummyCampaignForm : ZForm
		{
			public HRDummyCampaignForm(GlbCompanyCampaign campaign) : base(campaign)
			{
				this.CaptionRenderingEnabled = true;
			}

			public HRContentDesignerControlForTest ContentControl;
			protected override void InitializeComponent()
			{
				ContentControl = new HRContentDesignerControlForTest();
				Controls.Add(ContentControl);
			}
		}

		public class HRContentDesignerControlForTest : HRContentDesignerControl
		{
			public ZTextBox EmailContentTextBoxExposed
			{
				get
				{
#if !WINZOR
					return emailContentTextBox;
#else
					return null;
#endif
				}
			}

			public ZDropEdit DataSourceDropEditExposed
			{
				get
				{
					return DataSourceDropEdit;
				}
			}

			public ZGuidFindBox ContactGuidFindBoxExposed
			{
				get
				{
					return ContactGuidFindBox;
				}
			}

			public HRCampaignEmailTemplateEditor TemplateEditorExposed
			{
				get
				{
					return TemplateEditor;
				}
			}

			public string SelectPreviewContentBodyHtml()
			{
				string fullHtml = GetPreviewContentText();
				return Regex.Match(fullHtml, @"<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
			}

			public string GetPreviewContentText_Exposed()
			{
				return GetPreviewContentText();
			}
		}
	}
}
