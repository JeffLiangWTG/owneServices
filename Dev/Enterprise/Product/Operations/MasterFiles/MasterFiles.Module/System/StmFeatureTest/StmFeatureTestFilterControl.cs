using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class StmFeatureTestFilterControl : ZFilterStripControl
	{
		public StmFeatureTestFilterControl(IBusinessObjectCollection gridCollection, StmFeatureTestFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
