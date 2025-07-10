using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration;

public interface IExternalRequestGenerationProvider
{
	ZString GetRequestJobID();

	ZString GetRequestTypeCode();

	(ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType);
}
