using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderLineDataContextManager))]
	class OrderLineDataContextManagerTest : ShipmentDataContextManagerTestCase<OrderLineDataContextManager, OrderLine>
	{
		public void TestExportOrderLine()
		{
			var orderLineBO = GetOrderLineForTest();

			#region Expected XML Message

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrderLine</Type>
          <Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <Order>
      <OrderLineCollection Content=""Partial"">
        <OrderLine>
          <AdditionalInformation></AdditionalInformation>
          <AdditionalTerms></AdditionalTerms>
          <CommercialInvoiceNumber></CommercialInvoiceNumber>
          <ConfirmationNumber></ConfirmationNumber>
          <ContainerNumber></ContainerNumber>
          <ContainerPackingOrder>0</ContainerPackingOrder>
          <CustomsData>
          </CustomsData>
          <EarlyShipmentLimitDays>0</EarlyShipmentLimitDays>
          <ExpectedQuantity>0</ExpectedQuantity>
          <ExtendedLinePrice>0</ExtendedLinePrice>
          <HarmonisedCode></HarmonisedCode>
          <IncoTerm>
            <Code></Code>
          </IncoTerm>
          <InnerPacksQty>0</InnerPacksQty>
          <InnerPacksQtyUnit>
            <Code></Code>
          </InnerPacksQtyUnit>
          <LateShipmentLimitDays>0</LateShipmentLimitDays>
          <LineNumber>1</LineNumber>
          <LineReference></LineReference>
          <LineSplitNumber>0</LineSplitNumber>
          <OrderedQty>0</OrderedQty>
          <OrderedQtyUnit>
            <Code>UNT</Code>
            <Description>Unit</Description>
          </OrderedQtyUnit>
          <OverQuantityPercentageLimit>0</OverQuantityPercentageLimit>
          <PackageHeight>0</PackageHeight>
          <PackageLength>0</PackageLength>
          <PackageLengthUnit>
            <Code></Code>
            <Description></Description>
          </PackageLengthUnit>
          <PackageQty>0</PackageQty>
          <PackageQtyUnit>
            <Code></Code>
          </PackageQtyUnit>
          <PackageWidth>0</PackageWidth>
          <PartAttribute1></PartAttribute1>
          <PartAttribute2></PartAttribute2>
          <PartAttribute3></PartAttribute3>
          <QtyBooked>0</QtyBooked>
          <QtyPacked>0</QtyPacked>
          <QuantityMet>0</QuantityMet>
          <RequiredExWorks></RequiredExWorks>
          <RequiredInStore></RequiredInStore>
          <SerialNumber></SerialNumber>
          <ShipmentWindowEnd></ShipmentWindowEnd>
          <ShipmentWindowStart></ShipmentWindowStart>
          <SpecialInstructions></SpecialInstructions>
          <Status>
            <Code>INC</Code>
            <Description>Incomplete</Description>
          </Status>
          <SubLineNumber>2</SubLineNumber>
          <SupplierConfirmedAcceptance></SupplierConfirmedAcceptance>
          <UnderQuantityPercentageLimit>0</UnderQuantityPercentageLimit>
          <UnitPriceRecommended>0</UnitPriceRecommended>
          <Volume>0</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0</Weight>
          <WeightUnit>
            <Code></Code>
          </WeightUnit>
        </OrderLine>
      </OrderLineCollection>
    </Order>
  </Shipment>
</UniversalShipment>";

			#endregion

			AssertMessageIsAsExpected(orderLineBO, expectedMessage);
		}

		public void TestExportCustomFields()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(true);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";
			var customLabel1 = buyer.CustomLabels.AddNew();
			customLabel1.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag2;
			customLabel1.OT_Caption = "Custom Flag 2";
			var customLabel2 = buyer.CustomLabels.AddNew();
			customLabel2.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDate5;
			customLabel2.OT_Caption = "Custom Date 5";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_CustomFlag2 = true;
			orderLineBO.JO_CustomDate5 = new ZDateTime(2021, 4, 13);

			orderLineBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			orderLineBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			orderLineBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			orderLineBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			orderLineBO.SetUserDefinedValue("The Date You Are Happy", new ZDateTime(2021, 4, 15));

			Factory.SaveForTesting();

			#region Expected XML Message

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrderLine</Type>
          <Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <Order>
      <OrderLineCollection Content=""Partial"">
        <OrderLine>
          <AdditionalInformation></AdditionalInformation>
          <AdditionalTerms></AdditionalTerms>
          <CommercialInvoiceNumber></CommercialInvoiceNumber>
          <ConfirmationNumber></ConfirmationNumber>
          <ContainerNumber></ContainerNumber>
          <ContainerPackingOrder>0</ContainerPackingOrder>
          <CustomsData>
          </CustomsData>
          <EarlyShipmentLimitDays>0</EarlyShipmentLimitDays>
          <ExpectedQuantity>0</ExpectedQuantity>
          <ExtendedLinePrice>0</ExtendedLinePrice>
          <HarmonisedCode></HarmonisedCode>
          <IncoTerm>
            <Code></Code>
          </IncoTerm>
          <InnerPacksQty>0</InnerPacksQty>
          <InnerPacksQtyUnit>
            <Code></Code>
          </InnerPacksQtyUnit>
          <LateShipmentLimitDays>0</LateShipmentLimitDays>
          <LineNumber>1</LineNumber>
          <LineReference></LineReference>
          <LineSplitNumber>0</LineSplitNumber>
          <OrderedQty>0</OrderedQty>
          <OrderedQtyUnit>
            <Code>UNT</Code>
            <Description>Unit</Description>
          </OrderedQtyUnit>
          <OverQuantityPercentageLimit>0</OverQuantityPercentageLimit>
          <PackageHeight>0</PackageHeight>
          <PackageLength>0</PackageLength>
          <PackageLengthUnit>
            <Code></Code>
            <Description></Description>
          </PackageLengthUnit>
          <PackageQty>0</PackageQty>
          <PackageQtyUnit>
            <Code></Code>
          </PackageQtyUnit>
          <PackageWidth>0</PackageWidth>
          <PartAttribute1></PartAttribute1>
          <PartAttribute2></PartAttribute2>
          <PartAttribute3></PartAttribute3>
          <QtyBooked>0</QtyBooked>
          <QtyPacked>0</QtyPacked>
          <QuantityMet>0</QuantityMet>
          <RequiredExWorks></RequiredExWorks>
          <RequiredInStore></RequiredInStore>
          <SerialNumber></SerialNumber>
          <ShipmentWindowEnd></ShipmentWindowEnd>
          <ShipmentWindowStart></ShipmentWindowStart>
          <SpecialInstructions></SpecialInstructions>
          <Status>
            <Code>INC</Code>
            <Description>Incomplete</Description>
          </Status>
          <SubLineNumber>2</SubLineNumber>
          <SupplierConfirmedAcceptance></SupplierConfirmedAcceptance>
          <UnderQuantityPercentageLimit>0</UnderQuantityPercentageLimit>
          <UnitPriceRecommended>0</UnitPriceRecommended>
          <Volume>0</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0</Weight>
          <WeightUnit>
            <Code></Code>
          </WeightUnit>

          <CustomizedFieldCollection>
            <CustomizedField>
              <DataType>Boolean</DataType>
              <Key>Custom Flag 2</Key>
              <Value>true</Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>DateTime</DataType>
              <Key>Custom Date 5</Key>
              <Value>2021-04-13T00:00:00</Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>Boolean</DataType>
              <Key>Are you Happy?</Key>
              <Value>true</Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>DateTime</DataType>
              <Key>The Date You Are Happy</Key>
              <Value>2021-04-15T00:00:00</Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>Decimal</DataType>
              <Key>The Happy Decimal</Key>
              <Value>7.7</Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>Integer</DataType>
              <Key>The Happy Number</Key>
              <Value>42</Value>
            </CustomizedField>
            <CustomizedField>
              <DataType>String</DataType>
              <Key>What Makes You Happy?</Key>
              <Value>Lots Of Ice</Value>
            </CustomizedField>
          </CustomizedFieldCollection>
        </OrderLine>
      </OrderLineCollection>
    </Order>

    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>Boolean</DataType>
        <Key>Custom Flag 2</Key>
        <Value>true</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Custom Date 5</Key>
        <Value>2021-04-13T00:00:00</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Boolean</DataType>
        <Key>Are you Happy?</Key>
        <Value>true</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>The Date You Are Happy</Key>
        <Value>2021-04-15T00:00:00</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Decimal</DataType>
        <Key>The Happy Decimal</Key>
        <Value>7.7</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Integer</DataType>
        <Key>The Happy Number</Key>
        <Value>42</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>What Makes You Happy?</Key>
        <Value>Lots Of Ice</Value>
      </CustomizedField>
    </CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			AssertMessageIsAsExpected(orderLineBO, expectedMessage);
		}

		public void TestExportUniversalEventTrigger()
		{
			var factory = new BusinessObjectFactory();

			using (factory.AddDisposableService())
			{
				var order = factory.NewWithValidTestData<Order>();
				order.JD_TransportMode = Core.Constants.TransportModes.Sea;
				order.JD_BookingConfRef = "FUL423189120";
				order.JD_InvoiceNumber = "INVOICE2";
				order.JD_Waybill = "ONTHEHOUSE";
				order.JD_MasterWaybill = "IAMTHEMASTER";
				order.JD_OrderNumber = "P!~000001";
				order.JD_RL_NKGoodsAvailableAt = "AUMEL";
				order.JD_RL_NKGoodsDeliveredTo = "NZCHC";
				order.Buyer.OH_Code = "MY!~CODE";

				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_LineNo = 1;
				orderLine.JO_SubLineNo = 2;

				factory.Save();

				var trigger = orderLine.WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = orderLine.Logs.AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var actionWrapper = new ActionWrapper(action, orderLine, new Lazy<IStmALog>(() => logBO, false));
				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				processor.Process(logger);
				factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				var interchange = message.Interchange;
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "JobOrderLine", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", orderLine.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(ExpectedUniversalEventMessage.Trim(), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		#region TestExportOrderLineEscapeChars

		public void TestExportOrderLineEscapeChars_WithTilde_WhenEnableOrderLineReferenceMatchingIsOn()
		{
			var orderLineBO = GetOrderLineForTest();
			orderLineBO.JO_LineReference = "TILDE~TEST";

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMessageIsAsExpected(orderLineBO, string.Format(ExpectedMessageForEscapeChars, "OrderNumber~1~TESTBUYERORG~TILDE!~TEST", "TILDE~TEST"));
			}
		}

		public void TestExportOrderLineEscapeChars_WithTideAndExclaim_WhenEnableOrderLineReferenceMatchingIsOn()
		{
			var orderLineBO = GetOrderLineForTest();
			orderLineBO.JO_LineReference = "TILDE!~TEST";

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMessageIsAsExpected(orderLineBO, string.Format(ExpectedMessageForEscapeChars, "OrderNumber~1~TESTBUYERORG~TILDE!!!~TEST", "TILDE!~TEST"));
			}
		}

		#region Expected XML Message

		const string ExpectedMessageForEscapeChars = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrderLine</Type>
          <Key>{0}</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <Order>
      <OrderLineCollection Content=""Partial"">
        <OrderLine>
          <AdditionalInformation></AdditionalInformation>
          <AdditionalTerms></AdditionalTerms>
          <CommercialInvoiceNumber></CommercialInvoiceNumber>
          <ConfirmationNumber></ConfirmationNumber>
          <ContainerNumber></ContainerNumber>
          <ContainerPackingOrder>0</ContainerPackingOrder>
          <CustomsData>
          </CustomsData>
          <EarlyShipmentLimitDays>0</EarlyShipmentLimitDays>
          <ExpectedQuantity>0</ExpectedQuantity>
          <ExtendedLinePrice>0</ExtendedLinePrice>
          <HarmonisedCode></HarmonisedCode>
          <IncoTerm>
            <Code></Code>
          </IncoTerm>
          <InnerPacksQty>0</InnerPacksQty>
          <InnerPacksQtyUnit>
            <Code></Code>
          </InnerPacksQtyUnit>
          <LateShipmentLimitDays>0</LateShipmentLimitDays>
          <LineNumber>1</LineNumber>
          <LineReference>{1}</LineReference>
          <LineSplitNumber>0</LineSplitNumber>
          <OrderedQty>0</OrderedQty>
          <OrderedQtyUnit>
            <Code>UNT</Code>
            <Description>Unit</Description>
          </OrderedQtyUnit>
          <OverQuantityPercentageLimit>0</OverQuantityPercentageLimit>
          <PackageHeight>0</PackageHeight>
          <PackageLength>0</PackageLength>
          <PackageLengthUnit>
            <Code></Code>
            <Description></Description>
          </PackageLengthUnit>
          <PackageQty>0</PackageQty>
          <PackageQtyUnit>
            <Code></Code>
          </PackageQtyUnit>
          <PackageWidth>0</PackageWidth>
          <PartAttribute1></PartAttribute1>
          <PartAttribute2></PartAttribute2>
          <PartAttribute3></PartAttribute3>
          <QtyBooked>0</QtyBooked>
          <QtyPacked>0</QtyPacked>
          <QuantityMet>0</QuantityMet>
          <RequiredExWorks></RequiredExWorks>
          <RequiredInStore></RequiredInStore>
          <SerialNumber></SerialNumber>
          <ShipmentWindowEnd></ShipmentWindowEnd>
          <ShipmentWindowStart></ShipmentWindowStart>
          <SpecialInstructions></SpecialInstructions>
          <Status>
            <Code>INC</Code>
            <Description>Incomplete</Description>
          </Status>
          <SubLineNumber>2</SubLineNumber>
          <SupplierConfirmedAcceptance></SupplierConfirmedAcceptance>
          <UnderQuantityPercentageLimit>0</UnderQuantityPercentageLimit>
          <UnitPriceRecommended>0</UnitPriceRecommended>
          <Volume>0</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0</Weight>
          <WeightUnit>
            <Code></Code>
          </WeightUnit>
        </OrderLine>
      </OrderLineCollection>
    </Order>
  </Shipment>
