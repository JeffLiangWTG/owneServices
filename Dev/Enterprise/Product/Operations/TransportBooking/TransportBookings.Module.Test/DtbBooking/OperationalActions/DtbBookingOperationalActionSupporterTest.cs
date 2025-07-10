using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingOperationalActionSupporter))]
	public class DtbBookingOperationalActionSupporterTest : OperationalActionSupporterTest<DtbBookingOperationalActionSupporter>
	{
		OperationalActionSupporter GetDtbBookingOperationalActionSupporter()
		{
			using (ZFilterGridModule module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				return ((IOperationalActionSupportable)module).OperationalActionSupporter;
			}
		}

		public void TestPopulateMethods()
		{
			var supporter = GetDtbBookingOperationalActionSupporter();
			var allIds = supporter.Methods.GetAllIds();
			AssertCollectionContains(ActionMethodProviderIDs.Accounting, allIds);
		}

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBooking; }
		}
	}
}
