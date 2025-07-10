using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgCodeLists
	{
		CodeDescriptionPairList GetAddressTypesList(BusinessObjectFactory factory);
	}
}
