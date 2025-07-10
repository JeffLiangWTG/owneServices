using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	[TestedType(typeof(USLVClearanceDataContextManager))]
	public class USLVClearanceDataContextManagerTest : ShipmentDataContextManagerTestCase<USLVClearanceDataContextManager, CusUSLVClearance>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return System.Array.Empty<RecipientRoleType>(); }
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override bool ManagerChecksDataTargetToImport
		{
			get { return true; }
		}

		public void TestEventContextValues_WithTriggeringLog_ContainsMatchedConsignmentDetails()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "MASTBIL111";
			clearance.ULH_RL_NKPortOfLoading = "PRBQN";
			clearance.ULH_RL_NKPortOfDischarge = "USPIA";
			clearance.ULH_PortOfLoading = "4901";
			clearance.ULH_PortOfDischarge = "3902";
			clearance.ULH_PortOfEntry = "2127";
			clearance.ULH_MatchingKey = "ABC123";
			clearance.ULH_UseCode = "TES";

			var matchedConsignment = clearance.CusUSLVConsignments.AddNew();
			matchedConsignment.ULB_HouseBill = "HOSBIL111";
			matchedConsignment.CE_IssueDate = new ZDateTime(2020, 01, 02);
			matchedConsignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRN;

			var otherConsignment = clearance.CusUSLVConsignments.AddNew();
			otherConsignment.ULB_HouseBill = "HOSBIL222";
			otherConsignment.CE_IssueDate = new ZDateTime(2020, 03, 04);
			otherConsignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRL;

			Factory.SaveForTesting();

			var manager = clearance.GetUniversalDataContextManager() as IEventDataContextManagerWithTriggeringLog;

			var referenceNumber = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, matchedConsignment.ULB_HouseBill);
			var complianceStatus = new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, ImportEntryStatusList.Codes.CRF);

			var clearanceLog = clearance.Logs.AddNew(Events.MessageStatusChange, referenceNumber, complianceStatus);
			manager.TriggeringLogForUseInPopulatingEventContext = clearanceLog;

			var expectedContextValues = new[]
			{
				"HBOLNumber - HOSBIL111",
				"MBOLNumber - MASTBIL111",
				"MBOLOriginUNLOCO - PRBQN",
				"MBOLDestinationUNLOCO - USPIA",
				"MBOLPortOfLoadingScheduleK - 4901",
				"MBOLPortOfDischargeScheduleD - 3902",
				"MBOLPortOfEntryScheduleD - 2127",
				"USLowValueEntriesMatchingKey - ABC123",
				"USLowValueEntriesUseCode - TES",
				"CustomsReleaseDate - 02-Jan-20 00:00:00",
				"ComplianceStatus - CRF"
			};
			var actualContextValues = manager.EventContextValues.Select(e => $"{e.Key.Type} - {e.Value}");

			AssertContainsExactElementsInAnyOrder(expectedContextValues, actualContextValues);
		}

		public void TestEventContextValues_WithNoTriggeringLog_IsEmpty()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "MASTBIL111";
			clearance.ULH_RL_NKPortOfLoading = "PRBQN";
			clearance.ULH_RL_NKPortOfDischarge = "USPIA";
			clearance.ULH_PortOfLoading = "4901";
			clearance.ULH_PortOfDischarge = "3902";
			clearance.ULH_PortOfEntry = "2127";
			clearance.ULH_MatchingKey = "ABC123";
			clearance.ULH_UseCode = "TES";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "HOSBIL111";
			consignment.CE_IssueDate = new ZDateTime(2020, 01, 02);
			consignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRN;

			Factory.SaveForTesting();

			var manager = clearance.GetUniversalDataContextManager() as IEventDataContextManagerWithTriggeringLog;
			AssertEquals(0, manager.EventContextValues.Count());
		}

		public void TestEventContextValues_WithTriggeringLogWithNoRFN_IsEmpty()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "MASTBIL111";
			clearance.ULH_RL_NKPortOfLoading = "PRBQN";
			clearance.ULH_RL_NKPortOfDischarge = "USPIA";
			clearance.ULH_PortOfLoading = "4901";
			clearance.ULH_PortOfDischarge = "3902";
			clearance.ULH_PortOfEntry = "2127";
			clearance.ULH_MatchingKey = "ABC123";
			clearance.ULH_UseCode = "TES";

			var matchedConsignment = clearance.CusUSLVConsignments.AddNew();
			matchedConsignment.ULB_HouseBill = "HOSBIL111";
			matchedConsignment.CE_IssueDate = new ZDateTime(2020, 01, 02);
			matchedConsignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRN;

			Factory.SaveForTesting();

			var manager = clearance.GetUniversalDataContextManager() as IEventDataContextManagerWithTriggeringLog;

			var clearanceLog = clearance.Logs.AddNew(Events.MessageStatusChange);
			manager.TriggeringLogForUseInPopulatingEventContext = clearanceLog;

			AssertEquals(0, manager.EventContextValues.Count());
		}

		public void TestEventContextValues_WithTriggeringLogWithRFNNotFound_IsEmpty()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_MasterBill = "MASTBIL111";
			clearance.ULH_RL_NKPortOfLoading = "PRBQN";
			clearance.ULH_RL_NKPortOfDischarge = "USPIA";
			clearance.ULH_PortOfLoading = "4901";
			clearance.ULH_PortOfDischarge = "3902";
			clearance.ULH_PortOfEntry = "2127";
			clearance.ULH_MatchingKey = "ABC123";
			clearance.ULH_UseCode = "TES";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "HOSBIL111";
			consignment.CE_IssueDate = new ZDateTime(2020, 01, 02);
			consignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRN;

			Factory.SaveForTesting();

			var manager = clearance.GetUniversalDataContextManager() as IEventDataContextManagerWithTriggeringLog;

			var referenceNumber = new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "INVALID RFN");
			var clearanceLog = clearance.Logs.AddNew(Events.MessageStatusChange, referenceNumber);
			manager.TriggeringLogForUseInPopulatingEventContext = clearanceLog;

			AssertEquals(0, manager.EventContextValues.Count());
		}

		public void TestGetShipmentDataObjectReader_WhenSubShipmentIsHVLShipment_ReturnsETailUSLVClearanceDataObjectReader()
		{
			var manager = new USLVClearanceDataContextManager();
			var readerGetterMethod = typeof(USLVClearanceDataContextManager).GetMethod("GetShipmentDataObjectReader", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(UniversalShipment), typeof(IXmlImportLogger), typeof(UniversalObjectFactory) }, null);

			var logger = new DummyLogger();
			var factory = new UniversalObjectFactory();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.ShipmentType = new CodeDescriptionPair() { Code = "HVL", Description = "High Volume Low Value" };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(subShipment);
			var reader = readerGetterMethod.Invoke(manager, new object[] { shipment, logger, factory }) as ShipmentDataObjectReader<CusUSLVClearance>;

			AssertNotNull(reader);
			AssertType<eTailUSLVClearanceDataObjectReader>(reader);
		}
	}
}
