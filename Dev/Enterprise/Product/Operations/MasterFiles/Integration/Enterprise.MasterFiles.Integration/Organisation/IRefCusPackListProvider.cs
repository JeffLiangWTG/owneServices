using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefCusPackListProvider
	{
		CodeDescriptionPairList GetCustomsPackList(BusinessObjectFactory factory, ZString type, ZString country);

		CodeDescriptionPairList GetCommercialPackList(BusinessObjectFactory factory, ZString type);

		CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country);

		CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory);

		CodeDescriptionPairList GetPackConversionTypeList(BusinessObjectFactory factory);
	}
}
