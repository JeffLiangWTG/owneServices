using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceLineUserControlHelperTest : TestCaseWithFactory
	{
		public void TestColumnPositions()
		{
			using (DeclarationInvoiceLineUserControlForTesting invoiceLineUserControl = new DeclarationInvoiceLineUserControlForTesting())
			{
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_LineNo, 0);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, 1);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_PartNo, 2);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CC, 3);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Tariff, 4);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_InvoiceQuantity, 5);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_InvoiceUQ, 6);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomsQuantity, 7);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomsUnitQty, 8);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_LinePrice, 9);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Description, 10);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CountryOfOrigin, 11);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_RH_NKCommodity_Code, 12);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Weight, 13);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_WeightUQ, 14);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_NetWeight, 15);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_NetWeightUQ, 16);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Volume, 17);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_VolumeUQ, 18);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_OrderNumber, 19);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine, 20);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_PartAttrib1, 21);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_PartAttrib2, 22);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_PartAttrib3, 23);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_SerialNumber, 24);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.UnitPrice, 25);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomAttrib1, 26);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomAttrib2, 27);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomAttrib3, 28);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomAttrib4, 29);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomAttrib5, 30);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomAttrib6, 31);
				CheckPosition(invoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_CustomTextBlob1, 32);
			}
		}

		public void TestTariffFindBox()
		{
			using (var control = new DeclarationInvoiceLineUserControlForTesting())
			{
				var tariffFindBox = control.TariffFindBox;
				AssertNotNull("JI_TariffFindBox should be a Universal.GUI.TariffFindBox", tariffFindBox);
				AssertEquals("JI_TariffFindBox.GetCountryCode()", Core.Constants.CountryCodes.Eritrea, tariffFindBox.GetCountryCode());
				AssertEquals("JI_TariffFindBox.EffectiveTariffCountry", Core.Constants.CountryCodes.Eritrea, tariffFindBox.EffectiveTariffCountry);
				AssertEquals("JI_TariffFindBox.TariffType", "HSN", tariffFindBox.TariffType);
			}
		}

		void CheckPosition(DeclarationInvoiceLineUserControlForTesting invoiceLineUserControl, string columnName, int index)
		{
			AssertEquals("Invalid column at index [" + index.ToString() + "].", columnName, ((Core.Forms.ZGridColumnInfo)invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles[index]).ColumnName);
		}
	}

	sealed class DeclarationInvoiceLineUserControlForTesting : DeclarationInvoiceLineUserControl
	{
		readonly System.ComponentModel.Container components;

		public DeclarationInvoiceLineUserControlForTesting()
		{
			InitializeComponent();
			InvoiceLineUserControlHelper.SetTariffRelated(CustomsInvoiceLinesBoundGrid, Name, TariffFindBox, GetCustomsCountryCode, GetDataGroupingForUniversalTariff, GetUniversalTariffType());
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		internal Universal.GUI.TariffFindBox TariffFindBox;

		void InitializeComponent()
		{
			this.TariffFindBox = new Universal.GUI.TariffFindBox();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceLinesSummaryGroupBox, false);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceDetailsGroupBox, false);
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.TariffFindBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClassificationDetailsGroupBox, false);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TariffFindBox, 0);
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.TabIndex = 3;
			// 
			// JI_TariffFindBox
			// 
			this.BindingSource.SetBindingMember(this.TariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((BaseJobComInvoiceLine)(((IInvoicesProvider)(null)).FilteredInvoiceLines.SyncRoot)).JI_Tariff);
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.TariffFindBox.Name = "JI_TariffFindBox";
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TariffFindBox.TabIndex = 1;
			//
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			//
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.CaptionResourceString = NoResourceStringData.GetData("Insurance", "Insurance for current line item");
			// 
			// DeclarationInvoiceLineUserControlForTesting
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "DeclarationInvoiceLineUserControlForTesting";
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
