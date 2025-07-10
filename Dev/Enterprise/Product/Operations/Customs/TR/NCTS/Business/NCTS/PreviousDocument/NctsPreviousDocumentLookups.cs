using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsPreviousDocumentLookups : EU.NCTS.Business.NctsPreviousDocumentPhase4Lookups, INctsPreviousDocumentLookups
	{
		public NctsPreviousDocumentLookups(NctsPreviousDocument parent) : base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList IncotermList => Factory.GetCachedValue<IncotermCodeList>();

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<PaymentTypeList>();

		public CodeDescriptionPairList NatureOfBusinessList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, NatureOfBusinessType);

		const string NatureOfBusinessType = "TRNOB";
		const string PreviousDocumentsCodeListType = "DC44P";

		public ZZRefCusCodeListCombinedCollection PreviousDocumentsCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
				, Core.Constants.CountryCodes.Turkey
				, PreviousDocumentsCodeListType
				, ZDateTime.Today);

		RefCountryCollection INctsPreviousDocumentLookups.Countries => base.Countries;
		CodeDescriptionPairList INctsPreviousDocumentLookups.UnitOfQuantityList => base.UnitOfQuantityList;
	}
}
