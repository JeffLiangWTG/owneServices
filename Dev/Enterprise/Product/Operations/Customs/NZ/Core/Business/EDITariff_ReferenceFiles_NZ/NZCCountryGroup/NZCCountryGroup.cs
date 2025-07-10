using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCountryGroup : AutoNZCCountryGroup
	{
		public NZCCountryGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZString U4_GroupName
		{
			get
			{
				ZString result = ZString.Empty;
				NZCGroup group = Factory.LoadTop1<NZCGroup>(new ZQuery(NZCGroupSchema.Q4_Code, U4_Group));

				if (group != null)
				{
					result = group.Q4_Name;
				}

				return result;
			}
		}

		internal static void AddFilterForCountryGroups(ZQuery countryGroupQuery, ZString country, ZDateTime dateForDutyRate)
		{
			countryGroupQuery.AddToFilter(NZCCountryGroupSchema.U4_Country, country);
			countryGroupQuery.AddToFilter(new ZQuery(
				NZCCountryGroupSchema.U4_DateFrom, SQLComparisonOperator.LessThanOrEqualTo, dateForDutyRate));
			countryGroupQuery.AddToFilter(new ZQuery(
				NZCCountryGroupSchema.U4_DateTo, SQLComparisonOperator.GreaterThanOrEqualTo, dateForDutyRate).Or(
				NZCCountryGroupSchema.U4_DateTo, SQLComparisonOperator.Equal, ZDateTime.Empty));
		}
	}
}
