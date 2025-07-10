using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(WizardForm))]
	public class WizardFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestAddPages()
		{
			using (var wizard = new WizardForm(null))
			{
				AssertEquals(wizard.Pages.Count, 0);
			}

			using (var wizard = new WizardForm(null))
			{
				var step1 = new WizardPageStep();
				var step2 = new WizardPageStep();
				var step3 = new WizardPageStep();

				wizard.Pages.AddRange(new WizardPage[] { step1, step2, step3 });

				wizard.Show();

				AssertEquals(wizard.Pages.Count, 3);
				AssertEquals(wizard.Pages[0], step1);
				AssertEquals(wizard.Pages[1], step2);
				AssertEquals(wizard.Pages[2], step3);
			}
		}

		[ExpectNoExceptions]
		public void TestPageNavigation()
		{
			using (var wizard = new WizardForm(null))
			{
				var startPage = new WizardPageStart();
				var step1 = new WizardPageStep();
				var step2 = new WizardPageStep();
				var step3 = new WizardPageStep();
				var finishPage = new WizardPageFinish();

				wizard.Pages.AddRange(new WizardPage[] { startPage, step1, step2, step3, finishPage });

				wizard.Show();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, startPage);
				AssertEquals(wizard.BackButton.Enabled, false);
				AssertEquals(wizard.BackButton.Visible, true);
				AssertEquals(wizard.NextButton.Enabled && wizard.NextButton.Visible, true);
				AssertEquals(wizard.FinishButton.Visible, false);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);

				wizard.NextButton.PerformClick();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, step1);
				AssertEquals(wizard.BackButton.Enabled && wizard.BackButton.Visible, true);
				AssertEquals(wizard.NextButton.Enabled && wizard.NextButton.Visible, true);
				AssertEquals(wizard.FinishButton.Visible, false);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);

				wizard.NextButton.PerformClick();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, step2);
				AssertEquals(wizard.BackButton.Enabled && wizard.BackButton.Visible, true);
				AssertEquals(wizard.NextButton.Enabled && wizard.NextButton.Visible, true);
				AssertEquals(wizard.FinishButton.Visible, false);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);

				wizard.BackButton.PerformClick();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, step1);
				AssertEquals(wizard.BackButton.Enabled && wizard.BackButton.Visible, true);
				AssertEquals(wizard.NextButton.Enabled && wizard.NextButton.Visible, true);
				AssertEquals(wizard.FinishButton.Visible, false);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);

				wizard.NextButton.PerformClick();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, step2);
				AssertEquals(wizard.BackButton.Enabled && wizard.BackButton.Visible, true);
				AssertEquals(wizard.NextButton.Enabled && wizard.NextButton.Visible, true);
				AssertEquals(wizard.FinishButton.Visible, false);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);

				wizard.NextButton.PerformClick();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, step3);
				AssertEquals(wizard.BackButton.Enabled && wizard.BackButton.Visible, true);
				AssertEquals(wizard.NextButton.Visible, false);
				AssertEquals(wizard.FinishButton.Visible, true);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);

				wizard.FinishButton.PerformClick();

				AssertNotNull(wizard.CurrentPage);
				AssertEquals(wizard.CurrentPage, finishPage);
				AssertEquals(wizard.BackButton.Visible, false);
				AssertEquals(wizard.NextButton.Visible, false);
				AssertEquals(wizard.FinishButton.Visible, false);
				AssertEquals(wizard.CancelWizardButton.Enabled && wizard.CancelWizardButton.Visible, true);
			}
		}

		public void TestWizardFormText()
		{
			using (var wizard = new WizardForm(null))
			{
				var step1 = new WizardPageStep();
				var step2 = new WizardPageStep();
				var step3 = new WizardPageStep();

				wizard.Pages.AddRange(new WizardPage[] { step1, step2, step3 });

				wizard.Show();

				Assert(wizard.Text.Contains("Page 1 of 3"));
				wizard.NextButton.PerformClick();
				Assert(wizard.Text.Contains("Page 2 of 3"));
				wizard.FinishButton.PerformClick();
				Assert(wizard.Text.Contains("Page 3 of 3"));
			}
		}

		public void TestPageUpdatesHeaderText()
		{
			using (var wizard = new WizardForm(null))
			{
				var step1 = new TestStepWizardPage() { HeaderTitle = "Step 1", HeaderDescription = "Step 1 desc" };
				var step2 = new TestStepWizardPage() { HeaderTitle = "Step 2", HeaderDescription = "Step 2 desc" };
				var step3 = new TestStepWizardPage() { HeaderTitle = "Step 3", HeaderDescription = "Step 3 desc" };

				wizard.Pages.AddRange(new WizardPage[] { step1, step2, step3 });

				wizard.Show();

				Assert(wizard.PageHeaderVisible);
				AssertEquals(wizard.PageHeaderTitle, step1.HeaderTitle);
				AssertEquals(wizard.PageHeaderDescription, step1.HeaderDescription);

				wizard.NextButton.PerformClick();

				Assert(wizard.PageHeaderVisible);
				AssertEquals(wizard.PageHeaderTitle, step2.HeaderTitle);
				AssertEquals(wizard.PageHeaderDescription, step2.HeaderDescription);

				wizard.FinishButton.PerformClick();

				Assert(wizard.PageHeaderVisible);
				AssertEquals(wizard.PageHeaderTitle, step3.HeaderTitle);
				AssertEquals(wizard.PageHeaderDescription, step3.HeaderDescription);
			}
		}

		public void TestPageLeaving()
		{
			using (var wizard = new WizardForm(null))
			{
				var startPage = new WizardPageFinish();
				var step1 = new TestStepWizardPage();
				var step2 = new TestStepWizardCancelPage() { AllowForward = false };
				var step3 = new TestStepWizardPage();
				var finishPage = new WizardPageFinish();

				wizard.Pages.AddRange(new WizardPage[] { startPage, step1, step2, step3, finishPage });

				wizard.Show();

				AssertEquals(wizard.CurrentPage, startPage);
				wizard.NextButton.PerformClick();

				AssertEquals(wizard.CurrentPage, step1);
				wizard.NextButton.PerformClick();

				AssertEquals(wizard.CurrentPage, step2);
				wizard.NextButton.PerformClick();

				AssertEquals(wizard.CurrentPage, step2);
				wizard.NextButton.PerformClick();

				AssertEquals(wizard.CurrentPage, step2);
				step2.AllowForward = true;

				wizard.NextButton.PerformClick();
				AssertEquals(wizard.CurrentPage, step3);

				wizard.FinishButton.PerformClick();
				AssertEquals(wizard.CurrentPage, finishPage);
			}
		}

		public void TestCurrentPageIndex()
		{
			using (var wizard = new WizardForm(null))
			{
				var startPage = new WizardPageStart();
				var step1 = new TestStepWizardPage();
				var step2 = new TestStepWizardPage();
				var step3 = new TestStepWizardPage();
				var finishPage = new WizardPageFinish();

				wizard.Pages.AddRange(new WizardPage[] { startPage, step1, step2, step3, finishPage });

				AssertEquals(wizard.CurrentPageIndex, -1);

				wizard.Show();

				AssertEquals(wizard.CurrentPageIndex, 0);
				wizard.NextButton.PerformClick();
				AssertEquals(wizard.CurrentPageIndex, 1);
				wizard.NextButton.PerformClick();
				AssertEquals(wizard.CurrentPageIndex, 2);
				wizard.NextButton.PerformClick();

				AssertEquals(wizard.CurrentPageIndex, 3);

				wizard.BackButton.PerformClick();
				AssertEquals(wizard.CurrentPageIndex, 2);
				wizard.BackButton.PerformClick();
				AssertEquals(wizard.CurrentPageIndex, 1);
				wizard.BackButton.PerformClick();
				AssertEquals(wizard.CurrentPageIndex, 0);
			}
		}

		public void TestWizardPagesHaveTheSameDatasource()
		{
			using (var wizard = new WizardForm(null))
			{
				var step1 = new TestStepWizardPage();
				var step2 = new TestStepWizardPage();
				var step3 = new TestStepWizardPage();

				wizard.Pages.AddRange(new WizardPage[] { step1, step2, step3 });

				wizard.Show();

				AssertEquals(wizard.DataSource, step1.DataSourceForBinding);
				AssertEquals(wizard.DataSource, step2.DataSourceForBinding);
				AssertEquals(wizard.DataSource, step3.DataSourceForBinding);
			}
		}

		#region Implementation

		class TestStepWizardPage : WizardPageStep
		{
			public string HeaderTitle { get; set; }
			public string HeaderDescription { get; set; }

			public TestStepWizardPage()
				: base()
			{ }

			public override void NotifyActivated(WizardForm wizard)
			{
				base.NotifyActivated(wizard);

				wizard.PageHeaderTitle = HeaderTitle;
				wizard.PageHeaderDescription = HeaderDescription;
			}
		}

		class TestStepWizardCancelPage : WizardPageStep
		{
			public bool AllowForward;

			public TestStepWizardCancelPage()
				: base()
			{ }

			public override void NotifyLeaving(WizardSteppingEventArgs args)
			{
				base.NotifyLeaving(args);

				args.Cancel = args.MovementDirection == WizardSteppingEventArgs.Direction.Forward && !AllowForward;
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new WizardForm(null);
		}

		#endregion
	}
}
