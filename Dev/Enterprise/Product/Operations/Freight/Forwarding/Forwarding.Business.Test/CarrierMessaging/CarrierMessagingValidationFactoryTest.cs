using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CarrierMessagingValidationFactoryTest : TestCaseWithFactory
	{
		public void TestGetValidation()
		{
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			var validation = CarrierMessagingValidationFactory.GetValidation(consol);
			Assert("validation for road consol", validation is NotImplementedCarrierMessagingValidation);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			validation = CarrierMessagingValidationFactory.GetValidation(consol);
			Assert("validation for sea consol", validation is NotImplementedCarrierMessagingValidation);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			validation = CarrierMessagingValidationFactory.GetValidation(consol);
			Assert("validation for air consol", validation is NotImplementedCarrierMessagingValidation);

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			validation = CarrierMessagingValidationFactory.GetValidation(consol);
			Assert("validation for rail consol", validation is NotImplementedCarrierMessagingValidation);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_RL_NKDischargePort = "AUSYD";

				validation = CarrierMessagingValidationFactory.GetValidation(consol);
				Assert("validation for road consol in United States", validation is ForwardAirCarrierMessagingValidation);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "USCHI";

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport.JW_RL_NKLoadPort = "USCHI";
				transport.JW_RL_NKDiscPort = "AUSYD";

				validation = CarrierMessagingValidationFactory.GetValidation(consol);
				Assert("validation for air consol in United States", validation is ForwardAirCarrierMessagingValidation);
			}
		}
	}
}
