using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class StmMenuItemFilterControl : ZFilterStripControl
	{
		public StmMenuItemFilterControl(IBusinessObjectCollection gridCollection, StmMenuItemFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
