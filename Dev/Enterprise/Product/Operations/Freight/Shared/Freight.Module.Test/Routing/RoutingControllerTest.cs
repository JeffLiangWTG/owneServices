using System;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(RoutingController))]
	sealed class RoutingControllerTest : ZControllerBasherTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Routing;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(CommonConsol);
		}

		#endregion
	}
}
