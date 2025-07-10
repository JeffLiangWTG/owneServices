using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConsolChargesLocatorTest : TestCaseWithFactory
	{
		public void TestNZConsolCustomsCharges()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			IServiceLocator locator = new ConsolChargesLocator(consol);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.NZ.IConsolCustomsCharges>(), locator.GetService(typeof(ICustomsCharges)).GetType());
			AssertNull(locator.GetService(typeof(string)));
			consol.JK_RL_NKLoadPort = "USLAX";
			AssertNull(locator.GetService(typeof(ICustomsCharges)));
		}
	}
}
