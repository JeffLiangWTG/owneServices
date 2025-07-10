using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.Module
{
	public partial class USInBondMoveHeaderFilterControl : ZFilterStripControl
	{
		public USInBondMoveHeaderFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
