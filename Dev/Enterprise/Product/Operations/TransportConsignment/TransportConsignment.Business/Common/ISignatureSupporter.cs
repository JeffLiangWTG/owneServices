using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business
{
	public interface ISignatureSupporter
	{
		ZBool HasSignature { get; }
		ZBlob ReceivedBySignature { get; }
	}
}
