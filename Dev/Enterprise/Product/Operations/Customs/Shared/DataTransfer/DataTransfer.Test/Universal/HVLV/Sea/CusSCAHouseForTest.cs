using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	sealed class CusSCAHouseForTest : BaseCusSCAHouse
	{
		public CusSCAHouseForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection() => Enumerable.Empty<BaseCusSCAPivot>();
	}
}
