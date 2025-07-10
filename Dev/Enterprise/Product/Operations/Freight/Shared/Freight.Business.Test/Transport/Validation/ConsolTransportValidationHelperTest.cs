using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolTransportValidationHelperTest : BaseFreightTest
	{
		public void TestCheckLoadPort()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			consol.JK_RL_NKDischargePort = HomePort;
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_OA_DepartureLocation = Factory.New<OrgHeader>().MainAddress.PK;

			transport.Validation.ValidateJW_RL_NKLoadPort();
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "A routing leg cannot load at the consol's final discharge.");

			transport.JW_RL_NKLoadPort = AlternateHomePort;
			transport.Validation.ValidateJW_RL_NKLoadPort();
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
		}

		public void TestCheckLoadDiscPortsAreSame()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = HomePort;

			transport.Validation.ValidateJW_RL_NKLoadPort();
			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_TransportMode = Constants.TransportModes.Road;
			transport.Validation.ValidateJW_RL_NKLoadPort();
			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.Validation.ValidateJW_RL_NKLoadPort();
			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_TransportMode = Constants.TransportModes.Rail;
			transport.Validation.ValidateJW_RL_NKLoadPort();
			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			transport.Validation.ValidateJW_RL_NKLoadPort();
			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);
		}

		public void TestCheckDiscPort()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			consol.JK_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_OA_ArrivalLocation = Factory.New<OrgHeader>().MainAddress.PK;

			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "A routing leg cannot discharge at the consol's first load.");

			transport.JW_RL_NKDiscPort = AlternateHomePort;
			transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);
		}

		public void TestCheckETD()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;
			transport1.JW_RL_NKDiscPort = "USHNL";
			transport2.JW_RL_NKLoadPort = "AUSYD";

			var etd = ZDateTime.Now;
			transport1.JW_ETD = etd;
			transport1.JW_ETA = etd.AddDays(4);
			transport2.JW_ETD = etd.AddDays(4).AddHours(-6);
			transport2.JW_ETA = etd.AddDays(8);
			AssertNoErrors("ETD is less than a day before the previous transport's ETA", transport2.JW_ETDInfo);

			transport2.JW_ETD = etd.AddDays(2);
			AssertHasError("ETD is more than a day before the previous transport's ETA", transport2.JW_ETDInfo, legOutOfOrderMessage);
		}

		public void TestCheckETA()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;
			transport1.JW_RL_NKDiscPort = "USHNL";
			transport2.JW_RL_NKLoadPort = "AUSYD";

			transport2.JW_ETD = ZDateTime.Now.AddDays(10).AddHours(-6);
			transport2.JW_ETA = ZDateTime.Now.AddDays(20);
			transport1.JW_ETD = ZDateTime.Now;
			transport1.JW_ETA = ZDateTime.Now.AddDays(10);
			AssertNoErrors("ETA is less than a day after the next transport's ETD", transport1.JW_ETAInfo);

			transport1.JW_ETA = ZDateTime.Now.AddDays(12);
			AssertHasError("ETA is more than a day after the next transport's ETD", transport1.JW_ETAInfo, legOutOfOrderMessage);
		}

		public void TestOneDayToleranceForAirValidation_ETD()
		{
			var consol = Factory.New<CommonConsol>();
			var etd = ZDateTime.Now;

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "JPNRT";
			transport1.JW_RL_NKDiscPort = "JPHND";
			transport1.JW_ETD = etd;
			transport1.JW_ETA = etd.AddDays(1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "JPHND";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_ETD = etd.AddDays(1).AddHours(-6);
			transport2.JW_ETA = etd.AddDays(2);

			AssertHasError("ETD is less than a day before the previous transport's ETA", transport2.JW_ETDInfo, legOutOfOrderMessage);

			transport2.JW_RL_NKLoadPort = "USHNL";
			transport2.Validation.ValidateJW_ETD();
			AssertNoError("ETD is less than a day before the previous transport's ETA", transport2.JW_ETDInfo, legOutOfOrderMessage);
		}

		public void TestOneDayToleranceForAirValidation_ETA()
		{
			var consol = Factory.New<CommonConsol>();
			var etd = ZDateTime.Now;

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "JPNRT";
			transport1.JW_RL_NKDiscPort = "JPHND";
			transport1.JW_ETD = etd;
			transport1.JW_ETA = etd.AddDays(1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "JPHND";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_ETD = etd.AddDays(1).AddHours(-6);
			transport2.JW_ETA = etd.AddDays(2);

			AssertHasError("ETA is less than a day after the next transport's ETD", transport1.JW_ETAInfo, legOutOfOrderMessage);

			transport2.JW_RL_NKLoadPort = "USHNL";
			transport1.Validation.ValidateJW_ETA();
			AssertNoError("ETA is less than a day after the next transport's ETD", transport1.JW_ETAInfo, legOutOfOrderMessage);
		}

		#region TestCheckATD

		public void TestCheckATD()
		{
			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			AssertATDOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertATDOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertATDOrderValidated(transport2, transport1);
		}

		void AssertATDOrderValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.JW_ATA = now.AddDays(1);
			transport2.Validation.ValidateJW_ATD();
			AssertHasError(transport1.JW_ATAInfo, legOutOfOrderMessage);
			AssertHasError(transport2.JW_ATDInfo, legOutOfOrderMessage);

			transport1.JW_ATA = now.AddDays(-1);
			transport2.Validation.ValidateJW_ATD();
			AssertNoErrors(transport2.JW_ATDInfo);
			AssertNoErrors(transport1.JW_ATAInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;
		}

		void AssertATDOrderNotValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.JW_ATA = now.AddDays(1);
			transport2.JW_ATD = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;

			transport2.JW_ATA = now.AddDays(1);
			transport1.JW_ATD = now;
			AssertNoErrors(transport1.JW_ATDInfo);

			transport2.JW_ATA = ZDateTime.Empty;
			transport1.JW_ATD = ZDateTime.Empty;
		}

		#endregion

		#region TestCheckATA

		public void TestCheckATA()
		{
			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			AssertATAOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertATAOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertATAOrderValidated(transport2, transport1);
		}

		void AssertATAOrderValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport2.JW_ATD = ZDateTime.Empty;
			transport1.JW_ATA = now;
			AssertNoErrors(transport1.JW_ATAInfo);

			transport2.JW_ATD = now.AddDays(-1);
			transport1.Validation.ValidateJW_ATA();
			AssertHasError(transport1.JW_ATAInfo, legOutOfOrderMessage);

			transport2.JW_ATD = now.AddDays(1);
			transport1.Validation.ValidateJW_ATA();
			AssertNoErrors(transport1.JW_ATAInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;
		}

		void AssertATAOrderNotValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.JW_ATA = now.AddDays(1);
			transport2.JW_ATD = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;

			transport2.JW_ATA = now.AddDays(1);
			transport1.JW_ATD = now;
			AssertNoErrors(transport1.JW_ATDInfo);

			transport2.JW_ATA = ZDateTime.Empty;
			transport1.JW_ATD = ZDateTime.Empty;
		}

		#endregion

		const string legOutOfOrderMessage = "The date order does not reflect the leg order.";
	}
}
