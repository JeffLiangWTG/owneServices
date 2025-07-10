//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateViewLookups
//
//    This class should be used for overriding collections in AutoRateViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RateViewLookups : AutoRateViewLookups
	{
		public RateViewLookups(AutoRateView parent) : base(parent)
		{
		}

		public CusRefPreferenceCollection PreferenceCodeList
		{
			get
			{
				var result = new CusRefPreferenceCollection(Factory);
				result.AdditionalFilter = new ZQuery(CusRefPreferenceSchema.CR8_RN_NKCountryCode, SQLComparisonOperator.Equal, DataGrouping);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusRefPreferenceCollection.FilterConstants.CountryCode, "Property", DataGrouping, false));
				return result;
			}
		}

		public CusRefRateCodeCollection RateCodeList
		{
			get
			{
				var result = new CusRefRateCodeCollection(Factory);
				result.AdditionalFilter = new ZQuery(CusRefRateCodeSchema.CR7_RN_NKCountryCode, SQLComparisonOperator.Equal, DataGrouping);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusRefRateCodeCollection.FilterConstants.CountryCode, "Property", DataGrouping, false));
				return result;
			}
		}

		protected new RateView Parent => (RateView)base.Parent;

		ZString DataGrouping => Parent.CusTariff?.ZZ1_ZZZ_NKDataGrouping ?? ZString.Empty;
	}
}
