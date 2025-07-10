using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentDataObjectReaderForComplianceRiskStatusTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestComplianceRiskRegistryFalseWhenAddedShipmentAndComplianceRiskStatusHasNotCreated()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFile);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNull("Compliance risk status should not be created.", complianceRiskStatus);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Shipment (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x CusEntryNumber.".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryFalseWhenAddedShipmentAndComplianceRiskStatusHasCreated()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFile);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Shipment (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x CusEntryNumber.".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedShipmentAndCompliancePartyRiskStatusHasPotentialRisk()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFile);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "HSK", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodities risk status", "UNK", complianceRiskStatus.COR_CommodityRisk);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Shipment (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x CusEntryNumber.".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedShipmentAndCompliancePartyRiskStatusHasClear()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABABEU"));
			consignor.OH_ScreeningStatus = "CLR";

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AASDRA"));
			consignee.OH_ScreeningStatus = "CLR";

			Factory.SaveForTesting();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFile);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "CLR", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodity risk status", "UNK", complianceRiskStatus.COR_CommodityRisk);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Shipment (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x CusEntryNumber.".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenUpdatedShipmentAndCommoditiesRiskStatusHasClear()
		{
			var jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.JS_HouseBill = "CRT18231973";
			jobShipment.JS_UniqueConsignRef = "S00001000";
			jobShipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABABEU"));
			consignor.OH_ScreeningStatus = "CLR";

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AASDRA"));
			consignee.OH_ScreeningStatus = "CLR";

			Factory.SaveForTesting();

			var tariffView = LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "090121", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			CombineAssertions("Precondition :", () =>
			{
				AssertEquals("WCO", tariffView.ZZ1_ZZZ_NKDataGrouping);
				AssertEquals("HSN", tariffView.ZZ1_ZZI_NKTariffType);
				AssertEquals("090121", tariffView.ZZ1_TariffCode);
			});
			tariffView.Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithKeyPackLine);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "CLR", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodities risk status", "UNK", complianceRiskStatus.COR_CommodityRisk);
					AssertMultilineASCIIEquals("Import successful log", @"
Updated Shipment S00001000 (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x ForwardingPackLine.".Trim(), serviceTaskLog.ToString());
					AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Updated Shipment S00001000 (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x ForwardingPackLine.".Trim(), ediMessage.GetLogNoteText());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedShipmentAndCommoditiesRiskStatusHasClear()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABABEU"));
			consignor.OH_ScreeningStatus = "CLR";

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AASDRA"));
			consignee.OH_ScreeningStatus = "CLR";

			Factory.SaveForTesting();

			var tariffView = LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "090121", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			CombineAssertions("Precondition :", () =>
			{
				AssertEquals("WCO", tariffView.ZZ1_ZZZ_NKDataGrouping);
				AssertEquals("HSN", tariffView.ZZ1_ZZI_NKTariffType);
				AssertEquals("090121", tariffView.ZZ1_TariffCode);
			});
			tariffView.Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithPackLine);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "CLR", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commoditiess risk status", "UNK", complianceRiskStatus.COR_CommodityRisk);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Shipment (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x ForwardingPackLine.".Trim(), serviceTaskLog.ToString());
					AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Added Shipment (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x ForwardingPackLine.".Trim(), ediMessage.GetLogNoteText());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenUpdatedShipmentAndPartyRiskStatusHasClear()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNRSEA";
			consignor.OH_IsConsignor = true;
			consignor.OH_ScreeningStatus = "CLR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNESEA";
			consignee.OH_IsConsignee = true;
			consignee.OH_ScreeningStatus = "CLR";

			var jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.JS_HouseBill = "CRT18231973";
			jobShipment.JS_UniqueConsignRef = "S00001000";
			jobShipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;
			jobShipment.ConsignorPK = consignor.PK;
			jobShipment.ConsigneePK = consignee.PK;

			var riskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			riskStatus.COR_ParentTableCode = jobShipment.TablePrefix;
			riskStatus.COR_ParentID = jobShipment.PK;
			riskStatus.COR_PartyRisk = "CLR";
			riskStatus.COR_LocationRisk = "CLR";
			riskStatus.COR_CommodityRisk = "PSK";
			riskStatus.COR_OverallRisk = "PSK";

			Factory.SaveForTesting();

			TestConnection.ExecuteNonQuery($@"
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = '{ScreeningStatusesList.Codes.Clear}' WHERE OH_CODE = 'ABABEU'
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = '{ScreeningStatusesList.Codes.Unknown}' WHERE OH_CODE = 'AASDRA'");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithParty);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", "HSK", complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodities risk status", "UNK", complianceRiskStatus.COR_CommodityRisk);
					AssertMultilineASCIIEquals("Import successful log", @"
Updated Shipment S00001000 (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973').".Trim(), serviceTaskLog.ToString());
					AssertMultilineASCIIEquals("Import successful log", @"Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Updated Shipment S00001000 (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973').".Trim(), ediMessage.GetLogNoteText());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedDomesticShipment()
		{
			var domesticImportFile = ShipmentImportFileWithPackLine.Replace("<PortOfOrigin>\r\n\t  <Code>DEFRA</Code>\r\n\t  <Name>Frankfurt am Main</Name>\r\n\t</PortOfOrigin>", "<PortOfOrigin>\r\n\t  <Code>HK8ST</Code>\r\n\t  <Name>Sha Tau Kok</Name>\r\n\t</PortOfOrigin>");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(domesticImportFile);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("NAP", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalShipmentAndNoHSCodes()
		{
			var importFileWithoutHsCode = ShipmentImportFileWithPackLine.Replace("<HarmonisedCode>090121</HarmonisedCode>", "");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(importFileWithoutHsCode);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalShipmentAndWithHSCodes()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(new BusinessObjectFactory(), "090121");
			tariffView.Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithPackLine);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalShipmentAndWithHSCodesWithConditions()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(new BusinessObjectFactory(), "090121", "Test Conditions");
			tariffView.Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithPackLine);
				var (shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestJobMaterialChanges_ShouldResetAllCommoditiyRiskStatus()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "CRT18231973";
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = "CRI";
			eventLog.SCE_EventSubType = "CAI";
			eventLog.SCE_ParentID = shipment.PK;

			Factory.SaveForTesting();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithKeyPackLine);
				(shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				CombineAssertions(() =>
				{
					AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Commodities risk status NCH", 3, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertMultilineASCIIEquals("Import successful log", @"
Updated Shipment S00001000 (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x ForwardingPackLine.".Trim(), serviceTaskLog.ToString());
					AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Updated Shipment S00001000 (House Bill='CRT18231973') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='CRT18231973') with 1 x ForwardingPackLine.".Trim(), ediMessage.GetLogNoteText());
				});
			}
		}

		public void TestCommodityMaterialChangesPackLine_ShouldResetCommoditiyRiskStatus()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "CRT18231973";
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;
			shipment.JS_RL_NKOrigin = "DEFRA";
			shipment.JS_RL_NKDestination = "HKHKG";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "090121";
			packLine.JL_RN_NKOrigin = "AU";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "090121";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			commodity.CCD_RN_NKOrigin = "AU";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "123456";
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			Factory.SaveForTesting();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(ShipmentImportFileWithPackLine);
				(shipment, complianceRiskStatus) = GetDataObject("CRT18231973");

				AssertContainsExactElementsInAnyOrder("Commodities risk status of 090121 reset to NCH", new[]
				{
					("090121", ComplianceRiskStatusCodeList.Codes.NotChecked),
					("123456", ComplianceRiskStatusCodeList.Codes.Blocked),
				}, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Select(u => (u.CCD_HarmonizedCode.ToString(), u.CCD_RiskStatus.ToString())));
			}
		}

		#region Implementation

		TariffView LoadOrCreateNewTariff(string dataGrouping, string tariffCode, string nkTariffType)
		{
			var helper = new UniversalReferenceTestDataHelper(new BusinessObjectFactory());
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, nkTariffType);
			var tariffView = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			return tariffView;
		}

		(Messaging.Integration.IEDIMessage EDIMessage, ServiceTaskLogForTesting ServiceTaskLog) ProcessUniversalShipmentMessage(string xmlMessage)
		{
			var ediMessage = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(ediMessage);

			return (ediMessage, serviceTaskLog);
		}

		(ForwardingShipment Shipment, ComplianceRiskStatus ComplianceRiskStatus) GetDataObject(string houseBill)
		{
			var shipment = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, houseBill));
			var complianceRiskStatus = new BusinessObjectFactory().LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));

			return (shipment, complianceRiskStatus);
		}

		const string ShipmentImportFileWithParty = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment Action=""MERGE"">
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key>S00001000</Key>
		</DataTarget>
	  </DataTargetCollection>

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
		<Code>ARV</Code>
		<Description>Arrival</Description>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>DAT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDescription>test</TriggerDescription>
	  <TriggerType>Trigger</TriggerType>
	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>CNR</Code>
		  <Description>Consignor</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>0.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<CommunityTransitStatus>
	  <Code></Code>
	</CommunityTransitStatus>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>FCL</Code>
	  <Description>Full Container Load</Description>
	</ContainerMode>
	<DocumentedChargeable>0.000</DocumentedChargeable>
	<DocumentedVolume>0.000</DocumentedVolume>
	<DocumentedWeight>0.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription></GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>AUD</Code>
	  <Description>Australian Dollar</Description>
	</GoodsValueCurrency>
	<HBLAWBChargesDisplay>
	  <Code>SHW</Code>
	  <Description>Show Collect Charges</Description>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<HouseBillOfLadingType>
	  <Code>IAU</Code>
	  <Description>IT Club Australia</Description>
	</HouseBillOfLadingType>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>AUD</Code>
	  <Description>Australian Dollar</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCancelled>false</IsCancelled>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsHighRisk>false</IsHighRisk>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<ManifestedChargeable>0.000</ManifestedChargeable>
	<ManifestedVolume>0.000</ManifestedVolume>
	<ManifestedWeight>0.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>HKHKG</Code>
	  <Name>Hong Kong</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>DEFRA</Code>
	  <Name>Frankfurt am Main</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code>BRR</Code>
	  <Description>Letter of Credit (Bank Release)</Description>
	</ReleaseType>
	<ScreeningStatus>
	  <Code>UNK</Code>
	  <Description>Unknown</Description>
	</ScreeningStatus>
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
	<ShipperCODAmount>0.0000</ShipperCODAmount>
	<ShipperCODPayMethod>
	  <Code></Code>
	</ShipperCODPayMethod>
	<TotalNoOfPacks>0</TotalNoOfPacks>
	<TotalNoOfPacksPackageType>
	  <Code>CTN</Code>
	  <Description>Carton</Description>
	</TotalNoOfPacksPackageType>
	<TotalVolume>0.000</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>0.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>CRT18231973</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>
	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DeliveryRequiredFrom></DeliveryRequiredFrom>
	  <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
	  <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
	  <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
	  <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
	  <FCLDeliveryEquipmentNeeded>
		<Code></Code>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
	  <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
	  <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
	  <FCLPickupEquipmentNeeded>
		<Code></Code>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PickupRequiredFrom></PickupRequiredFrom>
	  <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
	  <PickupTruckWaitTime></PickupTruckWaitTime>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>
	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryDueDate</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>PickupReceiptRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryReceiptRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>PickupDispatchRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryDispatchRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>
	<OrganizationAddressCollection>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string ShipmentImportFileWithKeyPackLine = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment Action=""MERGE"">
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key>S00001000</Key>
		</DataTarget>
	  </DataTargetCollection>

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
		<Code>ARV</Code>
		<Description>Arrival</Description>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>DAT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDescription>test</TriggerDescription>
	  <TriggerType>Trigger</TriggerType>
	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>CNR</Code>
		  <Description>Consignor</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>0.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<CommunityTransitStatus>
	  <Code></Code>
	</CommunityTransitStatus>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>FCL</Code>
	  <Description>Full Container Load</Description>
	</ContainerMode>
	<DocumentedChargeable>0.000</DocumentedChargeable>
	<DocumentedVolume>0.000</DocumentedVolume>
	<DocumentedWeight>0.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription></GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>AUD</Code>
	  <Description>Australian Dollar</Description>
	</GoodsValueCurrency>
	<HBLAWBChargesDisplay>
	  <Code>SHW</Code>
	  <Description>Show Collect Charges</Description>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<HouseBillOfLadingType>
	  <Code>IAU</Code>
	  <Description>IT Club Australia</Description>
	</HouseBillOfLadingType>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>AUD</Code>
	  <Description>Australian Dollar</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCancelled>false</IsCancelled>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsHighRisk>false</IsHighRisk>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<ManifestedChargeable>0.000</ManifestedChargeable>
	<ManifestedVolume>0.000</ManifestedVolume>
	<ManifestedWeight>0.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>HKHKG</Code>
	  <Name>Hong Kong</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>DEFRA</Code>
	  <Name>Frankfurt am Main</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code>BRR</Code>
	  <Description>Letter of Credit (Bank Release)</Description>
	</ReleaseType>
	<ScreeningStatus>
	  <Code>UNK</Code>
	  <Description>Unknown</Description>
	</ScreeningStatus>
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
	<ShipperCODAmount>0.0000</ShipperCODAmount>
	<ShipperCODPayMethod>
	  <Code></Code>
	</ShipperCODPayMethod>
	<TotalNoOfPacks>0</TotalNoOfPacks>
	<TotalNoOfPacksPackageType>
	  <Code>CTN</Code>
	  <Description>Carton</Description>
	</TotalNoOfPacksPackageType>
	<TotalVolume>0.000</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>0.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>CRT18231973</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>
	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DeliveryRequiredFrom></DeliveryRequiredFrom>
	  <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
	  <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
	  <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
	  <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
	  <FCLDeliveryEquipmentNeeded>
		<Code></Code>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
	  <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
	  <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
	  <FCLPickupEquipmentNeeded>
		<Code></Code>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PickupRequiredFrom></PickupRequiredFrom>
	  <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
	  <PickupTruckWaitTime></PickupTruckWaitTime>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>
	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryDueDate</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>PickupReceiptRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryReceiptRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>PickupDispatchRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryDispatchRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>
	<OrganizationAddressCollection>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
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
  </Shipment>
