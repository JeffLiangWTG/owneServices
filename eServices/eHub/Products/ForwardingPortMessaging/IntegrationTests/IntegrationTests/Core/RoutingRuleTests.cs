using System.Linq;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using eServices.eHubRoutingRuleEngine;
using NUnit.Framework;
using Rule = eServices.eHubRoutingRuleEngine.Rule;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.IntegrationTests
{
    [TestFixture]
	public class RoutingRuleTests : RoutingRuleIntegrationTestBase
	{
		[Test]
		public void TestRule_FORWARDING_PORT_MESSAGING_SPLIT()
		{
			var message = new TestingMessage("HYEDAUUAT", "FORWARDING_PORT_MESSAGE");
			message.ShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerLoadPlan/1";
			message.DocumentName = "eCLP";
			var context = CreateeHubTransactionsContext();
			if (context.eHubClients != null)
			{
				var clientService = context.eHubClients.First(x => x.CC_ID == "FORWARDING_PORT_MESSAGE");
				var factResolver = new RoutingRuleMessageFactResolver(message);

				var rule = Rule.GetForReading(clientService);

				var result = rule.Evaluate(context, new IFactResolver[] { factResolver }, mockLogger);
				AssertClient("FORWARDING_PORT_MESSAGE_SPLIT", result);
			}
		}

		[Test]
		public void TestDefaultReject()
		{
			var message = new TestingMessage("HYEDAUUAT", "FORWARDING_PORT_MESSAGE");
			message.ShipmentNamespace = "XXXXX";
			message.Containers = "";
			var context = CreateeHubTransactionsContext();
			if (context.eHubClients != null)
			{
				var clientService = context.eHubClients.First(x => x.CC_ID == "FORWARDING_PORT_MESSAGE");
				var factResolver = new RoutingRuleMessageFactResolver(message);

				var rule = Rule.GetForReading(clientService);

				var result = rule.Evaluate(context, new IFactResolver[] { factResolver }, mockLogger);
				AssertError("IRJ", "Department=WiseTechGlobal|Reason=You are not registered with this Carrier for this message type. Contact WTG to register.", result);
			}
		}

		#region Test Data

		public string twoContainers = @"<ContainerCollection>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MAEU0494852</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DunnageWeight>0</DunnageWeight>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>20240</GrossWeight>
        <GrossWeightVerificationDateTime>2018-05-21T09:30:00</GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description='Method 2 - Packages'>PKG</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <Seal>3659788</Seal>
        <SealPartyType Description='Carrier/Shipping Line'>CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>2280</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <WeightUnit Description='Kilograms'>KG</WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <Address1>WESTINGHOUSE ROAD</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>MANCHESTER</City>
            <CompanyName>YOUR UNITED KINGDOM COMPANY (MANCHESTER)</CompanyName>
            <Contact>Operations</Contact>
            <Country Name='United Kingdom'>GB</Country>
            <Email>operations.gbmnc@youragent.com</Email>
            <Fax>+441618722222</Fax>
            <GovRegNum></GovRegNum>
            <Phone>+441618721111</Phone>
            <Port Name='Manchester'>GBMNC</Port>
            <Postcode>M17 1DP</Postcode>
            <State>Greater Manchester</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description='Legacy System Code'>LSC</Type>
                <CountryOfIssue Name='United Kingdom'>GB</CountryOfIssue>
                <Value>MNC</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MAEU1234021</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DunnageWeight>0</DunnageWeight>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>16900</GrossWeight>
        <GrossWeightVerificationDateTime>2018-05-21T09:00:00</GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description='Method 1 - Container'>CNT</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <Seal>3423423</Seal>
        <SealPartyType Description='Carrier/Shipping Line'>CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>2280</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <WeightUnit Description='Kilograms'>KG</WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <Address1>UNIT 14, LIDDALL WAY</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>WEST DRAYTON</City>
            <CompanyName>YOUR UNITED KINGDOM COMPANY (LONDON)</CompanyName>
            <Contact>Operations</Contact>
            <Country Name='United Kingdom'>GB</Country>
            <Email>operations.gblhr@youragent.com</Email>
            <Fax>+441899999998</Fax>
            <GovRegNum></GovRegNum>
            <Phone>+441899999999</Phone>
            <Port Name='Heathrow Apt/London'>GBLHR</Port>
            <Postcode>UB7 8PG</Postcode>
            <State>Greater London</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description='VAT Business Registration Number'>VAT</Type>
                <CountryOfIssue Name='United Kingdom'>GB</CountryOfIssue>
                <Value>945390992</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>";

		#endregion
	}
}
