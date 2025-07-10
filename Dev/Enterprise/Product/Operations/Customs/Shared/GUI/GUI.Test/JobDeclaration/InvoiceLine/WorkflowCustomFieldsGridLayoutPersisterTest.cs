using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(WorkflowCustomFieldsGridLayoutPersisterTest))]
	public class WorkflowCustomFieldsGridLayoutPersisterTest : TestCaseWithFactory
	{
		public void TestUpdateGridLayoutOnlyWhenCountOfInvoiceLinesFrom0To1Or1To0()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var customFieldName1 = "CUSTOM FIELD NAME 1";
			var customFieldName2 = "CUSTOM FIELD NAME 2";
			CreateProcessTaskTemplate(customFieldName1, ZString.Empty, ZGuid.Empty);
			CreateProcessTaskTemplate(customFieldName2, ZString.Empty, orgHeader.PK);
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			invoice1.JZ_OH_Buyer = orgHeader.PK;

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					userControl.LineDetailTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, false);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine11);
					var invoiceLine12 = invoice1.InvoiceLines.AddNew();
					var invoiceLine13 = invoice1.InvoiceLines.AddNew();
					AssertNoCustomBOInChildren(invoiceLine12);
					AssertNoCustomBOInChildren(invoiceLine13);

					var invoice2 = declaration.Invoices.AddNew();
					var invoiceLine21 = invoice2.InvoiceLines.AddNew();
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine11);
					AssertCustomBOInChildren(invoiceLine21);
					var invoiceLine22 = invoice2.InvoiceLines.AddNew();
					var invoiceLine23 = invoice2.InvoiceLines.AddNew();
					AssertNoCustomBOInChildren(invoiceLine22);
					AssertNoCustomBOInChildren(invoiceLine23);

					declaration.InvoiceLines.RemoveAndDelete(invoiceLine11);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					//The CustomBO of invoceLine12 come from BindingManager_PositionChanged rather than UpdateGridLayout.
					AssertCustomBOInChildren(invoiceLine12);
					AssertNoCustomBOInChildren(invoiceLine13);

					declaration.InvoiceLines.RemoveAndDelete(invoiceLine12);
					AssertCustomBOInChildren(invoiceLine13);
					declaration.InvoiceLines.RemoveAndDelete(invoiceLine13);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);

					declaration.InvoiceLines.RemoveAndDelete(invoiceLine21);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);
					AssertCustomBOInChildren(invoiceLine22);
					AssertNoCustomBOInChildren(invoiceLine23);

					declaration.InvoiceLines.RemoveAndDelete(invoiceLine22);
					AssertCustomBOInChildren(invoiceLine23);
					declaration.InvoiceLines.RemoveAndDelete(invoiceLine23);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, false);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);
				}
			}
		}

		public void TestWorkflowCustomFieldsGridLayoutPersisterForOneTemplate()
		{
			var customFieldName1 = "CUSTOM FIELD NAME 1";
			CreateProcessTaskTemplate(customFieldName1, ZString.Empty, ZGuid.Empty);
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					userControl.LineDetailTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertCustomBOInChildren(invoiceLine);
				}
			}
		}

		public void TestWorkflowCustomFieldsGridLayoutPersisterForTwoTemplates()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var customFieldName1 = "CUSTOM FIELD NAME 1";
			var customFieldName2 = "CUSTOM FIELD NAME 2";
			CreateProcessTaskTemplate(customFieldName1, ZString.Empty, ZGuid.Empty);
			CreateProcessTaskTemplate(customFieldName2, ZString.Empty, orgHeader.PK);
			Factory.Save();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Buyer = orgHeader.PK;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					userControl.LineDetailTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine1);
				}
			}
		}

		public void TestRefreshGridWhenRelatedPropertyChanged_JZ()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var customFieldName1 = "CUSTOM FIELD NAME 1";
			var customFieldName2 = "CUSTOM FIELD NAME 2";
			CreateProcessTaskTemplate(customFieldName1, ZString.Empty, ZGuid.Empty);
			CreateProcessTaskTemplate(customFieldName2, ZString.Empty, orgHeader.PK);
			Factory.Save();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					userControl.LineDetailTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);
					AssertCustomBOInChildren(invoiceLine);

					invoice.JZ_OH_Buyer = orgHeader.PK;
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, false);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine);
				}
			}
		}

		public void TestRefreshGridWhenRelatedPropertyChanged_JE()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var customFieldName1 = "CUSTOM FIELD NAME 1";
			var customFieldName2 = "CUSTOM FIELD NAME 2";
			CreateProcessTaskTemplate(customFieldName1, ZString.Empty, ZGuid.Empty);
			CreateProcessTaskTemplate(customFieldName2, ZString.Empty, orgHeader.PK);
			Factory.Save();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					userControl.LineDetailTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);
					AssertCustomBOInChildren(invoiceLine);

					declaration.JE_OH_Importer = orgHeader.PK;
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, false);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine);
				}
			}
		}

		public void TestRefreshGridWhenInvoiceLinesCountChanged()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var customFieldName1 = "CUSTOM FIELD NAME 1";
			var customFieldName2 = "CUSTOM FIELD NAME 2";
			CreateProcessTaskTemplate(customFieldName1, ZString.Empty, ZGuid.Empty);
			CreateProcessTaskTemplate(customFieldName2, ZString.Empty, orgHeader.PK);
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_OH_Buyer = orgHeader.PK;

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					userControl.LineDetailTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, false);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine);

					var invoice2 = declaration.Invoices.AddNew();
					var invoiceLine2 = invoice2.InvoiceLines.AddNew();
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, true);
					AssertCustomBOInChildren(invoiceLine);
					AssertCustomBOInChildren(invoiceLine2);

					declaration.InvoiceLines.RemoveAndDelete(invoiceLine);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, true);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);
					AssertCustomBOInChildren(invoiceLine2);

					declaration.InvoiceLines.RemoveAndDelete(invoiceLine2);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName1, false);
					AssertColumnStyleInfo(userControl.CustomsInvoiceLinesBoundGrid, customFieldName2, false);
				}
			}
		}

		void CreateProcessTaskTemplate(ZString customFieldName, ZString messageType, ZGuid clientPK)
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode;
			if (!messageType.IsEmpty)
			{
				processTaskTemplate.P0_SubType1 = messageType;
			}
			if (clientPK.IsValid)
			{
				processTaskTemplate.P0_OH_Client = clientPK;
			}
			var customField = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField.XC_Name = customFieldName;
			customField.XC_Type = AddOnColumnDataType.Codes.String;
		}

		void AssertColumnStyleInfo(ZGrid grid, ZString customFieldName, bool exist)
		{
			var columnStyleInfo = grid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName.Contains(customFieldName));
			if (exist)
			{
				AssertNotNull(customFieldName, columnStyleInfo);
				AssertEquals(customFieldName + " Caption", customFieldName, columnStyleInfo.Caption);
				AssertEquals(customFieldName + " Visible", true, columnStyleInfo.IsVisible);
			}
			else
			{
				AssertNull(columnStyleInfo);
			}
		}

		void AssertCustomBOInChildren(BaseJobComInvoiceLine invoiceLine)
		{
			AssertEquals("There should be only one CustomBusinessObject.", 1, ((IBusiness)invoiceLine).Children.OfType<CustomBusinessObject>().Count());
		}

		void AssertNoCustomBOInChildren(BaseJobComInvoiceLine invoiceLine)
		{
			AssertEquals("There should be no CustomBusinessObject.", 0, ((IBusiness)invoiceLine).Children.OfType<CustomBusinessObject>().Count());
		}
	}
}
