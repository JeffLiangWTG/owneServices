using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.PortHubs.Module
{
	public partial class PortDepotCarrierFilterStripControl : ZFilterStripControl
	{
		public PortDepotCarrierFilterStripControl(IBusinessObjectCollection gridCollection, PortDepotCarrierFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		private readonly System.ComponentModel.IContainer components;
	}
}