</UniversalShipment>";

		#endregion

		#endregion

		#region ExpectedUniversalEventMessage

		const string ExpectedUniversalEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrderLine</Type>
          <Key>P!!!~000001~0~MY!!!~CODE~1~2</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>RCV</Code>
        <Description>Received</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Received Goods</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <EventTime>2010-12-25T00:00:00.000+10:00</EventTime>
    <EventType>RCV</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>OrderNumber</Type>
        <Value>P!~000001</Value>
      </Context>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>IAMTHEMASTER</Value>
      </Context>
      <Context>
        <Type>HBOLNumber</Type>
        <Value>ONTHEHOUSE</Value>
      </Context>
      <Context>
        <Type>HBOLOriginUNLOCO</Type>
        <Value>AUMEL</Value>
      </Context>
      <Context>
        <Type>HBOLDestinationUNLOCO</Type>
        <Value>NZCHC</Value>
      </Context>
      <Context>
        <Type>CommercialInvoiceNumber</Type>
        <Value>INVOICE2</Value>
      </Context>
      <Context>
        <Type>ShippersReference</Type>
        <Value>FUL423189120</Value>
      </Context>
      <Context>
        <Type>OrderLineNumber</Type>
        <Value>1</Value>
      </Context>
      <Context>
        <Type>OrderLineSubLineNumber</Type>
        <Value>2</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>";

		#endregion

		public void TestExportOrderLine_WithAttributes()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_PartAttrib1 = "Attr1";
			orderLineBO.JO_PartAttrib2 = "Attr2";
			orderLineBO.JO_PartAttrib3 = "Attr3";
			orderLineBO.JO_SerialNumber = "SerNum";

			Factory.SaveForTesting();

			#region Expected XML Message

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrderLine</Type>
          <Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <Order>
      <OrderLineCollection Content=""Partial"">
        <OrderLine>
          <AdditionalInformation></AdditionalInformation>
          <AdditionalTerms></AdditionalTerms>
          <CommercialInvoiceNumber></CommercialInvoiceNumber>
          <ConfirmationNumber></ConfirmationNumber>
          <ContainerNumber></ContainerNumber>
          <ContainerPackingOrder>0</ContainerPackingOrder>
          <CustomsData>
          </CustomsData>
          <EarlyShipmentLimitDays>0</EarlyShipmentLimitDays>
          <ExpectedQuantity>0</ExpectedQuantity>
          <ExtendedLinePrice>0</ExtendedLinePrice>
          <HarmonisedCode></HarmonisedCode>
          <IncoTerm>
            <Code></Code>
          </IncoTerm>
          <InnerPacksQty>0</InnerPacksQty>
          <InnerPacksQtyUnit>
            <Code></Code>
          </InnerPacksQtyUnit>
          <LateShipmentLimitDays>0</LateShipmentLimitDays>
          <LineNumber>1</LineNumber>
          <LineReference></LineReference>
          <LineSplitNumber>0</LineSplitNumber>
          <OrderedQty>0</OrderedQty>
          <OrderedQtyUnit>
            <Code>UNT</Code>
            <Description>Unit</Description>
          </OrderedQtyUnit>
          <OverQuantityPercentageLimit>0</OverQuantityPercentageLimit>
          <PackageHeight>0</PackageHeight>
          <PackageLength>0</PackageLength>
          <PackageLengthUnit>
            <Code></Code>
            <Description></Description>
          </PackageLengthUnit>
          <PackageQty>0</PackageQty>
          <PackageQtyUnit>
            <Code></Code>
          </PackageQtyUnit>
          <PackageWidth>0</PackageWidth>
          <PartAttribute1>Attr1</PartAttribute1>
          <PartAttribute2>Attr2</PartAttribute2>
          <PartAttribute3>Attr3</PartAttribute3>
          <QtyBooked>0</QtyBooked>
          <QtyPacked>0</QtyPacked>
          <QuantityMet>0</QuantityMet>
          <RequiredExWorks></RequiredExWorks>
          <RequiredInStore></RequiredInStore>
          <SerialNumber>SerNum</SerialNumber>
          <ShipmentWindowEnd></ShipmentWindowEnd>
          <ShipmentWindowStart></ShipmentWindowStart>
          <SpecialInstructions></SpecialInstructions>
          <Status>
            <Code>INC</Code>
            <Description>Incomplete</Description>
          </Status>
          <SubLineNumber>2</SubLineNumber>
          <SupplierConfirmedAcceptance></SupplierConfirmedAcceptance>
          <UnderQuantityPercentageLimit>0</UnderQuantityPercentageLimit>
          <UnitPriceRecommended>0</UnitPriceRecommended>
          <Volume>0</Volume>
          <VolumeUnit>
            <Code></Code>
          </VolumeUnit>
          <Weight>0</Weight>
          <WeightUnit>
            <Code></Code>
          </WeightUnit>
        </OrderLine>
      </OrderLineCollection>
    </Order>
  </Shipment>
