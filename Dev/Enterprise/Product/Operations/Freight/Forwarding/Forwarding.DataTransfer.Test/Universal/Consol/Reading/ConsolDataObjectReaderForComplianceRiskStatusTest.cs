using System;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolDataObjectReaderForComplianceRiskStatusTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestComplianceRiskRegistryFalseWhenAddedConsolidationAndComplianceRiskStatusHasNotCreated()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ConsolidationImportFile);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNull("Compliance risk status should not be created.", complianceRiskStatus);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Consol (Master Bill='0182381973') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='0182381973') with 1 x ForwardingConsolStmNote, 1 x Transport.".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryFalseWhenAddedConsolidationAndComplianceRiskStatusHasCreated()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ConsolidationImportFile);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Consol (Master Bill='0182381973') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='0182381973') with 1 x ForwardingConsolStmNote, 1 x Transport.".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedConsolidationAndComplianceRiskStatusClear()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "HighWind";
			vessel.RV_LloydsNumber = "Lloyds";
			vessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
			vessel.RV_ScreeningStatus = "CLR";

			Factory.SaveForTesting();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ConsolidationImportFile);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(consolidation.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(consolidation.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "CLR", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "CLR", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodity risk status", "NAP", complianceRiskStatus.COR_CommodityRisk);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Consol (Master Bill='0182381973') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='0182381973') with 1 x ForwardingConsolStmNote, 1 x Transport.".Trim(), serviceTaskLog.ToString());
					AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: NZCHC Destination: AUSYD
Attempting to get Schedule for the Transport Leg
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Added Consol (Master Bill='0182381973') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='0182381973') with 1 x ForwardingConsolStmNote, 1 x Transport.".Trim(), ediMessage.GetLogNoteText());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenUpdatedConsolidationAndComplianceRiskStatusHasHeldRiskStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C000180403";
			consol.JK_RL_NKLoadPort = "NZCHC";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "0182381973";

			Factory.SaveForTesting();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ConsolidationImportFile);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(consol.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(consol.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "HSK", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodities risk status", "NAP", complianceRiskStatus.COR_CommodityRisk);

					AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C000180403 (Master Bill='0182381973') from UniversalShipment.
Successfully saved Consol C000180403 (Master Bill='0182381973') with 1 x ForwardingConsolStmNote, 1 x Transport.".Trim(), serviceTaskLog.ToString());
					AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: NZCHC Destination: AUSYD
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Updated Consol C000180403 (Master Bill='0182381973') from UniversalShipment.
Successfully saved Consol C000180403 (Master Bill='0182381973') with 1 x ForwardingConsolStmNote, 1 x Transport.".Trim(), ediMessage.GetLogNoteText());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedDomesticConsol()
		{
			var domesticImportFile = ConsolidationImportFile.Replace("<PortOfLoading>\r\n      <Code>NZCHC</Code>\r\n      <Name>Christchurch</Name>\r\n    </PortOfLoading>", "<PortOfLoading>\r\n      <Code>AUWTB</Code>\r\n      <Name>Brisbane West Wellcamp Airport</Name>\r\n    </PortOfLoading>");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(domesticImportFile);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("NAP", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalConsolAndNoHSCodes()
		{
			var importFileWithoutHsCode = ConsolidationImportFileWithCommodity.Replace("<HarmonisedCode>090121</HarmonisedCode>", "");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(importFileWithoutHsCode);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("NAP", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalConsolAndWithHSCodes()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(new BusinessObjectFactory(), "090121");
			tariffView.Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ConsolidationImportFileWithCommodity);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalConsolAndWithHSCodesWithConditions()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(new BusinessObjectFactory(), "090121", "Test Conditions");
			tariffView.Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ConsolidationImportFileWithCommodity);
				var (consolidation, complianceRiskStatus) = GetDataObject("0182381973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		#region Implementation

		const string ConsolidationImportFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DNZ</Code>
        <Name>NZ Demo Company</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <AgentsReference>Agent U</AgentsReference>
    <AWBServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </AWBServiceLevel>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code>NZD</Code>
      <Description>New Zealand, Dollars</Description>
    </FreightRateCurrency>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <NoCopyBills>4</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PortFirstForeign>
      <Code>CNSHA</Code>
      <Name>Shanghai</Name>
    </PortFirstForeign>
    <PortLastForeign>
      <Code>MXCUU</Code>
      <Name>Chihuahua</Name>
    </PortLastForeign>
    <PlaceOfIssue>
      <Code>NZAKL</Code>
      <Name>Auckland</Name>
    </PlaceOfIssue>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code>JPOSA</Code>
      <Name>Osaka</Name>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>NZCHC</Code>
      <Name>Christchurch</Name>
    </PortOfLoading>
    <ReleaseType>
      <Code>CAD</Code>
      <Description>Cash Against Documents</Description>
    </ReleaseType>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
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
    <TotalPreallocatedWeight>1</TotalPreallocatedWeight>
    <TotalPreallocatedWeightUnit>
      <Code>MG</Code>
      <Description>Milligrams</Description>
    </TotalPreallocatedWeightUnit>
    <TotalPreallocatedVolume>2</TotalPreallocatedVolume>
    <TotalPreallocatedVolumeUnit>
      <Code>CC</Code>
      <Description>CubicCentimeters</Description>
    </TotalPreallocatedVolumeUnit>
    <TotalPreallocatedChargeable>3</TotalPreallocatedChargeable>
    <CarrierCorrectedWeight>5</CarrierCorrectedWeight>
    <CarrierCorrectedWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </CarrierCorrectedWeightUnit>
    <CarrierCorrectedVolume>6</CarrierCorrectedVolume>
    <CarrierCorrectedVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </CarrierCorrectedVolumeUnit>
    <CarrierCorrectedChargeable>7</CarrierCorrectedChargeable>
    <ChargeableRate>8</ChargeableRate>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>HighWind</VesselName>
    <VoyageFlightNo>QF253</VoyageFlightNo>
    <WayBillNumber>0182381973</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-12-10T00:00:00</Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-12-14T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-12-16T00:00:00</Value>
      </Date>
      <Date>
        <Type>CutOffDate</Type>
        <IsEstimate>true</IsEstimate>
        <Value>1953-03-05T00:00:00</Value>
      </Date>
      <Date>
        <Type>DepartureReceiptRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-09T00:00:00</Value>
      </Date>
      <Date>
        <Type>ArrivalReceiptRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-11T00:00:00</Value>
      </Date>
      <Date>
        <Type>DepartureDispatchRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-13T00:00:00</Value>
      </Date>
      <Date>
        <Type>ArrivalDispatchRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-15T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note Action=""MERGE"">
        <Description>DOG FLOGGER!!</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Flog the Dog</NoteText>
        <NoteContext>
          <Code>BEB</Code>
          <Description>Module: B - Brokerage, Direction: E - Export, Freight: B - Air and Sea</Description>
        </NoteContext>
        <Visibility>
          <Code>PUB</Code>
          <Description>CLIENT-VISIBLE</Description>
        </Visibility>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress Action=""MERGE"">
        <AddressType>Creditor</AddressType>
        <OrganizationCode>THECODE</OrganizationCode>
        <Address1>SOME STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>MASCOT</City>
        <CompanyName>SOME COMPANY</CompanyName>
        <Contact>DENNIS</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>ben.govett@cargowise.com</Email>
        <Fax>3860 8799</Fax>
        <Mobile></Mobile>
        <Phone>3860 8844</Phone>
        <Postcode>2020</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <TransportLegCollection>
      <TransportLeg Action=""MERGE"">
        <PortOfDischarge>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>NZCHC</Code>
          <Name>Christchurch</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <TransportMode>Sea</TransportMode>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <Carrier Action=""MERGE"">
          <AddressType>Carrier</AddressType>
          <OrganizationCode>QANAIRSYD</OrganizationCode>
          <Address1>QANTAS CENTRE, 203 COWARD STREET</Address1>
          <Address2></Address2>
          <AddressOverride>false</AddressOverride>
          <City>MASCOT</City>
          <CompanyName>QANTAS AIRWAYS LIMITED</CompanyName>
          <Contact>DENNIS</Contact>
          <Country>
            <Code>AU</Code>
            <Name>Australia</Name>
          </Country>
          <Email>dps.compliancerisk@cargowise.com</Email>
          <Fax>3860 8799</Fax>
          <Mobile></Mobile>
          <Phone>3860 8844</Phone>
          <Postcode>2020</Postcode>
          <ScreeningStatus>
            <Code>UNK</Code>
            <Description>Unknown</Description>
          </ScreeningStatus>
          <State>NSW</State>
        </Carrier>
        <CarrierBookingReference>REFOFCAR</CarrierBookingReference>
        <EstimatedArrival>2010-12-20T12:20:00</EstimatedArrival>
        <EstimatedDeparture>2010-12-20T12:20:00</EstimatedDeparture>
        <LegType>Main</LegType>
        <VesselName>HighWind</VesselName>
        <VoyageFlightNo>QF253</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
    <SpecialHandlingCollection>
      <SpecialHandling>
        <Code>PER</Code>
      </SpecialHandling>
    </SpecialHandlingCollection>
    <RequiresTemperatureControl>true</RequiresTemperatureControl>
    <RequiredTemperatureMinimum>1.000</RequiredTemperatureMinimum>
    <RequiredTemperatureMaximum>100.0</RequiredTemperatureMaximum>
    <RequiredTemperatureUnit>
      <Code>C</Code>
    </RequiredTemperatureUnit>
</Shipment>
</UniversalShipment>
";

		const string ConsolidationImportFileWithCommodity = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DNZ</Code>
        <Name>NZ Demo Company</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <AgentsReference>Agent U</AgentsReference>
    <AWBServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </AWBServiceLevel>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code>NZD</Code>
      <Description>New Zealand, Dollars</Description>
    </FreightRateCurrency>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <NoCopyBills>4</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PortFirstForeign>
      <Code>CNSHA</Code>
      <Name>Shanghai</Name>
    </PortFirstForeign>
    <PortLastForeign>
      <Code>MXCUU</Code>
      <Name>Chihuahua</Name>
    </PortLastForeign>
    <PlaceOfIssue>
      <Code>NZAKL</Code>
      <Name>Auckland</Name>
    </PlaceOfIssue>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code>JPOSA</Code>
      <Name>Osaka</Name>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>NZCHC</Code>
      <Name>Christchurch</Name>
    </PortOfLoading>
    <ReleaseType>
      <Code>CAD</Code>
      <Description>Cash Against Documents</Description>
    </ReleaseType>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
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
    <TotalPreallocatedWeight>1</TotalPreallocatedWeight>
    <TotalPreallocatedWeightUnit>
      <Code>MG</Code>
      <Description>Milligrams</Description>
    </TotalPreallocatedWeightUnit>
    <TotalPreallocatedVolume>2</TotalPreallocatedVolume>
    <TotalPreallocatedVolumeUnit>
      <Code>CC</Code>
      <Description>CubicCentimeters</Description>
    </TotalPreallocatedVolumeUnit>
    <TotalPreallocatedChargeable>3</TotalPreallocatedChargeable>
    <CarrierCorrectedWeight>5</CarrierCorrectedWeight>
    <CarrierCorrectedWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </CarrierCorrectedWeightUnit>
    <CarrierCorrectedVolume>6</CarrierCorrectedVolume>
    <CarrierCorrectedVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </CarrierCorrectedVolumeUnit>
    <CarrierCorrectedChargeable>7</CarrierCorrectedChargeable>
    <ChargeableRate>8</ChargeableRate>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>HighWind</VesselName>
    <VoyageFlightNo>QF253</VoyageFlightNo>
    <WayBillNumber>0182381973</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-12-10T00:00:00</Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-12-14T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-12-16T00:00:00</Value>
      </Date>
      <Date>
        <Type>CutOffDate</Type>
        <IsEstimate>true</IsEstimate>
        <Value>1953-03-05T00:00:00</Value>
      </Date>
      <Date>
        <Type>DepartureReceiptRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-09T00:00:00</Value>
      </Date>
      <Date>
        <Type>ArrivalReceiptRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-11T00:00:00</Value>
      </Date>
      <Date>
        <Type>DepartureDispatchRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-13T00:00:00</Value>
      </Date>
      <Date>
        <Type>ArrivalDispatchRequested</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-08-15T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note Action=""MERGE"">
        <Description>DOG FLOGGER!!</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Flog the Dog</NoteText>
        <NoteContext>
          <Code>BEB</Code>
          <Description>Module: B - Brokerage, Direction: E - Export, Freight: B - Air and Sea</Description>
        </NoteContext>
        <Visibility>
          <Code>PUB</Code>
          <Description>CLIENT-VISIBLE</Description>
        </Visibility>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress Action=""MERGE"">
        <AddressType>Creditor</AddressType>
        <OrganizationCode>THECODE</OrganizationCode>
        <Address1>SOME STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>MASCOT</City>
        <CompanyName>SOME COMPANY</CompanyName>
        <Contact>DENNIS</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>ben.govett@cargowise.com</Email>
        <Fax>3860 8799</Fax>
        <Mobile></Mobile>
        <Phone>3860 8844</Phone>
        <Postcode>2020</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>SESYDDAU54661467</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
		<PackingLineCollection Content=""Complete"">
		  <PackingLine>
			<Commodity>
			  <Code>GEN</Code>
			  <Description>General</Description>
			</Commodity>
			<ContainerPackingOrder>0</ContainerPackingOrder>
			<CountryOfOrigin>
			  <Code></Code>
			</CountryOfOrigin>
			<DetailedDescription></DetailedDescription>
			<EndItemNo>0</EndItemNo>
			<ExportReferenceNumber></ExportReferenceNumber>
			<GoodsDescription></GoodsDescription>
			<HarmonisedCode>090121</HarmonisedCode>
			<Height>0.000</Height>
			<ImportReferenceNumber></ImportReferenceNumber>
			<ItemNo>0</ItemNo>
			<LastKnownCFSStatus>
			  <Code></Code>
			</LastKnownCFSStatus>
			<LastKnownCFSStatusDate></LastKnownCFSStatusDate>
			<Length>0.000</Length>
			<LengthUnit>
			  <Code>M</Code>
			  <Description>Meters</Description>
			</LengthUnit>
			<LinePrice>0.0000</LinePrice>
			<Link>1</Link>
			<LoadingMeters>0.000</LoadingMeters>
			<MarksAndNos></MarksAndNos>
			<OutturnComment></OutturnComment>
			<OutturnDamagedQty>0</OutturnDamagedQty>
			<OutturnedHeight>0.000</OutturnedHeight>
			<OutturnedLength>0.000</OutturnedLength>
			<OutturnedVolume>0.000</OutturnedVolume>
			<OutturnedWeight>0.000</OutturnedWeight>
			<OutturnedWidth>0.000</OutturnedWidth>
			<OutturnPillagedQty>0</OutturnPillagedQty>
			<OutturnQty>0</OutturnQty>
			<PackingLineID>EDIDAT00000012</PackingLineID>
			<PackQty>0</PackQty>
			<PackType>
			  <Code>PLT</Code>
			  <Description>Pallet</Description>
			</PackType>
			<ReferenceNumber></ReferenceNumber>
			<RequiresTemperatureControl>false</RequiresTemperatureControl>
			<Volume>0.000</Volume>
			<VolumeUnit>
			  <Code>M3</Code>
			  <Description>Cubic Meters</Description>
			</VolumeUnit>
			<Weight>0.000</Weight>
			<WeightUnit>
			  <Code>KG</Code>
			  <Description>Kilograms</Description>
			</WeightUnit>
			<Width>0.000</Width>
			<PackedItemCollection>
			</PackedItemCollection>
		  </PackingLine>
		</PackingLineCollection>
	  </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection>
      <TransportLeg Action=""MERGE"">
        <PortOfDischarge>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>NZCHC</Code>
          <Name>Christchurch</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <TransportMode>Sea</TransportMode>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <Carrier Action=""MERGE"">
          <AddressType>Carrier</AddressType>
          <OrganizationCode>QANAIRSYD</OrganizationCode>
          <Address1>QANTAS CENTRE, 203 COWARD STREET</Address1>
          <Address2></Address2>
          <AddressOverride>false</AddressOverride>
          <City>MASCOT</City>
          <CompanyName>QANTAS AIRWAYS LIMITED</CompanyName>
          <Contact>DENNIS</Contact>
          <Country>
            <Code>AU</Code>
            <Name>Australia</Name>
          </Country>
          <Email>dps.compliancerisk@cargowise.com</Email>
          <Fax>3860 8799</Fax>
          <Mobile></Mobile>
          <Phone>3860 8844</Phone>
          <Postcode>2020</Postcode>
          <ScreeningStatus>
            <Code>UNK</Code>
            <Description>Unknown</Description>
          </ScreeningStatus>
          <State>NSW</State>
        </Carrier>
        <CarrierBookingReference>REFOFCAR</CarrierBookingReference>
        <EstimatedArrival>2010-12-20T12:20:00</EstimatedArrival>
        <EstimatedDeparture>2010-12-20T12:20:00</EstimatedDeparture>
        <LegType>Main</LegType>
        <VesselName>HighWind</VesselName>
        <VoyageFlightNo>QF253</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
    <SpecialHandlingCollection>
      <SpecialHandling>
        <Code>PER</Code>
      </SpecialHandling>
    </SpecialHandlingCollection>
    <RequiresTemperatureControl>true</RequiresTemperatureControl>
    <RequiredTemperatureMinimum>1.000</RequiredTemperatureMinimum>
    <RequiredTemperatureMaximum>100.0</RequiredTemperatureMaximum>
    <RequiredTemperatureUnit>
      <Code>C</Code>
    </RequiredTemperatureUnit>
</Shipment>
</UniversalShipment>
";

		(Messaging.Integration.IEDIMessage EDIMessage, ServiceTaskLogForTesting ServiceTaskLog) ProcessUniversalShipmentMessage(string xmlMessage)
		{
			var ediMessage = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(ediMessage);

			return (ediMessage, serviceTaskLog);
		}

		(ForwardingConsol Comsolidation, ComplianceRiskStatus ComplianceRiskStatus) GetDataObject(string masterBill)
		{
			var consolidation = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, masterBill));
			var complianceRiskStatus = new BusinessObjectFactory().LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, consolidation.PK));

			return (consolidation, complianceRiskStatus);
		}

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}

		#endregion
	}
}
