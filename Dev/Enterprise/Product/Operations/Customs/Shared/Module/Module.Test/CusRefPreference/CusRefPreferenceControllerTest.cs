using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefPreferenceController))]
	sealed class CusRefPreferenceControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(CusRefPreferenceController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CusRefPreference;
	}
}