</UniversalShipment>";

			#endregion

			AssertMessageIsAsExpected(orderLineBO, expectedMessage);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override bool ManagerChecksDataTargetToImport => true;

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;

			Factory.SaveForTesting();
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrderLine</Type>
          <Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorised</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDate>2011-03-27T11:13:00</TriggerDate>
      <TriggerDescription>Test Trigger</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <OrderLine>
      <AdditionalInformation></AdditionalInformation>
      <AdditionalTerms></AdditionalTerms>
      <CommercialInvoiceNumber></CommercialInvoiceNumber>
      <ConfirmationNumber></ConfirmationNumber>
      <ContainerNumber></ContainerNumber>
      <ContainerPackingOrder>0</ContainerPackingOrder>
      <CustomsData>
      </CustomsData>
      <ExpectedQuantity>0</ExpectedQuantity>
      <ExtendedLinePrice>0</ExtendedLinePrice>
      <IncoTerm>
        <Code></Code>
      </IncoTerm>
      <InnerPacksQty>0</InnerPacksQty>
      <InnerPacksQtyUnit>
        <Code></Code>
      </InnerPacksQtyUnit>
      <LineNumber>1</LineNumber>
      <LineSplitNumber>0</LineSplitNumber>
      <OrderedQty>0</OrderedQty>
      <OrderedQtyUnit>
        <Code>UNT</Code>
        <Description>Unit</Description>
      </OrderedQtyUnit>
      <PackageHeight>0</PackageHeight>
      <PackageLength>0</PackageLength>
      <PackageLengthUnit>
        <Code></Code>
        <Description></Description>
      </PackageLengthUnit>
      <PackageQty>0</PackageQty>
      <PackageQtyUnit>
        <Code></Code>
      </PackageQtyUnit>
      <PackageWidth>0</PackageWidth>
      <QtyPacked>0</QtyPacked>
      <PartAttribute1></PartAttribute1>
      <PartAttribute2></PartAttribute2>
      <PartAttribute3></PartAttribute3>
      <QuantityMet>0</QuantityMet>
      <RequiredExWorks></RequiredExWorks>
      <RequiredInStore></RequiredInStore>
      <SpecialInstructions></SpecialInstructions>
      <Status>
        <Code>PLC</Code>
        <Description>Order Placed / finalized</Description>
      </Status>
      <SubLineNumber>2</SubLineNumber>
      <SupplierConfirmedAcceptance></SupplierConfirmedAcceptance>
      <UnitPriceRecommended>0</UnitPriceRecommended>
      <Volume>0</Volume>
      <VolumeUnit>
        <Code></Code>
      </VolumeUnit>
      <Weight>0</Weight>
      <WeightUnit>
        <Code></Code>
      </WeightUnit>
    </OrderLine>
  </Shipment>
