using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Matching.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using EventConstants = CargoWise.EventReference.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestConsolNotUpdatedInDBIfCreditorTypeIsNullInShipmentPenalty()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			string fileName = "UniversalShipmentWithNullCreditorType.xml";
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertNoExceptionThrown(() => {
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(fileName);
				var errorCounts = logs.Count(log => log.Message.Contains("Creditor type in container penalty can not be null. Invalid container numbers (MSDU1062310)"));
				Assert("One error related to a null container penalty should exist", errorCounts == 1);
			});

			Factory.SaveForTesting();
		}

		public void TestImportConsolWithCommodity()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.ConsolCommodity = new Commodity();
			consolDataObject.ConsolCommodity.Code = "SHIP";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SHIP", consolBO.JK_RH_NKConsolCommodity);

			consolDataObject = SetupConsol("ConsolImportFileWithoutConsolCommodity.xml");
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();
			AssertEquals("The commodity field will not be removed", "SHIP", consolBO.JK_RH_NKConsolCommodity);

			consolDataObject = SetupConsol("ConsolImportFileWithEmptyConsolCommodity.xml");
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();
			AssertEquals("The commodity field will not be removed", "SHIP", consolBO.JK_RH_NKConsolCommodity);

			consolDataObject = SetupConsol("ConsolImportFileWithEmptyConsolCommodityCode.xml");
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();
			AssertEquals("The commodity field has been removed", string.Empty, consolBO.JK_RH_NKConsolCommodity);
		}

		public void TestUniversalShipment_WayBillNum_BookingRef()
		{
			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_UniqueConsignRef = "C00002221";
			consolA.JK_MasterBillNum = "123456789";
			consolA.JK_BookingReference = "XXXXXXX";

			var consolB = Factory.New<ForwardingConsol>();
			consolB.JK_UniqueConsignRef = "C00002222";
			consolB.JK_BookingReference = "987654321";

			var consolC = Factory.New<ForwardingConsol>();
			consolC.JK_UniqueConsignRef = "C00002223";
			consolC.JK_MasterBillNum = "123456789";

			var consolD = Factory.New<ForwardingConsol>();
			consolD.JK_UniqueConsignRef = "C00002224";
			consolD.JK_MasterBillNum = "123456789";
			consolD.JK_BookingReference = "987654321";

			Factory.SaveForTesting();

			AssertMatchingLog("C00002224");

			void AssertMatchingLog(string uniqueConsignRef)
			{
				var testFile = "UniversalShipment_NotCoload_WayBillNum_BookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
				AssertGreaterThan(macthingCount, 0);
			}
		}

		public void TestUniversalShipment_Coload_WayBillNum_BookingRef()
		{
			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_UniqueConsignRef = "C00002221";
			consolA.JK_AgentType = Constants.AgentType.CoLoad;
			consolA.JK_CoLoadMasterBill = "123456789";
			consolA.JK_CoLoadBookingReference = "XXXXXXX";

			var consolB = Factory.New<ForwardingConsol>();
			consolB.JK_UniqueConsignRef = "C00002222";
			consolB.JK_AgentType = Constants.AgentType.CoLoad;
			consolB.JK_CoLoadBookingReference = "987654321";

			var consolC = Factory.New<ForwardingConsol>();
			consolC.JK_AgentType = Constants.AgentType.CoLoad;
			consolC.JK_UniqueConsignRef = "C00002223";
			consolC.JK_CoLoadMasterBill = "123456789";

			var consolD = Factory.New<ForwardingConsol>();
			consolD.JK_UniqueConsignRef = "C00002224";
			consolD.JK_AgentType = Constants.AgentType.CoLoad;
			consolD.JK_CoLoadMasterBill = "123456789";
			consolD.JK_CoLoadBookingReference = "987654321";

			Factory.SaveForTesting();

			AssertMatchingLog("C00002224");

			void AssertMatchingLog(string uniqueConsignRef)
			{
				var testFile = "UniversalShipment_Coload_WayBillNum_BookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
				AssertGreaterThan(macthingCount, 0);
			}
		}

		public void TestUniversalShipment_Coload_ColoadWayBillNum_ColoadBookingRef()
		{
			var consolD = Factory.New<ForwardingConsol>();
			consolD.JK_UniqueConsignRef = "C00002224";
			consolD.JK_MasterBillNum = "123456789";
			consolD.JK_BookingReference = "987654321";

			var consolE = Factory.New<ForwardingConsol>();
			consolE.JK_UniqueConsignRef = "C00002225";
			consolE.JK_AgentType = Constants.AgentType.CoLoad;
			consolE.JK_CoLoadMasterBill = "123456789";
			consolE.JK_CoLoadBookingReference = "987654321";

			var consolF = Factory.New<ForwardingConsol>();
			consolF.JK_UniqueConsignRef = "C00002226";
			consolF.JK_AgentType = Constants.AgentType.CoLoad;
			consolF.JK_CoLoadMasterBill = "coload_123456789";
			consolF.JK_CoLoadBookingReference = "coload_987654321";

			Factory.SaveForTesting();

			AssertMatchingLog("C00002226");

			void AssertMatchingLog(string uniqueConsignRef)
			{
				var testFile = "UniversalShipment_Coload_ColoadWayBillNum_ColoadBookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
				AssertGreaterThan(macthingCount, 0);
			}
		}

		public void TestUniversalShipment_CreateNew_IfMismatch()
		{
			var consolE = Factory.New<ForwardingConsol>();
			consolE.JK_UniqueConsignRef = "C00002225";
			consolE.JK_AgentType = Constants.AgentType.CoLoad;
			consolE.JK_CoLoadMasterBill = "XXXXX";
			consolE.JK_CoLoadBookingReference = "YYYYY";

			var consolF = Factory.New<ForwardingConsol>();
			consolF.JK_UniqueConsignRef = "C00002226";
			consolF.JK_AgentType = Constants.AgentType.CoLoad;
			consolF.JK_CoLoadMasterBill = "YYYYY";
			consolF.JK_CoLoadBookingReference = "XXXXX";

			Factory.SaveForTesting();

			AssertMatchingLog();

			void AssertMatchingLog()
			{
				var testFile = "UniversalShipment_Coload_ColoadWayBillNum_ColoadBookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains("No matching ForwardingConsol found, creating new ForwardingConsol."));
				AssertGreaterThan(macthingCount, 0);
			}
		}

		public void TestUniversalShipment_CLD_GetByLastestTime()
		{
			var consolE = Factory.New<ForwardingConsol>();
			consolE.JK_UniqueConsignRef = "C00002225";
			consolE.JK_AgentType = Constants.AgentType.CoLoad;
			consolE.JK_CoLoadMasterBill = "coload_123456789";
			consolE.JK_CoLoadBookingReference = "coload_987654321";
			consolE.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var consolF = Factory.New<ForwardingConsol>();
			consolF.JK_UniqueConsignRef = "C00002226";
			consolF.JK_AgentType = Constants.AgentType.CoLoad;
			consolF.JK_CoLoadMasterBill = "coload_123456789";
			consolF.JK_CoLoadBookingReference = "coload_987654321";
			consolF.JK_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Factory.SaveForTesting();

			AssertMatchingLog("C00002225");

			void AssertMatchingLog(string uniqueConsignRef)
			{
				var testFile = "UniversalShipment_Coload_ColoadWayBillNum_ColoadBookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
				AssertGreaterThan(macthingCount, 0);
			}
		}

		public void TestUniversalShipment_NotCLD_GetByLastestTime()
		{
			var consolE = Factory.New<ForwardingConsol>();
			consolE.JK_UniqueConsignRef = "C00002225";
			consolE.JK_MasterBillNum = "123456789";
			consolE.JK_BookingReference = "987654321";
			consolE.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var consolF = Factory.New<ForwardingConsol>();
			consolF.JK_UniqueConsignRef = "C00002226";
			consolF.JK_MasterBillNum = "123456789";
			consolF.JK_BookingReference = "987654321";
			consolF.JK_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Factory.SaveForTesting();

			AssertMatchingLog("C00002225");

			void AssertMatchingLog(string uniqueConsignRef)
			{
				var testFile = "UniversalShipment_NotCoload_WayBillNum_BookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
				AssertGreaterThan(macthingCount, 0);
			}
		}
		public void TestUniversalShipment_C1C_SCAC()
		{
			var consolE = Factory.New<ForwardingConsol>();
			consolE.JK_UniqueConsignRef = "C00002225";
			consolE.JK_MasterBillNum = "123456789";
			consolE.JK_BookingReference = "987654321";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsCreditor = true;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "C1CV", "US");

			var consolF = Factory.New<ForwardingConsol>();
			consolF.JK_UniqueConsignRef = "C00002226";
			consolF.JK_MasterBillNum = "123456789";
			consolF.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.SaveForTesting();

			AssertMatchingLog("C00002226");

			void AssertMatchingLog(string uniqueConsignRef)
			{
				var testFile = "UniversalShipment_WayBillNum_BookingRef_SCAC.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
				AssertGreaterThan(macthingCount, 0);
			}
		}

		public void TestUniversalShipment_IgnoreNotFullScoreIfFindMultiple()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsCreditor = true;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "XXXX", "US");

			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_UniqueConsignRef = "C00002225";
			consolA.JK_MasterBillNum = "123456789";
			consolA.JK_BookingReference = "987654321";
			consolA.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var consolB = Factory.New<ForwardingConsol>();
			consolB.JK_UniqueConsignRef = "C00002226";
			consolB.JK_MasterBillNum = "123456789";
			consolB.JK_BookingReference = "987654321";
			consolB.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.SaveForTesting();

			AssertMatchingLog(string.Empty, false);

			carrier.CustomsCodes.Cast<OrgCusCode>().Single().OK_CustomsRegNo = "C1CV";
			consolA.JK_SystemCreateTimeUtc = ZDateTime.UtcNow;
			consolB.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);
			Factory.SaveForTesting();

			AssertMatchingLog("C00002226", true);

			void AssertMatchingLog(string uniqueConsignRef, bool exists)
			{
				var testFile = "UniversalShipment_WayBillNum_BookingRef_SCAC.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				if (exists)
				{
					var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
					AssertGreaterThan(macthingCount, 0);
				}
				else
				{
					var macthingCount = logs.Count(log => log.Message.Contains("No matching ForwardingConsol found, creating new ForwardingConsol."));
					AssertGreaterThan(macthingCount, 0);
				}
			}
		}

		public void TestUniversalShipment_IgnoreCanceledConsol()
		{
			var consolD = Factory.New<ForwardingConsol>();
			consolD.JK_UniqueConsignRef = "C00002224";
			consolD.JK_MasterBillNum = "123456789";
			consolD.JK_BookingReference = "987654321";
			Factory.SaveForTesting();

			AssertMatchingLog("C00002224", true);

			consolD.IsCancelled = true;
			Factory.SaveForTesting();

			AssertMatchingLog("C00002224", false);

			void AssertMatchingLog(string uniqueConsignRef, bool exists)
			{
				var testFile = "UniversalShipment_NotCoload_WayBillNum_BookingRef.xml";
				var logs = CreateAndProcessUniversalShipmentWithProcessorLogs(testFile);
				if (exists)
				{
					var macthingCount = logs.Count(log => log.Message.Contains($"Successfully saved Consol {uniqueConsignRef}"));
					AssertGreaterThan(macthingCount, 0);
				}
				else
				{
					var macthingCount = logs.Count(log => log.Message.Contains("No matching ForwardingConsol found, creating new ForwardingConsol."));
					AssertGreaterThan(macthingCount, 0);
				}
			}
		}

		public void TestUniversalShipment_UpdateNonCoLoadToNonCoLoad()
		{
			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_UniqueConsignRef = "C00002221";
			consolA.JK_MasterBillNum = "123456789";

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Sea" };
			consolDataObject.WayBillNumber = "123456789";
			consolDataObject.BookingConfirmationReference = "987654321";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("123456789", consolBO.JK_MasterBillNum);
			AssertEquals("987654321", consolBO.JK_BookingReference);
		}

		public void TestUniversalShipment_UpdateNonCoLoadToCoLoad()
		{
			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_UniqueConsignRef = "C00002221";
			consolA.JK_AgentType = Constants.AgentType.CoLoad;
			consolA.JK_MasterBillNum = "AAA";
			consolA.JK_BookingReference = "BBB";
			consolA.JK_CoLoadMasterBill = "123456789";

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Sea" };
			consolDataObject.WayBillNumber = "123456789";
			consolDataObject.BookingConfirmationReference = "987654321";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("AAA", consolA.JK_MasterBillNum);
			AssertEquals("BBB", consolA.JK_BookingReference);
			AssertEquals("123456789", consolA.JK_CoLoadMasterBill);
			AssertEquals("987654321", consolA.JK_CoLoadBookingReference);
		}

		public void TestMAWBHandlingInformationExtraText()
		{
			using (FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"\"{First}\"\" == \"\"1\"\").First()>"))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var consolDataObject = SetupConsol();
				consolDataObject.TransportMode = new CodeDescriptionPair { Code = TransportModes.Air };
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				AssertExceptionThrown<DataObjectReadFailureException>("Should not throw ExportAWBHeaderReplaceMacrosException", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestDateCollection()
		{
			var consolDataObject = SetupConsol();
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("consolBO.JK_PackDepotReceiptRequested", new ZDateTime(2017, 8, 9), consolBO.JK_PackDepotReceiptRequested);
			AssertEquals("consolBO.JK_UnpackDepotReceiptRequested", new ZDateTime(2017, 8, 11), consolBO.JK_UnpackDepotReceiptRequested);
			AssertEquals("consolBO.JK_PackDepotDispatchRequested", new ZDateTime(2017, 8, 13), consolBO.JK_PackDepotDispatchRequested);
			AssertEquals("consolBO.JK_UnpackDepotDispatchRequested", new ZDateTime(2017, 8, 15), consolBO.JK_UnpackDepotDispatchRequested);
		}

		public void TestImportConsolWithKeyFailedIfKeyIsSpecified()
		{
			string fileName = "UniversalConsolWithKey.xml";
			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Match couldn't be found for ForwardingConsol with Key C00001000
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestUpdateConsolWithKeyRespectUpdateRegistry()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			string fileName = "UniversalConsol.xml";

			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='NLSHA0005422') from UniversalShipment.
Added Consol (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x ForwardingContainer, 1 x Transport, 2 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

			fileName = "UniversalConsolWithKey.xml";

			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Consol C00001000 (Master Bill='SITSHOSWB92209') wasn't updated because of registry settings 'eServices->Universal XML->Automatic Update on Import'.
Successfully saved, but nothing was reported as being updated.
".Trim(), serviceTaskLog.ToString());

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Shipment S00001000 (House Bill='NLSHA0005422') wasn't updated because of registry settings 'eServices->Universal XML->Automatic Update on Import'.
Updated Consol C00001000 (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x ForwardingContainer, 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='NLSHA0005422') from UniversalShipment.
Updated Consol C00001000 (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x ForwardingContainer, 1 x Transport, 2 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportConsolRespectRegistryUpdateConsolDuringAutomaticImportAndUpdateConsolShipmentsDuringAutomaticImport()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			string fileName = "UniversalConsol.xml";

			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='NLSHA0005422') from UniversalShipment.
Added Consol (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x ForwardingContainer, 1 x Transport, 2 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='NLSHA0005422') from UniversalShipment.
Updated Consol C00001000 (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x ForwardingContainer, 1 x Transport, 2 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Consol C00001000 (Master Bill='SITSHOSWB92209') wasn't updated because of registry settings 'eServices->Universal XML->Automatic Update on Import'.
Successfully saved, but nothing was reported as being updated.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentWithoutGrossWeight()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var fileName = "UniversalShipmentWithoutGrossWeight.xml";
			CreateAndProcessUniversalShipment(fileName);

			var container = Factory.LoadTop1<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_ContainerNum, "DFSU6004840"));
			AssertEquals("GrossWeight no presented, StandAloneCustomsContainer should not be set", false, container.StandAloneCustomsContainer);
			AssertEquals("GrossWeight should be combined from GoodsWeight", 13971.000m, container.JC_GrossWeight);
		}

		public void TestImportConsolRespectRegistryUpdateConsolContainersDuringAutomaticImport()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			string fileName = "UniversalConsol.xml";

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateAndProcessUniversalShipment(fileName);
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertNotNull("Consol wasn't created", consol);
			AssertEquals("Container was not created", 1, consol.Containers.Count);
			var oldValue = consol.Containers[0].JC_ArrivalSlotReference;
			ZString newValue = "blabla";
			consol.Containers[0].JC_ArrivalSlotReference = newValue;
			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateAndProcessUniversalShipment(fileName);
			consol = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertEquals("Container was not created", 1, consol.Containers.Count);
			AssertEquals("Container was updated", newValue, consol.Containers[0].JC_ArrivalSlotReference);

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateAndProcessUniversalShipment(fileName);
			consol = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertEquals("Container was not created", 1, consol.Containers.Count);
			AssertEquals("Container was not updated", oldValue, consol.Containers[0].JC_ArrivalSlotReference);
		}

		public void TestImportConsolRespectRegistryUpdateConsolsRoutingInformationDuringAutomaticImport()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			string fileName = "UniversalConsol.xml";

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateAndProcessUniversalShipment(fileName);
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertNotNull("Consol wasn't created", consol);
			AssertEquals("Transports was not created", 1, consol.Transports.Count);
			var oldValue = consol.Transports[0].JW_CarrierBookingReference;
			ZString newValue = "blabla";
			consol.Transports[0].JW_CarrierBookingReference = newValue;
			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateAndProcessUniversalShipment(fileName);
			consol = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertEquals("Container was not created", 1, consol.Transports.Count);
			AssertEquals("Container was updated", newValue, consol.Transports[0].JW_CarrierBookingReference);

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateAndProcessUniversalShipment(fileName);
			consol = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertEquals("Container was not created", 1, consol.Transports.Count);
			AssertEquals("Container was not updated", oldValue, consol.Transports[0].JW_CarrierBookingReference);
		}

		public void TestImportWorkflowExceptions()
		{
			var consolDataObject = SetupConsol();
			AssertNull("No ExceptionCollection on XML", consolDataObject.ExceptionCollection);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consol = reader.ReadIntoBusinessObject();

			AssertEquals("No ExceptionCollection, should import without errors ", 0, consol.WorkflowItems.Exceptions.Count);
			AssertContains("Information - No matching ForwardingConsol found, creating new ForwardingConsol.", logger.Logs);

			Factory.SaveForTesting();
			logger.ClearLogs();

			consolDataObject.SetExceptionCollection(() => new List<WorkflowException>());
			consol = reader.ReadIntoBusinessObject();

			AssertEquals("No Exception on ExceptionCollection, should import without errors ", 0, consol.WorkflowItems.Exceptions.Count);
			AssertContains("Information - Successfully loaded matching ForwardingConsol.", logger.Logs);

			Factory.SaveForTesting();
			logger.ClearLogs();

			var now = ZDateTimeOffset.Now;

			var workflowException = new[]
			{
				new WorkflowException()
				{
					Description = "Exception 1",
					Date = now,
				},
				new WorkflowException()
				{
					Description = "Exception 2",
					Date = now,
					Actioned = true
				},
			};

			consolDataObject.SetExceptionCollection(() => workflowException.ToList());
			consol = reader.ReadIntoBusinessObject();

			AssertEquals("Exceptions on ExceptionCollection, should import without errors ", 2, consol.WorkflowItems.Exceptions.Count);

			var exception1 = consol.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 1");
			AssertNotNull("Exception 1 is in consol", exception1);
			AssertEquals(now.ToZDateTime(), exception1.P9_ActualDate);
			AssertEquals(now, exception1.P9_ActualDateOffset);
			AssertEquals(false, exception1.IsExceptionActioned);

			var exception2 = consol.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 2");
			AssertNotNull("Exception 2 is in consol", exception2);
			AssertEquals(now.ToZDateTime(), exception2.P9_ActualDate);
			AssertEquals(now, exception2.P9_ActualDateOffset);
			AssertEquals(true, exception2.IsExceptionActioned);

			AssertContains("Information - Populating Exception: Exception 1...", logger.Logs);
			AssertContains("Information - Populating Exception: Exception 2...", logger.Logs);
		}

		public void TestImportWorkflowExceptionsInSubShipments()
		{
			var consolDataObject = SetupConsol();
			AssertNull("No ExceptionCollection on XML", consolDataObject.ExceptionCollection);

			var subShipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentDataObject.WayBillNumber = "SUBSHIPMENT1111";
			subShipmentDataObject.PortOfOrigin = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };

			var shipmentDataObject = ShipmentDataObjectReaderTest.SetupShipment();
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipmentDataObject);

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolDataObject.SubShipmentCollection.Add(shipmentDataObject);

			var now = ZDateTimeOffset.Now;

			var workflowException = new[]
			{
				new WorkflowException()
				{
					Description = "Exception 1",
					Date = now,
				},
				new WorkflowException()
				{
					Description = "Exception 2",
					Date = now,
					Actioned = true
				},
			};
			var workflowExceptionSub = new[]
{
				new WorkflowException()
				{
					Description = "Exception 3",
					Date = now,
				},
				new WorkflowException()
				{
					Description = "Exception 4",
					Date = now,
					Actioned = true
				},
			};

			shipmentDataObject.SetExceptionCollection(() => workflowException.ToList());
			subShipmentDataObject.SetExceptionCollection(() => workflowExceptionSub.ToList());
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consol = reader.ReadIntoBusinessObject();

			var shipmentBO = consol.GridShipments[0];

			AssertEquals("Exceptions on ExceptionCollection, should import without errors ", 2, shipmentBO.WorkflowItems.Exceptions.Count);

			var exception1 = shipmentBO.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 1");
			AssertNotNull("Exception 1 is in shipmentBO", exception1);
			AssertEquals(now, exception1.P9_ActualDateOffset);
			AssertEquals(false, exception1.IsExceptionActioned);

			var exception2 = shipmentBO.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 2");
			AssertNotNull("Exception 2 is in shipmentBO", exception2);
			AssertEquals(now, exception2.P9_ActualDateOffset);
			AssertEquals(true, exception2.IsExceptionActioned);

			var subShipmentBO = shipmentBO.CoLoadShipments[0];

			AssertEquals("Exceptions on ExceptionCollection, should import without errors ", 2, subShipmentBO.WorkflowItems.Exceptions.Count);

			var exception3 = subShipmentBO.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 3");
			AssertNotNull("Exception 3 is in subShipmentBO", exception3);
			AssertEquals(now, exception3.P9_ActualDateOffset);
			AssertEquals(false, exception3.IsExceptionActioned);

			var exception4 = subShipmentBO.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 4");
			AssertNotNull("Exception 4 is in subShipmentBO", exception4);
			AssertEquals(now, exception4.P9_ActualDateOffset);
			AssertEquals(true, exception4.IsExceptionActioned);
		}

		public void TestCreatingConsolFromXMLHasForeignPortAdded()
		{
			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testUShipment.TransportMode = new CodeDescriptionPair { Code = "Air" };
			testUShipment.PortLastForeign = new UNLOCO { Code = "NZAKL", Name = "Aukland" };
			testUShipment.PortFirstForeign = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("NZAKL", consolBO.JK_RL_NKLastForeignPort);
			AssertEquals("AUSYD", consolBO.JK_RL_NKFirstForeignPort);
		}

		public void TestDoNotImportTransportLegsFromTransitReceive() => TestDoNotImportTransportLegsFromTW(DataContextType.TransitReceive);

		public void TestDoNotImportTransportLegsFromTransitDispatch() => TestDoNotImportTransportLegsFromTW(DataContextType.TransitDispatch);

		void TestDoNotImportTransportLegsFromTW(DataContextType contextType)
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_MasterBillNum = "08111111214";

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(contextType, "RCN000001");
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Air" };
			consolDataObject.WayBillNumber = "081-11111214";
			consolDataObject.WayBillType = new WayBillType { Code = "mwb" };

			consolDataObject.PortOfLoading = new UNLOCO { Code = "Syd" };
			consolDataObject.PortOfDischarge = new UNLOCO { Code = "LAX" };
			consolDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var transportLegs = consolDataObject.TransportLegCollection;
			transportLegs.Add(new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO { Code = "Syd" }, PortOfDischarge = new UNLOCO { Code = "sin" } });
			transportLegs.Add(new TransportLeg { LegOrder = 2, PortOfLoading = new UNLOCO { Code = "SIN" }, PortOfDischarge = new UNLOCO { Code = "laX" } });

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNotContains("Populating Transport...", logger.Logs);
		}

		public void TestIATACodeGetsConvertedToUNLOCO()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_MasterBillNum = "08111111214";

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Air" };
			consolDataObject.WayBillNumber = "081-11111214";
			consolDataObject.WayBillType = new WayBillType { Code = "mwb" };

			consolDataObject.PortOfLoading = new UNLOCO { Code = "Syd" };
			consolDataObject.PortOfDischarge = new UNLOCO { Code = "LAX" };
			consolDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var transportLegs = consolDataObject.TransportLegCollection;
			transportLegs.Add(new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO { Code = "Syd" }, PortOfDischarge = new UNLOCO { Code = "sin" } });
			transportLegs.Add(new TransportLeg { LegOrder = 2, PortOfLoading = new UNLOCO { Code = "SIN" }, PortOfDischarge = new UNLOCO { Code = "laX" } });

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.JK_TransportMode", "AIR", consolBO.JK_TransportMode);

				AssertEquals("consolBO.JK_RL_NKLoadPort", "AUSYD", consolBO.JK_RL_NKLoadPort);
				AssertEquals("consolBO.JK_RL_NKDischargePort", "USLAX", consolBO.JK_RL_NKDischargePort);

				var leg1 = consolBO.Transports[0];
				AssertEquals("leg1.JW_RL_NKLoadPort", "AUSYD", leg1.JW_RL_NKLoadPort);
				AssertEquals("leg1.JW_RL_NKDiscPort", "SGSIN", leg1.JW_RL_NKDiscPort);

				var leg2 = consolBO.Transports[1];
				AssertEquals("leg2.JW_RL_NKLoadPort", "SGSIN", leg2.JW_RL_NKLoadPort);
				AssertEquals("leg2.JW_RL_NKDiscPort", "USLAX", leg2.JW_RL_NKDiscPort);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: Syd Destination: sin
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: SIN Destination: laX
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestLowerCaseWaybillTypeAndTransportModeWorksWhenIncomingMAWBContainsDashes()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_MasterBillNum = "08111111214";

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Air" };
			consolDataObject.WayBillNumber = "081-11111214";
			consolDataObject.WayBillType = new WayBillType { Code = "mwb" };
			consolDataObject.BookingConfirmationReference = "FieldUpdatedHowExciting";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.JK_BookingReference", "FieldUpdatedHowExciting", consolBO.JK_BookingReference);
				AssertEquals("consolBO.JK_TransportMode", "AIR", consolBO.JK_TransportMode);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestAddWarningForConsolCostingWhenDataContextIsNotMappedToTargetXML()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOKME";
			consolBOToLoad.JK_RL_NKDischargePort = "AUSYD";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsolWithConsolCostingNotMappedToTargetXML();
			consolDataObject.BookingConfirmationReference = "BOOKME";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.PK", consolBOToLoad.PK, consolBO.PK);
				AssertEquals("consolBO.JK_BookingReference", "BOOKME", consolBO.JK_BookingReference);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - MAWB Number will not be allocated from Stock due to the XML having the IsNeutralMaster element set to false.
