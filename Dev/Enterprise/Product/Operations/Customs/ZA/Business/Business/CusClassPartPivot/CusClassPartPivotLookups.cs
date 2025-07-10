using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(AutoZACusClassPartPivot parent) : base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public ICodeDescriptionPairList ROOTypeOrPreferenceList
		{
			get
			{
				if (Parent.CI_ChildType == ClassificationTypeList.Codes.HTI)
				{
					return PrimaryPreferenceList;
				}
				else
				{
					return ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, ZDateTime.Today);
				}
			}
		}

		#region RefCusTariff Lookups

		public override BusinessObjectCollection Tariffs
		{
			get
			{
				return TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, ZDateTime.Today);
			}
		}

		#endregion

		public VehicleFormatList VehicleFormats => Factory.GetCachedValue<VehicleFormatList>();

		public VehicleTypeList VehicleTypes => Factory.GetCachedValue<VehicleTypeList>();

		public GoodsTypeList GoodsTypeList => Factory.GetCachedValue<GoodsTypeList>();
	}
}
