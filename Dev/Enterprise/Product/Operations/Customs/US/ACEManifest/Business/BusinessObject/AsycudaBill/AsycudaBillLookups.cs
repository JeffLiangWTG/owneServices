using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override CodeDescriptionPairList CustomsStatusList => AIMDispositionCodesHelper.GetCustomsStatusList(Parent.Factory);

		public override CodeDescriptionPairList CustomsEntryNumberTypes => Factory.GetCachedValue<ACEManifestBillEntryNumberTypes>();

		public ChildTariffViewCollection TariffCollection => ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem, ZDateTime.Today, null);

		public RefCountryCollection GoodsOrigins => new RefCountryCollection(Factory);
	}
}
