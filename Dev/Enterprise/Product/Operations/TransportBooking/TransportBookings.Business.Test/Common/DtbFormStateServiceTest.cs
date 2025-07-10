using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportBookings.Business.Test.Common
{
	class DtbFormStateServiceTest : TestCaseWithFactory
	{
		public void TestDtbFormStateService()
		{
			AssertEquals(DtbFormState.Parent, DtbFormStateService.GetState(Factory));

			DtbFormStateService.SetState(Factory, DtbFormState.Booking);
			AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(Factory));

			DtbFormStateService.SetState(new BusinessObjectFactory(), DtbFormState.Parent);
			AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(Factory));
		}
	}
}
