using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Business
{
	public interface IDtbTransportConfirmationCollection : IActiveBusinessObjectCollection
	{
		new DtbTransportConfirmation this[int index] { get; }
	}
}
