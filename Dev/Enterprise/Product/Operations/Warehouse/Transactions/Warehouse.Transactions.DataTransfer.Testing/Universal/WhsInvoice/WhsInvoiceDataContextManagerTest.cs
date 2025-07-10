using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsInvoiceDataContextManager))]
	class WhsInvoiceDataContextManagerTest : ShipmentDataContextManagerTestCase<WhsInvoiceDataContextManager, WhsInvoice>
	{
		public void TestManagesShipment_RegistryItemEnabled()
		{
			TestManagesShipment_RegistryItemCore(true);
		}

		public void TestManagesShipment_RegistryItemDisabled()
		{
			TestManagesShipment_RegistryItemCore(false);
		}

		void TestManagesShipment_RegistryItemCore(bool enabled)
		{
			using (WarehouseDataRegistry.Instance.EnableWhsPeriodicInvoiceXUT.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled))
			{
				AssertEquals($"ManagesShipments should be {enabled}", enabled, new WhsInvoiceDataContextManager().ManagesShipments);
			}
		}

		public void TestManagesEvents()
		{
			AssertEquals("ManagesEvents should be false", false, new WhsInvoiceDataContextManager().ManagesEvents);
		}

		public void TestDataContextType()
		{
			AssertEquals("DataContextType should be WarehousePeriodicInvoice", DataContextType.WarehousePeriodicInvoice, new WhsInvoiceDataContextManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_StorageJobNumber = "12345";

			var contextManager = new WhsInvoiceDataContextManager();
			((IDataContextManager)contextManager).Init(invoice);

			AssertEquals("DataContextKey should return StorageJobNumber.", "12345", contextManager.DataContextKey);
		}

		public void TestGetShipmentDataObjectReader()
		{
			var whsInvoiceDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var objectReader = ((IShipmentDataContextManagerInternal)new WhsInvoiceDataContextManager()).GetShipmentDataObjectReader(whsInvoiceDO, new DummyLogger(), Factory);
			AssertType<WhsInvoiceDataObjectReader>(objectReader);
		}

		public void TestGetShipmentDataObjectWriter()
		{
			var objectWriter = ((IShipmentDataContextManager)new WhsInvoiceDataContextManager()).GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			AssertType<WhsInvoiceDataObjectWriter>(objectWriter);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value", null, new WhsInvoiceDataContextManager().DefaultOutputDirectory);
		}

		public void TestGetEventContextValues()
		{
			AssertEquals(Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>(), new WhsInvoiceDataContextManager().EventContextValues);
		}

		public void TestGetEventParentFinder()
		{
			const string invoiceXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>StorageJobNumber</Type>
				<Value>12345</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_StorageJobNumber = "12345";

			Factory.SaveForTesting();

			var xmlEvent = new XmlEventDeserializer().Parse(invoiceXmlText);
			var dataContextManager = new WhsInvoiceDataContextManager();

			var logParents = ((IEventDataContextManager)dataContextManager).GetLogParentsForEvent(xmlEvent, Factory.BOFactory, new TestErrorLogger());
			AssertContainsExactElementsInAnyOrder(Array.Empty<BusinessObject>(), logParents);
		}

		public void TestGetDataContextKeyMatchingQuery()
		{
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_StorageJobNumber = "12345";

			var topLevelDO = new Mock<ITopLevelDataObject>();
			topLevelDO.Setup(t => t.DataContext).Returns(Mock.Of<IDataContextDataObject>());

			var dataSource = new Mock<IDataSourceDataObject>();
			dataSource.Setup(d => d.Key).Returns(invoice.ET_StorageJobNumber);

			var bizosFromDataSource = new WhsInvoiceDataContextManager().LoadBusinessObjectFromDataSource(topLevelDO.Object, dataSource.Object, Factory.BOFactory, Mock.Of<IXmlImportLogger>());
			AssertContainsExactElementsInAnyOrder(new[] { invoice }, bizosFromDataSource);
		}

		public void TestGetUniversalDataContextManager()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertType<WhsInvoiceDataContextManager>(invoice.GetUniversalDataContextManager());
		}

		#region Implementation

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();
		protected override string ValidPopulatedUniversalShipmentXML => "";

		#endregion
	}
}
