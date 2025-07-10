using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentExtensionsTest : TestCaseWithFactory
	{
		public void TestIsExportFrom()
		{
			CommonShipment shipment = null;
			Assert("Shipment: null", !shipment.IsExportFrom(Core.Constants.CountryCodes.UnitedArabEmirates));

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = ZString.Empty;
			shipment.JS_RL_NKDestination = ZString.Empty;
			Assert("Origin/Destination: Empty", !shipment.IsExportFrom(Core.Constants.CountryCodes.UnitedArabEmirates));

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			Assert("Origin/Destination: Not UAE", !shipment.IsExportFrom(Core.Constants.CountryCodes.UnitedArabEmirates));

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AEAAN";
			Assert("Destination: UAE", !shipment.IsExportFrom(Core.Constants.CountryCodes.UnitedArabEmirates));

			shipment.JS_RL_NKOrigin = "AEAAN";
			shipment.JS_RL_NKDestination = "CNSHA";
			Assert("Origin: UAE", shipment.IsExportFrom(Core.Constants.CountryCodes.UnitedArabEmirates));
		}
	}
}
