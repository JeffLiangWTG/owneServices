using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(NZSendTestCustomsMessageController))]
	sealed class NZSendTestCustomsMessageControllerTest : ZSingletonControllerBasherTest
	{
		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.NewZealand;
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.SendTestCustomsMessage;
		}
	}
}
