using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBRoutingControl runat=server></{0}:AWBRoutingControl>")]
	public class AWBRoutingControl : AWBBaseControl
	{
		#region Constructors

		public AWBRoutingControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			ToTextBox = new AWBTextBox();
			ToTextBox.ID = (NoResString)"To";

			To1TextBox = new AWBTextBox();
			To1TextBox.ID = "To1";

			To2TextBox = new AWBTextBox();
			To2TextBox.ID = "To2";

			By1TextBox = new AWBTextBox();
			By1TextBox.ID = "By1";

			By2TextBox = new AWBTextBox();
			By2TextBox.ID = "By2";

			ByFindBox = new AWBTextBox();
			ByFindBox.ID = (NoResString)"By";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			ToTextBox.BindTo = "EH_To1st";
			ToTextBox.Caption = Res.GetString("45184fc5-66f6-43d2-959a-59349e893ed4", "To");
			ToTextBox.FieldCssClass = "AWBCode3";
			ToTextBox.FieldCaptionVisible = false;
			ToTextBox.DataFieldAlign = HorizontalAlign.Center;

			To1TextBox.BindTo = "EH_To2nd";
			To1TextBox.Caption = Res.GetString("727504dc-d2bd-423d-a3cc-7a9b7c93b862", "to");
			To1TextBox.FieldCssClass = "AWBCode3";
			To1TextBox.FieldCaptionVisible = false;
			To1TextBox.DataFieldAlign = HorizontalAlign.Center;

			To2TextBox.BindTo = "EH_To3rd";
			To2TextBox.Caption = Res.GetString("727504dc-d2bd-423d-a3cc-7a9b7c93b862", "to");
			To2TextBox.FieldCssClass = "AWBCode3";
			To2TextBox.FieldCaptionVisible = false;
			To2TextBox.DataFieldAlign = HorizontalAlign.Center;

			By1TextBox.BindTo = "EH_By2nd";
			By1TextBox.Caption = Res.GetString("b2234a59-b056-48fb-98b6-a03f412cf51a", "by");
			By1TextBox.FieldCssClass = "AWBCode2";
			By1TextBox.FieldCaptionVisible = false;
			By1TextBox.DataFieldAlign = HorizontalAlign.Center;

			By2TextBox.BindTo = "EH_By3rd";
			By2TextBox.Caption = Res.GetString("b2234a59-b056-48fb-98b6-a03f412cf51a", "by");
			By2TextBox.FieldCssClass = "AWBCode2";
			By2TextBox.FieldCaptionVisible = false;
			By2TextBox.DataFieldAlign = HorizontalAlign.Center;

			ByFindBox.BindTo = "EH_By1st";
			ByFindBox.Caption = Res.GetString("41e396c9-30cf-405b-9087-445bc549f94e", "By First Carrier");
			ByFindBox.FieldCssClass = "AWBCode3";
			ByFindBox.FieldCaptionVisible = false;
			ByFindBox.DataFieldAlign = HorizontalAlign.Center;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;

			TableRow layoutRow = new TableRow();
			TableCell cellTo = new TableCell();
			cellTo.CssClass = "AWBFieldSection";
			cellTo.Controls.Add(ToTextBox);

			TableCell cellBy = new TableCell();
			cellBy.CssClass = (NoResString)"AWBFieldSection AWBFirstCarrier";
			cellBy.Controls.Add(ByFindBox);

			TableCell cellTo1 = new TableCell();
			cellTo1.CssClass = "AWBFieldSection";
			cellTo1.Controls.Add(To1TextBox);

			TableCell cellTo2 = new TableCell();
			cellTo2.CssClass = "AWBFieldSection";
			cellTo2.Controls.Add(To2TextBox);

			TableCell cellBy1 = new TableCell();
			cellBy1.CssClass = "AWBFieldSection";
			cellBy1.Controls.Add(By1TextBox);

			TableCell cellBy2 = new TableCell();
			cellBy2.Controls.Add(By2TextBox);

			layoutRow.Cells.Add(cellTo);
			layoutRow.Cells.Add(cellBy);
			layoutRow.Cells.Add(cellTo1);
			layoutRow.Cells.Add(cellBy1);
			layoutRow.Cells.Add(cellTo2);
			layoutRow.Cells.Add(cellBy2);

			layoutTable.Rows.Add(layoutRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		AWBTextBox ToTextBox;
		AWBTextBox To1TextBox;
		AWBTextBox To2TextBox;
		AWBTextBox By1TextBox;
		AWBTextBox By2TextBox;
		AWBTextBox ByFindBox;

		#endregion
	}
}
