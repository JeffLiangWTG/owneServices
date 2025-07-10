using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ImportCustomsEntriesAndEntryLinesUserControlTest : CustomsEntriesAndEntryLinesUserControlAbstractTest<ImportCustomsEntriesAndEntryLinesUserControl>
	{
		public void TestSetCaptions()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var totalCustomsValueInLocalCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalCustomsValueInLocalCurrencyControl");
				AssertEquals("CIF (TWD) (22)", totalCustomsValueInLocalCurrencyControl.CaptionResourceString.Caption);

				var totalCustomsValueInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalCustomsValueInInvoiceCurrencyControl");
				AssertEquals("CIF (22)", totalCustomsValueInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalInternationalFreightAmountInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalInternationalFreightAmountInInvoiceCurrencyControl");
				AssertEquals("Freight (18)", totalInternationalFreightAmountInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalInternationalInsuranceAmountInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalInternationalInsuranceAmountInInvoiceCurrencyControl");
				AssertEquals("Insurance (19)", totalInternationalInsuranceAmountInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalAdditionsInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalAdditionsInInvoiceCurrencyControl");
				AssertEquals("Additions (20)", totalAdditionsInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalDeductionsInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalDeductionsInInvoiceCurrencyControl");
				AssertEquals("Deductions (21)", totalDeductionsInInvoiceCurrencyControl.CaptionResourceString.Caption);
			}
		}

		public void TestEntryLineColumns()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				var messageUserControl = (ImportCustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineGrid = messageUserControl.FindSingle<ZGrid>("EntryLineGrid");
				entryLineGrid.ResetColumns();
				CombineAssertions(() =>
				{
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_EnvironmentalProtectionCode).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_TariffExtensionCode).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_AdditionalDutyAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_AntiDumpingDutyAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_BusinessTaxAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_CommodityTaxCashAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_CommodityTaxNonCashAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_CountervailingDutyAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_HealthAndWelfareSurchargeAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_ImportDutyCashAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_ImportDutyNonCashAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_RetaliatoryDutyAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_TobaccoAndAlcoholTaxAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_TradePromotionFeeAmount).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_ChineseDescription).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_EnglishDescription).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_SHTCImportPermit).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CITESImportPermit).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Preference).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CatalyticConverter).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_StandardEquipment).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CarType).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Transmission).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_EngineType).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_LeftSideSteering).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CarCondition).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_ModelYear).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Displacement).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_NumberOfDoor).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_NumberOfSeat).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_NumberOfCylinder).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_NumberOfGear).IsVisible);
					AssertEquals("Duty Treatment", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Procedure).CaptionResourceString.Caption);
				});
			}
		}

		public void TestEntryLineGridColumnNamesInSortOrder()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				var messageUserControl = (ImportCustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineGrid = messageUserControl.FindSingle<ZGrid>("EntryLineGrid");
				entryLineGrid.ResetColumns();
				AssertEquals(58, entryLineGrid.Columns.Count);

				for (var i = 0; i < ExpectedEntryLineGridColumnNamesInSortOrderList.Count; i++)
				{
					var column = entryLineGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedEntryLineGridColumnNamesInSortOrderList[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		public void TestCalculationsControlsPropertySettingForImport()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					var customsValueCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalCustomsValueInLocalCurrencyControl");
					AssertEquals("TotalCustomsValueInLocalCurrencyControl Decimals", 0, customsValueCurrencyControl.Decimals);

					var totalIMPFOBAmountInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalIMPFOBAmountInInvoiceCurrencyControl");
					AssertEquals("TotalIMPFOBAmountInInvoiceCurrencyControl.LocationX", 91, totalIMPFOBAmountInInvoiceCurrencyControl.Location.X);
				});
			}
		}

		protected override ImportCustomsEntriesAndEntryLinesUserControl CreateCustomsEntriesAndEntryLinesUserControl() => new ImportCustomsEntriesAndEntryLinesUserControl();

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		}
	}
}
