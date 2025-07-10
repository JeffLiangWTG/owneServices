using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(TransactionOrderRetrieverService))]
	sealed class TransactionOrderRetrieverServiceTest : GMDCustomsMessagingServiceTest<TransactionOrderRetrieverService>
	{
		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, TransactionOrderRetrieverService serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);
			CombineAssertions(() =>
			{
				AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
				AssertMessage(interchange.ContainedMessages[0], ZAEDIMessageTypeList.Codes.ExternalWarehouse, expectedText);
			});
		}

		void AssertMessage(EDIMessage message, ZString messageType, ZString bodyText)
		{
			AssertEquals("message.EM_ApplicationCode", EDIInterchange.ApplicationCodes.SouthAfricanTransactionOrders, message.EM_ApplicationCode);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals("message.EM_MessageText", bodyText, message.EM_MessageText);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		}

		protected override TransactionOrderRetrieverService CreateServiceTask() => new TransactionOrderRetrieverService();

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var interchange = CreateInterchange();
			return new GMDCustomsMessagingServiceTestHelperData()
			{
				InterchangePK = interchange.PK
			};
		}

		EDIInterchange CreateInterchange()
		{
			EDIInterchange interchange1;
			ExternalWarehouseInterchangeTestHelper.SetUpTwoRealInterchangesForTest(Factory, out interchange1, out _);
			return interchange1;
		}

		readonly ZString expectedText = @"<ExternalWarehouseBatch>
  <Batch>Batch1</Batch>
  <OrganizationAddress>
    <AddressType>Warehouse1</AddressType>
    <AddressShortCode>3 NEW RD</AddressShortCode>
    <OrganizationCode>FORDWHS</OrganizationCode>
  </OrganizationAddress>
  <ExternalWarehouseTransactionCollection>
    <ExternalWarehouseTransaction>
      <BatchLineno>1</BatchLineno>
      <TransactionType>ORD</TransactionType>
      <ExportType>EXP</ExportType>
      <TransactionDate>2022-11-23</TransactionDate>
      <OwnerReference>111088</OwnerReference>
      <ProductCode>AB3921971ABSMR3</ProductCode>
      <Owner>FORDORG</Owner>
      <Quantity>22</Quantity>
      <TotalValue>1800.2389</TotalValue>
      <Currency>ZAR</Currency>
      <CountryOrigin>ZA</CountryOrigin>
    </ExternalWarehouseTransaction>
  </ExternalWarehouseTransactionCollection>
</ExternalWarehouseBatch>";

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"ZA Transaction Orders interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.GenericMessageDelivery)
				};
			}
		}
	}
}
