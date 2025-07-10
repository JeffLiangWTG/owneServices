using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccWithholdingFilterControl : ZFilterStripControl
	{
		public AccWithholdingFilterControl(IBusinessObjectCollection gridCollection, AccWithholdingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
