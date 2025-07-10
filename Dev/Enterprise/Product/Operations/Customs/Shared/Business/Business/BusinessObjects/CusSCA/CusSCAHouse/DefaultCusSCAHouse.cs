using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class DefaultCusSCAHouse : BaseCusSCAHouse
	{
		public DefaultCusSCAHouse(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection()
		{
			return Factory.Load<BaseCusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, PK));
		}
	}
}
