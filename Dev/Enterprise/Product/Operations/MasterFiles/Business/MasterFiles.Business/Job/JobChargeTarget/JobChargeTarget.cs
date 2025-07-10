using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeTarget : AutoJobChargeTarget
	{
		public JobChargeTarget(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
