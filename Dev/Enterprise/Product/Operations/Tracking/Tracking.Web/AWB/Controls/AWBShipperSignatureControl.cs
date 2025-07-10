using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBShipperSignatureControl runat=server></{0}:AWBShipperSignatureControl>")]
	public class AWBShipperSignatureControl : AWBBaseControl
	{
		#region Constructors

		public AWBShipperSignatureControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			InfoLine1Box = new ZTextBox();
			InfoLine1Box.ID = "InfoLine1";

			InfoLine2Box = new ZTextBox();
			InfoLine2Box.ID = "InfoLine2";

			SignatureBox = new ZTextBox();
			SignatureBox.ID = (NoResString)"Signature";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			InfoLine1Box.BindTo = "EH_ExtraShipperInfoLine1";
			InfoLine1Box.CssClass = "AWBSignatureLine";

			InfoLine2Box.BindTo = "EH_ExtraShipperInfoLine2";
			InfoLine2Box.CssClass = "AWBSignatureLine";

			SignatureBox.BindTo = "EH_ShippersSignature";
			SignatureBox.CssClass = "AWBSignatureLine";

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(97);

			TableRow textRow = new TableRow();
			TableCell textCell = new TableCell()
			{
				Text = Res.GetString("4eddf7a0-af75-4fc0-b35c-d811f8f73edb", "Shipper certifies that the particular on the face thereof are correct and that <b>insofar as any part of the consignment contains dangerous goods, such part is properly described by name and is in proper condition for carriage by air according to the applicable Dangerous Goods Regulations."),
				CssClass = "AWBShipperText"
			};
			textRow.Cells.Add(textCell);

			TableRow infoLine1Row = new TableRow();
			TableCell infoLine1Cell = new TableCell();
			infoLine1Cell.CssClass = "AWBSignatureLineCell";
			infoLine1Cell.Controls.Add(InfoLine1Box);

			TableRow infoLine2Row = new TableRow();
			TableCell infoLine2Cell = new TableCell();
			infoLine2Cell.CssClass = "AWBSignatureLineCell";
			infoLine2Cell.Controls.Add(InfoLine2Box);

			TableRow signatureRow = new TableRow();
			TableCell signatureCell = new TableCell();
			signatureCell.CssClass = "AWBSignatureLineCell";
			signatureCell.Controls.Add(SignatureBox);

			TableRow captionRow = new TableRow();
			TableCell captionCell = new TableCell()
			{
				Text = Res.GetString("d0bb911f-9f2b-4219-946e-c8599520282e", "Signature of Shipper or his Agent"),
				CssClass = "AWBShipperSignature"
			};
			captionCell.HorizontalAlign = HorizontalAlign.Center;
			captionCell.VerticalAlign = VerticalAlign.Top;
			captionCell.Style.Add("border-top", (NoResString)"1px dashed #000000");
			captionRow.Cells.Add(captionCell);

			infoLine1Row.Cells.Add(infoLine1Cell);
			infoLine2Row.Cells.Add(infoLine2Cell);
			signatureRow.Cells.Add(signatureCell);

			layoutTable.Rows.Add(textRow);
			layoutTable.Rows.Add(infoLine1Row);
			layoutTable.Rows.Add(infoLine2Row);
			layoutTable.Rows.Add(signatureRow);
			layoutTable.Rows.Add(captionRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZTextBox InfoLine1Box;
		ZTextBox InfoLine2Box;
		ZTextBox SignatureBox;

		#endregion
	}
}
