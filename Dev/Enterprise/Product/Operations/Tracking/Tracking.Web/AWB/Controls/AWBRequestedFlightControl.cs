using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBRequestedFlightControl runat=server></{0}:AWBRequestedFlightControl>")]
	public class AWBRequestedFlightControl : AWBFieldControl
	{
		#region Constructors

		public AWBRequestedFlightControl()
			: base() { }

		#endregion

		#region Overrides

		public override bool IsBindable(object dataSource)
		{
			return dataSource != null && dataSource is BusinessObject;
		}

		protected override WebControl GetNewFieldBox()
		{
			Carrier1CodeBox = new ZTextBox();
			Carrier1CodeBox.ID = "Carrier1Code";
			Carrier1CodeBox.BindTo = "EH_Booking1stCarrier";
			Carrier1CodeBox.CssClass = "AWBCode2";

			Carrier1FlightBox = new ZTextBox();
			Carrier1FlightBox.ID = "Carrier1Flight";
			Carrier1FlightBox.BindTo = "EH_Booking1stFlight";
			Carrier1FlightBox.CssClass = "AWBText5";

			Carrier1DateBox = new ZTextBox();
			Carrier1DateBox.ID = "Carrier1Date";
			Carrier1DateBox.BindTo = "EH_Booking1stFlightDate";
			Carrier1DateBox.CssClass = "AWBCode2";

			Carrier2CodeBox = new ZTextBox();
			Carrier2CodeBox.ID = "Carrier2Code";
			Carrier2CodeBox.BindTo = "EH_Booking2ndCarrier";
			Carrier2CodeBox.CssClass = "AWBCode2";

			Carrier2FlightBox = new ZTextBox();
			Carrier2FlightBox.ID = "Carrier2Flight";
			Carrier2FlightBox.BindTo = "EH_Booking2ndFlight";
			Carrier2FlightBox.CssClass = "AWBText5";

			Carrier2DateBox = new ZTextBox();
			Carrier2DateBox.ID = "Carrier2Date";
			Carrier2DateBox.BindTo = "EH_Booking2ndFlightDate";
			Carrier2DateBox.CssClass = "AWBCode2";

			Table layoutTable = new Table();
			TableRow layoutRow = new TableRow();

			TableCell cell1 = new TableCell();
			cell1.Controls.Add(Carrier1CodeBox);

			TableCell cell2 = new TableCell();
			cell2.Controls.Add(Carrier1FlightBox);

			TableCell cell3 = new TableCell();
			cell3.Controls.Add(new ZTextLabel() { Text = "/" });

			TableCell cell4 = new TableCell();
			cell4.CssClass = "AWBFieldSection";
			cell4.Controls.Add(Carrier1DateBox);

			TableCell cell5 = new TableCell();
			cell5.Controls.Add(Carrier2CodeBox);

			TableCell cell6 = new TableCell();
			cell6.Controls.Add(Carrier2FlightBox);

			TableCell cell7 = new TableCell();
			cell7.Controls.Add(new ZTextLabel() { Text = "/" });

			TableCell cell8 = new TableCell();
			cell8.Controls.Add(Carrier2DateBox);

			layoutRow.Cells.Add(cell1);
			layoutRow.Cells.Add(cell2);
			layoutRow.Cells.Add(cell3);
			layoutRow.Cells.Add(cell4);
			layoutRow.Cells.Add(cell5);
			layoutRow.Cells.Add(cell6);
			layoutRow.Cells.Add(cell7);
			layoutRow.Cells.Add(cell8);

			layoutTable.Rows.Add(layoutRow);
			return layoutTable;
		}

		#endregion

		#region Implementation

		ZTextBox Carrier1CodeBox;
		ZTextBox Carrier1FlightBox;
		ZTextBox Carrier1DateBox;

		ZTextBox Carrier2CodeBox;
		ZTextBox Carrier2FlightBox;
		ZTextBox Carrier2DateBox;

		#endregion
	}
}
