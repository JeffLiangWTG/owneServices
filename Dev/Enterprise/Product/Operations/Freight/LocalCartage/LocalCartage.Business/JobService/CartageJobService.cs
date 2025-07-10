using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageJobService : JobService
	{
		public CartageJobService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override JobServiceLookups GetNewLookups()
		{
			return new CartageJobServiceLookups(this);
		}
	}
}
