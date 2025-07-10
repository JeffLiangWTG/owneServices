using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override OrgHeaderCollection ShippingLines => new ForwarderCollection(Factory);

		public override CodeDescriptionPairList ApplicationCodeList() => FilterBizObj.Factory.GetCachedValue<ApplicationCodeFilterList>();

		public override CodeDescriptionPairList ContainerModeList => FilterBizObj.Factory.GetCachedValue<CargoPackingTypeCodeList>();

		public CargoPackingCodeList ContainerModeList_TN41 => FilterBizObj.Factory.GetCachedValue<CargoPackingCodeList>();

		public override CodeDescriptionPairList MessageSubTypeList() => FilterBizObj.Factory.GetCachedValue<DeclarationTypeCodeList>();

		public override CodeDescriptionPairList TransportTypeList => FilterBizObj.Factory.GetCachedValue<TransportModeCodeList>();

		public override CodeDescriptionPairList PaymentPartyList() => FilterBizObj.Factory.GetCachedValue<BGIndicatorCodeList>();

		public CodeDescriptionPairList GlobalManifestStatusList => FilterBizObj.Factory.GetCachedValue<GlobalManifestStatusList>();

		public OrgHeaderCollection Organisations => organisations ?? (organisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisations;
	}
}
