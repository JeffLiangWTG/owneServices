using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	sealed class CusSCAPivotForTest : BaseCusSCAPivot
	{
		public CusSCAPivotForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
