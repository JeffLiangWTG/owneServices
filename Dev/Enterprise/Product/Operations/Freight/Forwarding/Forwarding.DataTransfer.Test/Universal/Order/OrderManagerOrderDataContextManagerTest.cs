using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderManagerOrderDataContextManager))]
	class OrderManagerOrderDataContextManagerTest : ShipmentDataContextManagerTestCase<OrderManagerOrderDataContextManager, Order>
	{
		public void TestImportOrderWithDataContextKeyMaxLength()
		{
			var orgAddress = new OrganisationDataObjectReader(OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			orgAddress.Header.OH_Code = "TESTORGBUYER";

			var order = Factory.New<Order>();
			order.BuyerPK = orgAddress.Header.PK;
			order.JD_OrderNumber = "111111111122222222223333333333XXXXX";
			order.JD_OrderNumberSplit = 1;

			Factory.SaveForTesting();

			#region Expected XML Message

			const string expectedMessage = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OrderManagerOrder</Type>
          <Key>111111111122222222223333333333XXXXX~1~TESTORGBUYER</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <GoodsDescription>APPLE</GoodsDescription>
    <WayBillNumber>WayBill000000</WayBillNumber>
    <Order>
      <OrderNumber>111111111122222222223333333333XXXXX</OrderNumber>
      <ClientReference>222</ClientReference>
      <IsReleased>false</IsReleased>
      <OrderNumberSplit>1</OrderNumberSplit>
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

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Order 111111111122222222223333333333XXXXX-1 from UniversalShipment.
Successfully saved Order 111111111122222222223333333333XXXXX-1.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Successfully loaded matching Order.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Updated Order 111111111122222222223333333333XXXXX-1 from UniversalShipment.", logNoteText);
			});
		}

		public void TestExportOrderWithDataContextKeyMaxLength()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber_MaxLength_35_Characters";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var consigneeDocumentary = Factory.NewWithValidTestData<OrgHeader>();
			consigneeDocumentary.Contacts.AddNew().FillWithValidTestData();
			consigneeDocumentary.OH_Code = "TESTCNSGEE";
			orderBO.ConsigneeDocumentaryAddress.OrganisationPK = consigneeDocumentary.PK;
			orderBO.ConsigneeDocumentaryAddress.ContactPK = consigneeDocumentary.Contacts[0].PK;
			orderBO.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentary.MainAddress.PK;

			Factory.SaveForTesting();

			#region Expected XML Message

			const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrder</Type>
          <Key>OrderNumber_MaxLength_35_Characters~1~TESTBUYERORG</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber></InvoiceNumber>
          <InvoiceDate></InvoiceDate>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <FirstBuyerContact></FirstBuyerContact>
    <FreightRate>1</FreightRate>
    <FreightRateCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </FreightRateCurrency>
    <GoodsDescription></GoodsDescription>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <SecondBuyerContact></SecondBuyerContact>
    <ServiceLevel>
      <Code></Code>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
    <LocalProcessing>
      <DeliveryRequiredBy></DeliveryRequiredBy>
    </LocalProcessing>
    <Order>
      <OrderNumber>OrderNumber_MaxLength_35_Characters</OrderNumber>
      <ClientReference></ClientReference>
      <IsReleased>false</IsReleased>
      <OrderNumberSplit>1</OrderNumberSplit>
      <Status>
        <Code>INC</Code>
        <Description>Incomplete</Description>
      </Status>
    </Order>
    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber></BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
      </AdditionalBill>
    </AdditionalBillCollection>
    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DepartureVesselCutoffDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ExWorksRequiredBy</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FollowUp</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>OrderDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2025-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>ShipmentWindowStart</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShipmentWindowEnd</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
    <MilestoneCollection>
      <Milestone>
        <Description>Order Confirmed</Description>
        <EventCode>OCF</EventCode>
        <Sequence>1</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Ex Factory</Description>
        <EventCode>EXW</EventCode>
        <Sequence>2</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Origin Receival at Wharf / Depot</Description>
        <EventCode>GIN</EventCode>
        <Sequence>3</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>FAC=CTO</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Departure</Description>
        <EventCode>DEP</EventCode>
        <Sequence>4</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>LOC=&lt;FirstLeg.Origin&gt;</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Arrival</Description>
        <EventCode>ARV</EventCode>
        <Sequence>5</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>LOC=&lt;LastLeg.Destination&gt;</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Clearance Commenced</Description>
        <EventCode>CCC</EventCode>
        <Sequence>6</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Clearance Finalized</Description>
        <EventCode>CLR</EventCode>
        <Sequence>7</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Unpacked</Description>
        <EventCode>CAV</EventCode>
        <Sequence>8</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Estimated Pickup</Description>
        <EventCode>DCA</EventCode>
        <Sequence>9</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Order Delivered</Description>
        <EventCode>DCF</EventCode>
        <Sequence>10</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>GoodsDeliveredTo</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTBUYERORG</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Contact>L7NQXKXWDLK7JW0JNX4EJI0RX65RERYW7ENW1T7BMIHYKAKPQ005KVLCG420NGRUWX8FJDQ2Y21ABCHWVPANZ1MM1KP60GXGZVBWAFGNJ6GR2LBEXFE6FD43KWN7WTCSEVJOATVQJWE2V4SQJLIUBZAW0HLN1IZY4XMHBVDDSXXR7CIPE6XLLQQNVWW2HIYA6P66SKOWHEBNCRXY75I3UNNNG7VN7K5K8WUSEK62RAEGODKD873L83PTBTZTYAY6</Contact>
        <Email>BLBLA0NOIXP57W5L8P610U6DP6IF3A3R6OWGLNJQ5TEGQKQGI813QVAPO75CSIUH367S8Y8VFP4HPZWFPB53I8ALNH1MQH7X86MSL6GS8FYAZ28XK0VQLZ835WE16WE7RHISND2S3M4CKAUTHDBZUQLI7RVY2OVMBZRSUQKG6DW745T5CTT2UNNB48U4REZO0RBTRC80142Q2NNB7ZWI411WMPWV4PY2NHHM48N8U1BZU5HRVS50RQT7ZNHBPN</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTCNSGEE</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTBUYERORG</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BuyerDocumentaryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTBUYERORG</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			AssertMessageIsAsExpected(orderBO, expectedMessage);
		}

		#region TestExportOrderWithDataContextKeyEscapeChars

		public void TestExportOrder_WithDataContextKeyTilde()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber~Tilde";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			Factory.SaveForTesting();

			AssertMessageIsAsExpected(orderBO, string.Format(expectedMessage, "OrderNumber!~Tilde~1~TESTBUYERORG", "OrderNumber~Tilde"));
		}

		public void TestExportOrder_WithDataContextKeyTildeAndExclaim()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTBUYERORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "OrderNumber!~Tilde";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			Factory.SaveForTesting();

			AssertMessageIsAsExpected(orderBO, string.Format(expectedMessage, "OrderNumber!!!~Tilde~1~TESTBUYERORG", "OrderNumber!~Tilde"));
		}

		#region Expected XML Message

		const string expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrder</Type>
          <Key>{0}</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber></InvoiceNumber>
          <InvoiceDate></InvoiceDate>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <FirstBuyerContact></FirstBuyerContact>
    <FreightRate>1</FreightRate>
    <FreightRateCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </FreightRateCurrency>
    <GoodsDescription></GoodsDescription>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <SecondBuyerContact></SecondBuyerContact>
    <ServiceLevel>
      <Code></Code>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
    <LocalProcessing>
      <DeliveryRequiredBy></DeliveryRequiredBy>
    </LocalProcessing>
    <Order>
      <OrderNumber>{1}</OrderNumber>
      <ClientReference></ClientReference>
      <IsReleased>false</IsReleased>
      <OrderNumberSplit>1</OrderNumberSplit>
      <Status>
        <Code>INC</Code>
        <Description>Incomplete</Description>
      </Status>
    </Order>
    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber></BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
      </AdditionalBill>
    </AdditionalBillCollection>
    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DepartureVesselCutoffDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ExWorksRequiredBy</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FollowUp</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>OrderDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2025-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>ShipmentWindowStart</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShipmentWindowEnd</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
    <MilestoneCollection>
      <Milestone>
        <Description>Order Confirmed</Description>
        <EventCode>OCF</EventCode>
        <Sequence>1</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Ex Factory</Description>
        <EventCode>EXW</EventCode>
        <Sequence>2</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Origin Receival at Wharf / Depot</Description>
        <EventCode>GIN</EventCode>
        <Sequence>3</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>FAC=CTO</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Departure</Description>
        <EventCode>DEP</EventCode>
        <Sequence>4</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>LOC=&lt;FirstLeg.Origin&gt;</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Arrival</Description>
        <EventCode>ARV</EventCode>
        <Sequence>5</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>LOC=&lt;LastLeg.Destination&gt;</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Clearance Commenced</Description>
        <EventCode>CCC</EventCode>
        <Sequence>6</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Clearance Finalized</Description>
        <EventCode>CLR</EventCode>
        <Sequence>7</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Unpacked</Description>
        <EventCode>CAV</EventCode>
        <Sequence>8</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Estimated Pickup</Description>
        <EventCode>DCA</EventCode>
        <Sequence>9</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Order Delivered</Description>
        <EventCode>DCF</EventCode>
        <Sequence>10</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>GoodsDeliveredTo</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTBUYERORG</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTBUYERORG</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>TESTBUYERORG</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#endregion

		public void TestImportOrderThroughJobNumber()
		{
			var orderToLoad = Factory.New<Order>();
			var orgAddress = new OrganisationDataObjectReader(OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			orderToLoad.BuyerPK = orgAddress.Header.PK;
			orderToLoad.JD_OrderNumber = "P00001";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithDataTarget.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Order P00001 from UniversalShipment.
Successfully saved Order P00001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Successfully loaded matching Order.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Updated Order P00001 from UniversalShipment.", logNoteText);

				var order = new BusinessObjectFactory().LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_Waybill, "FRED235478923"));
				AssertEquals(orderToLoad.PK, order.PK);
			});
		}

		public void TestShouldNotMatchCancelledOrderThroughJobNumberAndImportAsNewOrder()
		{
			var orderToLoad = Factory.New<Order>();
			var orgAddress = new OrganisationDataObjectReader(OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			orderToLoad.BuyerPK = orgAddress.Header.PK;
			orderToLoad.JD_OrderNumber = "P00001";
			orderToLoad.JD_IsCancelled = true;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithDataTarget.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Match couldn't be found for OrderManagerOrder with Key P00001~0~CRAHOLSYD
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Message Discarded", logNoteText);
				AssertContains("message.GetLogNoteText()", "Error - Match couldn't be found for OrderManagerOrder with Key P00001~0~CRAHOLSYD", logNoteText);
			});
		}

		public void TestCanImportOrderViaUniversalDataBuss()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Order FRED235478923 from UniversalShipment.
