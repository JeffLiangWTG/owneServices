using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class LicensingMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestColumnNamesInSortOrder()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "LicensingTabPage");
					var licensingUserControl = invoiceLineUserControl.FindSingleOrDefault<LicensingUserControl>(c => c.Name == "LicensingUserControl");
					licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "MessagesTabPage");

					var licensingMessagesUserControl = licensingUserControl.FindSingleOrDefault<LicensingMessagesUserControl>(c => c.Name == "LicensingMessagesUserControl");
					var controllingAgencyGrid = licensingMessagesUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "ControllingAgencyGrid");
					controllingAgencyGrid.ResetColumns();
					AssertEquals(ExpectedColumnNamesInSortOrderList.Count, controllingAgencyGrid.Columns.Count);
					AssertEquals("ControllingAgencyGrid must have 9 columns", 9, controllingAgencyGrid.Columns.Count);
					for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
					{
						var column = controllingAgencyGrid.Columns[i];
						AssertNotNull(column);
						var expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
						AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
						AssertEquals("IsVisible", true, column.IsVisible);
					}
				}
			}
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (fExpectedColumnNamesInSortOrderList == null)
				{
					fExpectedColumnNamesInSortOrderList = new List<ZString>();
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.IsLinkedCMHeader);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.Sequence);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.FunctionalReferenceID);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.CertificateType);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.BusinessType);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.MessageType);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.MessageTypeDescription);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.ControllingAgency);
					fExpectedColumnNamesInSortOrderList.Add(InvoiceLineLinkControllingMsgHeader.Schema.ControllingAgencyDescription);
				}

				return fExpectedColumnNamesInSortOrderList;
			}
		}

		List<ZString> fExpectedColumnNamesInSortOrderList;
	}
}