</UniversalShipment>";
			}
		}

		#region Import OrderLine XML

		public void TestImportOrderLineXML_KeyIsMatched()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			#region expectedMessage

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineComment>BOOK</LineComment>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>
";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(expectedMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Populating OrderLine...
Updated Order Line from UniversalShipment.
Successfully saved Order Line.
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var orderLine2 = factory2.Load<OrderLine>(orderLineBO.PK);
				AssertEquals("BOOK", orderLine2.JO_Description);
			});
		}

		public void TestImportOrderLineXML_KeyIsMatchedByLineReference()
		{
			var orderLineBO = GetOrderLineForTest();
			orderLineBO.JO_LineReference = "ORL001";
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region expectedMessage

				const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>OrderNumber~1~TESTBUYERORG~ORL001</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineReference>ORL001</LineReference>
				<LineComment>BOOK</LineComment>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>
";

				#endregion

				var message = GetQueuedUniversalShipmentMessage(expectedMessage);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Populating OrderLine...
Updated Order Line from UniversalShipment.
Successfully saved Order Line.
".Trim(), message.GetLogNoteText());

					var factory2 = new BusinessObjectFactory();
					var orderLine2 = factory2.Load<OrderLine>(orderLineBO.PK);
					AssertEquals("BOOK", orderLine2.JO_Description);
				});
			}
		}

		public void TestImportOrderLineXML_UpdateCustomFields()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";
			var customLabel1 = buyer.CustomLabels.AddNew();
			customLabel1.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag2;
			customLabel1.OT_Caption = "Custom Flag 2";
			var customLabel2 = buyer.CustomLabels.AddNew();
			customLabel2.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDate5;
			customLabel2.OT_Caption = "Custom Date 5";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_Description = "CAR";
			orderLineBO.JO_CustomFlag2 = true;
			orderLineBO.JO_CustomDate5 = new ZDateTime(2021, 4, 13);

			orderLineBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			orderLineBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			orderLineBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			orderLineBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			orderLineBO.SetUserDefinedValue("The Date You Are Happy", new ZDateTime(2021, 4, 15));

			Factory.SaveForTesting();

			#region Message

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineComment>BOOK</LineComment>

				<CustomizedFieldCollection>
					<CustomizedField>
					  <DataType>Boolean</DataType>
					  <Key>Custom Flag 2</Key>
					  <Value>true</Value>
					</CustomizedField>
					<CustomizedField>
					  <DataType>DateTime</DataType>
					  <Key>Custom Date 5</Key>
					  <Value>2021-04-20T00:00:00</Value>
					</CustomizedField>
					<CustomizedField>
					  <DataType>Boolean</DataType>
					  <Key>Are you Happy?</Key>
					  <Value>true</Value>
					</CustomizedField>
					<CustomizedField>
					  <DataType>DateTime</DataType>
					  <Key>The Date You Are Happy</Key>
					  <Value>2021-04-15T00:00:00</Value>
					</CustomizedField>
					<CustomizedField>
					  <DataType>Decimal</DataType>
					  <Key>The Happy Decimal</Key>
					  <Value>8.8</Value>
					</CustomizedField>
					<CustomizedField>
					  <DataType>Integer</DataType>
					  <Key>The Happy Number</Key>
					  <Value>42</Value>
					</CustomizedField>
					<CustomizedField>
					  <DataType>String</DataType>
					  <Key>What Makes You Happy?</Key>
					  <Value>Lots Of Books</Value>
					</CustomizedField>
				</CustomizedFieldCollection>
			</OrderLine>
		</OrderLineCollection>
	</Order>

    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>Boolean</DataType>
        <Key>Custom Flag 2</Key>
        <Value>true</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Custom Date 5</Key>
        <Value>2021-04-17T00:00:00</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Boolean</DataType>
        <Key>Are you Happy?</Key>
        <Value>true</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>The Date You Are Happy</Key>
        <Value>2021-04-15T00:00:00</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Decimal</DataType>
        <Key>The Happy Decimal</Key>
        <Value>9.9</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Integer</DataType>
        <Key>The Happy Number</Key>
        <Value>42</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>What Makes You Happy?</Key>
        <Value>Lots Of Apples</Value>
      </CustomizedField>
    </CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>
