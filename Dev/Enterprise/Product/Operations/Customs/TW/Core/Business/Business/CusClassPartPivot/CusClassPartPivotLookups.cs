using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		IRefCusPackListProvider CachedCusPackListProvider => Factory.GetCachedValue<RefCusPackListProvider>();

		public CodeDescriptionPairList PartPivotUOMList => CachedCusPackListProvider.GetCommercialPackList(Factory, ZString.Empty);

		public CodeDescriptionPairList CarTypeCodeList => Factory.GetCachedValue<CarTypeCodeList>();

		public CodeDescriptionPairList TransmissionCodeList => Factory.GetCachedValue<TransmissionCodeList>();

		public CodeDescriptionPairList EngineTypeCodeList => Factory.GetCachedValue<EngineTypeCodeList>();

		public CodeDescriptionPairList LeftSideSteeringCodeList => Factory.GetCachedValue<LeftSideSteeringCodeList>();

		public CodeDescriptionPairList CatalystConverterPrintModeList => Factory.GetCachedValue<CatalystConverterPrintModeList>();

		public CodeDescriptionPairList CarConditionCodeList => Factory.GetCachedValue<CarConditionCodeList>();

		public CodeDescriptionPairList EquipmentPrintModeList => Factory.GetCachedValue<EquipmentPrintModeList>();

		public ICodeDescriptionPairList ModeOfStatistics => RefCusProcedure.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, ZString.Empty, Constants.CusInBondBill.ShipmentType.Export);

		public ICodeDescriptionPairList DutyTreatment => RefCusProcedure.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, ZString.Empty, Constants.CusInBondBill.ShipmentType.Import);

		public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);

		public CodeDescriptionPairList ContainerMaterialList => Factory.GetCachedValue<ContainerMaterialList>();

		public CodeDescriptionPairList ContainerCapacityList => Factory.GetCachedValue<ContainerCapacityList>();

		public CodeDescriptionPairList ContainerMaterialNumberList => Factory.GetCachedValue<ContainerMaterialNumberList>();

		public CodeDescriptionPairList DeclarationGoodsDescriptionModeList => Factory.GetCachedValue<DeclarationGoodsDescriptionModeList>();
	}
}
