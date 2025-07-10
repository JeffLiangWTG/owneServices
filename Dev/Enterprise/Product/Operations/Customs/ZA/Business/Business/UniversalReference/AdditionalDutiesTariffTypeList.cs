using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class AdditionalDutiesTariffTypeList : Customs.Business.AdditionalDutiesTariffTypeList
	{
		public AdditionalDutiesTariffTypeList(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.SouthAfrica,
				new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, SQLComparisonOperator.NotEqual,
					UniversalReferenceConstants.CusTariffCode.Schedule1Part1))
		{
		}
	}
}
