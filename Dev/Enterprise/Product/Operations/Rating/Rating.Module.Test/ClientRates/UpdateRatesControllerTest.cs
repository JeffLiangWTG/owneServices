using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(UpdateRatesController))]
	public class UpdateRatesControllerTest : ZSingletonControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(ClientRate);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ClientRateUpdates;
		}
	}
}