Warning - ConsolCosting element was ignored. To import ConsolCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Information - Updated Consol C00001000 (Master Bill='Meg') from UniversalShipment.".Trim(), logger.Logs);
			});
		}

		public void TestLoadingConsolFallsBackToDischargePort()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOKME";
			consolBOToLoad.JK_RL_NKDischargePort = "AUSYD";

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var dummyConsol = Factory.New<ForwardingConsol>();
			dummyConsol.JK_BookingReference = "BOOKME";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOKME";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.PK", consolBOToLoad.PK, consolBO.PK);
				AssertEquals("consolBO.JK_BookingReference", "BOOKME", consolBO.JK_BookingReference);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolFallsBackToLoadPort()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOKME";
			consolBOToLoad.JK_RL_NKLoadPort = "NZCHC";

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var dummyConsol = Factory.New<ForwardingConsol>();
			dummyConsol.JK_BookingReference = "BOOKME";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOKME";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.PK", consolBOToLoad.PK, consolBO.PK);
				AssertEquals("consolBO.JK_BookingReference", "BOOKME", consolBO.JK_BookingReference);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolTransportLegCollectionIsCompleteByDefault()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOKME";
			consolBOToLoad.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consolBOToLoad.Transports[0];
			transport1.JW_RL_NKLoadPort = "NZCHC";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = consolBOToLoad.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUBNE";

			Factory.SaveForTesting();

			AssertEquals("Precondition", 2, consolBOToLoad.Transports.Count);

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOKME";

			AssertNull("Content is not specified", consolDataObject.TransportLegCollection.Content);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			AssertEquals("TransportLegCollection was treated as complete", 1, consolBO.Transports.Count);
			AssertEquals("NZCHC", consolBO.Transports[0].JW_RL_NKLoadPort);
			AssertEquals("AUSYD", consolBO.Transports[0].JW_RL_NKDiscPort);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.PK", consolBOToLoad.PK, consolBO.PK);
				AssertEquals("consolBO.JK_BookingReference", "BOOKME", consolBO.JK_BookingReference);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolTransportLegCollectionWithPartialAttribute()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOKME";
			consolBOToLoad.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consolBOToLoad.Transports[0];
			transport1.JW_RL_NKLoadPort = "NZCHC";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = consolBOToLoad.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUBNE";

			Factory.SaveForTesting();

			AssertEquals("Precondition", 2, consolBOToLoad.Transports.Count);

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOKME";
			consolDataObject.TransportLegCollection.Content = CollectionContent.Partial;
			consolDataObject.TransportLegCollection.Add(new TransportLeg
			{
				PortOfLoading = new UNLOCO { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO { Code = "AUMEL", Name = "Melbourne" }
			});

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			AssertEquals("Transport count", 3, consolBO.Transports.Count);
			AssertNotNull("NZCHC->AUSYD", consolBO.Transports.Cast<Transport>().FirstOrDefault(x => x.JW_RL_NKLoadPort == "NZCHC" && x.JW_RL_NKDiscPort == "AUSYD"));
			AssertNotNull("AUSYD->AUBNE", consolBO.Transports.Cast<Transport>().FirstOrDefault(x => x.JW_RL_NKLoadPort == "AUSYD" && x.JW_RL_NKDiscPort == "AUBNE"));
			AssertNotNull("AUBNE->AUMEL", consolBO.Transports.Cast<Transport>().FirstOrDefault(x => x.JW_RL_NKLoadPort == "AUBNE" && x.JW_RL_NKDiscPort == "AUMEL"));

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.PK", consolBOToLoad.PK, consolBO.PK);
				AssertEquals("consolBO.JK_BookingReference", "BOOKME", consolBO.JK_BookingReference);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUBNE Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolThroughAdditionalReferences()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();

			var cusEntryNumber1 = consolBOToLoad.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = consolBOToLoad.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber3 = consolBOToLoad.Numbers.AddNew();
			cusEntryNumber3.CE_EntryNum = "CE00003";
			cusEntryNumber3.CE_EntryType = "UBR";
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "AMS", Description = "AMS Number" }, ReferenceNumber = "CE00001" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "COC", Description = "Customs Office Code (Override)" }, ReferenceNumber = "CE00002" };
			var additionalReference3 = new AdditionalReference { Type = new EntryType { Code = "UBR", Description = "Under Bond Approval Reference Number" }, ReferenceNumber = "CE00003" };

			var consolDataObject = SetupConsol();
			consolDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			consolDataObject.AdditionalReferenceCollection.Add(additionalReference1);
			consolDataObject.AdditionalReferenceCollection.Add(additionalReference2);
			consolDataObject.AdditionalReferenceCollection.Add(additionalReference3);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.CodesMappedToTarget = true;

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.Numbers.Count", 3, consolBO.Numbers.Count);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolThroughAgentsReference()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_AgentsReference = "007";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.AgentsReference = "007";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.PK", consolBOToLoad.PK, consolBO.PK);
				AssertEquals("consolBO.JK_MasterBillNum", "08111111214", consolBO.JK_MasterBillNum);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolThroughCarriersBookingReference()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOKME";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOKME, DONOBOOKME";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.JK_BookingReference", "BOOKME", consolBO.JK_BookingReference);
				AssertEquals("consolBO.Numbers.GetFirstReferenceNumberByType(BKG)", "DONOBOOKME", consolBO.Numbers.GetFirstReferenceNumberByType("BKG").CE_EntryNum);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolWithMultipleCarriersBookingReference()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "First;Second First,Third:Fourth";
			consolDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = "BKG"
					},
					ReferenceNumber = "Third"
				},
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = "BKG"
					},
					ReferenceNumber = "Fifth"
				}
			});
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.JK_BookingReference", "First", consolBO.JK_BookingReference);
				AssertContainsExactElementsInAnyOrder("consolBO.Numbers.GetAllReferenceNumbersByType(BKG)", new string[] { "Second", "Third", "Fourth", "Fifth" }, consolBO.Numbers.GetAllReferenceNumbersByType("BKG"));
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestEmptyLegCollectionDoesntGoIntoEndlessLoop()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.TransportLegCollection.Clear();
			consolDataObject.TransportLegCollection.Content = CollectionContent.Partial;

			var logger = new TestErrorLogger();
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			consolBO.JK_RL_NKLoadPort = "NZCHC";
			consolBO.JK_RL_NKDischargePort = "USCHI";
			var leg1 = consolBO.Transports[0];
			leg1.JW_RL_NKLoadPort = "NZCHC";
			leg1.JW_RL_NKDiscPort = "AUBNE";
			var leg2 = consolBO.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "AUBNE";
			leg2.JW_RL_NKDiscPort = "USCHI";
			AssertEquals("Precondition: consolBO.Transports.Count", 2, consolBO.Transports.Count);

			logger = new TestErrorLogger();
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var updatedConsolBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Successfully loaded matching ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);

				AssertEquals("consolBO.Transports.Count", 2, updatedConsolBO.Transports.Count);
				leg1 = updatedConsolBO.Transports[0];
				AssertEquals("leg1.JW_RL_NKLoadPort", "NZCHC", leg1.JW_RL_NKLoadPort);
				AssertEquals("leg1.JW_RL_NKDiscPort", "AUBNE", leg1.JW_RL_NKDiscPort);
				leg2 = updatedConsolBO.Transports[1];
				AssertEquals("leg2.JW_RL_NKLoadPort", "AUBNE", leg2.JW_RL_NKLoadPort);
				AssertEquals("leg2.JW_RL_NKDiscPort", "USCHI", leg2.JW_RL_NKDiscPort);
			});
		}

		public void TestWorkflowCustomFieldsOnConsolAreImported()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "CON";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			consolDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO")));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are customary", new ZString("GOODBYE")));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last Date", new ZDateTime(2011, 1, 2)));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			consolDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			consolDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = consolBO.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Custom Field 1 not found", customFieldsString.Contains("Deci Deca - 0.3"));
				Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - 01-Jan-11 00:00:00"));
				Assert("Custom Field 3 not found", customFieldsString.Contains("Flagger - Y"));
				Assert("Custom Field 4 not found", customFieldsString.Contains("Integer Mate - 42"));
				Assert("Custom Field 5 not found", customFieldsString.Contains("Textual context - HELLO"));

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestAdditionalReferenceNumbers()
		{
			var consolDataObject = SetupConsol();

			consolDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			var additionalReferenceNumberDataObject = AdditionalReferenceDataObjectReaderTest.SetupAdditionalReference();

			consolDataObject.AdditionalReferenceCollection.Add(additionalReferenceNumberDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Numbers.Count", 1, consolBO.Numbers.Count);

				var additionalReferenceNumberBO = consolBO.Numbers[0];
				AdditionalReferenceDataObjectReaderTest.AssertContents(additionalReferenceNumberBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			var newLogger = new TestErrorLogger();
			reader = new ConsolDataObjectReader(consolDataObject, newLogger, Factory);
			consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Numbers.Count", 1, consolBO.Numbers.Count);

				var additionalReferenceNumberBO = consolBO.Numbers[0];
				AdditionalReferenceDataObjectReaderTest.AssertContents(additionalReferenceNumberBO);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Successfully loaded matching ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestCusEntryNumbers()
		{
			var consolDataObject = SetupConsol();

			consolDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());

			var entryNumberDataObject = EntryNumberDataObjectReaderTest.SetupEntryNumberDataObject();

			consolDataObject.EntryNumberCollection.Add(entryNumberDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.CusEntryNumsForAllCountries.Count", 1, consolBO.CusEntryNumsForAllCountries.Count);

				var entryNumberBO = consolBO.CusEntryNumsForAllCountries[0];
				EntryNumberDataObjectReaderTest.AssertContents(entryNumberBO, consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			var newLogger = new TestErrorLogger();
			reader = new ConsolDataObjectReader(consolDataObject, newLogger, Factory);
			consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.CusEntryNumsForAllCountries.Count", 1, consolBO.CusEntryNumsForAllCountries.Count);

				var entryNumberBO = consolBO.CusEntryNumsForAllCountries[0];
				EntryNumberDataObjectReaderTest.AssertContentsWithReadOnly(entryNumberBO, consolBO);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Successfully loaded matching ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestCarrierBookingOffice()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.CarrierBookingOffice = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Sea Freight" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("USLAX", consolBO.JK_RL_NKCarrierBookingOffice);

			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR", Description = "Air Freight" };
			consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNullOrEmpty(consolBO.JK_RL_NKCarrierBookingOffice);
		}

		public void TestNotPopulateCarrierBookingLatest_StatusAndDate()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.CarrierBookingLatestDate = new ZDateTime(2022, 02, 28);
			consolDataObject.CarrierBookingLatestStatus = new CodeDescriptionPair()
			{
				Code = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent,
				Description = FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.Sent
			};

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals(ZDateTime.Empty, consolBO.JK_Calc_CarrierBookingLatestDate);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consolBO.JK_Calc_CarrierBookingLatestStatus);

			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR", Description = "Air Freight" };
			consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals(ZDateTime.Empty, consolBO.JK_Calc_CarrierBookingLatestDate);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consolBO.JK_Calc_CarrierBookingLatestStatus);
		}

		public void TestReaderKeepsHyphenOnImportOfSeaConsol()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.WayBillNumber = "333-4444444";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("consolBO.JK_MasterBillNum", "333-4444444", consolBO.JK_MasterBillNum);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='333-4444444') from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestReaderIgnoresHyphenOnImportOfAirConsol()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.WayBillNumber = "333-44444444";
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR", Description = "Air Freight" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("consolBO.JK_MasterBillNum", "33344444444", consolBO.JK_MasterBillNum);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='33344444444') from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestBasicConsolLevelFieldMappings()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BookTheBook";
			consolDataObject.ElectronicBillOfLadingReference = "TestData_ElectronicBillOfLadingReference";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.JK_BookingReference", "BookTheBook", consolBO.JK_BookingReference);
				AssertEquals("consolBO.JK_JX_JV_NKVessel", "HighWind", consolBO.JK_JX_JV_NKVessel);
				AssertEquals("consolBO.JK_JX_JV_VoyageFlight", "QF253", consolBO.JK_JX_JV_VoyageFlight);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 0, consolBO.JK_Calc_ContainerCount);
				AssertEquals("consolBO.JK_TotalDocumentedChargeable", 0m, consolBO.JK_TotalDocumentedChargeable);
				AssertEquals("consolBO.JK_TotalDocumentedVolume", 0m, consolBO.JK_TotalDocumentedVolume);
				AssertEquals("consolBO.JK_TotalDocumentedWeight", 0m, consolBO.JK_TotalDocumentedWeight);
				AssertEquals("consolBO.JK_TotalManifestedChargeable", 0m, consolBO.JK_TotalManifestedChargeable);
				AssertEquals("consolBO.JK_TotalManifestedVolume", 0m, consolBO.JK_TotalManifestedVolume);
				AssertEquals("consolBO.JK_TotalManifestedWeight", 0m, consolBO.JK_TotalManifestedWeight);
				AssertEquals("consolBO.JK_TotalShipmentQuantity", 0m, consolBO.JK_TotalShipmentQuantity);
				AssertEquals("consolBO.JK_TotalShipmentPackageCount", 0m, consolBO.JK_TotalShipmentPackageCount);
				AssertEquals("consolBO.JK_TotalShipmentVolume", 0m, consolBO.JK_TotalShipmentVolume);
				AssertEquals("consolBO.JK_TotalShipmentVolumeUnit", "M3", consolBO.JK_TotalShipmentVolumeUnit);
				AssertEquals("consolBO.JK_TotalShipmentWeight", 0m, consolBO.JK_TotalShipmentWeight);
				AssertEquals("consolBO.JK_TotalShipmentWeightUnit", "KG", consolBO.JK_TotalShipmentWeightUnit);
				AssertEquals("consolBO.JK_RL_NKMasterBillIssuePlace", "NZAKL", consolBO.JK_RL_NKMasterBillIssuePlace);
				AssertEquals("consolBO.JK_ElectronicBillOfLadingReference", "TestData_ElectronicBillOfLadingReference", consolBO.JK_ElectronicBillOfLadingReference);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingConsolThroughMasterBillNumber()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_MasterBillNum = "08111111214";
			consolBOToLoad.JK_BookingReference = "ThisFieldWillNotBeTouched";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.WayBillType = new WayBillType() { Code = "MWB", Description = "Master Waybill" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.JK_BookingReference", "ThisFieldWillNotBeTouched", consolBO.JK_BookingReference);
				AssertContents(consolBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadingSubShipmentOnShipmentOnConsolThroughHouseBillNumber()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_MasterBillNum = "MYMASTER";
			var shipmentParentBO = Factory.New<ForwardingShipment>();

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";
			shipmentParentBO.CoLoadShipments.Add(shipmentBOToLoad);
			consolBOToLoad.GridShipments.Add(shipmentParentBO);

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.WayBillNumber = "MYMASTER";
			consolDataObject.WayBillType = new WayBillType() { Code = "MWB", Description = "Master Waybill" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			AssertEquals("consolBO.GridShipments.Count", 1, consolBO.GridShipments.Count);
			AssertEquals("consolBO.GridShipments[0].CoLoadShipments.Count", 1, consolBO.GridShipments[0].CoLoadShipments.Count);
			AssertEquals("consolBO.GridShipments[0].CoLoadShipments[0].JS_AdditionalTerms", "THIS FIELD WILL NOT BE TOUCHED", consolBO.GridShipments[0].CoLoadShipments[0].JS_AdditionalTerms);
		}

		public void TestWithOrgAddresses()
		{
			var consolDataObject = SetupConsol();
			var addressBO = new OrganisationDataObjectReader(consolDataObject.OrganizationAddressCollection[0], logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNotNull(consolBO.Creditor);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_OA_CreditorAddress", addressBO.PK, consolBO.JK_OA_CreditorAddress);
				AssertOrgAddressContents(consolBO.CreditorAddress);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'THECODE' address '' with a score of 360.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Matching 'Creditor':- Matched to 'THECODE' by code, address '' (only address).
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses_CoLoadWith()
		{
			var consolDataObject = SetupConsol("ConsolImportFileWithCoLoadWithOrgAddress.xml");
			AssertEquals(1, consolDataObject.OrganizationAddressCollection.Count);
			AssertEquals("CoLoadWith", consolDataObject.OrganizationAddressCollection[0].AddressType);

			var addressBO = new OrganisationDataObjectReader(consolDataObject.OrganizationAddressCollection[0], logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNotNull(consolBO.Creditor);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO, "CLD", true);
				AssertEquals("consolBO.JK_OA_CreditorAddress", addressBO.PK, consolBO.JK_OA_CreditorAddress);
				AssertOrgAddressContents(consolBO.CreditorAddress);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Matching 'CoLoadWith':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'THECODE' address '' with a score of 360.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Matching 'CoLoadWith':- Matched to 'THECODE' by code, address '' (only address).
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses_CoLoadWith_ForGatewayCoLoadConsol()
		{
			var consolDataObject = SetupConsol("ConsolImportFileWithGatewayCoLoadWithOrgAddress.xml");
			AssertEquals(1, consolDataObject.OrganizationAddressCollection.Count);
			AssertEquals("CoLoadWith", consolDataObject.OrganizationAddressCollection[0].AddressType);

			var addressBO = new OrganisationDataObjectReader(consolDataObject.OrganizationAddressCollection[0], logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNotNull(consolBO.Creditor);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO, "CLD", isCoLoad: true, isGatewayCoLoad: true);
				AssertEquals("consolBO.JK_OA_CreditorAddress", addressBO.PK, consolBO.JK_OA_CreditorAddress);
				AssertOrgAddressContents(consolBO.CreditorAddress);
				AssertMultilineASCIIEquals("logger.Logs", @"
		Warning - Matching 'CoLoadWith':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'THECODE' address '' with a score of 360.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Matching 'CoLoadWith':- Matched to 'THECODE' by code, address '' (only address).
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses_CreditorAndCoLoadWith()
		{
			var consolDataObject = SetupConsol("ConsolImportFileWithCreditorAndCoLoadWithOrgAddress.xml");
			AssertEquals(2, consolDataObject.OrganizationAddressCollection.Count);
			AssertEquals("Creditor", consolDataObject.OrganizationAddressCollection[0].AddressType);
			AssertEquals("CoLoadWith", consolDataObject.OrganizationAddressCollection[1].AddressType);

			var addressBO = new OrganisationDataObjectReader(consolDataObject.OrganizationAddressCollection[0], logger, Factory).GetMatchedOrNewForTesting();
			var addressBOCoLoadWith = new OrganisationDataObjectReader(consolDataObject.OrganizationAddressCollection[1], logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNotNull(consolBO.Creditor);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO, "CLD", true);
				AssertEquals("consolBO.JK_OA_CreditorAddress", addressBOCoLoadWith.PK, consolBO.JK_OA_CreditorAddress);
				AssertOrgAddressContents(consolBO.CreditorAddress);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Matching 'Creditor':- No match found for '[Org. Code: Code05; Company Name: COMPANY05; Address 1: STREET05; City: City05]'.
Warning - Matching 'CoLoadWith':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'THECODE' address '' with a score of 360.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Matching 'CoLoadWith':- Matched to 'THECODE' by code, address '' (only address).
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses_Remove()
		{
			#region Create test consolBO

			var consolDataObject = SetupConsol("ConsolImportFileWithAllOrgAddresses.xml");
			for (int i = 0; i < 12; ++i)
			{
				new OrganisationDataObjectReader(consolDataObject.OrganizationAddressCollection[i], logger, Factory).GetMatchedOrNewForTesting();
			}

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertNotNull(consolBO.ArrivalCTOAddress);
			AssertNotNull(consolBO.ArrivalUnpackCFSTransportAddress);
			AssertNotNull(consolBO.ContainerYardEmptyPickupAddress);
			AssertNotNull(consolBO.ContainerYardEmptyReturnAddress);
			AssertNotNull(consolBO.CreditorAddress);
			AssertNotNull(consolBO.DepartureCTOAddress);
			AssertNotNull(consolBO.DeparturePackCFSTransportAddress);
			AssertNotNull(consolBO.PackDepotAddress);
			AssertNotNull(consolBO.ReceivingForwarderAddress);
			AssertNotNull(consolBO.SendingForwarderAddress);
			AssertNotNull(consolBO.ShippingLineAddress);
			AssertNotNull(consolBO.UnpackDepotAddress);

			#endregion

			for (int i = 0; i < 12; ++i)
			{
				SetEmptyOrgAddress(consolDataObject.OrganizationAddressCollection[i]);
			}

			logger.ClearLogs();
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();

			AssertNull(consolBO.ArrivalCTOAddress);
			AssertNull(consolBO.ArrivalUnpackCFSTransportAddress);
			AssertNull(consolBO.ContainerYardEmptyPickupAddress);
			AssertNull(consolBO.ContainerYardEmptyReturnAddress);
			AssertNull(consolBO.CreditorAddress);
			AssertNull(consolBO.DepartureCTOAddress);
			AssertNull(consolBO.DeparturePackCFSTransportAddress);
			AssertNull(consolBO.PackDepotAddress);
			AssertNull(consolBO.ReceivingForwarderAddress);
			AssertNull(consolBO.SendingForwarderAddress);
			AssertNull(consolBO.ShippingLineAddress);
			AssertNull(consolBO.UnpackDepotAddress);

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Successfully loaded matching ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Set 'ArrivalCTOAddress' to Empty.
Information - Set 'ArrivalCFSLocalTransportAddress' to Empty.
Information - Set 'ContainerYardEmptyPickupAddress' to Empty.
Information - Set 'ContainerYardEmptyReturnAddress' to Empty.
Information - Set 'DepartureCTOAddress' to Empty.
Information - Set 'DepartureCFSLocalTransportAddress' to Empty.
Information - Set 'DepartureCFSAddress' to Empty.
Information - Set 'ReceivingForwarderAddress' to Empty.
Information - Set 'SendingForwarderAddress' to Empty.
Information - Set 'ShippingLineAddress' to Empty.
Information - Set 'ArrivalCFSAddress' to Empty.
Information - Matching 'CarrierBookingAgent':- Matched to 'Code12' address '' with a score of 175.
Information - Set 'Creditor' to Empty.
Information - Updated Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
		}

		void SetEmptyOrgAddress(IDataObject orgAddress)
		{
			SetEmptyValues(orgAddress, "AddressType", "AddressOverride");
		}

		void SetEmptyValues(IDataObject dataObject, params string[] exceptionPropertyNames)
		{
			foreach (var property in dataObject.GetType().GetProperties())
			{
				if (!exceptionPropertyNames.Contains(property.Name))
				{
					property.SetValue(dataObject, null);
				}
			}
		}

		public void TestShippingLineOrgAddressWithFallback()
		{
			var consol = GetConsolBOToLoad();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";

			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "WiseTech2";

			AssertNotNull("orgHeader2.MainAddress", orgHeader2.MainAddress);

			orgHeader2.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader2.MainAddress.Address1 = "Address1";
			orgHeader2.MainAddress.Address2 = "Address2";
			orgHeader2.MainAddress.City = "City";
			orgHeader2.MainAddress.State = "State";
			orgHeader2.MainAddress.Postcode = "Postcode";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolImportFileWithShippingLineAddressUnmatched.xml");

			AssertNotNull(consolDataObject);

			var unmatchedOrganisation = new UnmatchedOrganisation(orgHeader.Factory) { IsEnabled = false };

			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("No match found and fallback failed", Guid.Empty, consolBO.JK_OA_ShippingLineAddress);

				logger.ClearLogs();
			}

			var cusCode1 = orgHeader.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_CustomsRegNo = "CCCM";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;

			var cusCode2 = orgHeader2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "CCCM";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();

			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("No match found, but fallback to CCC successful", orgHeader2.MainAddress.PK, consolBO.JK_OA_ShippingLineAddress);

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - Matching 'ShippingLineAddress':- No match found for '[Company Name: NYK LOGISTICS (CHINA) CO., LTD.; Address 1: 20F/21F, RAFFLES CITY, NO.268]'.
Information - Updated Consol C00001052 (Master Bill='DAM') from UniversalShipment.
".Trim(), logger.Logs);
				});

				logger.ClearLogs();
			}

			consol.JK_OA_ShippingLineAddress = Guid.Empty;

			var cusCode3 = orgHeader.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			cusCode3.OK_CustomsRegNo = "C1CM";

			Factory.SaveForTesting();

			unmatchedOrganisation.IsEnabled = true;

			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("No match found, but fallback to C1C successful", orgHeader.MainAddress.PK, consolBO.JK_OA_ShippingLineAddress);

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Matching 'ShippingLineAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Updated Consol C00001052 (Master Bill='DAM') from UniversalShipment.
".Trim(), logger.Logs);
				});

				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNotNull(consolBO.JK_OA_ShippingLineAddress_ZAddress);
				AssertEquals("No match found – assigned to UNMATCHED organization", OrgHeader.UnmatchedOrganisationPK, consolBO.JK_OA_ShippingLineAddress_ZAddress.OrgPK);

				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNotNull(consolBO.JK_OA_ShippingLineAddress_ZAddress);
				AssertEquals("No match found, but fallback to C1C successful", orgHeader.MainAddress.PK, consolBO.JK_OA_ShippingLineAddress);
			}
		}

		public void TestCoLoadOrgAddressWithFallback()
		{
			var consol = GetConsolBOToLoad();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";

			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "WiseTech2";

			AssertNotNull("orgHeader2.MainAddress", orgHeader2.MainAddress);

			orgHeader2.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader2.MainAddress.Address1 = "Address1";
			orgHeader2.MainAddress.Address2 = "Address2";
			orgHeader2.MainAddress.City = "City";
			orgHeader2.MainAddress.State = "State";
			orgHeader2.MainAddress.Postcode = "Postcode";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolImportFileWithCoLoadWithUnmatched.xml");

			AssertNotNull(consolDataObject);

			var unmatchedOrganisation = new UnmatchedOrganisation(orgHeader.Factory) { IsEnabled = true };

			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				consol.JK_AgentType = Constants.AgentType.Agent;

				Factory.SaveForTesting();

				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNull("No match found and fallback failed - JK_AgentType missmatch", consolBO.CreditorAddress);

				consol.JK_AgentType = Constants.AgentType.CoLoad;

				var cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode.OK_CustomsRegNo = "C1CM";

				Factory.SaveForTesting();

				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNull("No match found and fallback failed - Custom Registration Number missmatch", consolBO.CreditorAddress);

				logger.ClearLogs();

				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;

				Factory.SaveForTesting();

				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNotNull(consolBO.CreditorAddress);
				AssertEquals("No match found, but fallback to C1C successful", orgHeader.MainAddress.PK, consolBO.CreditorAddress.PK);

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Matching 'CoLoadWith':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Updated Consol C00001052 (Master Bill='DAM') from UniversalShipment.
".Trim(), logger.Logs);
				});

				consol.JK_OA_CreditorAddress = orgHeader2.MainAddress.PK;

				Factory.SaveForTesting();

				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNotNull(consolBO.CreditorAddress);
				AssertEquals("No need to update JK_OA_CreditorAddress", orgHeader2.MainAddress.PK, consolBO.CreditorAddress.PK);
			}
		}

		public void TestWithContainers()
		{
			var consolDataObject = SetupConsol();

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Add(containerDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("consolBO.Containers.Count", 1, consolBO.Containers.Count);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 2, consolBO.JK_Calc_ContainerCount);
				ContainerDataObjectTestHelper.AssertContents(consolBO.Containers[0]);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithContainersCompleteUpdate()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_UniqueConsignRef = "C00001052";
			consolBOToLoad.JK_AgentsReference = "Agent U";
			consolBOToLoad.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBOToLoad.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container = consolBOToLoad.Containers.AddNew();
			container.JC_ContainerNum = "BMOU4398176";
			container.JC_SealNum = "123";
			container.JC_SealParty = "CUS";
			container.JC_ContainerMode = "FCF";
			container = consolBOToLoad.Containers.AddNew();
			container.JC_ContainerNum = "BMOU4398177";
			container.JC_SealNum = "456";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerNumber = "BMOU4398176";
			containerDataObject1.Seal = "666";
			containerDataObject1.SealPartyType = new CodeDescriptionPair { Code = "CAR" };

			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.ContainerNumber = "BMOU4398178";
			containerDataObject2.Seal = "888";
			containerDataObject2.SealPartyType = new CodeDescriptionPair { Code = "CRD" };

			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Complete;
			consolDataObject.ContainerCollection.Add(containerDataObject1);
			consolDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("consolBO.Containers.Count", 2, consolBO.Containers.Count);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 2, consolBO.JK_Calc_ContainerCount);
				AssertEquals("Containers 1 JC_ContainerNum", "BMOU4398176", consolBO.Containers[0].JC_ContainerNum);
				AssertEquals("Containers 2 JC_ContainerNum", "BMOU4398178", consolBO.Containers[1].JC_ContainerNum);
				AssertEquals("Containers 1 JC_SealNum", "666", consolBO.Containers[0].JC_SealNum);
				AssertEquals("Containers 2 JC_SealNum", "888", consolBO.Containers[1].JC_SealNum);
				AssertEquals("Containers 1 JC_SealParty", "CAR", consolBO.Containers[0].JC_SealParty);
				AssertEquals("Containers 2 JC_SealParty", "CRD", consolBO.Containers[1].JC_SealParty);
				AssertEquals("Containers 1 JC_ContainerMode", "FCF", consolBO.Containers[0].JC_ContainerMode);
			});

			#endregion
		}

		public void TestWithContainersPartialUpdate()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_UniqueConsignRef = "C00001052";
			consolBOToLoad.JK_AgentsReference = "Agent U";
			consolBOToLoad.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBOToLoad.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container = consolBOToLoad.Containers.AddNew();
			container.JC_ContainerNum = "BMOU4398176";
			container.JC_SealNum = "123";
			container.JC_ContainerMode = "FCF";
			container = consolBOToLoad.Containers.AddNew();
			container.JC_ContainerNum = "BMOU4398177";
			container.JC_SealNum = "456";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerNumber = "BMOU4398176";
			containerDataObject1.Seal = "666";
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.ContainerNumber = "BMOU4398178";
			containerDataObject2.Seal = "888";

			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Partial;
			consolDataObject.ContainerCollection.Add(containerDataObject1);
			consolDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("consolBO.Containers.Count", 3, consolBO.Containers.Count);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 3, consolBO.JK_Calc_ContainerCount);
				AssertEquals("", "BMOU4398176", consolBO.Containers[0].JC_ContainerNum);
				AssertEquals("", "BMOU4398177", consolBO.Containers[1].JC_ContainerNum);
				AssertEquals("", "BMOU4398178", consolBO.Containers[2].JC_ContainerNum);
				AssertEquals("", "666", consolBO.Containers[0].JC_SealNum);
				AssertEquals("", "456", consolBO.Containers[1].JC_SealNum);
				AssertEquals("", "888", consolBO.Containers[2].JC_SealNum);
				AssertEquals("", "FCF", consolBO.Containers[0].JC_ContainerMode);
			});

			#endregion
		}

		public void TestWithContainerAdditionalServicesPartialUpdate()
		{
			var container = PrepareConsolAndReadFileIntoBusinessObject("ConsolImportFileContainersWithPartialAdditionalServices.xml");

			AssertEquals(3, container.Services.Count);

			AssertEquals(Constants.FreightServiceType.Codes.Fumigation, container.Services[0].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Cleaning, container.Services[1].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Washing, container.Services[2].ES_ServiceCode);
		}

		public void TestWithContainerAdditionalServicesCompleteUpdate()
		{
			var container = PrepareConsolAndReadFileIntoBusinessObject("ConsolImportFileContainersWithCompleteAdditionalServices.xml");

			AssertEquals(2, container.Services.Count);

			AssertEquals(Constants.FreightServiceType.Codes.Fumigation, container.Services[0].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Washing, container.Services[1].ES_ServiceCode);
		}

		public void TestWithContainerAdditionalServicesWithUndefinedCollectionTypeUpdate()
		{
			var container = PrepareConsolAndReadFileIntoBusinessObject("ConsolImportFileContainersWithUndefinedContentTypeAdditionalServices.xml");

			AssertEquals(3, container.Services.Count);

			AssertEquals(Constants.FreightServiceType.Codes.Fumigation, container.Services[0].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Cleaning, container.Services[1].ES_ServiceCode);
			AssertEquals(Constants.FreightServiceType.Codes.Washing, container.Services[2].ES_ServiceCode);
		}

		ForwardingContainer PrepareConsolAndReadFileIntoBusinessObject(string fileName)
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_MasterBillNum = "08111111214";
			consolInDB.JK_BookingReference = "BookingRefrence";

			var containerInDB = consolInDB.Containers.AddNew();
			containerInDB.JC_ContainerNum = "DFSU6004840";

			var fumService = containerInDB.Services.AddNew();
			fumService.ShouldPopulateServiceId = false;
			fumService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			var clnService = containerInDB.Services.AddNew();
			clnService.ShouldPopulateServiceId = false;
			clnService.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol(fileName);
			consolDataObject.WayBillNumber = consolInDB.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = consolInDB.JK_BookingReference;
			consolDataObject.ShipmentType = new CodeDescriptionPair() { Code = Constants.AgentType.CoLoad, Description = "Co-Load" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertEquals(1, consolBO.Containers.Count);

			var container = consolBO.Containers[0];
			return container;
		}

		public void TestEmptyContainerNumber()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_UniqueConsignRef = "C00001052";
			consolBOToLoad.JK_AgentsReference = "Agent U";
			consolBOToLoad.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBOToLoad.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container = consolBOToLoad.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 5;

			Factory.SaveForTesting();

			var containerDataObject = new Container
			{
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10003333",
			};

			var consolDataObject = SetupConsol();
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Sea" };
			consolDataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };
			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Partial;
			consolDataObject.ContainerCollection.Add(containerDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Containers.Count", 1, consolBO.Containers.Count);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 5, consolBO.JK_Calc_ContainerCount);

				var containerBO = consolBO.Containers[0];
				AssertEquals("containerBO.JC_ContainerNum", "", containerBO.JC_ContainerNum);
				AssertEquals("containerBO.JC_ContainerCount", new ZShort(5), containerBO.JC_ContainerCount);
				AssertNotNull("containerBO.RefContainer", containerBO.RefContainer);
				AssertEquals("containerBO.RefContainer.RC_Code", "20GP", containerBO.RefContainer.RC_Code);
				AssertEquals("containerBO.JC_ContainerMode", "FCL", containerBO.JC_ContainerMode);
				AssertEquals("containerBO.JC_ReleaseNum", "R10003333", containerBO.JC_ReleaseNum);

				AssertContains("Information - Successfully loaded matching ForwardingContainer.", logger.Logs);
			});
		}

		public void TestEmptyContainerNumber_MultipleMatchingContainersInXML()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBOToLoad.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBOToLoad.JK_UniqueConsignRef = "C00001052";
			consolBOToLoad.JK_AgentsReference = "Agent U";

			var container = consolBOToLoad.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 5;

			Factory.SaveForTesting();

			var containerDataObject1 = new Container
			{
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10003333",
			};

			var containerDataObject2 = new Container
			{
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10005555",
			};

			var consolDataObject = SetupConsol();
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Sea" };
			consolDataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };
			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Partial;
			consolDataObject.ContainerCollection.Add(containerDataObject1);
			consolDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Containers.Count", 3, consolBO.Containers.Count);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 15, consolBO.JK_Calc_ContainerCount);

				var containerBO1 = consolBO.Containers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ContainerCount", new ZShort(5), containerBO1.JC_ContainerCount);
				AssertNotNull("containerBO1.RefContainer", containerBO1.RefContainer);
				AssertEquals("containerBO1.RefContainer.RC_Code", "20GP", containerBO1.RefContainer.RC_Code);
				AssertEquals("containerBO1.JC_ContainerMode", "FCL", containerBO1.JC_ContainerMode);
				AssertEquals("containerBO1.JC_ReleaseNum", "", containerBO1.JC_ReleaseNum);

				var containerBO2 = consolBO.Containers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ContainerCount", new ZShort(5), containerBO2.JC_ContainerCount);
				AssertNotNull("containerBO2.RefContainer", containerBO2.RefContainer);
				AssertEquals("containerBO2.RefContainer.RC_Code", "20GP", containerBO2.RefContainer.RC_Code);
				AssertEquals("containerBO2.JC_ContainerMode", "FCL", containerBO2.JC_ContainerMode);
				AssertEquals("containerBO2.JC_ReleaseNum", "R10003333", containerBO2.JC_ReleaseNum);

				var containerBO3 = consolBO.Containers[2];
				AssertEquals("containerBO3.JC_ContainerNum", "", containerBO3.JC_ContainerNum);
				AssertEquals("containerBO3.JC_ContainerCount", new ZShort(5), containerBO3.JC_ContainerCount);
				AssertNotNull("containerBO3.RefContainer", containerBO3.RefContainer);
				AssertEquals("containerBO3.RefContainer.RC_Code", "20GP", containerBO3.RefContainer.RC_Code);
				AssertEquals("containerBO3.JC_ContainerMode", "FCL", containerBO3.JC_ContainerMode);
				AssertEquals("containerBO3.JC_ReleaseNum", "R10005555", containerBO3.JC_ReleaseNum);

				AssertContains("20GP container x 2 should be added", @"Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type.
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type."
.Trim(), logger.Logs);
			});
		}

		public void TestEmptyContainerNumber_MultipleMatchingContainersOnConsol()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBOToLoad.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBOToLoad.JK_UniqueConsignRef = "C00001052";
			consolBOToLoad.JK_AgentsReference = "Agent U";

			var container1 = consolBOToLoad.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 5;

			var container2 = consolBOToLoad.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_ContainerCount = 5;

			Factory.SaveForTesting();

			var containerDataObject = new Container
			{
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10003333",
			};

			var consolDataObject = SetupConsol();
			consolDataObject.TransportMode = new CodeDescriptionPair { Code = "Sea" };
			consolDataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };
			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Partial;
			consolDataObject.ContainerCollection.Add(containerDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Containers.Count", 3, consolBO.Containers.Count);
				AssertEquals("consolBO.JK_Calc_ContainerCount", 15, consolBO.JK_Calc_ContainerCount);

				var containerBO1 = consolBO.Containers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ContainerCount", new ZShort(5), containerBO1.JC_ContainerCount);
				AssertNotNull("containerBO1.RefContainer", containerBO1.RefContainer);
				AssertEquals("containerBO1.RefContainer.RC_Code", "20GP", containerBO1.RefContainer.RC_Code);
				AssertEquals("containerBO1.JC_ContainerMode", "FCL", containerBO1.JC_ContainerMode);
				AssertEquals("containerBO1.JC_ReleaseNum", "", containerBO1.JC_ReleaseNum);

				var containerBO2 = consolBO.Containers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ContainerCount", new ZShort(5), containerBO2.JC_ContainerCount);
				AssertNotNull("containerBO2.RefContainer", containerBO2.RefContainer);
				AssertEquals("containerBO2.RefContainer.RC_Code", "20GP", containerBO2.RefContainer.RC_Code);
				AssertEquals("containerBO2.JC_ContainerMode", "FCL", containerBO2.JC_ContainerMode);
				AssertEquals("containerBO2.JC_ReleaseNum", "", containerBO2.JC_ReleaseNum);

				var containerBO3 = consolBO.Containers[2];
				AssertEquals("containerBO3.JC_ContainerNum", "", containerBO3.JC_ContainerNum);
				AssertEquals("containerBO3.JC_ContainerCount", new ZShort(5), containerBO3.JC_ContainerCount);
				AssertNotNull("containerBO3.RefContainer", containerBO3.RefContainer);
				AssertEquals("containerBO3.RefContainer.RC_Code", "20GP", containerBO3.RefContainer.RC_Code);
				AssertEquals("containerBO3.JC_ContainerMode", "FCL", containerBO3.JC_ContainerMode);
				AssertEquals("containerBO3.JC_ReleaseNum", "R10003333", containerBO3.JC_ReleaseNum);

				AssertContains("Information - No matching ForwardingContainer found, creating new ForwardingContainer.", logger.Logs);
			});
		}

		public void TestWithLegs()
		{
			var consolDataObject = SetupConsol();

			var transportLegDataObject = TransportLegDataObjectReaderTest.SetupTransportLeg();
			transportLegDataObject.LegOrder = 2;
			transportLegDataObject.VesselName = "BUNGA DELIMA";

			consolDataObject.TransportLegCollection.Add(transportLegDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			AssertEquals("consolBO.Transports.Count", 2, consolBO.Transports.Count);

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_JX_JV_NKVessel", "BUNGA DELIMA", consolBO.JK_JX_JV_NKVessel);
				AssertEquals("consolBO.JK_JX_JV_VoyageFlight", "343L", consolBO.JK_JX_JV_VoyageFlight);
				AssertEquals("transportBO1.JW_Vessel", "HighWind", consolBO.Transports[0].JW_Vessel);
				TransportLegDataObjectReaderTest.AssertContents(consolBO.Transports[1]);
				AssertEquals("transportBO2.JW_Vessel", "BUNGA DELIMA", consolBO.Transports[1].JW_Vessel);
				AssertEquals("transportBO2 carrier", ZGuid.Empty, consolBO.Transports[1].JW_OA_CarrierAddress);
				AssertEquals("transportBO2 should have no sailing schedule as there is no carrier", ZGuid.Empty, consolBO.Transports[1].JW_JX);
				AssertEquals("transportBO2 should not be linked as we don't have a valid carrier", false, consolBO.Transports[1].JW_IsLinked);
				AssertEquals("transportBO2 is cargo only", true, consolBO.Transports[1].JW_IsCargoOnly);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithNotes()
		{
			var consolDataObject = SetupConsol();
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			StmNote[] note = consolBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				ShipmentDataObjectReaderTest.AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithSubShipments()
		{
			var consolDataObject = SetupConsol();

			var subShipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentDataObject.WayBillNumber = "SUBSHIPMENT1111";
			subShipmentDataObject.PortOfOrigin = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };

			var shipmentDataObject = ShipmentDataObjectReaderTest.SetupShipment();
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipmentDataObject);

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolDataObject.SubShipmentCollection.Add(shipmentDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals("consolBO.GridShipments.Count", 1, consolBO.GridShipments.Count);
			AssertEquals("consolBO.GridShipments[0].CoLoadShipments.Count", 1, consolBO.GridShipments[0].CoLoadShipments.Count);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_TotalDocumentedChargeable", 4560m, consolBO.JK_TotalDocumentedChargeable);
				AssertEquals("consolBO.JK_TotalDocumentedVolume", 3.45m, consolBO.JK_TotalDocumentedVolume);
				AssertEquals("consolBO.JK_TotalDocumentedWeight", 4560000m, consolBO.JK_TotalDocumentedWeight);
				AssertEquals("consolBO.JK_TotalManifestedChargeable", 9010m, consolBO.JK_TotalManifestedChargeable);
				AssertEquals("consolBO.JK_TotalManifestedVolume", 8.9m, consolBO.JK_TotalManifestedVolume);
				AssertEquals("consolBO.JK_TotalManifestedWeight", 9010000m, consolBO.JK_TotalManifestedWeight);
				AssertEquals("consolBO.JK_TotalShipmentQuantity", 44m, consolBO.JK_TotalShipmentQuantity);
				AssertEquals("consolBO.JK_TotalShipmentPackageCount", 45m, consolBO.JK_TotalShipmentPackageCount);
				AssertEquals("consolBO.JK_TotalShipmentVolume", 23.45m, consolBO.JK_TotalShipmentVolume);
				AssertEquals("consolBO.JK_TotalShipmentVolumeUnit", "CF", consolBO.JK_TotalShipmentVolumeUnit);
				AssertEquals("consolBO.JK_TotalShipmentWeight", 34560000m, consolBO.JK_TotalShipmentWeight);
				AssertEquals("consolBO.JK_TotalShipmentWeightUnit", "KG", consolBO.JK_TotalShipmentWeightUnit);

				var shipmentBO = consolBO.GridShipments[0];
				ShipmentDataObjectReaderTest.AssertContents(shipmentBO);

				var subShipmentBO = shipmentBO.CoLoadShipments[0];
				AssertEquals("consolBO.GridShipments[0].CoLoadShipments[0].JS_HouseBill", "SUBSHIPMENT1111", subShipmentBO.JS_HouseBill);
				AssertEquals("consolBO.GridShipments[0].CoLoadShipments[0].JS_RL_NKOrigin", "USLAX", subShipmentBO.JS_RL_NKOrigin);

				AssertMultilineASCIIEquals("logger.Logs", string.Format(@"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment (House Bill='SUBSHIPMENT1111') from UniversalShipment.
Information - Added Shipment from UniversalShipment.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
", subShipmentBO.JS_UniqueConsignRef, shipmentBO.JS_UniqueConsignRef).Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithDangerousGoods()
		{
			var consolDataObject = SetupConsol("ConsolImportFileWithDangerousGoods.xml");

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull("consolBO", consolBO);
			AssertNotNull("consolBO.ConsolRGRestrictionCollection", consolBO.ConsolDGRestrictionCollection);
			AssertEquals("consolBO.ConsolDGRestriction.Count", 3, consolBO.ConsolDGRestrictionCollection.Count);

			CombineAssertions(delegate
			{
				AssertContents(consolBO);
				AssertEquals("consolBO.JK_IsHazardous", true, consolBO.JK_IsHazardous);
				AssertEquals("consolBO.ConsolDGRestriction[0].JKD_Class", "2.1", consolBO.ConsolDGRestrictionCollection[0].JKD_Class);
				AssertEquals("consolBO.ConsolDGRestriction[0].JKD_Calc_Substance", "1001", consolBO.ConsolDGRestrictionCollection[0].JKD_Calc_Substance);
				AssertEquals("consolBO.ConsolDGRestriction[1].JKD_Class", "2.2", consolBO.ConsolDGRestrictionCollection[1].JKD_Class);
				AssertEquals("consolBO.ConsolDGRestriction[1].JKD_Calc_Substance", "1002", consolBO.ConsolDGRestrictionCollection[1].JKD_Calc_Substance);
				AssertEquals("consolBO.ConsolDGRestriction[2].JKD_Class", "2.3", consolBO.ConsolDGRestrictionCollection[2].JKD_Class);
				AssertEquals("consolBO.ConsolDGRestriction[2].JKD_Calc_Substance", "1003A", consolBO.ConsolDGRestrictionCollection[2].JKD_Calc_Substance);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ConsolDGRestrictions found, creating new ConsolDGRestrictions.
Information - Populating ConsolDGRestrictions...
Information - No matching ConsolDGRestrictions found, creating new ConsolDGRestrictions.
Information - Populating ConsolDGRestrictions...
Information - No matching ConsolDGRestrictions found, creating new ConsolDGRestrictions.
Information - Populating ConsolDGRestrictions...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment."
					, logger.Logs);
			});
		}

		public void TestConsolidateBooking_CreateNewConsol()
		{
			var bookingBO = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container1 = (CommonContainer)bookingBO.QuotedBookingContainers.AddNew();
			container1.JC_ContainerNum = "GLMR1230001";
			container1.JC_RC = ref20GP.PK;

			var container2 = (CommonContainer)bookingBO.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = ref40GP.PK;

			var shipmentBOToLoad = bookingBO.ForwardingShipment as ForwardingShipment;

			shipmentBOToLoad.JS_UniqueConsignRef = "S00001011";
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerCount = 1;
			containerDataObject1.ContainerNumber = "GLMR1230001";
			containerDataObject1.ContainerType = new ContainerType { Code = "20GP" };

			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.ContainerCount = 1;
			containerDataObject2.ContainerNumber = "GLMR1230002";
			containerDataObject2.ContainerType = new ContainerType { Code = "40GP" };

			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Partial;
			consolDataObject.ContainerCollection.Add(containerDataObject1);
			consolDataObject.ContainerCollection.Add(containerDataObject2);

			var shipmentDataObject = ShipmentDataObjectReaderTest.SetupShipment();
			var context = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			context.DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
			{
				new UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget()
				{
					Type = nameof(DataContextType.ForwardingShipment),
					Key = "S00001011"
				}
			};

			shipmentDataObject.DataContext = context;
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolDataObject.SubShipmentCollection.Add(shipmentDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);

				AssertEquals(3, consolBO.Containers.Count);

				AssertEquals("GLMR1230001", consolBO.Containers[0].JC_ContainerNum);
				AssertEquals(1, consolBO.Containers[0].JC_Calc_ContainerCount);
				AssertEquals(ref20GP.PK, consolBO.Containers[0].JC_RC);

				AssertEquals(string.Empty, consolBO.Containers[1].JC_ContainerNum);
				AssertEquals(6, consolBO.Containers[1].JC_Calc_ContainerCount);
				AssertEquals(ref40GP.PK, consolBO.Containers[1].JC_RC);

				AssertEquals("GLMR1230002", consolBO.Containers[2].JC_ContainerNum);
				AssertEquals(1, consolBO.Containers[2].JC_Calc_ContainerCount);
				AssertEquals(ref40GP.PK, consolBO.Containers[2].JC_RC);

				AssertEquals("consolBO.Shipments.Count", 1, consolBO.Shipments.Count);
				var subShipmentBO = consolBO.Shipments[0];
				AssertEquals("consolBO.Shipments[0].PK", shipmentBOToLoad.PK, subShipmentBO.PK);
				AssertEquals("consolBO.Shipments[0].JS_HouseBill", "MYHOUSE", subShipmentBO.JS_HouseBill);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: GLMR1230002.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001011 (House Bill='MYHOUSE') from UniversalShipment.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestConsolidateBooking_MatchExistConsol()
		{
			var bookingBO = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container1 = (CommonContainer)bookingBO.QuotedBookingContainers.AddNew();
			container1.JC_ContainerNum = "GLMR1230001";
			container1.JC_RC = ref20GP.PK;

			var container2 = (CommonContainer)bookingBO.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = ref40GP.PK;

			var shipmentBOToLoad = bookingBO.ForwardingShipment as ForwardingShipment;

			shipmentBOToLoad.JS_UniqueConsignRef = "S00001011";
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_BookingReference = "BOOK ME";
			consolBOToLoad.JK_RL_NKDischargePort = "AUSYD";

			Factory.SaveForTesting();

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOK ME";

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerCount = 1;
			containerDataObject1.ContainerNumber = "GLMR1230001";
			containerDataObject1.ContainerType = new ContainerType { Code = "20GP" };

			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.ContainerCount = 1;
			containerDataObject2.ContainerNumber = "GLMR1230002";
			containerDataObject2.ContainerType = new ContainerType { Code = "40GP" };

			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Content = CollectionContent.Partial;
			consolDataObject.ContainerCollection.Add(containerDataObject1);
			consolDataObject.ContainerCollection.Add(containerDataObject2);

			var shipmentDataObject = ShipmentDataObjectReaderTest.SetupShipment();
			var context = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			context.DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
			{
				new UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget()
				{
					Type = nameof(DataContextType.ForwardingShipment),
					Key = "S00001011"
				}
			};

			shipmentDataObject.DataContext = context;
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolDataObject.SubShipmentCollection.Add(shipmentDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);

			#region Check Contents of Consol Business Object

			CombineAssertions(delegate
			{
				AssertContents(consolBO);

				AssertEquals(3, consolBO.Containers.Count);

				AssertEquals("GLMR1230001", consolBO.Containers[0].JC_ContainerNum);
				AssertEquals(1, consolBO.Containers[0].JC_Calc_ContainerCount);
				AssertEquals(ref20GP.PK, consolBO.Containers[0].JC_RC);

				AssertEquals(string.Empty, consolBO.Containers[1].JC_ContainerNum);
				AssertEquals(6, consolBO.Containers[1].JC_Calc_ContainerCount);
				AssertEquals(ref40GP.PK, consolBO.Containers[1].JC_RC);

				AssertEquals("GLMR1230002", consolBO.Containers[2].JC_ContainerNum);
				AssertEquals(1, consolBO.Containers[2].JC_Calc_ContainerCount);
				AssertEquals(ref40GP.PK, consolBO.Containers[2].JC_RC);

				AssertEquals("consolBO.Shipments.Count", 1, consolBO.Shipments.Count);
				var subShipmentBO = consolBO.Shipments[0];
				AssertEquals("consolBO.Shipments[0].PK", shipmentBOToLoad.PK, subShipmentBO.PK);
				AssertEquals("consolBO.Shipments[0].JS_HouseBill", "MYHOUSE", subShipmentBO.JS_HouseBill);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Information - Successfully loaded matching Container Type.
Information - Container number advised: GLMR1230002.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001011 (House Bill='MYHOUSE') from UniversalShipment.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Updated Consol C00001000 (Master Bill='08111111214') from UniversalShipment.".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestBookingWasConvertedAndAttachedToConsol_ProcessTasks()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var bookingWorkflowProvider = (IWorkflowProvider)quotedBooking;
			var shipmentBOToLoad = quotedBooking.ForwardingShipment as ForwardingShipment;

			shipmentBOToLoad.JS_UniqueConsignRef = "S00001000";
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var bookingMilestone = bookingWorkflowProvider.WorkflowItems.Milestones.AddNew();
			var bookingTrigger = bookingWorkflowProvider.WorkflowItems.Triggers.AddNew();
			var bookingOpenTask = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			var bookingWorkingTask = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			var bookingException = bookingWorkflowProvider.WorkflowItems.Exceptions.AddNew();

			bookingOpenTask.P9_Status = "OPN";
			bookingWorkingTask.P9_Status = "WRK";

			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_ConsolMode = "BCN";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.BookingConfirmationReference = "BOOK ME";

			var shipmentDataObject = ShipmentDataObjectReaderTest.SetupShipment();
			var context = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			context.DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
			{
				new UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget()
				{
					Type = nameof(DataContextType.ForwardingShipment),
					Key = "S00001000"
				}
			};

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolDataObject.SubShipmentCollection.Add(shipmentDataObject);

			shipmentDataObject.DataContext = context;
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, bookingMilestone.TriggerConditions.TriggerCondition);
			AssertEquals("false", bookingMilestone.TriggerConditions.TriggerConditionValue);

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, bookingTrigger.TriggerConditions.TriggerCondition);
			AssertEquals("false", bookingTrigger.TriggerConditions.TriggerConditionValue);

			AssertEquals("CAN", bookingOpenTask.P9_Status);
			AssertEquals("CLS", bookingWorkingTask.P9_Status);
			AssertEquals("RSL", bookingException.P9_Status);
		}

		public void TestIsLinkedValueOfTransportLeg()
		{
			UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, "HighWind", "QF253");

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(1, consolBO.Transports.Count);
			AssertEquals(true, consolBO.Transports[0].JW_IsLinked);
		}

		public void TestIsLinkedValueOfTransportLeg2()
		{
			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(1, consolBO.Transports.Count);
			AssertEquals(true, consolBO.Transports[0].JW_IsLinked);
		}

		public void TestIsLinkedValueOfTransportLeg3()
		{
			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO1 = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(1, consolBO1.Transports.Count);
			AssertEquals(true, consolBO1.Transports[0].JW_IsLinked);

			consolBO1.Transports[0].JW_IsLinked = false;

			Factory.SaveForTesting();

			AssertEquals("prerequisite", false, consolBO1.Transports[0].JW_IsLinked);

			var consolBO2 = reader.ReadIntoBusinessObject();

			AssertEquals(consolBO1, consolBO2);
			AssertEquals(1, consolBO1.Transports.Count);
			AssertEquals(false, consolBO1.Transports[0].JW_IsLinked);
		}

		public void TestImportConsolWithLinkedTransportLegUnderDifferentBranchThanScheduleCreation()
		{
			ZGuid sailingPK;
			ZGuid voyagePK;
			ZGuid originPK;
			ZGuid destinationPK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var voyage = Factory.New<JobVoyage>();

				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "TG0461";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "CNPEK";
				voyage.Origins[0].JA_E_DEP = new ZDateTime(2018, 10, 1, 17, 5, 0);
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
				voyage.Destinations[0].JB_E_ARV = new ZDateTime(2018, 10, 2, 20, 0, 0);
				voyage.GenerateSailings();

				voyagePK = voyage.PK;
				sailingPK = voyage.Sailings[0].PK;
				originPK = voyage.Origins[0].PK;
				destinationPK = voyage.Destinations[0].PK;

				AssertEquals(1, voyage.Sailings.Count);
				AssertEquals(1, voyage.Origins.Count);
				AssertEquals(1, voyage.Destinations.Count);

				Factory.SaveForTesting();
			}

			using (SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var transportLegDataObject = new TransportLeg
				{
					LegOrder = 1,
					TransportMode = TransportMode.Air,
					LegType = UniversalDataBuss.DataObjects.Universal.LegType.Main,
					PortOfLoading = new UNLOCO() { Code = "VNHAN", Name = "Hanoi" },
					PortOfDischarge = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" },
					VoyageFlightNo = "TG0461",
					EstimatedDeparture = new ZDateTime(2018, 9, 28, 20, 45, 0),
					EstimatedArrival = new ZDateTime(2018, 9, 30, 20, 0, 0),
					IsCargoOnly = true
				};

				var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransportMode = new CodeDescriptionPair { Code = "Air" },
					WayBillNumber = "081-11111214",
					WayBillType = new WayBillType { Code = "MWB" },
					PortOfLoading = new UNLOCO { Code = "VNHAN" },
					PortOfDischarge = new UNLOCO { Code = "AUMEL" }
				};

				consolDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
				consolDataObject.TransportLegCollection.Add(transportLegDataObject);

				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertEquals("Consol has one transport", 1, consolBO.Transports.Count);
				Assert("It's linked to a new voyage", consolBO.Transports[0].Voyage.PK != voyagePK);
				Assert("It's linked to a new sailing", consolBO.Transports[0].Sailing.PK != sailingPK);
				Assert("It's linked to a new origin", consolBO.Transports[0].Sailing.Origin.PK != originPK);
				Assert("It's linked to a new destination", consolBO.Transports[0].Sailing.Destination.PK != destinationPK);

				AssertEquals("Transport ETD", new ZDateTime(2018, 9, 28, 20, 45, 0), consolBO.Transports[0].JW_ETD);
				AssertEquals("Transport ETA", new ZDateTime(2018, 9, 30, 20, 0, 0), consolBO.Transports[0].JW_ETA);
				AssertEquals("Voyage Flight Date", new ZDateTime(2018, 9, 30, 20, 0, 0), consolBO.Transports[0].Voyage.JV_FlightDate);
				AssertEquals("Origin ETD", new ZDateTime(2018, 9, 28, 20, 45, 0), consolBO.Transports[0].Sailing.Origin.JA_E_DEP);
				AssertEquals("Destination ETA", new ZDateTime(2018, 9, 30, 20, 0, 0), consolBO.Transports[0].Sailing.Destination.JB_E_ARV);
			}
		}

		public void TestImportColoadConsolidationDoNotAddNewConsolidationIfBookingRefrenceIsDifferent()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_MasterBillNum = "08111111214";
			consolInDB.JK_BookingReference = "BookingRefrence";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.WayBillNumber = consolInDB.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = consolInDB.JK_BookingReference;
			consolDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.CoLoad, Description = "Co-Load" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var readConsolBO = reader.ReadIntoBusinessObject();

			AssertEquals(consolInDB.PK, readConsolBO.PK);

			consolDataObject.BookingConfirmationReference = consolInDB.JK_BookingReference + "Extra";
			consolDataObject.AgentsReference = consolInDB.JK_AgentsReference + "Extra";

			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			readConsolBO = reader.ReadIntoBusinessObject();

			AssertEquals(consolInDB.PK, readConsolBO.PK);
		}

		public void TestImportConsol_WhenSendingForwarderHandlingTypeIsInvalid_ThenIgnoreAndLog()
		{
			var consolDataObject = SetupConsol();

			var invalidForwarderHandlingType = new CodeDescriptionPair() { Code = "SND" };
			consolDataObject.SendingForwarderHandlingType = invalidForwarderHandlingType;

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertNotNull(consolBO);
			AssertEquals(ZString.Empty, consolBO.JK_SendingForwarderHandlingType);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - JK_SendingForwarderHandlingType SND is invalid.
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestImportConsol_WhenReceivingForwarderHandlingTypeIsInvalid_ThenIgnoreAndLog()
		{
			var consolDataObject = SetupConsol();

			var invalidForwarderHandlingType = new CodeDescriptionPair() { Code = "RCV" };
			consolDataObject.ReceivingForwarderHandlingType = invalidForwarderHandlingType;

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertNotNull(consolBO);
			AssertEquals(ZString.Empty, consolBO.JK_ReceivingForwarderHandlingType);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - JK_ReceivingForwarderHandlingType RCV is invalid.
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestImportConsol_WhenForwarderHandlingTypeIsEmpty_ThenIgnoreAndLog()
		{
			var consolDataObject = SetupConsol();

			var invalidForwarderHandlingType = new CodeDescriptionPair() { Code = "", Description = null };
			consolDataObject.SendingForwarderHandlingType = invalidForwarderHandlingType;
			consolDataObject.ReceivingForwarderHandlingType = invalidForwarderHandlingType;

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertNotNull(consolBO);
			AssertEquals(ZString.Empty, consolBO.JK_ReceivingForwarderHandlingType);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Warning - Matching 'Creditor':- No match found for '[Org. Code: THECODE; Company Name: SOME COMPANY; Address 1: SOME STREET; City: MASCOT]'.
Information - Added Consol (Master Bill='08111111214') from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestImportColoadConsolidationChooseCorrectExistingOneToUpdate()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_MasterBillNum = "08111111214";
			consolInDB.JK_BookingReference = "";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol();
			consolDataObject.WayBillNumber = consolInDB.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = "BookingRefrence";
			consolDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.CoLoad, Description = "Co-Load" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var readConsolBO = reader.ReadIntoBusinessObject();

			AssertEquals(consolInDB.PK, readConsolBO.PK);

			consolInDB.JK_BookingReference = "";

			var consolInDB2 = Factory.New<ForwardingConsol>();
			consolInDB2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB2.JK_MasterBillNum = consolInDB.JK_MasterBillNum;
			consolInDB2.JK_BookingReference = (ZString)consolDataObject.BookingConfirmationReference;

			Factory.SaveForTesting();

			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			readConsolBO = reader.ReadIntoBusinessObject();

			AssertEquals(consolInDB2.PK, readConsolBO.PK);
		}

		public void TestConsolUpdatesAddressesUsingOnlyShortCode()
		{
			#region Setup Consol and Addresses

			#region Receiving Forwarding Addresses
			var receivingForwarderOrg = Factory.New<OrgHeader>();
			receivingForwarderOrg.OH_Code = "ORG1";
			receivingForwarderOrg.OH_IsForwarder = true;
			receivingForwarderOrg.OH_IsActive = true;

			OrgAppointedAgentPorts receivingForwarderPort = receivingForwarderOrg.AppointedAgentPorts.AddNew();
			receivingForwarderPort.O5_PortOrCountry = "AUSYD";
			receivingForwarderPort.O5_AirAgentStatus = "APP";
			receivingForwarderPort.O5_AgentDirection = "IMP";

			var receivingForwarderAddress = receivingForwarderOrg.MainAddress;
			receivingForwarderAddress.OA_Address1 = "123 STREET";
			receivingForwarderAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			receivingForwarderAddress.OA_PostCode = "1111";
			receivingForwarderAddress.OA_State = "QLD";
			receivingForwarderAddress.OA_Code = "DEFAULTRECEIVING";
			receivingForwarderPort.O5_OA_AgentOfficeAddress = receivingForwarderAddress.PK;

			var receivingForwarderOrgOtherAddress = Factory.New<OrgAddress>();
			receivingForwarderOrgOtherAddress.OA_Address1 = "321 OTHER";
			receivingForwarderOrgOtherAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			receivingForwarderOrgOtherAddress.OA_PostCode = "1111";
			receivingForwarderOrgOtherAddress.OA_State = "QLD";
			receivingForwarderOrgOtherAddress.OA_Code = "NEWRECEIVING";
			receivingForwarderOrg.Addresses.Add(receivingForwarderOrgOtherAddress);

			#endregion

			#region Sending Forwarding Addresses

			var sendingForwarderOrg = Factory.New<OrgHeader>();
			sendingForwarderOrg.OH_Code = "ORG2";
			sendingForwarderOrg.OH_IsForwarder = true;
			sendingForwarderOrg.OH_IsActive = true;

			OrgAppointedAgentPorts sendingForwarderPort = sendingForwarderOrg.AppointedAgentPorts.AddNew();
			sendingForwarderPort.O5_PortOrCountry = "USCHI";
			sendingForwarderPort.O5_AirAgentStatus = "APP";
			sendingForwarderPort.O5_AgentDirection = "EXP";

			var sendingForwarderAddress = sendingForwarderOrg.MainAddress;
			sendingForwarderAddress.OA_Address1 = "789 LANE";
			sendingForwarderAddress.OA_RL_NKRelatedPortCode = "USCHI";
			sendingForwarderAddress.OA_State = "IL";
			sendingForwarderAddress.OA_PostCode = "1234";
			sendingForwarderAddress.OA_Code = "DEFAULTSENDING";
			sendingForwarderPort.O5_OA_AgentOfficeAddress = sendingForwarderAddress.PK;
			sendingForwarderOrg.Addresses.Add(sendingForwarderAddress);

			var sendingForwarderOrgOtherAddress = Factory.New<OrgAddress>();
			sendingForwarderOrgOtherAddress.OA_Address1 = "987 OTHER";
			sendingForwarderOrgOtherAddress.OA_State = "IL";
			sendingForwarderOrgOtherAddress.OA_PostCode = "1234";
			sendingForwarderOrgOtherAddress.OA_RL_NKRelatedPortCode = "USCHI";
			sendingForwarderOrgOtherAddress.OA_Code = "NEWSENDING";
			sendingForwarderOrg.Addresses.Add(sendingForwarderOrgOtherAddress);

			#endregion

			#region Consignor Addresses

			var consignorOrg = Factory.New<OrgHeader>();
			consignorOrg.OH_Code = "ORG3";
			consignorOrg.OH_IsConsignor = true;
			consignorOrg.OH_IsActive = true;

			var consignorAddress = consignorOrg.MainAddress;
			consignorAddress.OA_Address1 = "111 FAKE STREET";
			consignorAddress.OA_RL_NKRelatedPortCode = "USCHI";
			consignorAddress.OA_State = "IL";
			consignorAddress.OA_PostCode = "1234";
			consignorAddress.OA_Code = "DEFAULTCONSIGNOR";
			consignorOrg.Addresses.Add(consignorAddress);

			var consignorOrgOtherAddress = Factory.New<OrgAddress>();
			consignorOrgOtherAddress.OA_Address1 = "222 FAKE STREET";
			consignorOrgOtherAddress.OA_RL_NKRelatedPortCode = "USCHI";
			consignorOrgOtherAddress.OA_State = "IL";
			consignorOrgOtherAddress.OA_PostCode = "1234";
			consignorOrgOtherAddress.OA_Code = "NEWCONSIGNOR";
			consignorOrg.Addresses.Add(consignorOrgOtherAddress);

			#endregion

			#region Consignee Addresses

			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "ORG4";
			consigneeOrg.OH_IsConsignee = true;
			consigneeOrg.OH_IsActive = true;

			var consigneeAddress = consigneeOrg.MainAddress;
			consigneeAddress.OA_Address1 = "111 REAL STREET";
			consigneeAddress.OA_PostCode = "1234";
			consigneeAddress.OA_State = "NSW";
			consigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consigneeAddress.OA_Code = "DEFAULTCONSIGNEE";
			consigneeOrg.Addresses.Add(consigneeAddress);

			var consigneeOrgOtherAddress = Factory.New<OrgAddress>();
			consigneeOrgOtherAddress.OA_Address1 = "222 REAL STREET";
			consigneeOrgOtherAddress.OA_PostCode = "1234";
			consigneeOrgOtherAddress.OA_State = "NSW";
			consigneeOrgOtherAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consigneeOrgOtherAddress.OA_Code = "NEWCONSIGNEE";
			consigneeOrg.Addresses.Add(consigneeOrgOtherAddress);

			#endregion

			#region Shipment Setup

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_HouseBill = "123";
			shipment.JS_HouseBillOfLadingType = WayBillTypeList.Codes.House;
			var consignorDocumentaryAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			consignorDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
			var consigneeDocumentaryAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			consigneeDocumentaryAddress.E2_OA_Address = consigneeAddress.PK;

			#endregion

			#region Consol Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.SetDefaultReceivingForwarderAddress(receivingForwarderOrg);
			consol.SetDefaultSendingForwarderAddress(sendingForwarderOrg);
			consol.JK_MasterBillNum = "123456";
			consol.JK_UniqueConsignRef = "C0001010";
			consol.Shipments.Add(shipment);

			Factory.SaveForTesting();

			#endregion

			#endregion

			CombineAssertions(delegate
			{
				AssertEquals("Precondition", "123 STREET", consol.ReceivingForwarderAddress.OA_Address1);
				AssertEquals("Precondition", "789 LANE", consol.SendingForwarderAddress.OA_Address1);
				AssertEquals("Precondition", "111 FAKE STREET", shipment.GetConsignorDocAddress.Address.OA_Address1);
				AssertEquals("Precondition", "111 REAL STREET", shipment.GetConsigneeDocAddress.Address.OA_Address1);
			});

			var import = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			import.DataContext = DataContextFactory.New();
			import.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C0001010");
			import.WayBillNumber = "123456";
			import.IsNeutralMaster = new IsNeutralMaster() { Value = false };
			var wayBill = new WayBillType();
			wayBill.Code = "MWB";
			import.WayBillType = wayBill;

			var transportLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transportLeg.PortOfDischarge = new UNLOCO();
			transportLeg.PortOfDischarge.Code = "AUSYD";
			import.SetTransportLegCollection(() => new DataObjectList<TransportLeg>() { transportLeg });

			var receivingAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			receivingAddress.AddressShortCode = "NEWRECEIVING";
			receivingAddress.OrganizationCode = "ORG1";
			receivingAddress.AddressType = AddressTypes.ReceivingForwarderAddress;

			var sendingAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			sendingAddress.AddressShortCode = "NEWSENDING";
			sendingAddress.OrganizationCode = "ORG2";
			sendingAddress.AddressType = AddressTypes.SendingForwarderAddress;

			import.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { receivingAddress, sendingAddress });

			var shipmentImport = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentImport.DataContext = DataContextFactory.New();
			shipmentImport.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S0001010");
			shipmentImport.WayBillNumber = "123";
			shipmentImport.WayBillType = new WayBillType() { Code = "HWB" };

			var consignorShipmentAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consignorShipmentAddress.AddressShortCode = "NEWCONSIGNOR";
			consignorShipmentAddress.OrganizationCode = "ORG3";
			consignorShipmentAddress.AddressType = "ConsignorDocumentaryAddress";

			var consigneeShipmentAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consigneeShipmentAddress.AddressShortCode = "NEWCONSIGNEE";
			consigneeShipmentAddress.OrganizationCode = "ORG4";
			consigneeShipmentAddress.AddressType = "ConsigneeDocumentaryAddress";

			shipmentImport.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorShipmentAddress, consigneeShipmentAddress });

			import.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentImport });

			var reader = new ConsolDataObjectReader(import, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			var shipmentBO = (ForwardingShipment)consolBO.Shipments.First();

			CombineAssertions(delegate
			{
				AssertEquals("Receiving Forwarder Address should have changed", "321 OTHER", consolBO.ReceivingForwarderAddress.OA_Address1);
				AssertEquals("Sending Forwarder Address should have changed", "987 OTHER", consolBO.SendingForwarderAddress.OA_Address1);
				AssertEquals("Consignor Address should have changed", "222 FAKE STREET", shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress).Address.OA_Address1);
				AssertEquals("Consignee Address should have changed", "222 REAL STREET", shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress).Address.OA_Address1);
			});
		}

		public void TestConsolCreateDefaultAgentAddress()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var sendingOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			sendingOrgHeader.OH_Code = "SEDOG";
			sendingOrgHeader.OH_IsForwarder = ZBool.True;

			var address1 = sendingOrgHeader.Addresses.AddNewMainAddress();
			address1.OA_Code = "address1 ShortCode";
			address1.OA_Address1 = "address1";

			var address2 = sendingOrgHeader.Addresses.AddNew();
			address2.OA_Code = "address2 ShortCode";
			address2.OA_Address1 = "address2";

			var receiveOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			receiveOrgHeader.OH_Code = "RECOG";
			receiveOrgHeader.OH_IsForwarder = ZBool.True;

			var address3 = receiveOrgHeader.Addresses.AddNewMainAddress();
			address3.OA_Code = "address3 ShortCode";
			address3.OA_Address1 = "address3";

			var address4 = receiveOrgHeader.Addresses.AddNew();
			address4.OA_Code = "address4 ShortCode";
			address4.OA_Address1 = "address4";

			var sendingAppointedAgentPort = sendingOrgHeader.AppointedAgentPorts.AddNew();
			sendingAppointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingAppointedAgentPort.O5_SeaAgentStatus = "PUB";
			sendingAppointedAgentPort.O5_OA_AgentOfficeAddress = address2.PK;
			sendingAppointedAgentPort.O5_PortOrCountry = "CNSHA";

			var receiveAppointedAgentPort = receiveOrgHeader.AppointedAgentPorts.AddNew();
			receiveAppointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receiveAppointedAgentPort.O5_SeaAgentStatus = "PUB";
			receiveAppointedAgentPort.O5_OA_AgentOfficeAddress = address4.PK;
			receiveAppointedAgentPort.O5_PortOrCountry = "JPOSA";

			Factory.SaveForTesting();

			var serviceTaskLog = CreateAndProcessUniversalShipment("UniversalConsolNoAgentAddress.xml");
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Consol (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));

			AssertNotNull(consol);
			AssertEquals("Sending Agent Address should be ADR02.", true, consol.JK_OA_SendingForwarderAddress == address2.PK);
			AssertEquals("Receive Agent Address should be ADR04.", true, consol.JK_OA_ReceivingForwarderAddress == address4.PK);
		}

		public void TestConsolCreateCalculateAgentAddress()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var sendingOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			sendingOrgHeader.OH_Code = "SEDOG";
			sendingOrgHeader.OH_IsForwarder = ZBool.True;

			var address1 = sendingOrgHeader.Addresses.AddNewMainAddress();
			address1.OA_Code = "address1 ShortCode";
			address1.OA_Address1 = "address1";

			var address2 = sendingOrgHeader.Addresses.AddNew();
			address2.OA_Code = "address2 ShortCode";
			address2.OA_Address1 = "address2";

			var receiveOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			receiveOrgHeader.OH_Code = "RECOG";
			receiveOrgHeader.OH_IsForwarder = ZBool.True;

			var address3 = receiveOrgHeader.Addresses.AddNewMainAddress();
			address3.OA_Code = "address3 ShortCode";
			address3.OA_Address1 = "address3";

			var address4 = receiveOrgHeader.Addresses.AddNew();
			address4.OA_Code = "address4 ShortCode";
			address4.OA_Address1 = "address4";

			var sendingAppointedAgentPort = sendingOrgHeader.AppointedAgentPorts.AddNew();
			sendingAppointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingAppointedAgentPort.O5_SeaAgentStatus = "PUB";
			sendingAppointedAgentPort.O5_OA_AgentOfficeAddress = address2.PK;
			sendingAppointedAgentPort.O5_PortOrCountry = "CNSHA";

			var receiveAppointedAgentPort = receiveOrgHeader.AppointedAgentPorts.AddNew();
			receiveAppointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receiveAppointedAgentPort.O5_SeaAgentStatus = "PUB";
			receiveAppointedAgentPort.O5_OA_AgentOfficeAddress = address4.PK;
			receiveAppointedAgentPort.O5_PortOrCountry = "JPOSA";

			Factory.SaveForTesting();

			var serviceTaskLog = CreateAndProcessUniversalShipment("UniversalConsolNoAgentAddressCollection.xml");
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Consol (Master Bill='SITSHOSWB92209') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='SITSHOSWB92209') with 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));

			AssertNotNull(consol);
			AssertEquals("Sending Agent Address should be ADR02.", true, consol.JK_OA_SendingForwarderAddress == address2.PK);
			AssertEquals("Receive Agent Address should be ADR04.", true, consol.JK_OA_ReceivingForwarderAddress == address4.PK);
		}

		public void TestGrossWeightVerificationTypeWithEmptyContainer()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_MasterBillNum = "08111111214";
			consolInDB.JK_BookingReference = "BookingRefrence";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolImportFileVGM.xml");
			consolDataObject.WayBillNumber = consolInDB.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = consolInDB.JK_BookingReference;
			consolDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.CoLoad, Description = "Co-Load" };

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			containerDataObject.GrossWeightVerificationType = new CodeDescriptionPair
			{
				Code = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages,
				Description = Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method2Packages
			};
			containerDataObject.IsEmptyContainer = true;
			consolDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			consolDataObject.ContainerCollection.Add(containerDataObject);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertContains(CommonContainerValidation.CannotAllowMethod2PackagesForEmptyContainer, logger.Logs);
		}

		public void TestCarrierContractNumber()
		{
			var consol = GetConsolBOToLoad();
			consol.JK_CarrierContractNumber = "CCA00000";
			Factory.SaveForTesting();

			var consolDataObject = ReadStringIntoUniversalShipment(EmptyConsolXML);
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertEquals("When CarrierContractNumber field not present in XML, it should not clear existing number.", "CCA00000", consolBO.JK_CarrierContractNumber);

			consolDataObject = ReadStringIntoUniversalShipment(GetConsolWithCarrierContractNumber(string.Empty));
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();
			AssertEquals("When CarrierContractNumber field present in XML, it should be used.", string.Empty, consolBO.JK_CarrierContractNumber);

			consolDataObject = ReadStringIntoUniversalShipment(GetConsolWithCarrierContractNumber("CCA69420"));
			reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();
			AssertEquals("When CarrierContractNumber field present in XML, it should be used.", "CCA69420", consolBO.JK_CarrierContractNumber);
		}

		const string EmptyConsolXML = @"
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>C00001052</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";

		string GetConsolWithCarrierContractNumber(string carrierContractNumber) => $@"
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>C00001052</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

	<CarrierContractNumber>{carrierContractNumber}</CarrierContractNumber>
  </Shipment>
