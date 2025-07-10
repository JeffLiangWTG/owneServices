using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefFacilityLocalCode : AutoRefFacilityLocalCode
	{
		public RefFacilityLocalCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
