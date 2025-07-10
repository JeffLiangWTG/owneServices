using System.Collections;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public CodeDescriptionPairList TR_GM_PresentationCustomsOfficeList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.AMA_RN_NKCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

		public CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue("TRAsycudaManifestHeaderLookups.TransportTypeList." + Parent.AMA_TransportMode, () =>
		{
			return TRTransportTypes.GetTransportTypesByTransportMode(Parent.AMA_TransportMode);
		});

		protected override ICollection GetCustomsDischargePortListCore()
		{
			return Parent.IsExport ? UnlocoList : base.GetCustomsDischargePortListCore();
		}

		protected override ICollection GetCustomsLoadingPortListCore()
		{
			return Parent.IsExport ? base.GetCustomsLoadingPortListCore() : UnlocoList;
		}

		RefUNLOCOCollection UnlocoList => new RefUNLOCOCollection(Factory);
	}
}
