using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRHiringRequestForm))]
	public class HRHiringRequestFormTest : ZFormBasherTest
	{
		public void TestNoEdocs()
		{
			using var form = GetFormToBash();
			Assert("Disallow any viewing or editing of edocs through CW1, users must use HRMS and its appropriate entity security", !form.PlugIns.IsPlugInAvailable(ControllerIDs.eDocsPlugIn));
		}

		protected new HRHiringRequestForm GetFormToBash()
			=> (HRHiringRequestForm)base.GetFormToBash();

		protected override Form GetFormToBashCore()
		{
			var hiringRequest = Factory.New<HRHiringRequest>();
			var jobApplicant = Factory.New<HRJobApplicant>();
			jobApplicant.HA_FullName = "dummy name";
			jobApplicant.HA_EmailAddress = "dummy@email.com";
			hiringRequest.HRR_HA_JobApplicant = jobApplicant.PK;
			return new HRHiringRequestForm(hiringRequest);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get
			{
				return true;
			}
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		public void TestControlsReadOnly()
		{
			var hiringRequest = Factory.NewWithValidTestData<HRHiringRequest>();
			using (var form = new HRHiringRequestFormForTest(hiringRequest))
			{
				form.Show();
				AssertCollectionNotContains(false, form.GuidFindBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
				AssertCollectionNotContains(false, form.CodeFindBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
				AssertCollectionNotContains(false, form.ZTextBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
			}
		}

		public void TestControlsVisibility()
		{
			var hiringRequest = Factory.NewWithValidTestData<HRHiringRequest>();
			using (var form = new HRHiringRequestFormForTest(hiringRequest))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("ApplicantGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("TeamCodeFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("WorkingBasisTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("WorkLocCountryCodeFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("StartDateTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("EndDateTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("StatusTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("JobTitleTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("ReportingMgrCodeFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("WorkLocCityTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("ProbDurationTextBox", true).Single().Visible);
			}
		}

		class HRHiringRequestFormForTest : HRHiringRequestForm
		{
			public HRHiringRequestFormForTest(HRHiringRequest hiringRequest) : base(hiringRequest)
			{
			}

			internal TabControl MainTabControlExposed
			{
				get
				{
					return MainTabControl;
				}
			}

			internal IReadOnlyList<ZGuidFindBox> GuidFindBoxControlsExposed
			{
				get
				{
					return new List<ZGuidFindBox>()
					{ ApplicantGuidFindBox };
				}
			}

			internal IReadOnlyList<ZCodeFindBox> CodeFindBoxControlsExposed
			{
				get
				{
					return new List<ZCodeFindBox>()
					{ TeamCodeFindBox, WorkLocCountryCodeFindBox, ReportingMgrCodeFindBox };
				}
			}

			internal IReadOnlyList<ZTextBox> ZTextBoxControlsExposed
			{
				get
				{
					return new List<ZTextBox>()
					{ WorkingBasisTextBox, StartDateTextBox, EndDateTextBox, StatusTextBox, JobTitleTextBox, WorkLocCityTextBox, ProbDurationTextBox };
				}
			}
		}
	}
}
