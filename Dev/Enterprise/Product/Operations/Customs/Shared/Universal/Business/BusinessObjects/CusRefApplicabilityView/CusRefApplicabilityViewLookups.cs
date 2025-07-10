using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefApplicabilityViewLookups : AutoCusRefApplicabilityViewLookups
	{
		public CusRefApplicabilityViewLookups(AutoCusRefApplicabilityView parent) : base(parent)
		{
		}

		public CusRefTradeGroupCollection TradeGroupList
		{
			get
			{
				var result = new CusRefTradeGroupCollection(Factory);
				var dataGrouping = Parent.Rate?.CusTariff?.ZZ1_ZZZ_NKDataGrouping ?? ZString.Empty;
				result.AdditionalFilter = new ZQuery(CusRefTradeGroupSchema.CR9_RN_NKCountryCode, SQLComparisonOperator.Equal, dataGrouping);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusRefTradeGroupCollection.FilterConstants.CountryCode, "Property", dataGrouping, false));
				return result;
			}
		}

		protected new CusRefApplicabilityView Parent => (CusRefApplicabilityView)base.Parent;
	}
}
