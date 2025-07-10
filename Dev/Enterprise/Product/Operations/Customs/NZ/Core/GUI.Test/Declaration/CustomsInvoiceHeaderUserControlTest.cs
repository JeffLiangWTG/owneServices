using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class CustomsInvoiceHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceNumberSupplierAndImporterAreAlwaysTheFirstFourColumns()
		{
			using (ZForm form = new ZForm(declaration))
			{
				using (CustomsInvoiceHeaderUserControl userControl = new CustomsInvoiceHeaderUserControl())
				{
					form.Controls.Add(userControl);
					userControl.JobDeclaration = declaration;
					form.Show();
					userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
					ZGridWithoutColumnStylesSerialisation grid = userControl.JobComInvoiceHeadersBoundGrid.InnerGrid;
					AssertEquals("Columns[0]", JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ((ZGridColumnInfo)grid.ColumnStyles[0]).ColumnName);
					AssertEquals("Columns[1]", JobComInvoiceHeader.Schema.JZ_OH_Supplier, ((ZGridColumnInfo)grid.ColumnStyles[1]).ColumnName);
					AssertEquals("Columns[2]", JobComInvoiceHeader.Schema.SupplierName, ((ZGridColumnInfo)grid.ColumnStyles[2]).ColumnName);
					AssertEquals("Columns[3]", JobComInvoiceHeader.Schema.JZ_OH_Buyer, ((ZGridColumnInfo)grid.ColumnStyles[3]).ColumnName);
				}
			}
		}

		public void TestNZSpecificControlVisibilityWhenMessageSubTypeChanges()
		{
			using (DeclarationForm form = new DeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.Simplified;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				CustomsInvoiceHeaderUserControl userControl = form.CustomsBrokerageUserControl.SupplierHeaderUserControl as CustomsInvoiceHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
				ZGridColumn column = userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.SupplierName];
				AssertEquals("SupplierName column.IsVisible", true, column.IsVisible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.Normal;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = form.CustomsBrokerageUserControl.SupplierHeaderUserControl as CustomsInvoiceHeaderUserControl;
				userControl.JobDeclaration = declaration;
				column = userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.SupplierName];
				AssertEquals("SupplierName column.IsVisible", false, column.IsVisible);
			}
		}

		public void TestDeclarationChargesGridVisibility()
		{
			using (ZForm form = new ZForm(declaration))
			{
				using (CustomsInvoiceHeaderUserControl userControl = new CustomsInvoiceHeaderUserControl())
				{
					form.Controls.Add(userControl);
					userControl.JobDeclaration = declaration;
					form.Show();
					var declarationChargesNotAvailableLabel = userControl.FindSingle<ZLabel>("DeclarationChargesNotAvailableLabel");
					userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
					AssertEquals("DeclarationChargesGrid.Visible", true, userControl.BaseGroupChargesGrid.Visible);
					AssertEquals("DeclarationChargesNotAvailableLabel.Visible", false, declarationChargesNotAvailableLabel.Visible);
					JobComInvoiceGroupHeader baseGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
					baseGroupHeader.JobComInvoiceGroupHeaders.AddNew();
					AssertEquals("DeclarationChargesGrid.Visible", false, userControl.BaseGroupChargesGrid.Visible);
					AssertEquals("DeclarationChargesNotAvailableLabel.Visible", true, declarationChargesNotAvailableLabel.Visible);
					baseGroupHeader.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
					AssertEquals("DeclarationChargesGrid.Visible", true, userControl.BaseGroupChargesGrid.Visible);
					AssertEquals("DeclarationChargesNotAvailableLabel.Visible", false, declarationChargesNotAvailableLabel.Visible);
				}
			}
		}

		public void TestNZHideShowOriginRegion()
		{
			using (ZForm form = new ZForm(declaration))
			{
				using (CustomsInvoiceHeaderUserControl userControl = new CustomsInvoiceHeaderUserControl())
				{
					form.Controls.Add(userControl);
					userControl.JobDeclaration = declaration;
					form.Show();
					userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
					declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Export;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					AssertEquals("Hide OriginRegion", false, (userControl.FindSingle<ZTextBox>("OriginRegionTextBox")).Visible);
					AssertEquals("Show Origin Region For TSW", false, (userControl.FindSingle<ZTextBox>("OriginRegionTextBoxForTSW")).Visible);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					AssertEquals("OtherTabPage.TabVisible - should be hidden for TSW Export", false, userControl.OtherTabPage.TabVisible);
					AssertNotNull("DefaultOrigin Column should still be in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin]);
					AssertEquals("Hide OriginRegion", false, userControl.OriginRegionTextBox.Visible);
					AssertEquals("Show Origin Region For TSW", true, (userControl.FindSingle<ZTextBox>("OriginRegionTextBoxForTSW")).Visible);
				}
			}

			using (DeclarationForm form1 = new DeclarationForm(declaration))
			{
				form1.Show();
				form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Export;
				form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.InvoicesTabPage;
				CustomsInvoiceHeaderUserControl userControl1 = form1.CustomsBrokerageUserControl.SupplierHeaderUserControl as CustomsInvoiceHeaderUserControl;
				userControl1.InvoiceTabControl.SelectedTab = userControl1.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals("Hide OriginRegion", false, (userControl1.FindSingle<ZTextBox>("OriginRegionTextBox")).Visible);
				AssertEquals("Show Origin Region For TSW", false, (userControl1.FindSingle<ZTextBox>("OriginRegionTextBoxForTSW")).Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("OtherTabPage.TabVisible - should be hidden for TSW Export", false, userControl1.OtherTabPage.TabVisible);
				AssertNotNull("DefaultOrigin Column should still be in Grid", userControl1.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin]);
				AssertEquals("Hide OriginRegion", false, userControl1.OriginRegionTextBox.Visible);
				AssertEquals("Show Origin Region For TSW", true, (userControl1.FindSingle<ZTextBox>("OriginRegionTextBoxForTSW")).Visible);
			}
		}

		public void TestNZSpecificControlVisibilityWhenMessageTypeChanges()
		{
			using (DeclarationForm form = new DeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				CustomsInvoiceHeaderUserControl userControl = form.CustomsBrokerageUserControl.SupplierHeaderUserControl as CustomsInvoiceHeaderUserControl;
				#region Check Field Visibility when Declaration is Export
				userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
				AssertEquals("ExchangeRateIndicatorDropEdit.Visible", true, userControl.JZ_ExchangeRateIndicatorDropEdit.Visible);
				AssertNotNull("ExchangeRateIndicator Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_ExchangeRateIndicator]);
				AssertEquals("RelationshipIndicatorDropEdit.Visible", false, userControl.JZ_RelationshipIndicatorDropEdit.Visible);
				AssertNull("RelationshipIndicator Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RelationshipIndicator]);
				AssertEquals("SupplierOrganisationControl.Visible", false, (userControl.FindSingle<ZOrganisationControl>("SupplierOrganisationControl")).Visible);
				AssertEquals("OverseasRegisteredSupplierGroupBox.Visible", false, (userControl.FindSingle<ZGroupBox>("OverseasRegisteredSupplierGroupBox")).Visible);
				AssertEquals("ImporterOrganisationControl.Visible", true, (userControl.FindSingle<ZOrganisationControl>("ImporterOrganisationControl")).Visible);
				AssertEquals("OtherTabPage.TabVisible - should be hidden for TSW Export", false, userControl.OtherTabPage.TabVisible);
				AssertNotNull("DefaultOrigin Column should still be in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin]);
				#endregion
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = form.CustomsBrokerageUserControl.SupplierHeaderUserControl as CustomsInvoiceHeaderUserControl;
				userControl.JobDeclaration = declaration;
				#region Check Field Visibility when Declaration is Import
				userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
				AssertEquals("ExchangeRateIndicatorDropEdit.Visible", false, userControl.JZ_ExchangeRateIndicatorDropEdit.Visible);
				AssertNull("ExchangeRateIndicator Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_ExchangeRateIndicator]);
				AssertEquals("RelationshipIndicatorDropEdit.Visible", true, userControl.JZ_RelationshipIndicatorDropEdit.Visible);
				AssertNotNull("RelationshipIndicator Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RelationshipIndicator]);
				AssertEquals("SupplierOrganisationControl.Visible", true, (userControl.FindSingle<ZOrganisationControl>("SupplierOrganisationControl")).Visible);
				AssertEquals("OverseasRegisteredSupplierGroupBox.Visible", true, (userControl.FindSingle<ZGroupBox>("OverseasRegisteredSupplierGroupBox")).Visible);
				AssertEquals("ImporterOrganisationControl.Visible", false, (userControl.FindSingle<ZOrganisationControl>("ImporterOrganisationControl")).Visible);
				userControl.InvoiceTabControl.SelectedTab = userControl.FindSingle<ZTabPage>("OtherTabPage");
				AssertEquals("CountryOfOriginCodeFindBox.Visible", true, (userControl.FindSingle<ZCodeFindBox>("CountryOfExportCodeFindBox")).Visible);
				AssertNotNull("DefaultOrigin Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin]);
				AssertEquals("PreferenceDropEdit.Visible", true, (userControl.FindSingle<ZDropEdit>("PreferenceDropEdit")).Visible);
				AssertEquals("PreferentialCountryGroupDropEdit.Visible", true, (userControl.FindSingle<ZDropEdit>("PreferentialCountryGroupDropEdit")).Visible);
				AssertNotNull("QualifiesForPreferentialDuty Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_DefaultQualifiesForPreferentialDuty]);
				AssertEquals("CountryOfExportCodeFindBox.Visible", true, (userControl.FindSingle<ZCodeFindBox>("CountryOfExportCodeFindBox")).Visible);
				AssertNotNull("DefaultExport Column in Grid", userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_RN_NKDefaultExport]);
				AssertEquals("OtherTabPage.TabVisible - should be visible for Import", true, userControl.OtherTabPage.TabVisible);
				#endregion
			}
		}

		public void TestGridId()
		{
			using (var control = new CustomsInvoiceHeaderUserControl())
			{
				AssertEquals("GridLayoutYmlykF3Qr3jStD2WohDXUQ==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Business.EDITariff_ReferenceFiles_NZ.Testing.NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
		#endregion
	}
}
