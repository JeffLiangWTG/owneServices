using System;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class RollBookingApplicatorControl : ZUserControl
	{
		public RollBookingApplicatorControl()
		{
			InitializeComponent();
		}

		#region SelectScheduleButton_Click

		void SelectScheduleButton_Click(object sender, EventArgs e)
		{
			if (TransportsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("d3c58036-50a4-4e51-a59a-bf5e821464b0", "Please select a linked routing leg first."));
				return;
			}
			else if (TransportsGrid.SelectedElements.Length > 1)
			{
				Globals.Message.ShowError(Res.GetString("91b9d284-6c6f-4971-aba3-258a2633d17e", "Please select only one routing leg."));
				return;
			}

			var selectedTransport = TransportsGrid.SelectedElements.Cast<Transport>().First();

			if (!selectedTransport.JW_IsLinked)
			{
				Globals.Message.ShowError(Res.GetString("2d0a252b-30a0-4a42-8c98-8bcd6f745980", "Current leg is not linked."));
			}
			else
			{
				var helper = new SailingIFindBox(selectedTransport, (ZForm)FindForm());
				helper.ShowModuleFromTransport();
			}
		}

		#endregion
	}
}


