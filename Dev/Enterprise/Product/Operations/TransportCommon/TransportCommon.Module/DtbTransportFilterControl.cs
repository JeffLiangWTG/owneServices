using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportCommon.Module
{
	public abstract partial class DtbTransportFilterControl<T> : ZFilterStripControl<DtbTransportWorkflowFilterStrip>
		where T : DtbTransport
	{
		// for the designer
		protected DtbTransportFilterControl()
		{
		}

		protected DtbTransportFilterControl(DtbTransportCollection<T> gridCollection, DtbTransportFilterBusinessObject<T> filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
