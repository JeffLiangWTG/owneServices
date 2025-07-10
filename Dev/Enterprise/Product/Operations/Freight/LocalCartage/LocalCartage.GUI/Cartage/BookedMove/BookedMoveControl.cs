using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.ComponentModel;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class BookedMoveControl : ZUserControl
	{
		public BookedMoveControl()
		{
			InitializeComponent();
			DistanceButton.Click += delegate
			{ if (BookedMove != null) { BookedMove.SetCalculatedDistance((INotifications)ParentForm); } };

			SetupLayout();
		}

		public enum BookedMovesLayout
		{
			CartageLeg,
			Container,
			Loose,
		}

		[Category("Misc"), Description("BookedMoveLayout. (Default: CartageLeg)")]
		public BookedMovesLayout BookedMoveLayout
		{
			get { return bookedMoveLayout; }
			set
			{
				bookedMoveLayout = value;
				SetupLayout();
			}
		}
		BookedMovesLayout bookedMoveLayout = BookedMovesLayout.CartageLeg;

		void SetupLayout()
		{
			switch (BookedMoveLayout)
			{
				case BookedMovesLayout.CartageLeg:
					LooseDetailsPanel.Visible = false;
					break;
				case BookedMovesLayout.Container:
					LooseDetailsPanel.Visible = false;
					break;
				case BookedMovesLayout.Loose:
					LooseDetailsPanel.Visible = true;
					break;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "BookedMoveLayout '{0}' not supported in SetupLayout.", BookedMoveLayout.ToString()));
			}
		}

		CommonBookedCtgMove BookedMove
		{
			get { return (CommonBookedCtgMove)CurrentDataItem; }
		}
	}
}
