using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingTmplController))]
	public class DtbBookingTmplControllerTest : ZPopupControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DtbBookingTmpl;
		}
	}
}
