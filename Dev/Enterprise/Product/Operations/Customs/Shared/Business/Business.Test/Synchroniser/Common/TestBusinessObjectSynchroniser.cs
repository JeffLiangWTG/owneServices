using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestBusinessObjectSynchroniser : BusinessObjectSynchroniser
	{
		public TestBusinessObjectSynchroniser(BusinessObject destination, BusinessObject source) : base(destination, source)
		{
		}

		public void ForceSynchroniseCoreExposed()
		{
			ForceSynchroniseCore();
		}

		public List<ISynchroniser> SynchronisersExposed => Synchronisers;

		protected override void HookSynchronisers()
		{
			hookSynchronisersCount++;
		}

		int hookSynchronisersCount;

		public bool IsHookSynchronisersCalled => hookSynchronisersCount > 0;

		public void HookEvents_Exposed() => base.HookEvents();
	}
}
