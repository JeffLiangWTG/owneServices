using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	internal class ExternalWarehouseInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestGenerateMessageFromInterchange()
		{
			var expectedText = @"<ExternalWarehouseBatch>
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

			var messageText = @"<ExternalWarehouseBatch>
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

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "A";
			interchange.EI_To = "B";
			interchange.EI_ApplicationCode = "GMD";
			interchange.EI_InterchangeType = "EWH";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			CombineAssertions(() =>
			{
				new ExternalWarehouseInboundInterchangeProcessor().ExecuteBatch();
				interchange.Reload();

				AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

				var createdMessage = interchange.ContainedMessages[0];
				AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
				AssertEquals("EM_ApplicationCode", "ZAA", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "EWH", createdMessage.EM_MessageType);
				AssertEquals("EM_MessageText", (ZString)expectedText, createdMessage.EM_MessageText);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				AssertEquals("EM_MessageNum", false, createdMessage.EM_MessageNum.IsEmpty);
				AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			});
		}

		public void TestGenerateMessageFromInterchangeWithInvalidBatchTag()
		{
			var messageText = @"<IncorrectExternalWarehouseBatch>
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
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</IncorrectExternalWarehouseBatch>";
			RunAndAssertInvalidXmlText(messageText);
		}

		void RunAndAssertInvalidXmlText(string messageText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "A";
			interchange.EI_To = "B";
			interchange.EI_ApplicationCode = "GMD";
			interchange.EI_InterchangeType = "EWH";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			CombineAssertions(() =>
			{
				new ExternalWarehouseInboundInterchangeProcessor().ExecuteBatch();
				interchange.Reload();

				AssertEquals("Interchange status should be set Error", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertEquals("Should have been 0 messages extracted from interchange", 0, interchange.ContainedMessages.Count);
				AssertEquals("Log error", true, (interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport)?.ReferenceFreeText ?? ZString.Empty) == "Invalid xml - Cannot find ExternalWarehouseBatch node");
			});
		}

		public void TestGenerateMessageFromInterchangeWithInvalidXML()
		{
			var messageText = @"
				<ExternalWarehouseBatch
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
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "A";
			interchange.EI_To = "B";
			interchange.EI_ApplicationCode = "GMD";
			interchange.EI_InterchangeType = "EWH";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			CombineAssertions(() =>
			{
				new ExternalWarehouseInboundInterchangeProcessor().ExecuteBatch();
				interchange.Reload();

				AssertEquals("Interchange status should be set Error", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertEquals("Should have been 0 messages extracted from interchange", 0, interchange.ContainedMessages.Count);
				AssertEquals("Log error", true, (interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport)?.ReferenceFreeText ?? ZString.Empty) == "Invalid xml - cannot process");
			});
		}

		internal static LoggingInformation GetNewLoggerForTesting() => new LoggingInformationForTesting();
	}
}
