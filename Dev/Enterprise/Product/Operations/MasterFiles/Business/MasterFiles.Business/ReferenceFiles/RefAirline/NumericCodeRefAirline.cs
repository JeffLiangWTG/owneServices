using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefAirlineSchema.Constants.RM_EagleAddedAirlinePrefixOrAccountingCode), DescriptionProperty(RefAirlineSchema.Constants.RM_AirlineName1)]
	public class NumericCodeRefAirline : RefAirline
	{
		public NumericCodeRefAirline(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
