using System;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(RefCusTariffController))]
	sealed class RefCusTariffControllerTest : Universal.Module.Testing.RefCusTariffControllerTest
	{
		public override Type ControllerToBashType => typeof(RefCusTariffController);

		protected override ControllerID GetControllerID()
		{
			return ZAControllerIDs.RefCusTariff;
		}
	}
}