</UniversalShipment>";

		public void TestShouldNotDefaultMasterBillNumberForNewConsols_WayBillNumberIsEmptyInUXML()
		{
			var newCustomisation = new BillOfLadingNumberCustomisation();
			newCustomisation.RemoveFountainPrefix = false;
			newCustomisation.UseShipmentSequenceNumber = true;
			newCustomisation.CheckDigitAlgorithm = "NON";
			newCustomisation.AutoAllocateMasterBillNumbersToConsols = true;

			using (FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation))
			{
				var consolDataObject = SetupConsol("ConsolImportFile.xml");
				consolDataObject.WayBillNumber = ZString.Empty;
				var transportModePair = new CodeDescriptionPair();
				transportModePair.Code = Constants.TransportModes.Road;
				consolDataObject.TransportMode = transportModePair;
				var reader1 = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader1.ReadIntoBusinessObject();

				Factory.SaveForTesting();

				AssertEquals("When MasterBillNumber field is not present in XML, Master number should not regenerated for new consols", ZString.Empty, consolBO.JK_MasterBillNum);
			}
		}

		public void TestShouldNotDefaultMasterBillNumberForExsitingConsols()
		{
			var newCustomisation = new BillOfLadingNumberCustomisation();
			newCustomisation.RemoveFountainPrefix = false;
			newCustomisation.UseShipmentSequenceNumber = true;
			newCustomisation.CheckDigitAlgorithm = "NON";
			newCustomisation.AutoAllocateMasterBillNumbersToConsols = true;

			using (FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation))
			{
				var consolBOToLoad = Factory.New<ForwardingConsol>();
				consolBOToLoad.JK_UniqueConsignRef = "C00001052";
				consolBOToLoad.JK_RL_NKLoadPort = "NZAKL";
				consolBOToLoad.JK_RL_NKDischargePort = "AUSYD";
				consolBOToLoad.JK_AgentType = Constants.AgentType.Agent;
				consolBOToLoad.JK_TransportMode = Constants.TransportModes.Road;
				consolBOToLoad.JK_AWBServiceLevel = "STD";
				consolBOToLoad.JK_AgentsReference = "Agent U";
				consolBOToLoad.JK_MasterBillNum = ZString.Empty;

				Factory.SaveForTesting();

				var consolDataObject = SetupConsol("ConsolImportFile.xml");
				consolDataObject.WayBillNumber = null;
				var transportModePair = new CodeDescriptionPair();
				transportModePair.Code = Constants.TransportModes.Road;
				consolDataObject.TransportMode = transportModePair;
				var reader1 = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader1.ReadIntoBusinessObject();

				Factory.SaveForTesting();

				AssertEquals("When MasterBillNumber field is not present in XML, Master number should not regenerated for exsiting consols", ZString.Empty, consolBO.JK_MasterBillNum);
			}
		}

		UniversalShipment ReadStringIntoUniversalShipment(string xml)
		{
			using (var memoryStream = (SubStreamableStream)new MemoryStream(System.Text.Encoding.ASCII.GetBytes(xml)))
			{
				var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				ObjectFactory.Get<IXmlReader>().ReadXML(universalShipment, memoryStream, logger);

				return universalShipment;
			}
		}

		public void TestErrorMessageForInvalidRequiredTemperatures()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.RequiresTemperatureControl = true;

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);

			consolDataObject.RequiredTemperatureUnit = null;
			consolDataObject.RequiredTemperatureMinimum = null;
			consolDataObject.RequiredTemperatureMaximum = null;
			AssertNoExceptionThrown("Existing value should be used if not specified in XML.", () => reader.ReadIntoBusinessObject());

			consolDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Kelvin };
			AssertExceptionThrown("Exception should be thrown when invalid temperature unit provided.",
				typeof(DataObjectReadFailureException),
				"Invalid temperature unit (K). Temperature must be set to C (Celsius) or F (Fahrenheit).",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Centigrade };
			consolDataObject.RequiredTemperatureMinimum = -273.2m;
			AssertExceptionThrown("Exception should be thrown when invalid minimum temperature in Celsius provided.",
				typeof(DataObjectReadFailureException),
				"Minimum temperature (-273.2°C) is below the minimum possible temperature of absolute zero (-273.15°C).",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Fahrenheit };
			consolDataObject.RequiredTemperatureMinimum = -459.7m;
			AssertExceptionThrown("Exception should be thrown when invalid minimum temperature in Fahrenheit provided.",
				typeof(DataObjectReadFailureException),
				"Minimum temperature (-459.7°F) is below the minimum possible temperature of absolute zero (-459.67°F).",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.RequiredTemperatureMinimum = -459.6m;
			consolDataObject.RequiredTemperatureMaximum = -459.7m;
			AssertExceptionThrown("Exception should be thrown when invalid maximum temperature provided.",
				typeof(DataObjectReadFailureException),
				"Minimum temperature (-459.6°F) cannot be higher than maximum temperature (-459.7°F).",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.RequiredTemperatureMaximum = -459.6m;
			AssertNoExceptionThrown("No exceptions should be thrown when valid temperatures provided.", () => reader.ReadIntoBusinessObject());
		}

		public void Test_ImportUXML_ShouldCreateNewSBREvent_WhenOnlyTransportLegHasChanges()
		{
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_Code = "AUAGENSYD";
			var consolDataObjectFirst = SetupConsol("ConsolImportFileTransportsWithMultipleLegs.xml");
			var readerFirst = new ConsolDataObjectReader(consolDataObjectFirst, logger, Factory);
			var consolBOFirst = readerFirst.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEventNumber("1 uncancelled SBR event on consol, 0 cancelled SBR event on consol", consolBOFirst, 1, 0);

			var consolDataObjectSecond = SetupConsol("ConsolImportFileTransportsWithMultipleLegs.xml");
			consolDataObjectFirst.DataContext.DataTargetCollection.FirstOrDefault().Key = consolBOFirst.JK_UniqueConsignRef;
			consolDataObjectSecond.TransportLegCollection[1].PortOfDischarge = new UNLOCO { Code = "sin" };
			var readerSecond = new ConsolDataObjectReader(consolDataObjectSecond, logger, Factory);
			var consolBOSecond = readerSecond.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEventNumber("1 uncancelled SBR event on consol, 1 cancelled SBR event on consol", consolBOSecond, 1, 1);
		}

		public void Test_ImportUXML_ShouldNotCreateNewSBREvent_WhenConsolAndTransportLegDoesNothaveChanges()
		{
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_Code = "AUAGENSYD";
			var consolDataObjectFirst = SetupConsol("ConsolImportFileTransportsWithMultipleLegs.xml");
			var readerFirst = new ConsolDataObjectReader(consolDataObjectFirst, logger, Factory);
			var consolBOFirst = readerFirst.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEventNumber("1 uncancelled SBR event on consol, 0 cancelled SBR event on consol", consolBOFirst, 1, 0);

			var consolDataObjectSecond = SetupConsol("ConsolImportFileTransportsWithMultipleLegs.xml");
			consolDataObjectFirst.DataContext.DataTargetCollection.FirstOrDefault().Key = consolBOFirst.JK_UniqueConsignRef;
			consolDataObjectSecond.TransportLegCollection[1].ActualArrival = new ZDateTime("2024-09-10T18:07:00");
			var readerSecond = new ConsolDataObjectReader(consolDataObjectSecond, logger, Factory);
			var consolBOSecond = readerSecond.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEventNumber("1 uncancelled SBR event on consol, 0 cancelled SBR event on consol", consolBOSecond, 1, 0);
		}

		static void AssertEventNumber(string message, CommonConsol consol, int expectedUncancelledEventNumber, int expectedCancelledEventNumber)
		{
			consol.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.SubscriptionRequested);
			AssertEquals(
				message,
				expectedUncancelledEventNumber,
				consol.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.SubscriptionRequested).Count());
			AssertEquals(
				message,
				expectedCancelledEventNumber,
				consol.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && x.IsCancelled));
		}

		public void Test_ImportUSXML_ShouldNotAllocateMAWB_WhenIsAllocationOfMawbAllowedReturnsFalse()
		{
			const string testXML = @"
<UniversalShipment version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
	<TransportMode>
      <Code>AIR</Code>
    </TransportMode>
	<IsNeutralMaster CreateAndAllocateNeutralStock=""true"">true</IsNeutralMaster>
	<WayBillNumber>50407806623</WayBillNumber>
  </Shipment>
</UniversalShipment>";

			var consolDataObject = ReadStringIntoUniversalShipment(testXML);
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("It's not allowed to allocate MAWB", false, consolBO.MAWBAllocation.IsAllocationOfMawbAllowed);
			AssertEquals("Neutral Master will be set to false as allocation of MAWB not allowed.", false, consolBO.JK_IsNeutralMaster);
			AssertEquals("MasterBillMAWB will be empty as no MAWB allocated.", string.Empty, consolBO.MasterBillMAWB);
			AssertEquals("Consol master bill number will be the airline prefix.", "504", consolBO.JK_MasterBillNum);

			var mawb = Factory.LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_MAWB, "07806623"));
			AssertNull("No MAWB is allocated", mawb);

			AssertContains("MAWB number will not be allocated from Stock because the consol is not valid for Neutral Master.", logger.Logs);
		}

		#region TestRejectDuplicateAirBooking

		public void TestRejectDuplicateAirBooking()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1406309";

			var interchange = factory.New<IXmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_From = "eBookingAPI";
			interchange.EI_To = "zzz";
			interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;

			var message = interchange.AddNeweHubMessage();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			message.EM_Status = EDIInterchangeStatusList.Codes.Received;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_MessageText = airBookingUniversalShipment;

			factory.Save();

			var consolDataObject = new UniversalShipment();
			consolDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2012_11.DataTarget>
				{
					new UniversalDataBuss.DataObjects.Universal._2012_11.DataTarget
					{
						Type = nameof(ForwardingConsol),
						Key = consol.JK_UniqueConsignRef
					}
				},
				DocumentaryOverride = new DocumentaryOverride
				{
					DocumentName = "AirBooking",
					SubmissionVersion = 1
				}
			};
			consolDataObject.WayBillNumber = consol.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = consol.JK_BookingReference;

			logger.TopLevelDataObject = consolDataObject;

			var reader = new ConsolDataObjectReader(consolDataObject, logger, new UniversalObjectFactory(factory));
			reader.ReadIntoBusinessObject();

			Assert("Import had no errors", !logger.HasErrors());

			var dim = consol.Logs.AddNew(Events.DataImport);

			var messageLogPivot = factory.New<IGenPivot>();
			messageLogPivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			messageLogPivot.XX_Relation1ID = dim.PK;
			messageLogPivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			messageLogPivot.XX_Relation2ID = message.PK;
			messageLogPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;

			factory.Save();

			logger.ClearLogs();
			reader = new ConsolDataObjectReader(consolDataObject, logger, new UniversalObjectFactory(factory));
			reader.ReadIntoBusinessObject();

			Assert("Import had errors, due to duplicate air booking uxml", logger.HasErrors());
			Assert("Import had errors, due to duplicate air booking uxml",
				logger.Logs.Contains("This Universal Shipment submission version is: 1. An Universal Shipment with the same or newer submission versions (1) has already been imported."));

			consolDataObject.DataContext.DocumentaryOverride.SubmissionVersion = 2;

			logger.ClearLogs();
			reader = new ConsolDataObjectReader(consolDataObject, logger, new UniversalObjectFactory(factory));
			reader.ReadIntoBusinessObject();

			Assert("Import had no errors", !logger.HasErrors());
		}

		const string airBookingUniversalShipment = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
			<Shipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
				</DataContext>

				<BookingConfirmationReference>XXXX</BookingConfirmationReference>
				<PortOfOrigin>HAM</PortOfOrigin>
				<PortOfDestination>HKG</PortOfDestination>
				<WayBillNumber>607-89506712</WayBillNumber>
				<TotalNoOfPacks>5</TotalNoOfPacks>
				<TotalWeight>749.6</TotalWeight>
				<TotalWeightUnit>KG</TotalWeightUnit>
				<TotalVolume>60.03</TotalVolume>
				<TotalVolumeUnit>M3</TotalVolumeUnit>

				<TransportLegCollection>
					<TransportLeg>
						<LegOrder>1</LegOrder>
						<TransportMode>Air</TransportMode>
						<PortOfLoading>HAM</PortOfLoading>
						<PortOfDischarge>HKG</PortOfDischarge>
						<EstimatedArrival>2020-05-09</EstimatedArrival>
						<EstimatedDeparture>2020-05-09</EstimatedDeparture>
						<LegType>Flight1</LegType>
						<TransportMode>Air</TransportMode>
						<VoyageFlightNo>EY7</VoyageFlightNo>
						<BookingStatus>CNF</BookingStatus>
					</TransportLeg>
					<TransportLeg>
						<LegOrder>2</LegOrder>
						<TransportMode>Air</TransportMode>
						<PortOfLoading>HKG</PortOfLoading>
						<PortOfDischarge>SIN</PortOfDischarge>
						<EstimatedArrival>2020-05-10</EstimatedArrival>
						<EstimatedDeparture>2020-05-10</EstimatedDeparture>
						<LegType>Flight2</LegType>
						<TransportMode>Air</TransportMode>
						<VoyageFlightNo>EY8</VoyageFlightNo>
						<BookingStatus>CNF</BookingStatus>
					</TransportLeg>
				</TransportLegCollection>

				<NoteCollection>
					<Note>
						<Description>CarrierResponse</Description>
						<NoteText>Flight XXX has been changed</NoteText>
						<IsCustomDescription>false</IsCustomDescription>
					</Note>
				</NoteCollection>

			</Shipment>
		</UniversalShipment>";

		#endregion

		#region Test Special Handling

		public void TestSpecialHandlingCodeAttachedToConsol()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_MasterBillNum = "08111111214";
			consolInDB.JK_BookingReference = "BookingRefrence";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolImportFile.xml");
			consolDataObject.WayBillNumber = consolInDB.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = consolInDB.JK_BookingReference;
			consolDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.CoLoad, Description = "Co-Load" };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertEquals(1, consolBO.AWBSpecialHandlingItems.Count);
			AssertEquals(consolBO.AWBSpecialHandlingItems[0].JKH_Code, "PER");
		}

		public void TestSpecialHandlingMultipleCodesAttachedToConsol()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_MasterBillNum = "08111111214";
			consolInDB.JK_BookingReference = "BookingRefrence";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolImportFile.xml");
			consolDataObject.WayBillNumber = consolInDB.JK_MasterBillNum;
			consolDataObject.BookingConfirmationReference = consolInDB.JK_BookingReference;
			consolDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.CoLoad, Description = "Co-Load" };

			var specialHandlingCode = new CodeDescriptionPair();
			specialHandlingCode.Code = "ACT";
			consolDataObject.SpecialHandlingCollection.Add(specialHandlingCode);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertEquals(2, consolBO.AWBSpecialHandlingItems.Count);
			AssertEquals(consolBO.AWBSpecialHandlingItems[0].JKH_Code, "PER");
			AssertEquals(consolBO.AWBSpecialHandlingItems[1].JKH_Code, "ACT");
		}

		#endregion

		#region TransportCollection Main Leg

		public void TestMainLegIsSetCorrectly_MultipleMainLegs()
		{
			var consolDataObject = SetupConsol("ConsolImportFileTransportsWithMultipleMainLegs.xml");

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var readConsolBO = reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder("First export leg is set as main",
				new[]
					{
						"AUSYD|AUBNE|001|PRE", "AUBNE|NZAKL|002|MAI", "NZAKL|CNSHA|003|ONF", "CNSHA|CNBJS|004|ONF"
					},
				FormatTransports(readConsolBO));
		}

		public void TestMainLegIsSetCorrectly_SingleMainLeg()
		{
			var consolDataObject = SetupConsol("ConsolImportFileTransportsWithSingleMainLeg.xml");

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var readConsolBO = reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder("Main leg set as per XML",
				new[]
					{
						"AUSYD|AUBNE|001|PRE", "AUBNE|NZAKL|002|PRE", "NZAKL|CNSHA|003|MAI", "CNSHA|CNBJS|004|ONF"
					},
				FormatTransports(readConsolBO));
		}

		public void TestMainLegIsSetCorrectly_NoMainLegs()
		{
			var consolDataObject = SetupConsol("ConsolImportFileTransportsWithNoMainLegs.xml");

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var readConsolBO = reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder("First export leg is set as main",
				new[]
					{
						"AUSYD|AUBNE|001|PRE", "AUBNE|NZAKL|002|MAI", "NZAKL|CNSHA|003|ONF", "CNSHA|CNBJS|004|ONF"
					},
				FormatTransports(readConsolBO));
		}

		public void TestMainLegIsSetCorrectly_NoInternationalLegs()
		{
			var consolDataObject = SetupConsol("ConsolImportFileTransportsWithNoInternationalLegs.xml");

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var readConsolBO = reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder("For domestic transports, the first sea transport's type is Main",
				new[]
					{
						"AUSYD|AUBNE|001|MAI", "AUBNE|AUPER|002|ONF", "AUPER|AUADL|003|ONF", "AUADL|AUMEL|004|ONF"
					},
				FormatTransports(readConsolBO));
		}

		string[] FormatTransports(ForwardingConsol consol)
		{
			return consol.Transports
				.Cast<Transport>()
				.Select(t => string.Format("{0}|{1}|{2}|{3}", t.JW_RL_NKLoadPort, t.JW_RL_NKDiscPort, t.JW_VoyageFlight, t.JW_TransportType))
				.ToArray();
		}

		#endregion

		#region TestImportNeutralMasterConsol - No Existing Consol

		public void TestImportNeutralMasterConsol_NoMatchingConsolExistsAndNoMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
				AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("08111111214"));

				var query = new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.NewZealand);
				var country = Factory.LoadTop1<RefCountry>(query);
				FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { country.PK.ToGuid() });

				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile.xml"));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Import successful log", @"
