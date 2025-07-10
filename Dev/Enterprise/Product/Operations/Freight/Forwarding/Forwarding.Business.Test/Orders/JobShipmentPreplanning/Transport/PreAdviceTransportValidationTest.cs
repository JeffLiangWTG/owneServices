using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class PreAdviceTransportValidationTest : BaseFreightTest
	{
		#region TestStorageLegsExemptFromPortConstraints

		public void TestStorageLegsExemptFromPortConstraints()
		{
			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			preadvice.EF_RL_NKPortLoad = HomePort;
			preadvice.EF_RL_NKPortDisch = OverseasPort;

			Transport transport2 = preadvice.PreAdviceTransports[0];
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_IsLinked = false;
			transport2.JW_LegOrder = 2;

			Transport transport1 = preadvice.PreAdviceTransports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Storage;
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKLoadPort = HomePort;

			Transport transport3 = preadvice.PreAdviceTransports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Storage;
			transport3.JW_RL_NKLoadPort = OverseasPort;
			transport3.JW_RL_NKDiscPort = OverseasPort;

			preadvice.PreAdviceTransports.RunPreSaveValidation();
			AssertEquals(2, transport1.Notifications.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(2, transport2.Notifications.GetWarnings().GetUniqueMessageList().Length);
			AssertEquals(2, transport3.Notifications.GetWarnings().GetUniqueMessageList().Length);
		}

		#endregion

		#region TestValidateLoadAgainstPreAdviceDischarge

		public void TestValidateLoadAgainstPreAdviceDischarge()
		{
			string errorMessage = "A routing leg cannot load at the pre-advice's final discharge.";

			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			Transport transport = preadvice.PreAdviceTransports.AddNew();

			preadvice.EF_RL_NKPortDisch = HomePort;
			transport.JW_RL_NKDiscPort = "";
			transport.JW_RL_NKLoadPort = HomePort;
			AssertHasError(transport.JW_RL_NKLoadPortInfo, errorMessage);

			transport.JW_RL_NKLoadPort = AlternateHomePort;
			AssertNoError(transport.JW_RL_NKLoadPortInfo, errorMessage);
		}

		#endregion

		#region TestValidateDischargeAgainstPreAdviceLoad

		public void TestValidateDischargeAgainstPreAdviceLoad()
		{
			string errorMessage = "A routing leg cannot discharge at the pre-advice's first load.";

			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			Transport transport = preadvice.PreAdviceTransports.AddNew();

			preadvice.EF_RL_NKPortLoad = HomePort;
			transport.JW_RL_NKLoadPort = "";
			transport.JW_RL_NKDiscPort = HomePort;
			AssertHasError(transport.JW_RL_NKDiscPortInfo, errorMessage);

			transport.JW_RL_NKDiscPort = AlternateHomePort;
			AssertNoError(transport.JW_RL_NKDiscPortInfo, errorMessage);
		}

		#endregion

		#region TestJW_LegOrderIsUnique

		public void TestJW_LegOrderIsUnique()
		{
			const string legOrderError = "This value must be unique on the pre-advice.";

			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			Transport transport1 = preadvice.PreAdviceTransports[0];
			Transport transport2 = preadvice.PreAdviceTransports.AddNew();

			AssertNoError(transport1.JW_LegOrderInfo, legOrderError);
			AssertNoError(transport2.JW_LegOrderInfo, legOrderError);

			transport2.JW_LegOrder = 1;
			transport1.Validation.ValidateJW_LegOrder();

			AssertHasError(transport1.JW_LegOrderInfo, legOrderError);
			AssertHasError(transport2.JW_LegOrderInfo, legOrderError);
		}

		#endregion

		#region TestValidateJW_ETD_TransportOrder

		public void TestValidateJW_ETD_TransportOrder()
		{
			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();

			Transport transport1 = preadvice.PreAdviceTransports.AddNew();
			transport1.JW_LegOrder = 1;

			Transport transport2 = preadvice.PreAdviceTransports.AddNew();
			transport2.JW_LegOrder = 2;

			AssertETDOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertETDOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertETDOrderValidated(transport2, transport1);
		}

		void AssertETDOrderValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = now;
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = now.AddDays(1);
			secondTransport.Validation.ValidateJW_ETD();
			AssertHasErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = now.AddDays(-1);
			secondTransport.Validation.ValidateJW_ETD();
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;
		}

		void AssertETDOrderNotValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			firstTransport.JW_ETA = now.AddDays(1);
			secondTransport.JW_ETD = now;
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;

			secondTransport.JW_ETA = now.AddDays(1);
			firstTransport.JW_ETD = now;
			AssertNoErrors(firstTransport.JW_ETDInfo);

			secondTransport.JW_ETA = ZDateTime.Empty;
			firstTransport.JW_ETD = ZDateTime.Empty;
		}

		#endregion

		#region TestValidateJW_ETD_TransportOrder

		public void TestValidateJW_ETA_TransportOrder()
		{
			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();

			Transport transport1 = preadvice.PreAdviceTransports.AddNew();
			transport1.JW_LegOrder = 1;

			Transport transport2 = preadvice.PreAdviceTransports.AddNew();
			transport2.JW_LegOrder = 2;

			AssertETAOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertETAOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertETAOrderValidated(transport2, transport1);
		}

		void AssertETAOrderValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			secondTransport.JW_ETD = ZDateTime.Empty;
			firstTransport.JW_ETA = now;
			AssertNoErrors(firstTransport.JW_ETAInfo);

			secondTransport.JW_ETD = now.AddDays(-1);
			firstTransport.Validation.ValidateJW_ETA();
			AssertHasErrors(firstTransport.JW_ETAInfo);

			secondTransport.JW_ETD = now.AddDays(1);
			firstTransport.Validation.ValidateJW_ETA();
			AssertNoErrors(firstTransport.JW_ETAInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;
		}

		void AssertETAOrderNotValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			firstTransport.JW_ETA = now.AddDays(1);
			secondTransport.JW_ETD = now;
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;

			secondTransport.JW_ETA = now.AddDays(1);
			firstTransport.JW_ETD = now;
			AssertNoErrors(firstTransport.JW_ETDInfo);

			secondTransport.JW_ETA = ZDateTime.Empty;
			firstTransport.JW_ETD = ZDateTime.Empty;
		}

		#endregion
	}
}
