using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportSupplierHeaderUserControlTest : Testing.CustomsUserControlBasherAbstractTest
	{
		public void TestDefaultSelectedTabpage()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				using (var control = new USExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(declaration, "");
					form.Show();
					control.InvoiceTabControl.Select();
					AssertEquals("Default selected tabpage on invoice header.", "SEDDetailsTabPage", control.InvoiceTabControl.SelectedTab.Name);
				}
			}
		}

		public void TestBindTo()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				using (var control = new USExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(declaration, "");
					form.Show();
					var grid = control.JobComInvoiceHeadersBoundGrid;
					var gridBindTo = grid.BindToGridList;
					AssertEquals("Invoices", gridBindTo);
					var isFixedRateCheckBox = control.InvoiceTabControl.Controls.Find("IsFixedRateCheckBox", true)[0] as ZCheckBox;
					var fieldBindTo = isFixedRateCheckBox.BindTo.Split('.')[0];
					AssertEquals("The field bind source should be equal to grid, otherwise CurrentDataItemChanged doesn't work.", gridBindTo, fieldBindTo);
				}
			}
		}

		public void TestECCNNumbersControlVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var refCusCodeC58 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C58, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC58PK = refCusCodeC58.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC58PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ECCNRequired, "Mandatory");

			var refCusCodeC32 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "1C351", USAESLicenseCode.Codes.C32, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC32PK = refCusCodeC32.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC32PK, RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C32);

			var refCusCodeC30 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "C2343", USAESLicenseCode.Codes.C30, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC30PK = refCusCodeC30.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC30PK, RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C30);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC30PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ECCNRequired, "Mandatory");

			Factory.Save();

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33 });

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "TEST ECCN Controls";
			var invoice = declaration.Invoices.AddNew();
			invoice.US_LicenseType = USAESLicenseCode.Codes.C32;
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new USExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");

					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
					control.InvoiceTabControl.SelectedTab = control.LicenseDDTCDetailsTabPage;
					control.ECCNCodeFindBox.IsAccessible = true;
					control.ECCNTextBox.IsAccessible = true;
					AssertEquals("ECCN DropDown Control should be shown", true, control.ECCNCodeFindBox.Visible);
					AssertEquals("ECCN TextBox Control should not be shown", false, control.ECCNTextBox.Visible);

					invoice.US_LicenseType = USAESLicenseCode.Codes.C58;
					AssertEquals("ECCN DropDown Control should be shown", false, control.ECCNCodeFindBox.Visible);
					AssertEquals("ECCN TextBox Control should not be shown", true, control.ECCNTextBox.Visible);
				}
			}
		}

		public void TestUS_USPPI_ContactsActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new USExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");

					var dropEditColumnStyleInfo = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "USPPIDocAddress+E2_Contact");
					AssertEquals("Only show active contact for US_USPPI", "US_USPPI+Organisation+ContactsActive", dropEditColumnStyleInfo.BindToList);
				}
			}
		}

		public void TestGridLayoutContext()
		{
			using (var control = new USExportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestColumnNamesInSortOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				using (var control = new USExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(declaration, "");
					var numberOfColumnsOnInvoiceHeaderGrid = ExpectedColumnNamesInSortOrderList.Length;
					Assert("Control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Count must have at least " + numberOfColumnsOnInvoiceHeaderGrid, numberOfColumnsOnInvoiceHeaderGrid <= control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Count);
					for (int i = 0; i < numberOfColumnsOnInvoiceHeaderGrid; i++)
					{
						var columnInfo = control.JobComInvoiceHeadersBoundGrid.ColumnStyles[i] as ZGridColumnInfo;
						ZString expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
						AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
					}
				}
			}
		}

		public void TestModifyColumnProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				using (var control = new USExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(declaration, "");
					AssertColumnIsVisible(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer, control.JobComInvoiceHeadersBoundGrid.ColumnStyles, AssertJZ_OH_Buyer);
					AssertColumnIsVisible(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm, control.JobComInvoiceHeadersBoundGrid.ColumnStyles, null);
					AssertColumnIsVisible(JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace, control.JobComInvoiceHeadersBoundGrid.ColumnStyles, null);
					AssertColumnIsVisible("InvoiceLineTotal", control.JobComInvoiceHeadersBoundGrid.ColumnStyles, null);
					AssertColumnIsVisible(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate, control.JobComInvoiceHeadersBoundGrid.ColumnStyles, null);
					AssertColumnIsVisible(JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier, control.JobComInvoiceHeadersBoundGrid.ColumnStyles, AssertJZ_OH_Supplier);
				}
			}
		}

		public void TestGridId()
		{
			using (var control = new USExportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutZIkHUoOCsN/evw2qbJLNpg==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestECCNColumnCharacterCasing()
		{
			using (var control = new USExportSupplierHeaderUserControl())
			{
				var grid = control.InvoiceHeadersBoundGrid;
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == "US_ECCN");
				AssertEquals(CharacterCasing.Upper, column.CharacterCasing);
			}
		}

		public void TestUSPPIEINLabelControl()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABC INC.";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "33-8888888AA", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.USPPIDocAddress.OrganisationPK = orgHeader.PK;

			using (var form = new ZForm(declaration))
			using (var control = new USExportSupplierHeaderUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(declaration, "");
				form.Show();
				control.SEDDetailsTabPage.Select();

				Assert(control.USPPIEINLabel.Visible);
				AssertEquals("EIN: 33-8888888AA", control.USPPIEINLabel.Text);

				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				invoice.USPPIDocAddress.OrganisationPK = ZGuid.Empty;
				invoice.USPPIDocAddress.OrganisationPK = orgHeader.PK;
				AssertEquals(ZString.Empty, control.USPPIEINLabel.Text);
			}
		}

		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;

		protected override Type UserControlToBashType => typeof(USExportSupplierHeaderUserControl);

		void AssertJZ_OH_Buyer(ZGridColumnInfo info)
		{
			AssertEquals(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer, info.ColumnName);
			AssertEquals("Caption", "Ult. Cnee.", info.Caption);
			AssertEquals("GroupName", "Ult. Cnee.", info.GroupName.Caption);
		}

		void AssertJZ_OH_Supplier(ZGridColumnInfo info)
		{
			AssertEquals(JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier, info.ColumnName);
			AssertEquals("Caption", "USPPI", info.Caption);
			AssertEquals("GroupName", "USPPI", info.GroupName.Caption);
		}

		delegate void ExtraAssertion(ZGridColumnInfo info);
		void AssertColumnIsVisible(string columnName, System.Collections.ArrayList arrayList, ExtraAssertion extraAssertion)
		{
			bool found = false;
			foreach (ZGridColumnInfo columnInfo in arrayList)
			{
				if (columnInfo.ColumnName == columnName)
				{
					found = true;
					AssertEquals(columnName + "'s IsVisible", true, columnInfo.IsVisible);
					extraAssertion?.Invoke(columnInfo);
				}
			}

			AssertEquals(columnName + " was not found", true, found);
		}

		string[] expectedColumnNamesInSortOrderList;
		string[] ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new[]
					{
						"JZ_OH_Supplier",
						"JZ_OH_Buyer",
						"US_UltimateDestinationCountry",
						"SupplierPickupAddress+OrganisationPK",
						"JZ_InvoiceNumber",
						"JZ_IncoTerm",
						"JZ_IncoTermPlace",
						"JZ_InvoiceAmount",
						"JZ_RX_NKInvoice_Currency",
						"JZ_InvoiceCurrExRate",
						"InvoiceLineTotal",
						"JZ_Calc_BalanceString",
						"JZ_InvoiceDate",
						"SupplierPickupAddress+E2_OA_Address",
						"USPPIDocAddress+E2_Contact",
						"USPPIDocAddress+E2_Phone_Formatted",
						"UltimateConsigneeDocAddress+E2_OA_Address",
						"UltimateConsigneeDocAddress+E2_Contact",
						"UltimateConsigneeDocAddress+E2_Phone_Formatted",
						"US_UltimateConsigneeType",
						"JZ_OH_Consignee",
						"IntermediateConsigneeDocAddress+E2_OA_Address",
						"IntermediateConsigneeDocAddress+E2_Contact",
						"IntermediateConsigneeDocAddress+E2_Phone_Formatted",
						"US_StateOfOrigin",
						"US_ForeignTradeZone",
						"US_ECCN",
						"US_RoutedTransaction",
						"US_TransactionsRelated",
						"US_LicenseType",
						"US_LicenseNo",
						"US_InbondType",
						"US_ImportEntryNo",
						"US_HazardousCargo",
						"US_ExportCode",
						"US_EntryNumber",
						"US_XTN",
						"US_DateOfExport"
					};
				}
				return expectedColumnNamesInSortOrderList;
			}
		}
	}
}
