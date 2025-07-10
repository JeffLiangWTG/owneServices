using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(InvoiceHeaderOrganisationsUserControl))]
	sealed class InvoiceHeaderOrganisationUserControlTest : ImportCustomsUserControlBasherAbstractTest
	{
		public void TestMIDParse()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var userControl = invoiceUserControl.invoiceHeaderOrganisationUserControl;
				AssertEquals(mainAddress.PK, userControl.JZ_OA_ManufacturerAddressAddressControl.Parse("MID234323"));
				AssertEquals(mainAddress.PK, userControl.InvoicerDocAddressAddressControl.Parse("MID234323"));
				AssertEquals(mainAddress.PK, userControl.JZ_OA_SupplierAddressAddressControl.Parse("MID234323"));
			}
		}

		public void TestUltimateConsigneeCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var userControl = invoiceUserControl.invoiceHeaderOrganisationUserControl;
				AssertEquals("Consignee", userControl.JZ_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var userControl = invoiceUserControl.invoiceHeaderOrganisationUserControl;
				AssertEquals("Ultimate Consignee", userControl.JZ_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestOrganizationVisibilityBetweenACSAndACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				invoiceUserControl.InvoiceTabControl.SelectedTab = invoiceUserControl.OrganizationsTabPage;
				var userControl = invoiceUserControl.invoiceHeaderOrganisationUserControl;
				AssertEquals("AIIExporterGuidFindBox.Visible when message type is ACS", true, userControl.AIIExporterGuidFindBox.Visible);
				AssertEquals("US_OA_InvoicerAddressAddressControl.Visible when message type is ACS", true, userControl.InvoicerDocAddressAddressControl.Visible);
				AssertEquals("AIISellingAgentGuidFindBox.Visible when message type is ACS", true, userControl.AIISellingAgentGuidFindBox.Visible);
				AssertEquals("AIIBuyingAgentGuidFindBox.Visible when message type is ACS", true, userControl.AIIBuyingAgentGuidFindBox.Visible);
				AssertEquals("ACEForeignExporterAddressControl.Visible when message type is ACS", false, userControl.ACEForeignExporterAddressControl.Visible);
				AssertEquals("JZ_OA_ShipperAddressControl.Visible when message type is ACS", false, userControl.JZ_OA_ShipperAddressControl.Visible);
				AssertEquals("JZ_OA_DistributorAddressControl.Visible when message type is ACS", false, userControl.JZ_OA_DistributorAddressControl.Visible);
				AssertEquals("JZ_OA_PackagerAddressControl.Visible when message type is ACS", false, userControl.JZ_OA_PackagerAddressControl.Visible);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				invoiceUserControl.InvoiceTabControl.SelectedTab = invoiceUserControl.OrganizationsTabPage;
				var userControl = invoiceUserControl.invoiceHeaderOrganisationUserControl;
				AssertEquals("AIIExporterGuidFindBox.Visible when message type is ACE", false, userControl.AIIExporterGuidFindBox.Visible);
				AssertEquals("US_OA_InvoicerAddressAddressControl.Visible when message type is ACE", false, userControl.InvoicerDocAddressAddressControl.Visible);
				AssertEquals("AIISellingAgentGuidFindBox.Visible when message type is ACE", false, userControl.AIISellingAgentGuidFindBox.Visible);
				AssertEquals("AIIBuyingAgentGuidFindBox.Visible when message type is ACE", false, userControl.AIIBuyingAgentGuidFindBox.Visible);
				AssertEquals("ACEForeignExporterAddressControl.Visible when message type is ACE", true, userControl.ACEForeignExporterAddressControl.Visible);
				AssertEquals("JZ_OA_ShipperAddressControl.Visible when message type is ACE", true, userControl.JZ_OA_ShipperAddressControl.Visible);
				AssertEquals("JZ_OA_DistributorAddressControl.Visible when message type is ACE", true, userControl.JZ_OA_DistributorAddressControl.Visible);
				AssertEquals("JZ_OA_PackagerAddressControl.Visible when message type is ACE", true, userControl.JZ_OA_PackagerAddressControl.Visible);
			}
		}

		protected override Type UserControlToBashType => typeof(InvoiceHeaderOrganisationsUserControl);
	}
}
