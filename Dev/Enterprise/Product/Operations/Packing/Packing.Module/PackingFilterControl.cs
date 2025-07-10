using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.Module
{
	public partial class PackingFilterControl : ZFilterStripControl
	{
		public PackingFilterControl()
		{
			InitializeComponent();
		}

		public PackingFilterControl(IBusinessObjectCollection gridCollection, PackingFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
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
