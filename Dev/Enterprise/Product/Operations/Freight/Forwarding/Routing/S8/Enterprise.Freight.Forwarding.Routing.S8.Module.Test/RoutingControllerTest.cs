using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Module.Test
{
	[TestedType(typeof(RealTimeRoutingController))]
	public class RoutingControllerTest : ZSingletonControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(RealTimeRoutingController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RoutingLookups;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new RoutingResponseHeader("", Factory);
		}
	}
}
