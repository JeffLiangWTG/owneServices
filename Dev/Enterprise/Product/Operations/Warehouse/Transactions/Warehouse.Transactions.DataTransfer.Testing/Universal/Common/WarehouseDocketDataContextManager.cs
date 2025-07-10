using System;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public abstract class WarehouseDocketDataContextManagerTestCase<TDataContextManager, TDocket> : ShipmentDataContextManagerTestCase<TDataContextManager, TDocket>
		where TDataContextManager : WarehouseDocketDataContextManager<TDocket>, new()
		where TDocket : WhsDocket
	{
		#region TestGetUniversalDataContextManager

		public void TestGetUniversalDataContextManager()
		{
			var docket = Factory.New<TDocket>();
			AssertType<TDataContextManager>(docket.GetUniversalDataContextManager());
		}

		#endregion

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals(nameof(WarehouseDocketDataContextManager<TDocket>.DataContextType), ExpectedDataContextType, new TDataContextManager().DataContextType);
		}

		protected abstract DataContextType ExpectedDataContextType { get; }

		#endregion

		#region TestDataContextKey

		public void TestDataContextKey()
		{
			const string DocketId = "DOCKETID";

			var docket = Factory.New<TDocket>();
			docket.WD_DocketID = DocketId;

			var docketContextManager = new TDataContextManager();
			((IDataContextManager)docketContextManager).Init(docket);

			AssertEquals(nameof(WarehouseDocketDataContextManager<TDocket>.DataContextKey), DocketId, docketContextManager.DataContextKey);
		}

		#endregion

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals(nameof(WarehouseDocketDataContextManager<TDocket>.ManagesShipments), true, new TDataContextManager().ManagesShipments);
		}

		#endregion

		#region TestManagesEvents

		public void TestManagesEvents()
		{
			AssertEquals(nameof(WarehouseDocketDataContextManager<TDocket>.ManagesEvents), true, new TDataContextManager().ManagesEvents);
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			const string TestDirectory = "C:\\Test";

			using (SystemDataRegistry.Instance.WarehouseExportDirectory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestDirectory))
			{
				AssertEquals(nameof(WarehouseDocketDataContextManager<TDocket>.DefaultOutputDirectory), TestDirectory, new TDataContextManager().DefaultOutputDirectory);
			}
		}

		#endregion

		#region TestGetDataContextKeyMatchingQuery

		public void TestGetDataContextKeyMatchingQuery()
		{
			const string DodgyDocketType = "LOL";
			const string DocketId = "DOCKETID";

			var docket1 = Factory.New<TDocket>();
			docket1.WD_DocketID = DocketId;
			AssertNotEquals("Precondition.", DodgyDocketType, docket1.WD_DocketType);

			var docket2 = Factory.New<TDocket>();

			var docketDodgy = Factory.New<TDocket>();
			docketDodgy.WD_DocketID = DocketId;
			docketDodgy.WD_DocketType = DodgyDocketType;

			var logger = Mock.Of<IXmlImportLogger>();
			var topLevelDO = new Mock<ITopLevelDataObject>();
			topLevelDO.Setup(t => t.DataContext).Returns(Mock.Of<IDataContextDataObject>());

			var dataSource = new Mock<IDataSourceDataObject>();
			dataSource.Setup(d => d.Key).Returns(DocketId);

			var bizosFromDataSource = new TDataContextManager().LoadBusinessObjectFromDataSource(topLevelDO.Object, dataSource.Object, Factory.BOFactory, logger);
			AssertContainsExactElementsInAnyOrder(new[] { docket1 }, bizosFromDataSource);
		}

		#endregion

		#region TestGetShipmentDataObjectReader

		public void TestGetShipmentDataObjectReader()
		{
			var dataContextManager = new TDataContextManager();

			var reader = ((IShipmentDataContextManagerInternal)dataContextManager).GetShipmentDataObjectReader(new Shipment(), Mock.Of<IXmlImportLogger>(), Factory);
			AssertType(ExpectedDataObjectReaderType, reader);
		}

		protected abstract Type ExpectedDataObjectReaderType { get; }

		#endregion

		#region TestGetShipmentDataObjectWriter

		public void TestGetShipmentDataObjectWriter()
		{
			var dataContextManager = new TDataContextManager();

			var reader = ((IShipmentDataContextManager)dataContextManager).GetShipmentDataObjectWriter(Mock.Of<IDataWritingManager>());
			AssertType(ExpectedDataObjectWriterType, reader);
		}

		protected abstract Type ExpectedDataObjectWriterType { get; }

		#endregion
	}
}
