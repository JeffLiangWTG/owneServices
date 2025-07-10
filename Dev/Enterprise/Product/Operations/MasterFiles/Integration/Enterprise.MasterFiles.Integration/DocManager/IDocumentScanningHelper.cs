using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentScanningHelper
	{
		IRefDocTypeCollection GetDocTypesFromJobType(ZString jobType, BusinessObjectFactory factory, bool checkSecurityRights);

		IRefDocTypeCollection GetAvailableDocumentTypes(ZString docManagerCode, BusinessObjectFactory factory);
	}
}
