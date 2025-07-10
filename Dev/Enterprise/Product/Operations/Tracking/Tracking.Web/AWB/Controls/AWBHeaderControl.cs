using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBHeaderControl runat=server></{0}:AWBHeaderControl>")]
	public class AWBHeaderControl : AWBBaseControl
	{
		#region Constructors

		public AWBHeaderControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			AirlinePrefixTextBox = new ZTextBox();
			AirlinePrefixTextBox.ID = "AirlinePrefix";

			AWBOriginTextBox = new ZTextBox();
			AWBOriginTextBox.ID = "AWBOrigin";

			AWBSerialTextBox = new ZTextBox();
			AWBSerialTextBox.ID = "AWBSerial";
		}

		protected override void CreateChildControls()
		{
			AirlinePrefixTextBox.BindTo = "EH_AirlinePrefix";
			AirlinePrefixTextBox.Width = 50;

			AWBOriginTextBox.BindTo = "EH_AWBOriginCode";
			AWBOriginTextBox.Width = 50;

			AWBSerialTextBox.BindTo = "EH_AWBSerialNo";
			AWBSerialTextBox.Width = 150;

			base.CreateChildControls();

			Table layoutTable = new Table();
			layoutTable.CellSpacing = 0;
			layoutTable.CellPadding = 5;

			TableRow layoutRow = new TableRow();
			TableCell layoutCell1 = new TableCell();
			TableCell layoutCell2 = new TableCell();
			TableCell layoutCell3 = new TableCell();

			layoutCell1.Controls.Add(AirlinePrefixTextBox);
			layoutCell2.Controls.Add(AWBOriginTextBox);
			layoutCell3.Controls.Add(AWBSerialTextBox);

			layoutRow.Cells.Add(layoutCell1);
			layoutRow.Cells.Add(layoutCell2);
			layoutRow.Cells.Add(layoutCell3);

			layoutCell1.Style.Add("border-right", (NoResString)"2px solid #000000");
			layoutCell2.Style.Add("border-right", (NoResString)"2px solid #000000");

			layoutTable.Rows.Add(layoutRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZTextBox AirlinePrefixTextBox;
		ZTextBox AWBOriginTextBox;
		ZTextBox AWBSerialTextBox;

		#endregion
	}
}
