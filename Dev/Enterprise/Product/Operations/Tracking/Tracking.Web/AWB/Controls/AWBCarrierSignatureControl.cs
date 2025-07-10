using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBCarrierSignatureControl runat=server></{0}:AWBCarrierSignatureControl>")]
	public class AWBCarrierSignatureControl : AWBBaseControl
	{
		#region Constructors

		public AWBCarrierSignatureControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			InfoLine1Box = new ZTextBox();
			InfoLine1Box.ID = "InfoLine1";

			IssuedPlaceBox = new ZTextBox();
			IssuedPlaceBox.ID = "IssuedPlace";

			SignatureBox = new ZTextBox();
			SignatureBox.ID = (NoResString)"Signature";

			IssuedDateBox = new ZDateEdit();
			IssuedDateBox.ID = "IssuedDate";

			AgentNumberBox = new ZTextBox();
			AgentNumberBox.ID = "AgentNumber";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			InfoLine1Box.BindTo = "EH_ExtraCarrierInfoLine2";
			InfoLine1Box.CssClass = "AWBSignatureLine";

			IssuedPlaceBox.BindTo = "EH_AWBIssuePlace";
			IssuedPlaceBox.CssClass = "AWBText20";

			SignatureBox.BindTo = "EH_AWBAgentsSignature";
			SignatureBox.CssClass = "AWBText20";

			IssuedDateBox.BindTo = "EH_AWBIssueDate";

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(97);

			TableRow infoLine1Row = new TableRow();
			TableCell infoLine1Cell = new TableCell();
			infoLine1Cell.ColumnSpan = 4;
			infoLine1Cell.CssClass = "AWBSignatureLineCell";
			infoLine1Cell.Controls.Add(InfoLine1Box);

			TableRow infoLine2Row = new TableRow();
			TableCell infoLine21Cell = new TableCell();
			infoLine21Cell.CssClass = "AWBSignatureLineCell";
			infoLine21Cell.Style.Add("border-bottom", (NoResString)"1px dashed #000000");
			infoLine21Cell.Controls.Add(IssuedDateBox);
			TableCell infoLine22Cell = new TableCell();
			infoLine22Cell.CssClass = "AWBSignatureLineCell";
			infoLine22Cell.Style.Add("border-bottom", (NoResString)"1px dashed #000000");
			infoLine22Cell.Controls.Add(IssuedPlaceBox);
			TableCell infoLine23Cell = new TableCell();
			infoLine23Cell.CssClass = "AWBSignatureLineCell";
			infoLine23Cell.Style.Add("border-bottom", (NoResString)"1px dashed #000000");
			infoLine23Cell.Controls.Add(SignatureBox);
			TableCell infoLine24Cell = new TableCell();
			infoLine24Cell.CssClass = "AWBSignatureLineCell";
			infoLine24Cell.Style.Add("border-bottom", (NoResString)"1px dashed #000000");
			infoLine24Cell.Text = "&nbsp;";

			TableRow captionRow = new TableRow();
			captionRow.Cells.Add(new TableCell()
			{
				Text = Res.GetString("ee523394-45f7-455a-b8ab-4a31d4a15957", "Executed on (date)"),
				CssClass = "AWBShipperSignature",
				HorizontalAlign = HorizontalAlign.Left,
				VerticalAlign = VerticalAlign.Top
			});
			captionRow.Cells.Add(new TableCell()
			{
				Text = Res.GetString("227b2a6c-69b7-435f-8fdc-ac437cd67e05", "At (Place)"),
				CssClass = "AWBShipperSignature",
				HorizontalAlign = HorizontalAlign.Center,
				VerticalAlign = VerticalAlign.Top
			});
			captionRow.Cells.Add(new TableCell()
			{
				Text = Res.GetString("2b148a3a-3ab5-4b66-b9d6-380d5d0a0fb5", "Signature of Issuing Carrier or its Agent"),
				CssClass = "AWBShipperSignature",
				HorizontalAlign = HorizontalAlign.Center,
				VerticalAlign = VerticalAlign.Top,
				ColumnSpan = 2
			});

			infoLine1Row.Cells.Add(infoLine1Cell);
			infoLine2Row.Cells.Add(infoLine21Cell);
			infoLine2Row.Cells.Add(infoLine22Cell);
			infoLine2Row.Cells.Add(infoLine23Cell);
			infoLine2Row.Cells.Add(infoLine24Cell);

			layoutTable.Rows.Add(infoLine1Row);
			layoutTable.Rows.Add(infoLine2Row);
			layoutTable.Rows.Add(captionRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZTextBox InfoLine1Box;
		ZTextBox IssuedPlaceBox;
		ZTextBox SignatureBox;
		ZTextBox AgentNumberBox;
		ZDateEdit IssuedDateBox;

		#endregion
	}
}
