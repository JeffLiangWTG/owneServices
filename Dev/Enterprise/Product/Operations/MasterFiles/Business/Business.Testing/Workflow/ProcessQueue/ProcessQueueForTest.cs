using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessQueueForTest : ProcessQueue
	{
		public ProcessQueueForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ProcessQueueLookups GetNewLookups()
		{
			return new ProcessQueueLookupsForTest(this);
		}
	}
}