Added Consol (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
	.Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Added Consol (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
	.Trim(), logNoteText);

					var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
					AssertNotNull("Import successful, new consol created", consolBO);
					AssertEquals("Import successful, matching  MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
					AssertEquals("Import successful, matching  MAWB stock created", true, consolBO.JK_IsNeutralMaster);
					AssertNotNull("Import successful, matching  MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
					AssertEquals("Import successful, matching  MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
					AssertEquals("Import successful, matching  MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
					AssertEquals("Import successful, matching  MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
					AssertEquals("Import successful, matching  MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
				});
			}
		}

		public void TestImportNeutralMasterConsol_NoMatchingConsolExistsAndMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				var mawb = Factory.New<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_MAWB = "11111214";
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_IsPrinted = false;
				mawb.JM_IsPaper = false;
				Factory.SaveForTesting();

				AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
				AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("08111111214"));

				var query = new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.NewZealand);
				var country = Factory.LoadTop1<RefCountry>(query);
				FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { country.PK.ToGuid() });

				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile.xml"));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Import successful log", @"
Added Consol from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
	.Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
	.Trim(), logNoteText);

					var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
					AssertNotNull("Import successful, new consol created", consolBO);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", "08111111214", consolBO.JK_MasterBillNum);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", true, consolBO.JK_IsNeutralMaster);
					AssertNotNull("Import successful, matching  MAWB stock allocated to consol", consolBO.MAWBAllocation.AllocatedMawb);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", mawb.PK, consolBO.MAWBAllocation.AllocatedMawb.PK);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
					AssertEquals("Import successful, matching  MAWB stock allocated to consol", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
				});
			}
		}

		public void TestImportNeutralMasterConsol_MAWBOnlyContainsPrefix_NoMatchingConsolExistsAndMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol_RegistryEnabled()
		{
			using (FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var mawb1 = Factory.New<JobMawb>();
				mawb1.JM_Airline3DigitPrefix = "081";
				mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb1.JM_MAWB = "11111214";
				mawb1.JM_ServiceLevel = "STD";
				mawb1.JM_IsPrinted = false;
				mawb1.JM_IsPaper = false;
				Factory.SaveForTesting();

				AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
				AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("08111111214"));

				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_MAWBOnlyContainsPrefix.xml"));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Added Consol (Master Bill='081') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
	.Trim(), logNoteText);

					var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
					AssertNotNull("New consol was created", consolBO);
					AssertEquals("08111111214", consolBO.JK_MasterBillNum);
					AssertEquals(true, consolBO.JK_IsNeutralMaster);
					AssertNotNull(consolBO.MAWBAllocation.AllocatedMawb);
					AssertEquals("081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
					AssertEquals("11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
					AssertEquals(consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
					AssertEquals(consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
				});
			}
		}

		public void TestImportNeutralMasterConsol_ConsolFinalMasterPrinted()
		{
			using (FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var mawb1 = Factory.New<JobMawb>();
				mawb1.JM_Airline3DigitPrefix = "081";
				mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb1.JM_MAWB = "11111214";
				mawb1.JM_ServiceLevel = "STD";
				mawb1.JM_IsPrinted = false;
				mawb1.JM_IsPaper = false;
				Factory.SaveForTesting();

				var consolBOToLoad = Factory.New<ForwardingConsol>();
				consolBOToLoad.JK_UniqueConsignRef = "C00001052";
				consolBOToLoad.JK_RL_NKLoadPort = "AUSYD";
				consolBOToLoad.JK_RL_NKDischargePort = "USLAX";
				consolBOToLoad.JK_AgentType = Constants.AgentType.Agent;
				consolBOToLoad.JK_TransportMode = Constants.TransportModes.Air;
				consolBOToLoad.JK_AWBServiceLevel = "STD";
				consolBOToLoad.MasterBillAirlinePrefix = "081";
				consolBOToLoad.JK_AgentsReference = "Agent U";
				consolBOToLoad.JK_MasterBillNum = "08111111214";
				consolBOToLoad.JK_IsNeutralMaster = true;
				Factory.SaveForTesting();

				mawb1.JM_IsPrinted = true;
				Factory.SaveForTesting();

				consolBOToLoad.Reload();

				AssertEquals("08111111214", consolBOToLoad.JK_MasterBillNum);
				AssertNotNull(consolBOToLoad.MAWBAllocation.AllocatedMawb);
				AssertEquals(mawb1.PK, consolBOToLoad.MAWBAllocation.AllocatedMawb.PK);
				AssertEquals(consolBOToLoad.PK, mawb1.JM_ParentID);
				AssertEquals(consolBOToLoad.Prefix, mawb1.JM_ParentTableCode);
				AssertEquals(true, consolBOToLoad.IsNeutralMAWBPrinted);

				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_MAWBOnlyContainsPrefix.xml"));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Error - Attempted to import a neutral master consolidation with a new MAWB number 081. Cannot create/allocate another MAWB to this consolidation, as the Final Master has already been printed.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
	.Trim(), logNoteText);
				});
			}
		}

		public void TestImportNeutralMasterConsol_MAWBOnlyContainsPrefix_NoMatchingConsolExistsAndMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol_ServiceLevelMismatch()
		{
			AssertNoMAWBAllocation("AirConsolImportFile_MAWBOnlyContainsPrefix-ServiceLevel.xml", allocateMAWBNumberRegistry: true, hasMasterBillNumberOutOfStockLog: true);
		}

		public void TestImportNeutralMasterConsol_MAWBOnlyContainsPrefix_NoMatchingConsolExists_RegistryDisabled()
		{
			AssertNoMAWBAllocation("AirConsolImportFile_MAWBOnlyContainsPrefix.xml", hasMasterBillNumberOutOfStockLog: true, hasRegistryOffLog: true);
		}

		public void TestImportNeutralMasterConsol_NoMatchingConsolExists_IncorrectWayBillNumberLength()
		{
			AssertNoMAWBAllocation("AirConsolImportFile_IncorrectWayBillNumberLength.xml", allocateMAWBNumberRegistry: true);
		}

		public void TestImportNeutralMasterConsol_NoMatchingConsolExists_IsNeutralMasterlStockIsFalse()
		{
			AssertNoMAWBAllocation("AirConsolImportFile_IsNeutralMasterIsFalse.xml", allocateMAWBNumberRegistry: true, hasIsNeutralMasterLog: true);
		}

		public void TestImportNeutralMasterConsol_NoMatchingConsolExists_CreateAndAllocateNeutralStockIsFalse()
		{
			AssertNoMAWBAllocation("AirConsolImportFile_CreateAndAllocateNeutralStockIsFalse.xml", allocateMAWBNumberRegistry: true, hasCreateAndAllocateNeutralStockLog: true);
		}

		void AssertNoMAWBAllocation(string fileName, bool allocateMAWBNumberRegistry = false, bool hasMasterBillNumberOutOfStockLog = false, bool hasRegistryOffLog = false, bool hasIsNeutralMasterLog = false, bool hasCreateAndAllocateNeutralStockLog = false)
		{
			using (FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allocateMAWBNumberRegistry))
			{
				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile(fileName));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message processed OK",
						hasMasterBillNumberOutOfStockLog || hasRegistryOffLog || hasIsNeutralMasterLog || hasCreateAndAllocateNeutralStockLog ? EDIMessage.Status.Warning : EDIMessage.Status.ProcessedOK,
						message.EM_Status);

					var logNoteText = message.GetLogNoteText();

					var expectedLog = string.Format(CultureInfo.InvariantCulture, @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
{0}{1}{2}{3}Added Consol (Master Bill='081') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='081') with 1 x ForwardingConsolStmNote, 1 x Transport.",
						hasIsNeutralMasterLog
							? "Warning - MAWB Number will not be allocated from Stock due to the XML having the IsNeutralMaster element set to false." + System.Environment.NewLine
							: string.Empty,
						hasCreateAndAllocateNeutralStockLog
							? "Warning - MAWB Number will not be allocated from Stock due to the XML having a CreateAndAllocateNeutralStock value set to false." + System.Environment.NewLine
							: string.Empty,
						hasMasterBillNumberOutOfStockLog
							? "Warning - No Master Bill Numbers in stock to allocate to this job. MAWB stock can be added via Maintain > Reference Files > MAWB Stock." + System.Environment.NewLine
							: string.Empty,
						hasRegistryOffLog
							? "Warning - MAWB Number will not be allocated from Stock due to the Registry setting found under Freight > MAWB > Allocate MAWB number from MAWB Stock upon XML import is currently set to No, and the MAWB number in this XML is incomplete." + System.Environment.NewLine
							: string.Empty);

					AssertMultilineASCIIEquals("Import successful log",
						expectedLog.Trim(), logNoteText);

					var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
					AssertNotNull("Consol should be created", consolBO);
					AssertEquals("081", consolBO.JK_MasterBillNum);
					AssertEquals(false, consolBO.JK_IsNeutralMaster);
					AssertNull(consolBO.MAWBAllocation.AllocatedMawb);
				});
			}
		}

		#endregion

		#region TestImportNeutralMasterConsol - Existing Consol Not Neutral

		public void TestImportNeutralMasterConsol_MatchingConsolNotNeutralAndNoMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			var consolBOToLoad = GetConsolBOToLoad();
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock created", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolNotNeutralAndNoMatchingMAWBStockExistsAndMAWBNumberIsUsedOnAnotherConsol()
		{
			var consolBOToLoad = GetConsolBOToLoad();

			var anotherConsol = Factory.New<ForwardingConsol>();
			anotherConsol.JK_UniqueConsignRef = "C00001050";
			anotherConsol.JK_RL_NKLoadPort = "AUMEL";
			anotherConsol.JK_RL_NKDischargePort = "JPTYO";
			anotherConsol.JK_AgentType = Constants.AgentType.Agent;
			anotherConsol.JK_TransportMode = Constants.TransportModes.Air;
			anotherConsol.JK_AWBServiceLevel = "STD";
			anotherConsol.MasterBillAirlinePrefix = "081";
			anotherConsol.JK_MasterBillNum = "08111111214";
			anotherConsol.JK_IsNeutralMaster = false;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: matching MAWB number is used on another consol", true, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock created", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);

				consolBO.RunPreSaveValidation();
				AssertHasError("Duplicate MAWB error expected", consolBO.MasterBillMAWBInfo, @"This Master Bill Number already exists on another Consol, Shipment or Booking.
Please select another number.");
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolNotNeutralAndMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			var consolBOToLoad = GetConsolBOToLoad();

			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "11111214";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_IsPrinted = false;
			mawb.JM_IsPaper = false;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C00001052 (Master Bill='081') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='081') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock allocated", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock allocated", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock allocated", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock allocated", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock allocated", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock allocated", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock allocated", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolNotNeutralAndMatchingMAWBStockExistsAndMAWBNumberIsUsedOnAnotherConsol()
		{
			var consolBOToLoad = GetConsolBOToLoad();

			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "11111214";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_IsPrinted = false;
			mawb.JM_IsPaper = false;

			Factory.SaveForTesting();

			var anotherConsol = Factory.New<ForwardingConsol>();
			anotherConsol.JK_UniqueConsignRef = "C00001050";
			anotherConsol.JK_RL_NKLoadPort = "AUMEL";
			anotherConsol.JK_RL_NKDischargePort = "JPTYO";
			anotherConsol.JK_AgentType = Constants.AgentType.Agent;
			anotherConsol.JK_TransportMode = Constants.TransportModes.Air;
			anotherConsol.JK_AWBServiceLevel = "STD";
			anotherConsol.MasterBillAirlinePrefix = "081";
			anotherConsol.JK_IsNeutralMaster = true;

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", true, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Import failed log", @"
ERROR - Attempted to import a consolidation with MAWB number 08111111214 that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import failed log", @"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Error - Attempted to import a consolidation with MAWB number 08111111214 that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import failed but matched existing consol by key", consolBO);
				AssertEquals("Import failed, existing consol unchanged", "08111111210", consolBO.JK_MasterBillNum);
			});
		}

		#endregion

		#region TestImportNeutralMasterConsol - Existing Consol Neutral

		public void TestImportNeutralMasterConsol_MatchingConsolNeutralAndNoMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "11111210";
			mawb.JM_ServiceLevel = "STD";

			Factory.SaveForTesting();

			var consolBOToLoad = GetConsolBOToLoad();
			consolBOToLoad.JK_IsNeutralMaster = true;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111210", consolBOToLoad.JK_MasterBillNum);
			AssertNotNull("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.MAWBAllocation.AllocatedMawb);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", mawb.PK, consolBOToLoad.MAWBAllocation.AllocatedMawb.PK);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.PK, mawb.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.Prefix, mawb.JM_ParentTableCode);

			AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock created", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);

				var previousMAWB = new BusinessObjectFactory().LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_MAWB, "11111210"));
				AssertEquals("Existing MAWB is returned back to stock", ZGuid.Empty, previousMAWB.JM_ParentID);
				AssertEquals("Existing MAWB is returned back to stock", ZString.Empty, previousMAWB.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolNeutralAndNoMatchingMAWBStockExistsAndMAWBNumberIsUsedOnAnotherConsol()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "11111210";
			mawb.JM_ServiceLevel = "STD";

			Factory.SaveForTesting();

			var consolBOToLoad = GetConsolBOToLoad();
			consolBOToLoad.JK_IsNeutralMaster = true;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111210", consolBOToLoad.JK_MasterBillNum);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.PK, mawb.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.Prefix, mawb.JM_ParentTableCode);

			var anotherConsol = Factory.New<ForwardingConsol>();
			anotherConsol.JK_UniqueConsignRef = "C00001050";
			anotherConsol.JK_RL_NKLoadPort = "AUMEL";
			anotherConsol.JK_RL_NKDischargePort = "JPTYO";
			anotherConsol.JK_AgentType = Constants.AgentType.Agent;
			anotherConsol.JK_TransportMode = Constants.TransportModes.Air;
			anotherConsol.JK_AWBServiceLevel = "STD";
			anotherConsol.MasterBillAirlinePrefix = "081";
			anotherConsol.JK_MasterBillNum = "08111111214";
			anotherConsol.JK_IsNeutralMaster = false;

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: matching MAWB number is used on another consol", true, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock created", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);

				var previousMAWB = new BusinessObjectFactory().LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_MAWB, "11111210"));
				AssertEquals("Existing MAWB is returned back to stock", ZGuid.Empty, previousMAWB.JM_ParentID);
				AssertEquals("Existing MAWB is returned back to stock", ZString.Empty, previousMAWB.JM_ParentTableCode);

				consolBO.RunPreSaveValidation();
				AssertHasError("Duplicate MAWB error expected", consolBO.MasterBillMAWBInfo, @"This Master Bill Number already exists on another Consol, Shipment or Booking.
Please select another number.");
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolNeutralAndMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			var mawb1 = Factory.New<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = "081";
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb1.JM_MAWB = "11111210";
			mawb1.JM_ServiceLevel = "STD";

			Factory.SaveForTesting();

			var consolBOToLoad = GetConsolBOToLoad();
			consolBOToLoad.JK_IsNeutralMaster = true;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111210", consolBOToLoad.JK_MasterBillNum);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.PK, mawb1.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.Prefix, mawb1.JM_ParentTableCode);

			var mawb2 = Factory.New<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "081";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb2.JM_MAWB = "11111214";
			mawb2.JM_ServiceLevel = "STD";
			mawb2.JM_IsPrinted = false;
			mawb2.JM_IsPaper = false;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='08111111210') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock allocated", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock allocated", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock allocated", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock allocated", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock allocated", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock allocated", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock allocated", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);

				var previousMAWB = new BusinessObjectFactory().LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_MAWB, "11111210"));
				AssertEquals("Existing MAWB is returned back to stock", ZGuid.Empty, previousMAWB.JM_ParentID);
				AssertEquals("Existing MAWB is returned back to stock", ZString.Empty, previousMAWB.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolNeutralAndMatchingMAWBStockExistsAndMAWBNumberIsUsedOnAnotherConsol()
		{
			var mawb1 = Factory.New<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = "081";
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb1.JM_MAWB = "11111210";
			mawb1.JM_ServiceLevel = "STD";

			Factory.SaveForTesting();

			var consolBOToLoad = GetConsolBOToLoad();
			consolBOToLoad.JK_IsNeutralMaster = true;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111210", consolBOToLoad.JK_MasterBillNum);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.PK, mawb1.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.Prefix, mawb1.JM_ParentTableCode);

			var mawb2 = Factory.New<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "081";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb2.JM_MAWB = "11111214";
			mawb2.JM_ServiceLevel = "STD";
			mawb2.JM_IsPrinted = false;
			mawb2.JM_IsPaper = false;

			Factory.SaveForTesting();

			var anotherConsol = Factory.New<ForwardingConsol>();
			anotherConsol.JK_UniqueConsignRef = "C00001050";
			anotherConsol.JK_RL_NKLoadPort = "AUMEL";
			anotherConsol.JK_RL_NKDischargePort = "JPTYO";
			anotherConsol.JK_AgentType = Constants.AgentType.Agent;
			anotherConsol.JK_TransportMode = Constants.TransportModes.Air;
			anotherConsol.JK_AWBServiceLevel = "STD";
			anotherConsol.MasterBillAirlinePrefix = "081";
			anotherConsol.JK_IsNeutralMaster = true;

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: matching MAWB number is used on another consol", true, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			AssertNotNull("Pre-condition: matching MAWB is allocated to another consol", anotherConsol.MAWBAllocation.AllocatedMawb);
			AssertEquals("Pre-condition: matching MAWB is allocated to another consol", mawb2.PK, anotherConsol.MAWBAllocation.AllocatedMawb.PK);
			AssertEquals("Pre-condition: matching MAWB is allocated to another consol", anotherConsol.PK, mawb2.JM_ParentID);
			AssertEquals("Pre-condition: matching MAWB is allocated to another consol", anotherConsol.TablePrefix, mawb2.JM_ParentTableCode);

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Import failed log", @"
ERROR - Attempted to import a consolidation with MAWB number 08111111214 that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import failed log", @"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Error - Attempted to import a consolidation with MAWB number 08111111214 that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import failed but matched existing consol by key", consolBO);
				AssertEquals("Existing consol unchanged", "08111111210", consolBOToLoad.JK_MasterBillNum);
				AssertEquals("Existing consol unchanged", consolBOToLoad.PK, mawb1.JM_ParentID);
				AssertEquals("Existing consol unchanged", consolBOToLoad.Prefix, mawb1.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_MatchingConsolAndMAWBNotExist_SecondImport()
		{
			var mawb1 = Factory.New<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = "112";
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb1.JM_MAWB = "11111332";
			mawb1.JM_ServiceLevel = "STD";

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB not exists in stock", false, MatchingMAWBExistsInStock("112", "65555626", "STD"));
			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("112", "11111332", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("11211111332"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey_WithMAWB.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import success log", @"Added Consol (Master Bill='11265555626') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='11265555626') with 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import success log", @"No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: CNPVG
Attempting to get Schedule for the Transport Leg
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Added Consol (Master Bill='11265555626') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='11265555626') with 1 x Transport."
.Trim(), logNoteText);
			});

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("112", "65555626", "STD"));
			AssertEquals("Pre-condition: MAWB number has been allocated", true, MAWBUsedOnAnotherConsol("11265555626"));
			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("112", "11111332", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("11211111332"));
			var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			AssertNotNull("Import successful, new consol created", consolBO);
			AssertEquals("Pre-condition: newly created consol is Neutral", true, consolBO.JK_IsNeutralMaster);

			message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey_WithMAWB2.xml"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import success log", @"Updated Consol C00001000 (Master Bill='11265555626') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='11265555626') with 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import success log", @"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: CNPVG
