using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRulesCollection : BusinessObjectCollection<RefCountryRules>
	{
		public RefCountryRulesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefCountryRulesCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public void LoadCountry(RefCountry country)
		{
			ZQuery query = new ZQuery();

			query.DefaultJoinCondition = JoinCondition.Or;
			query.AddToFilter(RefCountryRulesSchema.R7_RN_NKDestination, country.RN_Code);
			query.AddToFilter(RefCountryRulesSchema.R7_RN_NKOrigin, country.RN_Code);

			RemoveAllButLeaveRelationshipsIntact();
			AddRange(Factory.Load(typeof(RefCountryRules), query));
			LastLoadedAdditionalFilter = query;

			countryCode = country.Code;
			foreach (RefCountryRules rule in this)
			{
				rule.CurrentCountryCode = CountryCode;
			}
		}

		public ZString CountryCode
		{
			get { return countryCode; }
		}

		ZString countryCode;

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			var result = base.CreateBusinessObjectFromRow(row);
			((RefCountryRules)result).CurrentCountryCode = CountryCode;
			return result;
		}
	}
}
