using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefJobEquipmentFilterControl : ZFilterStripControl
	{
		public RefJobEquipmentFilterControl()
		{
			InitializeComponent();
		}

		public RefJobEquipmentFilterControl(IBusinessObjectCollection gridCollection, RefJobEquipmentFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
