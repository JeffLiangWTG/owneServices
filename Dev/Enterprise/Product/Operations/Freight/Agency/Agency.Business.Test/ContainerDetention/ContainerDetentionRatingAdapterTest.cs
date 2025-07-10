using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class ContainerDetentionTest
	{
		public void TestJobDatesProvider()
		{
			var containerDetention = Factory.New<ContainerDetention>();
			var adapter = new ContainerDetentionRatingAdapter(containerDetention);

			AssertType<JobDatesProvider<ContainerDetention>>(adapter.JobDatesProvider);
		}

		public void TestImportBroker()
		{
			var containerDetention = Factory.New<ContainerDetention>();
			var adapter = new ContainerDetentionRatingAdapter(containerDetention);

			AssertNull(adapter.ImportBroker);
		}

		public void TestExportBroker()
		{
			var containerDetention = Factory.New<ContainerDetention>();
			var adapter = new ContainerDetentionRatingAdapter(containerDetention);

			AssertNull(adapter.ExportBroker);
		}

		public void TestAdditionalJobs()
		{
			OrgHeader bneDepot = Factory.NewWithValidTestData<OrgHeader>();
			bneDepot.OH_Code = "Bne Depot";
			bneDepot.OH_RL_NKClosestPort = "AUBNE";

			OrgHeader sydDepot = Factory.NewWithValidTestData<OrgHeader>();
			sydDepot.OH_Code = "Syd Depot";
			sydDepot.OH_RL_NKClosestPort = "AUSYD";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";

			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "FAKE4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "FAKE4100029";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			BillOfLading shipment1 = Factory.New<BillOfLading>();
			shipment1.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE").PK;
			shipment1.JS_RL_NKDestination = "AUBNE";

			BillOfLadingContainer container1 = shipment1.RealContainers.AddNew();
			container1.JC_ContainerNum = stock1.R6_ContainerNum;

			ContainerMovement movement1 = stock1.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_OA_Depot = bneDepot.MainAddress.PK;
			movement1.E9_JV = voyage.PK;

			BillOfLading shipment2 = Factory.New<BillOfLading>();
			shipment2.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUSYD").PK;
			shipment2.JS_RL_NKDestination = "AUSYD";

			BillOfLadingContainer container2 = shipment2.RealContainers.AddNew();
			container2.JC_ContainerNum = stock2.R6_ContainerNum;

			ContainerMovement movement2 = stock2.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement2.E9_OA_Depot = sydDepot.MainAddress.PK;
			movement2.E9_JV = voyage.PK;

			Factory.Save();

			Detention.Movements.Add(movement1);
			Detention.Movements.Add(movement2);

			var nullString = new ZString("null");

			var adapters = Detention.GetRatingAdapters();
			var results = adapters.Select(x => ZString.Format("{0}-{1}", x.Origin?.Code ?? nullString, x.Destination?.Code ?? nullString));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "null-null", "null-AUBNE", "AUSYD-null" }, results);
		}

		public void TestEmptyRatingMembers()
		{
			IAutoRating rating = new ContainerDetentionRatingAdapter(Detention);
			AssertEquals(JobInvoicingConsumerTypes.AgencyDetentionInvoice, rating.ConsumerType);
			AssertEquals(MergeChargeOptions.WithinAdapter, rating.MergeCharges);
			AssertEquals(0, (int)rating.RateTypeToUse);
			AssertEquals(null, rating.Origin);
			AssertEquals(null, rating.Destination);
			AssertEquals(null, rating.GetVia(CostSell.Cost));
			AssertEquals(null, rating.GetVia(CostSell.Revenue));
			AssertEquals(null, rating.Carrier);
			AssertNull(rating.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.CNE]);
			AssertEquals("", rating.DeliveryCartageEquipment);
			AssertEquals(null, rating.DeliveryAddress);
			AssertNull(rating.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.CNR]);
			AssertEquals("", rating.PickupCartageEquipment);
			AssertEquals(null, rating.PickupAddress);
			AssertEquals(null, rating.Creditors);
			AssertEquals(FreightMode.SEA | FreightMode.Containerised, rating.FreightMode);
			AssertEquals("", rating.HousebillReleaseType);
			AssertNull(rating.PaymentTerm);
			AssertEquals(0, ((RateableMeasureSet)rating.RateableMeasures).MeasureTypeCount);
			AssertEquals(null, rating.WharfCTOAddress);
		}

		public void TestStatusInformation()
		{
			IAutoRating rating = new ContainerDetentionRatingAdapter(Detention);
			AssertEquals(false, rating.StatusInformation.CanExecute);
			AssertEquals("There are no containers relating to this detention job.", rating.StatusInformation.Message);

			Detention.Movements.AddNew();
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals("", rating.StatusInformation.Message);
		}

		public void TestImport()
		{
			IAutoRating rating = new ContainerDetentionRatingAdapter(Detention);

			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			AssertEquals(true, rating.IsImport());

			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			AssertEquals(false, rating.IsImport());
		}

		public void TestAdapterTypeAndID()
		{
			IAutoRating adapter = new ContainerDetentionRatingAdapter(Detention);
			AssertEquals(AdapterType.ContainerDetention, adapter.AdapterType);
			AssertEquals(Detention.NC_JobNumber, adapter.OperationalJobCode);
			AssertEquals(Detention.NC_JobNumber, adapter.JobID);
		}
	}
}
