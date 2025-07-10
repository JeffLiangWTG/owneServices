using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module
{
	public partial class HVLVConsignmentFilterControl : ZFilterStripControl
	{
		public HVLVConsignmentFilterControl(IBusinessObjectCollection gridCollection, HVLVConsignmentFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new HVLVConsignmentModuleStrip();
		}
	}
}
