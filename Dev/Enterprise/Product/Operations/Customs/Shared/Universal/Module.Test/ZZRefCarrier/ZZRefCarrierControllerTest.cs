using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCarrierController))]
	public class ZZRefCarrierControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Universal.ZZRefCarrier;
		}

		public override Type ControllerToBashType => typeof(ZZRefCarrierController);
	}
}
