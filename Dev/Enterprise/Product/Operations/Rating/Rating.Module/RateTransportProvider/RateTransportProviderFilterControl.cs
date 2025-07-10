using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Module
{
	public partial class RateTransportProviderFilterControl : ZFilterStripControl
	{
		public RateTransportProviderFilterControl(IBusinessObjectCollection gridCollection, RateTransportProviderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public RateTransportProviderFilterControl()
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
