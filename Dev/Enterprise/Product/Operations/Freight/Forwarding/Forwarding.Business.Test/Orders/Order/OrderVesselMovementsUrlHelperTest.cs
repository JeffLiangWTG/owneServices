using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderVesselMovementsUrlHelperTest : TestCaseWithFactory
	{
		public void TestNotSeaTransport()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Air;

			var helper = new OrderVesselMovementsUrlHelper(order);
			var (model, errorMessage) = helper.GetVesselMovementsUrlModel();

			CombineAssertions(() =>
			{
				AssertNull(model);
				AssertEquals("Transport Mode must be SEA.", errorMessage);
			});
		}

		public void TestOneVoyage()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			SetUpDepartureVessel(order, "1234567", "4321", "DepVoyage", "AUSYD", "USLAX");

			order.JD_Milestone_E_DEP = new ZDate(2020, 4, 5);
			order.JD_Milestone_E_ARV = new ZDate(2020, 4, 18);

			var helper = new OrderVesselMovementsUrlHelper(order);
			var (model, errorMessage) = helper.GetVesselMovementsUrlModel();

			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "1234567",
				DepartureTime = new ZDate(2020, 4, 5),
				ArrivalTime = new ZDate(2020, 4, 18),
				CarrierCode = "4321",
				VoyageNumber = "DepVoyage",
				DeparturePortUnloco = "AUSYD",
				ArrivalPortUnloco = "USLAX",
			};

			CombineAssertions(() =>
			{
				AssertEquals(expectedModel, model);
				AssertNull(errorMessage);
			});
		}

		public void TestOneVoyage_ActualDatesPreferred()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			SetUpDepartureVessel(order, "1234567", "4321", "DepVoyage", "AUSYD", "USLAX");

			order.JD_Milestone_E_DEP = new ZDate(2020, 4, 5);
			order.JD_Milestone_A_DEP = new ZDate(2020, 4, 6);
			order.JD_Milestone_E_ARV = new ZDate(2020, 4, 18);
			order.JD_Milestone_A_ARV = new ZDate(2020, 4, 19);

			var helper = new OrderVesselMovementsUrlHelper(order);
			var (model, errorMessage) = helper.GetVesselMovementsUrlModel();

			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "1234567",
				DepartureTime = new ZDate(2020, 4, 6),
				ArrivalTime = new ZDate(2020, 4, 19),
				CarrierCode = "4321",
				VoyageNumber = "DepVoyage",
				DeparturePortUnloco = "AUSYD",
				ArrivalPortUnloco = "USLAX",
			};

			CombineAssertions(() =>
			{
				AssertEquals(expectedModel, model);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2020, 4, 4)]
		public void TestAllVoyages_DateBeforeDepartureVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "1234567",
				DepartureTime = new ZDate(2020, 4, 5),
				ArrivalTime = new ZDate(2020, 4, 9),
				CarrierCode = "4321",
				VoyageNumber = "DepVoyage",
				DeparturePortUnloco = "AUSYD",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		[TestDate(2020, 4, 7)]
		public void TestAllVoyages_DateDuringDepartureVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "1234567",
				DepartureTime = new ZDate(2020, 4, 5),
				ArrivalTime = new ZDate(2020, 4, 9),
				CarrierCode = "4321",
				VoyageNumber = "DepVoyage",
				DeparturePortUnloco = "AUSYD",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		[TestDate(2020, 4, 10)]
		public void TestAllVoyages_DateBeforeIntermediateVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "2345678",
				DepartureTime = new ZDate(2020, 4, 11),
				ArrivalTime = new ZDate(2020, 4, 16),
				CarrierCode = "5432",
				VoyageNumber = "IntVoyage",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		[TestDate(2020, 4, 14)]
		public void TestAllVoyages_DateDuringIntermediateVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "2345678",
				DepartureTime = new ZDate(2020, 4, 11),
				ArrivalTime = new ZDate(2020, 4, 16),
				CarrierCode = "5432",
				VoyageNumber = "IntVoyage",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		[TestDate(2020, 4, 17)]
		public void TestAllVoyages_DateBeforeArrivalVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "3456789",
				DepartureTime = new ZDate(2020, 4, 18),
				ArrivalTime = new ZDate(2020, 4, 24),
				CarrierCode = "6543",
				VoyageNumber = "ArrVoyage",
				ArrivalPortUnloco = "USLAX",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		[TestDate(2020, 4, 21)]
		public void TestAllVoyages_DateDuringArrivalVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "3456789",
				DepartureTime = new ZDate(2020, 4, 18),
				ArrivalTime = new ZDate(2020, 4, 24),
				CarrierCode = "6543",
				VoyageNumber = "ArrVoyage",
				ArrivalPortUnloco = "USLAX",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		[TestDate(2020, 4, 26)]
		public void TestAllVoyages_DateAfterArrivalVoyage()
		{
			var expectedModel = new VesselMovementsUrlModel
			{
				LloydsNumber = "3456789",
				DepartureTime = new ZDate(2020, 4, 18),
				ArrivalTime = new ZDate(2020, 4, 24),
				CarrierCode = "6543",
				VoyageNumber = "ArrVoyage",
				ArrivalPortUnloco = "USLAX",
			};

			AssertModelWithAllVoyages(expectedModel);
		}

		void AssertModelWithAllVoyages(VesselMovementsUrlModel expectedModel)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			SetUpDepartureVessel(order, "1234567", "4321", "DepVoyage", "AUSYD", null);
			SetUpIntermediateVessel(order, "2345678", "5432", "IntVoyage");
			SetUpArrivalVessel(order, "3456789", "6543", "ArrVoyage", "USLAX");

			order.JD_Milestone_A_DEP = new ZDate(2020, 4, 5);
			order.JD_E_ARV_1stIntermediate = new ZDate(2020, 4, 9);
			order.JD_E_DEP_2 = new ZDate(2020, 4, 11);
			order.JD_E_ARV_2ndIntermediate = new ZDate(2020, 4, 16);
			order.JD_E_DEP_3 = new ZDate(2020, 4, 18);
			order.JD_Milestone_A_ARV = new ZDate(2020, 4, 24);

			var helper = new OrderVesselMovementsUrlHelper(order);
			var (model, errorMessage) = helper.GetVesselMovementsUrlModel();

			CombineAssertions(() =>
			{
				AssertEquals(expectedModel, model);
				AssertNull(errorMessage);
			});
		}

		void SetUpDepartureVessel(Order order, string lloydsNumber, string carrierCode, string voyageNumber, string departurePortUnloco, string arrivalPortUnloco)
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = lloydsNumber;
			vessel.RV_CarrierCode = carrierCode;
			order.JD_RV_NKDepartureVessel = vessel.RV_Code;
			order.JD_DepartureVoyage = voyageNumber;

			if (!string.IsNullOrEmpty(departurePortUnloco))
			{
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "SUPPLIER";
				supplier.OH_RL_NKClosestPort = departurePortUnloco;
				order.SupplierPK = supplier.PK;
			}

			if (!string.IsNullOrEmpty(arrivalPortUnloco))
			{
				var buyer = Factory.New<OrgHeader>();
				buyer.OH_Code = "BUYER";
				buyer.OH_RL_NKClosestPort = arrivalPortUnloco;
				order.BuyerPK = buyer.PK;
			}
		}

		void SetUpIntermediateVessel(Order order, string lloydsNumber, string carrierCode, string voyageNumber)
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = lloydsNumber;
			vessel.RV_CarrierCode = carrierCode;
			order.JD_RV_NKIntermediateVessel = vessel.RV_Code;
			order.JD_IntermediateVoyage = voyageNumber;
		}

		void SetUpArrivalVessel(Order order, string lloydsNumber, string carrierCode, string voyageNumber, string arrivalPortUnloco)
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = lloydsNumber;
			vessel.RV_CarrierCode = carrierCode;
			order.JD_RV_NKArrivalVessel = vessel.RV_Code;
			order.JD_ArrivalVoyage = voyageNumber;

			if (!string.IsNullOrEmpty(arrivalPortUnloco))
			{
				var buyer = Factory.New<OrgHeader>();
				buyer.OH_Code = "BUYER";
				buyer.OH_RL_NKClosestPort = arrivalPortUnloco;
				order.BuyerPK = buyer.PK;
			}
		}
	}
}
