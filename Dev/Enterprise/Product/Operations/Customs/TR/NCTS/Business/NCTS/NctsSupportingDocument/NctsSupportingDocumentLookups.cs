using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsSupportingDocumentLookups : EU.NCTS.Business.NctsSupportingDocumentPhase4Lookups
	{
		public NctsSupportingDocumentLookups(NctsSupportingDocument parent) : base(parent)
		{
		}

		public override ZZRefCusCodeListCombinedCollection TypeCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
				, DataGroupingCode
				, RefCusCodeListType
				, ZDateTime.Today);

		const string RefCusCodeListType = "DC44S";
	}
}
