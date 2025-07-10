using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RateTransportProviderController))]
	public class RateTransportProviderControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(RateTransportProvider);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RateTransportProvider;
		}
	}
}
