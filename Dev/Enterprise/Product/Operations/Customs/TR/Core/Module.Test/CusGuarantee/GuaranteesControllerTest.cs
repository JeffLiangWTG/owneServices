using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(GuaranteesController))]
	public class GuaranteesControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Guarantees;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Turkey;
	}
}