Attempting to get Schedule for the Transport Leg
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Updated Consol C00001000 (Master Bill='11265555626') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='11265555626') with 1 x Transport."
.Trim(), logNoteText);

				AssertEquals("Post-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("112", "11111332", "STD"));
				AssertEquals("Post-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("11211111332"));
			});
		}

		public void TestImportNeutralMasterConsol_MAWBOnlyContainsPrefix_MatchingConsolNeutralAndMatchingMAWBStockExistsAndMAWBNumberNotUsedOnAnotherConsol()
		{
			using (FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var mawb1 = Factory.New<JobMawb>();
				mawb1.JM_Airline3DigitPrefix = "081";
				mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb1.JM_MAWB = "11111210";
				mawb1.JM_ServiceLevel = "EXP";
				Factory.SaveForTesting();

				var mawb2 = Factory.New<JobMawb>();
				mawb2.JM_Airline3DigitPrefix = "099";
				mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb2.JM_MAWB = "11111214";
				mawb2.JM_ServiceLevel = "STD";
				Factory.SaveForTesting();

				var consolBOToLoad = Factory.New<ForwardingConsol>();
				consolBOToLoad.JK_UniqueConsignRef = "C00001052";
				consolBOToLoad.JK_RL_NKLoadPort = "AUSYD";
				consolBOToLoad.JK_RL_NKDischargePort = "USLAX";
				consolBOToLoad.JK_AgentType = Constants.AgentType.Agent;
				consolBOToLoad.JK_TransportMode = Constants.TransportModes.Air;
				consolBOToLoad.JK_AWBServiceLevel = "STD";
				consolBOToLoad.MasterBillAirlinePrefix = "099";
				consolBOToLoad.JK_AgentsReference = "Agent U";
				consolBOToLoad.JK_MasterBillNum = "09911111214";
				consolBOToLoad.JK_IsNeutralMaster = true;
				Factory.SaveForTesting();

				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey_MAWBOnlyContainsPrefix.xml"));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='081') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111210') with 1 x ForwardingConsolStmNote, 1 x Transport."
	.Trim(), logNoteText);

					var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
					AssertNotNull("Matched existing consol by key", consolBO);
					AssertEquals("08111111210", consolBO.JK_MasterBillNum);
					AssertEquals(true, consolBO.JK_IsNeutralMaster);
					AssertNotNull(consolBO.MAWBAllocation.AllocatedMawb);
					AssertEquals("EXP", consolBO.JK_AWBServiceLevel);
					AssertEquals("081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
					AssertEquals("11111210", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
					AssertEquals(consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
					AssertEquals(consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
				});
			}
		}

		#endregion

		#region TestImportNeutralMasterConsol - Existing Consol Expired

		public void TestImportNeutralMasterConsol_NoKeySpecified_MatchingConsolExpiredAndNotNeutral_MatchingMAWBStockExists()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var existingExpiredConsol = Factory.New<ForwardingConsol>();
			existingExpiredConsol.JK_RL_NKLoadPort = "AUMEL";
			existingExpiredConsol.JK_RL_NKDischargePort = "JPOSA";
			existingExpiredConsol.JK_AgentType = Constants.AgentType.Agent;
			existingExpiredConsol.JK_TransportMode = Constants.TransportModes.Air;
			existingExpiredConsol.JK_AWBServiceLevel = "STD";
			existingExpiredConsol.MasterBillAirlinePrefix = "081";
			existingExpiredConsol.JK_MasterBillNum = "08111111214";
			existingExpiredConsol.JK_IsNeutralMaster = false;
			existingExpiredConsol.JK_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-15);

			var matchingMAWB = Factory.New<JobMawb>();
			matchingMAWB.JM_Airline3DigitPrefix = "081";
			matchingMAWB.JM_GB = GlbBranch.CurrentBranch.PK;
			matchingMAWB.JM_MAWB = "11111214";
			matchingMAWB.JM_ServiceLevel = "STD";
			matchingMAWB.JM_IsPrinted = false;
			matchingMAWB.JM_IsPaper = false;
			Factory.SaveForTesting();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(existingExpiredConsol, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Added Consol from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
A Schedule has been found and linked to the Transport Leg.
Matching 'Carrier':- Matched to 'QANAIR' by code, address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Added Consol from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001001"));
				AssertNotNull("Import successful, new consol created", consolBO);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching  MAWB stock allocated to consol", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", matchingMAWB.PK, consolBO.MAWBAllocation.AllocatedMawb.PK);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_NoKeySpecified_MatchingConsolExpiredAndNeutral_MatchingMAWBStockExists()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var existingExpiredMAWB = Factory.New<JobMawb>();
			existingExpiredMAWB.JM_Airline3DigitPrefix = "081";
			existingExpiredMAWB.JM_GB = GlbBranch.CurrentBranch.PK;
			existingExpiredMAWB.JM_MAWB = "11111214";
			existingExpiredMAWB.JM_ServiceLevel = "STD";
			existingExpiredMAWB.JM_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-16);

			Factory.SaveForTesting();

			var existingExpiredConsol = Factory.New<ForwardingConsol>();
			existingExpiredConsol.JK_RL_NKLoadPort = "AUMEL";
			existingExpiredConsol.JK_RL_NKDischargePort = "JPOSA";
			existingExpiredConsol.JK_AgentType = Constants.AgentType.Agent;
			existingExpiredConsol.JK_TransportMode = Constants.TransportModes.Air;
			existingExpiredConsol.JK_AWBServiceLevel = "STD";
			existingExpiredConsol.MasterBillAirlinePrefix = "081";
			existingExpiredConsol.JK_IsNeutralMaster = true;
			existingExpiredConsol.JK_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-15);

			Factory.SaveForTesting();
			existingExpiredMAWB.Reload();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111214", existingExpiredConsol.JK_MasterBillNum);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", existingExpiredConsol.PK, existingExpiredMAWB.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", existingExpiredConsol.Prefix, existingExpiredMAWB.JM_ParentTableCode);

			var existingNewMAWB = Factory.New<JobMawb>();
			existingNewMAWB.JM_Airline3DigitPrefix = "081";
			existingNewMAWB.JM_GB = GlbBranch.CurrentBranch.PK;
			existingNewMAWB.JM_MAWB = "11111214";
			existingNewMAWB.JM_ServiceLevel = "STD";
			existingNewMAWB.JM_IsPrinted = false;
			existingNewMAWB.JM_IsPaper = false;
			Factory.SaveForTesting();

			existingNewMAWB.Reload();

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(existingExpiredConsol, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Added Consol from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
A Schedule has been found and linked to the Transport Leg.
Matching 'Carrier':- Matched to 'QANAIR' by code, address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Added Consol from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001001"));
				AssertNotNull("Import successful, new consol created", consolBO);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching  MAWB stock allocated to consol", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", existingNewMAWB.PK, consolBO.MAWBAllocation.AllocatedMawb.PK);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching  MAWB stock allocated to consol", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_NoKeySpecified_MatchingConsolExpiredAndNotNeutral_NoMatchingMAWBStockExists()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var existingExpiredConsol = Factory.New<ForwardingConsol>();
			existingExpiredConsol.JK_RL_NKLoadPort = "AUMEL";
			existingExpiredConsol.JK_RL_NKDischargePort = "JPOSA";
			existingExpiredConsol.JK_AgentType = Constants.AgentType.Agent;
			existingExpiredConsol.JK_TransportMode = Constants.TransportModes.Air;
			existingExpiredConsol.JK_AWBServiceLevel = "STD";
			existingExpiredConsol.MasterBillAirlinePrefix = "081";
			existingExpiredConsol.JK_MasterBillNum = "08111111214";
			existingExpiredConsol.JK_IsNeutralMaster = false;
			existingExpiredConsol.JK_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-15);

			Factory.SaveForTesting();

			AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(existingExpiredConsol, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Added Consol (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
A Schedule has been found and linked to the Transport Leg.
Matching 'Carrier':- Matched to 'QANAIR' by code, address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Added Consol (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001001"));
				AssertNotNull("Import successful, matched existing consol by key", consolBO);
				AssertEquals("Import successful, matching MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock created", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsol_NoKeySpecified_MatchingConsolExpiredAndNeutral_NoMatchingMAWBStockExists()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var existingExpiredMAWB = Factory.New<JobMawb>();
			existingExpiredMAWB.JM_Airline3DigitPrefix = "081";
			existingExpiredMAWB.JM_GB = GlbBranch.CurrentBranch.PK;
			existingExpiredMAWB.JM_MAWB = "11111214";
			existingExpiredMAWB.JM_ServiceLevel = "STD";
			existingExpiredMAWB.JM_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-16);

			Factory.SaveForTesting();

			var existingExpiredConsol = Factory.New<ForwardingConsol>();
			existingExpiredConsol.JK_RL_NKLoadPort = "AUMEL";
			existingExpiredConsol.JK_RL_NKDischargePort = "JPOSA";
			existingExpiredConsol.JK_AgentType = Constants.AgentType.Agent;
			existingExpiredConsol.JK_TransportMode = Constants.TransportModes.Air;
			existingExpiredConsol.JK_AWBServiceLevel = "STD";
			existingExpiredConsol.MasterBillAirlinePrefix = "081";
			existingExpiredConsol.JK_IsNeutralMaster = true;
			existingExpiredConsol.JK_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-15);

			Factory.SaveForTesting();
			existingExpiredMAWB.Reload();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111214", existingExpiredConsol.JK_MasterBillNum);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", existingExpiredConsol.PK, existingExpiredMAWB.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", existingExpiredConsol.Prefix, existingExpiredMAWB.JM_ParentTableCode);

			AssertEquals("Pre-condition: matching MAWB exists in stock", true, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(existingExpiredConsol, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Added Consol (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
A Schedule has been found and linked to the Transport Leg.
Matching 'Carrier':- Matched to 'QANAIR' by code, address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Added Consol (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001001 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001001"));
				AssertNotNull("Import successful, new consol created", consolBO);
				AssertEquals("Import successful, matching MAWB stock created", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Import successful, matching MAWB stock created", true, consolBO.JK_IsNeutralMaster);
				AssertNotNull("Import successful, matching MAWB stock created", consolBO.MAWBAllocation.AllocatedMawb);
				AssertEquals("Import successful, matching MAWB stock created", "081", consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
				AssertEquals("Import successful, matching MAWB stock created", "11111214", consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.PK, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentID);
				AssertEquals("Import successful, matching MAWB stock created", consolBO.Prefix, consolBO.MAWBAllocation.AllocatedMawb.JM_ParentTableCode);
			});
		}

		#endregion

		#region TestImportNeutralMasterConsol - Other cases

		public void TestImportNeutralMasterConsol_MatchingConsolFinalMasterPrinted()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "11111210";
			mawb.JM_ServiceLevel = "STD";
			Factory.SaveForTesting();

			var consolBOToLoad = GetConsolBOToLoad();
			consolBOToLoad.JK_IsNeutralMaster = true;
			Factory.SaveForTesting();
			mawb.JM_IsPrinted = true;
			Factory.SaveForTesting();

			consolBOToLoad.Reload();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111210", consolBOToLoad.JK_MasterBillNum);
			AssertNotNull("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.MAWBAllocation.AllocatedMawb);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", mawb.PK, consolBOToLoad.MAWBAllocation.AllocatedMawb.PK);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.PK, mawb.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.Prefix, mawb.JM_ParentTableCode);
			AssertEquals("Pre-condition: existing consol has Final MAWB printed", true, consolBOToLoad.IsNeutralMAWBPrinted);

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Import failed log", @"
ERROR - Attempted to import a neutral master consolidation with a new MAWB number 08111111214. Cannot create/allocate another MAWB to this consolidation, as the Final Master has already been printed.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import failed log", @"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Error - Attempted to import a neutral master consolidation with a new MAWB number 08111111214. Cannot create/allocate another MAWB to this consolidation, as the Final Master has already been printed.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import failed but matched existing consol by key", consolBO);

				AssertEquals("Existing consol unchanged", "08111111210", consolBO.JK_MasterBillNum);
				AssertEquals("Existing consol unchanged", consolBO.PK, mawb.JM_ParentID);
				AssertEquals("Existing consol unchanged", consolBO.Prefix, mawb.JM_ParentTableCode);
			});
		}

		public void TestImportNeutralMasterConsolWhenCreateAndAllocateNeutralStockFalse_MatchingConsolNeutral()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "11111210";
			mawb.JM_ServiceLevel = "STD";
			Factory.SaveForTesting();

			var consolBOToLoad = GetConsolBOToLoad();
			consolBOToLoad.JK_IsNeutralMaster = true;
			Factory.SaveForTesting();

			mawb.Reload();

			AssertEquals("Pre-condition: existing consol has MAWB allocated", "08111111210", consolBOToLoad.JK_MasterBillNum);
			AssertNotNull("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.MAWBAllocation.AllocatedMawb);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", mawb.PK, consolBOToLoad.MAWBAllocation.AllocatedMawb.PK);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.PK, mawb.JM_ParentID);
			AssertEquals("Pre-condition: existing consol has MAWB allocated", consolBOToLoad.Prefix, mawb.JM_ParentTableCode);

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey_DoNotCreateAndAllocateNeutralStock.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message processed OK", EDIMessage.Status.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Import successful log", @"
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Updated Consol C00001052 (Master Bill='08111111214') from UniversalShipment.
Successfully saved Consol C00001052 (Master Bill='08111111214') with 1 x ForwardingConsolStmNote, 1 x Transport."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import successful", consolBO);

				AssertEquals("Existing consol updated", "08111111214", consolBO.JK_MasterBillNum);
				AssertEquals("Existing consol updated", false, consolBO.JK_IsNeutralMaster);

				var previousMAWB = new BusinessObjectFactory().LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_MAWB, "11111210"));
				AssertEquals("Existing MAWB is returned back to stock", ZGuid.Empty, previousMAWB.JM_ParentID);
				AssertEquals("Existing MAWB is returned back to stock", ZString.Empty, previousMAWB.JM_ParentTableCode);
			});
		}

		[TestDate(2024, 5, 29, 0, 0, 0)]
		public void TestImportNeutralMasterConsol_CreateAndAllocateNeutralStockIsTrue_WithAllocatedAndExpiredMAWB()
		{
			TestCase(ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value - 1), "081", "00000000");
			TestCase(ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value), "082", "00000011");

			void TestCase(ZDateTime expiredTime, string prefix, string billNumber)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
				{
					var existingConsol = Factory.New<ForwardingConsol>();
					existingConsol.JK_MasterBillNum = $"{prefix}{billNumber}";

					var expiredMAWB = Factory.New<JobMawb>();
					expiredMAWB.JM_Airline3DigitPrefix = prefix;
					expiredMAWB.JM_MAWB = billNumber;
					expiredMAWB.JM_ServiceLevel = OrgCarrierServiceLevel.AllCode;
					expiredMAWB.JM_GB = GlbBranch.CurrentBranch.PK;
					expiredMAWB.JM_ParentID = existingConsol.PK;
					expiredMAWB.JM_ParentTableCode = existingConsol.Prefix;
					expiredMAWB.JM_SystemCreateTimeUtc = expiredTime;
					Factory.SaveForTesting();

					var consolDataObject = SetupConsol();
					consolDataObject.TransportMode = new CodeDescriptionPair { Code = TransportModes.Air };
					consolDataObject.WayBillNumber = $"{prefix}-{billNumber}";
					consolDataObject.IsNeutralMaster.Value = true;
					consolDataObject.IsNeutralMaster.CreateAndAllocateNeutralStock = true;

					var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
					var consolBO = reader.ReadIntoBusinessObject();

					CombineAssertions(() =>
					{
						AssertNoExceptionThrown(() => Factory.SaveForTesting());
						AssertNotEquals("New consol created", consolBO.PK, existingConsol.PK);
						AssertNotEquals("New MAWB created", expiredMAWB.PK, consolBO.MAWBAllocation.AllocatedMawb.PK);
						AssertEquals("New MAWB has same airline prefix", expiredMAWB.JM_Airline3DigitPrefix, consolBO.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix);
						AssertEquals("New MAWB has same number", expiredMAWB.JM_MAWB, consolBO.MAWBAllocation.AllocatedMawb.JM_MAWB);
						AssertGreaterThan("New MAWB is not expired",
							consolBO.MAWBAllocation.AllocatedMawb.JM_SystemCreateTimeUtc,
							ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
					});
				}
			}
		}

		public void TestImportNeutralMasterConsol_FailedToCreateNewMAWBStockDueToDuplicateNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				var mawb = Factory.New<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_MAWB = "11111214";
				mawb.JM_ServiceLevel = "GTW";
				mawb.JM_IsPrinted = false;
				mawb.JM_IsPaper = false;
				Factory.SaveForTesting();

				AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
				AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol("08111111214"));

				var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFile.xml"));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Import failed log", @"
