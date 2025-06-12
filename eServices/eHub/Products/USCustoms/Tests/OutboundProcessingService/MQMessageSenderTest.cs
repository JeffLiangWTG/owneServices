using System;
using System.IO;
using System.Reflection;
using System.Threading;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using CargoWise.eServices.USCustoms.OutboundProcessingService;
using Common.Logging.Simple;
using IBM.WMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class MQMessageSenderTest
	{
		[TestMethod]
		public void TestReplaceEscapedCharacters()
		{
			var messageSender = new MQMessageSender(new NoOpLogger(), new Mock<IMQQueueManagerProvider>().Object, CancellationToken.None);
			var AMAMessageContent =
@"
\x02FRI
JFKXYZ
999-12345675-M
WBL/FRA/T1/K10/TOYS
ARR/XYZ123/25OCT
SHP/TOTLERTOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYSRWE
/8812 FUN STREET
/NEWYORK/NY
/US/12345/123-456-7890
\x03";
			var expectedContent = @"
FRI
JFKXYZ
999-12345675-M
WBL/FRA/T1/K10/TOYS
ARR/XYZ123/25OCT
SHP/TOTLERTOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYSRWE
/8812 FUN STREET
/NEWYORK/NY
/US/12345/123-456-7890
";
			var sourceStream = new MemoryStream(MQMessageSender.OutgoingEncoding.GetBytes(AMAMessageContent));
			var outputStream = messageSender.ReplaceEscapedCharacters(sourceStream);
			Assert.AreEqual(AMAMessageContent, new StreamReader(sourceStream).ReadToEnd(), "Source Stream should be available and Position = 0.");
			Assert.AreEqual(expectedContent, new StreamReader(outputStream).ReadToEnd());
		}

		[TestMethod]
		public void TestGetUEMTransportModeFromXML()
		{
			var UEMMessageContent =
				@"
<CBPManifestMessage xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://manifest.cbp.dhs.gov/shared/model"">
 <Version />
  <Filing>
    <SenderId>
      <Value>8CAR</Value>
    </SenderId>
    <ReceiverId>
      <Value>CUSTOMS</Value>
    </ReceiverId>
    <MessageDateTime>
      <Value>20230901 170000</Value>
    </MessageDateTime>
    <MessageType>
      <Value>2</Value>
    </MessageType>
    <MessageControlNumber>
      <Value>MANPHL3W0001109</Value>
    </MessageControlNumber>
    <MessageReferenceNumber>
      <Value>1</Value>
    </MessageReferenceNumber>
    <ResponseMessage>
      <ResponseCode />
      <SeverityIndicator />
      <NarrativeText />
    </ResponseMessage>
  </Filing>
  <Conveyance>
    <CarrierCode>
      <Value>APLU</Value>
    </CarrierCode>
    <ConveyanceId>
      <Value>1234567</Value>
    </ConveyanceId>
    <ConveyanceName>
      <Value>TITANIC</Value>
    </ConveyanceName>
    <FlightTripVoyageNumber>
      <Value>1W</Value>
    </FlightTripVoyageNumber>
    <ConveyanceCountryCode>
      <Value>US</Value>
    </ConveyanceCountryCode>
    <ModeOfTransportationCode>
      <Value>11</Value>
    </ModeOfTransportationCode>
    <ScheduledArrivalDate>
      <Value />
    </ScheduledArrivalDate>
    <ArrivalPortCode>
      <Value />
    </ArrivalPortCode>
    <ScheduledDepartureDate>
      <Value>20230915</Value>
    </ScheduledDepartureDate>
    <DeparturePortCode>
      <Value/>1101</DeparturePortCode>
    <BOLInfoList>
      <BOLIssuerCode>
        <Value>APLU</Value>
      </BOLIssuerCode>
      <BOLNumber>
        <Value>123456789012</Value>
      </BOLNumber>
      <BOLTypeCode>
        <Value>00</Value>
      </BOLTypeCode>
      <BOLClassificationCode>
        <Value>BOL</Value>
      </BOLClassificationCode>
      <BOLActionType>
        <Value>A</Value>
      </BOLActionType>
      <BOLAmendmentReasonCode>
        <Value />
      </BOLAmendmentReasonCode>
      <SplitShipmentIndicator>
        <Value />
      </SplitShipmentIndicator>
      <Quantity>
        <Value>0</Value>
      </Quantity>
      <QuantityUnitOfMeasure>
        <Value />
      </QuantityUnitOfMeasure>
      <BoardedQuantity>
        <Value>0</Value>
      </BoardedQuantity>
      <Weight>
        <Value>0</Value>
      </Weight>
      <WeightUnitOfMeasure>
        <Value />
      </WeightUnitOfMeasure>
      <BoardedWeight>
        <Value>0</Value>
      </BoardedWeight>
      <Volume>
        <Value>0</Value>
      </Volume>
      <VolumeUnitOfMeasure>
        <Value />
      </VolumeUnitOfMeasure>
      <ModeOfTransportationCode>
        <Value>11</Value>
      </ModeOfTransportationCode>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>U</Value>
        </LocationTypeCode>
        <Location>
          <Value>GBLON</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>A</Value>
        </LocationTypeCode>
        <Location>
          <Value>GBFXT</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>D</Value>
        </LocationTypeCode>
        <Location>
          <Value>USNYC</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLReferenceInfoList>
        <ReferenceTypeCode>
          <Value>CSK</Value>
        </ReferenceTypeCode>
        <ReferenceData>
          <Value>41352</Value>
        </ReferenceData>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLReferenceInfoList>
      <BOLReferenceInfoList>
        <ReferenceType>
          <Value>ITN</Value>
        </ReferenceType>
        <ReferenceData>
          <Value>X20230809652244</Value>
        </ReferenceData>
      </BOLReferenceInfoList>
      <ResponseMessage>
        <ResponseCode />
        <SeverityIndicator />
        <NarrativeText />
      </ResponseMessage>
      <Action>
        <ActionCode>A</ActionCode>
        <NarrativeText />
      </Action>
    </BOLInfoList>
    <BOLInfoList>
      <BOLIssuerCode>
        <Value>OTT1</Value>
      </BOLIssuerCode>
      <BOLNumber>
        <Value>PMP03132310</Value>
      </BOLNumber>
      <BOLTypeCode>
        <Value>00</Value>
      </BOLTypeCode>
      <BOLClassificationCode>
        <Value>STD</Value>
      </BOLClassificationCode>
      <BOLActionType>
        <Value>A</Value>
      </BOLActionType>
      <BOLAmendmentReasonCode>
        <Value />
      </BOLAmendmentReasonCode>
      <SplitShipmentIndicator>
        <Value>Y</Value>
      </SplitShipmentIndicator>
      <Quantity>
        <Value>1</Value>
      </Quantity>
      <QuantityUnitOfMeasure>
        <Value>NO</Value>
      </QuantityUnitOfMeasure>
      <BoardedQuantity>
        <Value>0</Value>
      </BoardedQuantity>
      <Weight>
        <Value>1000</Value>
      </Weight>
      <WeightUnitOfMeasure>
        <Value>KG</Value>
      </WeightUnitOfMeasure>
      <BoardedWeight>
        <Value>0</Value>
      </BoardedWeight>
      <Volume>
        <Value>0</Value>
      </Volume>
      <VolumeUnitOfMeasure>
        <Value />
      </VolumeUnitOfMeasure>
      <ModeOfTransportationCode>
        <Value>11</Value>
      </ModeOfTransportationCode>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>L</Value>
        </LocationTypeCode>
        <Location>
          <Value>USNYC</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>U</Value>
        </LocationTypeCode>
        <Location>
          <Value>GBLON</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>A</Value>
        </LocationTypeCode>
        <Location>
          <Value>GBFXT</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>D</Value>
        </LocationTypeCode>
        <Location>
          <Value>USNYC</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>O</Value>
        </LocationTypeCode>
        <Location>
          <Value>USNYC</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>M</Value>
        </LocationTypeCode>
        <Location>
          <Value>GBLON</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLLocationList>
        <LocationTypeCode>
          <Value>R</Value>
        </LocationTypeCode>
        <Location>
          <Value>NEW YORK</Value>
        </Location>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLLocationList>
      <BOLReferenceInfoList>
        <ReferenceTypeCode>
          <Value>CSK</Value>
        </ReferenceTypeCode>
        <ReferenceData>
          <Value>41352</Value>
        </ReferenceData>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLReferenceInfoList>
      <BOLPartyInfoList>
        <PartyType>
          <Value>CN</Value>
        </PartyType>
        <PartyIdentificationNumber>
          <Value />
        </PartyIdentificationNumber>
        <PartyIdentificationTypeCode>
          <Value />
        </PartyIdentificationTypeCode>
        <PartyName>
          <Value>ACE TEST SUPPLIER HK</Value>
        </PartyName>
        <PartyAddressLine1>
          <Value>173 GLOUCESTER ROAD</Value>
        </PartyAddressLine1>
        <PartyAddressLine2>
          <Value>WAN CHAI</Value>
        </PartyAddressLine2>
        <PartyAddressLine3>
          <Value />
        </PartyAddressLine3>
        <CityName>
          <Value>HONG KONG</Value>
        </CityName>
        <StateCode>
          <Value />
        </StateCode>
        <PostalCode>
          <Value />
        </PostalCode>
        <CountryCode>
          <Value>HK</Value>
        </CountryCode>
        <ContactName>
          <Value />
        </ContactName>
        <ContactPhoneNumber>
          <Value>1234 8714</Value>
        </ContactPhoneNumber>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLPartyInfoList>
      <BOLPartyInfoList>
        <PartyType>
          <Value>SH</Value>
        </PartyType>
        <PartyIdentificationNumber>
          <Value />
        </PartyIdentificationNumber>
        <PartyIdentificationTypeCode>
          <Value />
        </PartyIdentificationTypeCode>
        <PartyName>
          <Value>ACE TEST IMPORTER 1</Value>
        </PartyName>
        <PartyAddressLine1>
          <Value>12A NWD ENTERPRISES 1</Value>
        </PartyAddressLine1>
        <PartyAddressLine2>
          <Value />
        </PartyAddressLine2>
        <PartyAddressLine3>
          <Value />
        </PartyAddressLine3>
        <CityName>
          <Value>Pittsburgh</Value>
        </CityName>
        <StateCode>
          <Value>PA</Value>
        </StateCode>
        <PostalCode>
          <Value>19444</Value>
        </PostalCode>
        <CountryCode>
          <Value>US</Value>
        </CountryCode>
        <ContactName>
          <Value />
        </ContactName>
        <ContactPhoneNumber>
          <Value />
        </ContactPhoneNumber>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLPartyInfoList>
      <BOLPartyInfoList>
        <PartyType>
          <Value>NY</Value>
        </PartyType>
        <PartyIdentificationNumber>
          <Value />
        </PartyIdentificationNumber>
        <PartyIdentificationTypeCode>
          <Value />
        </PartyIdentificationTypeCode>
        <PartyName>
          <Value>ACE TEST SUPPLIER HK</Value>
        </PartyName>
        <PartyAddressLine1>
          <Value>174 GLOUCESTER ROAD</Value>
        </PartyAddressLine1>
        <PartyAddressLine2>
          <Value>WAN CHAI DISTRICT</Value>
        </PartyAddressLine2>
        <PartyAddressLine3>
          <Value />
        </PartyAddressLine3>
        <CityName>
          <Value>HONG KONG</Value>
        </CityName>
        <StateCode>
          <Value />
        </StateCode>
        <PostalCode>
          <Value />
        </PostalCode>
        <CountryCode>
          <Value>HK</Value>
        </CountryCode>
        <ContactName>
          <Value />
        </ContactName>
        <ContactPhoneNumber>
          <Value>+225 55 56 76 51</Value>
        </ContactPhoneNumber>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </BOLPartyInfoList>
      <EquipmentInfoList>
        <EquipmentTypeCode>
          <Value>HV</Value>
        </EquipmentTypeCode>
        <EquipmentNumber>
          <Value>APLU6547893</Value>
        </EquipmentNumber>
        <EquipmentLength>
          <Value>40</Value>
        </EquipmentLength>
        <EquipmentHeight>
          <Value>9</Value>
        </EquipmentHeight>
        <EquipmentWidth>
          <Value>8</Value>
        </EquipmentWidth>
        <EquipmentSizeTypeCode>
          <Value>45G0</Value>
        </EquipmentSizeTypeCode>
        <LoadedEmptyStatus>
          <Value>L</Value>
        </LoadedEmptyStatus>
        <ServiceTypeCode>
          <Value />
        </ServiceTypeCode>
        <CargoInfoList>
          <Quantity>
            <Value>100</Value>
          </Quantity>
          <QuantityUnitOfMeasure>
            <Value>NO</Value>
          </QuantityUnitOfMeasure>
          <CargoDescription>
            <Value>STUFF</Value>
          </CargoDescription>
          <MarksAndNumbers>
            <Value>AS ADDRESSED</Value>
          </MarksAndNumbers>
          <CommodityCode>
            <Value />
          </CommodityCode>
          <CountryOfOrigin>
            <Value>US</Value>
          </CountryOfOrigin>
          <Value>
            <Value />
          </Value>
          <Weight>
            <Value />
          </Weight>
          <WeightUnitOfMeasure>
            <Value />
          </WeightUnitOfMeasure>
          <VINInfoList>
            <VehicleIdentificationNumber>
              <Value />
            </VehicleIdentificationNumber>
            <ResponseMessage>
              <ResponseCode />
              <SeverityIndicator />
              <NarrativeText />
            </ResponseMessage>
          </VINInfoList>
          <ResponseMessage>
            <ResponseCode />
            <SeverityIndicator />
            <NarrativeText />
          </ResponseMessage>
        </CargoInfoList>
        <ResponseMessage>
          <ResponseCode />
          <SeverityIndicator />
          <NarrativeText />
        </ResponseMessage>
      </EquipmentInfoList>
      <ResponseMessage>
        <ResponseCode />
        <SeverityIndicator />
        <NarrativeText />
      </ResponseMessage>
      <Action>
        <ActionCode>A</ActionCode>
        <NarrativeText />
      </Action>
    </BOLInfoList>
    <ResponseMessage>
      <ResponseCode />
      <SeverityIndicator />
      <NarrativeText />
    </ResponseMessage>
  </Conveyance>
  <ResponseMessage>
    <ResponseCode />
    <SeverityIndicator />
    <NarrativeText />
  </ResponseMessage>
</CBPManifestMessage>";
			
			var transportMode = MQMessageSender.GetUEMTransportModeFromXML(UEMMessageContent);
			Assert.AreEqual("Sea", transportMode);
		}

		[TestMethod]
		public void TestSend_AMA()
		{
			var mqQueueManagerProvider = new Mock<IMQQueueManagerProvider>();
			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>())).Callback((MQMessage message, MQPutMessageOptions options) =>
			{
				message.DataOffset = 0;
				string messageBody = message.ReadString(message.MessageLength);
				Assert.AreEqual(MQC.MQPMO_SYNCPOINT, options.Options);
			});

			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var senderMock = new Mock<MQMessageSender>(new NoOpLogger(), managerProviderMock.Object, CancellationToken.None) { CallBase = true };
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<string>()).Returns(mqConfigMock.Object);


			var messageStream = GetEmbeddedResource("TestFiles.TestOutboundAirAMAMessage.xml");
			Stream responseStream;
			var AMAMessage = new USCustomsOutboundMessage(messageStream);
			senderMock.Object.Send(AMAMessage, out responseStream);

			responseStream.Position = 0;
			using (var reader = new StreamReader(responseStream))
			{
				Assert.AreEqual($"<SendResponse ClientId=\"{AMAMessage.ClientId}\" IsProduction=\"{AMAMessage.IsProduction}\" MessageTrackingId=\"{AMAMessage.TrackingId}\" ResponseStatus=\"{Constants.ResponseStatus.Pass}\" ErrorDescription=\"\" />", reader.ReadToEnd());
			}

			senderMock.Protected().Verify("GetMQConfiguration", Times.Once(), "AMA", "", true, "");
			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
			managerWrapperMock.Verify(manager => manager.Commit(), Times.Once());
		}

		Stream GetEmbeddedResource(string resourceName)
		{
			return GetEmbeddedResource(resourceName, Assembly.GetExecutingAssembly());
		}

		Stream GetEmbeddedResource(string resourceName, Assembly executingAssembly)
		{
			string fullResourceName = executingAssembly.GetName().Name + '.' + resourceName;
			var resource = executingAssembly.GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(string.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}
	}
}
