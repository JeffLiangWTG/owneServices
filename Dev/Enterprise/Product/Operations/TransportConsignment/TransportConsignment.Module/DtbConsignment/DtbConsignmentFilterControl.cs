using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.Module
{
	public partial class DtbConsignmentFilterControl : ZFilterStripControl<DtbTransportWorkflowFilterStrip>
	{
		public DtbConsignmentFilterControl()
			: this(null, null)
		{
		}

		public DtbConsignmentFilterControl(IBusinessObjectCollection gridCollection, DtbConsignmentFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
