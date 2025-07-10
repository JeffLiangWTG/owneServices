using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderNumberDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBasicOrderItemLevelFieldMappings()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject = new OrderNumber { Sequence = 1, OrderReference = "REFERME" };
			var reader = new OrderNumberDataObjectReader(orderNumberDataObject, logger, Factory, shipmentBO.DocsAndCartage);
			var orderItemBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderItemBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(orderItemBO, new ZShort(1), "REFERME");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestOrderReferenceIsNull()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject = new OrderNumber { Sequence = 1 };
			var reader = new OrderNumberDataObjectReader(orderNumberDataObject, logger, Factory, shipmentBO.DocsAndCartage);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		public void TestOrderReferenceSequenceIsNull()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject = new OrderNumber { OrderReference = "REF" };
			var reader = new OrderNumberDataObjectReader(orderNumberDataObject, logger, Factory, shipmentBO.DocsAndCartage);
			var orderItemBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertContents(orderItemBO, new ZShort(0), "REF");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
".Trim(), logger.Logs);
			});
		}

		public static void AssertContents(OrderItem orderItemBO, ZShort sequence, ZString orderReference)
		{
			AssertEquals("orderItemBO.JT_Sequence", sequence, orderItemBO.JT_Sequence);
			AssertEquals("orderItemBO.JT_OrderReference", orderReference, orderItemBO.JT_OrderReference);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
