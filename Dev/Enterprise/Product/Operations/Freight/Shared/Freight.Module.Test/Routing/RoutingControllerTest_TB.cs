using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(RoutingController))]
	sealed class RoutingControllerTest_TB : ZControllerBasherTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Routing;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return (BusinessObject)Factory.New<IDtbBookingConsolidation>();
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(IDtbBookingConsolidation);
		}

		#endregion
	}
}
