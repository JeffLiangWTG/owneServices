using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBIssuedControl runat=server></{0}:AWBIssuedControl>")]
	public class AWBIssuedControl : AWBBaseControl
	{
		#region Constructors

		public AWBIssuedControl()
			: base() { }

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string LineCssClass { get; set; }

		[Category("Appearance"), DefaultValue("Issued By"), Browsable(true)]
		public string Caption { get; set; }

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			IssuedByLine1TextBox = new ZTextBox();
			IssuedByLine1TextBox.ID = "AddressLine1";

			IssuedByLine2TextBox = new ZTextBox();
			IssuedByLine2TextBox.ID = "AddressLine2";

			IssuedByLine3TextBox = new ZTextBox();
			IssuedByLine3TextBox.ID = "AddressLine3";

			IssuedByLine1CaptionLabel = new ZTextLabel();
			IssuedByLine1CaptionLabel.ID = "AddressLine1Caption";

			IssuedByLine2CaptionLabel = new ZTextLabel();
			IssuedByLine2CaptionLabel.ID = "AddressLine2Caption";

			IssuedByLine3CaptionLabel = new ZTextLabel();
			IssuedByLine3CaptionLabel.ID = "AddressLine3Caption";

			ControlCaption = new ZTextLabel();
			ControlCaption.ID = "AWBIssuedByCaption";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			IssuedByLine1TextBox.BindTo = "EH_IssuingAgentName";
			IssuedByLine2TextBox.BindTo = "EH_IssuingAgentAddress1";
			IssuedByLine3TextBox.BindTo = "EH_IssuingAgentAddress2";

			IssuedByLine1TextBox.CssClass = LineCssClass;
			IssuedByLine2TextBox.CssClass = LineCssClass;
			IssuedByLine3TextBox.CssClass = LineCssClass;

			IssuedByLine1CaptionLabel.Text = Res.GetString("b3442463-3daf-4636-95ba-01a9da7c7ddb", "Issued By:");
			IssuedByLine2CaptionLabel.Text = "&nbsp;";
			IssuedByLine3CaptionLabel.Text = "&nbsp;";

			ControlCaption.Text = Caption;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;

			TableRow rowCaption = new TableRow();
			TableCell cellCaption = new TableCell();
			cellCaption.CssClass = "AWBSectionTitle";
			cellCaption.ColumnSpan = 2;
			cellCaption.Controls.Add(ControlCaption);
			rowCaption.Cells.Add(cellCaption);
			layoutTable.Rows.Add(rowCaption);

			TableRow row1 = new TableRow();
			TableCell cell11 = new TableCell();
			cell11.CssClass = "AWBCaption";
			cell11.Controls.Add(IssuedByLine1CaptionLabel);
			TableCell cell12 = new TableCell();
			cell12.CssClass = "AWBSectionData";
			cell12.Controls.Add(IssuedByLine1TextBox);
			row1.Cells.Add(cell11);
			row1.Cells.Add(cell12);
			layoutTable.Rows.Add(row1);

			TableRow row2 = new TableRow();
			TableCell cell21 = new TableCell();
			cell21.CssClass = "AWBCaption";
			cell21.Controls.Add(IssuedByLine2CaptionLabel);
			TableCell cell22 = new TableCell();
			cell22.CssClass = "AWBSectionData";
			cell22.Controls.Add(IssuedByLine2TextBox);
			row2.Cells.Add(cell21);
			row2.Cells.Add(cell22);
			layoutTable.Rows.Add(row2);

			TableRow row3 = new TableRow();
			TableCell cell31 = new TableCell();
			cell31.Controls.Add(IssuedByLine3CaptionLabel);
			TableCell cell32 = new TableCell();
			cell32.CssClass = "AWBSectionData";
			cell32.Controls.Add(IssuedByLine3TextBox);
			row3.Cells.Add(cell31);
			row3.Cells.Add(cell32);
			layoutTable.Rows.Add(row3);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZTextBox IssuedByLine1TextBox;
		ZTextBox IssuedByLine2TextBox;
		ZTextBox IssuedByLine3TextBox;

		ZTextLabel IssuedByLine1CaptionLabel;
		ZTextLabel IssuedByLine2CaptionLabel;
		ZTextLabel IssuedByLine3CaptionLabel;

		ZTextLabel ControlCaption;

		#endregion
	}
}
