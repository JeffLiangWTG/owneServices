using System.Collections;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Manifest.Business;

public sealed class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
{
	public AsycudaBillLookups(AsycudaBill parent)
		: base(parent)
	{
	}

	public ICodeDescriptionPairList ImportProcedureCodeList => new NOImportProcedureCodeList();

	public ICodeDescriptionPairList ExportProcedureCodeList => new NOExportProcedureCodeList();

	public ICodeDescriptionPairList ForwarderStatePairList => Parent.Forwarder?.OA_State_List_For_Current_Address ?? new CodeDescriptionPairList();

	public ICodeDescriptionPairList TransportDocumentTypeList => Factory.GetCachedValue<ASYCUDA.Business.TransportDocumentTypes>();

	public IEnumerable PortOfLoadingCodes => Parent.ABL_RL_NKPortOfLoading.Length == 2 ? new RefCountryCollection(Factory) : new RefUNLOCOCollection(Factory);

	public IEnumerable PortOfDischargeCodes => Parent.ABL_RL_NKPortOfDischarge.Length == 2 ? new RefCountryCollection(Factory) : new RefUNLOCOCollection(Factory);

	public IEnumerable FinalDestinationCodes => Parent.ABL_RL_NKFinalDestination.Length == 2 ? new RefCountryCollection(Factory) : new RefUNLOCOCollection(Factory);
}
