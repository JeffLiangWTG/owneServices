using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DelayAlertDocumentDeliveryForVoyageChangesTest : TestCaseWithFactory
	{
		public void TestDomesticAndCrossTradeJobsThatRequireDelayAlertDeliveryFiltering()
		{
			var handledPort1 = Factory.NewWithValidTestData<GlbBranchExtraPorts>();
			handledPort1.GY_GB = GlbBranch.CurrentBranch.PK;
			handledPort1.GY_RL_NKAdditionalBranchRelatedPort = "MYPKG";

			var handledPort2 = Factory.NewWithValidTestData<GlbBranchExtraPorts>();
			handledPort2.GY_GB = GlbBranch.CurrentBranch.PK;
			handledPort2.GY_RL_NKAdditionalBranchRelatedPort = "USLAX";

			JobsForTesting.DomesticConsol.Transports[0].JW_JX = DomesticSailing.PK;
			JobsForTesting.CrossTradeConsol.Transports[0].JW_JX = CrossTradeSailing.PK;

			CommonShipment domesticShipment = JobsForTesting.DomesticConsol.Shipments.AddNew();
			domesticShipment.ConsigneePK = DelayAlertRecipient.PK;

			CommonShipment crossTrade = JobsForTesting.CrossTradeConsol.Shipments.AddNew();
			crossTrade.ConsignorPK = DelayAlertRecipient3.PK;

			CommonShipment domesticBill = JobsForTesting.DomesticAgencyDocumentation;
			domesticBill.JS_JX = DomesticSailing.PK;

			CommonShipment crossTradeBill = JobsForTesting.CrossTradeAgencyDocumentation;
			crossTradeBill.JS_JX = CrossTradeSailing.PK;

			Factory.Save();

			JobScheduleChange domesticChange = Factory.New<JobScheduleChange>();
			domesticChange.E7_ParentID = DomesticSailing.Destination.PK;
			domesticChange.E7_ParentTableCode = JobVoyDestinationSchema.Constants.Prefix;
			domesticChange.E7_DateType = ScheduleDateTypes.Codes.ETA;
			domesticChange.E7_PreviousValue = ZDateTime.Now.AddDays(1);
			domesticChange.E7_UpdatedValue = ZDateTime.Now.AddDays(2);

			JobScheduleChange crossTradeChange = Factory.New<JobScheduleChange>();
			crossTradeChange.E7_ParentID = CrossTradeSailing.Origin.PK;
			crossTradeChange.E7_ParentTableCode = JobVoyOriginSchema.Constants.Prefix;
			crossTradeChange.E7_DateType = ScheduleDateTypes.Codes.ETD;
			crossTradeChange.E7_PreviousValue = ZDateTime.Now.AddDays(1);
			crossTradeChange.E7_UpdatedValue = ZDateTime.Now.AddDays(2);

			JobScheduleChange[] changes = new[] { domesticChange, crossTradeChange };

			AssertDeliveryJobsFilter("All",
									 null,
									 new DelayAlertDeliveryRule(),
									 new BusinessObject[] { domesticShipment, domesticBill },
									 new BusinessObject[] { crossTrade, crossTradeBill },
									 changes);

			AssertDeliveryJobsFilter("Import Only",
									 null,
									 new DelayAlertDeliveryRule() { Direction = DelayAlertDeliveryDirections.Codes.Import },
									 new BusinessObject[] { domesticShipment, domesticBill },
									 Array.Empty<BusinessObject>(),
									 changes);

			AssertDeliveryJobsFilter("Export Only",
									 null,
									 new DelayAlertDeliveryRule() { Direction = DelayAlertDeliveryDirections.Codes.Export },
									 Array.Empty<BusinessObject>(),
									 new BusinessObject[] { crossTrade, crossTradeBill },
									 changes);

			AssertDeliveryJobsFilter("Forwarding Only",
									 null,
									 new DelayAlertDeliveryRule() { Module = DelayAlertDeliveryModules.Codes.Forwarding },
									 new BusinessObject[] { domesticShipment },
									 new BusinessObject[] { crossTrade },
									 changes);

			AssertDeliveryJobsFilter("Shipping Only",
									 null,
									 new DelayAlertDeliveryRule() { Module = DelayAlertDeliveryModules.Codes.ShippingManager },
									 new BusinessObject[] { domesticBill },
									 new BusinessObject[] { crossTradeBill },
									 changes);
		}

		public void TestLoadJobsThatRequireImportExportDelayAlertDeliveryFiltering()
		{
			BusinessObject importDeclaration = JobsForTesting.ImportDeclaration;
			importDeclaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			importDeclaration[JobDeclarationSchema.JE_OH_Importer] = DelayAlertRecipient.PK;
			importDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			importDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			importDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ImportSailing.Origin.JA_RL_NKPortOfLoading;
			importDeclaration[JobDeclarationSchema.JE_ExportDate] = ImportSailing.Origin.JA_E_DEP;
			importDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			importDeclaration[JobDeclarationSchema.JE_DateOfArrival] = ImportSailing.Destination.JB_E_ARV;
			importDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			BusinessObject exportDeclaration = JobsForTesting.ExportDeclaration;
			exportDeclaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			exportDeclaration[JobDeclarationSchema.JE_OH_Supplier] = DelayAlertRecipient3.PK;
			exportDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			exportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			exportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			exportDeclaration[JobDeclarationSchema.JE_ExportDate] = ExportSailing.Origin.JA_E_DEP;
			exportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ExportSailing.Destination.JB_RL_NKPortOfDischarge;
			exportDeclaration[JobDeclarationSchema.JE_DateOfArrival] = ExportSailing.Destination.JB_E_ARV;
			exportDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ExportSailing.PK;

			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.ConsigneePK = DelayAlertRecipient.PK;

			CommonShipment exportShipment = JobsForTesting.ExportConsol.Shipments.AddNew();
			exportShipment.ConsignorPK = DelayAlertRecipient3.PK;

			CommonShipment importBill = JobsForTesting.ImportAgencyDocumentation;
			importBill.JS_JX = ImportSailing.PK;

			CommonShipment exportBill = JobsForTesting.ExportAgencyDocumentation;
			exportBill.JS_JX = ExportSailing.PK;

			Factory.Save();

			JobScheduleChange importChange = Factory.New<JobScheduleChange>();
			importChange.E7_ParentID = ImportSailing.Destination.PK;
			importChange.E7_ParentTableCode = JobVoyDestinationSchema.Constants.Prefix;
			importChange.E7_DateType = ScheduleDateTypes.Codes.ETA;
			importChange.E7_PreviousValue = ZDateTime.Now.AddDays(1);
			importChange.E7_UpdatedValue = ZDateTime.Now.AddDays(2);

			JobScheduleChange exportChange = Factory.New<JobScheduleChange>();
			exportChange.E7_ParentID = ExportSailing.Origin.PK;
			exportChange.E7_ParentTableCode = JobVoyOriginSchema.Constants.Prefix;
			exportChange.E7_DateType = ScheduleDateTypes.Codes.ETD;
			exportChange.E7_PreviousValue = ZDateTime.Now.AddDays(1);
			exportChange.E7_UpdatedValue = ZDateTime.Now.AddDays(2);

			JobScheduleChange[] changes = new JobScheduleChange[] { importChange, exportChange };

			Dictionary<ZGuid, string> map = new Dictionary<ZGuid, string>();
			map[importDeclaration.PK] = "Import Declaration";
			map[exportDeclaration.PK] = "Export Declaration";
			map[importShipment.PK] = "Import Shipment";
			map[exportShipment.PK] = "Export Shipment";
			map[importBill.PK] = "Import Bill of Lading";
			map[exportBill.PK] = "Export Bill of Labing";

			Converter<BusinessObject, string> converter = delegate(BusinessObject bo)
			{
				string result;

				if (bo == null)
				{
					result = null;
				}
				else if (!map.TryGetValue(bo.PK, out result))
				{
					StringBuilder builder = new StringBuilder();
					builder.Append(bo.GetType());
					builder.Append(" (");
					builder.Append(bo.PK);
					builder.Append(")");
					return builder.ToString();
				}

				return result;
			};

			AssertDeliveryJobsFilter("All",
				converter,
				new DelayAlertDeliveryRule(),
				new BusinessObject[] { importDeclaration, importShipment, importBill },
				new BusinessObject[] { exportDeclaration, exportShipment, exportBill },
				changes);

			AssertDeliveryJobsFilter("Import Only",
				converter,
				new DelayAlertDeliveryRule() { Direction = DelayAlertDeliveryDirections.Codes.Import },
				new BusinessObject[] { importDeclaration, importShipment, importBill },
				Array.Empty<BusinessObject>(),
				changes);

			AssertDeliveryJobsFilter("Export Only",
				converter,
				new DelayAlertDeliveryRule() { Direction = DelayAlertDeliveryDirections.Codes.Export },
				Array.Empty<BusinessObject>(),
				new BusinessObject[] { exportDeclaration, exportShipment, exportBill },
				changes);

			AssertDeliveryJobsFilter("Forwarding Only",
				converter,
				new DelayAlertDeliveryRule() { Module = DelayAlertDeliveryModules.Codes.Forwarding },
				new BusinessObject[] { importShipment },
				new BusinessObject[] { exportShipment },
				changes);

			AssertDeliveryJobsFilter("Customs Only",
				converter,
				new DelayAlertDeliveryRule() { Module = DelayAlertDeliveryModules.Codes.Customs },
				new BusinessObject[] { importDeclaration },
				new BusinessObject[] { exportDeclaration },
				changes);

			AssertDeliveryJobsFilter("Shipping Only",
				converter,
				new DelayAlertDeliveryRule() { Module = DelayAlertDeliveryModules.Codes.ShippingManager },
				new BusinessObject[] { importBill },
				new BusinessObject[] { exportBill },
				changes);
		}

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_ForConsol()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;

			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00000101";
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ExportSailing.PK;

			CommonShipment importShipment1 = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment1.ConsigneePK = DelayAlertRecipient.PK;
			importShipment1.JS_UniqueConsignRef = "S00000100";

			CommonShipment importShipment2 = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment2.ConsigneePK = DelayAlertRecipient2.PK;
			importShipment2.JS_UniqueConsignRef = "S00000101";

			CommonShipment exportShipment = JobsForTesting.ExportConsol.Shipments.AddNew();
			exportShipment.ConsignorPK = DelayAlertRecipient3.PK;
			exportShipment.JS_UniqueConsignRef = "S00000102";

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);
			AssertDocumentNotDelivered(DelayAlertRecipient2);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);
			AssertDocumentNotDelivered(DelayAlertRecipient2);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			ExportSailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(1);
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();

			AddDocumentDeliveredEvents();
			AssertDocumentDelivered(DelayAlertRecipient);
			AssertDocumentDelivered(DelayAlertRecipient2);
			AssertDocumentDelivered(DelayAlertRecipient3);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">Delay alert documents have been delivered to one or more clients.</span></strong><br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-05 00:00 03-Jan-05 00:00
