using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(LicensingAnimalAndPlantUserControl))]
	sealed class LicensingAnimalAndPlantUserControlTest : TestCaseWithFactory
	{
		public void TestCalcEditProperties()
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
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage");
				var licensingAnimalAndPlantUserControl = licensingUserControl.FindSingleOrDefault<LicensingAnimalAndPlantUserControl>(c => c.Name == "LicensingAnimalAndPlantUserControl");
				foreach (var calcEditName in new string[] { "JI_AnimalAgeMonthCalcEdit", "JI_AnimalAgeYearCalcEdit", "JI_AnimalFemaleQtyCalcEdit", "JI_AnimalMaleQtyCalcEdit" })
				{
					var calcEdit = licensingAnimalAndPlantUserControl.FindSingle<ZCalcEdit>(c => c.Name == calcEditName);
					Assert(calcEdit.ShowEmptyStringForEmptyValue);
				}
			}
		}

		public void TestPackingHousesGrid()
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
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage");
				var licensingAnimalAndPlantUserControl = licensingUserControl.FindSingleOrDefault<LicensingAnimalAndPlantUserControl>(c => c.Name == "LicensingAnimalAndPlantUserControl");

				var grid = licensingAnimalAndPlantUserControl.FindSingle<ZGrid>(c => c.Name == "PackingHousesGrid");
				AssertEquals(5, grid.MaximumRows);
			}
		}

		public void TestPackingDatesGrid()
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
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage");
				var licensingAnimalAndPlantUserControl = licensingUserControl.FindSingleOrDefault<LicensingAnimalAndPlantUserControl>(c => c.Name == "LicensingAnimalAndPlantUserControl");

				var grid = licensingAnimalAndPlantUserControl.FindSingle<ZGrid>(c => c.Name == "PackingDatesGrid");
				AssertEquals(5, grid.MaximumRows);
				Assert(!grid.ReadOnly);
			}
		}

		public void TestSlaughterDatesGrid()
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
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage");
				var licensingAnimalAndPlantUserControl = licensingUserControl.FindSingleOrDefault<LicensingAnimalAndPlantUserControl>(c => c.Name == "LicensingAnimalAndPlantUserControl");

				var grid = licensingAnimalAndPlantUserControl.FindSingle<ZGrid>(c => c.Name == "SlaughterDatesGrid");
				AssertEquals(5, grid.MaximumRows);
				Assert(!grid.ReadOnly);
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
			var header = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			invoiceLine.AssignCMHeaderToInvoices(header);
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
	}
}
