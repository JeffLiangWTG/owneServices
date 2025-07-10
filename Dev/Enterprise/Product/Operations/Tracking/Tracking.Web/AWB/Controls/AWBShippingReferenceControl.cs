using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBShippingReferenceControl runat=server></{0}:AWBShippingReferenceControl>")]
	public class AWBShippingReferenceControl : AWBBaseControl
	{
		#region Constructors

		public AWBShippingReferenceControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			ReferenceTextBox = new AWBTextBox();
			ReferenceTextBox.ID = (NoResString)"Reference";

			OptionalInfo1TextBox = new AWBTextBox();
			OptionalInfo1TextBox.ID = "OptionalInfo1";

			OptionalInfo2TextBox = new AWBTextBox();
			OptionalInfo2TextBox.ID = "OptionalInfo2";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			ReferenceTextBox.BindTo = "EH_ConsolNumber";
			ReferenceTextBox.Caption = Res.GetString("e4e8b806-9da5-4958-8aa0-2e3ac322b3c6", "Reference Number");
			ReferenceTextBox.FieldCaptionVisible = false;
			ReferenceTextBox.FieldCssClass = "AWBText10";
			ReferenceTextBox.DataFieldAlign = HorizontalAlign.Center;

			OptionalInfo1TextBox.BindTo = "EH_OptionalShippingInformation";
			OptionalInfo1TextBox.Caption = Res.GetString("3b7b5291-90ec-406e-a978-d99333536ef4", "Optional Shipping Information");
			OptionalInfo1TextBox.FieldCaptionVisible = false;
			OptionalInfo1TextBox.FieldCssClass = "AWBText10";
			OptionalInfo1TextBox.DataFieldAlign = HorizontalAlign.Center;

			OptionalInfo2TextBox.BindTo = "EH_OptionalShippingInformation2";
			OptionalInfo2TextBox.Caption = "&nbsp;";
			OptionalInfo2TextBox.FieldCaptionVisible = false;
			OptionalInfo2TextBox.FieldCssClass = "AWBText10";
			OptionalInfo2TextBox.DataFieldAlign = HorizontalAlign.Center;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;

			TableRow layoutRow = new TableRow();
			TableCell cellReference = new TableCell();
			cellReference.CssClass = "AWBFieldSection";
			cellReference.Controls.Add(ReferenceTextBox);

			TableCell cellOptionalInfo1 = new TableCell();
			cellOptionalInfo1.Controls.Add(OptionalInfo1TextBox);

			TableCell cellOptionalInfo2 = new TableCell();
			cellOptionalInfo2.Controls.Add(OptionalInfo2TextBox);

			layoutRow.Cells.Add(cellReference);
			layoutRow.Cells.Add(cellOptionalInfo1);
			layoutRow.Cells.Add(cellOptionalInfo2);

			layoutTable.Rows.Add(layoutRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		AWBTextBox ReferenceTextBox;
		AWBTextBox OptionalInfo1TextBox;
		AWBTextBox OptionalInfo2TextBox;

		#endregion
	}
}