";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(expectedMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Populating OrderLine...
Updated Order Line from UniversalShipment.
Successfully saved Order Line.
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var orderLine2 = factory2.Load<OrderLine>(orderLineBO.PK);

				AssertEquals("BOOK", orderLine2.JO_Description);
				AssertEquals("Only orderline's custom fields will be applied.", new ZDateTime(2021, 4, 20), orderLine2.JO_CustomDate5);
				AssertEquals(8.8m, orderLine2.GetUserDefinedValue<ZDecimal>("The Happy Decimal"));
				AssertEquals("Lots Of Books", orderLine2.GetUserDefinedValue<ZString>("What Makes You Happy?"));
			});
		}

		public void TestImportOrderLineXML_KeyIsNoMatched()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 3;
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			#region expectedMessage

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>OrderNumber~1~TESTBUYERORG~1~2</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineComment>BOOK</LineComment>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>
";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(expectedMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Import Log", @"
Error - Match couldn't be found for OrderManagerOrderLine with Key OrderNumber~1~TESTBUYERORG~1~2
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestImportOrderLineXML_MatchOrderLine_WhenKeyIsNotAvailable()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			#region expectedMessage

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineComment>BOOK</LineComment>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>
";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(expectedMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Populating OrderLine...
Updated Order Line from UniversalShipment.
Successfully saved Order Line.
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var orderLine2 = factory2.Load<OrderLine>(orderLineBO.PK);
				AssertEquals("BOOK", orderLine2.JO_Description);
			});
		}

		public void TestImportOrderLineXML_DoNotCreateNewOrderLine()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 5;
			orderLineBO.JO_Description = "CAR";

			Factory.SaveForTesting();

			#region expectedMessage

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineComment>BOOK</LineComment>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>
";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(expectedMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);

				AssertMultilineASCIIEquals("Import Log", @"
Error - Cannot populate OrderLine because:
Cannot find a matched Order Line.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestImportOrderLineXML_KeyHasDifferentValue()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ORGASYD";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "ORD03";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 4;
			orderLineBO.JO_SubLineNo = 1;
			orderLineBO.JO_Description = "CAR";
			orderLineBO.JO_LineReference = "RF001";

			Factory.SaveForTesting();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				#region expectedMessage

				const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>ORD03~1~ORGASYD~4~1</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>2</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineReference>RF001</LineReference>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>";

				#endregion

				var message = GetQueuedUniversalShipmentMessage(expectedMessage);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);

					AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Error - Cannot populate OrderLine because:
