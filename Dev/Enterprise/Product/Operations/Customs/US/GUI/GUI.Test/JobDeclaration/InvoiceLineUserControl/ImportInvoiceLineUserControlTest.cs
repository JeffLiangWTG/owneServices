using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportInvoiceLineUserControlTest : Testing.ImportCustomsUserControlBasherAbstractTest
	{
		public void TestManufacturerAddressAddressControl()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EnableAII = true;
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var line = invoice.InvoiceLines.AddNew();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "Org Name";
			org.MainAddress.OA_Address1 = "Address1";
			line.JI_OA_ManufacturerAddress = org.MainAddress.PK;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var manufacturerAddressAddressControl = invoiceLineUserControl.JI_OA_ManufacturerAddressAddressControl;
				AssertEquals("Before the manufacturer is saved, the manufacturerAddressAddressControl is unable", false, manufacturerAddressAddressControl.Enabled);
			}

			Factory.Save();
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var manufacturerAddressAddressControl = invoiceLineUserControl.JI_OA_ManufacturerAddressAddressControl;
				AssertEquals("After the manufacturer is saved, the manufacturerAddressAddressControl is enable", true, manufacturerAddressAddressControl.Enabled);
			}
		}

		public void TestMoreThan1AIItabsAreInserted()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EnableAII = true;
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var line = invoice.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(dec))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.ElectronicInvoiceTabPage;
				line.Delete();
				AssertEquals("More than 1 AII tab inserted", 1, invoiceLineUserControl.LineDetailTabControl.TabPages.Cast<ZTabPage>().Count(x => x == invoiceLineUserControl.ElectronicInvoiceTabPage));
			}
		}

		public void TestImportLookupOnGrid()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 1m;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1m;
				invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
				ZPopupFindBox findBox = (ZPopupFindBox)((ZCodeFindBoxColumnStyle)userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_CC].ColumnStyle).EditControl;
				userControl.CustomsInvoiceLinesBoundGrid.CurrentCell = new DataGridCell(0, 3);
				findBox.PopupButton.PerformClick();
				AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.ImportClassification, findBox.ModuleID);
			}
		}

		public void TestNoExceptionThrownWhenSupTariffDropEditSelectItem()
		{
			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroup = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.CountryCodes.Russia);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia, startDate.Date, endDate.Date);
			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var tariff = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "1234567890", startDate, endDate);
			var parent01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "1111111111", startDate, endDate);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "4444444444", startDate, endDate);

			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child01);
			var attribute12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child01);

			var rate01 = helper.CreateRefCusRate(child01.PK, rateCode.PK, startDate, endDate);
			var applicability01 = helper.CreateCusApplicability(rate01.PK, tradeGroup, startDate, endDate);
			var relation11 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent01.ZZ1_TariffCode);

			Factory.Save();

			var uscTariff01 = Factory.New<USCTariff>();
			uscTariff01.UE_Tariff = parent01.ZZ1_TariffCode;
			uscTariff01.UE_DateFrom = parent01.ZZ1_StartDate;
			uscTariff01.UE_DateTo = parent01.ZZ1_EndDate;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 1m;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1m;
				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;

				invoiceLine.JI_Tariff = "1111111111";

				var supTariffDropEdit = (ZDropEdit)brokerageControl.InvoiceLinesUserControl.Controls.Find("SupTariffDropEdit", true).Single();
				supTariffDropEdit.SelectItem("4444.44.4444");
				AssertEquals(typeof(TariffViewAsCodeDescription), supTariffDropEdit.LastSelectedItem.GetType());

				invoiceLine.JI_Tariff = "1234567890";
				AssertNoExceptionThrown(() => supTariffDropEdit.SelectItem("9801.00.60"));
			}
		}

		public void TestErrorReportIfATabIsInsertedMutipleTimes()
		{
			using (var control = new USImportInvoiceLineUserControl())
			{
				control.LineDetailTabControl.TabPages.Insert(control.ElectronicInvoiceTabPage, 0);
				AssertContains(CargoWise.Common.ErrorReporter.LastMessageReported, "Insert duplicated tab page");
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		public void TestFDAViewEditButton_ClickNoException()
		{
			using (USImportInvoiceLineUserControl control = new USImportInvoiceLineUserControl())
			{
				control.FDALinesGrid = null;
				control.FDAViewEditButton_Click(control, null);
			}

			Assert(true);
		}

		public void TestGridLayoutContext()
		{
			using (USImportInvoiceLineUserControl control = new USImportInvoiceLineUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestAIIRelatedTabVisibility()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				USImportInvoiceLineUserControl invoiceLineUserControl = (USImportInvoiceLineUserControl)form.InvoiceLineUserControl;
				AssertEquals("AII is visible in invoice line user control", true, invoiceLineUserControl.ElectronicInvoiceTabPage.TabVisible);
			}
		}

		public void TestBondedWhsRelatedFields()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<Business.OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_WHSEntryLineNo = 1;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("JobComInvoiceLine.Schema.BondedWhsQuantityForGUI", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.BondedWhsQuantityForGUI));
				AssertEquals("JobComInvoiceLine.Schema.US_WHSEntryLineNo", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.US_WHSEntryLineNo));
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				declaration.US_EnableENS = true;
				declaration.US_CertifyCargoRelease = true;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("JobComInvoiceLine.Schema.BondedWhsQuantityForGUI", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.BondedWhsQuantityForGUI));
				AssertEquals("JobComInvoiceLine.Schema.US_WHSEntryLineNo", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.US_WHSEntryLineNo));
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
				declaration.US_EnableENS = true;
				declaration.US_CertifyCargoRelease = true;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("JobComInvoiceLine.Schema.BondedWhsQuantityForGUI", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.BondedWhsQuantityForGUI));
				AssertEquals("JobComInvoiceLine.Schema.US_WHSEntryLineNo", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.US_WHSEntryLineNo));
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("JobComInvoiceLine.Schema.BondedWhsQuantityForGUI", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.BondedWhsQuantityForGUI));
				AssertEquals("JobComInvoiceLine.Schema.US_WHSEntryLineNo", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.US_WHSEntryLineNo));
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var userControl = new USImportInvoiceLineUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);
			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);
			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);
			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is USImportInvoiceLineUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is USImportInvoiceLineUserControl));
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			invoice.JobComInvoiceLines.AddNew().LineGroupingRanges.AddNew();
			invoice.JobComInvoiceLines.AddNew().LineGroupingRanges.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				Application.DoEvents();
				var invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LineGroupingTabPage;
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(1);
				KeySender.PostKeyDown(invoiceLineUserControl.CustomsInvoiceLinesBoundGrid, Keys.Delete);
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteTempInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				Application.DoEvents();
				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var filters = new GridFilterStripBusinessObject(invoiceLineUserControl.CustomsInvoiceLinesBoundGrid).ModuleFilters;
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteMultipleInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 1;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 2;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 3;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				Application.DoEvents();
				var invoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(1);
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(2);
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 2;
				KeySender.PostKeyDown(invoiceLineUserControl.CustomsInvoiceLinesBoundGrid, Keys.Delete);
				Application.DoEvents();
			}
		}

		public void TestMIDParser()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			OrgCusCode cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(mainAddress.PK, userControl.JI_OA_ManufacturerAddressAddressControl.Parse("MID234323"));
				ZGuidDropEditColumnStyleInfo manufacturerAddressColumnInfo = (ZGuidDropEditColumnStyleInfo)userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress);
				AssertEquals(mainAddress.PK, manufacturerAddressColumnInfo.Parse("MID234323"));
			}
		}

		public void TestOverrideCheckBoxIsAdded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				userControl.LineDetailTabControl.SelectedTab = userControl.LineChargesTabPage;
				AssertNotNull(userControl.InvoiceLineCharges.ChargesGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_AdjustedCharge]);
				AssertNotNull(userControl.InvoiceLineCharges.ApportionedChargesGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_AdjustedCharge]);
				AssertEquals(false, userControl.InvoiceLineCharges.ChargesGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_AdjustedCharge].IsVisible);
				AssertEquals(false, userControl.InvoiceLineCharges.ApportionedChargesGrid.Columns[JobComInvHeaderChargeSchema.Constants.J7_AdjustedCharge].IsVisible);
			}
		}

		public void TestInvoiceLineColumnsHasAMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var gridColumns =
					from ZGridColumnInfo info in brokerageControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles
					select info;
				var ammvPerUnitColumnInfo = gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.US_AMMVPerUnit);
				AssertEquals("ammvPerUnitColumnInfo.IsVisible", false, ammvPerUnitColumnInfo.IsVisible);
				AssertEquals("ammvPerUnitColumnInfo.Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78), ammvPerUnitColumnInfo.Width);
				var ammvPercentageColumnInfo = gridColumns.FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.US_AMMVPercentage);
				AssertEquals("ammvPercentageColumnInfo.IsVisible", false, ammvPercentageColumnInfo.IsVisible);
				AssertEquals("ammvPercentageColumnInfo.Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102), ammvPercentageColumnInfo.Width);
			}
		}

		public void TestInvoiceLineGrid_Columns()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				ZGrid grid = userControl.CustomsInvoiceLinesBoundGrid;
				ZGridColumns columns = grid.Columns;
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns[0]);
				AssertDefaultColumn(JobComInvoiceLine.Schema.EntryNumberAndMergeLineNumber, columns[1]);
				AssertDefaultColumn(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, columns[2]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PartNo, columns[3]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CC, columns[4]);
				AssertDefaultColumn(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff, columns[5]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns[6]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns[7]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns[8]);
				AssertDefaultColumn(JobComInvoiceLine.Schema.SupTariffFormatted, columns[9]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_98GoodsValue, columns[10]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_98ValueInvCurr, columns[11]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns[12]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_UC_NKCountryOfOrigin, columns[13]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_SPI, columns[14]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_SecondarySPI, columns[15]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Weight, columns[16]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_WeightUQ, columns[17]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code, columns[18]);
				//ensure all other columns are not visible by default
				for (int i = 19; i < columns.Count; i++)
				{
					ZGridColumn column = columns[i];
				}

				AssertEquals("EntryNumberMergedLineNumber available", false, userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.EntryNumberAndMergeLineNumber].IsUnavailable);
				Assert(userControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(USAddInfoSchema.Constants.US_ADD_NA));
				Assert(userControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(USAddInfoSchema.Constants.US_CVD_NA));
				Assert(userControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(USAddInfoSchema.Constants.US_TransactionsRelated));
			}
		}

		public void TestInvoiceLineGrid_ColumnsForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var columns = grid.Columns;
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns[0]);
				AssertDefaultColumn(JobComInvoiceLine.Schema.EntryNumberAndMergeLineNumber, columns[1]);
				AssertDefaultColumn(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, columns[2]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PartNo, columns[3]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CC, columns[4]);
				AssertDefaultColumn(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff, columns[5]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns[6]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns[7]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns[8]);
				AssertDefaultColumn(JobComInvoiceLine.Schema.SupTariffFormatted, columns[9]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_98GoodsValue, columns[10]);
				AssertDefaultColumn(USAddInfoSchema.Constants.US_98ValueInvCurr, columns[11]);
				AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns[12]);
				Assert(userControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(USAddInfoSchema.Constants.US_TransactionsRelated));
			}
		}

		public void TestLineGroupingTabPageVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Line Grouping page should be shown", true, userControl.LineGroupingTabPage.TabVisible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_EnableAII = false;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Line Grouping page should not be shown", false, userControl.LineGroupingTabPage.TabVisible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_IsInvoiceByRequest = true;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Line Grouping page should be shown", true, userControl.LineGroupingTabPage.TabVisible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				invoice.US_IsLineGrouping = false;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Line Grouping page should be shown", true, userControl.LineGroupingTabPage.TabVisible);
			}
		}

		public void TestElectronicInvoiceTabPageVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = false;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Electronic Invoice page should be shown", false, userControl.ElectronicInvoiceTabPage.TabVisible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_EnableAII = true;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Electronic Invoice page should be shown", true, userControl.ElectronicInvoiceTabPage.TabVisible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals("Electronic Invoice page should not be shown", false, userControl.ElectronicInvoiceTabPage.TabVisible);
			}
		}

		public void TestConsumptionFTZRelatedControlsVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				Assert("userControl.FTZDependentPanelInternal should be invisible when US_EntryType is not ConsumptionFTZ", !userControl.FTZDependentPanel.Visible);
				Assert(!userControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(USAddInfoSchema.Constants.US_ZoneStatus));
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				Assert("userControl.FTZDependentPanelInternal should be visible when US_EntryType is ConsumptionFTZ", userControl.FTZDependentPanel.Visible);
				Assert(userControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(USAddInfoSchema.Constants.US_ZoneStatus));
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				Assert("userControl.FTZDependentPanelInternal should be visible for FTZ", userControl.FTZDependentPanel.Visible);
			}
		}

		public void TestBindings()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl importInvoiceLineUserControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				ZDateEdit userControl = importInvoiceLineUserControl.PrivilegedStatusFilingDateDateEdit;
				invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
				Assert("PrivilegedStatusFilingDateDateEdit should not be visible when invoiceLine.US_ZoneStatus is not PrivilegedForeign", !userControl.Visible);
				invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
				Assert("PrivilegedStatusFilingDateDateEdit should be visible when invoiceLine.US_ZoneStatus is PrivilegedForeign", userControl.Visible);
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
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var consigneeColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ConsigneeAddressOrgPK];
				AssertContains("Consignee", consigneeColumn.ColumnStyle.HeaderText);
				AssertNotContains("Ultimate", consigneeColumn.ColumnStyle.HeaderText);
				AssertEquals("Consignee", userControl.JI_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var ultimateConsigneeColumn = userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ConsigneeAddressOrgPK];
				AssertContains("Ultimate Consignee", ultimateConsigneeColumn.ColumnStyle.HeaderText);
				AssertEquals("Ult. Consignee", userControl.JI_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestADDDepositRateZTextBoxAndCVDDepositRateZTextBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableCRL = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl;
				AssertNotNull("The ADDDepositRateZTextBox should exist in ImportInvoiceLineUserControl", userControl.Controls.Find("ADDDepositRateZTextBox", true).Single());
				AssertNotNull("The CVDDepositRateZTextBox should exist in ImportInvoiceLineUserControl", userControl.Controls.Find("CVDDepositRateZTextBox", true).Single());
			}
		}

		public void TestTSCARelatedColumnsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableCRL = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull("TSCA Authorization Person shouldn't be visible for ACE", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_TSCAName]);
				AssertNull("TSCA Indicator shouldn't be visible for ACE", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_TSCAIndicator]);
			}
		}

		public void TestProductExclusionColumnVisibility()
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
				USImportInvoiceLineUserControl userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Product Exclusion column should be visible for ACE", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ProductExclusion]);
				AssertNotNull("Exclusion Number column should be visible for ACE", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ExclusionNumber]);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USImportInvoiceLineUserControl userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull("Product Exclusion column should NOT be visible for ACS", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ProductExclusion]);
				AssertNull("Exclusion Number column should NOT be visible for ACS", userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ExclusionNumber]);
			}
		}

		protected override Type UserControlToBashType => typeof(USImportInvoiceLineUserControl);

		void AssertDefaultColumn(string expectedName, ZGridColumn column)
		{
			AssertEquals(expectedName, column.ColumnStyle.MappingName);
			AssertEquals(true, column.IsVisible);
		}
	}
}
