using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentCustomsSupportTest : BaseFreightTest
	{
		public void TestInitialiseGatePassShipmentOnLoaded()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.FillWithValidTestData();
			Factory.Save();
			AssertNull(shipment.statusProvider);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CFSShipment shipment2 = factory2.Load<CFSShipment>(shipment.PK);
			AssertNotNull(shipment2.statusProvider);
		}

		public void TestStatusProvider()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertNull(shipment.statusProvider);
			AssertNotNull(shipment.StatusProvider);
			AssertEquals(shipment.statusProvider, shipment.StatusProvider);
		}

		public void TestIHasStatusProviderStatusProvider()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertNotNull(((IHasStatusProvider)shipment).StatusProvider);
			AssertEquals(shipment.StatusProvider, ((IHasStatusProvider)shipment).StatusProvider);
		}

		public void TestUserFriendlyStatus()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSShipmentStatusProvider.SetDummyForTest(new CFSShipmentStatusProviderDummyObject(shipment));

			AssertEquals("Cuckoo Squeaker of Message Details", shipment.UserFriendlyStatus);
		}

		public void TestCFSShipmentStatusProvider()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			DummyCFSShipmentStatusProvider dummyProvider = new DummyCFSShipmentStatusProvider(shipment);
			CFSShipmentStatusProvider.SetDummyForTest(dummyProvider);

			AssertEquals(typeof(FallbackCFSShipmentStatusProvider), ((IHasStatusProvider)shipment).StatusProvider.GetType());
			dummyProvider.StatusCoreExposed = "foo";
			dummyProvider.ShortStatusExposed = "bar";
			dummyProvider.StatusClassExposed = StatusClass.Underbonded;

			AssertEquals("foo", shipment.JS_GatePassStatus);
			AssertEquals("bar", shipment.JS_GatePassStatusShort);
			AssertEquals(StatusClass.Underbonded, ((IStatusClassProvider)shipment).StatusClass);
			AssertEquals(StatusClass.Underbonded, shipment.JS_GatePassStatusClass);
		}

		#region Test JS_GatePassStatus + JS_GatePassStatusShort

		public void TestJS_GatePassStatus()
		{
			const string clearReference = "CLEAR AP21P";
			const string unpackReference = "UNPACK AP21P";
			const string detainedReference = "DETAINED AP22J";
			const string docClearance = "CLEAR AP21P - DOCUMENTARY CLEARANCE REQUIRED";

			var startTestTime = ZDateTimeOffset.Now;

			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, clearReference, startTestTime.AddMinutes(1));
			AssertEquals("JS_GatePassStatus", clearReference, Shipment.JS_GatePassStatus);

			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, unpackReference, startTestTime.AddMinutes(2));
			AssertEquals("JS_GatePassStatus", clearReference, Shipment.JS_GatePassStatus);

			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, detainedReference, startTestTime.AddMinutes(3));
			AssertEquals("JS_GatePassStatus", detainedReference, Shipment.JS_GatePassStatus);

			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, docClearance, startTestTime.AddMinutes(4));
			AssertEquals("JS_GatePassStatus", docClearance, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_ClearReference()
		{
			const string ClearReference = "CLEAR 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, ClearReference);
			AssertEquals("JS_GatePassStatus", ClearReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_ConditionalClearReference()
		{
			const string ConditionalClearReference = "CONDCLEAR 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, ConditionalClearReference);
			AssertEquals("JS_GatePassStatus", ConditionalClearReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_AcsSeizedReference()
		{
			const string AcsSeizedReference = "ACSSEIZED 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, AcsSeizedReference);
			AssertEquals("JS_GatePassStatus", AcsSeizedReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_AqisSeizedReference()
		{
			const string AqisSeizedReference = "AQISSEIZED 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, AqisSeizedReference);
			AssertEquals("JS_GatePassStatus", AqisSeizedReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_ClearHrmReference()
		{
			const string ClearHrmReference = "CLEARHRM 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, ClearHrmReference);
			AssertEquals("JS_GatePassStatus", ClearHrmReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_HeldReference()
		{
			const string HeldReference = "HELD 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, HeldReference);
			AssertEquals("JS_GatePassStatus", HeldReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_SubUbMovReference()
		{
			const string SubUbMovReference = "SUBUBMOV 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, SubUbMovReference);
			AssertEquals("JS_GatePassStatus", SubUbMovReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_TranshipReference()
		{
			const string TranshipReference = "TRANSHIP 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, TranshipReference);
			AssertEquals("JS_GatePassStatus", TranshipReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_TranshpHrmReference()
		{
			const string TranshpHrmReference = "TRANSHPHRM 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, TranshpHrmReference);
			AssertEquals("JS_GatePassStatus", TranshpHrmReference, Shipment.JS_GatePassStatus);
		}

		public void TestCMRJS_GatePassStatus_TransitReference()
		{
			const string TransitReference = "TRANSIT 9914N";
			Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, TransitReference);
			AssertEquals("JS_GatePassStatus", TransitReference, Shipment.JS_GatePassStatus);
		}

		public void TestJS_Calc_TotalInStockForContainerisedImportShipment()
		{
			GatePassLoadListConsol consol = Factory.New<GatePassLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = ImportSailing.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = ImportSailing.JX_JB_RL_NKPortOfDischarge;
			consol.Transports[0].JW_JX = ImportSailing1.PK;

			GatePassContainer container = consol.Containers.AddNew();

			GatePassShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = HomePort;
			shipment.JS_RL_NKOrigin = OverseasPort;

			GatePassPackLine packLine1 = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine1);
			packLine1.JL_PackageCount = 8;
			packLine1.JL_Outturn = 7;

			GatePassPackLine packLine2 = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine2);
			packLine2.JL_PackageCount = 5;
			packLine2.JL_Outturn = 4;

			AssertEquals("Not delivered", 11, shipment.JS_Calc_TotalInStock);

			EventWatcher watcher = new EventWatcher();
			shipment.JS_Calc_TotalInStockInfo.ValueChanged += watcher.Handler;
			AssertEquals("Adding an event listener should not raise it", 0, watcher.Count);

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			AssertEquals("Adding a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 2, watcher.Count);
			AssertEquals("Fully Delivered", 0, shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.GetDivot(packLine1).J8_PackagesDelivered = 3;
			AssertEquals("Changing J8_PackagesDelivered should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, watcher.Count);
			AssertEquals("Partly Delivered", 4, shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Delete();
			// Uncomment the follwing line when work item I00024653 has been completed
			//AssertEquals("Removing a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, Watcher.Count);
			AssertEquals("Not delivered anymore", 11, shipment.JS_Calc_TotalInStock);

			CommonPickupDeliveryConfirm leg2 = shipment.OriginCFSArrivals.AddNew();
			leg2.GetDivot(packLine1).J8_PackagesDelivered = 6;

			Factory.Save();

			AssertEquals("JS_Calc_TotalInStock should be unaffected by Arrival Divots", 11, shipment.JS_Calc_TotalInStock);
		}

		public void TestJS_GatePassStatusShort()
		{
			var shipment = GatePassShipment.New(Factory);
			AssertEquals("GatePass has no SeaCargoDepot logs, Sea Cargo Status should be empty.", true, shipment.JS_GatePassStatusShort.IsEmpty);

			var now = ZDateTimeOffset.Now;

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, "Clear: some text here", now.AddMinutes(1));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'CLEAR', Sea Cargo Status should be 'CLEAR'.", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, "DETAINED: some text here", now.AddMinutes(2));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'DETAINED', Sea Cargo Status should be 'DETAINED'.", OldCFSShipmentStatusProvider.CMRGatePassStatuses.Detained, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, "Clear: some text here", now.AddMinutes(3));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'CLEAR', Sea Cargo Status should be 'CLEAR'.", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, "A Crappy Log", now.AddMinutes(4));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'CLEAR', Sea Cargo Status should be 'CLEAR'.", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.ClearHrm + " 9914N", now.AddMinutes(5));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'CLEARHRM', Sea Cargo Status should be 'CLEARHRM'.", CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.ConditionalClear + " 9914N", now.AddMinutes(6));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'CONDCLEAR', Sea Cargo Status should be 'CONDCLEAR'.", CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.SubUBMov + " 9914N", now.AddMinutes(7));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'SUBUBMOV', Sea Cargo Status should be 'SUBUBMOV'.", CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, shipment.JS_GatePassStatusShort);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.AcsSeized + " 9914N", now.AddMinutes(8));
			AssertEquals("GatePass' most recent SeaCargoDepot log is 'ACSSEIZED', Sea Cargo Status should be 'ACSSEIZED'.", CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, shipment.JS_GatePassStatusShort);
		}

		#endregion

		public void TestIControllerIDProviderMembers()
		{
			var shipment = Factory.New<CFSShipment>();
			IControllerIDProvider provider = shipment;
			AssertEquals("ControllerID", ControllerIDs.ShipmentReceival, provider.ControllerID);
			AssertEquals("BusinessObjectPK", shipment.PK.ToGuid(), provider.BusinessObjectPK);
		}

		#region Implementation

		protected GatePassShipment Shipment;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<GatePassShipment>();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			GlbBranch.CurrentBranch.GB_Code = "BNE";
			GlbDepartment.CurrentDepartment.GE_Code = "BRN";
		}

		#endregion
	}
}