Line (2-2) in the Order Line are different from key ORD03~1~ORGASYD~4~1 in the data context.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
				});
			}

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region expectedMessage

				const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>ORD03~1~ORGASYD~RF001</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>2</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineReference>RF002</LineReference>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>";

				#endregion

				var message = GetQueuedUniversalShipmentMessage(expectedMessage);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);

					AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Error - Cannot populate OrderLine because:
Line reference RF002 in the Order Line is different from key ORD03~1~ORGASYD~RF001 in the data context.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestImportOrderLineXML_ShouldMatchSameOrderLine()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ORGASYD";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "ORD03";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLine1BO = orderBO.OrderLines.AddNew();
			orderLine1BO.JO_LineNo = 4;
			orderLine1BO.JO_SubLineNo = 1;
			orderLine1BO.JO_Description = "CAR";
			orderLine1BO.JO_LineReference = "RF001";

			var orderLine2BO = orderBO.OrderLines.AddNew();
			orderLine2BO.JO_LineNo = 5;
			orderLine2BO.JO_SubLineNo = 1;
			orderLine2BO.JO_Description = "CAR";
			orderLine2BO.JO_LineReference = "RF002";

			Factory.SaveForTesting();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region expectedMessage

				const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>ORD03~1~ORGASYD~4~1</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>4</LineNumber>
				<SubLineNumber>1</SubLineNumber>
				<LineReference>RF003</LineReference>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>";

				#endregion

				var message = GetQueuedUniversalShipmentMessage(expectedMessage);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Populating OrderLine...