Successfully saved Order FRED235478923.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "No matching Order found, creating new Order.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Added Order FRED235478923 from UniversalShipment.", logNoteText);

				var order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, "FRED235478923"));
				AssertNotNull("Forwarding Order should exist with a Order Number of [FRED235478923].", order);
			});
		}

		public void TestMalformedJobNumberDoesNotCauseErrorOrLoadAnExistingBusinessObject()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P000001";
			order.Buyer.OH_Code = "MYCODE";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithMalformedJobNumber.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
				".Trim(), message.GetLogNoteText());

				var logs = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 0, logs.Length);
			});
		}

		public void TestExportUniversalEventTriggerViaEHub()
		{
			var factory = new BusinessObjectFactory();

			using (factory.AddDisposableService())
			{
				var order = factory.NewWithValidTestData<Order>();
				order.JD_TransportMode = Constants.TransportModes.Sea;
				order.JD_BookingConfRef = "FUL423189120";
				order.JD_InvoiceNumber = "INVOICE2";
				order.JD_Waybill = "ONTHEHOUSE";
				order.JD_MasterWaybill = "IAMTHEMASTER";
				order.JD_OrderNumber = "P!~000001";
				order.JD_RL_NKGoodsAvailableAt = "AUMEL";
				order.JD_RL_NKGoodsDeliveredTo = "NZCHC";
				order.Buyer.OH_Code = "MY!~CODE";

				factory.Save();

				var trigger = order.WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = order.Logs.AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var actionWrapper = new ActionWrapper(action, order, new Lazy<IStmALog>(() => logBO, false));
				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				processor.Process(logger);
				factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
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
					AssertEquals("message.EM_LinkTable", "JobOrderHeader", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", order.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(ExpectedUniversalEventMessage.Trim(), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		#region ExpectedUniversalEventMessage

		const string ExpectedUniversalEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OrderManagerOrder</Type>
          <Key>P!!!~000001~0~MY!!!~CODE</Key>
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
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>";

		#endregion

		public void TestImportUniversalEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P!~000001";
			order.Buyer.OH_Code = "MY!~CODE";

			var nonMatchingOrder = Factory.NewWithValidTestData<Order>();
			nonMatchingOrder.JD_OrderNumber = "P!~000001";
			nonMatchingOrder.JD_BookingConfRef = "FUL423189120";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Order P!~000001.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Order P!~000001.
				".Trim(), message.GetLogNoteText());

				var logs = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
MBOL Number - IAMTHEMASTER
HBOL Number - ONTHEHOUSE
Shippers Reference - FUL423189120
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		public void TestImportUniversalEventWithDataTargetUpdateAllOrderSplits()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P!~000001";
			order.Buyer.OH_Code = "^OO";
			order.JD_OrderNumberSplit = 0;
			order.JD_OrderStatus = Constants.OrderStatus.Open;
			order.JD_IsCancelled = false;

			Order orderSplit = order.SplitOrder(CreateOrderType.Split);
			orderSplit.JD_OrderStatus = Constants.OrderStatus.Open;
			orderSplit.JD_IsCancelled = false;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventToUpdateAllOrderSplits.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Order P!~000001.
Linked Event to Order P!~000001-1.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Matching 'Owner':- Matched to '^OO' by code, main address used.
Linked Event to Order P!~000001.
Field [JobOrderHeader.JD_IsCancelled] has been updated to value [true] on Order P!~000001.
Field [JobOrderHeader.JD_OrderStatus] has been updated to value [DLV] on Order P!~000001.
Linked Event to Order P!~000001-1.
Field [JobOrderHeader.JD_IsCancelled] has been updated to value [true] on Order P!~000001-1.
Field [JobOrderHeader.JD_OrderStatus] has been updated to value [DLV] on Order P!~000001-1.
				".Trim(), message.GetLogNoteText());

				AssertOrderLog(order);
				AssertOrderLog(orderSplit);
			});
		}

		void AssertOrderLog(Order order)
		{
			var logs = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
			AssertEquals(string.Format("[BKD] - Booked event count for Order({0})", order.JD_OrderNumberAndSplit), 1, logs.Length);
			var log = logs[0];

			var contextItems = log.SourceInfoItems;
			var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
			AssertMultilineASCIIEquals("Context Items on Event", @"
MBOL Number - IAMTHEMASTER
HBOL Number - ONTHEHOUSE
Shippers Reference - FUL423189120
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);

			AssertEquals(Constants.OrderStatus.Delivered, order.JD_OrderStatus);
			AssertEquals(true, order.JD_IsCancelled);
		}

		public void TestImportUniversalEventWithContextInformationOnly()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P000002";
			order.JD_Waybill = "ONTHEHOUSE";
			order.JD_MasterWaybill = "IAMTHEMASTER";
			order.JD_BookingConfRef = "FUL423189120";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Order P000002.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Order P000002.
				".Trim(), message.GetLogNoteText());

				var logs = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
