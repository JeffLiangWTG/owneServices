using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBDeclaredValuesControl runat=server></{0}:AWBDeclaredValuesControl>")]
	public class AWBDeclaredValuesControl : AWBBaseControl
	{
		#region Constructors

		public AWBDeclaredValuesControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			CurrencyTextBox = new AWBTextBox();
			CurrencyTextBox.ID = (NoResString)"Currency";

			ChargeCodeTextBox = new AWBDropEditListBox();
			ChargeCodeTextBox.ID = "ChargeCode";

			WTValTextBox = new AWBDropEditListBox();
			WTValTextBox.ID = "WTVal";

			OtherTextBox = new AWBDropEditListBox();
			OtherTextBox.ID = (NoResString)"Other";

			CarriageValueTextBox = new AWBNumericTextBox();
			CarriageValueTextBox.ID = "CarriageValue";

			CustomsValueTextBox = new AWBNumericTextBox();
			CustomsValueTextBox.ID = "CustomsValue";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			CurrencyTextBox.BindTo = "EH_Currency";
			CurrencyTextBox.Caption = Res.GetString("1c7f5fd2-9c96-4cb9-a196-d401e2d70e05", "Currency");
			CurrencyTextBox.FieldCaptionVisible = false;
			CurrencyTextBox.FieldCssClass = "AWBCode3";
			CurrencyTextBox.DataFieldAlign = HorizontalAlign.Center;

			ChargeCodeTextBox.BindTo = "EH_ChargesCode";
			ChargeCodeTextBox.Caption = Res.GetString("ac4c0441-6ae2-4e0d-aa63-a5e88765f038", "CHGS Code");
			ChargeCodeTextBox.FieldCaptionVisible = false;
			ChargeCodeTextBox.MaxLength = 5;
			ChargeCodeTextBox.Width = 65;
			ChargeCodeTextBox.DataFieldAlign = HorizontalAlign.Center;

			WTValTextBox.BindTo = "EH_WeightPrepaidCollect";
			WTValTextBox.Caption = Res.GetString("a796d761-db97-404d-b508-8c457eca528d", "WT / VAL");
			WTValTextBox.FieldCaptionVisible = false;
			WTValTextBox.MaxLength = 2;
			WTValTextBox.Width = 45;
			WTValTextBox.DataFieldAlign = HorizontalAlign.Center;

			OtherTextBox.BindTo = "EH_OtherPrepaidCollect";
			OtherTextBox.Caption = Res.GetString("0d798709-dfde-4c36-8b66-3ec9b282d254", "Other");
			OtherTextBox.FieldCaptionVisible = false;
			OtherTextBox.MaxLength = 2;
			OtherTextBox.Width = 45;
			OtherTextBox.DataFieldAlign = HorizontalAlign.Center;

			CarriageValueTextBox.BindTo = "EH_DeclaredValue";
			CarriageValueTextBox.Caption = Res.GetString("1af6cbeb-b332-4730-85aa-ffe2500f93ea", "D.V. for Carriage");
			CarriageValueTextBox.FieldCaptionVisible = false;
			CarriageValueTextBox.FieldCssClass = "AWBText5";
			CarriageValueTextBox.DataFieldAlign = HorizontalAlign.Center;

			CustomsValueTextBox.BindTo = "EH_DeclaredValue";
			CustomsValueTextBox.Caption = Res.GetString("99d4c3bb-d440-407a-975e-f5ee4f032d6f", "D.V. for Customs");
			CustomsValueTextBox.FieldCaptionVisible = false;
			CustomsValueTextBox.FieldCssClass = "AWBText5";
			CustomsValueTextBox.DataFieldAlign = HorizontalAlign.Center;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(100);

			TableRow layoutRow = new TableRow();
			TableCell cellCurrency = new TableCell();
			cellCurrency.CssClass = "AWBFieldSection";
			cellCurrency.Controls.Add(CurrencyTextBox);

			TableCell cellChargeCode = new TableCell();
			cellChargeCode.CssClass = "AWBFieldSection";
			cellChargeCode.Controls.Add(ChargeCodeTextBox);

			TableCell cellWTVal = new TableCell();
			cellWTVal.CssClass = "AWBFieldSection";
			cellWTVal.Controls.Add(WTValTextBox);

			TableCell cellOther = new TableCell();
			cellOther.CssClass = "AWBFieldSection";
			cellOther.Controls.Add(OtherTextBox);

			TableCell cellCarriageValue = new TableCell();
			cellCarriageValue.CssClass = "AWBFieldSection";
			cellCarriageValue.Controls.Add(CarriageValueTextBox);

			TableCell cellCustomsValue = new TableCell();
			cellCustomsValue.Controls.Add(CustomsValueTextBox);

			layoutRow.Cells.Add(cellCurrency);
			layoutRow.Cells.Add(cellChargeCode);
			layoutRow.Cells.Add(cellWTVal);
			layoutRow.Cells.Add(cellOther);
			layoutRow.Cells.Add(cellCarriageValue);
			layoutRow.Cells.Add(cellCustomsValue);

			layoutTable.Rows.Add(layoutRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		AWBTextBox CurrencyTextBox;
		AWBDropEditListBox ChargeCodeTextBox;
		AWBDropEditListBox WTValTextBox;
		AWBDropEditListBox OtherTextBox;
		AWBNumericTextBox CarriageValueTextBox;
		AWBNumericTextBox CustomsValueTextBox;

		#endregion
	}
}
