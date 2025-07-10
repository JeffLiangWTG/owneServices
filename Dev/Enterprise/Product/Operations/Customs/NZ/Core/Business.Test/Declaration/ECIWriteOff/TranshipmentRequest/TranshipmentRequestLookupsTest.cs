using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using NUnit.Framework;

	[TestedType(typeof(TranshipmentRequestLookups))]
	class TranshipmentRequestLookupsTest : Customs.Business.Testing.CusUnderbondLookupsTest
	{
		public void TestTranshipmentRequestModeOfMovementList()
		{
			var transhipmentRequest = Factory.New<TranshipmentRequest>();
			AssertNotEquals(Factory.GetCachedValue<TranshipmentRequestModeOfMovement>(), transhipmentRequest.Lookups.ModeOfMovement);
			AssertNotEquals(Factory.GetCachedValue<TranshipmentRequestModeOfMovement>(), transhipmentRequest.Lookups.TranshipmentModeOfMovement);
		}

		public void TestMovementReasonList()
		{
			var lookups = Factory.New<TranshipmentRequest>().Lookups;
			AssertEquals("MovementReasonList is of correct type", typeof(MovementReason), lookups.MovementReasonList.GetType());
			AssertEquals(true, lookups.MovementReasonList.ContainsCode(MovementReason.Codes.DomesticTranshipmentRequest));
			AssertEquals(true, lookups.MovementReasonList.ContainsCode(MovementReason.Codes.InternationalTranshipmentRequest));
		}

		public void TestVesselList()
		{
			var lookups = Factory.New<TranshipmentRequest>().Lookups;
			AssertEquals("VesselList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.VesselList).IsLoaded);
			AssertEquals("VesselList of correct type", typeof(RefVesselCollection), lookups.VesselList.GetType());
		}

		public void TestRequestModeOfMovementValues()
		{
			var transhipmentRequest = Factory.New<TranshipmentRequest>();
			AssertEquals(true, transhipmentRequest.Lookups.ModeOfMovement.ContainsCode(TranshipmentRequestModeOfMovement.Codes.SeaCV));
			AssertEquals(true, transhipmentRequest.Lookups.ModeOfMovement.ContainsCode(TranshipmentRequestModeOfMovement.Codes.SeaOV));
			AssertNotEquals(true, transhipmentRequest.Lookups.ModeOfMovement.ContainsCode(TranshipmentRequestModeOfMovement.Codes.Sea));
		}

		public void TestRequestTranshipModeOfMovementValues()
		{
			var transhipmentRequest = Factory.New<TranshipmentRequest>();
			AssertNotEquals(true, transhipmentRequest.Lookups.TranshipmentModeOfMovement.ContainsCode(TranshipmentRequestModeOfMovement.Codes.SeaCV));
			AssertNotEquals(true, transhipmentRequest.Lookups.TranshipmentModeOfMovement.ContainsCode(TranshipmentRequestModeOfMovement.Codes.SeaOV));
			AssertEquals(true, transhipmentRequest.Lookups.TranshipmentModeOfMovement.ContainsCode(TranshipmentRequestModeOfMovement.Codes.Sea));
		}

		public void TestTransitDestinationList()
		{
			var lookups = Factory.New<TranshipmentRequest>().Lookups;
			AssertEquals("TransitDestinationList.GetType()", typeof(CTOOrDepotOrWarehouseCollection), lookups.TransitDestinationList.GetType());
		}

		public void TestOriginLocationList()
		{
			var lookups = Factory.New<TranshipmentRequest>().Lookups;
			AssertEquals("The type of OrgHeaderCollection", typeof(OrgHeaderCollection), lookups.OriginLocationList.GetType());
		}

		public void TestCombinedMovementStatusList()
		{
			var lookups = Factory.New<TranshipmentRequest>().Lookups;
			AssertEquals("The type of CombinedMovementStatus", typeof(CombinedMovementStatus), lookups.CombinedMovementStatusList.GetType());
			AssertEquals(true, lookups.CombinedMovementStatusList.ContainsCode(CombinedMovementStatus.Codes.STC));
		}

		protected override CusUnderbond CreateNewUnderbond() => Factory.New<TranshipmentRequest>();
	}
}
