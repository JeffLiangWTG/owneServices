using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business;

interface IMovementRequestIdProvider
{
	ZString RequestId { get; }
}
