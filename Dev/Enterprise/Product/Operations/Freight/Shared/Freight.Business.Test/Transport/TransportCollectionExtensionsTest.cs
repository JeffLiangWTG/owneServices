using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportCollectionExtensionsTest : TestCaseWithFactory
	{
		public void RecalculateTransportTypesForAllLegs_FeederLegIsNotMarkedAsMain_WhenNonFeederSeaLegExists()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUNTL", "AUSYD", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			transport1.IsFeeder = true;
			var transport2 = CreateTransportLeg(transportCollection, "AUSYD", "AUMEL", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			transportCollection.UpdateTransportTypes(true);

			CombineAssertions(delegate
			{
				AssertEquals("AUNTL => AUSYD", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUSYD => AUMEL", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
			});
		}

		public void RecalculateTransportTypesForAllLegs_MarksFeederLegAsMain_WhenOnlyOneTransportAndItIsFeeder()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport = CreateTransportLeg(transportCollection, "AUNTL", "AUSYD", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			transport.IsFeeder = true;
			transportCollection.UpdateTransportTypes(true);

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => AUSYD", Constants.TransportPlanningType.MainVessel, transport.JW_TransportType);
			});
		}

		public void RecalculateTransportTypesForAllLegs_MarksFirstInternationalSeaLegAsMain_When_TransportsCollectionsHasOneSeaAndOneNonSeaLegPriorToMain()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUBNE", "AUNTL", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, "AUNTL", "AUSYD", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Rail, "");
			var transport3 = CreateTransportLeg(transportCollection, "AUSYD", "AUMEL", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, "");
			var transport5 = CreateTransportLeg(transportCollection, "SGSIN", "USLAX", today.AddDays(8), today.AddDays(10), Constants.TransportModes.Sea, "");
			transportCollection.UpdateTransportTypes(true);

			CombineAssertions(delegate
			{
				AssertEquals("AUBNE => AUNTL", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUNTL => AUSYD", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("AUSYD => AUMEL", Constants.TransportPlanningType.PreCarriage, transport3.JW_TransportType);
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport4.JW_TransportType);
				AssertEquals("SGSIN => USLAX", Constants.TransportPlanningType.OnForwarding, transport5.JW_TransportType);
			});
		}

		public void RecalculateTransportTypesForAllLegs_MarksEuropeSeaLegAsPreCarrier_When_InternationalSeaLegExists()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "DEABL", "DEHAM", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Rail, "");
			var transport2 = CreateTransportLeg(transportCollection, "DEHAM", "NLRTM", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, "NLRTM", "SGSIN", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, "SGSIN", "AUSYD", today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, "");
			transportCollection.UpdateTransportTypes(true);

			CombineAssertions(delegate
			{
				AssertEquals("DEABL => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => NLRTM", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("NLRTM => SGSIN", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
				AssertEquals("SGSIN => AUSYD", Constants.TransportPlanningType.OnForwarding, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUNTL", "AUSYD", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Rail, "");
			var transport2 = CreateTransportLeg(transportCollection, "AUSYD", "AUMEL", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, "SGSIN", "USLAX", today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => AUSYD", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUSYD => AUMEL", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
				AssertEquals("SGSIN => USLAX", Constants.TransportPlanningType.OnForwarding, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_DomesticTransportsOnly()
		{
			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUNTL", "AUSYD", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Rail, "");
			var transport2 = CreateTransportLeg(transportCollection, "AUSYD", "AUMEL", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, "AUMEL", "AUPER", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, "AUPER", "AUDRW", today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => AUSYD", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUSYD => AUMEL", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("AUMEL => AUPER", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
				AssertEquals("AUPER => AUDRW", Constants.TransportPlanningType.OnForwarding, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_SingleLegWithNoLegType()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUMEL", "AUSYD", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			AssertEquals(Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);
		}

		public void TestTryToSetTransportType_NoSeaLeg()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(1), today.AddDays(2), "", "");
			var transport2 = CreateTransportLeg(transportCollection, "SGSIN", "HKHKG", today.AddDays(3), today.AddDays(4), "", "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => SGSIN", "", transport1.JW_TransportType);
				AssertEquals("SGSIN => HKHKG", "", transport2.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_NoSeaLeg_HasOneMainLeg()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(1), today.AddDays(2), "", Constants.TransportPlanningType.MainVessel);
			var transport2 = CreateTransportLeg(transportCollection, "SGSIN", "HKHKG", today.AddDays(3), today.AddDays(4), "", "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);
				AssertEquals("SGSIN => HKHKG", "", transport2.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_HasOnMainLeg_MainLegIsDomestic()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUSYD", "AUMEL", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, "AUMEL", "NZAKL", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, "NZAKL", "NZTRG", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUSYD => AUMEL", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUMEL => NZAKL", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("NZAKL => NZTRG", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_HasOnMainLeg_MainLegIsDomestic_OnlyEuropeCountry()
		{
			var today = DateTime.Today;
			var portDEHAI = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHEI"));
			var portDEHAM = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBLON = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));

			AssertEquals(true, portDEHAI.IsInEU);
			AssertEquals(true, portDEHAM.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBLON.IsInEU);

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portDEHAI.RL_Code, portDEHAM.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portGBPME.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portGBLON.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("DEHEI => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => GBPME", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("GBPME => GBLON", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_HasOneMainLeg_MainLegIsInEuropeDifferentCountryCode()
		{
			var portDEHAI = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHEI"));
			var portDEHAM = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portSGSIN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));

			AssertEquals(true, portDEHAI.IsInEU);
			AssertEquals(true, portDEHAM.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(false, portSGSIN.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portDEHAI.RL_Code, portDEHAM.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portSGSIN.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portSGSIN.RL_Code, portDEHAM.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portGBPME.RL_Code, today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("DEHEI => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHEI => SGSIN", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("SGSIN => DEHAM", Constants.TransportPlanningType.PreCarriage, transport3.JW_TransportType);
				AssertEquals("DEHEI => GBPME", Constants.TransportPlanningType.MainVessel, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_HasOneMainLeg_MainLegIsInEuropeSameCountryCode()
		{
			var portDEHAI = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHEI"));
			var portDEHAM = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
			var portSGSIN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));

			AssertEquals(true, portDEHAI.IsInEU);
			AssertEquals(true, portDEHAM.IsInEU);
			AssertEquals(false, portSGSIN.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portDEHAI.RL_Code, portDEHAM.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portSGSIN.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portSGSIN.RL_Code, portDEHAM.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portDEHAI.RL_Code, today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("DEHEI => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => SGSIN", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("SGSIN => DEHAM", Constants.TransportPlanningType.PreCarriage, transport3.JW_TransportType);
				AssertEquals("DEHAM => DEHEI", Constants.TransportPlanningType.MainVessel, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_NoSeaLeg_HasMultipleMainLeg()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(1), today.AddDays(2), "", Constants.TransportPlanningType.MainVessel);
			var transport2 = CreateTransportLeg(transportCollection, "SGSIN", "HKHKG", today.AddDays(3), today.AddDays(4), "", Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);
				AssertEquals("SGSIN => HKHKG", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithNoLegType_HasInternationalLegOnly()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, "SGSIN", "HKHKG", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);
				AssertEquals("SGSIN => HKHKG", Constants.TransportPlanningType.OnForwarding, transport2.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleMainLegs()
		{
			var today = DateTime.Today;

			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "SESOE", "DEHAM", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, Constants.TransportPlanningType.PreCarriage);
			var transport2 = CreateTransportLeg(transportCollection, "DEHAM", "DEHAM", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.PreCarriage);
			var transport3 = CreateTransportLeg(transportCollection, "DEHAM", "MYPKG", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport4 = CreateTransportLeg(transportCollection, "MYPKG", "SGSIN", today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport5 = CreateTransportLeg(transportCollection, "SGSIN", "THBKK", today.AddDays(9), today.AddDays(10), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("SESOE => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => DEHAM", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("DEHAM => MYPKG", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
				AssertEquals("MYPKG => SGSIN", Constants.TransportPlanningType.OnForwarding, transport4.JW_TransportType);
				AssertEquals("SGSIN => THBKK", Constants.TransportPlanningType.OnForwarding, transport5.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithNoLegType_HasEuropeAndInternationalLeg()
		{
			var portDEHAI = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHEI"));
			var portDEHAM = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portSGSIN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));
			var portAUSYD = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));

			AssertEquals(true, portDEHAI.IsInEU);
			AssertEquals(true, portDEHAM.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(false, portSGSIN.IsInEU);
			AssertEquals(false, portAUSYD.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portDEHAI.RL_Code, portDEHAM.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Rail, "");
			var transport2 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portGBPME.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portSGSIN.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			var transport4 = CreateTransportLeg(transportCollection, portSGSIN.RL_Code, portAUSYD.RL_Code, today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("DEHEI => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => GBPME", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("GBPME => SGSIN", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
				AssertEquals("SGSIN => AUSYD", Constants.TransportPlanningType.OnForwarding, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithNoLegType_HasEuropeLegOnly_DifferentCountryCode()
		{
			var portDEHAI = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHEI"));
			var portDEHAM = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBLON = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));

			AssertEquals(true, portDEHAI.IsInEU);
			AssertEquals(true, portDEHAM.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBLON.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portDEHAI.RL_Code, portDEHAM.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Rail, "");
			var transport2 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portGBPME.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portGBLON.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("DEHEI => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => GBPME", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("GBPME => GBLON", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithNoLegType_HasEuropeLegOnly_SameCountryCode()
		{
			var portGBNCS = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNCS"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBLON = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));
			var portGBNWN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNWN"));

			AssertEquals(true, portGBNCS.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBLON.IsInEU);
			AssertEquals(true, portGBNWN.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portGBNCS.RL_Code, portGBPME.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portGBLON.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portGBLON.RL_Code, portGBNWN.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("GBNCS => GBPME", Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);
				AssertEquals("GBPME => GBLON", Constants.TransportPlanningType.OnForwarding, transport2.JW_TransportType);
				AssertEquals("GBLON => GBNWN", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithSomeHasNoLegType_HasEuropeLegOnly()
		{
			var portGBNCS = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNCS"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBLON = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));
			var portGBNWN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNWN"));

			AssertEquals(true, portGBNCS.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBLON.IsInEU);
			AssertEquals(true, portGBNWN.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portGBNCS.RL_Code, portGBPME.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Rail, Constants.TransportPlanningType.PreCarriage);
			var transport2 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portGBLON.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, "");
			var transport3 = CreateTransportLeg(transportCollection, portGBLON.RL_Code, portGBNWN.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("GBNCS => GBPME", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("GBPME => GBLON", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("GBLON => GBNWN", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithOneMainLeg()
		{
			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUSYD", "AUMEL", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport3 = CreateTransportLeg(transportCollection, "SGSIN", "USLAX", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, "");
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUSYD => AUMEL", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("SGSIN => USLAX", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithMultipleMainLegs_HasInternationalLeg()
		{
			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, "AUSYD", "AUBNE", today.AddDays(1), today.AddDays(2), Constants.TransportModes.Sea, "");
			var transport2 = CreateTransportLeg(transportCollection, "AUBNE", "AUMEL", today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport3 = CreateTransportLeg(transportCollection, "AUMEL", "SGSIN", today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport4 = CreateTransportLeg(transportCollection, "SGSIN", "USLAX", today.AddDays(7), today.AddDays(8), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("AUSYD => AUBNE", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("AUBNE => AUMEL", Constants.TransportPlanningType.PreCarriage, transport2.JW_TransportType);
				AssertEquals("AUMEL => SGSIN", Constants.TransportPlanningType.MainVessel, transport3.JW_TransportType);
				AssertEquals("SGSIN => USLAX", Constants.TransportPlanningType.OnForwarding, transport4.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithMultipleMainLegs_HasEuropeLegOnly_SameCountryCode()
		{
			var portGBNCS = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNCS"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBLON = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));
			var portGBNWN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNWN"));

			AssertEquals(true, portGBNCS.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBLON.IsInEU);
			AssertEquals(true, portGBNWN.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portGBNCS.RL_Code, portGBPME.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Road, Constants.TransportPlanningType.MainVessel);
			var transport2 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portGBLON.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport3 = CreateTransportLeg(transportCollection, portGBLON.RL_Code, portGBNWN.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("GBNCS => GBPME", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("GBPME => GBLON", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("GBLON => GBNWN", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithMultipleMainLegs_HasEuropeLegOnly_DifferentCountryCode()
		{
			var portDEHAI = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHEI"));
			var portDEHAM = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBLON = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));

			AssertEquals(true, portDEHAI.IsInEU);
			AssertEquals(true, portDEHAM.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBLON.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portDEHAI.RL_Code, portDEHAM.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Road, Constants.TransportPlanningType.MainVessel);
			var transport2 = CreateTransportLeg(transportCollection, portDEHAM.RL_Code, portGBPME.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport3 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portGBLON.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("DEHEI => DEHAM", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("DEHAM => GBPME", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("GBPME => GBLON", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		public void TestTryToSetTransportType_MultipleLegsWithMultipleMainLegs_HasEuropeAndInternationLeg()
		{
			var portGBNCS = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNCS"));
			var portGBPME = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBPME"));
			var portGBNWN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBNWN"));
			var portSGSIN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));

			AssertEquals(true, portGBNCS.IsInEU);
			AssertEquals(true, portGBPME.IsInEU);
			AssertEquals(true, portGBNWN.IsInEU);
			AssertEquals(false, portSGSIN.IsInEU);

			var today = DateTime.Today;
			var transportCollection = Factory.New<CommonShipment>().Transports;
			var transport1 = CreateTransportLeg(transportCollection, portGBNCS.RL_Code, portGBPME.RL_Code, today.AddDays(1), today.AddDays(2), Constants.TransportModes.Road, Constants.TransportPlanningType.MainVessel);
			var transport2 = CreateTransportLeg(transportCollection, portGBPME.RL_Code, portSGSIN.RL_Code, today.AddDays(3), today.AddDays(4), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			var transport3 = CreateTransportLeg(transportCollection, portSGSIN.RL_Code, portGBNWN.RL_Code, today.AddDays(5), today.AddDays(6), Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel);
			transportCollection.TryToSetTransportType();

			CombineAssertions(delegate
			{
				AssertEquals("GBNCS => GBPME", Constants.TransportPlanningType.PreCarriage, transport1.JW_TransportType);
				AssertEquals("GBPME => SGSIN", Constants.TransportPlanningType.MainVessel, transport2.JW_TransportType);
				AssertEquals("SGSIN => GBNWN", Constants.TransportPlanningType.OnForwarding, transport3.JW_TransportType);
			});
		}

		Transport CreateTransportLeg(TransportCollection transportCollection, ZString loadPort, ZString discPort, ZDateTime etd, ZDateTime eta, ZString transportMode, ZString transportType)
		{
			var transport = transportCollection.AddNew();
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			transport.JW_TransportMode = transportMode;
			transport.JW_TransportType = transportType;

			return transport;
		}
	}
}
