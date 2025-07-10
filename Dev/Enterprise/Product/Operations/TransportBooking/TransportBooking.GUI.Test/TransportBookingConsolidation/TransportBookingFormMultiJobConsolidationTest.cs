using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Testing
{
	[TestedType(typeof(TransportBookingMultiForm))]
	public class TransportBookingFormMultiJobConsolidationTest : TransportBookingMultiFormTest
	{
		protected override DtbBookingConsolidation GetNewConsolidation()
		{
			return Helper.CreateConsolidationMultiJob();
		}

		protected override bool NewActionEnabled
		{
			get { return true; }
		}

		protected override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBookingConsolidation; }
		}
	}
}
