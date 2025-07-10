using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeCollection : BusinessObjectCollection<JobCharge>
	{
		public JobChargeCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public JobChargeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override void Load(ZQuery filter)
		{
			throw new NotSupportedException(@"Loading of the JobChargeCollection is not supported.
We experienced poor memory performance when the JobChargeCollection was used for loading the same JobCharge entity many times in different collections. Those collections were accumulated and not disposed,
Use Factory.Load<JobCharge>(...) and List<JobCharge> in non-GUI bound cases. See this alternative for the old BusinessObjectCollection https://stackoverflow.com/c/wisetechglobal/questions/901");
		}
	}
}
