using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(NZCConcessionController))]
	sealed class NZCConcessionControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get
			{
				return "NZ";
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.Concession;
		}
	}
}
