using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.HRM.GUI.Testing
{
	[TestedType(typeof(ReviewProcessForm))]

	public class ReviewProcessFormTest : ZFormBasherTest
	{
		public void TestNoEdocs()
		{
			using var form = GetFormToBash();
			Assert("Disallow any viewing or editing of edocs through CW1, users must use HRMS and its appropriate entity security", !form.PlugIns.IsPlugInAvailable(ControllerIDs.eDocsPlugIn));
		}

		protected new ReviewProcessForm GetFormToBash()
			=> (ReviewProcessForm)base.GetFormToBash();

		protected override Form GetFormToBashCore()
		{
			var reviewProcess = Factory.New<ReviewProcess>();
			return new ReviewProcessForm(reviewProcess);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		public void TestControlsReadOnly()
		{
			var reviewProcess = Factory.NewWithValidTestData<ReviewProcess>();
			using (var form = new ReviewProcessFormForTest(reviewProcess))
			{
				form.Show();
				AssertCollectionNotContains(false, form.GuidFindBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
				AssertCollectionNotContains(false, form.CodeFindBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
				AssertCollectionNotContains(false, form.ZTextBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
			}
		}

		public void TestControlsVisibility()
		{
			var reviewProcess = Factory.NewWithValidTestData<ReviewProcess>();
			using (var form = new ReviewProcessFormForTest(reviewProcess))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("NameTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("TypeTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("StatusTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("PrimaryHierarchyTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("OverrideHierarchyTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("CurrencyFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("EffectiveDateEdit", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("SubmissionDateEdit", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("EmployeesInReviewFindBox", true).Single().Visible);
			}
		}

		class ReviewProcessFormForTest : ReviewProcessForm
		{
			public ReviewProcessFormForTest(ReviewProcess reviewProcess)
				: base(reviewProcess)
			{
			}

			internal TabControl MainTabControlExposed
			{
				get { return MainTabControl; }
			}

			internal IReadOnlyList<ZGuidFindBox> GuidFindBoxControlsExposed
			{
				get { return new List<ZGuidFindBox>() { EmployeesInReviewFindBox }; }
			}

			internal IReadOnlyList<ZCodeFindBox> CodeFindBoxControlsExposed
			{
				get { return new List<ZCodeFindBox>() { CurrencyFindBox }; }
			}

			internal IReadOnlyList<ZTextBox> ZTextBoxControlsExposed
			{
				get { return new List<ZTextBox>() { NameTextBox, TypeTextBox, StatusTextBox, PrimaryHierarchyTextBox, OverrideHierarchyTextBox }; }
			}

			internal IReadOnlyList<ZDateEdit> ZDateEditControlsExposed
			{
				get { return new List<ZDateEdit>() { EffectiveDateEdit }; }
			}

			internal IReadOnlyList<ZDateTimeOffsetEdit> ZDateTimeOffsetEditControlsExposed
			{
				get { return new List<ZDateTimeOffsetEdit>() { SubmissionDateEdit }; }
			}
		}
	}
}
