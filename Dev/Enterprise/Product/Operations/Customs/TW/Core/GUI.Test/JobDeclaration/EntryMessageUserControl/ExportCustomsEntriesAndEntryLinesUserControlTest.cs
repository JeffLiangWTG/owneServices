using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ExportCustomsEntriesAndEntryLinesUserControlTest : CustomsEntriesAndEntryLinesUserControlAbstractTest<ExportCustomsEntriesAndEntryLinesUserControl>
	{
		public void TestSetCaptions()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var totalCustomsValueInLocalCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalCustomsValueInLocalCurrencyControl");
				AssertEquals("FOB (TWD) (21)", totalCustomsValueInLocalCurrencyControl.CaptionResourceString.Caption);

				var totalCustomsValueInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalCustomsValueInInvoiceCurrencyControl");
				AssertEquals("FOB (21)", totalCustomsValueInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalInternationalFreightAmountInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalInternationalFreightAmountInInvoiceCurrencyControl");
				AssertEquals("Freight (17)", totalInternationalFreightAmountInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalInternationalInsuranceAmountInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalInternationalInsuranceAmountInInvoiceCurrencyControl");
				AssertEquals("Insurance (18)", totalInternationalInsuranceAmountInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalAdditionsInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalAdditionsInInvoiceCurrencyControl");
				AssertEquals("Additions (19)", totalAdditionsInInvoiceCurrencyControl.CaptionResourceString.Caption);

				var totalDeductionsInInvoiceCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalDeductionsInInvoiceCurrencyControl");
				AssertEquals("Deductions (20)", totalDeductionsInInvoiceCurrencyControl.CaptionResourceString.Caption);
			}
		}

		public void TestEntryLineColumns()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();
				var messageUserControl = (ExportCustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineGrid = messageUserControl.FindSingle<ZGrid>("EntryLineGrid");
				entryLineGrid.ResetColumns();
				CombineAssertions(() =>
				{
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_BondedGoodsCode).IsVisible);
					AssertEquals(false, entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_TradePromotionFeeAmount).IsVisible);
					AssertEquals("Mode of Statistics", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Procedure).CaptionResourceString.Caption);
				});
			}
		}

		public void TestEntryLineGridColumnNamesInSortOrder()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();
				var messageUserControl = (ExportCustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineGrid = messageUserControl.FindSingle<ZGrid>("EntryLineGrid");
				entryLineGrid.ResetColumns();
				AssertEquals(27, entryLineGrid.Columns.Count);
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

		public void TestCalculationsControlsDecimalsForExport()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();
				var customsValueCurrencyControl = userControl.FindSingle<ConvertToLocalCurrencyControl>("TotalCustomsValueInLocalCurrencyControl");
				AssertEquals(0, customsValueCurrencyControl.Decimals);

				var businessTaxBaseAmountCalcEdit = userControl.FindSingle<ZCalcEdit>("BusinessTaxBaseAmountCalcEdit");
				AssertEquals(0, businessTaxBaseAmountCalcEdit.Decimals);

				var totalCashTaxAmountCalcEdit = userControl.FindSingle<ZCalcEdit>("TotalCashTaxAmountCalcEdit");
				AssertEquals(0, totalCashTaxAmountCalcEdit.Decimals);
			}
		}

		public void TestEntryLinesGridFOBValueColumnCaption()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();
				var messageUserControl = (ExportCustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineGrid = messageUserControl.FindSingle<ZGrid>("EntryLineGrid");
				AssertEquals("FOB Value", entryLineGrid.GetColumnStyle("CL_CustomsValue").CaptionResourceString.Caption);
			}
		}

		public void TestTotalCashTaxAmountCalcEditCaption()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();
				var totalCashTaxAmountCalcEdit = userControl.FindSingle<ZCalcEdit>("TotalCashTaxAmountCalcEdit");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Total Tax Amount", totalCashTaxAmountCalcEdit.CaptionResourceString.Caption);
					AssertEquals("FullDescription", "The total amount of the duties, taxes and fees.", totalCashTaxAmountCalcEdit.CaptionResourceString.FullDescription);
				});
			}
		}

		protected override ExportCustomsEntriesAndEntryLinesUserControl CreateCustomsEntriesAndEntryLinesUserControl() => new ExportCustomsEntriesAndEntryLinesUserControl();
		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}
	}
}