ERROR - Attempted to import a consolidation with MAWB number 08111111214 that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data."
	.Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("Import failed log", @"No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Attempting to get Schedule for the Transport Leg
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Error - Attempted to import a consolidation with MAWB number 08111111214 that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
	.Trim(), logNoteText);

					var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
					AssertNull("Import failed, no consol created", consolBO);
				});
			}
		}

		public void TestImportNeutralMasterConsol_WithEmptyMAWBNumber()
		{
			var consolBOToLoad = GetConsolBOToLoad();

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey_EmptyMAWB.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Import failed log", @"
ERROR - Attempted to import a neutral master consolidation without MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import failed log", @"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Error - Attempted to import a neutral master consolidation without MAWB number.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import failed but matched existing consol by key", consolBO);

				AssertEquals("Existing consol unchanged", "08111111210", consolBO.JK_MasterBillNum);
			});
		}

		public void TestImportXmlWithMawb_MatchExistingConsol()
		{
			TestImportXmlWithMawb_MatchExistingConsol_Case("001", TransportModes.Air, shouldMatchExistingConsol: false);
			TestImportXmlWithMawb_MatchExistingConsol_Case("001", TransportModes.Sea, shouldMatchExistingConsol: true);
			TestImportXmlWithMawb_MatchExistingConsol_Case("001-1234567", TransportModes.Air, shouldMatchExistingConsol: false);
			TestImportXmlWithMawb_MatchExistingConsol_Case("001-12345678", TransportModes.Air, shouldMatchExistingConsol: true);
		}

		void TestImportXmlWithMawb_MatchExistingConsol_Case(string mawbNum, string transportMode, bool shouldMatchExistingConsol)
		{
			var consolInDb = Factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052")).FirstOrDefault() ?? GetConsolBOToLoad();
			consolInDb.JK_MasterBillNum = mawbNum;
			consolInDb.JK_TransportMode = transportMode;
			Factory.SaveForTesting();

			var importXml = ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey_WithOnlyMAWBPrefix.xml")
				.Replace("{mawbNum}", mawbNum).Replace("{transport}", transportMode);
			var message = GetQueuedUniversalShipmentMessage(importXml);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var logNoteText = message.GetLogNoteText();
			AssertContains(shouldMatchExistingConsol ?
				"Successfully loaded matching ForwardingConsol." : "No matching ForwardingConsol found, creating new ForwardingConsol.", logNoteText);
		}

		public void TestImportXmlWithMawbPrefix_Set_JK_IsNeutralMaster()
		{
			var mawbPrefix = "001";
			var mawb1 = Factory.New<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = mawbPrefix;
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb1.JM_MAWB = "11111214";
			mawb1.JM_ServiceLevel = "STD";
			mawb1.JM_IsPrinted = false;
			mawb1.JM_IsPaper = false;
			Factory.SaveForTesting();

			TestImportXmlWithMawbPrefix_Set_JK_IsNeutralMaster_Case(mawbPrefix, "AUSYD", expectedJK_IsNeutralMaster: true);
			TestImportXmlWithMawbPrefix_Set_JK_IsNeutralMaster_Case(mawbPrefix, "USATL", expectedJK_IsNeutralMaster: false);
		}

		void TestImportXmlWithMawbPrefix_Set_JK_IsNeutralMaster_Case(string mawbPrefix, string loadPort, bool expectedJK_IsNeutralMaster)
		{
			var importXml = ReadTextFromEmbeddedResourceFile("AirConsolImportFile_NoKey_WithOnlyMAWBPrefix.xml")
				.Replace("{mawbNum}", mawbPrefix).Replace("{transport}", "AIR").Replace("{loadPort}", loadPort);
			var message = GetQueuedUniversalShipmentMessage(importXml);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			using (FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				manager.Process(message);
			}

			var createdConsolNum = GetConsolNumberFromLog(serviceTaskLog);
			var createdConsol = Factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, createdConsolNum)).FirstOrDefault();
			AssertEquals(expectedJK_IsNeutralMaster, createdConsol.JK_IsNeutralMaster);
		}

		static string GetConsolNumberFromLog(ServiceTaskLogForTesting serviceLog)
		{
			var log = serviceLog.ToString();
			var start = log.LastIndexOf("Consol") + 6;
			var end = log.LastIndexOf("(Master Bill=");

			if (end > start && start > 0)
			{
				return log.Substring(start, end - start).Trim();
			}

			return string.Empty;
		}

		public void TestImportNeutralMasterConsol_FailsIfMAWBMaxLengthIsExceeded()
		{
			var consolBOToLoad = GetConsolBOToLoad();

			AssertEquals("Pre-condition: no matching MAWB exists in stock", false, MatchingMAWBExistsInStock("081", "11111214", "STD"));
			AssertEquals("Pre-condition: no other consol using matching MAWB number", false, MAWBUsedOnAnotherConsol(consolBOToLoad, "08111111214"));

			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile("AirConsolImportFileWithKey_InvalidMAWB.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			CombineAssertions(delegate
			{
				AssertNoExceptionThrown(() => manager.Process(message));

				AssertEquals("Message discarded", EDIMessage.Status.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Import failed log", @"
ERROR - Attempted to import an Air consol with an invalid MAWB number: '081111112141548751'. Maximum of 11 digits are allowed.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data."
.Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import failed log", @"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: USLAX
Matching 'Carrier':- Matched to 'QANAIR' address 'PST: PO BOX 372' with a score of 140.
Transport Leg updated.
Error - Attempted to import an Air consol with an invalid MAWB number: '081111112141548751'. Maximum of 11 digits are allowed.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded."
.Trim(), logNoteText);

				var consolBO = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001052"));
				AssertNotNull("Import failed but matched existing consol by key", consolBO);

				AssertEquals("Existing consol unchanged", "08111111210", consolBO.JK_MasterBillNum);
			});
		}

		public void TestImportConsol_DefaultCarrierBasedOnMAWB()
		{
			var secondFactory = new BusinessObjectFactory();
			var ekMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			ekMiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(secondFactory, "176").PK;
			ekMiscServ.Header.OH_FullName = "Emirate Agent";

			var qfMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			qfMiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(secondFactory, "081").PK;
			qfMiscServ.Header.OH_FullName = "Qantas Agent";

			var importXml = ReadTextFromEmbeddedResourceFile("AirConsolImportFile.xml")
			.Replace("Sea", "Air").Replace("08111111214", "176-11111214").Replace("Main", "Flight1").RemoveBetween("ActualDeparture>", "<CarrierBookingReference");

			var message = GetQueuedUniversalShipmentMessage(importXml);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var createdConsol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			//Flight number is QF, yet MAWB prefix is 176(Emirates). This should be mapped to Emirate Agent.
			AssertEquals("Emirate Agent", createdConsol.JK_OA_ShippingLineAddress_ZAddress.Address);
		}

		public void TestImportConsol_DoNotDefaultCarrierBasedOnMAWBIfCarrierAndCreditorExist()
		{
			var secondFactory = new BusinessObjectFactory();
			var ekMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			ekMiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(secondFactory, "176").PK;
			ekMiscServ.Header.OH_FullName = "Emirate Agent";

			var qfMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			qfMiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(secondFactory, "081").PK;
			qfMiscServ.Header.OH_FullName = "Qantas Agent";

			var realMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			realMiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(secondFactory, "125").PK;
			realMiscServ.Header.OH_FullName = "SILVERWATER CHUTE ENTERPRISES";
			realMiscServ.Header.OH_Code = "SILVCHUENT";

			var orgGenerator = new OrganisationTestHelper(Factory);
			var shippingLineDataObject = orgGenerator.CreateDataObject("SILVERWATER", "CHUTE", "4792");
			shippingLineDataObject.AddressType = nameof(DocAddressType.ShippingLineAddress);
			shippingLineDataObject.OrganizationCode = "SILVCHUENT";

			Factory.SaveForTesting();

			var importXml = ReadTextFromEmbeddedResourceFile("AirConsolImportFile.xml")
			.Replace("Sea", "Air").Replace("08111111214", "176-17654321").Replace("Main", "Flight1").Replace("\t</NoteCollection>\r\n\r\n\t<TransportLegCollection>", "\t</NoteCollection>\r\n\t<OrganizationAddressCollection>\r\n\t  <OrganizationAddress>\r\n        <AddressType>ShippingLineAddress</AddressType>\r\n        <Address1>792 SILVERWATER BOULEVARDE</Address1>\r\n        <Address2 />\r\n        <AddressOverride>false</AddressOverride>\r\n        <AddressShortCode>792 SILVERWATER BOULEVARDE</AddressShortCode>\r\n        <City>CHUTEVILLE</City>\r\n        <CompanyName>SILVERWATER CHUTE ENTERPRISES</CompanyName>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>AUSTRALIA</Name>\r\n        </Country>\r\n        <Email />\r\n        <Fax />\r\n        <OrganizationCode>SILVCHUENT</OrganizationCode>\r\n        <Phone />\r\n        <Port>\r\n          <Code>AURCH</Code>\r\n          <Name />\r\n        </Port>\r\n        <Postcode>4792</Postcode>\r\n        <ScreeningStatus>\r\n          <Code>CLR</Code>\r\n          <Description>Clear</Description>\r\n        </ScreeningStatus>\r\n        <State>NSW</State>\r\n      </OrganizationAddress>\r\n    </OrganizationAddressCollection>\r\n\t<TransportLegCollection>");

			var message = GetQueuedUniversalShipmentMessage(importXml);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var createdConsol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
			//Even though MAWB prefix is 176(Emirates) and flight number is QF, this should be mapped to "SILVERWATER CHUTE ENTERPRISES" as we have the Shipping Line Address for it.
			AssertEquals("SILVERWATER CHUTE ENTERPRISES", createdConsol.JK_OA_ShippingLineAddress_ZAddress.Address);
		}

		#endregion

		#region Test Handling Types
		public void TestSendingForwaderHandlingType_UpdatesOnConsolidation()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.SendingForwarderHandlingType = new CodeDescriptionPair() { Code = AgentStatusList.Codes.GatewayAgent };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("Consol Sending Type has been updated", AgentStatusList.Codes.GatewayAgent, consolBO.JK_SendingForwarderHandlingType);
		}

		public void TestReceivingForwarderHandlingType_UpdatesOnConsolidation()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.ReceivingForwarderHandlingType = new CodeDescriptionPair() { Code = AgentStatusList.Codes.GatewayAgent };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("Consol Receiving Type has been updated", AgentStatusList.Codes.GatewayAgent, consolBO.JK_ReceivingForwarderHandlingType);
		}

		public void TestSendingAndReceivingHandlingType_UpdatesOnConsolidation()
		{
			var consolDataObject = SetupConsol();
			consolDataObject.SendingForwarderHandlingType = new CodeDescriptionPair() { Code = AgentStatusList.Codes.GatewayAgent };
			consolDataObject.ReceivingForwarderHandlingType = new CodeDescriptionPair() { Code = AgentStatusList.Codes.GatewayAgentWithTariff };

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("Consol Sending Type has been updated", AgentStatusList.Codes.GatewayAgent, consolBO.JK_SendingForwarderHandlingType);
			AssertEquals("Consol Receiving Type has been updated", AgentStatusList.Codes.GatewayAgentWithTariff, consolBO.JK_ReceivingForwarderHandlingType);
		}

		public void TestHandlingTypesAreEmpty_ConsolidationIsUnchanged()
		{
			var consolDataObject = SetupConsol();
			AssertEquals("Precondition: Handling types are empty", null, consolDataObject.SendingForwarderHandlingType);
			AssertEquals("Precondition: Handling types are empty", null, consolDataObject.ReceivingForwarderHandlingType);

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("Consol Receiving Type has not been updated", ZString.Empty, consolBO.JK_ReceivingForwarderHandlingType);
			AssertEquals("Consol Sending Type has not been updated", ZString.Empty, consolBO.JK_SendingForwarderHandlingType);
		}

		#endregion

		#region OverrideConsolChargeable

		public void TestJKOverrideConsolChargeable_ShouldBeTrue_OnlyWhenAllValuesArePresentInXml_WhenImportXmlCreatesNewConsol()
		{
			var unitOfVolume = new UnitOfVolume() { Code = Constants.Volume.CubicMetres };
			var unitOfWeight = new UnitOfWeight() { Code = Constants.Weight.Kilograms };

			AssertJKOverrideConsolChargeableForNewConsol(weight: null, volume: 18, unitOfVolume: unitOfVolume, unitOfWeight: unitOfWeight, chargeable: 18, expectedOverrideConsolChargeableValue: false);
			AssertJKOverrideConsolChargeableForNewConsol(weight: 18, volume: null, unitOfVolume: unitOfVolume, unitOfWeight: unitOfWeight, chargeable: 18, expectedOverrideConsolChargeableValue: false);
			AssertJKOverrideConsolChargeableForNewConsol(weight: 18, volume: 18, unitOfVolume: null, unitOfWeight: unitOfWeight, chargeable: 18, expectedOverrideConsolChargeableValue: false);
			AssertJKOverrideConsolChargeableForNewConsol(weight: 18, volume: 18, unitOfVolume: unitOfVolume, unitOfWeight: null, chargeable: 18, expectedOverrideConsolChargeableValue: false);
			AssertJKOverrideConsolChargeableForNewConsol(weight: 18, volume: 18, unitOfVolume: unitOfVolume, unitOfWeight: unitOfWeight, chargeable: null, expectedOverrideConsolChargeableValue: false);
			AssertJKOverrideConsolChargeableForNewConsol(chargeable: 18, volume: 18, unitOfVolume: unitOfVolume, weight: 18, unitOfWeight: unitOfWeight, expectedOverrideConsolChargeableValue: true);
			AssertJKOverrideConsolChargeableForNewConsol(chargeable: 0, volume: 0, unitOfVolume: unitOfVolume, weight: 0, unitOfWeight: unitOfWeight, expectedOverrideConsolChargeableValue: true);
		}

		public void TestJKOverrideConsolChargeable_ShouldChangeCurrentValue_OnlyWhenAllValuesArePresentInXml_WhenImportXmlUpdatesExistingConsol()
		{
			var unitOfWeight = new UnitOfWeight() { Code = Constants.Weight.Kilograms };
			var unitOfVolume = new UnitOfVolume() { Code = Constants.Volume.CubicMetres };

			AssertJKOverrideConsolChargeableForExistingConsol(order: "01", overrideConsolChargeableOldValue: true, expectedOverrideConsolChargeableValue: true);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "02", overrideConsolChargeableOldValue: false, expectedOverrideConsolChargeableValue: false);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "03", overrideConsolChargeableOldValue: true, weight: 18, volume: 18, chargeable: 18, unitOfWeight: unitOfWeight, unitOfVolume: null, expectedOverrideConsolChargeableValue: true);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "04", overrideConsolChargeableOldValue: false, weight: 18, volume: 18, chargeable: 18, unitOfWeight: unitOfWeight, unitOfVolume: null, expectedOverrideConsolChargeableValue: false);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "05", overrideConsolChargeableOldValue: false, weight: 18, volume: 18, chargeable: 18, unitOfWeight: unitOfWeight, unitOfVolume: unitOfVolume, expectedOverrideConsolChargeableValue: true);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "06", overrideConsolChargeableOldValue: true, weight: 18, volume: 18, chargeable: 18, unitOfWeight: unitOfWeight, unitOfVolume: unitOfVolume, expectedOverrideConsolChargeableValue: true);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "07", overrideConsolChargeableOldValue: false, weight: 0, volume: 0, chargeable: 0, unitOfWeight: unitOfWeight, unitOfVolume: unitOfVolume, expectedOverrideConsolChargeableValue: true);

			AssertJKOverrideConsolChargeableForExistingConsol(order: "08", overrideConsolChargeableOldValue: true, weight: 0, volume: 0, chargeable: 0, unitOfWeight: unitOfWeight, unitOfVolume: unitOfVolume, expectedOverrideConsolChargeableValue: true);
		}

		public void TestImport_ShouldNotChangeValuesAndShouldAddWarningLog_WhenSomeCarrierCorrectedFieldsAreNotPresentInXml()
		{
			var consol = GetConsolBOToLoad();
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 12;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Milligrams;
			consol.JK_CorrectedConsolVolume = 134;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicFeet;
			consol.JK_ConsolChargeable = 60;

			Factory.SaveForTesting();

			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testUShipment.DataContext = DataContextFactory.New();
			testUShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C00001052");

			testUShipment.CarrierCorrectedWeight = 14;
			testUShipment.CarrierCorrectedWeightUnit = new UnitOfWeight() { Code = Constants.Weight.Kilograms };
			testUShipment.CarrierCorrectedChargeable = 1200;
			testUShipment.CarrierCorrectedVolume = 14;
			testUShipment.CarrierCorrectedVolumeUnit = null;

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			var uxmlImportAffectedConsol = reader.ReadIntoBusinessObject();

			AssertContains("Warning - Some of the carrier corrected details were missing and have been ignored.", logger.Logs);

			AssertEquals(true, uxmlImportAffectedConsol.JK_OverrideConsolChargeable);
			AssertEquals(new ZDecimal(12), uxmlImportAffectedConsol.JK_CorrectedConsolWeight);
			AssertEquals(Constants.Weight.Milligrams, uxmlImportAffectedConsol.JK_CorrectedConsolWeightUnit);
			AssertEquals(new ZDecimal(134), uxmlImportAffectedConsol.JK_CorrectedConsolVolume);
			AssertEquals(Constants.Volume.CubicFeet, uxmlImportAffectedConsol.JK_CorrectedConsolVolumeUnit);
			AssertEquals(new ZDecimal(60), uxmlImportAffectedConsol.JK_ConsolChargeable);
		}

		public void TestImport_ShouldChangeValues_WhenAllCarrierCorrectedFieldsArePresentInXml()
		{
			var consol = GetConsolBOToLoad();
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 12;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Milligrams;
			consol.JK_CorrectedConsolVolume = 134;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicFeet;
			consol.JK_ConsolChargeable = 60;

			Factory.SaveForTesting();

			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testUShipment.DataContext = DataContextFactory.New();
			testUShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C00001052");

			testUShipment.CarrierCorrectedWeight = 14;
			testUShipment.CarrierCorrectedWeightUnit = new UnitOfWeight() { Code = Constants.Weight.Kilograms };
			testUShipment.CarrierCorrectedChargeable = 1200;
			testUShipment.CarrierCorrectedVolume = 14;
			testUShipment.CarrierCorrectedVolumeUnit = new UnitOfVolume { Code = Constants.Volume.CubicMetres };

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			var uxmlImportAffectedConsol = reader.ReadIntoBusinessObject();

			AssertEquals(true, uxmlImportAffectedConsol.JK_OverrideConsolChargeable);
			AssertEquals(new ZDecimal(14), uxmlImportAffectedConsol.JK_CorrectedConsolWeight);
			AssertEquals(Constants.Weight.Kilograms, uxmlImportAffectedConsol.JK_CorrectedConsolWeightUnit);
			AssertEquals(new ZDecimal(14), uxmlImportAffectedConsol.JK_CorrectedConsolVolume);
			AssertEquals(Constants.Volume.CubicMetres, uxmlImportAffectedConsol.JK_CorrectedConsolVolumeUnit);
			AssertEquals(new ZDecimal(1200), uxmlImportAffectedConsol.JK_ConsolChargeable);
		}

		#endregion

		#region MaximumAllowableMeasures

		public void TestImport_ShouldReadMaximumAllowableValuesFromXML()
		{
			var consol = GetConsolBOToLoad();
			consol.JK_MaximumAllowablePackageLength = 12;
			consol.JK_MaximumAllowablePackageWidth = 134;
			consol.JK_MaximumAllowablePackageHeight = 60;
			consol.JK_MaximumAllowablePackageUnit = Constants.Length.Feet;

			Factory.SaveForTesting();

			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testUShipment.DataContext = DataContextFactory.New();
			testUShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C00001052");

			testUShipment.MaximumAllowablePackageLength = 120;
			testUShipment.MaximumAllowablePackageWidth = 13;
			testUShipment.MaximumAllowablePackageHeight = 9;
			testUShipment.MaximumAllowablePackageLengthUnit = new UnitOfLength() { Code = Constants.Length.Centimetres };

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			var uxmlImportAffectedConsol = reader.ReadIntoBusinessObject();

			AssertEquals(new ZDecimal(120), uxmlImportAffectedConsol.JK_MaximumAllowablePackageLength);
			AssertEquals(new ZDecimal(13), uxmlImportAffectedConsol.JK_MaximumAllowablePackageWidth);
			AssertEquals(new ZDecimal(9), uxmlImportAffectedConsol.JK_MaximumAllowablePackageHeight);
			AssertEquals(Constants.Length.Centimetres, uxmlImportAffectedConsol.JK_MaximumAllowablePackageUnit);
		}

		#endregion

		#region eBooking Import

		public void TestImportEBookingReply_WithFreightCharges_CurrentCompany_StandardChargeCode()
		{
			AssertImportEBookingReply_WithFreightCharges(
				companyCodeToImportInto: GlbCompany.CurrentCompany.GC_Code,
				expectedChargeCode: "FRT");
		}

		public void TestImportEBookingReply_WithFreightCharges_CurrentCompany_CustomChargeCode()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";

			Factory.SaveForTesting();

			Env.Registry.FreightChargeCode = chargeCode.PK.ToGuid();

			AssertImportEBookingReply_WithFreightCharges(
				companyCodeToImportInto: GlbCompany.CurrentCompany.GC_Code,
				expectedChargeCode: "TST");
		}

		public void TestImportEBookingReply_WithFreightCharges_AnotherCompany_CustomChargeCode()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TS1";

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TS2";

			Factory.SaveForTesting();

			var otherCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			var otherCompanyBranch = otherCompany.Branches.First();

			Env.Registry.FreightChargeCode = chargeCode1.PK.ToGuid();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Env.Registry.FreightChargeCode = chargeCode2.PK.ToGuid();
			}

			AssertImportEBookingReply_WithFreightCharges(
				companyCodeToImportInto: "DEM",
				expectedChargeCode: "TS2");
		}

		void AssertImportEBookingReply_WithFreightCharges(string companyCodeToImportInto, string expectedChargeCode)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "05722386700";
			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKDischargePort = "USJFK";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";

			Factory.SaveForTesting();

			var usxml = CreateEBookingMessage(consol.JK_UniqueConsignRef, companyCodeToImportInto);

			var message = GetQueuedUniversalShipmentMessage(usxml);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var expectedMessageProcessingLog = $@"Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Populating ForwardingConsolStmNote...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: CDG Destination: JFK