MBOL Number - IAMTHEMASTER
HBOL Number - ONTHEHOUSE
Shippers Reference - FUL423189120
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		EmbeddedResourceRetriever ResourceRetriever => new EmbeddedResourceRetriever(GetType().Assembly);

		public void TestContextInformationIsAllThereForSea()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P000001";
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_MasterWaybill = "IAMTHEMASTER";
			order.JD_Waybill = "ONTHEHOUSE";
			order.JD_BookingConfRef = "FUL423189120";
			order.JD_InvoiceNumber = "INVOICE2";
			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "NZCHC";

			var manager = (IEventDataContextManager)order.GetUniversalDataContextManager();

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
OrderNumber - P000001
MBOLNumber - IAMTHEMASTER
HBOLNumber - ONTHEHOUSE
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - NZCHC
CommercialInvoiceNumber - INVOICE2
ShippersReference - FUL423189120
			".Trim(), eventContextValues);
		}

		public void TestContextInformationIsAllThereForAir()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "P000001";
			order.JD_TransportMode = Constants.TransportModes.Air;
			order.JD_MasterWaybill = "IAMTHEMASTER";
			order.JD_Waybill = "ONTHEHOUSE";
			order.JD_BookingConfRef = "FUL423189120";
			order.JD_InvoiceNumber = "INVOICE2";
			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "NZCHC";

			var manager = (IEventDataContextManager)order.GetUniversalDataContextManager();

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
OrderNumber - P000001
MAWBNumber - IAMTHEMASTER
HAWBNumber - ONTHEHOUSE
HAWBOriginIATAAirportCode - MEL
HAWBDestinationIATAAirportCode - CHC
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - NZCHC
CommercialInvoiceNumber - INVOICE2
ShippersReference - FUL423189120
			".Trim(), eventContextValues);
		}

		#region Implementation

		string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Order.TestFiles.{fileName}";
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return Array.Empty<RecipientRoleType>(); }
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			new OrganisationDataObjectReader(OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
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
          <Type>OrderManagerOrder</Type>
          <Key>P00001003</Key>
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

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>CHOCOLATE EGGS</GoodsDescription>
    <GoodsValue>49.9900</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australia, Dollars</Description>
    </GoodsValueCurrency>
    <OuterPacks>1</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PortOfDestination>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>DEFRA</Code>
      <Name>Frankfurt am Main</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>OBR</Code>
      <Description>Original Bill Required at Destinati</Description>
    </ReleaseType>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <TotalNoOfPacks>12</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>2.89</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>440.00</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <OrganizationCode>CRAHOLSYD</OrganizationCode>
        <Address1>1804 Fudrucker Way</Address1>
        <CompanyName>CRACKERJACK HOLDINGS</CompanyName>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
			}
		}

		void AssertMessageIsAsExpected(Order orderBO, string expectedMessage)
		{
			var manager = orderBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var universalShipment = writer.GetDataObject(orderBO);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
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
		}

		#endregion
	}
}