</UniversalShipment>
";

		const string ShipmentImportFile = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>

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
	<IsForwardRegistered>true</IsForwardRegistered>
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
	<WayBillNumber>CRT18231973</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>

	<AdditionalReferenceCollection>
	  <AdditionalReference>
		<Type>
		  <Code>AMS</Code>
		  <Description>AMS Number</Description>
		</Type>
		<ReferenceNumber>CE00001</ReferenceNumber>
	  </AdditionalReference>
	</AdditionalReferenceCollection>

	<OrganizationAddressCollection>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<Phone></Phone>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<Phone></Phone>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		const string ShipmentImportFileWithPackLine = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment Action=""MERGE"">
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>

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
		<Code>ARV</Code>
		<Description>Arrival</Description>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>DAT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDescription>test</TriggerDescription>
	  <TriggerType>Trigger</TriggerType>
	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>CNR</Code>
		  <Description>Consignor</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>0.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<CommunityTransitStatus>
	  <Code></Code>
	</CommunityTransitStatus>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>FCL</Code>
	  <Description>Full Container Load</Description>
	</ContainerMode>
	<DocumentedChargeable>0.000</DocumentedChargeable>
	<DocumentedVolume>0.000</DocumentedVolume>
	<DocumentedWeight>0.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription></GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>AUD</Code>
	  <Description>Australian Dollar</Description>
	</GoodsValueCurrency>
	<HBLAWBChargesDisplay>
	  <Code>SHW</Code>
	  <Description>Show Collect Charges</Description>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<HouseBillOfLadingType>
	  <Code>IAU</Code>
	  <Description>IT Club Australia</Description>
	</HouseBillOfLadingType>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>AUD</Code>
	  <Description>Australian Dollar</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCancelled>false</IsCancelled>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsHighRisk>false</IsHighRisk>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<ManifestedChargeable>0.000</ManifestedChargeable>
	<ManifestedVolume>0.000</ManifestedVolume>
	<ManifestedWeight>0.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>HKHKG</Code>
	  <Name>Hong Kong</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>DEFRA</Code>
	  <Name>Frankfurt am Main</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code>BRR</Code>
	  <Description>Letter of Credit (Bank Release)</Description>
	</ReleaseType>
	<ScreeningStatus>
	  <Code>UNK</Code>
	  <Description>Unknown</Description>
	</ScreeningStatus>
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
	<ShipperCODAmount>0.0000</ShipperCODAmount>
	<ShipperCODPayMethod>
	  <Code></Code>
	</ShipperCODPayMethod>
	<TotalNoOfPacks>0</TotalNoOfPacks>
	<TotalNoOfPacksPackageType>
	  <Code>CTN</Code>
	  <Description>Carton</Description>
	</TotalNoOfPacksPackageType>
	<TotalVolume>0.000</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>0.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>CRT18231973</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>
	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DeliveryRequiredFrom></DeliveryRequiredFrom>
	  <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
	  <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
	  <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
	  <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
	  <FCLDeliveryEquipmentNeeded>
		<Code></Code>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
	  <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
	  <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
	  <FCLPickupEquipmentNeeded>
		<Code></Code>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PickupRequiredFrom></PickupRequiredFrom>
	  <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
	  <PickupTruckWaitTime></PickupTruckWaitTime>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>
	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryDueDate</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>PickupReceiptRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryReceiptRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>PickupDispatchRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>DeliveryDispatchRequested</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>
	<OrganizationAddressCollection>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<PackingLineCollection Content=""Complete"">
	  <PackingLine>
		<Commodity>
		  <Code>GEN</Code>
		  <Description>General</Description>
		</Commodity>
		<ContainerPackingOrder>0</ContainerPackingOrder>
		<CountryOfOrigin>
		  <Code>US</Code>
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
  </Shipment>
</UniversalShipment>
";

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