Transport Leg updated.
Imported JobConsolCost {expectedChargeCode} 440.45 EUR.
Updated Consol C00001000 (Master Bill='05722386700') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='05722386700') with 1 x ForwardingConsolStmNote, 1 x Transport.";

			void AssertEBookingImport()
			{
				AssertEquals("message.EM_Status",
					EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task USXml Import Log",
					expectedMessageProcessingLog, manager.Logger.ToString());

				var importCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCodeToImportInto);
				var importCompanyBranch = importCompany.Branches.First();

				var consolCostingQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK);
				var consolCosts = Factory.BOFactory.Load<IJobConsolCost>(consolCostingQuery);
				AssertEquals("consol cost has been created", 1, consolCosts.Length);

				var consolCostBizObj = (BusinessObject)consolCosts[0];

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, importCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("used import company FreightCharge Code E6_AC_ChargeCode", Env.Registry.FreightChargeCode, consolCostBizObj[JobConsolCostSchema.Constants.E6_AC_ChargeCode]);
				}
				AssertEquals("charge was created for the import company E6_GC)", importCompany.PK, consolCostBizObj[JobConsolCostSchema.Constants.E6_GC]);
				AssertEquals("imported SupplierReference from USXml into E6_CostReference", "48525500", consolCostBizObj[JobConsolCostSchema.Constants.E6_CostReference]);
				AssertEquals("imported CostOSAmount from USXml into E6_OSCostAmount", 440.45m, consolCostBizObj[JobConsolCostSchema.Constants.E6_OSCostAmount]);
				AssertEquals("imported CostOSCurrency from USXml into E6_RX_NKCurrency", "EUR", consolCostBizObj[JobConsolCostSchema.Constants.E6_RX_NKCurrency]);
				AssertEquals("imported ApportionmentMethod from USXml into E6_ApportionmentMethod", "CHG", consolCostBizObj[JobConsolCostSchema.Constants.E6_ApportionmentMethod]);
				AssertEquals("imported RatingBehaviour from USXml into E6_RatingBehaviour", "SPT", consolCostBizObj[JobConsolCostSchema.Constants.E6_RatingBehaviour]);
			}

			CombineAssertions(AssertEBookingImport);
		}

		#region eBooking USXml

		string CreateEBookingMessage(string consolID, string companyCode) => $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
      <DataContext>
        <DocumentaryOverride>
          <DocumentName>AirBooking</DocumentName>
          <SubmissionVersion>4</SubmissionVersion>
        </DocumentaryOverride>
        <DataTargetCollection>
          <DataTarget>
            <Key>{consolID}</Key>
            <Type>ForwardingConsol</Type>
          </DataTarget>
        </DataTargetCollection>
        <Workflow>
          <CodesMappedToTarget>true</CodesMappedToTarget>
          <Company>
            <Code>{companyCode}</Code>
          </Company>
        </Workflow>
      </DataContext>
      <WayBillNumber>057-22386700</WayBillNumber>
      <PortOfOrigin>CDG</PortOfOrigin>
      <PortOfDestination>JFK</PortOfDestination>
      <TotalNoOfPacks>7</TotalNoOfPacks>
      <TotalWeight>123</TotalWeight>
      <TotalWeightUnit>KG</TotalWeightUnit>
      <TotalVolume>0.637875</TotalVolume>
      <TotalVolumeUnit>M3</TotalVolumeUnit>
      <BookingConfirmationReference>48525500</BookingConfirmationReference>
      <TransportLegCollection>
        <TransportLeg>
          <LegOrder>1</LegOrder>
          <PortOfLoading>CDG</PortOfLoading>
          <PortOfDischarge>JFK</PortOfDischarge>
          <EstimatedDeparture>2021-12-01T20:55:00</EstimatedDeparture>
          <EstimatedArrival>2021-12-02T06:15:00</EstimatedArrival>
          <VoyageFlightNo>AF1234</VoyageFlightNo>
          <BookingStatus>CNF</BookingStatus>
          <TransportMode>AIR</TransportMode>
          <LegType>Flight1</LegType>
        </TransportLeg>
      </TransportLegCollection>
      <NoteCollection>
        <Note>
          <Description>Booking Confirmation Notes</Description>
          <NoteText>- The total cost provided by the Airline for this eBooking is: 440.45 EUR
- The breakdown of charges provided by the Airline for this eBooking is as follows:
  Freight Charge: 356.7 EUR, Valuation: 0.0 EUR, Other (CG): 3.0 EUR, Other (SC): 8.2 EUR, Other (MY): 43.05 EUR, Other (CH): 29.5 EUR
- Please note the Total Volume for this eBooking request (0.64 M3) is different to what the Airline confirmed (0.637875 M3)</NoteText>
          <IsCustomDescription>true</IsCustomDescription>
        </Note>
      </NoteCollection>
      <ConsolCosts>
        <ConsolCostLineCollection>
          <ConsolCostLine>
            <ChargeCode>
              <Code>FRT</Code>
              <Description>International Freight</Description>
            </ChargeCode>
            <SupplierReference>48525500</SupplierReference>
            <CostOSAmount>440.45</CostOSAmount>
            <CostOSCurrency>EUR</CostOSCurrency>
            <RatingBehaviour>SPT</RatingBehaviour>
            <ApportionmentMethod>CHG</ApportionmentMethod>
            <ImportMetaData>
              <Instruction>Insert</Instruction>
            </ImportMetaData>
          </ConsolCostLine>
        </ConsolCostLineCollection>
      </ConsolCosts>
    </Shipment>
  </UniversalShipment>";

		#endregion

		#endregion

		#region Consolidation Advice ORG

		public void TestReadIntoBusinessObject_ORG_WithNoReferences()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithNoReferences_PartialShipmentLinked()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithNoReferences_PartialShipmentsLinkedToOtherConsol()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var consolInDB2 = Factory.New<ForwardingConsol>();
			consolInDB2.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB2.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNull(consolBO);
			AssertMultilineASCIIEquals(@"Error - Cannot populate ForwardingConsol because:
[*Shipment(s) on Consolidation Advice has been already linked to a Consolidation.*]", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithNoReferences_ExtraShipments()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);
			var subShipment3 = Factory.New<ForwardingShipment>();
			subShipment3.JS_UniqueConsignRef = "S00001402";
			consolInDB.Shipments.Add(subShipment3);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			Assert(!consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001402"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Shipment S00001402 has been unlinked from consol C00001000 for it no longer exists in the Consolidation Advice message.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadBookingConfirmationReference()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill_CoLoadBookingConfirmationReference()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill_CoLoadBookingConfirmationReference_PartialShipmentLinked()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill_CoLoadBookingConfirmationReference_PartialShipmentsLinkedToOtherConsol()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "CoLoadBookingReference";
			consolInDB.JK_CoLoadMasterBill = "CoLoadMasterBill";

			var consolInDB2 = Factory.New<ForwardingConsol>();
			consolInDB2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB2.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB2.JK_CoLoadMasterBill = "VERSJU2104661454";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB2.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals(consolInDB2.JK_CoLoadBookingReference, consolBO.JK_CoLoadBookingReference);
			AssertEquals(consolInDB2.JK_CoLoadMasterBill, consolBO.JK_CoLoadMasterBill);
			AssertMRREventForConsolidationAdvice(consolBO, false);
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
[*Shipment(s) on Consolidation Advice has been linked to another Consolidation.*]", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill_CoLoadBookingConfirmationReference_NoConsol_NoShipmentLinked()
		{
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - No matching ForwardingConsol found, creating new ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Consol from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill_CoLoadBookingConfirmationReference_ShipmentsLinkedToOtherConsol()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "CoLoadBookingReference";
			consolInDB.JK_CoLoadMasterBill = "CoLoadMasterBill";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNull(consolBO);
			AssertMultilineASCIIEquals(@"Error - Cannot populate ForwardingConsol because:
[*Shipment(s) on Consolidation Advice has been already linked to a Consolidation.*]", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadBookingConfirmationReference_CoLoadMasterBill_HIR_ExtraShipments()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";
			consolInDB.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "CAD0000001081");

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);
			var subShipment3 = Factory.New<ForwardingShipment>();
			subShipment3.JS_UniqueConsignRef = "S00001402";
			consolInDB.Shipments.Add(subShipment3);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Original;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			Assert(!consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001402"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Original");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Shipment S00001402 has been unlinked from consol C00001000 for it no longer exists in the Consolidation Advice message.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		#endregion

		#region Consolidation Advice AMD

		public void TestReadIntoBusinessObject_AMD_WithCoLoadBookingConfirmationReference_HIR()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "CAD0000001081");

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Amendment;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Amendment");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithCoLoadMasterBill_HIR()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";
			consolInDB.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "CAD0000001081");

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Amendment;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Amendment");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_ORG_WithHIR()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "CAD0000001081");

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Amendment;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Amendment");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_AMD_WithCoLoadBookingConfirmationReference_CoLoadMasterBill_HIR_PartialShipmentLinked()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";
			consolInDB.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "CAD0000001081");

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Amendment;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Amendment");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_AMD_WithCoLoadBookingConfirmationReference_CoLoadMasterBill_HIR_ExtraShipments()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB.JK_CoLoadMasterBill = "VERSJU2104661454";
			consolInDB.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "CAD0000001081");

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB.Shipments.Add(subShipment2);
			var subShipment3 = Factory.New<ForwardingShipment>();
			subShipment3.JS_UniqueConsignRef = "S00001402";
			consolInDB.Shipments.Add(subShipment3);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Amendment;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals("SMX2104661454", consolBO.JK_CoLoadBookingReference);
			AssertEquals("VERSJU2104661454", consolBO.JK_CoLoadMasterBill);
			var hirNumber = consolBO.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR).FirstOrDefault();
			AssertNotNullOrEmpty(hirNumber);
			AssertEquals("CAD0000001081", hirNumber);
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001400"));
			Assert(consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001401"));
			Assert(!consolBO.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_UniqueConsignRef == "S00001402"));
			AssertMRREventForConsolidationAdvice(consolBO, true, "Amendment");
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - No matching ForwardingConsolStmNote found, creating new ForwardingConsolStmNote.
Information - Populating ForwardingConsolStmNote...
Warning - Description(value: Booking Confirmation Notes) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: MXVER Destination: PRSJU
Information - Transport Leg updated.
Information - Shipment S00001402 has been unlinked from consol C00001000 for it no longer exists in the Consolidation Advice message.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001400 from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001401 from UniversalShipment.
Warning - Matching 'SendingForwarderAddress':- No match found for '[Company Name: WISETECH GLOBAL PTY LTD; Address 1: SE 116 87 TURNER STREET; City: PORT MELBOURNE]'.
Warning - Matching 'CoLoadWith':- No match found for '[Company Name: CARGOWISE; Address 1: 3A 72 O RIORDAN STREET; City: ALEXANDRIA]'.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Consol C00001000 from UniversalShipment.", logger.Logs);
		}

		public void TestReadIntoBusinessObject_AMD_WithCoLoadBookingConfirmationReference_CoLoadMasterBill_HIR_PartialShipmentsLinkedToOtherConsol()
		{
			var consolInDB = Factory.New<ForwardingConsol>();
			consolInDB.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB.JK_CoLoadBookingReference = "CoLoadBookingReference";
			consolInDB.JK_CoLoadMasterBill = "CoLoadMasterBill";

			var consolInDB2 = Factory.New<ForwardingConsol>();
			consolInDB2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInDB2.JK_CoLoadBookingReference = "SMX2104661454";
			consolInDB2.JK_CoLoadMasterBill = "VERSJU2104661454";

			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_UniqueConsignRef = "S00001400";
			consolInDB.Shipments.Add(subShipment1);
			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_UniqueConsignRef = "S00001401";
			consolInDB2.Shipments.Add(subShipment2);

			Factory.SaveForTesting();

			var consolDataObject = SetupConsol("ConsolidationAdviceImportFile.xml");
			consolDataObject.DataContext.DocumentaryOverride.Purpose.Code = MessagePurposes.Codes.Amendment;
			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals(consolInDB2.JK_CoLoadBookingReference, consolBO.JK_CoLoadBookingReference);
			AssertEquals(consolInDB2.JK_CoLoadMasterBill, consolBO.JK_CoLoadMasterBill);
			AssertMRREventForConsolidationAdvice(consolBO, false);
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
[*Shipment(s) on Consolidation Advice has been linked to another Consolidation.*]", logger.Logs);
		}

		#endregion

		public void TestReadIntoBusinessObject_MaximumAllowablePackageLengthUnit()
		{
			var consolDataObject = SetupConsol();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);

			consolDataObject.MaximumAllowablePackageHeight = 0;
			consolDataObject.MaximumAllowablePackageWidth = 0;
			consolDataObject.MaximumAllowablePackageLength = 0;
			consolDataObject.MaximumAllowablePackageLengthUnit = null;
			AssertNoExceptionThrown("No exception when Height, Weight and Length are 0.", () => reader.ReadIntoBusinessObject());

			consolDataObject.MaximumAllowablePackageHeight = 10;
			AssertExceptionThrown("Exception should be thrown when Height is greater than 0 and unit is not provided.",
				typeof(DataObjectReadFailureException),
				"MaximumAllowablePackageLengthUnit is required when MaximumAllowablePackageWidth, MaximumAllowablePackageLength or MaximumAllowablePackageHeight is greater than zero.",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.MaximumAllowablePackageLengthUnit = new UnitOfLength();
			AssertExceptionThrown("Exception should be thrown when Height is greater than 0 and unit is not provided.",
				typeof(DataObjectReadFailureException),
				"MaximumAllowablePackageLengthUnit is required when MaximumAllowablePackageWidth, MaximumAllowablePackageLength or MaximumAllowablePackageHeight is greater than zero.",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.MaximumAllowablePackageLengthUnit = new UnitOfLength { Code = ZString.Empty };
			AssertExceptionThrown("Exception should be thrown when Height is greater than 0 and unit is not provided.",
				typeof(DataObjectReadFailureException),
				"MaximumAllowablePackageLengthUnit is required when MaximumAllowablePackageWidth, MaximumAllowablePackageLength or MaximumAllowablePackageHeight is greater than zero.",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.MaximumAllowablePackageLengthUnit = new UnitOfLength { Code = Weight.Kilograms };
			AssertExceptionThrown("Exception should be thrown when Height is greater than 0 and unit is invalid.",
				typeof(DataObjectReadFailureException),
				"Invalid MaximumAllowablePackageLengthUnit. Value must be one of valid length units: MM, CM, M, KM, MI, IN, FT, YD.",
				() => reader.ReadIntoBusinessObject());

			consolDataObject.MaximumAllowablePackageLengthUnit = new UnitOfLength { Code = Length.Metres };
			AssertNoExceptionThrown("No exceptions should be thrown when valid MaximumAllowablePackageLengthUnit is provided.", () => reader.ReadIntoBusinessObject());
			AssertNoExceptionThrown("No exceptions should be thrown when saving after import.", () => Factory.SaveAtEndOfImport(logger));
		}

		public void TestReadIntoBusinessObject_MaximumAllowablePackageLengthUnit_WithExistingValues()
		{
			var consol = GetConsolBOToLoad();
			consol.JK_MaximumAllowablePackageHeight = 10;
			consol.JK_MaximumAllowablePackageWidth = 10;
			consol.JK_MaximumAllowablePackageLength = 10;
			consol.JK_MaximumAllowablePackageUnit = Length.Metres;
			Factory.SaveForTesting();

			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testUShipment.DataContext = DataContextFactory.New();
			testUShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C00001052");

			testUShipment.MaximumAllowablePackageLength = null;
			testUShipment.MaximumAllowablePackageWidth = null;
			testUShipment.MaximumAllowablePackageHeight = null;
			testUShipment.MaximumAllowablePackageLengthUnit = null;

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			AssertNoExceptionThrown("No exceptions should be thrown.", () => reader.ReadIntoBusinessObject());

			testUShipment.MaximumAllowablePackageLengthUnit = new UnitOfLength();
			AssertNoExceptionThrown("No exceptions should be thrown.", () => reader.ReadIntoBusinessObject());

			testUShipment.MaximumAllowablePackageLengthUnit = new UnitOfLength { Code = ZString.Empty };
			AssertNoExceptionThrown("No exceptions should be thrown.", () => reader.ReadIntoBusinessObject());

			testUShipment.MaximumAllowablePackageLengthUnit = new UnitOfLength { Code = Weight.Kilograms };
			AssertExceptionThrown("Exception should be thrown when unit is invalid.",
				typeof(DataObjectReadFailureException),
				"Invalid MaximumAllowablePackageLengthUnit. Value must be one of valid length units: MM, CM, M, KM, MI, IN, FT, YD.",
				() => reader.ReadIntoBusinessObject());
		}

		#region TestAddressAdditionalInfo

		public void TestConsolAdditionalAddressInfo()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_UniqueConsignRef = "C00001000";
			consolBO.JK_OA_PackDepotAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consolBO.JK_OA_UnpackDepotAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			Factory.SaveForTesting();

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C00001000");

			consolDataObject.SetAdditionalAddressInfoCollection(() => new List<AdditionalAddressInfo>
			{
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.DepartureCFSAddress),
					TransportMode = new CodeDescriptionPair { Code = "RAI", Description = "Rail Freight" }
				},
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.ArrivalCFSAddress),
					TransportMode = new CodeDescriptionPair { Code = "IWT", Description = "Inland Waterways" }
				}
			});

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBOAfterImport = reader.ReadIntoBusinessObject();

			AssertEquals("RAI", consolBOAfterImport.CFSDepartureByTransportMode);
			AssertEquals("IWT", consolBOAfterImport.CFSArrivalByTransportMode);
		}

		#endregion

		#region CO2e

		public void TestReaderDoesNotPopulateCO2eWhenConsolDoesNotMatchCO2eCalculationParameters()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
			consol.JK_UniqueConsignRef = "C00001000";
			consol.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			consol.JK_MasterBillNum = "HOUSENUM1";
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			dataObject.WayBillNumber = "HOUSENUM1";

			consol.JK_RL_NKLoadPort = "USLAX";
			var reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - Cannot populate CO2e for ForwardingConsol because CO2e Calculation input parameters have been changed