AUSYD               ETD                01-Jan-00 00:00 03-Jan-05 00:00
         MYPKG      ETA                02-Jan-05 00:00 03-Jan-05 00:00

The following Consolidation (and related shipment) jobs are affected:
C00000101 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ExportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000102 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, exportShipment.PK.ToGuid()) + @") - delay alert delivered to the consignor
C00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment1.PK.ToGuid()) + @") - delay alert delivered to the consignee
&nbsp;&nbsp;&#3S00000101 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment2.PK.ToGuid()) + @") - delay alert delivered to the consignee

Schedule change notifications");
		}

		#region CS00985785 - Excessive DelayAlertDocumentDeliveryJob

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_WhenExcessiveDelayAlertDetected_ShouldReportError()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;

			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.ConsigneePK = DelayAlertRecipient.PK;
			importShipment.JS_UniqueConsignRef = "S00000100";

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(2);
			Factory.Save();
			AssertEquals("Error should not be reported because Delay Alerts does not exceed the limit", 0, ErrorReporter.TotalErrorCount);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(3);
			Factory.Save();

			AssertEquals("Error should be reported if Delay Alerts exceeds the limit", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("CS00985785 - Excessive DelayAlertDocumentDeliveryJob", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		#endregion

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_ForDeclaration()
		{
			BusinessObject importDeclaration1 = JobsForTesting.ImportDeclaration;
			importDeclaration1[JobDeclarationSchema.JE_DeclarationReference] = "B00005001";
			importDeclaration1[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			importDeclaration1[JobDeclarationSchema.JE_OH_Importer] = DelayAlertRecipient.PK;
			importDeclaration1[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			importDeclaration1[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			importDeclaration1[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ImportSailing.Origin.JA_RL_NKPortOfLoading;
			importDeclaration1[JobDeclarationSchema.JE_ExportDate] = ImportSailing.Origin.JA_E_DEP;
			importDeclaration1[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			importDeclaration1[JobDeclarationSchema.JE_DateOfArrival] = ImportSailing.Destination.JB_E_ARV;
			importDeclaration1[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			BusinessObject importDeclaration2 = JobsForTesting.ImportDeclaration2;
			importDeclaration2[JobDeclarationSchema.JE_DeclarationReference] = "B00005002";
			importDeclaration2[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			importDeclaration2[JobDeclarationSchema.JE_OH_Importer] = UndeliverableRecipient.PK;
			importDeclaration2[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			importDeclaration2[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			importDeclaration2[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ImportSailing.Origin.JA_RL_NKPortOfLoading;
			importDeclaration2[JobDeclarationSchema.JE_ExportDate] = ImportSailing.Origin.JA_E_DEP;
			importDeclaration2[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			importDeclaration2[JobDeclarationSchema.JE_DateOfArrival] = ImportSailing.Destination.JB_E_ARV;
			importDeclaration2[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			BusinessObject exportDeclaration1 = JobsForTesting.ExportDeclaration;
			exportDeclaration1[JobDeclarationSchema.JE_DeclarationReference] = "B00005003";
			exportDeclaration1[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			exportDeclaration1[JobDeclarationSchema.JE_OH_Supplier] = DelayAlertRecipient3.PK;
			exportDeclaration1[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			exportDeclaration1[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			exportDeclaration1[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			exportDeclaration1[JobDeclarationSchema.JE_ExportDate] = ExportSailing.Origin.JA_E_DEP;
			exportDeclaration1[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ExportSailing.Destination.JB_RL_NKPortOfDischarge;
			exportDeclaration1[JobDeclarationSchema.JE_DateOfArrival] = ExportSailing.Destination.JB_E_ARV;
			exportDeclaration1[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			BusinessObject exportDeclaration2 = JobsForTesting.ExportDeclaration2;
			exportDeclaration2[JobDeclarationSchema.JE_DeclarationReference] = "B00005004";
			exportDeclaration2[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			exportDeclaration2[JobDeclarationSchema.JE_OH_Supplier] = UndeliverableRecipient.PK;
			exportDeclaration2[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			exportDeclaration2[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			exportDeclaration2[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			exportDeclaration2[JobDeclarationSchema.JE_ExportDate] = ExportSailing.Origin.JA_E_DEP;
			exportDeclaration2[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ExportSailing.Destination.JB_RL_NKPortOfDischarge;
			exportDeclaration2[JobDeclarationSchema.JE_DateOfArrival] = ExportSailing.Destination.JB_E_ARV;
			exportDeclaration2[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			ExportSailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(1);
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();

			AddDocumentDeliveredEvents();
			AssertDocumentDelivered(DelayAlertRecipient);
			AssertDocumentDelivered(DelayAlertRecipient3);
			AssertDocumentNotDelivered(UndeliverableRecipient);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">!! Delay alert document failed to be delivered to one or more clients. !!</span></strong><br>
Check the event log of each job, or try to deliver the document manually.<br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-05 00:00 03-Jan-05 00:00
AUSYD               ETD                01-Jan-00 00:00 03-Jan-05 00:00
         MYPKG      ETA                02-Jan-05 00:00 03-Jan-05 00:00

The following Customs Declaration jobs are affected:
B00005003 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Customs.JobDeclaration, exportDeclaration1.PK.ToGuid()) + @") - delay alert delivered to the exporter
B00005004 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Customs.JobDeclaration, exportDeclaration2.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered (Insufficient delay alert delivery information: Error: DeliveryAddress: Please enter an Email Address.)</span></strong>
B00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Customs.JobDeclaration, importDeclaration1.PK.ToGuid()) + @") - delay alert delivered to the importer
B00005002 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Customs.JobDeclaration, importDeclaration2.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered (Insufficient delay alert delivery information: Error: DeliveryAddress: Please enter an Email Address.)</span></strong>

Schedule change notifications");
		}

		[TestDate(2005, 1, 2)]
		public void TestEventAddedWhenDeliveryDidntWork()
		{
			BusinessObject importDeclaration1 = JobsForTesting.ImportDeclaration;
			importDeclaration1[JobDeclarationSchema.JE_DeclarationReference] = "B00005001";
			importDeclaration1[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			importDeclaration1[JobDeclarationSchema.JE_OH_Importer] = DelayAlertRecipient.PK;
			importDeclaration1[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			importDeclaration1[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			importDeclaration1[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ImportSailing.Origin.JA_RL_NKPortOfLoading;
			importDeclaration1[JobDeclarationSchema.JE_ExportDate] = ImportSailing.Origin.JA_E_DEP;
			importDeclaration1[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			importDeclaration1[JobDeclarationSchema.JE_DateOfArrival] = ImportSailing.Destination.JB_E_ARV;
			importDeclaration1[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			new DelayAlertDocumentDeliveryJob(importDeclaration1 as IDocumentSupportable, false).DocumentCommand.SU_FilterList = "truth == lies";
			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">!! Delay alert document failed to be delivered to one or more clients. !!</span></strong><br>
Check the event log of each job, or try to deliver the document manually.<br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-05 00:00 03-Jan-05 00:00

The following Customs Declaration jobs are affected:
B00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Customs.JobDeclaration, importDeclaration1.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered (Delay Alert delivery attempted but failed)</span></strong>

Schedule change notifications");
		}

		[MemoryTestRetryCount(2)]
		public void TestDeliverForVoyageChanges_DontDeliverIfDeclarationHasShipmentAttached()
		{
			BusinessObject importDeclaration = JobsForTesting.ImportDeclaration;
			importDeclaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Air;
			importDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005001";

			importDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			importDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			importDeclaration[JobDeclarationSchema.JE_OH_Importer] = DelayAlertRecipient.PK;
			importDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ImportSailing.Origin.JA_RL_NKPortOfLoading;
			importDeclaration[JobDeclarationSchema.JE_ExportDate] = ImportSailing.Origin.JA_E_DEP;
			importDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			importDeclaration[JobDeclarationSchema.JE_DateOfArrival] = ImportSailing.Destination.JB_E_ARV;
			importDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			ITransportParent declaration = (ITransportParent)importDeclaration;
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			Transport transport = declaration.Transports[0];
			AssertEquals("Precondition: transport.JW_IsLinked", true, transport.JW_IsLinked);
			transport.JW_JX = ImportSailing.PK;
			JobSailing sailing = transport.Sailing;
			AssertNotNull("Precondition: transport.Sailing", sailing);

			sailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();

			sailing = transport.Sailing;
			importDeclaration[JobDeclarationSchema.JE_JS] = ZGuid.Empty;
			sailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			Thread.Sleep(500);
			AssertDocumentDelivered(DelayAlertRecipient);

			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddSeconds(-10));
			var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, DelayAlertRecipient.MainAddress.OA_Email);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			if (printJob != null)
			{
				printJob.Delete();
			}

			CommonShipment attachedShipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			importDeclaration[JobDeclarationSchema.JE_JS] = attachedShipment.PK;
			sailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			Thread.Sleep(500);
			AssertDocumentNotDelivered("Allow the attached CommonShipment to send the delay alert", DelayAlertRecipient);
		}

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_ForImportAgencyShipment()
		{
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now;

			CommonShipment importAgencyDocumentation = JobsForTesting.ImportAgencyDocumentation;
			importAgencyDocumentation.JS_UniqueConsignRef = "V00000100";
			importAgencyDocumentation.JS_JX = ImportSailing.PK;
			importAgencyDocumentation.ConsigneePK = DelayAlertRecipient.PK;

			CommonShipment importAgencyBooking = JobsForTesting.ImportAgencyBooking;
			importAgencyBooking.JS_UniqueConsignRef = "V00000102";
			importAgencyBooking.JS_JX = ImportSailing.PK;
			importAgencyBooking.ConsigneePK = DelayAlertRecipient3.PK;

			CommonShipment exportAgencyDocumentation = JobsForTesting.ExportAgencyDocumentation;
			exportAgencyDocumentation.JS_UniqueConsignRef = "V00000101";
			exportAgencyDocumentation.JS_JX = ExportSailing.PK;
			exportAgencyDocumentation.ConsigneePK = DelayAlertRecipient3.PK;

			CommonShipment exportAgencyBooking = JobsForTesting.ExportAgencyBooking;
			exportAgencyBooking.JS_UniqueConsignRef = "V00000103";
			exportAgencyBooking.JS_JX = ExportSailing.PK;
			exportAgencyBooking.ConsigneePK = DelayAlertRecipient3.PK;

			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(-1);
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);
			AssertDocumentNotDelivered(DelayAlertRecipient3);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();

			AddDocumentDeliveredEvents();
			AssertDocumentDelivered(DelayAlertRecipient);
			AssertDocumentNotDelivered(DelayAlertRecipient3);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include import agency shipments affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">Delay alert documents have been delivered to one or more clients.</span></strong><br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-05 00:00 03-Jan-05 00:00
         MYPKG      ETA                02-Jan-05 00:00 03-Jan-05 00:00

The following Shipping Booking jobs are affected:
V00000102 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBooking, JobsForTesting.ImportAgencyBooking.PK.ToGuid()) + @") 

The following Shipping Bill of Lading jobs are affected:
V00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBillOfLading, JobsForTesting.ImportAgencyDocumentation.PK.ToGuid()) + @") - delay alert delivered to the consignee

Schedule change notifications");
		}

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_ForExportAgencyShipment()
		{
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(4);
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(4);
			ImportSailing.Origin.JA_E_DEP = ZDateTime.Now;
			ExportSailing.Origin.JA_E_DEP = ZDateTime.Now;

			CommonShipment importAgencyDocumentation = JobsForTesting.ImportAgencyDocumentation;
			importAgencyDocumentation.JS_UniqueConsignRef = "V00000100";
			importAgencyDocumentation.JS_JX = ImportSailing.PK;
			importAgencyDocumentation.ConsignorPK = DelayAlertRecipient3.PK;

			CommonShipment importAgencyBooking = JobsForTesting.ImportAgencyBooking;
			importAgencyBooking.JS_UniqueConsignRef = "V00000102";
			importAgencyBooking.JS_JX = ImportSailing.PK;
			importAgencyBooking.ConsignorPK = DelayAlertRecipient3.PK;

			CommonShipment exportAgencyDocumentation = JobsForTesting.ExportAgencyDocumentation;
			exportAgencyDocumentation.JS_UniqueConsignRef = "V00000101";
			exportAgencyDocumentation.JS_JX = ExportSailing.PK;
			exportAgencyDocumentation.ConsignorPK = DelayAlertRecipient.PK;

			CommonShipment exportAgencyBooking = JobsForTesting.ExportAgencyBooking;
			exportAgencyBooking.JS_UniqueConsignRef = "V00000103";
			exportAgencyBooking.JS_JX = ExportSailing.PK;
			exportAgencyBooking.ConsignorPK = DelayAlertRecipient2.PK;

			Factory.Save();

			ImportSailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(-1);
			ExportSailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);
			AssertDocumentNotDelivered(DelayAlertRecipient2);
			AssertDocumentNotDelivered(DelayAlertRecipient3);

			ImportSailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(1);
			ExportSailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(1);
			Factory.Save();

			AddDocumentDeliveredEvents();
			AssertDocumentDelivered(DelayAlertRecipient);
			AssertDocumentDelivered(DelayAlertRecipient2);
			AssertDocumentNotDelivered(DelayAlertRecipient3);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include export agency shipments affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">Delay alert documents have been delivered to one or more clients.</span></strong><br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
MYPKG               ETD                02-Jan-05 00:00 03-Jan-05 00:00
AUSYD               ETD                02-Jan-05 00:00 03-Jan-05 00:00

The following Shipping Booking jobs are affected:
V00000103 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBooking, JobsForTesting.ExportAgencyBooking.PK.ToGuid()) + @") - delay alert delivered to the consignor

The following Shipping Bill of Lading jobs are affected:
V00000101 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBillOfLading, JobsForTesting.ExportAgencyDocumentation.PK.ToGuid()) + @") - delay alert delivered to the consignor

Schedule change notifications");
		}

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_WhenDeliveryInstructionsIncomplete()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.JS_UniqueConsignRef = "S00000100";
			importShipment.ConsigneePK = DelayAlertRecipient.PK;

			OrgContact contact = DelayAlertRecipient.Contacts.AddNew();
			contact.OC_Email = ""; // incomplete delivery instructions
			DelayAlertRecipient.MainAddress.OA_Email = ""; // incomplete delivery instructions
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			document.OD_SU_MenuItem = ShipmentDelayAlertDocument.PK;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">!! Delay alert document failed to be delivered to one or more clients. !!</span></strong><br>
Check the event log of each job, or try to deliver the document manually.<br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-05 00:00 03-Jan-05 00:00

The following Consolidation (and related shipment) jobs are affected:
C00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered (Insufficient delay alert delivery information: Error: DeliveryAddress: Please enter an Email Address.)</span></strong>

Schedule change notifications");
		}

		[TestDate(2005, 1, 2)]
		public void TestDeliverForVoyageChanges_WhenDeliveryFailed()
		{
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";

			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.JS_UniqueConsignRef = "S00000100";
			importShipment.ConsigneePK = DelayAlertRecipient.PK;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertDocumentDelivered(DelayAlertRecipient);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2005, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change",
@"Sailing Schedule changes (first time run) to 02-Jan-05 00:01 UTC
<strong><span style=""color: #FF0000"">!! Delay alert document failed to be delivered to one or more clients. !!</span></strong><br>
Check the event log of each job, or try to deliver the document manually.<br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-05 00:00 03-Jan-05 00:00

The following Consolidation (and related shipment) jobs are affected:
C00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered</span></strong>

Schedule change notifications");
		}

		public void TestDeliverForVoyageChanges_DontDeliverIfDocumentIsToBePrinted()
		{
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.ConsigneePK = DelayAlertRecipient.PK;
			delayAlertRecipient.MainAddress.OA_Email = "";

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);
		}

		[MemoryTestRetryCount(2)]
		public void TestDeliverForVoyageChanges_AllowDelayAlertsToBeSentOnSave_DontDeliverWhenFalse()
		{
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;

			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.ConsigneePK = DelayAlertRecipient.PK;
			Factory.Save();

			var queryProvider = new Mock<IScheduleUpdateQueryProvider>();
			ScheduleUpdateQueryProviderFactory.Set(Factory, () => queryProvider.Object);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(true, false)).Returns(false);
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			StmALog voyageLog = ImportSailing.Voyage.Logs.MostRecentLogByEventTime(Events.DocumentNotDelivered);
			StmALog jobLog = importShipment.Logs.MostRecentLogByEventTime(Events.DocumentNotDelivered);
			AssertEquals("Auto-delivery of Delay Alert document was prevented by the user|RES=CAN|TYP=Delay Alert", voyageLog.SL_Reference);
			AssertEquals("Auto-delivery of Delay Alert document was prevented by the user|RES=CAN|TYP=Delay Alert", jobLog.SL_Reference);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(true, false)).Returns(true);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(2);
			Factory.Save();
			AssertDocumentDelivered(DelayAlertRecipient);

			queryProvider.VerifyAll();
		}

		public void TestDeliverForVoyageChanges_DontCreateDelayAlertNotDeliveredLogIfNoJobsAffectedByETAChange()
		{
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();

			StmALog voyageLogBeforeSave = ImportSailing.Voyage.Logs.MostRecentLogByEventTime(Events.DocumentNotDelivered);
			AssertNull("Precondition: No DocumentNotDelivered log before test", voyageLogBeforeSave);

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();

			StmALog voyageLog = ImportSailing.Voyage.Logs.MostRecentLogByEventTime(Events.DocumentNotDelivered);
			AssertNull("No log should be created if there are no affected jobs", voyageLog);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			FreightDataRegistry.Instance.SeaScheduleChangeNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.PK.ToGuid());
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection()
			{
				new DelayAlertDeliveryRule()
			});

			emailTemplateSwitcher = new ScheduleChangeEmailTextFormatTemplateSwitcher();

			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();

			emailTemplateSwitcher.Dispose();
		}

		void AssertDeliveryJobsFilter(string message, Converter<BusinessObject, string> converter, DelayAlertDeliveryRule filter, BusinessObject[] importJobs, BusinessObject[] exportJobs, JobScheduleChange[] changes)
		{
			DelayAlertDocumentDelivery delivery = new DelayAlertDocumentDelivery();

			SetFilter(filter);

			CombineAssertions(delegate
			{
				AssertDeliveryJobs(message + " - Import", converter, importJobs, delivery.LoadJobsThatRequireImportDelayAlertDelivery(changes));
				AssertDeliveryJobs(message + " - Export", converter, exportJobs, delivery.LoadJobsThatRequireExportDelayAlertDelivery(changes));
			});
		}

		void AssertDeliveryJobs(string message, Converter<BusinessObject, string> converter, BusinessObject[] expected, JobSailingRelatedJob[] actual)
		{
			AssertContainsExactElementsInAnyOrder(
				message,
				BusinessObjectEqualityComparer<BusinessObject>.IgnoreFactoryComparer,
				expected,
				Array.ConvertAll(actual, (r) => r.Job));
		}

		void AssertDocumentDelivered(OrgHeader recipient)
		{
			AssertDocumentDelivered("Document should be delivered", recipient);
		}

		void AssertDocumentDelivered(string message, OrgHeader recipient)
		{
			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
			var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, recipient.MainAddress.OA_Email);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull("Document should be delivered", printJob);
		}

		void AssertDocumentNotDelivered(OrgHeader recipient)
		{
			AssertDocumentNotDelivered("Document should NOT be delivered", recipient);
		}

		void AssertDocumentNotDelivered(string message, OrgHeader recipient)
		{
			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddSeconds(-10));
			var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, recipient.MainAddress.OA_Email);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNull("Document should NOT be delivered", printJob);
		}

		void AssertEmailSent(ZString message, ZString expectedBody)
		{
			AssertEquals(message, 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertMultilineASCIIEquals(message + "; body", expectedBody, Env.OutgoingMailManager.EmailsCreated[0].Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		void AddDocumentDeliveredEvents()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddSeconds(-10));

			StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(query);
			foreach (StmPrintJob printJob in printJobs)
			{
				BaseStmALog documentDeliveredLog = Factory.New<BaseStmALog>();
				documentDeliveredLog.SL_Table = printJob.SP_ParentTableName;
				documentDeliveredLog.SL_Parent = printJob.SP_ParentGuid;
				documentDeliveredLog.SL_SE_NKEvent = Events.DocumentSent.Code;
				documentDeliveredLog.SL_Reference = "clinton@edi.com.au - EDI (BNE) - Import Delay Alert - B00001000";
			}
			Factory.Save();
		}

		void SetFilter(params DelayAlertDeliveryRule[] filters)
		{
			DelayAlertDeliveryRuleCollection collection = new DelayAlertDeliveryRuleCollection();
			collection.AddRange(filters);

			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		JobVoyage NewJobVoyage(ZString transportMode, ZString vesselCode, ZString voyage, ZGuid carrierPK)
		{
			RefVessel vessel = RefVessel.LookupVesselByCode(vesselCode, Factory);
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = vesselCode;
			}

			JobVoyage result = Factory.New<JobVoyage>();
			result.JV_AirSeaRoad = transportMode;
			result.JV_RV_NKVessel = vesselCode;
			result.JV_VoyageFlight = voyage;
			result.JV_OH_Line = carrierPK;

			return result;
		}

		JobSailing NewJobSailing(JobVoyage voyage, ZString loadPort, ZString dischargePort, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			VoyageOrigin origin = NewVoyageOrigin(voyage, loadPort, eTD, aTD);
			VoyageDestination destination = NewVoyageDestination(voyage, dischargePort, eTA, aTA);
			return voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, dischargePort);
		}

		VoyageOrigin NewVoyageOrigin(JobVoyage voyage, ZString loadPort, ZDateTime eTD, ZDateTime aTD)
		{
			VoyageOrigin result = voyage.Origins.AddNew();
			result.JA_RL_NKPortOfLoading = loadPort;
			result.JA_E_DEP = eTD;
			result.JA_A_DEP = aTD;
			return result;
		}

		VoyageDestination NewVoyageDestination(JobVoyage voyage, ZString dischargePort, ZDateTime eTA, ZDateTime aTA)
		{
			VoyageDestination result = voyage.Destinations.AddNew();
			result.JB_RL_NKPortOfDischarge = dischargePort;
			result.JB_E_ARV = eTA;
			result.JB_A_ARV = aTA;
			return result;
		}

		JobsForTesting JobsForTesting
		{
			get
			{
				if (jobsForTesting == null)
				{
					jobsForTesting = new JobsForTesting(Factory);
				}
				return jobsForTesting;
			}
		}
		JobsForTesting jobsForTesting;

		NotificationBuffer Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = new NotificationBuffer();
				}
				return notifications;
			}
		}
		NotificationBuffer notifications;

		GlbGroup NotificationGroup
		{
			get
			{
				if (notificationGroup == null)
				{
					notificationGroup = Factory.New<GlbGroup>();
					GlbStaff emailRecipient = notificationGroup.Staff.AddNew();

					emailRecipient.GS_EmailAddress = "clinton@edi.com.au";
					emailRecipient.GS_Code = "ZAC";
					Factory.Save();
				}
				return notificationGroup;
			}
		}
		GlbGroup notificationGroup;

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null || voyage.IsDeleted)
				{
					voyage = NewJobVoyage(Core.Constants.TransportModes.Sea, "Vessel", "Voyage", ShippingLine.PK);
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		OrgHeader ShippingLine
		{
			get { return shippingLine ?? (shippingLine = new VoyageTestHelper(Factory).CreateCarrier("Carrier")); }
		}
		OrgHeader shippingLine;

		JobSailing ImportSailing
		{
			get
			{
				if (importSailing == null || importSailing.IsDeleted)
				{
					importSailing = NewJobSailing(Voyage, "MYPKG", "AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return importSailing;
			}
		}
		JobSailing importSailing;

		JobSailing ExportSailing
		{
			get
			{
				if (exportSailing == null || exportSailing.IsDeleted)
				{
					exportSailing = NewJobSailing(Voyage, "AUSYD", "MYPKG", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return exportSailing;
			}
		}
		JobSailing exportSailing;

		JobSailing DomesticSailing
		{
			get
			{
				if (domesticSailing == null || domesticSailing.IsDeleted)
				{
					domesticSailing = NewJobSailing(Voyage, "AUPER", "AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return domesticSailing;
			}
		}
		JobSailing domesticSailing;

		JobSailing CrossTradeSailing
		{
			get
			{
				if (crossTradeSailing == null || crossTradeSailing.IsDeleted)
				{
					crossTradeSailing = NewJobSailing(Voyage, "USLAX", "MYPKG", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return crossTradeSailing;
			}
		}
		JobSailing crossTradeSailing;

		OrgHeader DelayAlertRecipient
		{
			get
			{
				if (delayAlertRecipient == null)
				{
					delayAlertRecipient = Factory.NewWithValidTestData<OrgHeader>();
					delayAlertRecipient.MainAddress.OA_Email = "DelayAlertRecipient@edi.com.au";
				}
				return delayAlertRecipient;
			}
		}
		OrgHeader delayAlertRecipient;

		OrgHeader DelayAlertRecipient2
		{
			get
			{
				if (delayAlertRecipient2 == null)
				{
					delayAlertRecipient2 = Factory.NewWithValidTestData<OrgHeader>();
					delayAlertRecipient2.MainAddress.OA_Email = "DelayAlertRecipient2@edi.com.au";
				}
				return delayAlertRecipient2;
			}
		}
		OrgHeader delayAlertRecipient2;

		OrgHeader DelayAlertRecipient3
		{
			get
			{
				if (delayAlertRecipient3 == null)
				{
					delayAlertRecipient3 = Factory.NewWithValidTestData<OrgHeader>();
					delayAlertRecipient3.MainAddress.OA_Email = "DelayAlertRecipient3@edi.com.au";
				}
				return delayAlertRecipient3;
			}
		}
		OrgHeader delayAlertRecipient3;

		OrgHeader UndeliverableRecipient
		{
			get
			{
				if (undeliverableRecipient == null)
				{
					undeliverableRecipient = Factory.NewWithValidTestData<OrgHeader>();
					undeliverableRecipient.MainAddress.OA_Email = "";

					OrgContact contact = undeliverableRecipient.Contacts.AddNew();
					contact.OC_ContactName = "Bob";
					contact.OC_Email = "";

					OrgDocument document = contact.Documents.AddNew();
					document.OD_DocumentGroup = ContactType.All.Code;
					document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
				}
				return undeliverableRecipient;
			}
		}
		OrgHeader undeliverableRecipient;

		DocumentCommand ShipmentDelayAlertDocument
		{
			get
			{
				if (shipmentDelayAlertDocument == null)
				{
					DocumentZQuery query = new DocumentZQuery(BusinessContext.Shipment, "Delay Alert");
					shipmentDelayAlertDocument = Factory.LoadTop1<DocumentCommand>(query);
				}
				return shipmentDelayAlertDocument;
			}
		}
		DocumentCommand shipmentDelayAlertDocument;

		IShowEditFormUrlCreator ShowEditFormUrlCreator
		{
			get { return showEditFormUrlCreator ?? (showEditFormUrlCreator = ObjectFactory.Get<IShowEditFormUrlCreator>()); }
		}
		IShowEditFormUrlCreator showEditFormUrlCreator;

		ScheduleChangeEmailTextFormatTemplateSwitcher emailTemplateSwitcher;

		#endregion
	}
}
