using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.Common
{
	class TransitUniversalExtensionsTest : TransitUniversalTestCase
	{
		#region TestIsConsolidated

		public void TestIsConsolidated()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C1000000");

			var runsheet = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			runsheet.DataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "R1000000");

			var runsheetInstruction = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			runsheetInstruction.DataContext.AddDataSource(DataContextType.TransportConsignmentRunSheetInstruction, "R1000000PickFromA");

			var airManifest = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			airManifest.DataContext.AddDataSource(DataContextType.AirManifest, "O10000000");

			var underBond = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			underBond.DataContext.AddDataSource(DataContextType.UnderBond, "U10000000");

			var gateBooking = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			gateBooking.DataContext.AddDataSource(DataContextType.GateBooking, "GTB000000");

			var nonSupported = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			nonSupported.DataContext.AddDataSource(DataContextType.LocalTransport, "L1000000");

			AssertEquals(true, consol.DataContext.DataSourceCollection.IsSupportedJob());
			AssertEquals(true, runsheet.DataContext.DataSourceCollection.IsSupportedJob());
			AssertEquals(true, runsheetInstruction.DataContext.DataSourceCollection.IsSupportedJob());
			AssertEquals(true, airManifest.DataContext.DataSourceCollection.IsSupportedJob());
			AssertEquals(true, underBond.DataContext.DataSourceCollection.IsSupportedJob());
			AssertEquals(true, gateBooking.DataContext.DataSourceCollection.IsSupportedJob());
			AssertEquals(false, nonSupported.DataContext.DataSourceCollection.IsSupportedJob());
		}

		#endregion

		#region IsConsolDataTarget

		public void TestIsConsolDataTarget()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C1000000");

			var runsheet = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			runsheet.DataContext.AddDataTarget(DataContextType.TransportConsignmentRunSheet, "R1000000");

			var runsheetInstruction = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			runsheetInstruction.DataContext.AddDataTarget(DataContextType.TransportConsignmentRunSheetInstruction, "R1000000PickFromA");

			var seaCargoOutturn = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			seaCargoOutturn.DataContext.AddDataTarget(DataContextType.SeaCargoOutturn, "S1000000");

			var transitReceiveConsol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			transitReceiveConsol.DataContext.AddDataTarget(DataContextType.TransitReceiveConsol, "S1000000");

			var airManifest = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			airManifest.DataContext.AddDataTarget(DataContextType.AirManifest, "O10000000");

			var nonConolidation = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			nonConolidation.DataContext.AddDataTarget(DataContextType.LocalTransport, "L1000000");

			AssertEquals(true, consol.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(true, runsheet.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(true, runsheetInstruction.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(true, seaCargoOutturn.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(true, transitReceiveConsol.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(true, airManifest.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(false, nonConolidation.DataContext.DataTargetCollection.IsConsolDataTarget());
			AssertEquals(false, TransitUniversalExtensions.IsConsolDataTarget(null));
		}

		#endregion

		#region TestGetConsolDataObject_InvalidUniversalType

		public void TestGetConsolDataObject_InvalidUniversalType()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			Logger.TopLevelDataObject = transaction;
			AssertExceptionThrown("Should throw an error when wrong transaction type.", typeof(DataObjectReadFailureException), @"Transit Warehouse does not support this transaction type.", () => TransitUniversalExtensions.GetConsolDataObject(Logger, new UniversalObjectFactory()));
		}

		#endregion
	}
}
