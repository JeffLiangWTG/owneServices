using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class LinehaulAndRunSheetRatingAdapterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region IAutoRating Members

		#region IAutoRating Members

		#region TestIAutoRating_ChargeCodeGroups

		public void TestIAutoRating_ChargeCodeGroups()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var iAutoRating = (IAutoRating)new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRating.ChargeCodeGroups);
			AssertEquals(ChargeCodeFilter.AutorateNothing, iAutoRating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateConsolLevelOnly, iAutoRating.ChargeCodeGroups.CostChargesFilter);
		}

		#endregion

		#region TestIAutoRating_Properties

		public void TestIAutoRating_Properties()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var iAutoRating = (IAutoRating)new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			AssertEquals(MergeChargeOptions.WithinAdapter, iAutoRating.MergeCharges);
			AssertEquals(RateType.TransportBookings, iAutoRating.RateTypeToUse);
			AssertEquals(0, iAutoRating.JobServices.Count);
			AssertEquals(null, iAutoRating.ConsumerType);
			AssertEquals(AdapterType.RunSheet, iAutoRating.AdapterType);
			AssertEquals(dummy.Z0_Code, iAutoRating.OperationalJobCode);
		}

		#endregion

		#region TestIAutoRating_StatusInformation

		public void TestIAutoRating_StatusInformation()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var iAutoRating = (IAutoRating)new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			AssertEquals(true, iAutoRating.StatusInformation.CanExecute);
			AssertEquals("", iAutoRating.StatusInformation.Message);
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

		#region TestIAutoRatingLocations_Properties

		public void TestIAutoRatingLocations_Properties()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var adapter = new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			AssertNull(adapter.Origin);
			AssertNull(adapter.Destination);
			AssertNull(adapter.GetVia(CostSell.Cost));
			AssertNull(adapter.GetVia(CostSell.Revenue));
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		#region TestAutoRatingOrganisations_Properties

		public void TestAutoRatingOrganisations_Properties()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var iAutoRating = new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			AssertNull(iAutoRating.Carrier);
			AssertNull(iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull(iAutoRating.PickupAddress);
			AssertEquals("", iAutoRating.PickupCartageEquipment);
			AssertNull(iAutoRating.DeliveryAddress);
			AssertNull(iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertEquals("", iAutoRating.DeliveryCartageEquipment);
			AssertNull(iAutoRating.WharfCTOAddress);
		}

		#endregion

		#region TestIAutoRatingOrganisations_TransportProviders

		public void TestIAutoRatingOrganisations_TransportProviders()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var adapter = new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			var iAutoRating = (IAutoRating)adapter;
			AssertEquals("Precondition.", 0, iAutoRating.Creditors.AllOrgs.Count);

			var transportCo = Helper.CreateOrganisation("AAA");
			adapter.CarrierToReturn = transportCo;
			AssertContainsExactElementsInAnyOrder(new[] { transportCo }, iAutoRating.Creditors.AllOrgs);
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region TestIAutoRatingFreightInfo_Properties

		public void TestIAutoRatingFreightInfo_Properties()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var adapter = new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			adapter.PackagesToReturn = System.Array.Empty<PkgPackage>();

			AssertEquals(FreightMode.FRO, adapter.FreightMode);
			AssertEquals("", adapter.HousebillReleaseType);
			AssertNull(adapter.PaymentTerm);
			AssertEquals(13, ((RateableMeasureSet)adapter.RateableMeasures).Time.Span.Minutes);
		}

		#endregion

		#endregion

		#region IImportExport Members

		public void TestIImportExport_IsImport()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var iAutoRating = (IAutoRating)new DummyLinehaulAndRunSheetRatingAdapter(dummy);
			AssertEquals(Directions.Unknown, iAutoRating.JobDirection);
		}

		#endregion

		#endregion
	}
}
