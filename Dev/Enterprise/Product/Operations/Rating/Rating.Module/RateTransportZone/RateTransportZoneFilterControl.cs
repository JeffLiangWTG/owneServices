using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Module
{
	public partial class RateTransportZoneFilterControl : ZFilterStripControl
	{
		public RateTransportZoneFilterControl(IBusinessObjectCollection gridCollection, RateTransportZoneFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public RateTransportZoneFilterControl()
		{
			InitializeComponent();
		}

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