Updated Order Line from UniversalShipment.
Successfully saved Order Line.
".Trim(), message.GetLogNoteText());

					orderBO.Reload();
					orderBO.OrderLines.RefreshFromDb();
					AssertEquals(2, orderBO.OrderLines.Count);
					AssertEquals("RF003", orderBO.OrderLines.Single(line => line.JO_LineNo == 4).JO_LineReference);
				});
			}

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region expectedMessage

				const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>ORD03~1~ORGASYD~4~1</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>4</LineNumber>
				<SubLineNumber>1</SubLineNumber>
				<LineReference>RF002</LineReference>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>";

				#endregion

				var message = GetQueuedUniversalShipmentMessage(expectedMessage);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);

					AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Error - Cannot populate OrderLine because:
Matching order line's (order line 4 and sub-line 1) Line Reference cannot be updated to RF002 because RF002 already exists on another order line.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());

					orderBO.Reload();
					orderBO.OrderLines.RefreshFromDb();
					AssertEquals(2, orderBO.OrderLines.Count);
					AssertEquals("RF003", orderBO.OrderLines.Single(line => line.JO_LineNo == 4).JO_LineReference);
				});
			}
		}

		public void TestImportOrderLineXML_KeyIsMatchedByLineReferenceWithTilde_GivenRegistryEnabled()
		{
			var orderLineBO = GetOrderLineForTest();
			orderLineBO.JO_LineReference = "TILDE~TEST";
			orderLineBO.JO_Description = "Tilde";

			Factory.SaveForTesting();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region expectedMessage

				const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>OrderManagerOrderLine</Type>
				<Key>OrderNumber~1~TESTBUYERORG~TILDE!~TEST</Key>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<Order>
		<OrderNumber>OrderNumber</OrderNumber>
		<OrderLineCollection Content=""Partial"">
			<OrderLine>
				<LineNumber>1</LineNumber>
				<SubLineNumber>2</SubLineNumber>
				<LineReference>TILDE~TEST</LineReference>
				<LineComment>Tilde</LineComment>
			</OrderLine>
		</OrderLineCollection>
	</Order>
  </Shipment>
</UniversalShipment>";

				#endregion

				var message = GetQueuedUniversalShipmentMessage(expectedMessage);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Import Log", @"
Successfully loaded matching OrderLine.
Populating OrderLine...
Updated Order Line from UniversalShipment.
Successfully saved Order Line.
".Trim(), message.GetLogNoteText());

					var factory2 = new BusinessObjectFactory();
					var orderLine2 = factory2.Load<OrderLine>(orderLineBO.PK);
					AssertEquals("Tilde", orderLine2.JO_Description);
				});
			}
		}

		#endregion

		#region Import Event

		public void TestImportUniversalEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P!~000001";
			order.Buyer.OH_Code = "MY!~CODE";
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			var nonMatchingOrder = Factory.NewWithValidTestData<Order>();
			nonMatchingOrder.JD_OrderNumber = "P!~000001";
			nonMatchingOrder.JD_BookingConfRef = "FUL423189120";
			var orderLine2 = nonMatchingOrder.OrderLines.AddNew();
			orderLine2.JO_LineNo = 1;
			orderLine2.JO_SubLineNo = 2;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("OrderLineUniversalEvent.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Order Line.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Order Line.
				".Trim(), message.GetLogNoteText());

				var logs = orderLine.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
MBOL Number - IAMTHEMASTER
HBOL Number - ONTHEHOUSE
Shippers Reference - FUL423189120
Order Line Number - 1
Order Line Sub Line Number - 2
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}
		public void TestImportUniversalEventWithContextInformationOnly()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P000002";
			order.JD_Waybill = "ONTHEHOUSE";
			order.JD_MasterWaybill = "IAMTHEMASTER";
			order.JD_BookingConfRef = "FUL423189120";
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("OrderLineUniversalEvent.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Order Line.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Order Line.
				".Trim(), message.GetLogNoteText());

				var logs = orderLine.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
MBOL Number - IAMTHEMASTER
HBOL Number - ONTHEHOUSE
Shippers Reference - FUL423189120
Order Line Number - 1
Order Line Sub Line Number - 2
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Order.TestFiles.{fileName}";
		}

		EmbeddedResourceRetriever ResourceRetriever => new EmbeddedResourceRetriever(GetType().Assembly);

		#endregion

		#region Data Context Key

		public void TestDataContextKey_ShouldBeLineReference_GivenRegistryEnabledAndNonEmpty()
		{
			var orderLine = GetOrderLineForTest();
			var manager = orderLine.GetUniversalDataContextManager() as IShipmentDataContextManager;

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNotEquals(orderLine.JO_LineReference, manager.DataContextKey);

				orderLine.JO_LineReference = "ORL001";

				AssertEquals("OrderNumber~1~TESTBUYERORG~ORL001", manager.DataContextKey);
			}
		}

		public void TestDataContextKey_ShouldUseOrderSplitCombination_GivenLineReferenceRegistryDisabled()
		{
			var orderLine = GetOrderLineForTest();
			var manager = orderLine.GetUniversalDataContextManager() as IShipmentDataContextManager;

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("OrderNumber~1~TESTBUYERORG~1~2", manager.DataContextKey);

				orderLine.JO_LineReference = "ORL001";

				AssertEquals("OrderNumber~1~TESTBUYERORG~1~2", manager.DataContextKey);
			}
		}

		public void TestDataContextKey_ShouldBeEmpty_GivenLineReferenceRegistryDisabledAndNoOrder()
		{
			var orderLine = Factory.New<OrderLine>();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;

			var manager = orderLine.GetUniversalDataContextManager() as IShipmentDataContextManager;

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(string.Empty, manager.DataContextKey);
			}
		}

		public void TestDataContextKey_ShouldBeLineReferenceWithTilde_GivenRegistryEnabled()
		{
			var orderLine = GetOrderLineForTest();
			var manager = orderLine.GetUniversalDataContextManager() as IShipmentDataContextManager;

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNotEquals(orderLine.JO_LineReference, manager.DataContextKey);

				orderLine.JO_LineReference = "ORL~001";

				AssertEquals("OrderNumber~1~TESTBUYERORG~ORL!~001", manager.DataContextKey);
			}
		}

		void AssertMessageIsAsExpected(OrderLine orderLineBO, string expectedMessage)
		{
			var manager = orderLineBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLineBO)));
			var universalShipment = writer.GetDataObject(orderLineBO);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(universalShipment, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Expected Universal Shipment Message", expectedMessage, result);
				}
			}
		}

		OrderLine GetOrderLineForTest()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 1;
			orderLineBO.JO_SubLineNo = 2;

			Factory.SaveForTesting();
			return orderLineBO;
		}

		#endregion
	}
}
