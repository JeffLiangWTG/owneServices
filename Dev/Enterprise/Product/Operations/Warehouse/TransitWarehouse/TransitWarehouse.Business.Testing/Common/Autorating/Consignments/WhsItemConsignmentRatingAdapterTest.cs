using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class WhsItemConsignmentRatingAdapterTest<T> : TestCaseWithFactory
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitConsignmentForRating,
		IHaveServices,
		ITransitJobInvoicingPlugIn,
		ITransitJobForConsolCosting
	{
		public void TestAdapterType()
		{
			var consignment = Factory.New<T>();
			consignment[ServiceSchemaColumn] = "STD";
			var ratingAdapter = new WhsItemConsignmentRatingAdapter<T>(consignment);
			AssertEquals(AdapterType.TransitWarehouse, ratingAdapter.AdapterType);
			AssertEquals(RateType.TransitWarehouse, ratingAdapter.RateTypeToUse);
			AssertEquals(MergeChargeOptions.WithinAdapter, ratingAdapter.MergeCharges);
			AssertEquals(consignment.ConsumerType, ratingAdapter.ConsumerType);
			AssertContainsExactElementsInAnyOrder(new[] { ServiceLevelType.Client, ServiceLevelType.Carrier },
				ratingAdapter.ServiceLevel.ServiceLevelData.Select(s => s.ServiceLevelType).ToArray());
			AssertEquals("STD", ratingAdapter.ServiceLevel.ServiceLevelData.Select(s => s.ServiceLevel).Distinct().Single());
			AssertContainsExactElementsInAnyOrder(new[] { consignment.ChargeCodeGroup, ChargeCodeGroupList.Codes.WHSStorage }, ratingAdapter.ChargeCodeGroups);
		}

		protected abstract SchemaStringColumn ServiceSchemaColumn { get; }

		public virtual void TestDebtorOrgs()
		{
			var consignment = CreateConsignmentWithValidData();
			var ratingAdapter = new WhsItemConsignmentRatingAdapter<T>(consignment);

			var consignorOrg = new DebtorOrg(consignment.ConsignorDocAddress.Organisation, RatingDebtorOrgTypes.CNR);
			var consigneeOrg = new DebtorOrg(consignment.ConsigneeDocAddress.Organisation, RatingDebtorOrgTypes.CNE);
			var bookingPartyOrg = new DebtorOrg(consignment.BookingPartyDocAddress.Organisation, RatingDebtorOrgTypes.CCUS);
			Assert("Should contains Consignor", ratingAdapter.DebtorOrgs.Contains(consignorOrg));
			Assert("Should contains Consignee", ratingAdapter.DebtorOrgs.Contains(consigneeOrg));
			Assert("Should contains Booking Party", ratingAdapter.DebtorOrgs.Contains(bookingPartyOrg));

			var jobHeader = new JobHeader.Loader(consignment).TryLoadOrCreate();
			var billingPartyOrg = new DebtorOrg(consignment.JobHeader.LocalCharges, RatingDebtorOrgTypes.LC);
			Assert("Should contains Billing Party", ratingAdapter.DebtorOrgs.Contains(billingPartyOrg));
		}

		public void TestJobService()
		{
			var consignment = CreateConsignmentWithValidData();
			consignment.Services.Add(Helper.CreateJobService(consignment.PK, consignment.TablePrefix, "FUM", ZDateTime.Now));

			var ratingAdapter = new WhsItemConsignmentRatingAdapter<T>(consignment);
			AssertEquals(1, ratingAdapter.JobServices.Count(s => s.ServiceCode == "FUM"));
		}

		public void TestConditionsSupporter()
		{
			var consignment = Factory.New<T>();
			consignment[ServiceSchemaColumn] = "STD";
			var ratingAdapter = new WhsItemConsignmentRatingAdapter<T>(consignment);
			AssertType(typeof(TransitConditionsSupporter<T>), ratingAdapter.ConditionsSupporter);
		}

		protected T CreateConsignmentWithValidData()
		{
			var consignor = Helper.CreateClient("CNR1");
			var consignee = Helper.CreateClient("CNE1");
			var bookingParty = Helper.CreateClient("BKP1");
			var localClient = Helper.CreateClient("LCC1");
			var consignment = Factory.New<T>();

			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.LocalCartageExporter, consignor.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, consignee.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookingParty.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			return consignment;
		}

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}

	public class WhsItemReceiveConsignmentRatingAdapterTest : WhsItemConsignmentRatingAdapterTest<WhsItemReceiveConsignment>
	{
		protected override SchemaStringColumn ServiceSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_RS_NKServiceLevel;
	}

	public class WhsItemDispatchConsignmentRatingAdapterTest : WhsItemConsignmentRatingAdapterTest<WhsItemDispatchConsignment>
	{
		protected override SchemaStringColumn ServiceSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_RS_NKServiceLevel;
	}
}
