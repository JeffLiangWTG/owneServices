using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Manifest.Business;

public sealed class PreviousDocumentLookups : CusSupportingInfoLookups
{
	public PreviousDocumentLookups(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	public override ICollection CodeList => GetPreviousDocumentTypeCodes();

	ZZRefCusCodeListCombinedCollection GetPreviousDocumentTypeCodes()
	{
		return PreviousDocumentRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill, ZDateTime.Today);
	}
}
