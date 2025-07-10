using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBCarrierControl runat=server></{0}:AWBCarrierControl>")]
	public class AWBCarrierControl : AWBBaseControl
	{
		#region Constructors

		public AWBCarrierControl()
			: base() { }

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string LineCssClass { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string Caption { get; set; }

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			NameTextBox = new ZTextBox();
			NameTextBox.ID = (NoResString)"Name";

			CityTextBox = new ZTextBox();
			CityTextBox.ID = (NoResString)"City";

			NameCaptionLabel = new ZTextLabel();
			NameCaptionLabel.ID = "NameCaption";

			CityCaptionLabel = new ZTextLabel();
			CityCaptionLabel.ID = "CityCaption";

			ControlCaption = new ZTextLabel();
			ControlCaption.ID = "CarrierCaption";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			NameTextBox.BindTo = "EH_AgentName";
			CityTextBox.BindTo = "EH_AgentPlace";

			NameTextBox.CssClass = LineCssClass;
			CityTextBox.CssClass = LineCssClass;

			NameCaptionLabel.Text = Res.GetString("33c5dd72-fc5a-4339-a051-23f5f2dd925b", "Name:");
			CityCaptionLabel.Text = Res.GetString("02f56a96-167d-47b5-b544-1634976033ea", "City:");

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
			cell11.Controls.Add(NameCaptionLabel);
			TableCell cell12 = new TableCell();
			cell12.CssClass = "AWBSectionData";
			cell12.Controls.Add(NameTextBox);
			row1.Cells.Add(cell11);
			row1.Cells.Add(cell12);
			layoutTable.Rows.Add(row1);

			TableRow row2 = new TableRow();
			TableCell cell21 = new TableCell();
			cell21.CssClass = "AWBCaption";
			cell21.Controls.Add(CityCaptionLabel);
			TableCell cell22 = new TableCell();
			cell22.CssClass = "AWBSectionData";
			cell22.Controls.Add(CityTextBox);
			row2.Cells.Add(cell21);
			row2.Cells.Add(cell22);
			layoutTable.Rows.Add(row2);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZTextBox NameTextBox;
		ZTextBox CityTextBox;

		ZTextLabel NameCaptionLabel;
		ZTextLabel CityCaptionLabel;

		ZTextLabel ControlCaption;

		#endregion
	}
}
