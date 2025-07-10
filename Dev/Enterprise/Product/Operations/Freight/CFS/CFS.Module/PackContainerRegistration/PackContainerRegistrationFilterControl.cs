using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	public partial class PackContainerRegistrationFilterControl : ZFilterStripControl<PackContainerRegistrationFilterStrip>
	{
		readonly System.ComponentModel.Container components;

		public PackContainerRegistrationFilterControl()
		{
			InitializeComponent();
		}

		public PackContainerRegistrationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
