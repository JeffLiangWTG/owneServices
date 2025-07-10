using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefAirlineProductCodeSchema.Constants.RAR_Code), DescriptionProperty(RefAirlineProductCodeSchema.Constants.RAR_Description)]
	public class RefAirlineProductCode : AutoRefAirlineProductCode
	{
		public RefAirlineProductCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
