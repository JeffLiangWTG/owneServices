using System.Collections;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public interface INctsAdditionalInfoLookups
	{
		ZZRefCusCodeListCombinedCollection AdditionalInfoCodesList { get; }
		ICollection CodeList { get; }
		CodeDescriptionPairList CustomsOfficeList { get; }
		CodeDescriptionPairList ProcedureList { get; }
		CodeDescriptionPairList StatusList { get; }
		CodeDescriptionPairList SubTypeList { get; }
		CodeDescriptionPairList TypeList { get; }
		CodeDescriptionPairList UnitOfQuantityList { get; }
		CodeDescriptionPairList UnitOfQuantity2List { get; }
		CodeDescriptionPairList UnitOfQuantity3List { get; }
		CodeDescriptionPairList IssuerTypeList { get; }
		CodeDescriptionPairList PackTypeList { get; }
		CodeDescriptionPairList List63ExportFromCountries { get; }
	}
}


