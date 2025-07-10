using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsJobService : JobService
	{
		public WhsJobService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override JobServiceLookups GetNewLookups()
		{
			return new WhsJobServiceLookups(this);
		}

		protected override JobServiceValidation GetNewValidation()
		{
			return new WhsJobServiceValidation(this);
		}
	}
}
