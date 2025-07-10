using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class ContainersFilterControl : ZFilterStripControl
	{
		public ContainersFilterControl()
		{
			InitializeComponent();
		}

		public ContainersFilterControl(IBusinessObjectCollection gridCollection, ContainerManagerFilterStrip filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ContainerModuleStrip();
		}
	}
}
