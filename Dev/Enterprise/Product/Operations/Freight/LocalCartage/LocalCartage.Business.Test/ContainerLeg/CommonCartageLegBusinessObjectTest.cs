using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageLeg))]
	public class CommonCartageLegBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocumentMenuItemVisibilityDependingOnImportExportState()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.LooseBookedMoves.DeleteAll();
			var move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			var leg = move.CartageLegs.AddNew();
			Factory.Save();
			var documentCommands = new DocumentCommandCollection(leg);
			documentCommands.Load();
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Export, cartage.JobDirection);
			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "ARV"))
			{
				Assert(!documentCommand.IsApplicable);
			}

			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "DEP"))
			{
				Assert(documentCommand.IsApplicable);
			}

			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			cartage.LooseBookedMoves.DeleteAll();
			move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			leg = move.CartageLegs.AddNew();
			Factory.Save();
			documentCommands = new DocumentCommandCollection(leg);
			documentCommands.Load();
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Import, cartage.JobDirection);
			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "ARV"))
			{
				Assert(documentCommand.IsApplicable);
			}

			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "DEP"))
			{
				Assert(!documentCommand.IsApplicable);
			}
		}

		public void TestWaitPointAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Address1 = "100 Main St";
			var cartage = Factory.New<CommonCartage>();
			cartage.FirstDocAddress.E2_OA_Address = address.PK;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			AssertEquals("", leg.WaitPointAddress);
			leg.JU_E2WaitPointAddressID = address.PK;
			AssertEquals("100 Main St", leg.WaitPointAddress);
		}

		public void TestWaitPointAddressInfo()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(CommonCartageLeg.Schema.WaitPointAddress, leg.WaitPointAddressInfo.Name);
		}

		public void TestWaitPointCity()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_City = "Balmain";
			var cartage = Factory.New<CommonCartage>();
			cartage.FirstDocAddress.E2_OA_Address = address.PK;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			AssertEquals("", leg.WaitPointCity);
			leg.JU_E2WaitPointAddressID = address.PK;
			AssertEquals("Balmain", leg.WaitPointCity);
		}

		public void TestWaitPointCityInfo()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(CommonCartageLeg.Schema.WaitPointCity, leg.WaitPointCityInfo.Name);
		}

		public void TestWaitPointOrganization()
		{
			var org = Factory.New<OrgHeader>();
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			AssertNull(leg.WaitPointOrganisation);
			leg.JU_E2WaitPointAddressID = org.MainAddress.PK;
			AssertEquals(org.PK, leg.WaitPointOrganisation.PK);
		}

		public void TestPickupFromCity()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_City = "Hurstville";
			var cartage = Factory.New<CommonCartage>();
			cartage.FirstDocAddress.E2_OA_Address = address.PK;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			AssertEquals("", leg.PickupFromCity);
			leg.JU_E2PickupAddressID = address.PK;
			AssertEquals("Hurstville", leg.PickupFromCity);
		}

		public void TestPickupCityInfo()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(CommonCartageLeg.Schema.PickupFromCity, leg.PickupFromCityInfo.Name);
		}

		public void TestDeliverToCity()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_City = "Hornsby";
			var cartage = Factory.New<CommonCartage>();
			cartage.FirstDocAddress.E2_OA_Address = address.PK;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			AssertEquals("", leg.DeliverToCity);
			leg.JU_E2DeliveryAddressID = address.PK;
			AssertEquals("Hornsby", leg.DeliverToCity);
		}

		public void TestDeliverToCityInfo()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(CommonCartageLeg.Schema.DeliverToCity, leg.DeliverToCityInfo.Name);
		}

		public void TestValidateTimesOnEy_runsheet()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			CommonWorkSheet sheet = Factory.New<CommonWorkSheet>();
			sheet.EY_StartTime = ZDateTime.Today;
			sheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			AssertNoErrors(leg.JU_EstimatedDeliveryTimeInfo);
			AssertNoErrors(leg.JU_PlannedPickupTimeInfo);
			leg.JU_EY_RunSheet = sheet.PK;
			AssertHasErrors(leg.JU_EstimatedDeliveryTimeInfo);
			AssertHasErrors(leg.JU_PlannedPickupTimeInfo);
		}

		[TestDate(2009, 8, 3, 9, 55, 0)]
		public void TestSignedByCreatesLog()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(0, leg.Logs.GetAllLogs().Count);
			leg.JU_DeliverySignedFor = "Mr.T";
			AssertEquals("only when to consignee", 0, leg.Logs.GetAllLogs().Count);
			CommonBookedCtgMove bookedMove = Factory.New<CommonBookedCtgMove>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			bookedMove.EW_JJ = cartage.PK;
			leg.JU_EW = bookedMove.PK;
			JobDocAddress consigneeAdr = null;
			consigneeAdr = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter, 1);
			consigneeAdr.DocAddressType = DocAddressType.LocalCartageImporter;
			leg.JU_E2DeliveryAddressID = consigneeAdr.PK;
			leg.JU_DeliverySignedFor = "loves";
			AssertEquals("it should not add a log when delivered to time is empty", 0, leg.Logs.GetAllLogs().Count);
			leg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("it should add a log when delivered to consignee", 1, FindEvents(leg.PK, AutoEvents.DeliveryCartageCompleteFinalised).Count());
			leg.Logs.RemoveAndDeleteAll();
			leg.JU_WaitPointTimeOut = ZDateTime.Now;
			AssertEquals("it should add a log when delivered to consignee", 1, FindEvents(leg.PK, AutoEvents.DeliveryCartageCompleteFinalised).Count());
			AssertEquals(new ZDateTime(2009, 8, 3, 9, 55, 0), FindEvents(leg.PK, AutoEvents.DeliveryCartageCompleteFinalised).First().SL_EventTime);
			leg.JU_DeliverTimeOut = ZDateTime.Now.AddDays(2);
			leg.JU_DeliverySignedFor = "Shirley";
			AssertEquals(1, FindEvents(leg.PK, AutoEvents.DeliveryCartageCompleteFinalised).Count());
			AssertEquals(new ZDateTime(2009, 8, 5, 9, 55, 0), FindEvents(leg.PK, AutoEvents.DeliveryCartageCompleteFinalised).First().SL_EventTime);
		}

		public void TestSignature()
		{
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(false, leg.ShowSignature);
			StmData stm = Factory.New<StmData>();
			SignatureData data = new SignatureData(leg);
			Factory.Save();
			AssertEquals(false, leg.ShowSignature);
			stm.SD_Owner = leg.PK;
			AssertEquals(false, leg.ShowSignature);
			Factory.Save();
			AssertEquals(false, leg.ShowSignature);
			var stream = new MemoryStream();
			stream.Write(BitConverter.GetBytes(800), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(480), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(1), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(2), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(300), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(300), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(200), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(200), 0, sizeof(Int32));
			stm.SD_BinaryValue = new ZBlob(stream.ToArray());
			Factory.Save();
			AssertEquals(false, leg.ShowSignature);
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, leg.ShowSignature);
			var img = leg.Signature;
			AssertNotNull(img);
		}

		public void TestUpdateSignature()
		{
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cne = Helper.CreateOrgHeader("CNESYD", "CNE");
			cto.MainAddress.OA_City = "Botany";
			cne.MainAddress.OA_City = "Alexandria";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cne.MainAddress.PK;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			var ctoToCNE = move.CartageLegs[0];
			ctoToCNE.JU_AdditionalService = "FUT";
			ctoToCNE.JU_DeliverySignedFor = "John Smith";
			Factory.Save();
			var signatureCapturedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SignatureCaptured.Code);
			AssertEquals("Precondition. Should not have a signature.", false, ctoToCNE.Signature.HasValidSignature);
			AssertEquals("Precondition. Should not have an event.", 0, ctoToCNE.Logs.Find(signatureCapturedQuery).Length);
			var departmentGuid = Guid.NewGuid();
			ctoToCNE.UpdateSignature(null, departmentGuid);
			Factory.Save();
			AssertEquals("Signature should not be stored when data is null", false, ctoToCNE.Signature.HasValidSignature);
			AssertEquals("Should not add an event.", false, ctoToCNE.Logs.Find(signatureCapturedQuery).Any());
			ctoToCNE.UpdateSignature(Array.Empty<byte>(), departmentGuid);
			Factory.Save();
			AssertEquals("Signature should not be stored when data is empty", false, ctoToCNE.Signature.HasValidSignature);
			AssertEquals("Should not add an event.", false, ctoToCNE.Logs.Find(signatureCapturedQuery).Any());
			var signatureBytes = ZBlob.FromAscii("signatureBytes");
			ctoToCNE.UpdateSignature(signatureBytes, departmentGuid);
			Factory.Save();
			var storedSignatureData1 = new BusinessObjectFactory().Load<CommonCartageLeg>(ctoToCNE.PK).Signature.GetSignatureData();
			AssertEquals("Signature bytes should be the same", signatureBytes, storedSignatureData1.SD_BinaryValue);
			AssertEquals("Signature department ID should be the same", departmentGuid, storedSignatureData1.SD_DepartmentGuid);
			AssertEquals("Should add an event.", 1, ctoToCNE.Logs.Find(signatureCapturedQuery).Length);
			ctoToCNE.UpdateSignature(null, departmentGuid);
			Factory.Save();
			var storedSignatureData2 = new BusinessObjectFactory().Load<CommonCartageLeg>(ctoToCNE.PK).Signature.GetSignatureData();
			AssertEquals("Existing Signature should be removed when data is null", null, storedSignatureData2);
			AssertEquals("Should not have added another event.", 1, ctoToCNE.Logs.Find(signatureCapturedQuery).Length);
		}

		public void TestSettingTimeSetsGroupedLegTimes()
		{
			var sheet = Factory.New<CommonWorkSheet>();
			var leg1 = sheet.CartageLegs.AddNew();
			var leg2 = sheet.CartageLegs.AddNew();
			// No relation
			var now = ZDateTime.Now;
			leg1.JU_PickupTimeIn = now.AddMinutes(1);
			leg1.JU_PickupTimeOut = now.AddMinutes(2);
			leg1.JU_WaitPointTimeIn = now.AddMinutes(3);
			leg1.JU_WaitPointTimeOut = now.AddMinutes(4);
			leg1.JU_DeliverTimeIn = now.AddMinutes(5);
			leg1.JU_DeliverTimeOut = now.AddMinutes(6);
			AssertEquals(ZDateTime.Empty, leg2.JU_PickupTimeIn);
			AssertEquals(ZDateTime.Empty, leg2.JU_PickupTimeOut);
			AssertEquals(ZDateTime.Empty, leg2.JU_WaitPointTimeIn);
			AssertEquals(ZDateTime.Empty, leg2.JU_WaitPointTimeOut);
			AssertEquals(ZDateTime.Empty, leg2.JU_DeliverTimeIn);
			AssertEquals(ZDateTime.Empty, leg2.JU_DeliverTimeOut);
			// Same Sequence - Blank Addresses
			leg1.JU_RunSheetSequence = 2;
			leg2.JU_RunSheetSequence = 2;
			leg1.JU_PickupTimeIn = now.AddMinutes(11);
			leg1.JU_PickupTimeOut = now.AddMinutes(12);
			leg1.JU_WaitPointTimeIn = now.AddMinutes(13);
			leg1.JU_WaitPointTimeOut = now.AddMinutes(14);
			leg1.JU_DeliverTimeIn = now.AddMinutes(15);
			leg1.JU_DeliverTimeOut = now.AddMinutes(16);
			AssertEquals(now.AddMinutes(11), leg2.JU_PickupTimeIn);
			AssertEquals(now.AddMinutes(12), leg2.JU_PickupTimeOut);
			AssertEquals(now.AddMinutes(13), leg2.JU_WaitPointTimeIn);
			AssertEquals(now.AddMinutes(14), leg2.JU_WaitPointTimeOut);
			AssertEquals(now.AddMinutes(15), leg2.JU_DeliverTimeIn);
			AssertEquals(now.AddMinutes(16), leg2.JU_DeliverTimeOut);
			// Same Sequence - Different Addresses
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD, 2);
			cartage.FirstDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			cartage.SecondDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			cartage.ThirdDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			var leg3 = cartage.CartageLegs[0];
			var leg4 = cartage.CartageLegs[1];
			sheet.CartageLegs.Add(leg3);
			sheet.CartageLegs.Add(leg4);
			leg3.JU_RunSheetSequence = 3;
			leg4.JU_RunSheetSequence = 3;
			leg4.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(Factory.New<OrgAddress>(), DocAddressType.LocalCartageCTO).PK;
			leg4.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(Factory.New<OrgAddress>(), DocAddressType.LocalCartageImporter).PK;
			leg4.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(Factory.New<OrgAddress>(), DocAddressType.LocalCartageYard).PK;
			leg3.JU_PickupTimeIn = now.AddMinutes(21);
			leg3.JU_PickupTimeOut = now.AddMinutes(22);
			leg3.JU_WaitPointTimeIn = now.AddMinutes(23);
			leg3.JU_WaitPointTimeOut = now.AddMinutes(24);
			leg3.JU_DeliverTimeIn = now.AddMinutes(25);
			leg3.JU_DeliverTimeOut = now.AddMinutes(26);
			AssertEquals(ZDateTime.Empty, leg4.JU_PickupTimeIn);
			AssertEquals(ZDateTime.Empty, leg4.JU_PickupTimeOut);
			AssertEquals(ZDateTime.Empty, leg4.JU_WaitPointTimeIn);
			AssertEquals(ZDateTime.Empty, leg4.JU_WaitPointTimeOut);
			AssertEquals(ZDateTime.Empty, leg4.JU_DeliverTimeIn);
			AssertEquals(ZDateTime.Empty, leg4.JU_DeliverTimeOut);
			// Same Sequence - Same Addresses
			leg4.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			leg4.JU_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			leg4.JU_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			leg3.JU_PickupTimeIn = now.AddMinutes(31);
			leg3.JU_PickupTimeOut = now.AddMinutes(32);
			leg3.JU_WaitPointTimeIn = now.AddMinutes(33);
			leg3.JU_WaitPointTimeOut = now.AddMinutes(34);
			leg3.JU_DeliverTimeIn = now.AddMinutes(35);
			leg3.JU_DeliverTimeOut = now.AddMinutes(36);
			AssertEquals(now.AddMinutes(31), leg4.JU_PickupTimeIn);
			AssertEquals(now.AddMinutes(32), leg4.JU_PickupTimeOut);
			AssertEquals(now.AddMinutes(33), leg4.JU_WaitPointTimeIn);
			AssertEquals(now.AddMinutes(34), leg4.JU_WaitPointTimeOut);
			AssertEquals(now.AddMinutes(35), leg4.JU_DeliverTimeIn);
			AssertEquals(now.AddMinutes(36), leg4.JU_DeliverTimeOut);
		}

		public void TestSettingTruckSetsTransportCoWhenEmpty()
		{
			CoreSettingTruckSetsTransportCoWhenEmpty(ZDateTime.Now);
		}

		public void TestSettingTruckSetsTransportCoWhenEmptyIfPickupTimeAlmostMidnight()
		{
			CoreSettingTruckSetsTransportCoWhenEmpty(ZDateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59));
		}

		void CoreSettingTruckSetsTransportCoWhenEmpty(ZDateTime plannedPickupTime)
		{
			OrgHeader someOrg = Factory.New<OrgHeader>();
			OrgHeader proxy = Factory.New<OrgHeader>();
			GlbBranch branch = Factory.New<GlbBranch>();
			RefEquipment truck = Factory.New<RefEquipment>();
			CommonWorkSheet sheet = Factory.New<CommonWorkSheet>();
			sheet.EY_StartTime = plannedPickupTime.Date;
			sheet.EY_EndTime = plannedPickupTime.Date.AddHours(23).AddMinutes(59);
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = plannedPickupTime;
			branch.GB_OH_OrgProxy = proxy.PK;
			truck.RQ_OH_Owner = proxy.PK;
			leg.QuickOHTransportCompany = someOrg.PK;
			leg.QuickRQTruck = truck.PK;
			AssertEquals(someOrg.PK, leg.QuickOHTransportCompany);
			leg.QuickRQTruck = ZGuid.Empty;
			leg.QuickOHTransportCompany = ZGuid.Empty;
			sheet.EY_RQ_Truck = truck.PK;
			leg.QuickOHTransportCompany = ZGuid.Empty;
			sheet.EY_OH_TransportCo = ZGuid.Empty;
			leg.QuickRQTruck = truck.PK;
			AssertEquals(proxy.PK, leg.QuickOHTransportCompany);
			AssertEquals(proxy.PK, sheet.EY_OH_TransportCo);
		}

		public void TestClearDeletedDocAddress()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "CNR Address", "2000", "Sydney", "AUSYD", true);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "CNE Address", "2000", "Sydney", "AUSYD", true);
			leg.JU_E2PickupAddressID = cnr.PK;
			leg.JU_E2WaitPointAddressID = cne.PK;
			leg.JU_E2DeliveryAddressID = cne.PK;
			leg.ClearDeletedDocAddress(cnr.PK);
			AssertEquals(ZGuid.Empty, leg.JU_E2PickupAddressID);
			AssertEquals(cne.PK, leg.JU_E2WaitPointAddressID);
			AssertEquals(cne.PK, leg.JU_E2DeliveryAddressID);
			leg.ClearDeletedDocAddress(cne.PK);
			AssertEquals(ZGuid.Empty, leg.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg.JU_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, leg.JU_E2DeliveryAddressID);
		}

		public void TestConfirmIsIDistanceConsumer()
		{
			AssertEquals(true, Factory.New<CommonCartageLeg>() is IDistanceCalculationConsumer);
		}

		public void TestIDistanceCalculationConsumer_Checkpoint()
		{
			AssertEquals(Env.Security.RoadDistanceCalculationServiceLocalTransport, ((IDistanceCalculationConsumer)Factory.New<CommonCartageLeg>()).Checkpoint);
		}

		public void TestSetCalculatedDistance()
		{
			OrgAddress packAddress = Factory.New<OrgAddress>();
			packAddress.OA_City = "Pack";
			packAddress.OA_Address1 = "Address";
			OrgAddress unpackAddress = Factory.New<OrgAddress>();
			unpackAddress.OA_City = "Unpack";
			unpackAddress.OA_Address1 = "Address";
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.BookedMovesCollection.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			NotificationBuffer notifications = new NotificationBuffer();
			FreightDistanceCalculator calculator = new FreightDistanceCalculator(leg, notifications);
			JobDocAddress pickupDocAddress = Factory.New<JobDocAddress>();
			pickupDocAddress.E2_OA_Address = packAddress.PK;
			cartage.DocAddresses.Add(pickupDocAddress);
			leg.JU_E2PickupAddressID = pickupDocAddress.PK;
			JobDocAddress deliveryDocAddress = Factory.New<JobDocAddress>();
			deliveryDocAddress.E2_OA_Address = unpackAddress.PK;
			cartage.DocAddresses.Add(deliveryDocAddress);
			leg.JU_E2DeliveryAddressID = deliveryDocAddress.PK;
			leg.JU_DistanceUnit = Constants.Length.Miles;
			calculator.SetCalculatedDistance();
			AssertEquals(new ZDecimal("PackAddressAustraliaUnpackAddressAustralia".Length), leg.JU_Distance);
			AssertEquals(Constants.Length.Miles, leg.JU_DistanceUnit);
			leg.JU_DistanceUnit = Constants.Length.Kilometres;
			calculator.SetCalculatedDistance();
			AssertEquals(new ZDecimal("PackAddressAustraliaUnpackAddressAustralia".Length), leg.JU_Distance);
			AssertEquals(Constants.Length.Kilometres, leg.JU_DistanceUnit);
			leg.JU_DistanceUnit = Constants.Length.Kilometres;
			calculator = new FreightDistanceCalculator(leg, notifications);
			calculator.SetCalculatedDistance();
			AssertEquals(new ZDecimal("PackAddressAustraliaUnpackAddressAustralia".Length), leg.JU_Distance);
			AssertEquals(Constants.Length.Kilometres, leg.JU_DistanceUnit);
		}

		public void TestSetupDistanceCalculationConfiguration()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(consignor, "CNS", "10", "AAA");
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(consignee, "CNE", "20", "BBB");
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.BookedMovesCollection.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			NotificationBuffer notifications = new NotificationBuffer();
			FreightDistanceCalculator calculator = new FreightDistanceCalculator(leg, notifications);
			OrgAddress packAddress = Factory.New<OrgAddress>();
			packAddress.OA_City = "Pack";
			packAddress.OA_Address1 = "Address";
			OrgAddress unpackAddress = Factory.New<OrgAddress>();
			unpackAddress.OA_City = "Unpack";
			unpackAddress.OA_Address1 = "Address";
			JobDocAddress pickupDocAddress = Factory.New<JobDocAddress>();
			pickupDocAddress.E2_OA_Address = packAddress.PK;
			cartage.DocAddresses.Add(pickupDocAddress);
			pickupDocAddress.OrganisationPK = consignor.PK;
			leg.JU_E2PickupAddressID = pickupDocAddress.PK;
			JobDocAddress deliveryDocAddress = Factory.New<JobDocAddress>();
			deliveryDocAddress.E2_OA_Address = unpackAddress.PK;
			cartage.DocAddresses.Add(deliveryDocAddress);
			deliveryDocAddress.OrganisationPK = consignee.PK;
			leg.JU_E2DeliveryAddressID = deliveryDocAddress.PK;
			new JobHeader.Loader(leg.Cartage).TryLoadOrCreate();
			leg.Cartage.LocalClientPK = consignor.PK;
			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)leg).DistanceCalculationConfig, consignor);
		}

		void SetupClientDistanceConfig(OrgHeader client, string provider, string version, string method)
		{
			client.MiscServ.OM_CMDistanceCalculationProvider = provider;
			client.MiscServ.OM_CMDistanceCalculationVersion = version;
			client.MiscServ.OM_CMDistanceCalculationMethod = method;
		}

		void AssertDistanceConfigIsFromClient(DistanceCalculationConfiguration config, OrgHeader client)
		{
			AssertEquals("Provider", client.MiscServ.OM_CMDistanceCalculationProvider, config.ProviderCode);
			AssertEquals("Version", client.MiscServ.OM_CMDistanceCalculationVersion, config.ProviderVersion);
			AssertEquals("Method", client.MiscServ.OM_CMDistanceCalculationMethod, config.CalculationMethod);
		}

		public void TestSettingE2()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Org1";
			OrgAddress org1Address1 = org1.MainAddress;
			org1Address1.OA_Code = "org1Address1";
			OrgAddress org1Address2 = org1.Addresses.AddNew();
			org1Address2.OA_Code = "org1Address2";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Org2";
			OrgAddress org2Address1 = org2.MainAddress;
			org2Address1.OA_Code = "org2Address1";
			OrgAddress org2Address2 = org2.Addresses.AddNew();
			org2Address2.OA_Code = "org2Address2";
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "Org3";
			OrgAddress org3Address1 = org3.MainAddress;
			org3Address1.OA_Code = "org3Address1";
			OrgAddress org3Address2 = org3.Addresses.AddNew();
			org3Address2.OA_Code = "org3Address2";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.FirstDocAddress.E2_OA_Address = org1Address1.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2Address1.PK;
			AssertEquals(2, cartage.DocAddresses.Count);
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			leg1.JU_E2DeliveryAddressID = org1Address2.PK;
			AssertNotEquals(org1Address2.PK, leg1.JU_E2DeliveryAddressID);
			JobDocAddress org1docAddress2 = Factory.Load<JobDocAddress>(leg1.JU_E2DeliveryAddressID);
			AssertEquals(3, cartage.DocAddresses.Count);
			leg1.JU_E2DeliveryAddressID = cartage.FirstDocAddress.PK;
			AssertEquals(3, cartage.DocAddresses.Count);
		}

		public void TestQuickRQTruckUsesCorrectListAttribute()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			PropertyInfo pi = PropertyInfoFetcher.GetFromLowestSubclass(leg.GetType(), "QuickRQTruck");
			object[] listAttribs = pi.GetCustomAttributes(typeof(ListAttribute), false);
			foreach (object attrib in listAttribs)
			{
				ListAttribute listAtt = attrib as ListAttribute;
				if (listAtt.ListDataSourceMember.ToLower().Contains("vehicles") && listAtt.ListDataSourceMember.ToLower() != "vehicles")
				{
					Assert("QuickRQTruck property should use the Vehicles in CommonCartageLeg class for its List Attribute because this list uses FilterDefaults for Driver", false);
				}
			}

			Assert("Should use a list attribute for QuickRQTruck property", listAttribs.Length > 0);
		}

		public void TestVehiclesListFilterDefaults()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			foreach (FilterBusinessObjectDefault fbod in leg.BindToLists.Vehicles.FilterBusinessObjectDefaults)
			{
				if (!leg.Vehicles.FilterBusinessObjectDefaults.ContainsDefaultFor(fbod.Key))
				{
					Assert("Some FilterDefault was added to CommonCartageLeg.BindToList.Vehicles which doesn't exist in CommonCartageLeg.Vehicles", false);
				}
			}

			AssertEquals("CommonCartageLeg.Vehicles list should use Driver:Property Filter Default", true, leg.Vehicles.FilterBusinessObjectDefaults.ContainsDefaultFor("Driver:Property"));
			AssertEquals("CommonCartageLeg.Vehicles list should use Vehicle Status:Property Filter Default", true, leg.Vehicles.FilterBusinessObjectDefaults.ContainsDefaultFor("Vehicle Status:Property"));
		}

		public void TestQuickAllocationRunSheet()
		{
			CoreQuickAllocationRunSheet(ZDateTime.Now.AddDays(2));
		}

		public void TestQuickAllocationRunSheetIfPlannedPickupTimeAlmostMidnight()
		{
			CoreQuickAllocationRunSheet(ZDateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59).AddDays(2));
		}

		void CoreQuickAllocationRunSheet(ZDateTime plannedPickupTime)
		{
			var bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			var bill = Factory.New<GlbStaff>();
			bill.GS_FullName = "Bill Ranger";
			bill.GS_LoginName = "Bill";
			bill.GS_Code = "Bil";
			var bobsTruckingCo = Factory.New<OrgHeader>();
			bobsTruckingCo.OH_Code = "Bob";
			var billsTruckingCo = Factory.New<OrgHeader>();
			billsTruckingCo.OH_Code = "Bill";
			var bobsTruck = Factory.New<RefEquipment>();
			bobsTruck.RQ_ShortCode = "BobT";
			bobsTruck.RQ_IsVehicle = true;
			bobsTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			var billsTruck = Factory.New<RefEquipment>();
			billsTruck.RQ_ShortCode = "BillT";
			billsTruck.RQ_IsVehicle = true;
			billsTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			var cartage = Factory.New<CommonCartage>();
			var move1 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			var leg2 = move1.CartageLegs.AddNew();
			leg1.QuickRQTruck = bobsTruck.PK;
			AssertEquals(bob.GS_Code, leg1.QuickGSDriver);
			Factory.Save();
			AssertNull(leg1.WorkSheet);
			leg1.JU_PlannedPickupTime = plannedPickupTime;
			Factory.Save();
			var runSheet = leg1.WorkSheet;
			AssertNotNull(runSheet);
			AssertEquals(bobsTruck.PK, runSheet.EY_RQ_Truck);
			AssertEquals(bob.GS_Code, runSheet.EY_GS_NKTruckDriver);
			AssertEquals(ZGuid.Empty, runSheet.EY_OH_TransportCo);
			AssertEquals(leg1.JU_PlannedPickupTime.Date, runSheet.EY_StartTime);
			AssertEquals(leg1.JU_PlannedPickupTime.EndOfDay().AddSeconds(-59), runSheet.EY_EndTime);
			AssertEquals(1, runSheet.CartageLegs.Count);
			Assert(runSheet.CartageLegs.Contains(leg1));
			leg1.QuickOHTransportCompany = bobsTruckingCo.PK;
			AssertEquals("Only leg so change", bobsTruckingCo.PK, runSheet.EY_OH_TransportCo);
			AssertEquals(runSheet.PK, leg1.JU_EY_RunSheet);
			Factory.Save();
			leg1.QuickRQTruck = billsTruck.PK;
			AssertEquals("Driver should still be BOB", bob.GS_Code, leg1.QuickGSDriver);
			AssertEquals("Only leg so change runsheet value", billsTruck.PK, runSheet.EY_RQ_Truck);
			AssertEquals(runSheet.PK, leg1.JU_EY_RunSheet);
			Factory.Save();
			leg1.QuickRQTruck = billsTruck.PK;
			AssertEquals("Driver should still be BOB", bob.GS_Code, leg1.QuickGSDriver);
			AssertEquals("Only leg so change runsheet value", billsTruck.PK, runSheet.EY_RQ_Truck);
			AssertEquals(runSheet.PK, leg1.JU_EY_RunSheet);
			Factory.Save();
			leg2.JU_PlannedPickupTime = leg1.JU_PlannedPickupTime;
			leg2.QuickRQTruck = billsTruck.PK;
			AssertEquals("Should check for runsheets of bill before defaulting the driver", runSheet.PK, leg2.JU_EY_RunSheet);
			AssertEquals(bob.GS_Code, leg2.QuickGSDriver);
			AssertEquals(bobsTruckingCo.PK, leg2.QuickOHTransportCompany);
			leg2.QuickGSDriver = bill.GS_Code;
			AssertEquals("The runsheet contains other legs, and changing values, remove runsheet", ZGuid.Empty, leg2.JU_EY_RunSheet);
			AssertEquals(bill.GS_Code, leg2.QuickGSDriver);
			AssertEquals(billsTruck.PK, leg2.QuickRQTruck);
			AssertEquals(bobsTruckingCo.PK, leg2.QuickOHTransportCompany);
			leg2.QuickOHTransportCompany = ZGuid.Empty;
			AssertEquals("Still empty", ZGuid.Empty, leg2.JU_EY_RunSheet);
			AssertEquals(bill.GS_Code, leg2.QuickGSDriver);
			AssertEquals(billsTruck.PK, leg2.QuickRQTruck);
			AssertEquals(ZGuid.Empty, leg2.QuickOHTransportCompany);
			Factory.Save();
			var runSheet2 = leg2.WorkSheet;
			AssertNotNull(runSheet2);
			AssertEquals(bill.GS_Code, runSheet2.EY_GS_NKTruckDriver);
			AssertEquals(billsTruck.PK, runSheet2.EY_RQ_Truck);
			AssertEquals(ZGuid.Empty, runSheet2.EY_OH_TransportCo);
			AssertEquals(leg1.JU_PlannedPickupTime.Date, runSheet2.EY_StartTime);
			AssertEquals(leg1.JU_PlannedPickupTime.EndOfDay().AddSeconds(-59), runSheet2.EY_EndTime);
			AssertEquals(1, runSheet2.CartageLegs.Count);
			Assert(runSheet2.CartageLegs.Contains(leg2));
			leg1.JU_EY_RunSheet = runSheet2.PK;
			leg2.QuickOHTransportCompany = billsTruckingCo.PK;
			AssertEquals("Even though rs has more than 1 leg, its changing from an empty value", billsTruckingCo.PK, runSheet2.EY_OH_TransportCo);
			AssertEquals(billsTruck.PK, leg1.QuickRQTruck);
			AssertEquals(bill.GS_Code, leg2.QuickGSDriver);
			AssertEquals(2, runSheet2.CartageLegs.Count);
			AssertEquals(0, runSheet.CartageLegs.Count);
			runSheet.EY_StartTime = plannedPickupTime.Date.AddDays(4);
			runSheet.EY_EndTime = plannedPickupTime.Date.AddDays(5);
			runSheet.EY_GS_NKTruckDriver = bill.GS_Code;
			runSheet.EY_RQ_Truck = billsTruck.PK;
			runSheet.EY_OH_TransportCo = billsTruckingCo.PK;
			leg1.QuickPlannedPickupTime = plannedPickupTime.AddDays(4);
			AssertEquals("leg1 should now be on runSheet1", runSheet.PK, leg1.JU_EY_RunSheet);
			leg1.QuickPlannedPickupTime = plannedPickupTime.AddDays(6);
			AssertEquals("leg1 should STILL be on runSheet1", runSheet.PK, leg1.JU_EY_RunSheet);
			AssertEquals("runSheet1 should have changed start time to match (cause only leg)", plannedPickupTime.Date.AddDays(6), runSheet.EY_StartTime);
			AssertEquals("runSheet1 should have changed end time to match (cause only leg)", plannedPickupTime.AddDays(6).EndOfDay().AddSeconds(-59), runSheet.EY_EndTime);
			leg1.QuickPlannedPickupTime = ZDateTime.Empty;
			AssertEquals("leg1 should have no runsheet", ZGuid.Empty, leg1.JU_EY_RunSheet);
		}

		// These are regression tests added to make sure that original code intention was kept - ie if run sheet no longer passes QuickEstimatedRunSheetTime criteria then remove run sheet from leg
		public void TestQuickAllocationRunSheetRemovedFromLegIfNoLongerBetweenRunSheetTimes()
		{
			CoreTestQuickAllocationRunSheetRemovedFromLegIfNoLongerBetweenRunSheetTimes(ZDateTime.Now);
		}

		public void TestQuickAllocationRunSheetRemovedFromLegIfNoLongerBetweenRunSheetTimesWhenPickupTimeJustBeforeMidnight()
		{
			CoreTestQuickAllocationRunSheetRemovedFromLegIfNoLongerBetweenRunSheetTimes(ZDateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59));
		}

		void CoreTestQuickAllocationRunSheetRemovedFromLegIfNoLongerBetweenRunSheetTimes(ZDateTime plannedPickupTime)
		{
			var bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			var bill = Factory.New<GlbStaff>();
			bill.GS_FullName = "Bill Ranger";
			bill.GS_LoginName = "Bill";
			bill.GS_Code = "Bil";
			var bobsTruckingCo = Factory.New<OrgHeader>();
			bobsTruckingCo.OH_Code = "Bob";
			var billsTruckingCo = Factory.New<OrgHeader>();
			billsTruckingCo.OH_Code = "Bill";
			var bobsTruck = Factory.New<RefEquipment>();
			bobsTruck.RQ_ShortCode = "BobT";
			bobsTruck.RQ_IsVehicle = true;
			bobsTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			var billsTruck = Factory.New<RefEquipment>();
			billsTruck.RQ_ShortCode = "BillT";
			billsTruck.RQ_IsVehicle = true;
			billsTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			var cartage = Factory.New<CommonCartage>();
			var move1 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			var leg2 = move1.CartageLegs.AddNew();
			leg1.QuickRQTruck = bobsTruck.PK;
			AssertEquals("Precondition: Bob is set as driver for leg 1", bob.GS_Code, leg1.QuickGSDriver);
			Factory.Save();
			AssertNull(leg1.WorkSheet);
			leg1.JU_PlannedPickupTime = plannedPickupTime;
			Factory.Save();
			var runSheet = leg1.WorkSheet;
			AssertNotNull(runSheet);
			AssertEquals("Precondition: created runSheet has just one leg", 1, runSheet.CartageLegs.Count);
			Assert("Precondition: created runSheet contains leg1", runSheet.CartageLegs.Contains(leg1));
			leg1.QuickRQTruck = billsTruck.PK;
			AssertEquals("Precondition: leg1 still attached to runsheet", runSheet.PK, leg1.JU_EY_RunSheet);
			Factory.Save();
			leg2.JU_PlannedPickupTime = leg1.JU_PlannedPickupTime;
			leg2.QuickRQTruck = billsTruck.PK;
			AssertEquals("Precondition: leg2 now attached to runSheet", runSheet.PK, leg2.JU_EY_RunSheet);
			AssertEquals("Precondition: leg2 now has bob as driver", bob.GS_Code, leg2.QuickGSDriver);
			runSheet.EY_EndTime = plannedPickupTime.AddMinutes(-1);
			leg2.QuickOHTransportCompany = billsTruckingCo.PK;
			AssertEquals("Should have removed run sheet from leg as even though we are adding info leg no longer satisfies run sheet start and end times", ZGuid.Empty, leg2.JU_EY_RunSheet);
		}

		public void TestSettingRunSheetValidatesDriverTruck()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			CommonWorkSheet sheet = Factory.New<CommonWorkSheet>();
			GlbGroup driversGroup = Factory.New<GlbGroup>();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_LoginName = "bob";
			bob.GS_Code = "bob";
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_IsVehicle = false;
			leg.QuickGSDriver = bob.GS_Code;
			leg.Validation.ValidateQuickGSDriver();
			AssertHasErrors(leg.QuickGSDriverInfo);
			leg.QuickRQTruck = truck.PK;
			leg.Validation.ValidateQuickRQTruck();
			AssertHasErrors(leg.QuickRQTruckInfo);
			bob.Groups.Add(driversGroup);
			truck.RQ_IsVehicle = true;
			leg.JU_EY_RunSheet = sheet.PK;
			AssertNoErrors(leg.QuickGSDriverInfo);
			AssertNoErrors(leg.QuickRQTruckInfo);
		}

		public void TestQuickAllocationRunSheet_EstimatedDelivery()
		{
			CoreQuickAllocationRunSheet_EstimatedDelivery(ZDateTime.Now.AddDays(2));
		}

		public void TestQuickAllocationRunSheet_EstimatedDeliveryIfAlmostMidnight()
		{
			CoreQuickAllocationRunSheet_EstimatedDelivery(ZDateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59).AddDays(2));
		}

		void CoreQuickAllocationRunSheet_EstimatedDelivery(ZDateTime estimatedDeliveryTime)
		{
			var bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			var bill = Factory.New<GlbStaff>();
			bill.GS_FullName = "Bill Ranger";
			bill.GS_LoginName = "Bill";
			bill.GS_Code = "Bil";
			var bobsTruckingCo = Factory.New<OrgHeader>();
			bobsTruckingCo.OH_Code = "Bob";
			var billsTruckingCo = Factory.New<OrgHeader>();
			billsTruckingCo.OH_Code = "Bill";
			var bobsTruck = Factory.New<RefEquipment>();
			bobsTruck.RQ_ShortCode = "BobT";
			bobsTruck.RQ_IsVehicle = true;
			bobsTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			var billsTruck = Factory.New<RefEquipment>();
			billsTruck.RQ_ShortCode = "BillT";
			billsTruck.RQ_IsVehicle = true;
			billsTruck.RQ_GS_NKPreferredDriver = bill.GS_Code;
			var cartage = Factory.New<CommonCartage>();
			var move1 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			var leg2 = move1.CartageLegs.AddNew();
			leg1.QuickRQTruck = bobsTruck.PK;
			AssertEquals(bob.GS_Code, leg1.QuickGSDriver);
			Factory.Save();
			AssertNull(leg1.WorkSheet);
			leg1.JU_EstimatedDeliveryTime = estimatedDeliveryTime;
			Factory.Save();
			var runSheet = leg1.WorkSheet;
			AssertNotNull(runSheet);
			AssertEquals(bobsTruck.PK, runSheet.EY_RQ_Truck);
			AssertEquals(bob.GS_Code, runSheet.EY_GS_NKTruckDriver);
			AssertEquals(ZGuid.Empty, runSheet.EY_OH_TransportCo);
			AssertEquals(leg1.JU_EstimatedDeliveryTime.Date, runSheet.EY_StartTime);
			AssertEquals(leg1.JU_EstimatedDeliveryTime.EndOfDay().AddSeconds(-59), runSheet.EY_EndTime);
			AssertEquals(1, runSheet.CartageLegs.Count);
			Assert(runSheet.CartageLegs.Contains(leg1));
			leg1.QuickOHTransportCompany = bobsTruckingCo.PK;
			AssertEquals("Only leg so change", bobsTruckingCo.PK, runSheet.EY_OH_TransportCo);
			AssertEquals(runSheet.PK, leg1.JU_EY_RunSheet);
			Factory.Save();
			leg1.QuickRQTruck = billsTruck.PK;
			AssertEquals("Driver should still be BOB", bob.GS_Code, leg1.QuickGSDriver);
			AssertEquals("Only leg so change runsheet value", billsTruck.PK, runSheet.EY_RQ_Truck);
			AssertEquals(runSheet.PK, leg1.JU_EY_RunSheet);
			Factory.Save();
			leg2.JU_EstimatedDeliveryTime = leg1.JU_EstimatedDeliveryTime;
			leg2.QuickRQTruck = billsTruck.PK;
			AssertEquals("Should check for runsheets of bill before defaulting the driver", runSheet.PK, leg2.JU_EY_RunSheet);
			AssertEquals(bob.GS_Code, leg2.QuickGSDriver);
			AssertEquals(bobsTruckingCo.PK, leg2.QuickOHTransportCompany);
			leg2.QuickGSDriver = bill.GS_Code;
			AssertEquals("The runsheet contains other legs, and changing values, remove runsheet", ZGuid.Empty, leg2.JU_EY_RunSheet);
			AssertEquals(bill.GS_Code, leg2.QuickGSDriver);
			AssertEquals(billsTruck.PK, leg2.QuickRQTruck);
			AssertEquals(bobsTruckingCo.PK, leg2.QuickOHTransportCompany);
			leg2.QuickOHTransportCompany = ZGuid.Empty;
			AssertEquals("Still empty", ZGuid.Empty, leg2.JU_EY_RunSheet);
			AssertEquals(bill.GS_Code, leg2.QuickGSDriver);
			AssertEquals(billsTruck.PK, leg2.QuickRQTruck);
			AssertEquals(ZGuid.Empty, leg2.QuickOHTransportCompany);
			Factory.Save();
			var runSheet2 = leg2.WorkSheet;
			AssertNotNull(runSheet2);
			AssertEquals(bill.GS_Code, runSheet2.EY_GS_NKTruckDriver);
			AssertEquals(billsTruck.PK, runSheet2.EY_RQ_Truck);
			AssertEquals(ZGuid.Empty, runSheet2.EY_OH_TransportCo);
			AssertEquals(leg1.JU_EstimatedDeliveryTime.Date, runSheet2.EY_StartTime);
			AssertEquals(leg1.JU_EstimatedDeliveryTime.EndOfDay().AddSeconds(-59), runSheet2.EY_EndTime);
			AssertEquals(1, runSheet2.CartageLegs.Count);
			Assert(runSheet2.CartageLegs.Contains(leg2));
			leg1.JU_EY_RunSheet = runSheet2.PK;
			leg2.QuickOHTransportCompany = billsTruckingCo.PK;
			AssertEquals("Even though rs has more than 1 leg, its changing from an empty value", billsTruckingCo.PK, runSheet2.EY_OH_TransportCo);
			AssertEquals(billsTruck.PK, leg1.QuickRQTruck);
			AssertEquals(bill.GS_Code, leg2.QuickGSDriver);
			AssertEquals(2, runSheet2.CartageLegs.Count);
			AssertEquals(0, runSheet.CartageLegs.Count);
			runSheet.EY_StartTime = estimatedDeliveryTime.Date.AddDays(4);
			runSheet.EY_EndTime = estimatedDeliveryTime.Date.AddDays(5);
			runSheet.EY_GS_NKTruckDriver = bill.GS_Code;
			runSheet.EY_RQ_Truck = billsTruck.PK;
			runSheet.EY_OH_TransportCo = billsTruckingCo.PK;
			leg1.QuickEstimatedDeliveryTime = estimatedDeliveryTime.AddDays(4);
			AssertEquals("leg1 should now be on runSheet1", runSheet.PK, leg1.JU_EY_RunSheet);
			leg1.QuickEstimatedDeliveryTime = estimatedDeliveryTime.AddDays(6);
			AssertEquals("leg1 should STILL be on runSheet1", runSheet.PK, leg1.JU_EY_RunSheet);
			AssertEquals("runSheet1 should have changed start time to match (cause only leg)", estimatedDeliveryTime.Date.AddDays(6), runSheet.EY_StartTime);
			AssertEquals("runSheet1 should have changed end time to match (cause only leg)", estimatedDeliveryTime.AddDays(6).EndOfDay().AddSeconds(-59), runSheet.EY_EndTime);
			leg1.QuickEstimatedDeliveryTime = ZDateTime.Empty;
			AssertEquals("leg1 should have no runsheet", ZGuid.Empty, leg1.JU_EY_RunSheet);
		}

		public void TestQuickGSDriver_HasChanges()
		{
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move1.CartageLegs.AddNew();
			Factory.Save();
			leg1.QuickGSDriver = bob.GS_Code;
			Assert(leg1.HasChanges);
		}

		public void TestAutoCalculateDemurrage_Pickup()
		{
			var leg = Factory.New<CommonCartageLeg>();
			var now = ZDateTime.Now;
			TransportRegistry.Instance.AutoPopulateDemurrage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			leg.JU_PickupTimeIn = now;
			leg.JU_PickupTimeOut = now.AddHours(2).AddMinutes(20);
			AssertEquals(ZDateTime.Empty, leg.JU_CartagePickupDemurrage);
			TransportRegistry.Instance.AutoPopulateDemurrage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			leg.JU_PickupTimeIn = now;
			leg.JU_PickupTimeOut = now.AddHours(2).AddMinutes(30);
			AssertEquals(2, leg.JU_CartagePickupDemurrage.Hour);
			AssertEquals(30, leg.JU_CartagePickupDemurrage.Minute);
			leg.JU_PickupTimeIn = ZDateTime.Invalid;
			AssertEquals("Keep existing", 2, leg.JU_CartagePickupDemurrage.Hour);
			AssertEquals("Keep existing", 30, leg.JU_CartagePickupDemurrage.Minute);
			leg.JU_PickupTimeIn = now;
			leg.JU_PickupTimeOut = ZDateTime.Invalid;
			AssertEquals("Keep existing", 2, leg.JU_CartagePickupDemurrage.Hour);
			AssertEquals("Keep existing", 30, leg.JU_CartagePickupDemurrage.Minute);
		}

		public void TestAutoCalculateDemurrage_WaitPoint()
		{
			var leg = Factory.New<CommonCartageLeg>();
			var now = ZDateTime.Now;
			TransportRegistry.Instance.AutoPopulateDemurrage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			leg.JU_WaitPointTimeIn = now;
			leg.JU_WaitPointTimeOut = now.AddHours(2).AddMinutes(20);
			AssertEquals(ZDateTime.Empty, leg.JU_CartageWaitPointDemurrage);
			TransportRegistry.Instance.AutoPopulateDemurrage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			leg.JU_WaitPointTimeIn = now;
			leg.JU_WaitPointTimeOut = now.AddHours(2).AddMinutes(30);
			AssertEquals(2, leg.JU_CartageWaitPointDemurrage.Hour);
			AssertEquals(30, leg.JU_CartageWaitPointDemurrage.Minute);
			leg.JU_WaitPointTimeIn = ZDateTime.Invalid;
			AssertEquals("Keep existing", 2, leg.JU_CartageWaitPointDemurrage.Hour);
			AssertEquals("Keep existing", 30, leg.JU_CartageWaitPointDemurrage.Minute);
			leg.JU_WaitPointTimeIn = now;
			leg.JU_WaitPointTimeOut = ZDateTime.Invalid;
			AssertEquals("Keep existing", 2, leg.JU_CartageWaitPointDemurrage.Hour);
			AssertEquals("Keep existing", 30, leg.JU_CartageWaitPointDemurrage.Minute);
		}

		public void TestAutoCalculateDemurrage_Delivery()
		{
			var leg = Factory.New<CommonCartageLeg>();
			var now = ZDateTime.Now;
			TransportRegistry.Instance.AutoPopulateDemurrage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			leg.JU_DeliverTimeIn = now;
			leg.JU_DeliverTimeOut = now.AddHours(2).AddMinutes(20);
			AssertEquals(ZDateTime.Empty, leg.JU_CartageDeliveryDemurrage);
			TransportRegistry.Instance.AutoPopulateDemurrage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			leg.JU_DeliverTimeIn = now;
			leg.JU_DeliverTimeOut = now.AddHours(2).AddMinutes(30);
			AssertEquals(2, leg.JU_CartageDeliveryDemurrage.Hour);
			AssertEquals(30, leg.JU_CartageDeliveryDemurrage.Minute);
			leg.JU_DeliverTimeIn = ZDateTime.Invalid;
			AssertEquals("Keep existing", 2, leg.JU_CartageDeliveryDemurrage.Hour);
			AssertEquals("Keep existing", 30, leg.JU_CartageDeliveryDemurrage.Minute);
			leg.JU_DeliverTimeIn = now;
			leg.JU_DeliverTimeOut = ZDateTime.Invalid;
			AssertEquals("Keep existing", 2, leg.JU_CartageDeliveryDemurrage.Hour);
			AssertEquals("Keep existing", 30, leg.JU_CartageDeliveryDemurrage.Minute);
		}

		public void TestReloadIfNoChanges()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonCartageLeg legInNewFactory = newFactory.Load<CommonCartageLeg>(leg.PK);
			leg.JU_EY_RunSheet = workSheet.PK;
			legInNewFactory.JU_EY_RunSheet = workSheet.PK;
			Assert(leg.HasChanges);
			Assert(legInNewFactory.HasChanges);
			newFactory.Save();
			Assert("Should have reloaded as there were no other changes", !leg.HasChanges);
			Assert(!legInNewFactory.HasChanges);
			CommonWorkSheet workSheet2 = newFactory.New<CommonWorkSheet>();
			newFactory.Save();
			leg.JU_EY_RunSheet = workSheet2.PK;
			legInNewFactory.JU_EY_RunSheet = workSheet2.PK;
			leg.JU_PlannedPickupTime = ZDateTime.Now;
			newFactory.Save();
			Assert("Also loses it changes :(", !leg.HasChanges);
			Assert(!legInNewFactory.HasChanges);
		}

		public void TestDeleteDeletesGPSActivities()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.GPSActivities.AddNew();
			leg.GPSActivities.AddNew();
			leg.Delete();
			AssertEquals("should delete gps collection when deletin the leg", 0, leg.GPSActivities.Count);
		}

		public void TestDeletedCartageMustReturnNull()
		{
			var leg = Factory.New<CommonCartageLeg>();
			//leg.GPSActivities.AddNew();
			//leg.GPSActivities.AddNew();
			leg.Delete();
			AssertEquals("Should set true after deleting leg", true, leg.IsDeleted);
			AssertEquals("A Cartage should return null after deleting leg", null, leg.Cartage);
			AssertEquals("BookedCartage Move Should return null after deleting leg", null, leg.BookedCtgMove);
		}

		public void TestSettingQuickDriverSetsPrefferedTruck()
		{
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			RefEquipment prefferedTruck = Factory.New<RefEquipment>();
			prefferedTruck.RQ_IsVehicle = true;
			prefferedTruck.RQ_Registration = "abc";
			prefferedTruck.RQ_ShortCode = "abc";
			prefferedTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move1.CartageLegs.AddNew();
			Factory.Save();
			leg1.QuickGSDriver = bob.GS_Code;
			AssertEquals(prefferedTruck.PK, leg1.QuickRQTruck);
		}

		public void TestSettingQuickTruckSetsPrefferedDriver()
		{
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			RefEquipment prefferedTruck = Factory.New<RefEquipment>();
			prefferedTruck.RQ_IsVehicle = true;
			prefferedTruck.RQ_Registration = "abc";
			prefferedTruck.RQ_ShortCode = "abc";
			prefferedTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move1.CartageLegs.AddNew();
			Factory.Save();
			leg1.QuickRQTruck = prefferedTruck.PK;
			AssertEquals(bob.GS_Code, leg1.QuickGSDriver);
		}

		public void TestSettingJU_EY_RunSheet_ShouldValidateLeg()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move1.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = runSheet.EY_StartTime.AddHours(5);
			leg2.JU_EstimatedDeliveryTime = runSheet.EY_StartTime.AddHours(6);
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			runSheet.RunPreSaveValidation();
			Assert(leg1.JU_PlannedPickupTimeInfo.HasErrors());
			Assert(leg1.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert(!leg2.JU_PlannedPickupTimeInfo.HasErrors());
			Assert(!leg2.JU_EstimatedDeliveryTimeInfo.HasErrors());
		}

		public void TestUniqueID()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonCartageLeg cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			AssertNotNull(cartage.BookedMovesCollection);
			Assert(cartage.BookedMovesCollection.Count > 0);
			AssertEquals("Should be empty", ZString.Empty, cartageLeg.JU_SplitDeliverySuffix);
			AssertEquals("Should be empty", ZString.Empty, cartageLeg.UniqueID);
			cartage.JJ_ConsignmentID = "cartageid";
			Factory.Save();
			AssertEquals("Should NOT be empty", "A", cartageLeg.JU_SplitDeliverySuffix);
			AssertEquals("Should NOT be empty", "A", cartageLeg.UniqueID);
			cartageLeg = cartage.BookedMovesCollection[0].CartageLegs.AddNew();
			AssertEquals("Should be empty", ZString.Empty, cartageLeg.JU_SplitDeliverySuffix);
			AssertEquals("Should be empty", ZString.Empty, cartageLeg.UniqueID);
			Factory.Save();
			AssertEquals("Should NOT be empty", "B", cartageLeg.JU_SplitDeliverySuffix);
			AssertEquals("Should NOT be empty", "B", cartageLeg.UniqueID);
		}

		public void TestCodePropertyAttribute()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.LooseBookedMoves.DeleteAll();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			cartage.JJ_ConsignmentID = "T12345678";
			Factory.Save();
			AssertEquals("T12345678/A", leg.UniqueIDWithJobNumber);
			AssertEquals(leg.UniqueIDWithJobNumber, CodePropertyAttribute.CodeFromBusinessObject(leg));
		}

		public void TestDescriptionPropertyAttribute()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.LooseBookedMoves.DeleteAll();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			cartage.JJ_ConsignmentID = "T12345678";
			Factory.Save();
			AssertEquals("T12345678/A", leg.UniqueIDWithJobNumber);
			AssertEquals(leg.UniqueIDWithJobNumber, DescriptionPropertyAttribute.DescriptionFromBusinessObject(leg));
		}

		public void TestUniqueIDOverMax()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			leg1.JU_SplitDeliverySuffix = CommonUtils.GetLetterRepresentation(CommonUtils.MaxNumberRepresentation - 1);
			var leg2 = move.CartageLegs.AddNew();
			AssertExceptionThrown(typeof(ZCannotSaveException), () =>
			{
				Factory.Save();
			});
			AssertExceptionThrown(typeof(ZCannotSaveException), () =>
			{
				leg2.SetUniqueID();
			});
		}

		public void TestPickupAddressCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "Org";
			OrgAddress orgAddress = org.MainAddress;
			orgAddress.OA_Code = "orgAddress";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.FirstDocAddress.E2_OA_Address = orgAddress.PK;
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			AssertEquals("ORG - ORGADDRESS", leg.PickupAddressCode);
			leg.JU_E2PickupAddressID = ZGuid.Empty;
			AssertEquals("", leg.PickupAddressCode);
		}

		public void TestDeliveryAddressCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "Org";
			OrgAddress orgAddress = org.MainAddress;
			orgAddress.OA_Code = "orgAddress";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.FirstDocAddress.E2_OA_Address = orgAddress.PK;
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			leg.JU_E2DeliveryAddressID = cartage.FirstDocAddress.PK;
			AssertEquals("ORG - ORGADDRESS", leg.DeliveryAddressCode);
			leg.JU_E2DeliveryAddressID = ZGuid.Empty;
			AssertEquals("", leg.DeliveryAddressCode);
		}

		public void TestWaitPointAddressCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "WPOrg";
			var address = org.MainAddress;
			address.OA_Code = "WPAddress";
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD;
			cartage.SecondDocAddress.E2_OA_Address = address.PK;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			leg.JU_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			AssertEquals("WPORG - WPADDRESS", leg.WaitPointAddressCode);
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			AssertEquals("", leg.WaitPointAddressCode);
		}

		public void TestPickupTimeSummary()
		{
			var year = ZDateTime.Now.Year;
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals("@", leg.PickupTimeSummary);
			leg.JU_PlannedPickupTime = new ZDateTime(year, 4, 1, 2, 33, 0);
			AssertEquals("@ 01 Apr 02:33", leg.PickupTimeSummary);
			leg.JU_PickupTimeIn = new ZDateTime(year, 4, 1, 3, 21, 0);
			AssertEquals("@ 01 Apr 03:21 -", leg.PickupTimeSummary);
			leg.JU_PickupTimeOut = new ZDateTime(year, 4, 1, 4, 45, 0);
			AssertEquals("@ 01 Apr 03:21 - 01 Apr 04:45", leg.PickupTimeSummary);
		}

		public void TestWaitPointTimeSummary()
		{
			var year = ZDateTime.Now.Year;
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals("@", leg.WaitPointTimeSummary);
			leg.JU_EstimatedDeliveryTime = new ZDateTime(year, 2, 4, 13, 20, 0);
			AssertEquals("@ 04 Feb 13:20", leg.WaitPointTimeSummary);
			leg.JU_WaitPointTimeIn = new ZDateTime(year, 2, 4, 13, 55, 0);
			AssertEquals("@ 04 Feb 13:55 -", leg.WaitPointTimeSummary);
			leg.JU_WaitPointTimeOut = new ZDateTime(year, 2, 4, 14, 2, 0);
			AssertEquals("@ 04 Feb 13:55 - 04 Feb 14:02", leg.WaitPointTimeSummary);
		}

		public void TestDeliveryTimeSummary()
		{
			var year = ZDateTime.Now.Year;
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals("@", leg.PickupTimeSummary);
			leg.JU_EstimatedDeliveryTime = new ZDateTime(year, 4, 1, 2, 33, 0);
			AssertEquals("@ 01 Apr 02:33", leg.DeliveryTimeSummary);
			leg.JU_DeliverTimeIn = new ZDateTime(year, 4, 1, 3, 21, 0);
			AssertEquals("@ 01 Apr 03:21 -", leg.DeliveryTimeSummary);
			leg.JU_DeliverTimeOut = new ZDateTime(year, 4, 1, 4, 45, 0);
			AssertEquals("@ 01 Apr 03:21 - 01 Apr 04:45", leg.DeliveryTimeSummary);
		}

		public void TestSequenceAndActive()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertNull(runSheet.PrimaryActiveLeg);
			CommonCartageLeg leg1 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg2 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg3 = runSheet.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			AssertNull(runSheet.PrimaryActiveLeg);
			AssertEquals("1", leg1.SequenceAndActive);
			AssertEquals("2", leg2.SequenceAndActive);
			AssertEquals("3", leg3.SequenceAndActive);
			leg1.JU_PickupTimeIn = ZDateTime.Now;
			AssertEquals(leg1.PK, runSheet.PrimaryActiveLeg.PK);
			AssertEquals("♦ 1", leg1.SequenceAndActive);
			AssertEquals("2", leg2.SequenceAndActive);
			AssertEquals("3", leg3.SequenceAndActive);
			leg2.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(leg2.PK, runSheet.PrimaryActiveLeg.PK);
			AssertEquals("1", leg1.SequenceAndActive);
			AssertEquals("♦ 2", leg2.SequenceAndActive);
			AssertEquals("3", leg3.SequenceAndActive);
			leg3.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals(leg3.PK, runSheet.PrimaryActiveLeg.PK);
			AssertEquals("1", leg1.SequenceAndActive);
			AssertEquals("2", leg2.SequenceAndActive);
			AssertEquals("♦ 3", leg3.SequenceAndActive);
		}

		[TestDate(2010, 2, 3, 15, 37, 0)]
		public void TestSettingTruckFindsTheBestMatchWorkSheet()
		{
			CommonWorkSheet sheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet sheet2 = Factory.New<CommonWorkSheet>();
			RefEquipment truck = Factory.New<RefEquipment>();
			sheet1.EY_RQ_Truck = truck.PK;
			sheet2.EY_RQ_Truck = truck.PK;
			ZDateTime now = ZDateTime.Now;
			sheet1.EY_StartTime = now;
			sheet1.EY_EndTime = now.AddHours(2);
			sheet2.EY_StartTime = now.AddHours(3);
			sheet2.EY_EndTime = now.AddHours(5);
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = now.AddHours(4);
			leg.QuickRQTruck = truck.PK;
			AssertCollectionNotContains(leg, sheet1.CartageLegs);
			AssertCollectionContains(leg, sheet2.CartageLegs);
		}

		[TestDate(2010, 2, 3, 15, 37, 0)]
		public void TestSettingTruckCreatesNewRunSheetIfBestMatchIsNotFound()
		{
			CommonWorkSheet sheet1 = Factory.New<CommonWorkSheet>();
			CommonWorkSheet sheet2 = Factory.New<CommonWorkSheet>();
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_ShortCode = "HELLO";
			sheet1.EY_RQ_Truck = truck.PK;
			sheet2.EY_RQ_Truck = truck.PK;
			ZDateTime now = ZDateTime.Now;
			sheet1.EY_StartTime = now;
			sheet1.EY_EndTime = now.AddHours(2);
			sheet2.EY_StartTime = now.AddHours(3);
			sheet2.EY_EndTime = now.AddHours(5);
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = now.AddHours(7);
			leg.QuickRQTruck = truck.PK;
			AssertCollectionNotContains(leg, sheet1.CartageLegs);
			AssertCollectionNotContains(leg, sheet2.CartageLegs);
			AssertNull(leg.WorkSheet);
			Factory.Save();
			AssertNotNull(leg.WorkSheet);
			AssertNotEquals(sheet1, leg.WorkSheet);
			AssertNotEquals(sheet2, leg.WorkSheet);
		}

		public void TestWhatSummary_Containerised()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CommonBookedCtgMove move = cartage.ContainerBookedMoves.AddNew();
			CommonContainer container = move.Container;
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			AssertEquals("", leg.WhatSummary);
			cartage.JJ_ConsignmentID = "S12345678/I";
			Factory.Save();
			container.JC_ContainerNum = "CONT111222";
			AssertEquals("CONT111222  S12345678/I/A", leg.WhatSummary);
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("CONT111222  20GP  S12345678/I/A", leg.WhatSummary);
			move.EW_DropMode = Core.Constants.FCLEquipmentNeeded.SideLoader;
			AssertEquals("CONT111222  20GP  SDL  S12345678/I/A", leg.WhatSummary);
			var trailer = Factory.New<RefEquipment>();
			trailer.RQ_ShortCode = "TRL1";
			leg.JU_RQ_Trailer = trailer.PK;
			AssertEquals("CONT111222  20GP  SDL  S12345678/I/A  TRL1", leg.WhatSummary);
			AssertEquals("Leg for CONT111222  20GP  SDL  S12345678/I/A  TRL1", leg.HumanReadableName);
		}

		public void TestWhatSummary_Loose()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.LooseBookedMoves.DeleteAll();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			move.EW_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			AssertEquals("0 PLT", leg.WhatSummary);
			cartage.JJ_ConsignmentID = "S12345678/I";
			Factory.Save();
			move.EW_BookedPackCount = 10;
			AssertEquals("10 PLT  S12345678/I/A", leg.WhatSummary);
			move.EW_BookedWeight = 10;
			AssertEquals("10 PLT  10 KG  S12345678/I/A", leg.WhatSummary);
			move.EW_BookedWeight = 1234.123;
			AssertEquals("10 PLT  1,234 KG  S12345678/I/A", leg.WhatSummary);
			move.EW_BookedVolume = 1;
			AssertEquals("10 PLT  1.0 M3  1,234 KG  S12345678/I/A", leg.WhatSummary);
			move.EW_BookedVolume = 10;
			AssertEquals("10 PLT  10.0 M3  1,234 KG  S12345678/I/A", leg.WhatSummary);
			move.EW_BookedVolume = 1234.123;
			AssertEquals("10 PLT  1,234.1 M3  1,234 KG  S12345678/I/A", leg.WhatSummary);
			move.EW_DropMode = Core.Constants.LCLAIREquipmentNeeded.HandHaulier;
			AssertEquals("10 PLT  1,234.1 M3  1,234 KG  HWL  S12345678/I/A", leg.WhatSummary);
			AssertEquals("Leg for 10 PLT  1,234.1 M3  1,234 KG  HWL  S12345678/I/A", leg.HumanReadableName);
		}

		public void TestJU_MessageStatus()
		{
			var cartageLeg = Factory.New<CommonCartageLeg>();
			AssertEquals("", cartageLeg.JU_MessageStatus);
			cartageLeg.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(-30);
			AssertEquals("", cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
			cartageLeg.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(-20);
			AssertEquals(Constants.CartageLegDispatchStatusList.Codes.PickedUp, cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
			cartageLeg.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(-10);
			AssertEquals(Constants.CartageLegDispatchStatusList.Codes.PickedUp, cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
			cartageLeg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(Constants.CartageLegDispatchStatusList.Codes.Delivered, cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
			cartageLeg.JU_DeliverTimeOut = ZDateTime.Empty;
			AssertEquals(Constants.CartageLegDispatchStatusList.Codes.PickedUp, cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
			cartageLeg.JU_DeliverTimeIn = ZDateTime.Empty;
			AssertEquals(Constants.CartageLegDispatchStatusList.Codes.PickedUp, cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
			cartageLeg.JU_PickupTimeOut = ZDateTime.Empty;
			AssertEquals("", cartageLeg.JU_MessageStatus);
			AssertNoErrors(cartageLeg.JU_MessageStatusInfo);
		}

		public void TestErrorStatusDescription()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals("", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Runsheet;
			AssertEquals("", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals("Rejected", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Futile;
			AssertEquals("Futile", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = "";
			AssertEquals("", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.PickedUp;
			AssertEquals("", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.PickingUp;
			AssertEquals("", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Delivered;
			AssertEquals("", leg.ErrorStatusDescription);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Delivering;
			AssertEquals("", leg.ErrorStatusDescription);
		}

		public void TestLegStatus()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(CommonCartageLeg.LegStatuses.None, leg.LegStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Runsheet;
			AssertEquals(CommonCartageLeg.LegStatuses.Dispatched, leg.LegStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals(CommonCartageLeg.LegStatuses.Error, leg.LegStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Futile;
			AssertEquals(CommonCartageLeg.LegStatuses.Error, leg.LegStatus);
			leg.JU_MessageStatus = "";
			AssertEquals(CommonCartageLeg.LegStatuses.None, leg.LegStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.PickedUp;
			AssertEquals(CommonCartageLeg.LegStatuses.Dispatched, leg.LegStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Delivered;
			AssertEquals(CommonCartageLeg.LegStatuses.Dispatched, leg.LegStatus);
		}

		public void TestPickupStatus()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.None), leg.PickupStatus);
			leg.JU_PickupTimeIn = ZDateTime.Now;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.TimeIn), leg.PickupStatus);
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.TimeOut), leg.PickupStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.Error), leg.PickupStatus);
		}

		public void TestDeliveryStatus()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.None), leg.DeliveryStatus);
			leg.JU_DeliverTimeIn = ZDateTime.Now;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.TimeIn), leg.DeliveryStatus);
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.TimeOut), leg.DeliveryStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.Error), leg.DeliveryStatus);
		}

		public void TestWaitPointStatus()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.None), leg.WaitPointStatus);
			leg.JU_WaitPointTimeIn = ZDateTime.Now;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.TimeIn), leg.WaitPointStatus);
			leg.JU_WaitPointTimeIn = ZDateTime.Empty;
			leg.JU_WaitPointTimeOut = ZDateTime.Now;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.TimeOut), leg.WaitPointStatus);
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals(nameof(CommonCartageLeg.LegStatuses.Error), leg.WaitPointStatus);
		}

		public void TestHasWaitPoint()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			AssertEquals(true, leg.JU_E2WaitPointAddressID.IsEmpty);
			AssertEquals(false, leg.HasWaitPoint);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "CNE Address", "2000", "Sydney", "AUSYD", true);
			leg.JU_E2WaitPointAddressID = cne.PK;
			AssertEquals(true, leg.HasWaitPoint);
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			AssertEquals(false, leg.HasWaitPoint);
		}

		public void TestIsComplete()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			Assert(!leg.IsComplete);
			leg.JU_PickupTimeIn = ZDateTime.Now;
			Assert(!leg.IsComplete);
			leg.JU_PickupTimeOut = ZDateTime.Now;
			Assert(!leg.IsComplete);
			leg.JU_DeliverTimeIn = ZDateTime.Now;
			Assert(!leg.IsComplete);
			leg.JU_DeliverTimeOut = ZDateTime.Now;
			Assert(leg.IsComplete);
		}

		public void TestIsPartComplete()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			Assert(!leg.IsComplete);
			leg.JU_PickupTimeIn = ZDateTime.Now;
			Assert(leg.IsPartComplete);
			leg.JU_PickupTimeOut = ZDateTime.Now;
			Assert(leg.IsPartComplete);
			leg.JU_DeliverTimeIn = ZDateTime.Now;
			Assert(leg.IsPartComplete);
			leg.JU_DeliverTimeOut = ZDateTime.Now;
			Assert(leg.IsPartComplete);
		}

		public void TestIsFirstLeg()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var cartageLeg1 = cartage.ContainerBookedMoves.AddNew().CartageLegs[0];
			var cartageLeg2 = cartage.ContainerBookedMoves.AddNew().CartageLegs[1];
			AssertEquals("Should be true for first leg on cartage", true, cartageLeg1.IsFirstLeg);
			AssertEquals("Should not be first leg on cartage", false, cartageLeg2.IsFirstLeg);
		}

		public void TestIsFirstLeg_NoJobType()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			var move = cartage.ContainerBookedMoves.AddNew();
			var cartageLeg1 = move.CartageLegs[0];
			var cartageLeg2 = move.CartageLegs.AddNew();
			cartageLeg1.JU_IsEmptyContainer = true;
			AssertEquals("Looking for the first Leg, should be it", true, cartageLeg1.IsFirstLeg);
			AssertEquals("Should be false for second leg", false, cartageLeg2.IsFirstLeg);
			cartageLeg1.Delete();
			AssertEquals("Should be true for only leg on cartage even if empty", true, cartageLeg2.IsFirstLeg);
		}

		public void TestIsFirstLeg_NoJobTypeButMixed()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			var cartageLegMT = containerMove.CartageLegs[0];
			var cartageLegFull = containerMove.CartageLegs.AddNew();
			cartageLegMT.JU_IsEmptyContainer = true;
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			var looseLeg11 = looseMove1.CartageLegs[0];
			var looseLeg12 = looseMove1.CartageLegs.AddNew();
			var looseLeg21 = looseMove2.CartageLegs[0];
			var looseLeg22 = looseMove2.CartageLegs.AddNew();
			AssertEquals(false, cartageLegMT.IsFirstFullLeg);
			AssertEquals(false, cartageLegFull.IsFirstFullLeg);
			AssertEquals(true, looseLeg11.IsFirstFullLeg);
			AssertEquals(false, looseLeg12.IsFirstFullLeg);
			AssertEquals(false, looseLeg21.IsFirstFullLeg);
			AssertEquals(false, looseLeg22.IsFirstFullLeg);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartageLegMT.IsFirstFullLeg);
			AssertEquals(true, cartageLegFull.IsFirstFullLeg);
			AssertEquals(false, looseLeg11.IsFirstFullLeg);
			AssertEquals(false, looseLeg12.IsFirstFullLeg);
			AssertEquals(false, looseLeg21.IsFirstFullLeg);
			AssertEquals(false, looseLeg22.IsFirstFullLeg);
		}

		public void TestIsFirstFullLeg()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var cartageLegMT = cartage.GetBookedMoves(container)[0].CartageLegs[0];
			var cartageLegFull = cartage.GetBookedMoves(container)[0].CartageLegs[1];
			AssertEquals("Should not be true for empty leg", false, cartageLegMT.IsFirstFullLeg);
			AssertEquals("Should be true for first full leg on cartage", true, cartageLegFull.IsFirstFullLeg);
			cartage.Containers.DeleteAll();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCNEtoCYD;
			var onlyEmptyLeg = cartage.ContainerBookedMoves.AddNew().CartageLegs[0];
			AssertEquals("Should be true for only leg on cartage even if empty", true, onlyEmptyLeg.IsFirstFullLeg);
		}

		public void TestIsFirstFullLeg_NoJobType()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			var move = cartage.ContainerBookedMoves.AddNew();
			var cartageLeg1 = move.CartageLegs[0];
			var cartageLeg2 = move.CartageLegs.AddNew();
			cartageLeg1.JU_IsEmptyContainer = true;
			AssertEquals("Should not be true for empty leg", false, cartageLeg1.IsFirstFullLeg);
			AssertEquals("Should be true for first full leg on cartage", true, cartageLeg2.IsFirstFullLeg);
			cartageLeg2.Delete();
			AssertEquals("Should be true for only leg on cartage even if empty", true, cartageLeg1.IsFirstFullLeg);
		}

		public void TestIsFirstFullLeg_NoJobTypeButMixed()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			var cartageLegMT = containerMove.CartageLegs[0];
			var cartageLegFull = containerMove.CartageLegs.AddNew();
			cartageLegMT.JU_IsEmptyContainer = true;
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			var looseLeg11 = looseMove1.CartageLegs[0];
			var looseLeg12 = looseMove1.CartageLegs.AddNew();
			var looseLeg21 = looseMove2.CartageLegs[0];
			var looseLeg22 = looseMove2.CartageLegs.AddNew();
			AssertEquals(false, cartageLegMT.IsFirstFullLeg);
			AssertEquals(false, cartageLegFull.IsFirstFullLeg);
			AssertEquals(true, looseLeg11.IsFirstFullLeg);
			AssertEquals(false, looseLeg12.IsFirstFullLeg);
			AssertEquals(false, looseLeg21.IsFirstFullLeg);
			AssertEquals(false, looseLeg22.IsFirstFullLeg);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartageLegMT.IsFirstFullLeg);
			AssertEquals(true, cartageLegFull.IsFirstFullLeg);
			AssertEquals(false, looseLeg11.IsFirstFullLeg);
			AssertEquals(false, looseLeg12.IsFirstFullLeg);
			AssertEquals(false, looseLeg21.IsFirstFullLeg);
			AssertEquals(false, looseLeg22.IsFirstFullLeg);
			cartageLegFull.Delete();
			AssertEquals(true, cartageLegMT.IsFirstFullLeg);
		}

		public void TestUpdateContainerEmptyReturned()
		{
			var now = ZDateTime.Now;
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			var container = cartage.Containers.First();
			var lastLeg = cartage.GetBookedMoves(container)[0].LastCartageLeg;
			lastLeg.JU_DeliverTimeOut = now.AddMinutes(-10);
			AssertEquals(now.AddMinutes(-10), container.JC_ContainerYardEmptyReturnGateIn);
			lastLeg.JU_DeliverTimeIn = now.AddMinutes(-20);
			AssertEquals(now.AddMinutes(-20), container.JC_ContainerYardEmptyReturnGateIn);
			container.JC_ContainerYardEmptyReturnGateIn = now.AddMinutes(-40);
			lastLeg.JU_DeliverTimeIn = now.AddMinutes(-30);
			AssertEquals(now.AddMinutes(-40), container.JC_ContainerYardEmptyReturnGateIn);
			lastLeg.JU_DeliverTimeIn = now.AddMinutes(-40);
			lastLeg.JU_DeliverTimeIn = ZDateTime.Empty;
			AssertEquals(now.AddMinutes(-10), container.JC_ContainerYardEmptyReturnGateIn);
			lastLeg.JU_DeliverTimeOut = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, container.JC_ContainerYardEmptyReturnGateIn);
			var firstLeg = cartage.GetBookedMoves(container)[0].FirstCartageLeg;
			firstLeg.JU_DeliverTimeIn = now.AddMinutes(-40);
			AssertEquals("Should still be empty. Container Yard Return Empty only updates when the address is the Container Yard.", ZDateTime.Empty, container.JC_ContainerYardEmptyReturnGateIn);
		}

		public void TestLegsStartTime()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(ZDateTime.Empty, leg.StartTime);
			ZDateTime now = ZDateTime.Now;
			CommonWorkSheet runsheet = Factory.New<CommonWorkSheet>();
			runsheet.EY_StartTime = now;
			runsheet.CartageLegs.Add(leg);
			AssertEquals(runsheet.EY_StartTime, leg.StartTime);
			leg.JU_PlannedPickupTime = now.AddMinutes(10);
			AssertEquals(leg.JU_PlannedPickupTime, leg.StartTime);
			leg.JU_PickupTimeIn = now.AddMinutes(20);
			AssertEquals(leg.JU_PickupTimeIn, leg.StartTime);
			leg.JU_PickupTimeOut = now.AddMinutes(40);
			AssertEquals(leg.JU_PickupTimeOut, leg.StartTime);
		}

		public void TestLegsEndTime()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertEquals(ZDateTime.Empty, leg.EndTime);
			ZDateTime now = ZDateTime.Now;
			CommonWorkSheet runsheet = Factory.New<CommonWorkSheet>();
			runsheet.EY_EndTime = now;
			runsheet.CartageLegs.Add(leg);
			AssertEquals(runsheet.EY_EndTime, leg.EndTime);
			leg.JU_EstimatedDeliveryTime = now.AddMinutes(10);
			AssertEquals(leg.JU_EstimatedDeliveryTime, leg.EndTime);
			leg.JU_DeliverTimeIn = now.AddMinutes(20);
			AssertEquals(leg.JU_DeliverTimeIn, leg.EndTime);
			leg.JU_DeliverTimeOut = now.AddMinutes(40);
			AssertEquals(leg.JU_DeliverTimeOut, leg.EndTime);
		}

		public void TestUniqueIDWithJobNumber()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.LooseBookedMoves.DeleteAll();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			move.EW_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			cartage.JJ_ConsignmentID = "S12345678/I";
			Factory.Save();
			AssertEquals("S12345678/I/A", leg.UniqueIDWithJobNumber);
			cartage.JJ_ConsignmentID = "T12345678";
			AssertEquals("T12345678/A", leg.UniqueIDWithJobNumber);
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			Factory.Save();
			AssertEquals("T12345678/A", leg.UniqueIDWithJobNumber);
			AssertEquals("T12345678/B", leg2.UniqueIDWithJobNumber);
		}

		public void TestJU_AdditionalService_Futile()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.LooseBookedMoves.DeleteAll();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg = move.CartageLegs.AddNew();
			AssertEquals("default empty", "", leg.JU_MessageStatus);
			leg.JU_AdditionalService = Constants.CartageAdditional.AdditionalService;
			AssertEquals("should stay empty", "", leg.JU_MessageStatus);
			leg.JU_AdditionalService = Constants.CartageAdditional.Futile;
			AssertEquals("Should be set to futile too ... will affect colours", Constants.CartageLegDispatchStatusList.Codes.Futile, leg.JU_MessageStatus);
		}

		public void TestQuickGSDriverInfo_HumanReadableName()
		{
			AssertEquals("Driver", Factory.New<CommonCartageLeg>().QuickGSDriverInfo.HumanReadableName);
		}

		public void TestQuickRQTruckInfo_HumanReadableName()
		{
			AssertEquals("Vehicle", Factory.New<CommonCartageLeg>().QuickRQTruckInfo.HumanReadableName);
		}

		public void TestQuickOHTransportCompanyInfo_HumanReadableName()
		{
			AssertEquals("Transport Company", Factory.New<CommonCartageLeg>().QuickOHTransportCompanyInfo.HumanReadableName);
		}

		public void TestQuickPlannedPickupTimeInfo_HumanReadableName()
		{
			AssertEquals("Planned Pickup", Factory.New<CommonCartageLeg>().QuickPlannedPickupTimeInfo.HumanReadableName);
		}

		public void TestQuickEstimatedDeliveryTimeInfo_HumanReadableName()
		{
			AssertEquals("Planned Delivery", Factory.New<CommonCartageLeg>().QuickEstimatedDeliveryTimeInfo.HumanReadableName);
		}

		public void TestRegisteredEditable()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			Assert(!cartage.IsRoot);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLSHPtoCTO;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.CartageLegs.DeleteAll();
			var leg = move.CartageLegs.AddNew();
			Assert(leg.IsRegisteredEditableChildObject(leg.BookedCtgMove));
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonCartage cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			CommonCartageLeg leg_NewFactory = newFactory.Load<CommonCartageLeg>(leg.PK);
			Assert(!cartage_NewFactory.IsRoot);
			Assert(leg_NewFactory.IsRegisteredEditableChildObject(leg_NewFactory.BookedCtgMove));
			newFactory = new BusinessObjectFactory();
			cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			leg_NewFactory = newFactory.Load<CommonCartageLeg>(leg.PK);
			cartage_NewFactory.IsRoot = true;
			Assert(cartage_NewFactory.IsRoot);
			Assert(!leg_NewFactory.IsRegisteredEditableChildObject(leg_NewFactory.BookedCtgMove));
		}

		public void TestTotalWeight()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLSHPtoCTO;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.CartageLegs.DeleteAll();
			var leg = move.CartageLegs.AddNew();
			container.JC_TareWeight = 1000m;
			container.JC_GrossWeight = 3000m;
			AssertEquals(3000m, leg.TotalWeight);
			leg.JU_IsEmptyContainer = true;
			AssertEquals(1000m, leg.TotalWeight);
		}

		public void TestJU_IsEmptyContainer()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "CTO Address", "2000", "Sydney", "AUSYD", true);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "CNE Address", "2000", "Sydney", "AUSYD", true);
			var cyd = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageYard, "CYD", "CYD Address", "2000", "Sydney", "AUSYD", true);
			leg.JU_E2PickupAddressID = ZGuid.Empty;
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			leg.JU_E2DeliveryAddressID = ZGuid.Empty;
			AssertEquals("No addresses set", false, leg.JU_IsEmptyContainer);
			leg.JU_E2PickupAddressID = cto.PK;
			leg.JU_E2DeliveryAddressID = cne.PK;
			AssertEquals("No CYD", false, leg.JU_IsEmptyContainer);
			leg.JU_E2PickupAddressID = cne.PK;
			leg.JU_E2DeliveryAddressID = cyd.PK;
			AssertEquals("Delivering to CYD, so should be empty by default", true, leg.JU_IsEmptyContainer);
			leg.JU_E2PickupAddressID = cto.PK;
			leg.JU_E2WaitPointAddressID = cne.PK;
			leg.JU_E2DeliveryAddressID = cyd.PK;
			AssertEquals("Delivering to CYD, but has a waitpoint, so should NOT be empty by default", false, leg.JU_IsEmptyContainer);
			leg.JU_E2PickupAddressID = cyd.PK;
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			leg.JU_E2DeliveryAddressID = cne.PK;
			AssertEquals("Picking up from CYD, so should be empty by default", true, leg.JU_IsEmptyContainer);
			leg.JU_E2PickupAddressID = cne.PK;
			leg.JU_E2DeliveryAddressID = cto.PK;
			AssertEquals("No CYD", false, leg.JU_IsEmptyContainer);
			leg.JU_E2PickupAddressID = cyd.PK;
			leg.JU_E2WaitPointAddressID = cne.PK;
			leg.JU_E2DeliveryAddressID = cto.PK;
			AssertEquals("Picking up from CYD, but has a waitpoint, so should NOT be empty by default", false, leg.JU_IsEmptyContainer);
		}

		public void TestJU_MessageStatus_EventLogsLegAcceptanceStatus()
		{
			AssertEventLogsLegAcceptanceStatus(true, Constants.CartageLegDispatchStatusList.Codes.WIP, Events.MessageAcceptedCode, "Accepted", "Driver");
			AssertEventLogsLegAcceptanceStatus(true, Constants.CartageLegDispatchStatusList.Codes.Rejected, Events.MessageRejectedCode, "Rejected", "Driver");
			AssertEventLogsLegAcceptanceStatus(true, Constants.CartageLegDispatchStatusList.Codes.Futile, Events.MessageWithdrawCancelAcceptedCode, "Futile", "Driver");
			AssertEventLogsLegAcceptanceStatus(false, Constants.CartageLegDispatchStatusList.Codes.WIP, Events.MessageAcceptedCode, "Accepted");
			AssertEventLogsLegAcceptanceStatus(false, Constants.CartageLegDispatchStatusList.Codes.Rejected, Events.MessageRejectedCode, "Rejected");
			AssertEventLogsLegAcceptanceStatus(false, Constants.CartageLegDispatchStatusList.Codes.Futile, Events.MessageWithdrawCancelAcceptedCode, "Futile");
		}

		void AssertEventLogsLegAcceptanceStatus(bool isUpdatedViaWebService, string messageStatus, string expectedLogType, string expectedRes, string expectedDep = "")
		{
			var leg = Factory.New<CommonCartageLeg>();
			leg.IsMessageStatusUpdatedViaWebservice = isUpdatedViaWebService;
			leg.JU_MessageStatus = messageStatus;
			AssertEquals(expectedLogType, leg.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals(expectedRes, leg.Logs.MostRecentLog.Parameters["RES"]);
			if (!string.IsNullOrEmpty(expectedDep))
			{
				AssertEquals(expectedDep, leg.Logs.MostRecentLog.Parameters["DEP"]);
			}
		}

		public void TestUpdatingPickupTime_UpdatesEventsOnCorrectTransportBooking()
		{
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			var unrelatedConsolidation = Factory.New<IDtbBookingConsolidation>();
			var unrelatedBooking = Factory.New<IDtbBooking>();
			unrelatedBooking.KM_KB_Booking = unrelatedConsolidation.PK;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_OrderReferenceNumber = unrelatedBooking.KM_JobID;

			var move = cartage.LooseBookedMoves.AddNew();

			var cartageLeg = move.CartageLegs.AddNew();

			Factory.Save();

			cartage.AdditionalReferenceNumbers.AddNewIfNotExist("ETB", unrelatedBooking.KM_JobID);

			AssertEquals("Precondition: Parent booking of transport job should not have any PUP events yet.", 0, FindEvents(booking.PK, AutoEvents.PickedUp).Count());

			cartageLeg[JobContainerLegsSchema.JU_PickupTimeIn] = DateTime.Now;
			cartageLeg[JobContainerLegsSchema.JU_PickupTimeOut] = DateTime.Now.AddHours(1);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Unrelated booking should not have any PUP events.", 0, FindEvents(unrelatedBooking.PK, AutoEvents.PickedUp).Count());
				AssertEquals("Parent booking of transport job should have a PUP event.", 1, FindEvents(booking.PK, AutoEvents.PickedUp).Count());
				AssertEquals("CurrentlyPublishingAnEventToTheOrgProxy property should have been reset to 'false'.", false, cartageLeg.Cartage.CurrentlyPublishingAnEventToTheOrgProxy);
			});
		}

		[TestedType(typeof(CommonCartageLeg))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
			protected override BusinessObject GetBizo()
			{
				var cartage = new LocalCartageTestHelper(Factory).CreateCartage(Constants.CartageJobType.NEW_AirImport, 1);
				return cartage.CartageLegs[0];
			}
		}

		public void TestICustomFieldProvider_GetCustomBusinessObject()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirImport, 1);
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.CartageLegWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var leg = cartage.CartageLegs[0];
			var legProvider = (ICustomFieldProvider)leg;
			var legCustomBizo = legProvider.GetCustomBusinessObject();
			var legDynamicBizo = (IDynamicBusinessObject)legCustomBizo;
			AssertNotNull(legDynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
			AssertNotNull(legDynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
			AssertNotNull(legDynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
			AssertNotNull(legDynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
		}

		public void TestTotalDemurrage_Pickup()
		{
			var year = ZDateTime.Now.Year;
			var dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "Hi";
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			AssertEquals(0, dummyParent.Demurrage.Ticks);
			var move = cartage.ContainerBookedMoves.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			leg1.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 0, 0);
			leg1.JU_CartageWaitPointDemurrage = new ZDateTime(year, 1, 1, 1, 0, 0);
			leg1.JU_CartageDeliveryDemurrage = new ZDateTime(year, 1, 1, 1, 0, 0);
			AssertEquals(0, dummyParent.Demurrage.Ticks);
			Factory.Save();
			AssertEquals(108000000000, dummyParent.Demurrage.Ticks);
		}

		public void TestGetDemurrageIncludingFreeTime()
		{
			var year = ZDateTime.Now.Year;
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = freeWaitingTimeCollection.AddNew();
			freeWaitingTime.DropMode = Constants.EquipmentNeeded.Any;
			freeWaitingTime.CTO = new ZDateTime(2012, 1, 1, 0, 10, 0);
			freeWaitingTime.CNE = new ZDateTime(2012, 1, 1, 0, 20, 0);
			freeWaitingTime.CNR = new ZDateTime(2012, 1, 1, 0, 20, 0);
			freeWaitingTime.CYD = new ZDateTime(2012, 1, 1, 0, 30, 0);
			TransportRegistry.Instance.AmountOfFreeWaitingTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, freeWaitingTimeCollection);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			leg.JU_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			leg.JU_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			leg.JU_PickupTimeIn = new ZDateTime(year, 1, 3, 10, 0, 0);
			leg.JU_PickupTimeOut = new ZDateTime(year, 1, 4, 10, 25, 0);
			leg.JU_WaitPointTimeIn = new ZDateTime(year, 1, 6, 10, 0, 0);
			leg.JU_WaitPointTimeOut = new ZDateTime(year, 1, 7, 11, 0, 0);
			leg.JU_DeliverTimeIn = new ZDateTime(year, 1, 10, 10, 0, 0);
			leg.JU_DeliverTimeOut = new ZDateTime(year, 1, 10, 10, 15, 0);
			AssertEquals("Pickup Demurrage.", "1.00:15:00", leg.GetDemurrageIncludingFreeTime(leg.JU_CartagePickupDemurrage, leg.PickupFromDocAddress).ToString());
			AssertEquals("Wait Point Demurrage.", "1.00:40:00", leg.GetDemurrageIncludingFreeTime(leg.JU_CartageWaitPointDemurrage, leg.WaitPointDocAddress).ToString());
			AssertEquals("Delivery Demurrage.", "00:00:00", leg.GetDemurrageIncludingFreeTime(leg.JU_CartageDeliveryDemurrage, leg.DeliverToDocAddress).ToString());
		}

		public void TestGetTotalDemurrage_NegativeCumulative()
		{
			var year = ZDateTime.Today.Year;
			var dayZero = new ZDateTime(year, 1, 1, 0, 0, 0);
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = freeWaitingTimeCollection.AddNew();
			freeWaitingTime.DropMode = Constants.EquipmentNeeded.Any;
			freeWaitingTime.CTO = dayZero.AddMinutes(20);
			freeWaitingTime.CNE = dayZero.AddMinutes(30);
			freeWaitingTime.CYD = dayZero.AddMinutes(40);
			TransportRegistry.Instance.AmountOfFreeWaitingTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, freeWaitingTimeCollection);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			leg.JU_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			leg.JU_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			TransportRegistry.Instance.UseCumulativeFreeWaitingTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			{
				leg.JU_PickupTimeIn = new ZDateTime(year, 1, 3, 10, 0, 0);
				leg.JU_PickupTimeOut = new ZDateTime(year, 1, 3, 10, 5, 0);
				AssertEquals("Pickup CTO Demurrage.", -15d, leg.GetDemurrageIncludingFreeTime(leg.JU_CartagePickupDemurrage, leg.PickupFromDocAddress).TotalMinutes);
				leg.JU_WaitPointTimeIn = new ZDateTime(year, 1, 6, 10, 0, 0);
				leg.JU_WaitPointTimeOut = new ZDateTime(year, 1, 6, 10, 10, 0);
				AssertEquals("Wait Point CNE Demurrage.", -20d, leg.GetDemurrageIncludingFreeTime(leg.JU_CartageWaitPointDemurrage, leg.WaitPointDocAddress).TotalMinutes);
				leg.JU_DeliverTimeIn = new ZDateTime(year, 1, 10, 10, 0, 0);
				leg.JU_DeliverTimeOut = new ZDateTime(year, 1, 10, 10, 15, 0);
				AssertEquals("Delivery CYD Demurrage.", -25d, leg.GetDemurrageIncludingFreeTime(leg.JU_CartageDeliveryDemurrage, leg.DeliverToDocAddress).TotalMinutes);
				AssertEquals("Although Total cumulative demurrage is negative '-60', Zero must be returned.", 0d, leg.GetTotalDemurrage().TotalMinutes);
			}
		}

		public void TestRaiseWorkSheetAdded()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			// no exeption if no event has been hooked
			leg.RaiseWorkSheetAdded(new WorkSheetLegLinkEventArgs(null));
			// event called if it has been hooked
			bool didWorkSheetAddedCall = false;
			leg.OnWorkSheetAdded += new EventHandler<WorkSheetLegLinkEventArgs>(delegate(object sender, WorkSheetLegLinkEventArgs e)
			{
				didWorkSheetAddedCall = true;
			});
			leg.RaiseWorkSheetAdded(new WorkSheetLegLinkEventArgs(null));
			AssertEquals("Event sould be called", true, didWorkSheetAddedCall);
		}

		public void TestIRelatedJobNumberMembers()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			cartage.JJ_ConsignmentID = "blah";
			var leg = cartage.GetBookedMoves(cartage.Containers.First())[0].CartageLegs.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { "blah" }, ((IRelatedJobNumber)leg).JobNumber);
		}

		public void TestOrganisationsForCreditChecks()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			var client = Helper.CreateOrgHeader("Client", "Addy");
			var consignor = Helper.CreateOrgHeader("Nor", "Addy");
			var consignor2 = Helper.CreateOrgHeader("Nor2", "Addy");
			var consignee = Helper.CreateOrgHeader("Nee", "Addy");
			var cfs = Helper.CreateOrgHeader("cfs", "Addy");
			new JobHeader.Loader(cartage).TryCreate();
			cartage.LocalClientAddressPK = client.MainAddress.PK;
			var consignorDocAddress = cartage.DocAddresses.AddNew(consignor.MainAddress, DocAddressType.LocalCartageExporter);
			var consignor2DocAddress = cartage.DocAddresses.AddNew(consignor2.MainAddress, DocAddressType.LocalCartageExporter);
			var consigneeDocAddress = cartage.DocAddresses.AddNew(consignee.MainAddress, DocAddressType.LocalCartageImporter);
			var cfsDocAddress = cartage.DocAddresses.AddNew(consignee.MainAddress, DocAddressType.LocalCartageCFS);
			var leg = cartage.GetBookedMoves(cartage.Containers.First())[0].CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = consignorDocAddress.PK;
			leg.JU_E2WaitPointAddressID = consignor2DocAddress.PK;
			leg.JU_E2DeliveryAddressID = consigneeDocAddress.PK;
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(4, ((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(consignor));
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(consignor2));
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(consignee));
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(client));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
			Action<string> setupOrganizationsEvaluatedForCreditControlRegistry = organizationType =>
			{
				var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
				var orgCreditControl1 = orgCreditControlCollection.AddNew();
				orgCreditControl1.JobType = cartage.InvoicingSupporter.ConsumerType.Code;
				orgCreditControl1.OrganizationType = organizationType;
				AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
				Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			};
			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.LocalClient);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(3, ((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(consignor));
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(consignor2));
			Assert(((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Contains(consignee));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.All);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(0, ((ICreditControlledDocumentDelivery)leg).OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestIsRunSheetAuthorised_JU_EY_RunSheet()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(false, leg.IsRunSheetAuthorised);
			var runSheet = Factory.New<CommonWorkSheet>();
			var legWithRunSheet = Factory.New<CommonCartageLeg>();
			legWithRunSheet.JU_EY_RunSheet = runSheet.PK;
			AssertEquals(true, legWithRunSheet.IsRunSheetAuthorised);
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var leg_OtherFactory = otherFactory.Load<CommonCartageLeg>(leg.PK);
			AssertEquals("Loaded leg with no runsheet.", false, leg_OtherFactory.IsRunSheetAuthorised);
			var legWithRunSheet_OtherFactory = otherFactory.Load<CommonCartageLeg>(legWithRunSheet.PK);
			AssertEquals("Loaded leg with a runsheet should be authorised", true, legWithRunSheet_OtherFactory.IsRunSheetAuthorised);
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			legWithRunSheet = Factory.New<CommonCartageLeg>();
			legWithRunSheet.JU_EY_RunSheet = runSheet.PK;
			AssertEquals("Provider didn't authorise", false, legWithRunSheet.IsRunSheetAuthorised);
			provider.AuthoriseBy = "Bob";
			legWithRunSheet.JU_EY_RunSheet = ZGuid.Empty;
			AssertEquals("Doesn't call Provider if empty", false, legWithRunSheet.IsRunSheetAuthorised);
			legWithRunSheet.JU_EY_RunSheet = runSheet.PK;
			AssertEquals("Provider Authroised", true, legWithRunSheet.IsRunSheetAuthorised);
			AssertEquals(true, legWithRunSheet.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_Reference == "A non Customs Cleared Leg added to a Run Sheet by Bob."));
		}

		public void TestIsRunSheetAuthorised_QuickGSDriver()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(false, leg.IsRunSheetAuthorised);
			var driver = Factory.New<GlbStaff>();
			driver.GS_Code = "BB";
			leg.QuickGSDriver = "BB";
			AssertEquals(true, leg.IsRunSheetAuthorised);
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			leg = Factory.New<CommonCartageLeg>();
			leg.QuickGSDriver = "BB";
			AssertEquals("Provider didn't authroise", false, leg.IsRunSheetAuthorised);
			provider.AuthoriseBy = "Bob";
			leg.QuickGSDriver = "";
			AssertEquals("Doesn't call Provider if empty", false, leg.IsRunSheetAuthorised);
			leg.QuickGSDriver = "BB";
			AssertEquals("Provider Authroised", true, leg.IsRunSheetAuthorised);
			AssertEquals(true, leg.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_Reference == "A non Customs Cleared Leg added to a Run Sheet by Bob."));
		}

		public void TestIsRunSheetAuthorised_QuickOHTransportCompany()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(false, leg.IsRunSheetAuthorised);
			var transCo = Factory.New<OrgHeader>();
			leg.QuickOHTransportCompany = transCo.PK;
			AssertEquals(true, leg.IsRunSheetAuthorised);
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			leg = Factory.New<CommonCartageLeg>();
			leg.QuickOHTransportCompany = transCo.PK;
			AssertEquals("Provider didn't authroise", false, leg.IsRunSheetAuthorised);
			provider.AuthoriseBy = "Bob";
			leg.QuickOHTransportCompany = ZGuid.Empty;
			AssertEquals("Doesn't call Provider if empty", false, leg.IsRunSheetAuthorised);
			leg.QuickOHTransportCompany = transCo.PK;
			AssertEquals("Provider Authroised", true, leg.IsRunSheetAuthorised);
			AssertEquals(true, leg.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_Reference == "A non Customs Cleared Leg added to a Run Sheet by Bob."));
		}

		public void TestIsRunSheetAuthorised_QuickRQTruck()
		{
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(false, leg.IsRunSheetAuthorised);
			var truck = Factory.New<RefEquipment>();
			truck.RQ_IsVehicle = true;
			leg.QuickRQTruck = truck.PK;
			AssertEquals(true, leg.IsRunSheetAuthorised);
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			leg = Factory.New<CommonCartageLeg>();
			leg.QuickRQTruck = truck.PK;
			AssertEquals("Provider didn't authroise", false, leg.IsRunSheetAuthorised);
			provider.AuthoriseBy = "Bob";
			leg.QuickRQTruck = ZGuid.Empty;
			AssertEquals("Doesn't call Provider if empty", false, leg.IsRunSheetAuthorised);
			leg.QuickRQTruck = truck.PK;
			AssertEquals("Provider Authroised", true, leg.IsRunSheetAuthorised);
			AssertEquals(true, leg.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_Reference == "A non Customs Cleared Leg added to a Run Sheet by Bob."));
		}

		class RunSheetSecurityQueryProviderForTest : IRunSheetSecurityQueryProvider
		{
			void IRunSheetSecurityQueryProvider.TryAuthorise(CommonCartageLeg leg)
			{
				if (!AuthoriseBy.IsEmpty)
				{
					leg.AuthoriseRunSheet(AuthoriseBy);
				}
			}

			public ZString AuthoriseBy { get; set; }
		}

		public void TestAddLogWhenEachContainerPickedUpFromConsignor()
		{
			const int numContainers = 2;
			const int numLooseMoves = 0;
			// CYD -> CNO -> CTO
			var cartage = Helper.CreateCartageWithoutTemplate(numContainers, numLooseMoves, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			AssertEquals(2, cartage.ContainerBookedMoves.Count);
			AssertEquals("2 containers with 2 legs each", 2 * 2, cartage.CartageLegs.Count);
			for (int i = 0; i < 2; i++)
			{
				var container = cartage.ContainerBookedMoves[i];
				AssertNotNull(container);
				var legCYD_CNO = container.FirstCartageLeg;
				AssertEquals("Precondition: First leg is a pickup from CYD", DocAddressType.LocalCartageYard, legCYD_CNO.PickupDocAddressType);
				legCYD_CNO.JU_PickupTimeOut = ZDateTime.Now;
				AssertEquals("No PickupCartageCompleteFinalised logs should be added for pickup from CYD", 0, legCYD_CNO.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode));
				var legCNO_CTO = container.LastCartageLeg;
				AssertEquals("Precondition: Second leg is a pickup from CNO", DocAddressType.LocalCartageExporter, legCNO_CTO.PickupDocAddressType);
				AssertEquals("Precondition: There are no logs for CNO->CTO leg", 0, legCNO_CTO.Logs.GetAllLogs().Count);
				legCNO_CTO.JU_PickupTimeOut = ZDateTime.Now;
				var eventLog = legCNO_CTO.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode);
				AssertNotNull("PickupCartageCompleteFinalised should be added when we set pickup time out for CNO", eventLog);
				AssertEquals(EventConstants.Facilities.Code.Consignor, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
				AssertEquals(Constants.EventReferenceParameterReasons.Pickup, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
				AssertEquals(legCNO_CTO.PickupFromCity, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			}
		}

		public void TestAddLogWhenContainerPickedUpFromConsignor_WaitPoint()
		{
			var cartage = Factory.New<CommonCartage>();
			// CYD -> (CNO) -> CTO  <= The leg with wait point 
			var move = cartage.ContainerBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			Helper.AddCartageLegWithWaitPoint(move, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			var legWithCNOwaitPoint = move.FirstCartageLeg;
			AssertEquals("Precondition: The leg has a waitpoint from CNO", DocAddressType.LocalCartageExporter, legWithCNOwaitPoint.WaitPointDocAddressType);
			AssertEquals("Precondition: There are no logs for CNO->CTO leg", 0, legWithCNOwaitPoint.Logs.GetAllLogs().Count);
			legWithCNOwaitPoint.JU_WaitPointTimeOut = ZDateTime.Now;
			var eventLog = legWithCNOwaitPoint.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode);
			AssertNotNull("PickupCartageCompleteFinalised should be added when we set pickup time out for CNO", eventLog);
			AssertEquals(EventConstants.Facilities.Code.Consignor, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals(Constants.EventReferenceParameterReasons.Pickup, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
			AssertEquals(legWithCNOwaitPoint.WaitPointCity, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		public void TestAddLogWhenAllLoosePickedUpFromConsignor()
		{
			const int numContainers = 0;
			const int numLooseMoves = 2;
			// CNO -> CTO -> CYD
			var cartage = Helper.CreateCartageWithoutTemplate(numContainers, numLooseMoves, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageYard);
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			AssertEquals("2 loose moves with 2 legs each", 2 * 2, cartage.CartageLegs.Count);
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode));
			var loose1CNO_CTOLeg = cartage.LooseBookedMoves[0].FirstCartageLeg;
			AssertEquals("Precondition: First leg pickup in loose move is Consignor", DocAddressType.LocalCartageExporter, loose1CNO_CTOLeg.PickupDocAddressType);
			var loose2CNO_CTOLeg = cartage.LooseBookedMoves[1].FirstCartageLeg;
			AssertEquals("Precondition: First leg pickup in loose move is Consignor", DocAddressType.LocalCartageExporter, loose2CNO_CTOLeg.PickupDocAddressType);
			loose1CNO_CTOLeg.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode));
			loose2CNO_CTOLeg.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals("All loose packs picked from Consignor - the PickupCartageCompleteFinalised event should be added to the cartage", 1, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode));
			var eventLog = cartage.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode);
			AssertEquals(EventConstants.Facilities.Desc.Consignor, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals(Constants.EventReferenceParameterReasons.Pickup, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
		}

		public void TestAddLogWhenAllLoosePickedUpFromConsignor_WithWaitPoint()
		{
			var cartage = Factory.New<CommonCartage>();
			// CNO -> CTO -> CYD
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			Helper.AddCartageLegsToBookedMovement(looseMove1, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageYard);
			// CNO - (CTO) - CYD  <= This leg has wait point 
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			Helper.AddCartageLegWithWaitPoint(looseMove2, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			AssertEquals("2 legs from looseMove1 + 1 leg from looseMove2", 3, cartage.CartageLegs.Count);
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			var loose1CNO_CTOLeg = cartage.LooseBookedMoves[0].FirstCartageLeg;
			AssertEquals("Precondition: First leg pickup in loose move is Consignor", DocAddressType.LocalCartageExporter, loose1CNO_CTOLeg.PickupDocAddressType);
			var loose2_Leg = cartage.LooseBookedMoves[1].FirstCartageLeg;
			AssertEquals("Precondition: First leg waitpoint in loose move is Consignor", DocAddressType.LocalCartageExporter, loose2_Leg.WaitPointDocAddressType);
			loose1CNO_CTOLeg.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			loose2_Leg.JU_WaitPointTimeOut = ZDateTime.Now;
			AssertEquals("All loose packs picked from Consignor - the PickupCartageCompleteFinalised event should be added to the cartage", 1, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode));
			var eventLog = cartage.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode);
			AssertEquals(EventConstants.Facilities.Desc.Consignor, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals(Constants.EventReferenceParameterReasons.Pickup, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
		}

		public void TestAddLogWhenEachContainerDeliveredToConsignee()
		{
			const int numContainers = 2;
			const int numLooseMoves = 0;
			// CTO -> CNE -> CYD
			var cartage = Helper.CreateCartageWithoutTemplate(numContainers, numLooseMoves, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			AssertEquals(2, cartage.ContainerBookedMoves.Count);
			AssertEquals("2 containers with 2 legs each", 2 * 2, cartage.CartageLegs.Count);
			for (int i = 0; i < 2; i++)
			{
				var container = cartage.ContainerBookedMoves[i];
				AssertNotNull(container);
				var legCTO_CNE = container.FirstCartageLeg;
				AssertEquals("Precondition: First leg is a delivery to CNE", DocAddressType.LocalCartageImporter, legCTO_CNE.DeliverToDocAddressType);
				AssertEquals("Precondition: There are no logs for CTO->CNE leg", 0, legCTO_CNE.Logs.GetAllLogs().Count);
				legCTO_CNE.JU_DeliverTimeOut = ZDateTime.Now;
				var eventLog = legCTO_CNE.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode);
				AssertNotNull("DeliveryCartageCompleteFinalisedCode should be added when we set deliver time out for CNE", eventLog);
				AssertEquals(EventConstants.Facilities.Code.Consignee, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
				AssertEquals(Constants.EventReferenceParameterReasons.Delivery, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
				AssertEquals(legCTO_CNE.DeliverToCity, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
				var legCNE_CYD = container.LastCartageLeg;
				AssertEquals("Precondition: Second leg is a delivery to CYD", DocAddressType.LocalCartageYard, legCNE_CYD.DeliverToDocAddressType);
				legCNE_CYD.JU_PickupTimeOut = ZDateTime.Now;
				AssertEquals("No DeliveryCartageCompleteFinalisedCode logs should be added for deliver to CYD", 0, legCNE_CYD.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			}
		}

		public void TestAddLogWhenContainerDeliveredToConsignee_WithWaitPoint()
		{
			var cartage = Factory.New<CommonCartage>();
			// CTO - (CNE) - CYD  <= The leg with wait point 
			var move = cartage.ContainerBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			Helper.AddCartageLegWithWaitPoint(move, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			var legWithCNEwaitPoint = move.FirstCartageLeg;
			AssertEquals("Precondition: First leg has a waitpoint CNE", DocAddressType.LocalCartageImporter, legWithCNEwaitPoint.WaitPointDocAddressType);
			AssertEquals("Precondition: There are no logs for CNO->CYD leg", 0, legWithCNEwaitPoint.Logs.GetAllLogs().Count);
			legWithCNEwaitPoint.JU_WaitPointTimeOut = ZDateTime.Now;
			var eventLog = legWithCNEwaitPoint.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode);
			AssertNotNull("DeliveryCartageCompleteFinalisedCode should be added when we set deliver time out for CNE", eventLog);
			AssertEquals(EventConstants.Facilities.Code.Consignee, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals(Constants.EventReferenceParameterReasons.Delivery, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
			AssertEquals(legWithCNEwaitPoint.WaitPointCity, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		public void TestAddLogWhenAllLooseDeliveredToConsignee()
		{
			const int numContainers = 0;
			const int numLooseMoves = 2;
			// CTO -> CNE -> CYD
			var cartage = Helper.CreateCartageWithoutTemplate(numContainers, numLooseMoves, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			AssertEquals("2 loose moves with 2 legs each", 2 * 2, cartage.CartageLegs.Count);
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			var loose1CTO_CNELeg = cartage.LooseBookedMoves[0].FirstCartageLeg;
			AssertEquals("Precondition: First leg delivery in loose move is Consignee", DocAddressType.LocalCartageImporter, loose1CTO_CNELeg.DeliverToDocAddressType);
			var loose2CTO_CNELeg = cartage.LooseBookedMoves[1].FirstCartageLeg;
			AssertEquals("Precondition: First leg delivery in loose move is Consignee", DocAddressType.LocalCartageImporter, loose2CTO_CNELeg.DeliverToDocAddressType);
			loose1CTO_CNELeg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			loose2CTO_CNELeg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("All loose packs delivered to Consignee - the DeliveryCartageCompleteFinalised event should be added to the cartage", 1, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			var eventLog = cartage.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode);
			AssertEquals(EventConstants.Facilities.Desc.Consignee, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals(Constants.EventReferenceParameterReasons.Delivery, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
		}

		public void TestAddLogWhenAllLooseDeliveredToConsignee_WithWaitPoint()
		{
			var cartage = Factory.New<CommonCartage>();
			// CTO -> CNE -> CYD
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			Helper.AddCartageLegsToBookedMovement(looseMove1, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			// CTO - (CNE) - CYD  <= This leg has wait point 
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			Helper.AddCartageLegWithWaitPoint(looseMove2, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			AssertEquals("2 legs from looseMove1 + 1 leg from looseMove2", 3, cartage.CartageLegs.Count);
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			var loose1CTO_CNELeg = cartage.LooseBookedMoves[0].FirstCartageLeg;
			AssertEquals("Precondition: First leg delivery in loose move is Consignee", DocAddressType.LocalCartageImporter, loose1CTO_CNELeg.DeliverToDocAddressType);
			var loose2_Leg = cartage.LooseBookedMoves[1].FirstCartageLeg;
			AssertEquals("Precondition: First leg waitpoint in loose move is Consignee", DocAddressType.LocalCartageImporter, loose2_Leg.WaitPointDocAddressType);
			loose1CTO_CNELeg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			loose2_Leg.JU_WaitPointTimeOut = ZDateTime.Now;
			AssertEquals("All loose packs delivered to Consignee - the DeliveryCartageCompleteFinalised event should be added to the cartage", 1, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode));
			var eventLog = cartage.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.DeliveryCartageCompleteFinalisedCode);
			AssertEquals(EventConstants.Facilities.Desc.Consignee, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals(Constants.EventReferenceParameterReasons.Delivery, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
		}

		public void TestAddLogWhenJobComplete()
		{
			const int numContainers = 3;
			const int numLooseMoves = 2;
			// CTO -> CNE -> CYD
			var cartage = Helper.CreateCartageWithoutTemplate(numContainers, numLooseMoves, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			AssertEquals(3, cartage.ContainerBookedMoves.Count);
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			AssertEquals("3 containers and 2 loose moves with 2 legs each", (3 + 2) * 2, cartage.CartageLegs.Count);
			AssertEquals(0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.CartageCompleteFinalisedCode));
			for (int i = 0; i < cartage.CartageLegs.Count - 1; i++)
			{
				cartage.CartageLegs[i].JU_DeliverTimeOut = ZDateTime.Now;
				AssertEquals("Not all legs completed - no event should be added", 0, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.CartageCompleteFinalisedCode));
			}

			// last leg complete
			cartage.CartageLegs[cartage.CartageLegs.Count - 1].JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("All legs completed - CartageCompleteFinalised event should be added", 1, cartage.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.CartageCompleteFinalisedCode));
		}

		public void TestAddLogWhenContainerDeliveredToAContainerYard()
		{
			// CTO -> CNE -> CYD
			var cartage = Helper.CreateCartageWithoutTemplate(2, 0, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			AssertEquals(true, cartage.IsContainerised);
			AssertEquals(false, cartage.IsLoose);
			AssertEquals(2, cartage.ContainerBookedMoves.Count);
			AssertEquals("2 containers with 2 legs each", 2 * 2, cartage.CartageLegs.Count);
			for (int i = 0; i < 2; i++)
			{
				var container = cartage.ContainerBookedMoves[i];
				AssertNotNull(container);
				var legCTO_CNE = container.FirstCartageLeg;
				AssertEquals("Precondition: First leg is a delivery to CNE", DocAddressType.LocalCartageImporter, legCTO_CNE.DeliverToDocAddressType);
				legCTO_CNE.JU_DeliverTimeOut = ZDateTime.Now;
				AssertEquals("No DeliveryCartageCompleteFinalisedCode logs should be added for deliver to CYD", 0, legCTO_CNE.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.GateInCode));
				var legCNE_CYD = container.LastCartageLeg;
				AssertEquals("Precondition: Second leg is a delivery to CYD", DocAddressType.LocalCartageYard, legCNE_CYD.DeliverToDocAddressType);
				AssertEquals("Precondition: There are no logs for CNE->CYD leg", 0, legCNE_CYD.Logs.GetAllLogs().Count);
				legCNE_CYD.JU_DeliverTimeOut = ZDateTime.Now;
				var eventLog = legCNE_CYD.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == Events.GateInCode);
				AssertNotNull("GateInCode should be added when we set deliver time out for CNE", eventLog);
				AssertEquals(EventConstants.Facilities.Code.ContainerYard, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
				AssertEquals(Constants.EventReferenceParameterReasons.Delivery, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
				AssertEquals(legCNE_CYD.DeliverToCity, eventLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			}
		}

		public void TestInOutEvents()
		{
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cne = Helper.CreateOrgHeader("CNESYD", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			cto.MainAddress.OA_City = "Botany";
			cne.MainAddress.OA_City = "Alexandria";
			cyd.MainAddress.OA_City = "Mascot";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cne.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			var ctoToCNE = move.CartageLegs[0];
			var cneToCYD = move.CartageLegs[1];
			AssertEquals("Precondition.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.Arrival).Any());
			AssertEquals("Precondition.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.Departure).Any());
			AssertEquals("Precondition.", false, FindEventReferences(cneToCYD.PK, AutoEvents.Arrival).Any());
			AssertEquals("Precondition.", false, FindEventReferences(cneToCYD.PK, AutoEvents.Departure).Any());
			ctoToCNE.JU_PickupTimeIn = ZDateTime.Now;
			ctoToCNE.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(1);
			ctoToCNE.JU_PickupTimeOut = ZDateTime.Now;
			ctoToCNE.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(1);
			ctoToCNE.JU_DeliverTimeIn = ZDateTime.Now;
			ctoToCNE.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(1);
			ctoToCNE.JU_DeliverTimeOut = ZDateTime.Now;
			ctoToCNE.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_PickupTimeIn = ZDateTime.Now;
			cneToCYD.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_PickupTimeOut = ZDateTime.Now;
			cneToCYD.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_DeliverTimeIn = ZDateTime.Now;
			cneToCYD.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_DeliverTimeOut = ZDateTime.Now;
			cneToCYD.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(1);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Botany|RES=Pickup", "CONT123456|FAC=CNE|LOC=Alexandria|RES=Delivery" }, FindEventReferences(ctoToCNE.PK, AutoEvents.Arrival));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Botany|RES=Pickup", "CONT123456|FAC=CNE|LOC=Alexandria|RES=Delivery" }, FindEventReferences(ctoToCNE.PK, AutoEvents.Departure));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CNE|LOC=Alexandria|RES=Pickup", "CONT123456|FAC=CY|LOC=Mascot|RES=Delivery" }, FindEventReferences(cneToCYD.PK, AutoEvents.Arrival));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CNE|LOC=Alexandria|RES=Pickup", "CONT123456|FAC=CY|LOC=Mascot|RES=Delivery" }, FindEventReferences(cneToCYD.PK, AutoEvents.Departure));
		}

		IEnumerable<string> FindEventReferences(ZGuid parentID, Event eventType)
		{
			return FindEvents(parentID, eventType).Select(e => e.SL_Reference.ToString());
		}

		IEnumerable<StmALog> FindEvents(ZGuid parentID, Event eventType)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, parentID);
			return Factory.Load<StmALog>(query);
		}

		public void TestPickupAndDeliveryEvents()
		{
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cne = Helper.CreateOrgHeader("CNESYD", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			cto.MainAddress.OA_City = "Botany";
			cne.MainAddress.OA_City = "Alexandria";
			cyd.MainAddress.OA_City = "Mascot";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cne.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			var ctoToCNE = move.CartageLegs[0];
			var cneToCYD = move.CartageLegs[1];
			AssertEquals("Precondition.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Precondition.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.Delivered).Any());
			AssertEquals("Precondition.", false, FindEventReferences(cneToCYD.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Precondition.", false, FindEventReferences(cneToCYD.PK, AutoEvents.Delivered).Any());
			ctoToCNE.JU_AdditionalService = "FUT";
			cneToCYD.JU_AdditionalService = "FUT";
			ctoToCNE.JU_PickupTimeIn = ZDateTime.Now;
			ctoToCNE.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(1);
			ctoToCNE.JU_PickupTimeOut = ZDateTime.Now;
			ctoToCNE.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(1);
			ctoToCNE.JU_DeliverTimeIn = ZDateTime.Now;
			ctoToCNE.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(1);
			ctoToCNE.JU_DeliverTimeOut = ZDateTime.Now;
			ctoToCNE.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_PickupTimeIn = ZDateTime.Now;
			cneToCYD.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_PickupTimeOut = ZDateTime.Now;
			cneToCYD.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_DeliverTimeIn = ZDateTime.Now;
			cneToCYD.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(1);
			cneToCYD.JU_DeliverTimeOut = ZDateTime.Now;
			cneToCYD.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(1);
			Factory.Save();
			AssertEquals("Futile, so don't add events.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Futile, so don't add events.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.Delivered).Any());
			AssertEquals("Futile, so don't add events.", false, FindEventReferences(cneToCYD.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Futile, so don't add events.", false, FindEventReferences(cneToCYD.PK, AutoEvents.Delivered).Any());
			ctoToCNE.JU_AdditionalService = "";
			cneToCYD.JU_AdditionalService = "";
			Factory.Save();
			AssertEquals("Was futile, can only add events if the setters are triggered.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Was futile, can only add events if the setters are triggered.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.Delivered).Any());
			AssertEquals("Was futile, can only add events if the setters are triggered.", false, FindEventReferences(cneToCYD.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Was futile, can only add events if the setters are triggered.", false, FindEventReferences(cneToCYD.PK, AutoEvents.Delivered).Any());
			ctoToCNE.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(2);
			ctoToCNE.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(2);
			ctoToCNE.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(2);
			ctoToCNE.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(2);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Botany|TYP=FUL" }, FindEventReferences(ctoToCNE.PK, AutoEvents.PickedUp));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CNE|LOC=Alexandria|TYP=FUL" }, FindEventReferences(ctoToCNE.PK, AutoEvents.Delivered));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CNE|LOC=Alexandria|TYP=EMT" }, FindEventReferences(cneToCYD.PK, AutoEvents.PickedUp));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Mascot|TYP=EMT" }, FindEventReferences(cneToCYD.PK, AutoEvents.Delivered));
		}

		public void TestPickupAndDeliveryEvent_ContainerType_NoWaitPoint()
		{
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cne = Helper.CreateOrgHeader("CNESYD", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			cto.MainAddress.OA_City = "Botany";
			cne.MainAddress.OA_City = "Alexandria";
			cyd.MainAddress.OA_City = "Mascot";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cne.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			var ctoToCNE = move.CartageLegs[0];
			var cneToCYD = move.CartageLegs[1];
			AssertEquals("Precondition.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Precondition.", false, FindEventReferences(ctoToCNE.PK, AutoEvents.Delivered).Any());
			AssertEquals("Precondition.", false, FindEventReferences(cneToCYD.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Precondition.", false, FindEventReferences(cneToCYD.PK, AutoEvents.Delivered).Any());
			ctoToCNE.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(2);
			ctoToCNE.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(2);
			ctoToCNE.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(2);
			ctoToCNE.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(2);
			cneToCYD.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(2);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Botany|TYP=FUL" }, FindEventReferences(ctoToCNE.PK, AutoEvents.PickedUp));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CNE|LOC=Alexandria|TYP=FUL" }, FindEventReferences(ctoToCNE.PK, AutoEvents.Delivered));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CNE|LOC=Alexandria|TYP=EMT" }, FindEventReferences(cneToCYD.PK, AutoEvents.PickedUp));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Mascot|TYP=EMT" }, FindEventReferences(cneToCYD.PK, AutoEvents.Delivered));
		}

		public void TestPickupAndDeliveryEvent_ContainerType_HasWaitPoint_DirectionImport()
		{
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cfs = Helper.CreateOrgHeader("CFSCAN", "CFS");
			var cyd = Helper.CreateOrgHeader("CYDMEL", "CYD");
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			cartage.JJ_ConsignmentID = "Hi";
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			//	Direction:	Import
			//		CTO[F] â€“ [F]CFS[E] â€“ [E]CYD
			move.CartageLegs.DeleteAll();
			Helper.AddCartageLegWithWaitPoint(move, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageYard);
			var legWithWaitPoint = move.FirstCartageLeg;
			AssertEquals("Precondition: First leg has a waitpoint CFS", DocAddressType.LocalCartageCFS, legWithWaitPoint.WaitPointDocAddressType);
			AssertEquals("Precondition: There are no logs yet.", 0, legWithWaitPoint.Logs.GetAllLogs().Count);
			legWithWaitPoint.JU_PickupTimeIn = ZDateTime.Now;
			legWithWaitPoint.JU_PickupTimeOut = ZDateTime.Now;
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Sydney|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeIn = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Sydney|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeOut = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Sydney|TYP=FUL", "CONT123456|FAC=CFS|LOC=Canberra|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_DeliverTimeIn = ZDateTime.Now.AddHours(9);
			legWithWaitPoint.JU_DeliverTimeOut = ZDateTime.Now.AddHours(9);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=FUL", "CONT123456|FAC=CY|LOC=Melbourne|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
		}

		public void TestPickupAndDeliveryEvent_ContainerType_HasWaitPoint_DirectionDestination()
		{
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cfs = Helper.CreateOrgHeader("CFSCAN", "CFS");
			var cyd = Helper.CreateOrgHeader("CYDMEL", "CYD");
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.JJ_Direction = Constants.CartageDirection.Destination;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			//	Direction:	Destination
			//		CTO[F] â€“ [F]CFS[E] â€“ [E]CYD
			move.CartageLegs.DeleteAll();
			Helper.AddCartageLegWithWaitPoint(move, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageYard);
			var legWithWaitPoint = move.FirstCartageLeg;
			AssertEquals("Precondition: First leg has a waitpoint CFS", DocAddressType.LocalCartageCFS, legWithWaitPoint.WaitPointDocAddressType);
			AssertEquals("Precondition: There are no logs yet.", 0, legWithWaitPoint.Logs.GetAllLogs().Count);
			legWithWaitPoint.JU_PickupTimeIn = ZDateTime.Now;
			legWithWaitPoint.JU_PickupTimeOut = ZDateTime.Now;
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Sydney|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeIn = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Sydney|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeOut = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CTO|LOC=Sydney|TYP=FUL", "CONT123456|FAC=CFS|LOC=Canberra|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_DeliverTimeIn = ZDateTime.Now.AddHours(9);
			legWithWaitPoint.JU_DeliverTimeOut = ZDateTime.Now.AddHours(9);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=FUL", "CONT123456|FAC=CY|LOC=Melbourne|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
		}

		public void TestPickupAndDeliveryEvent_ContainerType_HasWaitPoint_DirectionExport()
		{
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			var cfs = Helper.CreateOrgHeader("CFSCAN", "CFS");
			var cto = Helper.CreateOrgHeader("CTOMEL", "CTO");
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			cartage.JJ_ConsignmentID = "Hi";
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			//	Direction:	Export
			//		CYD[E] â€“ [E]CFS[F] â€“ [F]CTO
			move.CartageLegs.DeleteAll();
			Helper.AddCartageLegWithWaitPoint(move, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageCTO);
			var legWithWaitPoint = move.FirstCartageLeg;
			AssertEquals("Precondition: First leg has a waitpoint CFS", DocAddressType.LocalCartageCFS, legWithWaitPoint.WaitPointDocAddressType);
			AssertEquals("Precondition: There are no logs yet.", 0, legWithWaitPoint.Logs.GetAllLogs().Count);
			legWithWaitPoint.JU_PickupTimeIn = ZDateTime.Now;
			legWithWaitPoint.JU_PickupTimeOut = ZDateTime.Now;
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Sydney|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeIn = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Sydney|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeOut = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Sydney|TYP=EMT", "CONT123456|FAC=CFS|LOC=Canberra|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_DeliverTimeIn = ZDateTime.Now.AddHours(9);
			legWithWaitPoint.JU_DeliverTimeOut = ZDateTime.Now.AddHours(9);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=EMT", "CONT123456|FAC=CTO|LOC=Melbourne|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
		}

		public void TestPickupAndDeliveryEvent_ContainerType_HasWaitPoint_DirectionOrigin()
		{
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			var cfs = Helper.CreateOrgHeader("CFSCAN", "CFS");
			var cto = Helper.CreateOrgHeader("CTOMEL", "CTO");
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.JJ_Direction = Constants.CartageDirection.Origin;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			//	Direction:	Origin
			//		CYD[E] â€“ [E]CFS[F] â€“ [F]CTO
			move.CartageLegs.DeleteAll();
			Helper.AddCartageLegWithWaitPoint(move, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageCTO);
			var legWithWaitPoint = move.FirstCartageLeg;
			AssertEquals("Precondition: First leg has a waitpoint CFS", DocAddressType.LocalCartageCFS, legWithWaitPoint.WaitPointDocAddressType);
			AssertEquals("Precondition: There are no logs yet.", 0, legWithWaitPoint.Logs.GetAllLogs().Count);
			legWithWaitPoint.JU_PickupTimeIn = ZDateTime.Now;
			legWithWaitPoint.JU_PickupTimeOut = ZDateTime.Now;
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Sydney|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeIn = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Sydney|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_WaitPointTimeOut = ZDateTime.Now.AddHours(3);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=EMT" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CY|LOC=Sydney|TYP=EMT", "CONT123456|FAC=CFS|LOC=Canberra|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.PickedUp));
			legWithWaitPoint.JU_DeliverTimeIn = ZDateTime.Now.AddHours(9);
			legWithWaitPoint.JU_DeliverTimeOut = ZDateTime.Now.AddHours(9);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT123456|FAC=CFS|LOC=Canberra|TYP=EMT", "CONT123456|FAC=CTO|LOC=Melbourne|TYP=FUL" }, FindEventReferences(legWithWaitPoint.PK, AutoEvents.Delivered));
		}

		public void TestPickupAndDeliveryEvent_LooseMoveDoNotHavePickedUpAndDeliveryEvent()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			var move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			var leg = move.CartageLegs.AddNew();
			var addressCFS = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			var addressCTO = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			leg.JU_E2PickupAddressID = addressCFS.PK;
			leg.JU_E2DeliveryAddressID = addressCTO.PK;
			AssertEquals("Precondition.", false, FindEventReferences(leg.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Precondition.", false, FindEventReferences(leg.PK, AutoEvents.Delivered).Any());
			leg.JU_AdditionalService = "FUT";
			leg.JU_PickupTimeIn = ZDateTime.Now;
			leg.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(1);
			leg.JU_PickupTimeOut = ZDateTime.Now;
			leg.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(1);
			leg.JU_DeliverTimeIn = ZDateTime.Now;
			leg.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(1);
			leg.JU_DeliverTimeOut = ZDateTime.Now;
			leg.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(1);
			Factory.Save();
			AssertEquals("Futile, so don't add events.", false, FindEventReferences(leg.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Futile, so don't add events.", false, FindEventReferences(leg.PK, AutoEvents.Delivered).Any());
			leg.JU_AdditionalService = "";
			Factory.Save();
			AssertEquals("Was futile, can only add events if the setters are triggered.", false, FindEventReferences(leg.PK, AutoEvents.PickedUp).Any());
			AssertEquals("Was futile, can only add events if the setters are triggered.", false, FindEventReferences(leg.PK, AutoEvents.Delivered).Any());
			leg.JU_PickupTimeIn = ZDateTime.Now.AddMinutes(2);
			leg.JU_PickupTimeOut = ZDateTime.Now.AddMinutes(2);
			leg.JU_DeliverTimeIn = ZDateTime.Now.AddMinutes(2);
			leg.JU_DeliverTimeOut = ZDateTime.Now.AddMinutes(2);
			var pickedupLog = leg.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.PickedUpCode);
			var deliveredLog = leg.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.DeliveredCode);
			AssertNotNull(pickedupLog);
			AssertNotNull(deliveredLog);
			AssertEquals("Loose move leg events should not have Type parameter.", false, pickedupLog.Parameters.Any(p => p.Key == EventConstants.EventReferenceParameters.Codes.Type));
			AssertEquals("Loose move leg events should not have Type parameter.", false, deliveredLog.Parameters.Any(p => p.Key == EventConstants.EventReferenceParameters.Codes.Type));
		}

		public void TestSignatureReceivedEvents()
		{
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			var cne = Helper.CreateOrgHeader("CNESYD", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			cto.MainAddress.OA_City = "Botany";
			cne.MainAddress.OA_City = "Alexandria";
			cyd.MainAddress.OA_City = "Mascot";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "Hi";
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cne.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			var move = cartage.GetBookedMoves(container)[0];
			var ctoToCNE = move.CartageLegs[0];
			var cneToCYD = move.CartageLegs[1];
			ctoToCNE.JU_AdditionalService = Constants.CartageAdditional.Futile;
			cneToCYD.JU_AdditionalService = Constants.CartageAdditional.Futile;
			Factory.Save();
			var signatureCapturedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SignatureCaptured.Code);
			AssertEquals("Precondition. Should not have any events", false, ctoToCNE.Logs.Find(signatureCapturedQuery).Any());
			AssertEquals("Precondition. Should not have a signature.", false, ctoToCNE.ShowSignature);
			AssertEquals("Precondition. Should not have any events", false, cneToCYD.Logs.Find(signatureCapturedQuery).Any());
			AssertEquals("Precondition. Should not have a signature.", false, cneToCYD.ShowSignature);
			ctoToCNE.UpdateSignature(ZBlob.FromAscii("signatureBytes"), Guid.Empty);
			Factory.Save();
			AssertEquals("Should have an attached signature.", true, ctoToCNE.ShowSignature);
			AssertEquals("Should not add an event when DeliverySignedFor does not have a value even though a signature exists.", false, ctoToCNE.Logs.Find(signatureCapturedQuery).Any());
			ctoToCNE.JU_DeliverySignedFor = "Jane Smith";
			Factory.Save();
			AssertEquals("Should add an event when DeliverySignedFor has a value and a signature exists.", true, ctoToCNE.Logs.Find(signatureCapturedQuery).Any());
			AssertMostRecentSignatureEvent(ctoToCNE, "CNE", cne.MainAddress.OA_City, "Jane Smith");
			cneToCYD.JU_DeliverySignedFor = "John Smith";
			Factory.Save();
			AssertEquals("Should not have an attached signature.", false, cneToCYD.ShowSignature);
			AssertEquals("Should not add an event when DeliverySignedFor has a value and a signature does not exist.", false, cneToCYD.Logs.Find(signatureCapturedQuery).Any());
			cneToCYD.UpdateSignature(ZBlob.FromAscii("signatureBytes"), Guid.Empty);
			Factory.Save();
			AssertEquals("Should have an attached signature.", true, cneToCYD.ShowSignature);
			AssertEquals("Should add an event when DeliverySignedFor has a value and a signature is set.", 1, cneToCYD.Logs.Find(signatureCapturedQuery).Length);
			AssertMostRecentSignatureEvent(cneToCYD, "CY", cyd.MainAddress.OA_City, "John Smith");
			cneToCYD.JU_DeliverySignedFor = "Mike Smith";
			Factory.Save();
			AssertEquals("Should add a new event when DeliverySignedFor is changed and a signature exists.", 2, cneToCYD.Logs.Find(signatureCapturedQuery).Length);
			AssertMostRecentSignatureEvent(cneToCYD, "CY", cyd.MainAddress.OA_City, "Mike Smith");
			cneToCYD.UpdateSignature(ZBlob.FromAscii("signatureBytes"), Guid.Empty);
			Factory.Save();
			AssertEquals("Should add a new event when DeliverySignedFor exists and signature changes.", 3, cneToCYD.Logs.Find(signatureCapturedQuery).Length);
			AssertMostRecentSignatureEvent(cneToCYD, "CY", cyd.MainAddress.OA_City, "Mike Smith");
		}

		void AssertMostRecentSignatureEvent(CommonCartageLeg leg, ZString facility, ZString location, ZString name)
		{
			var queryResults = leg.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SignatureCaptured.Code));
			var logEntry = queryResults.OrderByDescending(l => l.SL_EventTime).FirstOrDefault();
			AssertNotNull("Signature event should exist", logEntry);
			AssertEquals("Signature event Facility parameter should match", facility, logEntry.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Signature event Location parameter should match", location, logEntry.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Signature event Name parameter should match", name, logEntry.Parameters[EventConstants.EventReferenceParameters.Codes.Name]);
			AssertEquals(Constants.EventReferenceParameterReasons.Delivery, logEntry.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
		}

		public void TestGetWorkflowInformationProvider()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			AssertNull(leg.Cartage);
			AssertExceptionThrown(typeof(NullReferenceException), () => (leg as IWorkflowProvider).GetWorkflowInformationProvider());
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var workflowInformationProvider = (leg1.Cartage as IWorkflowProvider).GetWorkflowInformationProvider();
			AssertNotNull(workflowInformationProvider);
			AssertEquals("Origin", "", workflowInformationProvider.Origin);
			AssertEquals("Destination", "", workflowInformationProvider.Destination);
			AssertEquals("Business Context", TrackingConstants.BusinessContext.Cartage, workflowInformationProvider.BusinessContext);
			AssertContainsExactElementsInAnyOrder("Companies", new[] { GlbBranch.CurrentBranch.PK }, workflowInformationProvider.Companies);
		}

		public void TestIWorkflowProviderEventMembers()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy, ((IWorkflowProviderEvent)leg).RecipientOrganisations.Single());
		}

		public void TestSetsReadOnlyCorrectlyWhenUpdateReadOnlyForWhenCancelledMethodIsCalled()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			var cartageLegPk = cartage.CartageLegs.First().PK;
			cartage.JJ_IsCancelled = true;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartageLegInNewFactory = newFactory.Load<CommonCartageLeg>(cartageLegPk);
			AssertEquals("Cartage leg should not yet have readonly set", false, cartageLegInNewFactory.ReadOnly);
			cartage.JJ_IsCancelled = true;
			Factory.Save();
			AssertEquals("Cartage leg should not yet have readonly set", false, cartageLegInNewFactory.ReadOnly);
			cartageLegInNewFactory.UpdateReadOnlyForWhenCancelled();
			AssertEquals("Cartage leg should now have readonly set", true, cartageLegInNewFactory.ReadOnly);
			cartage.JJ_IsCancelled = false;
			Factory.Save();
			cartageLegInNewFactory.UpdateReadOnlyForWhenCancelled();
			AssertEquals("Cartage leg should now have readonly un-set again", false, cartageLegInNewFactory.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CommonCartage>().LooseBookedMoves.AddNew().CartageLegs.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
