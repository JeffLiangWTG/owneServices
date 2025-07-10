using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbConsignmentRunSheetRatingAdapterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region IAutoRating Members

		#region IAutoRating Members

		#region TestIAutoRating_ChargeCodeGroups

		public void TestIAutoRating_ChargeCodeGroups()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRating.ChargeCodeGroups);
			AssertEquals(ChargeCodeFilter.AutorateNothing, iAutoRating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateConsolLevelOnly, iAutoRating.ChargeCodeGroups.CostChargesFilter);
		}

		#endregion

		#region TestIAutoRating_Properties

		public void TestIAutoRating_Properties()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertEquals(MergeChargeOptions.WithinAdapter, iAutoRating.MergeCharges);
			AssertEquals(RateType.TransportBookings, iAutoRating.RateTypeToUse);
			AssertEquals(15, iAutoRating.JobServices.Count);
			AssertEquals(null, iAutoRating.ConsumerType);
			AssertEquals(AdapterType.RunSheet, iAutoRating.AdapterType);
			AssertEquals(runSheet.KG_RunSheetNumber, iAutoRating.OperationalJobCode);
		}

		#endregion

		#region TestIAutoRating_StatusInformation

		public void TestIAutoRating_StatusInformation()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			var expectedErrorMessage = string.Format("{0} doesn't have at least 2 Instructions. Cannot continue.", runSheet.HumanReadableName);
			var info = iAutoRating.StatusInformation;
			AssertEquals("Precondition", false, info.CanExecute);
			AssertEquals("Precondition", expectedErrorMessage, info.Message);

			var populatedRunSheet = GetPopulatedRunSheet();
			var iPopulatedAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(populatedRunSheet);
			var populatedInfo = iPopulatedAutoRating.StatusInformation;
			AssertEquals(true, populatedInfo.CanExecute);
			AssertEquals("", populatedInfo.Message);
		}

		#endregion

		#region TestIAutoRatingOrganisations_ServiceLevel

		public void TestIAutoRatingOrganisations_ServiceLevel()
		{
			var runSheet = GetRunSheet();
			var adapter = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);

			AssertEquals(1, adapter.ServiceLevel.ServiceLevelData.Length);
			adapter.ServiceLevel.ServiceLevelData.Single(
				s => s.ServiceLevelType == ServiceLevelType.Carrier && s.ServiceLevel == "");

			runSheet.KG_PL_NKCarrierServiceLevel = "SVC";
			adapter.ServiceLevel.ServiceLevelData.Single(
				s => s.ServiceLevelType == ServiceLevelType.Carrier && s.ServiceLevel == "SVC");
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

		#region TestIAutoRatingLocations_Origin

		public void TestIAutoRatingLocations_Origin()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertNull("Precondition", iAutoRating.Origin);

			var populatedRunSheet = GetPopulatedRunSheet();
			var iPopulatedAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(populatedRunSheet);
			AssertEquals("AUMEL", iPopulatedAutoRating.Origin.Code);

			var firstAddress = populatedRunSheet.RunSheetInstructions[0].Address;
			firstAddress.E2_AddressOverride = true;
			firstAddress.E2_RN_NKCountryCode = "NZ";
			AssertEquals("NZ", iPopulatedAutoRating.Origin.Code);
		}

		#endregion

		#region TestIAutoRatingLocations_Properties

		public void TestIAutoRatingLocations_Properties()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertNull(iAutoRating.Destination);
			AssertNull(iAutoRating.GetVia(CostSell.Cost));
			AssertNull(iAutoRating.GetVia(CostSell.Revenue));
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		#region TestAutoRatingOrganisations_Carrier

		public void TestIAutoRatingOrganisations_Carrier()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertNull("Precondition.", iAutoRating.Carrier);

			var transportCo = Helper.CreateOrganisation("AAA");
			runSheet.KG_OH_TransportCo = transportCo.PK;
			AssertEquals(transportCo, iAutoRating.Carrier);
		}

		#endregion

		#region TestAutoRatingOrganisations_Properties

		public void TestAutoRatingOrganisations_Properties()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
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
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertEquals("Precondition.", 0, iAutoRating.Creditors.AllOrgs.Count);

			var transportCo = Helper.CreateOrganisation("AAA");
			runSheet.KG_OH_TransportCo = transportCo.PK;
			AssertContainsExactElementsInAnyOrder(new[] { transportCo }, iAutoRating.Creditors.AllOrgs);
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region TestIAutoRatingFreightInfo_Properties

		public void TestIAutoRatingFreightInfo_Properties()
		{
			var runSheet = GetRunSheet();
			runSheet.KG_StartTime = new ZDateTimeOffset(new ZDateTime(2015, 05, 08, 09, 00, 00), DateTimeKind.Local);
			runSheet.KG_EndTime = new ZDateTimeOffset(new ZDateTime(2015, 05, 08, 11, 00, 00), DateTimeKind.Local);

			var ratingAdapter = new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertEquals(FreightMode.NonContainerised, ratingAdapter.FreightMode);
			AssertEquals("", ratingAdapter.HousebillReleaseType);
			AssertNull(ratingAdapter.PaymentTerm);
			AssertEquals(2, ((RateableMeasureSet)ratingAdapter.RateableMeasures).Time.Span.Hours);

			runSheet.KG_StartTime = ZDateTimeOffset.Empty;
			runSheet.KG_EndTime = ZDateTimeOffset.Empty;
			runSheet.KG_Duration = ZDateTime.Empty;

			AssertEquals("Pre-condition", ZDateTime.Empty, runSheet.KG_Duration);
			AssertEquals(TimeSpan.Zero, ((RateableMeasureSet)ratingAdapter.RateableMeasures).Time.Span);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures

		#region TestFreightMode

		public void TestFreightMode()
		{
			var runSheet = GetRunSheet();
			runSheet.KG_TransportMode = TransportModes.Rail;
			runSheet.KG_ContainerMode = ContainerModes.FCL;
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertEquals(FreightMode.RAI | FreightMode.Containerised, iAutoRating.FreightMode);
		}

		#endregion

		#endregion

		#endregion

		#region IImportExport Members

		public void TestIImportExport_IsImport()
		{
			var runSheet = GetRunSheet();
			var iAutoRating = (IAutoRating)new DtbConsignmentRunSheetRatingAdapter(runSheet);
			AssertEquals(Directions.Unknown, iAutoRating.JobDirection);
		}

		#endregion

		#endregion

		#region Implementation

		#region GetRunSheet

		DtbConsignmentRunSheet GetRunSheet()
		{
			return Helper.CreateRunSheet();
		}

		DtbConsignmentRunSheet GetPopulatedRunSheet()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runsheet = Helper.CreateRunSheet();
			runsheet.AddNewRunSheetInstructions(new[] { consignment1.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			runsheet.AddNewRunSheetInstructions(new[] { consignment2.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			return runsheet;
		}

		#endregion

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}
		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}
}
