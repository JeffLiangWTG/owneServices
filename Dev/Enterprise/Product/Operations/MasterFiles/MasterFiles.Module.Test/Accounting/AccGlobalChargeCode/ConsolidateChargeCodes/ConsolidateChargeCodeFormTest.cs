using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ConsolidateChargeCodeForm))]
	sealed class ConsolidateChargeCodeFormTest : ZFormBasherTest
	{
		class ConsolidateChargeCodeFormForTest : ConsolidateChargeCodeForm
		{
			public ConsolidateChargeCodeFormForTest(BusinessObjectFactory f) : base(f) { }
			public AccChargeCodeFilterControl FilterControl { get { return filterControl; } }
			public void CallOnConsolidateClicked()
			{
				OnConsolidateClicked(this, EventArgs.Empty);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ConsolidateChargeCodeForm(Factory);
		}

		[RequiresSTA]
		public void TestConsolidateClick()
		{
			using (var form = new ConsolidateChargeCodeFormForTest(Factory))
			{
				form.Show();
				form.CallOnConsolidateClicked();
				AssertEquals("Please select message", "Please select one or more Charge Codes to consolidate", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.FilterControl.FirePerformSearch();
				form.FilterControl.Grid.Select(0);
				form.CallOnConsolidateClicked();
				using (var globalChargeCodeForm = form.LastOpenedForm)
				{
					Assert("Form is for global charge code", globalChargeCodeForm is AccGlobalChargeCodeForm);
					Assert("Global charge code form is opened", Application.OpenForms.Cast<object>().Contains(globalChargeCodeForm));
				}

				form.FilterControl.Grid.Select(0);
				form.FilterControl.Grid.Select(1);
				AssertEquals("precondition", 2, form.FilterControl.Grid.SelectedElements.Length);
				form.CallOnConsolidateClicked();
				using (var globalChargeCodeForm = form.LastOpenedForm)
				{
					Assert("Form is for global charge code", globalChargeCodeForm is AccGlobalChargeCodeForm);
					Assert("Global charge code form is opened", Application.OpenForms.Cast<object>().Contains(globalChargeCodeForm));
				}
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestConsolidateClickThenSaveAndCreateNewCharge()
		{
			AccChargeCodeTest.EnsureAllGSTRegisteredCompaniesHaveRatedGST(Factory);

			using (var form = new ConsolidateChargeCodeFormForTest(Factory))
			{
				form.Show();
				form.FilterControl.FirePerformSearch();
				form.FilterControl.Grid.Select(0);

				object formCreated = null;
				var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args)
				{ formCreated = sender; });
				ZForm.FormCreated += formCreatedHandler;

				try
				{
					form.CallOnConsolidateClicked();
					using (var globalChargeCodeForm = (ZForm)formCreated)
					{
						Assert("Form is for global charge code", globalChargeCodeForm is AccGlobalChargeCodeForm);
						Assert("Global charge code form is opened", Application.OpenForms.Cast<object>().Contains(globalChargeCodeForm));

						var gCCF = globalChargeCodeForm as AccGlobalChargeCodeForm;
						((AccChargeCode)gCCF.BusinessEntity).AC_Code += "Z";

						var toolStrip = (ZToolStrip)gCCF.Controls.Find("toolStrip", true).First();
						var button = toolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Text == "&Save");
						AssertNotNull(button);
						button.PerformClick();

						Assert("Global charge code form is opened", Application.OpenForms.Cast<object>().Contains(gCCF));

						button = toolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Text == "&New");
						AssertNotNull(button);
						button.PerformClick();

						AssertEquals(true, gCCF.IsDisposed);

						((IDisposable)formCreated).Dispose();
					}
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		[RequiresSTA]
		public void TestFilterStrip()
		{
			using (var form = new ConsolidateChargeCodeFormForTest(Factory))
			{
				form.FilterControl.FirePerformSearch();
				Assert("Search works", form.FilterControl.GridCollection.Count > 0);
			}
		}
	}
}
