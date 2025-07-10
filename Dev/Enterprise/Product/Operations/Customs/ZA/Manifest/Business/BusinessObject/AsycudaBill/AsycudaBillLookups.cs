using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override CodeDescriptionPairList CustomsStatusList => Parent.Header?.IsAQM ?? ZBool.False
			? CargoReleaseStatusList
			: base.CustomsStatusList;

		public CodeDescriptionPairList CargoReleaseStatusList => ASYCUDA.Business.CountryHelper.GetCustomsStatusList(Factory, Parent.CountryCode, nameof(ManifestDocumentType.AQM));
		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<ZAMessageStatusList>();
		public override CodeDescriptionPairList CustomsEntryNumberTypes => Factory.GetCachedValue<ZaLRNTypes>();
	}
}
