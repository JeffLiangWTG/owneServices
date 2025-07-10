using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBHandlingInfoControl runat=server></{0}:AWBHandlingInfoControl>")]
	public class AWBHandlingInfoControl : AWBBaseControl
	{
		#region Constructors

		public AWBHandlingInfoControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			HandlingInfoTextBox = new AWBTextBox();
			HandlingInfoTextBox.ID = "HandlingInfo";

			RNNumberTextBox = new AWBTextBox();
			RNNumberTextBox.ID = "RNNumber";

			SCIBox = new AWBDropEditListBox();
			SCIBox.ID = "SCI";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			HandlingInfoTextBox.BindTo = "EH_HandlingInformation";
			HandlingInfoTextBox.Caption = Res.GetString("e71a4e57-3af0-4940-a654-6abd35c765fb", "Handling Information");
			HandlingInfoTextBox.FieldCaptionVisible = false;
			HandlingInfoTextBox.FieldCssClass = "AWBHandlingInfo";
			HandlingInfoTextBox.DataFieldAlign = HorizontalAlign.Left;
			HandlingInfoTextBox.TextMode = TextBoxMode.MultiLine;

			RNNumberTextBox.BindTo = "EH_ECNCRNNumber";
			RNNumberTextBox.Caption = "&nbsp;";
			RNNumberTextBox.FieldCaptionVisible = false;
			RNNumberTextBox.FieldCssClass = "AWBText20";
			RNNumberTextBox.DataFieldAlign = HorizontalAlign.Right;

			SCIBox.BindTo = "EH_SpecialHandlingCode";
			SCIBox.Caption = "SCI"; // May be a some code abbreviature.
			SCIBox.FieldCaptionVisible = false;
			SCIBox.MaxLength = 5;
			SCIBox.Width = 65;
			SCIBox.DataFieldAlign = HorizontalAlign.Center;
			SCIBox.CaptionAlign = HorizontalAlign.Center;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(100);

			TableRow layoutRow1 = new TableRow();
			TableCell cellHandlingInfo = new TableCell();
			cellHandlingInfo.RowSpan = 2;
			cellHandlingInfo.VerticalAlign = VerticalAlign.Top;
			cellHandlingInfo.Controls.Add(HandlingInfoTextBox);

			TableCell cellRNNumber = new TableCell();
			cellRNNumber.VerticalAlign = VerticalAlign.Top;
			cellRNNumber.Controls.Add(RNNumberTextBox);

			TableRow layoutRow2 = new TableRow();
			TableCell cellSCI = new TableCell();
			cellSCI.CssClass = (NoResString)"AWBSection AWBSCISection";
			cellSCI.Controls.Add(SCIBox);

			layoutRow1.Cells.Add(cellHandlingInfo);
			layoutRow1.Cells.Add(cellRNNumber);
			layoutRow2.Cells.Add(cellSCI);

			layoutTable.Rows.Add(layoutRow1);
			layoutTable.Rows.Add(layoutRow2);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		AWBTextBox HandlingInfoTextBox;
		AWBTextBox RNNumberTextBox;
		AWBDropEditListBox SCIBox;

		#endregion
	}
}
