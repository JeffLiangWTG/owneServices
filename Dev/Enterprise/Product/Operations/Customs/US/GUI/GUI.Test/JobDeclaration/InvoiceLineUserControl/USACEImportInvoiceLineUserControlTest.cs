using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USACEImportInvoiceLineUserControl))]
	sealed class USACEImportInvoiceLineUserControlTest : ImportCustomsUserControlBasherAbstractTest
	{
		public void TestSanctionsTabVisible()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var conditionType2 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Mining);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0301930000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType1.PK, tariff1.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7102310000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType2.PK, tariff2.PK, "Mining Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			using (var frm = new JobDeclarationForm(declaration))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Sanctions, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				frm.Show();
				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)frm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.SanctionsTabPage.TabVisible", false, userControl.SanctionsTabPage.TabVisible);
				invoiceLine.JI_Tariff = "0301930000";
				invoiceLine.US_UC_NKCountryOfOrigin = "RU";
				AssertEquals("userControl.SanctionsTabPage.TabVisible", true, userControl.SanctionsTabPage.TabVisible);
				userControl.LineDetailTabControl.SelectedTab = userControl.SanctionsTabPage;
				AssertNotNull(userControl.SanctionsUserControl);
				AssertEquals("FilteredInvoiceLines", ((IDataBoundControl)userControl.SanctionsUserControl).DataMember);
				AssertEquals("userControl.SanctionsUserControl.Visible", true, userControl.SanctionsUserControl.Visible);
				AssertEquals("userControl.SanctionsUserControl.Enabled", true, userControl.SanctionsUserControl.Enabled);
			}
		}

		public void TestColumnCaption_ProductClaim()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			using (var frm = new JobDeclarationForm(declaration))
			{
				frm.Show();
				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var userControl = (USACEImportInvoiceLineUserControl)frm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;

				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var columnStyle = grid.GetColumnStyle(USAddInfoSchema.Constants.US_SecondarySPI);

				AssertEquals("Product Claim", columnStyle.Caption);
			}
		}

		public void TestSteelIronGroupBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				AssertEquals("SteelIronGroupBox is at the bottom.", true, userControl.SteelIronGroupBox.Visible);
				AssertEquals("SteelIronGroupBox has MeltedCountry", true, userControl.SteelIronGroupBox.Controls.Find("MeltedCountry", true)[0].Visible);
				AssertEquals("SteelIronGroupBox has CertificateOfOrigin", true, userControl.SteelIronGroupBox.Controls.Find("CertificateOfOrigin", true)[0].Visible);

				AssertNotNull("US_RN_NKCertOrigin on grid", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_RN_NKCertOrigin]);
				AssertNotNull("US_RN_NKMeltCtry on grid", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_RN_NKMeltCtry]);
			}
		}

		public void TestCBMARelatedControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;

				AssertEquals(false, invoiceLine.IsCBMAProductClaim);
				AssertEquals(false, invoiceLine.IsCBMA23Effective);

				AssertEquals(false, userControl.FlavorContentCreditIndCheckBox.Visible);

				AssertEquals(false, userControl.ControlledGroupNameDropEdit.Visible);
				AssertEquals(false, userControl.ForeignProducerIdentifierDropEdit.Visible);
				AssertEquals(false, userControl.AllocationQuantityCalcEdit.Visible);

				AssertEquals(false, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(false, userControl.CBMARateDesignationCodeDropEdit.Visible);
				AssertEquals(false, userControl.CBMARateCalcEdit.Visible);
			}

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;

				AssertEquals(true, invoiceLine.IsCBMAProductClaim);
				AssertEquals(false, invoiceLine.IsCBMA23Effective);

				AssertEquals(true, userControl.FlavorContentCreditIndCheckBox.Visible);

				AssertEquals(true, userControl.ControlledGroupNameDropEdit.Visible);
				AssertEquals(true, userControl.ForeignProducerIdentifierDropEdit.Visible);
				AssertEquals(true, userControl.AllocationQuantityCalcEdit.Visible);

				AssertEquals(false, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(false, userControl.CBMARateDesignationCodeDropEdit.Visible);
				AssertEquals(false, userControl.CBMARateCalcEdit.Visible);

				var flavorContentCreditIndCheckBox = form.FindSingle<ZCheckBox>("FlavorContentCreditIndCheckBox");
				var controlledGroupNameDropEdit = form.FindSingle<ZDropEdit>("ControlledGroupNameDropEdit");
				var foreignProducerIdentifierDropEdit = form.FindSingle<ZDropEdit>("ForeignProducerIdentifierDropEdit");
				var allocationQuantityCalcEdit = form.FindSingle<ZCalcEdit>("AllocationQuantityCalcEdit");

				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FlavorContentCreditInd", flavorContentCreditIndCheckBox.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_ControlledGroupName", controlledGroupNameDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FPI", foreignProducerIdentifierDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_AllocationQuantity", allocationQuantityCalcEdit.GetBindingMember());
			}

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;

				AssertEquals(true, invoiceLine.IsCBMAProductClaim);
				AssertEquals(true, invoiceLine.IsCBMA23Effective);

				AssertEquals(true, userControl.FlavorContentCreditIndCheckBox.Visible);

				AssertEquals(false, userControl.ControlledGroupNameDropEdit.Visible);
				AssertEquals(false, userControl.ForeignProducerIdentifierDropEdit.Visible);
				AssertEquals(false, userControl.AllocationQuantityCalcEdit.Visible);

				AssertEquals(true, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(true, userControl.CBMARateDesignationCodeDropEdit.Visible);
				AssertEquals(true, userControl.CBMARateCalcEdit.Visible);

				var flavorContentCreditIndCheckBox = form.FindSingle<ZCheckBox>("FlavorContentCreditIndCheckBox");
				var foreignProducerIdentifierTextBox = form.FindSingle<ZTextBox>("ForeignProducerIdentifierTextBox");
				var cbmaRateDesignationCodeDropEdit = form.FindSingle<ZDropEdit>("CBMARateDesignationCodeDropEdit");
				var cbmaRateCalcEdit = form.FindSingle<ZCalcEdit>("CBMARateCalcEdit");

				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FlavorContentCreditInd", flavorContentCreditIndCheckBox.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FPI", foreignProducerIdentifierTextBox.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_TTBRateDesignationCode", cbmaRateDesignationCodeDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_CBMADefaultTaxRate", cbmaRateCalcEdit.GetBindingMember());
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;
				AssertEquals(false, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(true, userControl.ForeignProducerIdentifierDropEdit.Visible);
			}

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;

				AssertEquals(true, invoiceLine.IsCBMAProductClaim);
				AssertEquals(false, invoiceLine.IsCBMA23Effective);

				AssertEquals(true, userControl.FlavorContentCreditIndCheckBox.Visible);

				AssertEquals(true, userControl.ControlledGroupNameDropEdit.Visible);
				AssertEquals(true, userControl.ForeignProducerIdentifierDropEdit.Visible);
				AssertEquals(true, userControl.AllocationQuantityCalcEdit.Visible);

				AssertEquals(false, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(false, userControl.CBMARateDesignationCodeDropEdit.Visible);
				AssertEquals(false, userControl.CBMARateCalcEdit.Visible);

				var flavorContentCreditIndCheckBox = form.FindSingle<ZCheckBox>("FlavorContentCreditIndCheckBox");
				var controlledGroupNameDropEdit = form.FindSingle<ZDropEdit>("ControlledGroupNameDropEdit");
				var foreignProducerIdentifierDropEdit = form.FindSingle<ZDropEdit>("ForeignProducerIdentifierDropEdit");
				var allocationQuantityCalcEdit = form.FindSingle<ZCalcEdit>("AllocationQuantityCalcEdit");

				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FlavorContentCreditInd", flavorContentCreditIndCheckBox.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_ControlledGroupName", controlledGroupNameDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FPI", foreignProducerIdentifierDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_AllocationQuantity", allocationQuantityCalcEdit.GetBindingMember());
			}

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;

				AssertEquals(true, invoiceLine.IsCBMAProductClaim);
				AssertEquals(true, invoiceLine.IsCBMA23Effective);

				AssertEquals(true, userControl.FlavorContentCreditIndCheckBox.Visible);

				AssertEquals(false, userControl.ControlledGroupNameDropEdit.Visible);
				AssertEquals(false, userControl.ForeignProducerIdentifierDropEdit.Visible);
				AssertEquals(false, userControl.AllocationQuantityCalcEdit.Visible);

				AssertEquals(true, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(true, userControl.CBMARateDesignationCodeDropEdit.Visible);
				AssertEquals(true, userControl.CBMARateCalcEdit.Visible);

				var flavorContentCreditIndCheckBox = form.FindSingle<ZCheckBox>("FlavorContentCreditIndCheckBox");
				var foreignProducerIdentifierTextBox = form.FindSingle<ZTextBox>("ForeignProducerIdentifierTextBox");
				var cbmaRateDesignationCodeDropEdit = form.FindSingle<ZDropEdit>("CBMARateDesignationCodeDropEdit");
				var cbmaRateCalcEdit = form.FindSingle<ZCalcEdit>("CBMARateCalcEdit");

				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FlavorContentCreditInd", flavorContentCreditIndCheckBox.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_FPI", foreignProducerIdentifierTextBox.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_TTBRateDesignationCode", cbmaRateDesignationCodeDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "FilteredInvoiceLines.US_CBMADefaultTaxRate", cbmaRateCalcEdit.GetBindingMember());
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as USACEImportInvoiceLineUserControl;
				AssertEquals(false, userControl.ForeignProducerIdentifierTextBox.Visible);
				AssertEquals(true, userControl.ForeignProducerIdentifierDropEdit.Visible);
			}
		}

		public void TestADDCVDQtyDepositValueVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;

				AssertEquals("DepositValue should be visible by default", true, userControl.ADDDepositValueCalcFindBox.Visible);
				AssertEquals("DepositValue should be visible by default", true, userControl.CVDDepositValueCalcFindBox.Visible);
				AssertEquals("DepositValue should be visible by default", false, userControl.ADDQtyCalcDropEdit.Visible);
				AssertEquals("DepositValue should be visible by default", false, userControl.CVDQtyCalcDropEdit.Visible);

				invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
				AssertEquals("Qty should be visible if rate type == 'S'", false, userControl.ADDDepositValueCalcFindBox.Visible);
				AssertEquals("Qty should be visible if rate type == 'S'", true, userControl.ADDQtyCalcDropEdit.Visible);

				AssertEquals("DepositValue should be visible by default", true, userControl.CVDDepositValueCalcFindBox.Visible);
				AssertEquals("DepositValue should be visible", false, userControl.CVDQtyCalcDropEdit.Visible);

				invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
				AssertEquals("Qty should be visible if rate type == 'S'", false, userControl.CVDDepositValueCalcFindBox.Visible);
				AssertEquals("Qty should be visible if rate type == 'S'", true, userControl.CVDQtyCalcDropEdit.Visible);
			}
		}

		public void TestNoNullReferenceException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLine.JI_JZ = ZGuid.Empty;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				invoice.Delete();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var listManager = userControl.CustomsInvoiceLinesBoundGrid.ListManager;
				listManager.Position = 0;
				AssertEquals("userControl.OGAReqTabPage.TabVisible", true, userControl.OGAReqTabPage.TabVisible);
				AssertEquals("userControl.OGAReqTabPage.Text", "PGA Requirements", userControl.OGAReqTabPage.Text);
				AssertEquals("userControl.FDAOtherTabPage.TabVisible", false, userControl.FDAOtherTabPage.TabVisible);
				AssertEquals("userControl.ACEFDATabPage.TabVisible", false, userControl.ACEFDATabPage.TabVisible);
				AssertEquals("userControl.OGATabPage.Text", "FCC/DOT", userControl.OGATabPage.Text);
				AssertEquals("userControl.DOTGroupBox.Visible", false, userControl.DOTGroupBox.Visible);
				var invoiceLine2 = userControl.CustomsInvoiceLinesBoundGrid.List.AddNew();
				AssertEquals("userControl.OGAReqTabPage.TabVisible", true, userControl.OGAReqTabPage.TabVisible);
				AssertEquals("userControl.OGAReqTabPage.Text", "PGA Requirements", userControl.OGAReqTabPage.Text);
				AssertEquals("userControl.FDAOtherTabPage.TabVisible", false, userControl.FDAOtherTabPage.TabVisible);
				AssertEquals("userControl.ACEFDATabPage.TabVisible", false, userControl.ACEFDATabPage.TabVisible);
				AssertEquals("userControl.OGATabPage.Text", "FCC/DOT", userControl.OGATabPage.Text);
				AssertEquals("userControl.DOTGroupBox.Visible", false, userControl.DOTGroupBox.Visible);
				AssertEquals("userControl.CPSCTabPage.TabVisible", false, userControl.CPSCTabPage.TabVisible);
				AssertEquals("userControl.DEATabPage.TabVisible", false, userControl.DEATabPage.TabVisible);
			}
		}

		public void TestACEFDAVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.ACEFDATabPage.TabVisible", false, userControl.ACEFDATabPage.TabVisible);
				AssertNull(userControl.acefdaUserControl);
				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.ACEFDATabPage.TabVisible", true, userControl.ACEFDATabPage.TabVisible);
				AssertNull(userControl.acefdaUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.ACEFDATabPage;
				AssertNotNull(userControl.acefdaUserControl);
				AssertEquals("FilteredInvoiceLines.ACE_FDALines", ((IDataBoundControl)userControl.acefdaUserControl).DataMember);
				AssertEquals("userControl.acefdaUserControl.Visible", true, userControl.acefdaUserControl.Visible);
				AssertEquals("userControl.acefdaUserControl.Enabled", true, userControl.acefdaUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.ACEFDATabPage.TabVisible", false, userControl.ACEFDATabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.acefdaUserControl).DataMember);
				AssertEquals("userControl.acefdaUserControl.Visible", false, userControl.acefdaUserControl.Visible);
				AssertEquals("userControl.acefdaUserControl.Enabled", false, userControl.acefdaUserControl.Enabled);
			}
		}

		public void TestACEFDAVisibilityAfterMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.ACEFDATabPage.TabVisible", true, userControl.ACEFDATabPage.TabVisible);
			}
		}

		public void TestAMSTAabVisibilityWhenDisclaimedForEG1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.EG1;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_AMSInd]);
				AssertEquals("AMSTabPage is NOT visible when disclaim program is EG1", false, userControl.AMSTabPage.TabVisible);

				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("AMSTabPage is visible when indicator is declared", true, userControl.AMSTabPage.TabVisible);

				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.B;
				invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;
				AssertEquals("AMSTabPage is visible when disclaim program is not EG1", true, userControl.AMSTabPage.TabVisible);

				invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.EG1;
				AssertEquals("AMSTabPage is NOT visible when disclaim program is EG1", false, userControl.AMSTabPage.TabVisible);
			}
		}

		public void TestAMSVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_AMSInd]);
				AssertEquals("userControl.AMSTabPage.TabVisible", false, userControl.AMSTabPage.TabVisible);
				AssertNull(userControl.amsUserControl);
				AssertNull(userControl.amsDisclaimControl);
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.AMSTabPage.TabVisible);
				AssertNull(userControl.amsUserControl);
				AssertNull(userControl.amsDisclaimControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.AMSTabPage;
				AssertNotNull(userControl.amsUserControl);
				AssertNotNull(userControl.amsDisclaimControl);
				AssertEquals("userControl.amsUserControl.Visible", true, userControl.amsUserControl.Visible);
				AssertEquals("FilteredInvoiceLines.AMSLines", ((IDataBoundControl)userControl.amsUserControl).DataMember);
				AssertEquals("userControl.amsUserControl.Visible", true, userControl.amsUserControl.Visible);
				AssertEquals("userControl.amsUserControl.Enabled", true, userControl.amsUserControl.Enabled);
				AssertEquals("userControl.amsDisclaimControl.Visible", false, userControl.amsDisclaimControl.Visible);
				AssertEquals("", ((IDataBoundControl)userControl.amsDisclaimControl).DataMember);
				AssertEquals("userControl.amsDisclaimControl.Visible", false, userControl.amsDisclaimControl.Visible);
				AssertEquals("userControl.amsDisclaimControl.Enabled", false, userControl.amsDisclaimControl.Enabled);

				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_AMSInd = ZString.Empty;
				AssertEquals("userControl.AMSTabPage.TabVisible", false, userControl.AMSTabPage.TabVisible);
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.AMSTabPage.TabVisible);
				invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.AMSTabPage.TabVisible);
				userControl.LineDetailTabControl.SelectedTab = userControl.AMSTabPage;
				AssertNotNull(userControl.amsUserControl);
				AssertNotNull(userControl.amsDisclaimControl);
				invoiceLine.US_NOPInd = ZString.Empty;
				AssertEquals("userControl.amsUserControl.Visible", true, userControl.amsUserControl.Visible);
				AssertEquals("FilteredInvoiceLines.AMSLines", ((IDataBoundControl)userControl.amsUserControl).DataMember);
				AssertEquals("userControl.amsUserControl.Visible", true, userControl.amsUserControl.Visible);
				AssertEquals("userControl.amsUserControl.Enabled", true, userControl.amsUserControl.Enabled);
				AssertEquals("userControl.amsDisclaimControl.Visible", true, userControl.amsDisclaimControl.Visible);
				AssertEquals("FilteredInvoiceLines", ((IDataBoundControl)userControl.amsDisclaimControl).DataMember);
				AssertEquals("userControl.amsDisclaimControl.Visible", true, userControl.amsDisclaimControl.Visible);
				AssertEquals("userControl.amsDisclaimControl.Enabled", true, userControl.amsDisclaimControl.Enabled);
			}
		}

		public void TestCPSCVisibility()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertEquals("userControl.CPSCTabPage.TabVisible", false, userControl.CPSCTabPage.TabVisible);
					AssertNull(userControl.cpscUserControl);
					invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("userControl.CPSCTabPage.TabVisible", true, userControl.CPSCTabPage.TabVisible);
					AssertNull(userControl.cpscUserControl);
					userControl.LineDetailTabControl.SelectedTab = userControl.CPSCTabPage;
					AssertNotNull(userControl.cpscUserControl);
					AssertEquals("FilteredInvoiceLines.CPSCHeaders", ((IDataBoundControl)userControl.cpscUserControl).DataMember);
					AssertEquals("userControl.cpscUserControl.Visible", true, userControl.cpscUserControl.Visible);
					AssertEquals("userControl.cpscUserControl.Enabled", true, userControl.cpscUserControl.Enabled);
					AssertEquals("userControl.cpscDisclaimControl.Visible", false, userControl.cpscDisclaimControl.Visible);
					AssertEquals("userControl.cpscDisclaimControl.Enabled", false, userControl.cpscDisclaimControl.Enabled);
					userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
					invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
					AssertEquals("userControl.CPSCTabPage.TabVisible", false, userControl.CPSCTabPage.TabVisible);
					AssertEquals("", ((IDataBoundControl)userControl.cpscUserControl).DataMember);
					AssertEquals("", ((IDataBoundControl)userControl.cpscDisclaimControl).DataMember);
					AssertEquals("userControl.cpscUserControl.Visible", false, userControl.cpscUserControl.Visible);
					AssertEquals("userControl.cpscUserControl.Enabled", false, userControl.cpscUserControl.Enabled);
					AssertEquals("userControl.cpscDisclaimControl.Visible", false, userControl.cpscDisclaimControl.Visible);
					AssertEquals("userControl.cpscDisclaimControl.Enabled", false, userControl.cpscDisclaimControl.Enabled);
					invoiceLine.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
					userControl.LineDetailTabControl.SelectedTab = userControl.CPSCTabPage;
					AssertEquals("userControl.CPSCTabPage.TabVisible", true, userControl.CPSCTabPage.TabVisible);
					AssertNotNull(userControl.cpscDisclaimControl);
					AssertEquals("FilteredInvoiceLines.CPSCHeaders", ((IDataBoundControl)userControl.cpscDisclaimControl).DataMember);
					AssertEquals("userControl.cpscDisclaimControl.Visible", true, userControl.cpscDisclaimControl.Visible);
					AssertEquals("userControl.cpscDisclaimControl.Enabled", true, userControl.cpscDisclaimControl.Enabled);
					AssertEquals("userControl.cpscUserControl.Visible", false, userControl.cpscUserControl.Visible);
					AssertEquals("userControl.cpscUserControl.Enabled", false, userControl.cpscUserControl.Enabled);
					invoiceLine.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.B;
					AssertEquals("userControl.CPSCTabPage.TabVisible", false, userControl.CPSCTabPage.TabVisible);
					AssertEquals("", ((IDataBoundControl)userControl.cpscUserControl).DataMember);
					AssertEquals("", ((IDataBoundControl)userControl.cpscDisclaimControl).DataMember);
					AssertEquals("userControl.cpscUserControl.Visible", false, userControl.cpscUserControl.Visible);
					AssertEquals("userControl.cpscUserControl.Enabled", false, userControl.cpscUserControl.Enabled);
					AssertEquals("userControl.cpscDisclaimControl.Visible", false, userControl.cpscDisclaimControl.Visible);
					AssertEquals("userControl.cpscDisclaimControl.Enabled", false, userControl.cpscDisclaimControl.Enabled);
				}
			}
		}

		public void TestATFVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.ATFTabPage.TabVisible", false, userControl.ATFTabPage.TabVisible);
				AssertNull(userControl.atfUserControl);
				invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.ATFTabPage.TabVisible", true, userControl.ATFTabPage.TabVisible);
				AssertNull(userControl.atfUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.ATFTabPage;
				AssertNotNull(userControl.atfUserControl);
				AssertEquals("FilteredInvoiceLines.ATFLines", ((IDataBoundControl)userControl.atfUserControl).DataMember);
				AssertEquals("userControl.atfUserControl.Visible", true, userControl.atfUserControl.Visible);
				AssertEquals("userControl.atfUserControl.Enabled", true, userControl.atfUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.ATFTabPage.TabVisible", false, userControl.ATFTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.atfUserControl).DataMember);
				AssertEquals("userControl.atfUserControl.Visible", false, userControl.atfUserControl.Visible);
				AssertEquals("userControl.atfUserControl.Enabled", false, userControl.atfUserControl.Enabled);
			}
		}

		public void TestODSVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ODSInd]);
				AssertEquals("userControl.ODSTabPage.TabVisible", false, userControl.ODSTabPage.TabVisible);
				AssertNull(userControl.odsAndTSCAControl);
				invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.ODSTabPage.TabVisible", true, userControl.ODSTabPage.TabVisible);
				AssertNull(userControl.odsAndTSCAControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.ODSTabPage;
				AssertNotNull(userControl.odsAndTSCAControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.ODSTabPage.TabVisible", false, userControl.ODSTabPage.TabVisible);
				AssertEquals("FilteredInvoiceLines", ((IDataBoundControl)userControl.odsAndTSCAControl).DataMember);
				AssertEquals("userControl.odsAndTSCAControl.Visible", false, userControl.odsAndTSCAControl.Visible);
				AssertEquals("userControl.odsAndTSCAControl.Enabled", true, userControl.odsAndTSCAControl.Enabled);
			}
		}

		public void TestVNEVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_VNEInd]);
				AssertEquals("userControl.VNETabPage.TabVisible", false, userControl.VNETabPage.TabVisible);
				AssertNull(userControl.vneUserControl);
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.VNETabPage.TabVisible", true, userControl.VNETabPage.TabVisible);
				AssertNull(userControl.vneUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.VNETabPage;
				AssertNotNull(userControl.vneUserControl);
				AssertEquals("FilteredInvoiceLines.VehicleLines", ((IDataBoundControl)userControl.vneUserControl).DataMember);
				AssertEquals("userControl.vneUserControl.Visible", true, userControl.vneUserControl.Visible);
				AssertEquals("userControl.vneUserControl.Enabled", true, userControl.vneUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.VNETabPage.TabVisible", false, userControl.VNETabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.vneUserControl).DataMember);
				AssertEquals("userControl.vneUserControl.Visible", false, userControl.vneUserControl.Visible);
				AssertEquals("userControl.vneUserControl.Enabled", false, userControl.vneUserControl.Enabled);
			}
		}

		public void TestPSTVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_PSTIndicator]);
				AssertEquals("userControl.PSTTabPage.TabVisible", false, userControl.PSTTabPage.TabVisible);
				AssertNull(userControl.pstUserControl);
				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.PSTTabPage.TabVisible", true, userControl.PSTTabPage.TabVisible);
				AssertNull(userControl.pstUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.PSTTabPage;
				AssertNotNull(userControl.pstUserControl);
				AssertEquals("FilteredInvoiceLines.PSTLines", ((IDataBoundControl)userControl.pstUserControl).DataMember);
				AssertEquals("userControl.pstUserControl.Visible", true, userControl.pstUserControl.Visible);
				AssertEquals("userControl.pstUserControl.Enabled", true, userControl.pstUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.PSTTabPage.TabVisible", true, userControl.PSTTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.pstUserControl).DataMember);
				AssertEquals("userControl.pstUserControl.Visible", false, userControl.pstUserControl.Visible);
				AssertEquals("userControl.pstUserControl.Enabled", false, userControl.pstUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.PSTTabPage;
				AssertEquals("FilteredInvoiceLines", ((IDataBoundControl)userControl.pstDisclaimControl).DataMember);
				AssertEquals("userControl.pstDisclaimControl.Visible", true, userControl.pstDisclaimControl.Visible);
				AssertEquals("userControl.pstDisclaimControl.Enabled", true, userControl.pstDisclaimControl.Enabled);
			}
		}

		public void TestHFCVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.HFCTabPage.TabVisible", false, userControl.HFCTabPage.TabVisible);
				AssertNull(userControl.HFCUserControl);
				invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.HFCTabPage.TabVisible", true, userControl.HFCTabPage.TabVisible);
				AssertNull(userControl.HFCUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.HFCTabPage;
				AssertNotNull(userControl.HFCUserControl);
				AssertEquals("FilteredInvoiceLines.USHFCHeaders", ((IDataBoundControl)userControl.HFCUserControl).DataMember);
				AssertEquals("userControl.HFCUserControl.Visible", true, userControl.HFCUserControl.Visible);
				AssertEquals("userControl.HFCUserControl.Enabled", true, userControl.HFCUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.HFCTabPage.TabVisible", false, userControl.HFCTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.HFCUserControl).DataMember);
				AssertEquals("userControl.HFCUserControl.Visible", false, userControl.HFCUserControl.Visible);
				AssertEquals("userControl.HFCUserControl.Enabled", false, userControl.HFCUserControl.Enabled);
			}
		}

		public void TestOGAReqVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.OGAReqTabPage.TabVisible", true, userControl.OGAReqTabPage.TabVisible);
				AssertNull(userControl.ogapgaRequirementsControl);
				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				AssertNull(userControl.ogapgaRequirementsControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				AssertNotNull(userControl.ogapgaRequirementsControl);
			}
		}

		public void TestNHTSAVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_NHTSAIndicator]);
				AssertEquals("userControl.NHTSATabPage.TabVisible", false, userControl.NHTSATabPage.TabVisible);
				AssertNull(userControl.nhtsaUserControl);
				invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NHTSATabPage.TabVisible", true, userControl.NHTSATabPage.TabVisible);
				AssertNull(userControl.nhtsaUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.NHTSATabPage;
				AssertNotNull(userControl.nhtsaUserControl);
				AssertEquals("FilteredInvoiceLines.NHTSALines", ((IDataBoundControl)userControl.nhtsaUserControl).DataMember);
				AssertEquals("userControl.nhtsaUserControl.Visible", true, userControl.nhtsaUserControl.Visible);
				AssertEquals("userControl.nhtsaUserControl.Enabled", true, userControl.nhtsaUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.NHTSATabPage.TabVisible", false, userControl.NHTSATabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.nhtsaUserControl).DataMember);
				AssertEquals("userControl.nhtsaUserControl.Visible", false, userControl.nhtsaUserControl.Visible);
				AssertEquals("userControl.nhtsaUserControl.Enabled", false, userControl.nhtsaUserControl.Enabled);
			}
		}

		public void TestNMFSVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;

				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_NMFS370Ind]);
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_NMFSAMRInd]);
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_NMFSHMSInd]);
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_NMFSSIMPInd]);
				AssertEquals("userControl.NMFSTabPage.TabVisible", false, userControl.NMFSTabPage.TabVisible);
				AssertNull(userControl.nmfsUserControl);
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				AssertNull(userControl.nmfsUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.NMFSTabPage;
				AssertNotNull(userControl.nmfsUserControl);
				AssertEquals("FilteredInvoiceLines.NMFSLines", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", true, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", true, userControl.nmfsUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.NMFSTabPage.TabVisible", false, userControl.NMFSTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", false, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", false, userControl.nmfsUserControl.Enabled);
				invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				AssertEquals("FilteredInvoiceLines.NMFSLines", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", false, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", true, userControl.nmfsUserControl.Enabled);
				invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.NMFSTabPage.TabVisible", false, userControl.NMFSTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", false, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", false, userControl.nmfsUserControl.Enabled);
				invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				AssertEquals("FilteredInvoiceLines.NMFSLines", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", false, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", true, userControl.nmfsUserControl.Enabled);
				invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.NMFSTabPage.TabVisible", false, userControl.NMFSTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", false, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", false, userControl.nmfsUserControl.Enabled);
				invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				AssertEquals("FilteredInvoiceLines.NMFSLines", ((IDataBoundControl)userControl.nmfsUserControl).DataMember);
				AssertEquals("userControl.nmfsUserControl.Visible", false, userControl.nmfsUserControl.Visible);
				AssertEquals("userControl.nmfsUserControl.Enabled", true, userControl.nmfsUserControl.Enabled);
			}
		}

		public void TestFDAAndDOTVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;

				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;

				Assert(userControl.ACEFDATabPage.TabVisible);
				Assert(userControl.NHTSATabPage.TabVisible);
				Assert(!userControl.FDAOtherTabPage.TabVisible);
				Assert(!userControl.OGATabPage.TabVisible);

				invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
				Assert(userControl.OGATabPage.TabVisible);
				AssertEquals("FCC", userControl.OGATabPage.Text);
				Assert(!userControl.DOTGroupBox.Visible);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;

				Assert(!userControl.ACEFDATabPage.TabVisible);
				Assert(!userControl.NHTSATabPage.TabVisible);
				Assert(userControl.FDAOtherTabPage.TabVisible);
				Assert(userControl.OGATabPage.TabVisible);
				AssertEquals("FCC/DOT", userControl.OGATabPage.Text);

				userControl.LineDetailTabControl.SelectedTab = userControl.OGATabPage;
				Assert(userControl.DOTGroupBox.Visible);
			}
		}

		public void TestTTBVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;

				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_TTBInd]);
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_TTBDisclaimReason]);
				AssertEquals("userControl.TTBTabPage.TabVisible", false, userControl.TTBTabPage.TabVisible);
				AssertNull(userControl.ttbUserControl);
				invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.TTBTabPage.TabVisible", true, userControl.TTBTabPage.TabVisible);
				AssertNull(userControl.ttbUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.TTBTabPage;
				AssertNotNull(userControl.ttbUserControl);
				AssertEquals("FilteredInvoiceLines.TTBLines", ((IDataBoundControl)userControl.ttbUserControl).DataMember);
				AssertEquals("userControl.ttbUserControl.Visible", true, userControl.ttbUserControl.Visible);
				AssertEquals("userControl.ttbUserControl.Enabled", true, userControl.ttbUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.TTBTabPage.TabVisible", false, userControl.TTBTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.ttbUserControl).DataMember);
				AssertEquals("userControl.ttbUserControl.Visible", false, userControl.ttbUserControl.Visible);
				AssertEquals("userControl.ttbUserControl.Enabled", false, userControl.ttbUserControl.Enabled);
			}
		}

		public void TestAPHISVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_APHISInd]);
				AssertEquals("userControl.APHISTabPage.TabVisible", false, userControl.APHISTabPage.TabVisible);
				AssertNull(userControl.aphisUserControl);
				invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.APHISTabPage.TabVisible", true, userControl.APHISTabPage.TabVisible);
				AssertNull(userControl.aphisUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.APHISTabPage;
				AssertNotNull(userControl.aphisUserControl);
				AssertEquals("FilteredInvoiceLines.APHISHeaders", ((IDataBoundControl)userControl.aphisUserControl).DataMember);
				AssertEquals("userControl.aphisUserControl.Visible", true, userControl.aphisUserControl.Visible);
				AssertEquals("userControl.aphisUserControl.Enabled", true, userControl.aphisUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.APHISTabPage.TabVisible", false, userControl.APHISTabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.aphisUserControl).DataMember);
				AssertEquals("userControl.aphisUserControl.Visible", false, userControl.aphisUserControl.Visible);
				AssertEquals("userControl.aphisUserControl.Enabled", false, userControl.aphisUserControl.Enabled);
			}
		}

		public void TestFWSVisibility()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGAFWS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableENS = true;

				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertNotNull("Column is available", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_FWSInd]);
					AssertEquals("userControl.FWSTabPage.TabVisible", false, userControl.FWSTabPage.TabVisible);
					AssertNull(userControl.fwsUserControl);
					invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("userControl.FWSTabPage.TabVisible", true, userControl.FWSTabPage.TabVisible);
					AssertNull(userControl.fwsUserControl);
					userControl.LineDetailTabControl.SelectedTab = userControl.FWSTabPage;
					AssertNotNull(userControl.fwsUserControl);
					AssertEquals("FilteredInvoiceLines.FWSHeaders", ((IDataBoundControl)userControl.fwsUserControl).DataMember);
					AssertEquals("userControl.fwsUserControl.Visible", true, userControl.fwsUserControl.Visible);
					AssertEquals("userControl.fwsUserControl.Enabled", true, userControl.fwsUserControl.Enabled);
					userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
					invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
					AssertEquals("userControl.FWSTabPage.TabVisible", false, userControl.FWSTabPage.TabVisible);
					AssertEquals("", ((IDataBoundControl)userControl.fwsUserControl).DataMember);
					AssertEquals("userControl.fwsUserControl.Visible", false, userControl.fwsUserControl.Visible);
					AssertEquals("userControl.fwsUserControl.Enabled", false, userControl.fwsUserControl.Enabled);
				}
			}
		}

		public void TestADDCVDDepositRateDescriptionVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;

				AssertEquals("Description box for ADD Deposit Rate should be visible by default", false, userControl.ADDDepositRateDropEdit.ShowDescriptionBox);
				AssertEquals("Description box for CVD Deposit Rate should be visible by default", false, userControl.CVDDepositRateDropEdit.ShowDescriptionBox);
			}
		}

		public void TestADDGroupBoxAndCVDGroupBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;

				AssertEquals("IsADCVDCertCheckBox should be visible", true, userControl.IsADCVDCertCheckBox.Visible);
				AssertEquals("ADDGroupBox should be visible", true, userControl.ADDGroupBox.Visible);
				AssertEquals("IsADDBondedCheckBox should be visible", true, userControl.IsADDBondedCheckBox.Visible);
				AssertEquals("CVDGroupBox should be visible", true, userControl.CVDGroupBox.Visible);
				AssertEquals("IsCVDBondedCheckBox should be visible", true, userControl.IsCVDBondedCheckBox.Visible);
			}
		}

		public void TestConsumptionFTZControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(true, userControl.MostInnerPackQtyCalcDropEdit.Visible);

				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
				AssertEquals(false, userControl.MostInnerPackQtyCalcDropEdit.Visible);
			}
		}

		public void TestInnermostPacksColumnVisibility()
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
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var innermostColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ManifestQty];
				AssertNotNull("Column is available in ACE", innermostColumn);
				AssertContains("Innermost Pack Qty", innermostColumn.ColumnStyle.HeaderText);
				AssertContains("Innermost Pack Qty", innermostColumn.GroupName.ToString());
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var innermostColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ManifestQty];
				AssertNotNull("Column is available in ACE", innermostColumn);
				AssertContains("FTZ Pack Qty", innermostColumn.ColumnStyle.HeaderText);
				AssertContains("FTZ Pack Qty", innermostColumn.GroupName.ToString());
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var innermostColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ManifestQty];
				AssertNotNull("Column is available in ACS", innermostColumn);
				AssertContains("Innermost Pack Qty", innermostColumn.ColumnStyle.HeaderText);
				AssertContains("Innermost Pack Qty", innermostColumn.GroupName.ToString());
			}
		}

		public void TestDEAVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.DEATabPage.TabVisible", false, userControl.DEATabPage.TabVisible);
				AssertNull(userControl.DEAUserControl);
				invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.DEATabPage.TabVisible", true, userControl.DEATabPage.TabVisible);
				AssertNull(userControl.DEAUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.DEATabPage;
				AssertNotNull(userControl.DEAUserControl);
				AssertEquals("FilteredInvoiceLines.DEAHeaders", ((IDataBoundControl)userControl.DEAUserControl).DataMember);
				AssertEquals("userControl.deaUserControl.Visible", true, userControl.DEAUserControl.Visible);
				AssertEquals("userControl.deaUserControl.Enabled", true, userControl.DEAUserControl.Enabled);
				userControl.LineDetailTabControl.SelectedTab = userControl.OGAReqTabPage;
				invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.DEATabPage.TabVisible", false, userControl.DEATabPage.TabVisible);
				AssertEquals("", ((IDataBoundControl)userControl.DEAUserControl).DataMember);
				AssertEquals("userControl.deaUserControl.Visible", false, userControl.DEAUserControl.Visible);
				AssertEquals("userControl.deaUserControl.Enabled", false, userControl.DEAUserControl.Enabled);
			}
		}

		public void TestColumnHasSetIndicator()
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
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var setInd = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_SetInd];
				AssertNotNull("Column is available in ACE", setInd);
				AssertContains("Set Indicator", setInd.ColumnStyle.HeaderText);
			}
		}

		public void TestShouldNotAccessDeletedBusinessObject()
		{
			var orgHeader = Factory.New<MasterFiles.Business.OrgHeader>();
			orgHeader.OH_Code = "TESTMANU";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.JE_OA_ManufacturerAddress = orgHeader.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.CustomsInvoiceLinesBoundGrid.Select(0);
				userControl.LineDetailTabControl.SelectedTab = userControl.CPSCTabPage;
				var menu = userControl.CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.FindByText("Delete");
				AssertNotNull(menu);
				AssertNoExceptionThrown(() =>
				{
					menu.PerformClick();
				});
			}
		}

		public void TestInvoiceLineGridColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_PrivilegedStatusDate]);
				AssertNull(userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FTZCurrentTariffFormatted]);
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.SellerOrgPK, "Seller", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), "Seller");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_OA_Seller, "Seller Address", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), "Seller");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.ShipToPartyOrgPK, "Ship To Party", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), "ShipToParty");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_OA_ShipToPartyAddress, "ShipToParty Address", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), "ShipToParty");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ProductExclusion, "Product Exclusion", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), "Product Exclusion");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ExclusionNumber, "Exclusion Number", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), "Product Exclusion");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FirstPermitLicenseType, "License/Permit Type", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FirstPermitLicenseNumber, "License/Permit Number", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150));

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_PrivilegedStatusDate, "Privileged Status Date", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.FTZCurrentTariffFormatted, "FTZ Current Tariff", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.SellerOrgPK, "Seller", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), "Seller");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_OA_Seller, "Seller Address", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), "Seller");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.ShipToPartyOrgPK, "Ship To Party", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), "ShipToParty");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_OA_ShipToPartyAddress, "ShipToParty Address", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), "ShipToParty");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ProductExclusion, "Product Exclusion", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), "Product Exclusion");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ExclusionNumber, "Exclusion Number", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), "Product Exclusion");
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FirstPermitLicenseType, "License/Permit Type", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FirstPermitLicenseNumber, "License/Permit Number", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150));
			}
		}

		public void TestCBMAColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_ControlledGroupName, "Controlled Group Name", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(136));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FPI, "Foreign Producer Identifier", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FPI, "Foreign Producer Identifier", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_AllocationQuantity, "Allocation Quantity", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_FlavorContentCreditInd, "Flavor Content Credit Indicator", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(172));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_TTBRateDesignationCode, "CBMA Rate Desig.", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150));
				AssertColumn(userControl.CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.US_CBMADefaultTaxRate, "CBMA Rate", false, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117));
			}
		}

		public void TestAluminumSmeltAndCastCountryControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				AssertEquals(true, userControl.AluminumSmeltGroupBox.Visible);

				var primaryCountryNAColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_Prim_NA];
				AssertNotNull("Primary Country N/A columns is available", primaryCountryNAColumn);
				var primaryCountryColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_RN_NKPrimCtry];
				AssertNotNull("Primary Country columns is available", primaryCountryColumn);
				var secondaryCountryNAColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_Sec_NA];
				AssertNotNull("Secondary Country N/A columns is available", secondaryCountryNAColumn);
				var secondaryCountryColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_RN_NKSecCtry];
				AssertNotNull("Secondary Country columns is available", secondaryCountryColumn);
				var castCountryColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_RN_NKCastCtry];
				AssertNotNull("Cast Country columns is available", castCountryColumn);
			}
		}

		public void TestAdditionalProvTariffColumnAndUserControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var additionalProvTariff1Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["SupFormattedAdditionalTariff1"];
				AssertNotNull("Prov Add. Tariff 1 columns is available", additionalProvTariff1Column);
				AssertEquals("Prov Add. Tariff 1", additionalProvTariff1Column.ColumnStyle.HeaderText);
				var additionalProvDuty1Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff1Duty"];
				AssertNotNull("Prov Add. Duty 1 columns is available", additionalProvDuty1Column);
				AssertEquals("Prov Add. Duty 1", additionalProvDuty1Column.ColumnStyle.HeaderText);
				var overrideAdditionalProvDuty1Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_OverrideSupAdditionalTariff1Duty"];
				AssertNotNull("Override Prov Add. Duty 1 columns is available", overrideAdditionalProvDuty1Column);
				AssertEquals("Override Prov Add. Duty 1", overrideAdditionalProvDuty1Column.ColumnStyle.HeaderText);
				var supAdditionalTariff1QtyColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff1Qty"];
				AssertNotNull("Prov/Prog Add. Qty 1 column is available", supAdditionalTariff1QtyColumn);
				var additionalProvTariff2Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["SupFormattedAdditionalTariff2"];
				AssertNotNull("Prov Add. Tariff 2 columns is available", additionalProvTariff2Column);
				AssertEquals("Prov Add. Tariff 2", additionalProvTariff2Column.ColumnStyle.HeaderText);
				var additionalProvDuty2Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff2Duty"];
				AssertNotNull("Prov Add. Duty 2 columns is available", additionalProvDuty2Column);
				AssertEquals("Prov Add. Duty 2", additionalProvDuty2Column.ColumnStyle.HeaderText);
				var overrideAdditionalProvDuty2Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_OverrideSupAdditionalTariff2Duty"];
				AssertNotNull("Override Prov Add. Duty 2 columns is available", overrideAdditionalProvDuty2Column);
				AssertEquals("Override Prov Add. Duty 2", overrideAdditionalProvDuty2Column.ColumnStyle.HeaderText);
				var supAdditionalTariff2QtyColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff2Qty"];
				AssertNotNull("Prov/Prog Add. Qty 2 column is available", supAdditionalTariff2QtyColumn);
				var additionalProvTariff3Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["SupFormattedAdditionalTariff3"];
				AssertNotNull("Prov Add. Tariff 3 columns is available", additionalProvTariff3Column);
				AssertEquals("Prov Add. Tariff 3", additionalProvTariff3Column.ColumnStyle.HeaderText);
				var additionalProvDuty3Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff3Duty"];
				AssertNotNull("Prov Add. Duty 3 columns is available", additionalProvDuty3Column);
				AssertEquals("Prov Add. Duty 3", additionalProvDuty3Column.ColumnStyle.HeaderText);
				var overrideAdditionalProvDuty3Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_OverrideSupAdditionalTariff3Duty"];
				AssertNotNull("Override Prov Add. Duty 3 columns is available", overrideAdditionalProvDuty3Column);
				AssertEquals("Override Prov Add. Duty 3", overrideAdditionalProvDuty3Column.ColumnStyle.HeaderText);
				var supAdditionalTariff3QtyColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff3Qty"];
				AssertNotNull("Prov/Prog Add. Qty 3 column is available", supAdditionalTariff3QtyColumn);
				var additionalProvTariff4Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["SupFormattedAdditionalTariff4"];
				AssertNotNull("Prov Add. Tariff 4 columns is available", additionalProvTariff4Column);
				AssertEquals("Prov Add. Tariff 4", additionalProvTariff4Column.ColumnStyle.HeaderText);
				var additionalProvDuty4Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff4Duty"];
				AssertNotNull("Prov Add. Duty 4 columns is available", additionalProvDuty4Column);
				AssertEquals("Prov Add. Duty 4", additionalProvDuty4Column.ColumnStyle.HeaderText);
				var overrideAdditionalProvDuty4Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_OverrideSupAdditionalTariff4Duty"];
				AssertNotNull("Override Prov Add. Duty 4 columns is available", overrideAdditionalProvDuty4Column);
				AssertEquals("Override Prov Add. Duty 4", overrideAdditionalProvDuty4Column.ColumnStyle.HeaderText);
				var supAdditionalTariff4QtyColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff4Qty"];
				AssertNotNull("Prov/Prog Add. Qty 4 column is available", supAdditionalTariff4QtyColumn);
				var additionalProvTariff5Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["SupFormattedAdditionalTariff5"];
				AssertNotNull("Prov Add. Tariff 5 columns is available", additionalProvTariff5Column);
				AssertEquals("Prov Add. Tariff 5", additionalProvTariff5Column.ColumnStyle.HeaderText);
				var additionalProvDuty5Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff5Duty"];
				AssertNotNull("Prov Add. Duty 5 columns is available", additionalProvDuty5Column);
				AssertEquals("Prov Add. Duty 5", additionalProvDuty5Column.ColumnStyle.HeaderText);
				var overrideAdditionalProvDuty5Column = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_OverrideSupAdditionalTariff5Duty"];
				AssertNotNull("Override Prov Add. Duty 5 columns is available", overrideAdditionalProvDuty5Column);
				AssertEquals("Override Prov Add. Duty 5", overrideAdditionalProvDuty5Column.ColumnStyle.HeaderText);
				var supAdditionalTariff5QtyColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns["US_SupAdditionalTariff5Qty"];
				AssertNotNull("Prov/Prog Add. Qty 5 column is available", supAdditionalTariff5QtyColumn);
			}
		}

		protected override Type UserControlToBashType => typeof(USACEImportInvoiceLineUserControl);

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}

		void AssertColumn(ZGrid grid, string columnName, string caption, bool isVisible, int witdth, string groupName = null)
		{
			CombineAssertions(columnName, () =>
			{
				var columnInfo = grid.GetColumnStyle(columnName);
				AssertEquals("Caption", caption, grid.GetColumnCaption(columnName));
				AssertEquals("IsVisible", isVisible, columnInfo.IsVisible);
				AssertEquals("Width", witdth, columnInfo.Width);
				if (groupName != null)
				{
					AssertEquals("GroupName", groupName, columnInfo.GroupName.Caption);
				}
			});
		}
	}
}
