using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class JobOrphanScan : AutoJobOrphanScan
	{
		public JobOrphanScan(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}