Information - Updated Consol C00001000 (Master Bill='HOUSENUM1') from UniversalShipment.", logger.Logs);

			logger.ClearLogs();
			consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithIncompleteLegs(Factory.BOFactory);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			consol.JK_MasterBillNum = "222-12345678";
			consol.JK_UniqueConsignRef = "C00001001";
			Factory.SaveForTesting();

			dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectWithVirtualLegs();
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			dataObject.WayBillNumber = "222-12345678";

			reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Information - Importing greenhouse gas emissions calculation result.
Information - CO2e is calculated for Transport: Transport Leg (Consol='C00001001', Flight='FL1')
Information - CO2e is calculated for ForwardingConsol: Consol C00001001 (Master Bill='22212345678')
Information - Updated Consol C00001001 (Master Bill='22212345678') from UniversalShipment.", logger.Logs);
		}

		public void TestCO2eCalculation_GHGUpdatedEventsLogged()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
			consol.JK_MasterBillNum = "HOUSENUM1";
			consol.JK_TotalShipmentActWeightCheck = 1m;
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			dataObject.TotalWeight = 1m;
			Factory.SaveForTesting();
			var reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertGHGEvent(consolBO.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertGHGEvent(consolBO.Transports[0].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertGHGEvent(consolBO.Transports[1].Logs, "|NEW=2000|OLD=NA|TYP=Updated");
			AssertGHGEvent(consolBO.Transports[1].Sailing.Logs, "|NEW=2000|OLD=NA|TYP=Updated");
		}

		public void TestCO2eCalculation_GHGUpdatedEvents_WhenPreviousCO2e()
		{
			// Arrange & Act
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
			consol.JK_MasterBillNum = "HOUSENUM1";
			consol.JK_TotalShipmentActWeightCheck = 1m;

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			dataObject.TotalWeight = 1m;

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			// Assert
			AssertGHGEvent(consolBO.Logs, "|NEW=10000|OLD=NA|TYP=Updated");

			// Act
			dataObject.GreenhouseGasEmission.CO2e = 25000m;
			Factory.SaveForTesting();
			reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();

			// Assert
			AssertGHGEvent(consolBO.Logs, "|NEW=25000|OLD=10000|TYP=Updated", 2);
		}

		public void TestCO2eCalculation_GHGRejectedEventsLogged_WhenParameterChanged()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
			consol.JK_MasterBillNum = "HOUSENUM1";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			Factory.SaveForTesting();
			var reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();
			AssertGHGEvent(consolBO.Logs, "|RES=Input value(s) have changed|TYP=Rejected");
			AssertEquals("No GHG event created for transport", 0, consolBO.Transports[0].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals("No GHG event created for transport", 0, consolBO.Transports[1].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals("No GHG event created for linked sailing", 0, consolBO.Transports[1].Sailing.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
		}

		public void TestCO2eCalculation_TriggerByWorkflow_ChangeParamBeforeReceivingResponse_GHGRejectedEventsLogged()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
			var trigger = consol.WorkflowItems.Triggers.AddNew();

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports[1];

			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 1;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest;
			consol.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.SaveForTesting();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: Customizable Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

			consol.JK_MasterBillNum = "HOUSENUM1";
			consol.JK_RL_NKLoadPort = "USLAX";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "DEHAM";
			transport3.JW_RL_NKDiscPort = "USLAX";
			transport3.JW_TransportMode = "SEA";
			transport3.JW_VoyageFlight = "VY2";

			var loggerprocess = new NotificationBuffer();
			using (Factory.BOFactory.AddDisposableService())
			{
				processor.Process(loggerprocess);
				Factory.SaveForTesting();
			}

			AssertMultilineASCIIEquals("loggger results from processor.Process()", string.Empty, loggerprocess.AsString);
			var newFactory = new BusinessObjectFactory();
			var messages = newFactory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			var message = messages[0];
			Assert(message.IsInDatabase);
			CombineAssertions("EDI Message sent", () =>
			{
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertContains("message.EM_MessageText", "UniversalShipment", message.EM_MessageText);
			});

			var interchange = newFactory.Load<EDIInterchange>(message.EM_EI);
			Assert(interchange.IsInDatabase);
			CombineAssertions("EDI Interchange", () =>
			{
				AssertNotNull(interchange);
				AssertEquals("EMISSION_CALCULATOR", interchange.EI_To);
			});

			AssertEquals(CO2eStatusList.Codes.Pending, consol.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Pending, transport1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Pending, transport2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Pending, transport3.GetCO2eStatus());

			consol.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport3.GetCO2eStatus());

			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingConsol.
Information - Populating ForwardingConsol...
Warning - Cannot populate CO2e for ForwardingConsol because CO2e Calculation input parameters have been changed
Information - Updated Consol C00001000 (Master Bill='HOUSENUM1') from UniversalShipment."
			, logger.Logs);

			var transportGHGEvent = consolBO.Logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", "|RES=Input value(s) have changed|TYP=Rejected", transportGHGEvent.SL_Reference);
			AssertEquals("One GHG event created for transport", 1, consolBO.Transports[0].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals("One GHG event created for transport", 1, consolBO.Transports[1].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
		}

		void AssertGHGEvent(Logs logs, string reference, int logCount = 1)
		{
			AssertEquals("New GHG event created", logCount, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", reference, transportGHGEvent.SL_Reference);
		}

		#endregion

		#region JK_ElectronicBillOfLadingType

		public void TestJK_ElectronicBillOfLadingType()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consolDataObject = SetupConsol();
				consolDataObject.BillType = null;

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillType = new CodeDescriptionPair() { Code = "INV" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingType);
				AssertContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillType = new CodeDescriptionPair() { Code = "STR" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("STR", consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillType = new CodeDescriptionPair() { Code = ZString.Empty };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("STR", consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);
			}
		}

		public void TestJK_ElectronicBillOfLadingType_EnableBoleroEBLIntegration_False()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false, GalileoEndPointUrl = "http://test.test" }))
			{
				var consolDataObject = SetupConsol();
				consolDataObject.BillType = null;

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillType = new CodeDescriptionPair() { Code = "INV" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillType = new CodeDescriptionPair() { Code = "STR" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillType = new CodeDescriptionPair() { Code = ZString.Empty };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingType);
				AssertNotContains("The Bill Type is not updated because the input value is not a valid code", logger.Logs);
			}
		}

		#endregion

		#region JK_ElectronicBillOfLadingTerms

		public void TestJK_ElectronicBillOfLadingTerms()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consolDataObject = SetupConsol();
				consolDataObject.BillType = null;

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillTerms = new CodeDescriptionPair() { Code = "INV" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingTerms);
				AssertContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillTerms = new CodeDescriptionPair() { Code = "TRA" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("TRA", consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillTerms = new CodeDescriptionPair() { Code = ZString.Empty };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals("TRA", consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);
			}
		}

		public void TestJK_ElectronicBillOfLadingTerms_EnableBoleroEBLIntegration_False()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false, GalileoEndPointUrl = "http://test.test" }))
			{
				var consolDataObject = SetupConsol();
				consolDataObject.BillType = null;

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillTerms = new CodeDescriptionPair() { Code = "INV" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillTerms = new CodeDescriptionPair() { Code = "TRA" };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);

				logger.ClearLogs();
				consolDataObject.BillTerms = new CodeDescriptionPair() { Code = ZString.Empty };
				reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertNullOrEmpty(consolBO.JK_ElectronicBillOfLadingTerms);
				AssertNotContains("The Bill Terms is not updated because the input value is not a valid code", logger.Logs);
			}
		}

		#endregion

		#region JK_RS_NKGatewayServiceLevel

		public void TestImportConsol_GatewayServiceLevel()
		{
			var consolDataObject = SetupConsol();

			var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertNotNull(consolBO);
			AssertEquals("STD", consolBO.JK_RS_NKGatewayServiceLevel);
		}

		#endregion

		#region OriginalBillNotes

		[TestDate(2023, 08, 08)]
		public void TestPopulateOriginalBillNotes()
		{
			GetConsolBOToLoad();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";
			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var triCusCode1 = orgHeader.CustomsCodes.AddNew();
			triCusCode1.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode1.OK_CustomsRegNo = "SH1234567890123455";

			var triCusCode2 = orgHeader.CustomsCodes.AddNew();
			triCusCode2.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode2.OK_CustomsRegNo = "SH1234567890123456";
			triCusCode2.OK_OA_PremisesAddress = arAddress.PK;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consolDataObject = SetupConsol("ConsolImportFileWithTRIOrgAddresses.xml");

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);

				var query = new ZQuery(StmNoteSchema.ST_ParentID, consolBO.PK);
				query.AddToFilter(StmNoteSchema.ST_Table, consolBO.TableName);
				query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
				query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.INT));

				var originalBillNotes = Factory.LoadTop1<StmNote>(query);
				AssertEquals(@"08-Aug-23 00:00:00 +00:00 DAM000008 [Reference:CMATESTBOL0003]
Publisher : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Holder : WiseTech, Address1, Address2, City State Postcode, Australia
First Holder : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Shipper : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Consignee : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
To Order : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
Pledgee : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Surrender Agent : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China", originalBillNotes.ST_NoteText);
			}
		}

		public void TestPopulateOriginalBillNotes__EnableBoleroEBLIntegration_False()
		{
			GetConsolBOToLoad();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";
			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var triCusCode1 = orgHeader.CustomsCodes.AddNew();
			triCusCode1.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode1.OK_CustomsRegNo = "SH1234567890123455";

			var triCusCode2 = orgHeader.CustomsCodes.AddNew();
			triCusCode2.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode2.OK_CustomsRegNo = "SH1234567890123456";
			triCusCode2.OK_OA_PremisesAddress = arAddress.PK;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false, GalileoEndPointUrl = "http://test.test" }))
			{
				var consolDataObject = SetupConsol("ConsolImportFileWithTRIOrgAddresses.xml");

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);

				var query = new ZQuery(StmNoteSchema.ST_ParentID, consolBO.PK);
				query.AddToFilter(StmNoteSchema.ST_Table, consolBO.TableName);
				query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
				query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.INT));

				AssertNull(Factory.LoadTop1<StmNote>(query));
			}
		}

		[TestDate(2024, 10, 31, 9, 32, 21)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPopulateOriginalBillNotes_Append()
		{
			TestDateAttribute.UseUNLOCO = true;
			var consol = GetConsolBOToLoad();
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Department, "Carrier"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, "Bill Of Lading"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, BillStatusUpdatedTypes.OriginalBillPublished)
			};
			consol.Logs.AddNew(Events.BillStatusUpdated, eventParameters);

			var note = consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.ToString();
			note.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.INT);
			note.ST_NoteText = "Happy to copy and paste the code. o.O";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";
			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var triCusCode1 = orgHeader.CustomsCodes.AddNew();
			triCusCode1.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode1.OK_CustomsRegNo = "SH1234567890123455";

			var triCusCode2 = orgHeader.CustomsCodes.AddNew();
			triCusCode2.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode2.OK_CustomsRegNo = "SH1234567890123456";
			triCusCode2.OK_OA_PremisesAddress = arAddress.PK;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consolDataObject = SetupConsol("ConsolImportFileWithTRIOrgAddresses.xml");

				logger.ClearLogs();
				var reader = new ConsolDataObjectReader(consolDataObject, logger, Factory);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);

				var query = new ZQuery(StmNoteSchema.ST_ParentID, consolBO.PK);
				query.AddToFilter(StmNoteSchema.ST_Table, consolBO.TableName);
				query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
				query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.INT));

				var originalBillNotes = Factory.LoadTop1<StmNote>(query);
				AssertEquals(@"31-Oct-24 09:32:21 +00:00 DAM000008 Original Bill Received [Reference:CMATESTBOL0003]
Publisher : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Holder : WiseTech, Address1, Address2, City State Postcode, Australia
First Holder : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Shipper : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Consignee : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
To Order : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
Pledgee : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Surrender Agent : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China

Happy to copy and paste the code. o.O", originalBillNotes.ST_NoteText);

				orgHeader.MainAddress.Address1 = "Address11111";
				consolBO = reader.ReadIntoBusinessObject();
				originalBillNotes = Factory.LoadTop1<StmNote>(query);
				AssertEquals(@"31-Oct-24 09:32:21 +00:00 DAM000008 Original Bill Received [Reference:CMATESTBOL0003]
Publisher : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Holder : WiseTech, Address11111, Address2, City State Postcode, Australia
First Holder : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Shipper : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Consignee : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
To Order : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
Pledgee : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Surrender Agent : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China

31-Oct-24 09:32:21 +00:00 DAM000008 Original Bill Received [Reference:CMATESTBOL0003]
Publisher : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Holder : WiseTech, Address1, Address2, City State Postcode, Australia
First Holder : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Shipper : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Consignee : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
To Order : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China
Pledgee : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Surrender Agent : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China

Happy to copy and paste the code. o.O", originalBillNotes.ST_NoteText);
			}
		}

		#endregion

		#region Implementation

		bool MAWBUsedOnAnotherConsol(ForwardingConsol consolBO, ZString mawb)
		{
			var query = new ZQuery(JobConsolSchema.PK, SQLComparisonOperator.NotEqual, (consolBO != null) ? consolBO.PK : ZGuid.Empty);
			query.AddToFilter(JobConsolSchema.JK_IsCancelled, false);
			query.AddToFilter(JobConsolSchema.JK_TransportMode, Core.Constants.TransportModes.Air);
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, mawb);

			return Factory.BOFactory.Load<ForwardingConsol>(query).Any();
		}

		bool MAWBUsedOnAnotherConsol(ZString mawb)
		{
			return MAWBUsedOnAnotherConsol(null, mawb);
		}

		bool MatchingMAWBExistsInStock(ZString airlinePrefix, ZString mawb, ZString serviceLevel)
		{
			return GetMatchingMAWB(airlinePrefix, mawb, serviceLevel) != null;
		}

		ZQuery GetMatchingMAWBQuery(ZString airlinePrefix, ZString waybill, ZString serviceLevel)
		{
			var query = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, airlinePrefix);
			query.AddToFilter(JobMawbSchema.JM_MAWB, waybill);
			query.AddToFilter(JobMawbSchema.JM_IsPrinted, false);
			query.AddToFilter(JobMawbSchema.JM_IsPaper, false);
			query.AddToFilter(JobMawbSchema.JM_ServiceLevel, SQLComparisonOperator.Equal, serviceLevel);
			query.AddToFilter(JobMawbSchema.JM_OH_AllocatedTo, null);
			query.AddToFilter(JobMawbSchema.JM_GB, GlbBranch.CurrentBranch.PK);

			return query;
		}

		JobMawb GetMatchingMAWB(ZString airlinePrefix, ZString waybill, ZString serviceLevel)
			=> Factory.BOFactory.Load<JobMawb>(GetMatchingMAWBQuery(airlinePrefix, waybill, serviceLevel)).FirstOrDefault()
				?? Factory.BOFactory.Load<JobMawb>(GetMatchingMAWBQuery(airlinePrefix, waybill, OrgCarrierServiceLevel.AllCode)).FirstOrDefault();

		ServiceTaskLogForTesting CreateAndProcessUniversalShipment(string fileName)
		{
			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile(fileName));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			return serviceTaskLog;
		}

		IEnumerable<ISimpleLog>  CreateAndProcessUniversalShipmentWithProcessorLogs(string fileName)
		{
			var message = GetQueuedUniversalShipmentMessage(ReadTextFromEmbeddedResourceFile(fileName));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			return manager.Logger.Logs;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Name = "HighWind";
			refVessel.RV_IsActive = true;
			refVessel.RV_LloydsNumber = "Lloyds";
			refVessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
		}

		TestErrorLogger logger;

		EmbeddedResourceRetriever ResourceRetriever => new EmbeddedResourceRetriever(GetType().Assembly);

		internal UniversalShipment SetupConsol(string fileName = "ConsolImportFile.xml")
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var inputStream = (SubStreamableStream)new MemoryStream(ResourceRetriever.GetBytes(GetEmbeddedResourceReadingTestFilePathFor(fileName))))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, inputStream, logger);
			}

			return shipmentDataObject;
		}

		internal UniversalShipment SetupConsolWithConsolCostingNotMappedToTargetXML()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var inputStream = (SubStreamableStream)new MemoryStream(ResourceRetriever.GetBytes(GetEmbeddedResourceReadingTestFilePathFor("ConsolCostingNotMappedImportFile.xml"))))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, inputStream, logger);
			}

			return shipmentDataObject;
		}

		string ReadTextFromEmbeddedResourceFile(string fileName)
		{
			return ResourceRetriever.GetString(GetEmbeddedResourceTestFilePathFor(fileName));
		}

		static string GetEmbeddedResourceTestFilePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.TestFiles.{fileName}";
		}

		static string GetEmbeddedResourceReadingTestFilePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.Reading.TestFiles.{fileName}";
		}

		internal static void AssertContents(ForwardingConsol consolBO, string agentType = "AGT", bool isCoLoad = false, bool isGatewayCoLoad = false)
		{
			AssertEquals("consolBO.JK_AgentsReference", "Agent U", consolBO.JK_AgentsReference);
			AssertEquals("consolBO.JK_AWBServiceLevel", "STD", consolBO.JK_AWBServiceLevel);
			AssertEquals("consolBO.JK_ConsolMode", "LSE", consolBO.JK_ConsolMode);
			AssertEquals("consolBO.IsBuyersConsol", false, consolBO.IsBuyersConsol);
			AssertEquals("consolBO.JK_IsCFS", false, consolBO.JK_IsCFS);
			AssertEquals("consolBO.IsCoLoad", isCoLoad, consolBO.IsCoLoad);
			AssertEquals("consolBO.isGatewayCoLoad", isGatewayCoLoad, consolBO.IsCoLoad && consolBO.IsSendingOrReceivingForwarderGateway);
			AssertEquals("consolBO.IsDirect", false, consolBO.IsDirect);
			AssertEquals("consolBO.JK_IsForwarding", true, consolBO.JK_IsForwarding);
			AssertEquals("consolBO.JK_IsNeutralMaster", false, consolBO.JK_IsNeutralMaster);
			AssertEquals("consolBO.JK_NoCopyBills", new ZByte(4), consolBO.JK_NoCopyBills);
			AssertEquals("consolBO.JK_NoOriginalBills", new ZByte(3), consolBO.JK_NoOriginalBills);
			AssertEquals("consolBO.JK_PrepaidCollect", "PPD", consolBO.JK_PrepaidCollect);
			AssertEquals("consolBO.JK_RL_NKDischargePort", "AUSYD", consolBO.JK_RL_NKDischargePort);
			AssertEquals("consolBO.JK_RL_NKLoadPort", "NZCHC", consolBO.JK_RL_NKLoadPort);
			AssertEquals("consolBO.JK_RL_NKPortOfFirstArrival", "JPOSA", consolBO.JK_RL_NKPortOfFirstArrival);
			AssertEquals("consolBO.JK_RL_NKFirstForeignPort", "CNSHA", consolBO.JK_RL_NKFirstForeignPort);
			AssertEquals("consolBO.JK_ReleaseType", "CAD", consolBO.JK_ReleaseType);
			AssertEquals("consolBO.JK_AgentType", agentType, consolBO.JK_AgentType);
			AssertEquals("consolBO.JK_TransportMode", "SEA", consolBO.JK_TransportMode);
			AssertEquals("consolBO.JK_MasterBillNum", "08111111214", consolBO.JK_MasterBillNum);
			AssertEquals("consolBO.JK_ShippedOnBoardDate", new ZDateTime(2010, 12, 10), consolBO.JK_ShippedOnBoardDate);
			AssertEquals("consolBO.JK_MasterBillIssueDate", new ZDateTime(2010, 12, 14), consolBO.JK_MasterBillIssueDate);
			AssertEquals("consolBO.JK_DatePortOfFirstArrival", new ZDateTime(2010, 12, 16), consolBO.JK_DatePortOfFirstArrival);
			AssertEquals("consolBO.JK_TotalShipmentActWeightCheck", 1m, consolBO.JK_TotalShipmentActWeightCheck);
			AssertEquals("consolBO.WeightVerificationUnit", Constants.Weight.Milligrams, consolBO.WeightVerificationUnit);
			AssertEquals("consolBO.JK_TotalShipmentActVolumeCheck", 2m, consolBO.JK_TotalShipmentActVolumeCheck);
			AssertEquals("consolBO.VolumeVerificationUnit", Constants.Volume.CubicCentimeters, consolBO.VolumeVerificationUnit);
			AssertEquals("consolBO.JK_TotalShipmentChargableCheck", 3m, consolBO.JK_TotalShipmentChargableCheck);
			AssertEquals("consolBO.JK_ConsolCutOffDate", new ZDateTime(1953, 03, 05), consolBO.JK_ConsolCutOffDate);
			AssertEquals("consolBO.JK_CorrectedConsolWeight", 5m, consolBO.JK_CorrectedConsolWeight);
			AssertEquals("consolBO.JK_CorrectedConsolWeightUnit", Constants.Weight.Kilograms, consolBO.JK_CorrectedConsolWeightUnit);
			AssertEquals("consolBO.JK_CorrectedConsolVolume", 6m, consolBO.JK_CorrectedConsolVolume);
			AssertEquals("consolBO.JK_CorrectedConsolVolumeUnit", Constants.Volume.CubicMetres, consolBO.JK_CorrectedConsolVolumeUnit);
			AssertEquals("consolBO.JK_ConsolChargeable", 7m, consolBO.JK_ConsolChargeable);
			AssertEquals("consolBO.JK_ConsolChargeableRate", 8m, consolBO.JK_ConsolChargeableRate);
			AssertEquals("consolBO.JK_OverrideConsolChargeable", true, consolBO.JK_OverrideConsolChargeable);
			AssertEquals("consolBO.JK_RequiresTemperatureControl", true, consolBO.JK_RequiresTemperatureControl);
			AssertEquals("consolBO.JK_RequiredTemperatureMinimum", (ZDecimal)1.0, consolBO.JK_RequiredTemperatureMinimum);
			AssertEquals("consolBO.JK_RequiredTemperatureMaximum", (ZDecimal)100.0, consolBO.JK_RequiredTemperatureMaximum);
			AssertEquals("consolBO.JK_RequiredTemperatureUnit", Core.Constants.Temperature.Centigrade, consolBO.JK_RequiredTemperatureUnit);

			if (isCoLoad || isGatewayCoLoad)
			{
				AssertEquals("consolBO.JK_CoLoadMasterBill", "COLOADMASTERBILL1", consolBO.JK_CoLoadMasterBill);
				AssertEquals("consolBO.JK_CoLoadBookingReference", "COLOADREF1", consolBO.JK_CoLoadBookingReference);
			}
		}

		static void AssertOrgAddressContents(OrgAddress addressBO)
		{
			AssertEquals("addressBO.OA_Address1", "SOME STREET", addressBO.OA_Address1);
			AssertEquals("addressBO.OA_Address2", "", addressBO.OA_Address2);
			AssertEquals("addressBO.OA_City", "MASCOT", addressBO.OA_City);
			AssertEquals("addressBO.OA_Email", "ben.govett@cargowise.com", addressBO.OA_Email);
			AssertEquals("addressBO.OA_Fax", "3860 8799", addressBO.OA_Fax);
			AssertEquals("addressBO.OA_Mobile", "", addressBO.OA_Mobile);
			AssertEquals("addressBO.OA_Phone", "3860 8844", addressBO.OA_Phone);
			AssertEquals("addressBO.OA_PostCode", "2020", addressBO.OA_PostCode);
			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
			AssertEquals("addressBO.OA_State", "NSW", addressBO.OA_State);
		}

		ForwardingConsol GetConsolBOToLoad()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_UniqueConsignRef = "C00001052";
			consolBOToLoad.JK_RL_NKLoadPort = "AUSYD";
			consolBOToLoad.JK_RL_NKDischargePort = "USLAX";
			consolBOToLoad.JK_AgentType = Constants.AgentType.Agent;
			consolBOToLoad.JK_TransportMode = Constants.TransportModes.Air;
			consolBOToLoad.JK_AWBServiceLevel = "STD";
			consolBOToLoad.MasterBillAirlinePrefix = "081";
			consolBOToLoad.JK_AgentsReference = "Agent U";
			consolBOToLoad.JK_MasterBillNum = "08111111210";

			return consolBOToLoad;
		}

		void AssertJKOverrideConsolChargeableForNewConsol(ZDecimal? chargeable = null, ZDecimal? volume = null, UnitOfVolume unitOfVolume = null, ZDecimal? weight = null, UnitOfWeight unitOfWeight = null, bool expectedOverrideConsolChargeableValue = false)
		{
			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			testUShipment.CarrierCorrectedChargeable = chargeable;
			testUShipment.CarrierCorrectedVolume = volume;
			testUShipment.CarrierCorrectedVolumeUnit = unitOfVolume;
			testUShipment.CarrierCorrectedWeight = weight;
			testUShipment.CarrierCorrectedWeightUnit = unitOfWeight;

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals($"Override Consol Chareable should be {expectedOverrideConsolChargeableValue}.", expectedOverrideConsolChargeableValue, consolBO.JK_OverrideConsolChargeable);
		}

		void AssertJKOverrideConsolChargeableForExistingConsol(string order, bool overrideConsolChargeableOldValue = false,
				ZDecimal? chargeable = null, ZDecimal? volume = null, UnitOfVolume unitOfVolume = null, ZDecimal? weight = null, UnitOfWeight unitOfWeight = null, bool expectedOverrideConsolChargeableValue = false)
		{
			var consol = GetConsolBOToLoad();
			consol.JK_MasterBillNum = "081";
			consol.JK_OverrideConsolChargeable = overrideConsolChargeableOldValue;
			consol.JK_UniqueConsignRef = $"C000010{order}";
			consol.JK_CorrectedConsolVolume = 18;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicMetres;
			consol.JK_CorrectedConsolWeight = 18;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;
			consol.JK_ConsolChargeable = 18;

			Factory.SaveForTesting();

			var testUShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testUShipment.DataContext = DataContextFactory.New();
			testUShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, consol.JK_UniqueConsignRef);

			testUShipment.CarrierCorrectedChargeable = chargeable;
			testUShipment.CarrierCorrectedVolume = volume;
			testUShipment.CarrierCorrectedVolumeUnit = unitOfVolume;
			testUShipment.CarrierCorrectedWeight = weight;
			testUShipment.CarrierCorrectedWeightUnit = unitOfWeight;

			var reader = new ConsolDataObjectReader(testUShipment, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertEquals($"Override Consol Chareable should be {expectedOverrideConsolChargeableValue} for #{order} test.", expectedOverrideConsolChargeableValue, consolBO.JK_OverrideConsolChargeable);
		}

		void AssertMRREventForConsolidationAdvice(ForwardingConsol consolBO, bool eventExpected, string purpose = "Original")
		{
			var logMRR = consolBO.Logs.MostRecentLogByEventTime(Events.MessageReceived);

			if (eventExpected)
			{
				AssertNotNull("MRR event is added.", logMRR);
				AssertEquals("Event reference.", "from Carrier", logMRR.ReferenceFreeText);
				AssertEquals("Event message type.", "Consolidation Advice", logMRR.Parameters.GetValueOrDefault("MST"));
				AssertEquals("Event message sub type/purpose.", purpose, logMRR.Parameters.GetValueOrDefault("MSB"));
			}
			else
			{
				AssertNull("MRR event is not added.", logMRR);
			}
		}

		#endregion
	}
}
