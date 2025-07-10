using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ApportionmentProgressForm))]
	sealed class ApportionmentProgressFormTest : ZFormBasherTest
	{
		public void TestContruct()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			using (ApportionmentProgressForm testForm = new ApportionmentProgressForm(testDec))
			{
				AssertEquals("Progress bar visible", true, testForm.ShowProgressBar);
				AssertEquals("Cancel button invisible", false, testForm.ShowCancelButton);
			}
		}

		public void TestUpdateProgress()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			using (TestProgressForm testForm = new TestProgressForm(testDec))
			{
				testDec.UpdateApportionmentProgress();
				AssertEquals("Percentage is now 0", 0, testForm.Percentage);
			}
		}

		public void TestControlsArePlacedProperly()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			using (TestProgressForm testForm = new TestProgressForm(testDec))
			{
				Assert("Enterprise logo is located inside this form", testForm.EnterpriseLogoExposed.Location.X > 0);
				Assert("Enterprise logo is located inside this form", testForm.EnterpriseLogoExposed.Location.Y > 0);
				Assert("Enterprise logo is located inside this form", testForm.EnterpriseLogoExposed.Location.X + testForm.EnterpriseLogoExposed.Width < testForm.Size.Width);
				Assert("Enterprise logo is located inside this form", testForm.EnterpriseLogoExposed.Location.Y + testForm.EnterpriseLogoExposed.Height < testForm.Size.Height);
				Assert("ProgressBar is located inside this form", testForm.ProgressBarExposed.Location.X > 0);
				Assert("ProgressBar is located inside this form", testForm.ProgressBarExposed.Location.Y > 0);
				Assert("ProgressBar is located inside this form", testForm.ProgressBarExposed.Location.X + testForm.ProgressBarExposed.Width < testForm.Size.Width);
				Assert("ProgressBar is located inside this form", testForm.ProgressBarExposed.Location.Y + testForm.ProgressBarExposed.Height < testForm.Size.Height);
				Assert("ProgressLabel is located inside this form", testForm.ProgressLabelExposed.Location.Y > 0);
				Assert("ProgressLabel is located inside this form", testForm.ProgressLabelExposed.Location.Y > 0);
				Assert("ProgressLabel is located inside this form", testForm.ProgressLabelExposed.Location.X + testForm.ProgressLabelExposed.Width < testForm.Size.Width);
				Assert("ProgressLabel is located inside this form", testForm.ProgressLabelExposed.Location.Y + testForm.ProgressLabelExposed.Height < testForm.Size.Height);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var jobDeclaration = BaseJobDeclaration.New(Factory);
			return new ApportionmentProgressForm(jobDeclaration);
		}

		protected override bool AllowFormSizeFixed => true;

		sealed class TestProgressForm : ApportionmentProgressForm
		{
			public int Percentage = -100;
			public TestProgressForm(BaseJobDeclaration jobDeclaration) : base(jobDeclaration)
			{
			}

			protected override void UpdateProgress(int percentage)
			{
				Percentage = percentage;
			}

			public Control ProgressBarExposed => ProgressBar;

			public Control ProgressLabelExposed => ProgressLabel;

			public Control EnterpriseLogoExposed => EnterpriseLogo;
		}
	}
}
