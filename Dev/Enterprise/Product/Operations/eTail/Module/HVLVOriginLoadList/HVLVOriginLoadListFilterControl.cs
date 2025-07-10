using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module
{
	partial class HVLVOriginLoadListFilterControl : ZFilterStripControl
	{
		public HVLVOriginLoadListFilterControl(IBusinessObjectCollection gridCollection, HVLVOriginLoadListFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new HVLVOriginLoadListModuleStrip();
		}
	}
}
