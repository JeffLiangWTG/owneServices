using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(ForwardingConsolToNZECIManifestSyncroniserController))]
	sealed class ForwardingConsolToNZECIManifestSyncroniserControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ForwardingConsolToNZECIManifestSyncroniserController);
			}
		}

		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.NewZealand;
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.ECIWriteOffManifestingConsolSynchroniser;
		}
	}
}
