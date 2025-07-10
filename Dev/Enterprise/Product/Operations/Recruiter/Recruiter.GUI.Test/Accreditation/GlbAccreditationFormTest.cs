using System;
using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(GlbAccreditationForm))]
	public class GlbAccreditationFormTest : ZFormBasherTest
	{
		public void TestActionsMenuIsSaved()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			using (var form = new GlbAccreditationFormForTest(accreditation))
			{
				form.Show();
				var accreditationUpdaterItem = form.ActionsMenuItemExposed.MenuItems.FindByText("Create Accreditation Attempts for Existing Exam Attempts", true);
				accreditationUpdaterItem.PerformClick();
				AssertEquals("Should show error since accreditation is unsaved.", FormattableString.Invariant($"Cannot create Accreditation Attempts until {accreditation.HumanReadableName} is saved."), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new GlbAccreditationForm(Factory.New<GlbAccreditation>());
		}

		class GlbAccreditationFormForTest : GlbAccreditationForm
		{
			public GlbAccreditationFormForTest(GlbAccreditation accreditation) : base(accreditation)
			{
			}

			public MenuItem ActionsMenuItemExposed => ActionsMenuItem;
		}
		#endregion
	}
}
