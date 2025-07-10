using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.Module
{
	public partial class DtbConsignmentRunSheetFilterControl : ZFilterStripControl
	{
		// for the designer
		public DtbConsignmentRunSheetFilterControl()
			: this(null, null)
		{
		}

		public DtbConsignmentRunSheetFilterControl(IBusinessObjectCollection gridCollection, DtbConsignmentRunSheetFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
