using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	[CodeProperty(RefAirlineSchema.Constants.RM_ThreeLetterCode), DescriptionProperty(RefAirlineSchema.Constants.RM_AirlineName1)]
	public class ThreeLetterRefAirline : RefAirline
	{
		public ThreeLetterRefAirline(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public static ThreeLetterRefAirline LoadFromAirline3LetterCode(BusinessObjectFactory factory, ZString airline3LetterCode)
		{
			ZQuery filter = new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, airline3LetterCode);
			filter.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, "");
			return factory.LoadTop1<ThreeLetterRefAirline>(filter);
		}
	}
}
