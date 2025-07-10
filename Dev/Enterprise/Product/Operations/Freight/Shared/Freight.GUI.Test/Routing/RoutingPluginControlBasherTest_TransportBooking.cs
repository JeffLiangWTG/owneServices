using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(RoutingPluginControlTestForm_TransportBooking))]
	sealed class RoutingPluginControlBasherTest_TransportBooking : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var transportParentCore = (ITransportParentCommon)bookingConsolidation;
			return new RoutingPluginControlTestForm_TransportBooking(new TransportCollection(transportParentCore));
		}

		#endregion
	}
}
