using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	[TestedType(typeof(ZABillLayouts))]
	sealed class ZABillLayoutsTest : LayoutsAbstractTest
	{
		public void TestCargoReleaseStatusDropEdit_Visible()
		{
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var billUserControl = billsTabPage.FindSingle<AsycudaBillUserControl>(control => control.Name == "asycudaBillUserControl");
				var cargoReleaseStatusDropEdit = billUserControl.Controls.Find("CargoReleaseStatusDropEdit", true).First();
				AssertEquals(true, cargoReleaseStatusDropEdit.Visible);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				header.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				AssertEquals(false, cargoReleaseStatusDropEdit.Visible);
			}
		}

		public void TestCargoReleaseStatusOtherDescriptionTextBox_Visible()
		{
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = "51";
			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var billUserControl = billsTabPage.FindSingle<AsycudaBillUserControl>(control => control.Name == "asycudaBillUserControl");
				var cargoReleaseStatusOtherDescriptionTextBox = billUserControl.Controls.Find("CargoReleaseStatusOtherDescriptionTextBox", true).First();

				AssertEquals(true, cargoReleaseStatusOtherDescriptionTextBox.Visible);

				bill.CargoReleaseStatus = "1";
				form.Show();
				AssertEquals(false, cargoReleaseStatusOtherDescriptionTextBox.Visible);
			}
		}

		public void TestBillLevelMessageControlsVisibility()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCusCodeType("MFDOC", "Manifest Document Type Code");

			var docTypeCOH = testHelper.CreateCusCodeList(countryCode, "MFDOC", "COH", "Container House", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(docTypeCOH.PK, "BillLevelMessage", "true");

			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();

			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var messageStatus = form.FindSingle<ZTextBox>(nameof(CommonBillControlBag.MessageStatusTextBox));
				var billStatus = form.FindSingle<ZDropEdit>(nameof(CommonBillControlBag.BillStatusDropEdit));
				var registrationDate = form.FindSingle<ZDateEdit>(nameof(CommonBillControlBag.RegistrationDateEdit));

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				manifest.AMA_ManifestType = "RFM";
				AssertEquals("MessageStatus", false, messageStatus.Visible);
				AssertEquals("BillStatus", false, billStatus.Visible);
				AssertEquals("RegistrationDate", false, registrationDate.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_ManifestType = "COH";
				AssertEquals(nameof(FieldType.Text), bill.BillIssuerNameFieldType);
				AssertEquals("MessageStatus", true, messageStatus.Visible);
				AssertEquals("BillStatus", true, billStatus.Visible);
				AssertEquals("RegistrationDate", true, registrationDate.Visible);
			}
		}

		public void TestBillIssuerControlsVisibility()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;

			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateNewOrGetExistingCusCodeType("MVAL", "Manifest Validation");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList(countryCode, "MVAL", "BillIssuer", "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, "MANDATORYFORMESSAGETYPE", nameof(ManifestDocumentType.HAB));
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));

			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();

			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billIssuerCodeFindBoxForTest = form.Controls.Find("BillIssuerCodeFindBox", true).First();
				var billIssuerTextBoxForTest = form.Controls.Find("BillIssuerTextBox", true).First();
				var billIssuerNameForTest = form.FindSingle<ZTextBox>(nameof(CommonBillControlBag.BillIssuerNameTextBox));

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				manifest.AMA_ManifestType = "RFM";
				AssertEquals("BillIssuerCodeFindBox", false, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", false, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", false, billIssuerNameForTest.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_ManifestType = "COH";
				AssertEquals(nameof(FieldType.Text), bill.BillIssuerNameFieldType);
				AssertEquals("BillIssuerCodeFindBox", false, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", true, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", true, billIssuerNameForTest.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				manifest.AMA_ManifestType = "HAB";
				AssertEquals(nameof(FieldType.Text), bill.BillIssuerNameFieldType);
				AssertEquals("BillIssuerCodeFindBox", false, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", true, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", true, billIssuerNameForTest.Visible);
			}

			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "DJC";
			carrier.ZZ4_CountryOrGrouping = "ZA";
			carrier.ZZ4_Description = "Daniel";

			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billIssuerCodeFindBoxForTest = form.Controls.Find("BillIssuerCodeFindBox", true).First();
				var billIssuerTextBoxForTest = form.Controls.Find("BillIssuerTextBox", true).First();
				var billIssuerNameForTest = form.FindSingle<ZTextBox>(nameof(CommonBillControlBag.BillIssuerNameTextBox));

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				manifest.AMA_ManifestType = "RFM";
				AssertEquals("BillIssuerCodeFindBox", false, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", false, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", false, billIssuerNameForTest.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_ManifestType = "COH";
				AssertEquals(nameof(FieldType.TextCodeFindBox), bill.BillIssuerNameFieldType);
				AssertEquals("BillIssuerCodeFindBox", true, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", false, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", false, billIssuerNameForTest.Visible);
			}
		}

		public void TestBillIssuerControlsVisibilityOnBillCreation()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "DJC";
			carrier.ZZ4_CountryOrGrouping = "ER";
			carrier.ZZ4_Description = "Daniel";
			Factory.Save();

			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateNewOrGetExistingCusCodeType("MVAL", "Manifest Validation");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList("ZA", "MVAL", "BillIssuer", "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));

			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = "ZA";
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = "COH";

			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var billUserControl = billsTabPage.Controls.Find("asycudaBillUserControl", true).First() as AsycudaBillUserControl;
				billUserControl.Focus();

				var billIssuerCodeFindBoxForTest = billUserControl.Controls.Find("BillIssuerCodeFindBox", true).First();
				var billIssuerTextBoxForTest = billUserControl.Controls.Find("BillIssuerTextBox", true).First();
				var billIssuerNameForTest = form.FindSingle<ZTextBox>(nameof(CommonBillControlBag.BillIssuerNameTextBox));
				AssertEquals("BillIssuerCodeFindBox", false, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", false, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", false, billIssuerNameForTest.Visible);

				var bill = manifest.Bills.AddNew();

				AssertEquals(nameof(FieldType.Text), bill.BillIssuerNameFieldType);
				AssertEquals("BillIssuerCodeFindBox", false, billIssuerCodeFindBoxForTest.Visible);
				AssertEquals("BillIssuerTextBox", true, billIssuerTextBoxForTest.Visible);
				AssertEquals("BillIssuerName", true, billIssuerNameForTest.Visible);
			}
		}

		public void TestMultipleCustomsNumberGrid()
		{
			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();

				var zGroupBoxCustomsNumbers = form.Controls.Find("CustomsNumbersGroupBox", true).First();
				var zGroupBoxAssociatedPacks = form.Controls.Find("AssociatedPacksGroupBox", true).First();

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals(true, zGroupBoxCustomsNumbers.Visible);
				AssertEquals(true, zGroupBoxAssociatedPacks.Visible);
				AssertEquals("BillCountriesGrid should not overflow Customs Numbers", System.Windows.Forms.DockStyle.None, zGroupBoxCustomsNumbers.Dock);
				AssertEquals("Align", zGroupBoxCustomsNumbers.Top, zGroupBoxAssociatedPacks.Top);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(false, zGroupBoxCustomsNumbers.Visible);
				AssertEquals(false, zGroupBoxAssociatedPacks.Visible);
			}
		}

		public void TestCaseNumberGridColumns()
		{
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = "51";

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;

				var grid = asycudaManifestUserControl.Controls.Find("CaseNumberGrid", true)[0] as ZGrid;

				var columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Code);
				Assert("Case Type IsUnavailable", !columnStyle.IsUnavailable);
				columnStyle = grid.GetColumnStyle(CaseNumber.Schema.Description);
				Assert("Description IsUnavailable", !columnStyle.IsUnavailable);
				columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Data);
				Assert("Case Number IsUnavailable", !columnStyle.IsUnavailable);
			}
		}

		[TestDate(2019, 12, 03, 00, 00, 01)]
		public void TestCaseNumberVisibility()
		{
			#region RefData:

			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCusCodeType("MFDOC", "Manifest Document Type Code");

			var docTypeCOM = testHelper.CreateCusCodeList(countryCode, "MFDOC", "COM", "Container Master", startDate, endDate);
			var docTypeCOH = testHelper.CreateCusCodeList(countryCode, "MFDOC", "COH", "Container House", startDate, endDate);
			var docTypeFWB = testHelper.CreateCusCodeList(countryCode, "MFDOC", "FWB", "Master Air Waybill", startDate, endDate);
			var docTypeHAB = testHelper.CreateCusCodeList(countryCode, "MFDOC", "HAB", "House Air Waybill", startDate, endDate);

			testHelper.CreateCusCodeListAttribute(docTypeCOM.PK, "BillLevelMessage", "false");
			testHelper.CreateCusCodeListAttribute(docTypeCOH.PK, "BillLevelMessage", "true");
			testHelper.CreateCusCodeListAttribute(docTypeFWB.PK, "BillLevelMessage", "false");
			testHelper.CreateCusCodeListAttribute(docTypeHAB.PK, "BillLevelMessage", "true");

			Factory.Save();

			#endregion RefData.

			#region Setup Test Scenarios:
			var effectiveDate = new ZDateTime(2019, 12, 01);
			var scenarios = new List<CaseNumberVisibilityTestScenario>()
			{
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.COM), ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Sea, effectiveDate, false),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.COH), ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Sea, effectiveDate, true),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.FWB), ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Air, effectiveDate, false),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.HAB), ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Air, effectiveDate, true),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.COH), ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Sea, new ZDateTime(2019, 12, 13), false)
			};

			#endregion Setup Test Scenarios.

			Action<CaseNumberVisibilityTestScenario> testAction = scenario =>
			{
				bool zaManifestCaseNumbersFeatureIsActive = true;
				if (!scenario.FunctionalityStartDate.Equals(effectiveDate))
				{
					zaManifestCaseNumbersFeatureIsActive = false;
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers,
					Core.Constants.CountryCodes.SouthAfrica, new ZDate(2019, 12, 03), zaManifestCaseNumbersFeatureIsActive))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
					header.AMA_RN_NKCountry = countryCode;
					header.AMA_ApplicationCode = scenario.ManifestStyle;
					header.AMA_ManifestType = scenario.ManifestDocumentType;
					header.AMA_TransportMode = scenario.TransportMode;
					header.AMA_Nature = ShipmentTypeList.Codes.Import23;

					var bill01 = header.Bills.AddNew();
					bill01.ABL_BillNumber = "BLX-111";
					bill01.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

					var bill02 = header.Bills.AddNew();
					bill02.ABL_BillNumber = "BLX-222";
					bill02.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

					using (var form = new ManifestForm(header))
					{
						form.Show();
						form.Update();
						var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

						var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
						var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
						mainTabControl.SelectedTab = billsAndPacksTabPage;

						var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
						var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
						billsAndPacksTabControl.SelectedTab = billsTabPage;

						var billsGrid = asycudaManifestUserControl.FindSingle<ZGrid>(c => c.Name == "BillsGrid");
						var caseNumberControl = asycudaManifestUserControl.FindSingle<ZGroupBox>(nameof(ZABillControlBag.CaseNumberBillsGroupBox));
						var expectedValue = scenario.ExpectedToBeVisible;

						billsGrid.CurrentRowIndex = 0;
						var bill = (Business.AsycudaBill)billsGrid.GetCurrent();
						AssertEquals("Prerequisite", "BLX-111", bill.ABL_BillNumber);
						var assertMsg = "Check Visibility of CaseNumberGroupBox for manifest type " + scenario.ManifestDocumentType + " and bill number " + bill.ABL_BillNumber;
						AssertEquals(assertMsg, expectedValue, caseNumberControl.Visible);

						billsGrid.CurrentRowIndex = 1;
						bill = (Business.AsycudaBill)billsGrid.GetCurrent();
						AssertEquals("Prerequisite", "BLX-222", bill.ABL_BillNumber);
						assertMsg = "Check Visibility of CaseNumberGroupBox for manifest type " + scenario.ManifestDocumentType + " and bill number " + bill.ABL_BillNumber;
						AssertEquals(assertMsg, expectedValue, caseNumberControl.Visible);
					}
				}
			};
			scenarios.ForEach(testAction);
		}

		[ExpectNoExceptions]
		public void TestChangeControlsVisibilityWhenBillDeleted()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCusCodeType("MFDOC", "Manifest Document Type Code");

			var docTypeCOH = testHelper.CreateCusCodeList(countryCode, "MFDOC", "COH", "Container House", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(docTypeCOH.PK, "BillLevelMessage", "true");

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers,
				countryCode, ZDateTime.Today, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.New<Business.AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				header.AMA_TransportMode = "SEA";
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.AMA_ManifestType = nameof(ManifestDocumentType.COH);

				var bill01 = header.Bills.AddNew();
				bill01.ABL_BillNumber = "BLX-111";
				bill01.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

				using (var form = new ManifestForm(header))
				{
					form.Show();
					form.Update();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;

					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
					billsAndPacksTabControl.SelectedTab = billsTabPage;

					var billsGrid = asycudaManifestUserControl.FindSingle<ZGrid>(c => c.Name == "BillsGrid");

					billsGrid.CurrentRowIndex = 0;
					billsGrid.DeleteMenuItem.PerformClick();
				}
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Auto);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillIssuerTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillIssuerNameTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillIssuerCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (ZABillControlBag.Instance.CargoReleaseStatusDropEdit, ControlWidthClass.Long);
				yield return (ZABillControlBag.Instance.CargoReleaseStatusOtherDescriptionTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ForwarderAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.UCRNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsNumbersGroupBox, ControlWidthClass.Long);
				yield return (ZABillControlBag.Instance.CaseNumberBillsGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.AssociatedPacksGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<Business.AsycudaBill>();
	}

	sealed class CaseNumberVisibilityTestScenario
	{
		public CaseNumberVisibilityTestScenario(string manifestDocumentType, string manifestStyle, string transportMode, ZDateTime functionalityStartDate, bool expectedToBeVisible)
		{
			ManifestDocumentType = manifestDocumentType;
			ManifestStyle = manifestStyle;
			TransportMode = transportMode;
			FunctionalityStartDate = functionalityStartDate;
			ExpectedToBeVisible = expectedToBeVisible;
		}

		public string ManifestDocumentType;
		public string ManifestStyle;
		public string TransportMode;
		public ZDateTime FunctionalityStartDate;
		public bool ExpectedToBeVisible;
	}
}
