using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBDestinationControl runat=server></{0}:AWBDestinationControl>")]
	public class AWBDestinationControl : AWBBaseControl
	{
		#region Constructors

		public AWBDestinationControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			AirportTextBox = new AWBTextBox();
			AirportTextBox.ID = (NoResString)"Airport";

			AirportCodeTextBox = new AWBTextBox();
			AirportCodeTextBox.ID = "AirportCode";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			AirportTextBox.BindTo = "EH_AirportOfDestinationText";
			AirportTextBox.Caption = Res.GetString("26c08d33-2876-4792-9dbb-58077b5e6c90", "Airport of Destination");
			AirportTextBox.FieldCaptionVisible = false;
			AirportTextBox.FieldCssClass = "AWBText15";

			AirportCodeTextBox.BindTo = "EH_AirportOfDestinationCode";
			AirportCodeTextBox.Caption = Res.GetString("7bf9e015-0238-417e-bb56-8ab71b18201b", "Code");
			AirportCodeTextBox.FieldCaptionVisible = false;
			AirportCodeTextBox.FieldCssClass = "AWBCode3";

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;

			TableRow layoutRow = new TableRow();
			TableCell cellAirport = new TableCell();
			cellAirport.Controls.Add(AirportTextBox);

			TableCell cellAirportCode = new TableCell();
			cellAirportCode.Controls.Add(AirportCodeTextBox);

			layoutRow.Cells.Add(cellAirport);
			layoutRow.Cells.Add(cellAirportCode);

			layoutTable.Rows.Add(layoutRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		AWBTextBox AirportTextBox;
		AWBTextBox AirportCodeTextBox;

		#endregion
	}
}
