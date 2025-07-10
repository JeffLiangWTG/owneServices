//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGateTransportCFSDetailValidation
//
//    This class should be used for overriding validation in AutoGateTransportCFSDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailValidation : AutoGateTransportCFSDetailValidation
	{
		public GateTransportCFSDetailValidation(AutoGateTransportCFSDetail parent) : base(parent)
		{
		}
	}
}
