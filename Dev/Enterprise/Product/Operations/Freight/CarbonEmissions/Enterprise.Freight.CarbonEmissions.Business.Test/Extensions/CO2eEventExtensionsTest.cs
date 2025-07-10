using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	public class CO2eEventExtensionsTest : TestCaseWithFactory
	{
		public void TestLogGHGEvent_Updated()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogGHGEvent(CO2eEventType.Updated, "10000");
			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(nameof(CO2eEventType.Updated), log.Parameters[Params.Type]);
			AssertEquals("10000", log.Parameters[Params.New]);
		}

		public void TestLogGHGEvent_Rejected()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogGHGEvent(CO2eEventType.Rejected, "Invalid input data");

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(nameof(CO2eEventType.Rejected), log.Parameters[Params.Type]);
			AssertEquals("Invalid input data", log.Parameters[Params.Reason]);
		}

		public void TestLogGHGEvent_Unauthorized()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogGHGEvent(CO2eEventType.Unauthorized, "Access denied");

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(nameof(CO2eEventType.Unauthorized), log.Parameters[Params.Type]);
			AssertEquals("Access denied", log.Parameters[Params.Reason]);
		}

		public void TestLogGHGEvent_NotRequired()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogGHGEvent(CO2eEventType.NotRequired, "CO2e value is current");

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(nameof(CO2eEventType.NotRequired), log.Parameters[Params.Type]);
			AssertEquals("CO2e value is current", log.Parameters[Params.Reason]);
		}

		public void TestLogGHGEvent_ServiceUnavailable()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogGHGEvent(CO2eEventType.ServiceUnavailable, "Issue communicating with server. Please try again later.");

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(nameof(CO2eEventType.ServiceUnavailable), log.Parameters[Params.Type]);
			AssertEquals("Issue communicating with server. Please try again later.", log.Parameters[Params.Reason]);
		}

		public void TestLogGHGEvent_WithoutExtra()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogGHGEvent(CO2eEventType.Requested);

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(nameof(CO2eEventType.Requested), log.Parameters[Params.Type]);
		}

		public void TestLogSTUEvent_StatusChanged()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogSTUEvent("PEN", "CUR", new CO2eStatusChangedReason("Weight changed"));

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code));
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals("PEN", log.Parameters[Params.Old]);
			AssertEquals("CUR", log.Parameters[Params.New]);
			AssertEquals("CO2e Status", log.Parameters[Params.Type]);
			AssertEquals("Input value(s) have changed: Weight changed", log.Parameters[Params.Reason]);
		}

		public void TestLogSTUEvent_NoReason()
		{
			var provider = Factory.New<IForwardingShipment>() as ICO2eProvider;
			var logProvider = provider as IStmALogProvider;

			provider.LogSTUEvent("CUR", "PEN", CO2eStatusChangedReason.Empty);

			var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code));
			AssertEquals(0, logs.Length);
		}
	}
}
