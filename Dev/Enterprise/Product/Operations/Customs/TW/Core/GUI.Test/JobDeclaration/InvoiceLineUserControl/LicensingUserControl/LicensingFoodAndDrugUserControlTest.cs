using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(LicensingFoodAndDrugUserControl))]
	sealed class LicensingFoodAndDrugUserControlTest : TestCaseWithFactory
	{
		public void TestNumeric_TextChanged()
		{
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "LicensingTabPage");
				var licensingUserControl = invoiceLineUserControl.FindSingleOrDefault<LicensingUserControl>(c => c.Name == "LicensingUserControl");
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage");
				var licensingFoodAndDrugUserControl = licensingUserControl.FindSingleOrDefault<LicensingFoodAndDrugUserControl>(c => c.Name == "LicensingFoodAndDrugUserControl");
				var sterilizationValueCalcEdit = licensingFoodAndDrugUserControl.FindSingle<ZCalcEdit>(c => c.Name == "TW_SterilizationValueCalcEdit");
				var pHValueCalcEdit = licensingFoodAndDrugUserControl.FindSingle<ZCalcEdit>(c => c.Name == "TW_PHValueCalcEdit");
				AssertEquals(99.9m, sterilizationValueCalcEdit.MaxValue);
				AssertEquals(99.9m, pHValueCalcEdit.MaxValue);
				jobDeclaration.FilteredInvoiceLines[0].JI_SterilizationValueNumeric = 1;
				AssertEquals("1.0", sterilizationValueCalcEdit.Text);
				jobDeclaration.FilteredInvoiceLines[0].JI_SterilizationValueNumeric = -10;
				AssertEquals("", sterilizationValueCalcEdit.Text);
				jobDeclaration.FilteredInvoiceLines[0].JI_PHValueNumeric = 1;
				AssertEquals("1.0", pHValueCalcEdit.Text);
				jobDeclaration.FilteredInvoiceLines[0].JI_PHValueNumeric = -10;
				AssertEquals("", pHValueCalcEdit.Text);
			}
		}

		public void TestStorageAndShippingConditionGrid()
		{
			using (var control = new LicensingFoodAndDrugUserControl())
			{
				var storageAndShippingConditionGrid = control.StorageAndShippingConditionGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Maximum Rows", 9, storageAndShippingConditionGrid.MaximumRows);
					AssertContainsExactElementsInAnyOrder(new[] { "JG_ReferenceNumber", "Description" }, storageAndShippingConditionGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
				});
			}
		}

		public void TestTW_SterilizationValueCalcEdit_MaxValue()
		{
			using (var control = new LicensingFoodAndDrugUserControl())
			{
				var sterilizationValueCalcEdit = control.TW_SterilizationValueCalcEdit;
				AssertEquals(99.9m, sterilizationValueCalcEdit.MaxValue);
			}
		}

		public void TestTW_PHValueCalcEdit_MaxValue()
		{
			using (var control = new LicensingFoodAndDrugUserControl())
			{
				var phValueCalcEdit = control.TW_PHValueCalcEdit;
				AssertEquals(99.9m, phValueCalcEdit.MaxValue);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			invoiceLine = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault().IsLinkedCMHeader = true;
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
	}
}
