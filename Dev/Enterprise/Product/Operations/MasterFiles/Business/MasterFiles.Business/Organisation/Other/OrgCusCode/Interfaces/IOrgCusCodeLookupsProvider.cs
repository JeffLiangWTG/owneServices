using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	interface IOrgCusCodeLookupsProvider
	{
		CodeDescriptionPairList GetCustomsRegNoLookupList(ZString orgCusCodeType);
		string GetCustomsRegNoFieldType(ZString orgCusCodeType);
	}
}
