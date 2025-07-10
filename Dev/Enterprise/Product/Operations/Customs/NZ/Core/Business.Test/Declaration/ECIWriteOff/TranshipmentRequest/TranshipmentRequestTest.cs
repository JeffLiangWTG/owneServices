using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(TranshipmentRequest))]
	public class TranshipmentRequestTest : CusUnderbondTest<TranshipmentRequest>
	{
		public override void TestCanDoUBM()
		{
			var underbond = Factory.New<TranshipmentRequest>();
			Assert(!underbond.CanDoUBM);
		}

		public void TestIsCancelled()
		{
			var underbond = Factory.New<TranshipmentRequest>();
			underbond.C4_Status = CombinedMovementStatus.Codes.CAN;

			AssertEquals(true, underbond.IsCancelled);
			AssertEquals(true, underbond.ReadOnly);
			AssertEquals(true, underbond.EDocPivotCollection.ReadOnly);
		}

		public void TestTSWNumber()
		{
			var underbond = Factory.New<TranshipmentRequest>();
			AssertEquals(ZString.Empty, underbond.TSWNumber);

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = CusUnderbondSchema.Constants.TableName;
			entryNum.CE_ParentID = underbond.PK;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.DomesticTranshipmentRequest;
			entryNum.CE_EntryNum = "12345";
			AssertEquals("12345", underbond.TSWNumber);
		}

		public void TestMovementReasonDefaulting()
		{
			var testMAWB = Factory.NewWithValidTestData<Express.CusMAWB>();
			testMAWB.CM_RL_NKLoadPort = "NZAKL";
			testMAWB.CM_RL_NKDischargePort = "AUSYD";
			var testHAWB = testMAWB.ChildBills.AddNew();
			testHAWB.CS_RL_NKDestination = "AUSYD";
			var request = TranshipmentRequest.Create(testHAWB);
			AssertEquals("Movement Reason for Export defaults to ITR", MovementReason.Codes.InternationalTranshipmentRequest, request.C4_MovementReason);

			testMAWB.CM_RL_NKLoadPort = "AUSYD";
			testMAWB.CM_RL_NKDischargePort = "NZAKL";
			testHAWB.CS_RL_NKDestination = "NZAKL";
			request = TranshipmentRequest.Create(testHAWB);
			AssertEquals("Movement Reason default for Import with NZ destinatin port", MovementReason.Codes.DomesticTranshipmentRequest, request.C4_MovementReason);

			testMAWB.CM_RL_NKLoadPort = "AUSYD";
			testMAWB.CM_RL_NKDischargePort = "NZAKL";
			testHAWB.CS_RL_NKDestination = "NZWLG";
			request = TranshipmentRequest.Create(testHAWB);
			AssertEquals("Movement Reason for Import with a different local destination port should default as DTR as per above", MovementReason.Codes.DomesticTranshipmentRequest, request.C4_MovementReason);

			testMAWB.CM_RL_NKLoadPort = "AUSYD";
			testMAWB.CM_RL_NKDischargePort = "NZAKL";
			testHAWB.CS_RL_NKDestination = "FJNAD";
			request = TranshipmentRequest.Create(testHAWB);
			AssertEquals("Movement Reason for Import with a foreign destination port defaults to ITR", MovementReason.Codes.InternationalTranshipmentRequest, request.C4_MovementReason);
		}

		public void TestVesselNameDefaultingFromLloyds()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, vesselName)).Length);
			const string lloydsNumber = "112233";
			AssertEquals("Precondition: There are no Vessels with Lloyds Number " + lloydsNumber, 0, Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber)).Length);

			var request1 = Factory.New<TranshipmentRequest>();
			request1.C4_TranshipBySeaLloydsIMONum = lloydsNumber;
			AssertEquals("Vessel Name does not update when Lloyds does not match any vessels", ZString.Empty, request1.C4_TranshipBySeaVessel);

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = lloydsNumber;

			var request2 = Factory.New<TranshipmentRequest>();
			request2.C4_TranshipBySeaLloydsIMONum = lloydsNumber;
			AssertEquals("Vessel Name updates when Lloyds matches a vessel", vesselA.RV_Code, request2.C4_TranshipBySeaVessel);
		}

		public void TestLloydsNumberDefaultingForMultipleVessels()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, vesselName)).Length);

			var request1 = Factory.New<TranshipmentRequest>();
			request1.C4_TranshipBySeaVessel = vesselName;
			AssertEquals("Lloyds does not update when RV_Code does not match any vessels", ZString.Empty, request1.C4_TranshipBySeaLloydsIMONum);

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = "112233";

			var request2 = Factory.New<TranshipmentRequest>();
			request2.C4_TranshipBySeaVessel = vesselName;
			AssertEquals("Lloyds updates when RV_Code matches a single vessel", vesselA.RV_LloydsNumber, request2.C4_TranshipBySeaLloydsIMONum);

			var vesselB = Factory.New<RefVessel>();
			vesselB.RV_Code = vesselName;
			vesselB.RV_LloydsNumber = "998877";

			var request3 = Factory.New<TranshipmentRequest>();
			request3.C4_TranshipBySeaVessel = vesselName;
			AssertEquals("Lloyds does not update when RV_Code matches multiple vessels", ZString.Empty, request3.C4_TranshipBySeaLloydsIMONum);
		}

		public void TestMovementReasonAccessibility()
		{
			var testMAWB = Factory.NewWithValidTestData<Express.CusMAWB>();
			testMAWB.CM_RL_NKLoadPort = "NZAKL";
			testMAWB.CM_RL_NKDischargePort = "AUSYD";
			var testHAWB = testMAWB.ChildBills.AddNew();
			testHAWB.CS_RL_NKDestination = "AUSYD";
			var request = TranshipmentRequest.Create(testHAWB);
			Assert("MovementReason_ReadOnly", request.MovementReason_ReadOnly);
		}

		public void TestIsITR()
		{
			var testMAWB = Factory.NewWithValidTestData<Express.CusMAWB>();
			testMAWB.CM_RL_NKLoadPort = "AUSYD";
			testMAWB.CM_RL_NKDischargePort = "NZAKL";
			var testHAWB = testMAWB.ChildBills.AddNew();
			testHAWB.CS_RL_NKDestination = "FJNAD";
			var request = TranshipmentRequest.Create(testHAWB);
			request.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			Assert("IsITR", request.IsITR);
		}

		public void TestIsDTR()
		{
			var testMAWB = Factory.NewWithValidTestData<Express.CusMAWB>();
			testMAWB.CM_RL_NKLoadPort = "AUMEL";
			testMAWB.CM_RL_NKDischargePort = "NZAKL";
			var testHAWB = testMAWB.ChildBills.AddNew();
			testHAWB.CS_RL_NKDestination = "NZWLG";
			var request = TranshipmentRequest.Create(testHAWB);
			request.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			Assert("IsDTR", request.IsDTR);
		}

		public void TestCusEntryNumbers()
		{
			var underbond = Factory.New<TranshipmentRequest>();
			AssertEquals("No CusEntryNumbers", 0, underbond.CusEntryNumbers.Length);
			Factory.Save();
			AssertEquals("No CusEntryNumbers", 0, underbond.CusEntryNumbers.Length);
		}

		public void TestICusUnderbondCorrectlySetup()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			TranshipmentRequest.Create(dec);
			var request = dec.TranshipmentRequest;
			Factory.Save();
			AssertEquals(typeof(TranshipmentRequest), new BusinessObjectFactory().Load<CusUnderbond>(request.PK).GetType());
			AssertEquals(typeof(TranshipmentRequest), new BusinessObjectFactory().Load<Integration.Customs.NZ.ICusUnderbond>(request.PK).GetType());
		}

		public void TestClearIrrelevantData()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			TranshipmentRequest.Create(dec);
			var request = dec.TranshipmentRequest;
			PopulateCraftData(request);
			AssertCraftDetails(request, "ADMIRALENGRACHT", "12345", "VY123", "SA123", ZDateTime.Today);

			request.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertCraftDetails(request, ZString.Empty, ZString.Empty, ZString.Empty, "SA123", ZDateTime.Today);
			PopulateCraftData(request);

			request.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertCraftDetails(request, "ADMIRALENGRACHT", "12345", "VY123", ZString.Empty, ZDateTime.Today);
			PopulateCraftData(request);

			request.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			AssertCraftDetails(request, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			PopulateCraftData(request);
		}

		void PopulateCraftData(TranshipmentRequest request)
		{
			request.C4_TranshipBySeaVessel = "ADMIRALENGRACHT";
			request.C4_TranshipBySeaLloydsIMONum = "12345";
			request.C4_TranshipBySeaVoyage = "VY123";
			request.C4_FlightNo = "SA123";
			request.C4_ArrivalDate = ZDateTime.Today;
		}

		void AssertCraftDetails(TranshipmentRequest request, ZString vessel, ZString lloyds, ZString voyage, ZString flight, ZDateTime arrivalDate)
		{
			AssertEquals("C4_TranshipBySeaVessel", vessel, request.C4_TranshipBySeaVessel);
			AssertEquals("C4_TranshipBySeaVessel", lloyds, request.C4_TranshipBySeaLloydsIMONum);
			AssertEquals("C4_TranshipBySeaVoyage", voyage, request.C4_TranshipBySeaVoyage);
			AssertEquals("C4_FlightNo", flight, request.C4_FlightNo);
			AssertEquals("C4_ArrivalDate", arrivalDate, request.C4_ArrivalDate);
		}

		public void TestLoadResultsInOneDbHitAndSame()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			TranshipmentRequest.Load(dec);
			AssertEquals(0, Factory.GetTableHitCount(CusUnderbondSchema.Constants.TableName));

			TranshipmentRequest.Create(dec);
			TranshipmentRequest.Create(dec);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDec = newFactory.Load<JobDeclaration>(dec.PK);
			AssertEquals(0, newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName));
			var newRequest1 = TranshipmentRequest.Load(newDec);
			AssertEquals("DB hit when first loading", 1, newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName));
			var newRequest2 = TranshipmentRequest.Load(newDec);
			AssertEquals("No DB hit thereafter", 1, newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName));
			AssertSame("Loading should be consistent even when multiple exist", newRequest1, newRequest2);
		}

		public void TestReload()
		{
			Express.CusMAWB testMAWB = Factory.NewWithValidTestData<Express.CusMAWB>();
			Express.CusHAWB testHAWB = testMAWB.ChildBills.AddNew();
			testHAWB.CS_RL_NKDestination = "FJNAN";
			var itr = TranshipmentRequest.Create(testHAWB);
			itr.C4_ModeOfMovement = "1";
			Factory.Save();
			itr.C4_ModeOfMovement = "";
			ZGuid createdITR_PK = itr.PK;

			testHAWB.ReLoadTranshipmentFromDB();
			AssertEquals("Reloaded transhipment request", "1", testHAWB.TranshipmentRequest.C4_ModeOfMovement);
			AssertEquals("Transhipment Request reloaded", createdITR_PK, testHAWB.TranshipmentRequest.PK);
		}

		[TestDate(2018, 08, 15)]
		public void TestDefaultFromRouting()
		{
			var t = ZDateTime.Today;
			var t1 = ZDateTime.Today.AddDays(1);
			var t2 = ZDateTime.Today.AddDays(2);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			TranshipmentRequest.Create(declaration);
			var request = declaration.TranshipmentRequest;

			CreatePopulatedTransport(declaration, "DEFRA", "ZAJNB", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, t2, ZDateTime.Empty, ZDateTime.Empty);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertIncomingCraftDetails(request, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, "Should not default non-macthing ports");

			CreatePopulatedTransport(declaration, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, t2, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertIncomingCraftDetails(request, "1", "ADMIRALENGRACHT", "VY123", ZString.Empty, t2, "Should default from NZ transport");

			CreatePopulatedTransport(declaration, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Air, ZString.Empty, "SA123", ZDateTime.Empty, t1, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertIncomingCraftDetails(request, "4", ZString.Empty, ZString.Empty, "SA123", t1, "Should default transport for NZ with earliest ETA");

			CreatePopulatedTransport(declaration, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Air, ZString.Empty, "SA124", t, t1, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			request.C4_ModeOfMovement = ZString.Empty;
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertIncomingCraftDetails(request, "4", ZString.Empty, ZString.Empty, "SA124", t, "Should default transport for NZ with earliest ATA or ETA");

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			CreatePopulatedTransport(importDeclaration, "AUSYD", "NZAKL", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, t);
			CreatePopulatedTransport(importDeclaration, "NZTRG", "FJSUV", Core.Constants.TransportModes.Sea, "AAL FREMANTLE", "753E", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, t2);

			TranshipmentRequest.Create(importDeclaration);
			var itrRequest = importDeclaration.TranshipmentRequest;
			itrRequest.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertOutgoingCraftDetails(itrRequest, "1", "AAL FREMANTLE", "753E", ZString.Empty, t2, "Should default to outgoing leg of transhipment");
		}

		[TestDate(2018, 08, 15)]
		public void TestDefaultFromShipmentRouting()
		{
			var t = ZDateTime.Today;
			var t1 = ZDateTime.Today.AddDays(1);
			var t2 = ZDateTime.Today.AddDays(2);

			var shipment = Factory.New<CommonShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_JS = shipment.PK;
			TranshipmentRequest.Create(declaration);
			var request = declaration.TranshipmentRequest;

			CreatePopulatedTransport(declaration, "DEFRA", "ZAJNB", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, t2, ZDateTime.Empty, ZDateTime.Empty);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaCV;
			AssertIncomingCraftDetails(request, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, "Should not default non-macthing ports");

			CreateShipmentTransport(shipment, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, t2, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			shipment.Transports.Load();
			declaration.Shipment.Transports.Reload(false);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertIncomingCraftDetails(request, "1", "ADMIRALENGRACHT", "VY123", ZString.Empty, t2, "Should default from NZ transport");

			CreateShipmentTransport(shipment, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Air, ZString.Empty, "SA123", ZDateTime.Empty, t1, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			shipment.Transports.Load();
			declaration.Shipment.Transports.Reload(false);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertIncomingCraftDetails(request, "4", ZString.Empty, ZString.Empty, "SA123", t1, "Should default transport for NZ with earliest ETA");

			CreateShipmentTransport(shipment, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Air, ZString.Empty, "SA124", t, t1, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			shipment.Transports.Load();
			declaration.Shipment.Transports.Reload(false);
			request.C4_ModeOfMovement = ZString.Empty;
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertIncomingCraftDetails(request, "4", ZString.Empty, ZString.Empty, "SA124", t, "Should default transport for NZ with earliest ATA or ETA");

			var importShipment = Factory.New<CommonShipment>();
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			importDeclaration.JE_JS = importShipment.PK;
			CreateShipmentTransport(importShipment, "AUSYD", "NZAKL", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, t);
			CreateShipmentTransport(importShipment, "NZTRG", "FJSUV", Core.Constants.TransportModes.Sea, "AAL FREMANTLE", "753E", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, t2);

			TranshipmentRequest.Create(importDeclaration);
			var itrRequest = importDeclaration.TranshipmentRequest;
			itrRequest.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertOutgoingCraftDetails(itrRequest, "1", "AAL FREMANTLE", "753E", ZString.Empty, t2, "Should default to outgoing leg of transhipment");
		}

		[TestDate(2018, 08, 15)]
		public void TestDefaultFromConsolRouting()
		{
			var t = ZDateTime.Today;
			var t1 = ZDateTime.Today.AddDays(1);
			var t2 = ZDateTime.Today.AddDays(2);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_JS = shipment.PK;
			TranshipmentRequest.Create(declaration);
			var request = declaration.TranshipmentRequest;

			CreateConsolTransport(consol, "DEFRA", "ZAJNB", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, t2, ZDateTime.Empty, ZDateTime.Empty);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertIncomingCraftDetails(request, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, "Should not default non-macthing ports");

			CreateConsolTransport(consol, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, t2, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			shipment.Transports.Load();
			declaration.Shipment.Transports.Reload(false);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertIncomingCraftDetails(request, "1", "ADMIRALENGRACHT", "VY123", ZString.Empty, t2, "Should default from NZ transport");

			CreateConsolTransport(consol, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Air, ZString.Empty, "SA123", ZDateTime.Empty, t1, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			shipment.Transports.Load();
			declaration.Shipment.Transports.Reload(false);
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaOV;
			AssertIncomingCraftDetails(request, "4", ZString.Empty, ZString.Empty, "SA123", t1, "Should default transport for NZ with earliest ETA");

			CreateConsolTransport(consol, "ZAJNB", "NZAKL", Core.Constants.TransportModes.Air, ZString.Empty, "SA124", t, t1, ZDateTime.Empty, ZDateTime.Empty);
			ClearIncomingCraftDetails(request);
			shipment.Transports.Load();
			declaration.Shipment.Transports.Reload(false);
			request.C4_ModeOfMovement = ZString.Empty;
			request.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaCV;
			AssertIncomingCraftDetails(request, "4", ZString.Empty, ZString.Empty, "SA124", t, "Should default transport for NZ with earliest ATA or ETA");

			var importConsol = Factory.New<ForwardingConsol>();
			var importShipment = importConsol.Shipments.AddNew();
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			importDeclaration.JE_JS = importShipment.PK;
			CreateConsolTransport(importConsol, "AUSYD", "NZAKL", Core.Constants.TransportModes.Sea, "ADMIRALENGRACHT", "VY123", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, t);
			CreateConsolTransport(importConsol, "NZTRG", "FJSUV", Core.Constants.TransportModes.Sea, "AAL FREMANTLE", "753E", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, t2);

			TranshipmentRequest.Create(importDeclaration);
			var itrRequest = importDeclaration.TranshipmentRequest;
			itrRequest.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertOutgoingCraftDetails(itrRequest, "1", "AAL FREMANTLE", "753E", ZString.Empty, t2, "Should default to outgoing leg of transhipment");
		}

		public void TestSavesOnChanges()
		{
			CombineAssertions(() =>
			{
				var underbond = TranshipmentRequest.Create(Factory.New<JobDeclaration>(), savesOnChanges: false);
				AssertEquals("By default object is normal to be saved", true, underbond.IsSavedByFactory);

				underbond = TranshipmentRequest.Create(Factory.New<JobDeclaration>(), savesOnChanges: true);
				AssertEquals("Object is set not to be saved", false, underbond.IsSavedByFactory);
				Factory.Save();
				AssertEquals("Object is set not to be saved", false, underbond.IsInDatabase);

				underbond.C4_ModeOfMovement = "A";
				AssertEquals("Once changed, object should be saved", true, underbond.IsSavedByFactory);
				Factory.Save();
				AssertEquals("Changes are saved", "A", Factory.CreateNewFactory().Load<TranshipmentRequest>(underbond.PK).C4_ModeOfMovement);

				underbond = TranshipmentRequest.Create(Factory.New<Express.CusHAWB>(), savesOnChanges: true);
				Factory.Save();
				AssertEquals("Parent CusHAWB is saved", true, underbond.Parent.IsInDatabase);
				AssertEquals("Transhipment Reqeust is not saved initially", false, underbond.IsInDatabase);

				underbond = TranshipmentRequest.Create(Factory.New<CusSCAHouse>(), savesOnChanges: true);
				Factory.Save();
				AssertEquals("Parent CusSCAHouse is saved", true, underbond.Parent.IsInDatabase);
				AssertEquals("Transhipment Reqeust is not saved initially", false, underbond.IsInDatabase);

				underbond = TranshipmentRequest.Create(Factory.New<Express.CusMAWB>(), savesOnChanges: true);
				Factory.Save();
				AssertEquals("Parent CusMAWB is saved", true, underbond.Parent.IsInDatabase);
				AssertEquals("Transhipment Reqeust is not saved initially", false, underbond.IsInDatabase);

				var oceanbill = Factory.New<CusSCAOceanBill>();
				var container = oceanbill.Containers.AddNew();
				underbond = TranshipmentRequest.Create(container, savesOnChanges: true);
				Factory.Save();
				AssertEquals("Parent CusSCAContainer is saved", true, underbond.Parent.IsInDatabase);
				AssertEquals("Transhipment Reqeust is not saved initially", false, underbond.IsInDatabase);
			});
		}

		public void TestCusStorageDocPivotInterfaces()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var transhipmentRequest = container.ContainerDTRs.AddNew();
			var eDoc1 = oceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "First.pdf", "CIV");
			var eDoc2 = oceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Second.pdf", "CIV");

			CombineAssertions(() =>
			{
				var pivotParent = (ICusStorageDocPivotParent)transhipmentRequest;
				AssertNotNull("EDocPivotCollection is not null", pivotParent.EDocPivotCollection);
				AssertEquals("There is 1 EDocCollections", 1, pivotParent.EDocCollections.Count());
				Assert("ICusStorageDocPivotParent method edoc1", pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
				Assert("ICusStorageDocPivotParent method edoc2", pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));

				var newPivot = pivotParent.EDocPivotCollection.AddNew();
				newPivot.CSD_DocType = "T1";
				Factory.Save();

				var loadedPivot = new BusinessObjectFactory().Load<BaseCusStorageDocPivot>(newPivot.PK);
				AssertType("ICusStorageDocPivotTypeSupporter method, should have returned the correct type", typeof(CusStorageDocPivot), loadedPivot);
			});
		}

		void CreatePopulatedTransport(JobDeclaration declaration, string from, string to, ZString transportMode, ZString vessel, ZString voyageFlight, ZDateTime ata, ZDateTime eta, ZDateTime atd, ZDateTime etd)
		{
			var transport = declaration.Transports.AddNew(from, to);
			transport.JW_TransportMode = transportMode;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			if (atd == ZDateTime.Empty && etd == ZDateTime.Empty)
			{
				transport.JW_ATA = ata;
				transport.JW_ETA = eta;
			}
			else
			{
				transport.JW_ATD = atd;
				transport.JW_ETD = etd;
			}
		}

		void CreateShipmentTransport(CommonShipment shipment, string from, string to, ZString transportMode, ZString vessel, ZString voyageFlight, ZDateTime ata, ZDateTime eta, ZDateTime atd, ZDateTime etd)
		{
			var transport = shipment.Transports.AddNew(from, to);
			transport.JW_TransportMode = transportMode;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			if (atd == ZDateTime.Empty && etd == ZDateTime.Empty)
			{
				transport.JW_ATA = ata;
				transport.JW_ETA = eta;
			}
			else
			{
				transport.JW_ATD = atd;
				transport.JW_ETD = etd;
			}
		}

		void CreateConsolTransport(ForwardingConsol consol, string from, string to, ZString transportMode, ZString vessel, ZString voyageFlight, ZDateTime ata, ZDateTime eta, ZDateTime atd, ZDateTime etd)
		{
			var transport = consol.Transports.AddNew(from, to);
			transport.JW_TransportMode = transportMode;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			if (atd == ZDateTime.Empty && etd == ZDateTime.Empty)
			{
				transport.JW_ATA = ata;
				transport.JW_ETA = eta;
			}
			else
			{
				transport.JW_ATD = atd;
				transport.JW_ETD = etd;
			}
		}

		void AssertIncomingCraftDetails(TranshipmentRequest request, ZString incomingTransportMode, ZString vessel, ZString voyage, ZString flight, ZDateTime eta, string message)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Incoming Transport Mode", incomingTransportMode, request.C4_TranshipModeOfMovement);
				AssertEquals("Vessel", vessel, request.C4_TranshipBySeaVessel);
				AssertEquals("Voyage", voyage, request.C4_TranshipBySeaVoyage);
				AssertEquals("Flight", flight, request.C4_FlightNo);
				AssertEquals("Arrival Date", eta, request.C4_ArrivalDate);
			});
		}

		void AssertOutgoingCraftDetails(TranshipmentRequest request, ZString outgoingTransportMode, ZString vessel, ZString voyage, ZString flight, ZDateTime etd, string message)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Outgoing Transport Mode", outgoingTransportMode, request.C4_TranshipModeOfMovement);
				AssertEquals("Vessel", vessel, request.C4_TranshipBySeaVessel);
				AssertEquals("Voyage", voyage, request.C4_TranshipBySeaVoyage);
				AssertEquals("Flight", flight, request.C4_FlightNo);
				AssertEquals("Departure Date", etd, request.C4_TranshipDepartureDate);
			});
		}

		void ClearIncomingCraftDetails(TranshipmentRequest request)
		{
			request.C4_TranshipModeOfMovement = ZString.Empty;
			request.C4_TranshipBySeaVessel = ZString.Empty;
			request.C4_TranshipBySeaVoyage = ZString.Empty;
			request.C4_FlightNo = ZString.Empty;
			request.C4_ArrivalDate = ZDateTime.Empty;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => Factory.New<TranshipmentRequest>();
	}
}
