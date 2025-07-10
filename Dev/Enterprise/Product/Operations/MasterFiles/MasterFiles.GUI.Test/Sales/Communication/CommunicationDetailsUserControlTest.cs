using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BasherForm))]
	sealed class CommunicationDetailsUserControlTest : ZFormBasherTest
	{
		public void TestSetupLinkedInquiryControls()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			communication.LinkedInquiry = null;
			using (var form = new ZForm(communication))
			using (var control = new CommunicationDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("ClientGuidFindBox.Visible", true, control.ClientGuidFindBox.Visible);
					AssertEquals("ContactGuidDropEdit.Visible", true, control.ContactGuidDropEdit.Visible);
					AssertEquals("ClientTextBox.Visible", false, control.ClientTextBox.Visible);
					AssertEquals("ContactTextBox.Visible", false, control.ContactTextBox.Visible);
				});

				communication.LinkedInquiry = inquiry;
				CombineAssertions(() =>
				{
					AssertEquals("ClientGuidFindBox.Visible", false, control.ClientGuidFindBox.Visible);
					AssertEquals("ContactGuidDropEdit.Visible", false, control.ContactGuidDropEdit.Visible);
					AssertEquals("ClientTextBox.Visible", true, control.ClientTextBox.Visible);
					AssertEquals("ContactTextBox.Visible", true, control.ContactTextBox.Visible);
				});
			}
		}

		public void TestCalendarIntegrationLabel()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_CallDate = default;
			using (var form = new ZForm(communication))
			using (var control = new CommunicationDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("LastInvitationTimeLabel color should be green.", Color.Green, control.LastInvitationTimeLabel.ForeColor);
				AssertEquals("RegistryCalendarIntegrationLabel color should be red.", Color.Red, control.RegistryCalendarIntegrationLabel.ForeColor);
				AssertEquals("LastInvitationTimeLabel should be always visible.", true, control.LastInvitationTimeLabel.Visible);
				AssertEquals("RegistryCalendarIntegrationLabel should be invisible.", false, control.RegistryCalendarIntegrationLabel.Visible);

				Env.Registry.CalendarIntegration = true;
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "123@wtg.com";
				communication.OQ_GS_NKSalesRep = staff.GS_Code;
				Factory.Save();
				control.SendInvitationButton.PerformClick();

				AssertEquals("LastInvitationTimeLabel should be always visible.", true, control.LastInvitationTimeLabel.Visible);
				AssertEquals("RegistryCalendarIntegrationLabel should be invisible.", false, control.RegistryCalendarIntegrationLabel.Visible);

				Env.Registry.CalendarIntegration = false;
				staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "456@wtg.com";
				communication.OQ_GS_NKSalesRep = staff.GS_Code;
				Factory.Save();
				control.SendInvitationButton.PerformClick();

				AssertEquals("LastInvitationTimeLabel should be always visible.", true, control.LastInvitationTimeLabel.Visible);
				AssertEquals("RegistryCalendarIntegrationLabel should be visible.", true, control.RegistryCalendarIntegrationLabel.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			return new BasherForm(communication);
		}

		internal sealed class BasherForm : ZForm
		{
			internal BasherForm(OrgSalesCall communication)
				: base(communication)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = ControlDpiScalingHelper.NewScaledSize(1100, 768, true);

				var control = new CommunicationDetailsUserControl();
				Controls.Add(control);
				BindingSource.SetBindingMember(control, ".");
				CaptionRenderingEnabled = true;
			}
		}

		#endregion
	}
}
