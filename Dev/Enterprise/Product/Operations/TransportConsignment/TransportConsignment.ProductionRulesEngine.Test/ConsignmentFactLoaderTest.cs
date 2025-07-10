using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.ProductionRules.Business.LandTransport;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine.Testing
{
	public class ConsignmentFactLoaderTest : TestCaseWithFactory
	{
		public void Test_LoadFacts_ShouldCreateShipmentFact_WhenParentOrderIsForwardingShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
			transportBooking.ConsolidationSingleJob.KB_ParentTableCode = ShipmentJobType;

			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);
			AssertNotNull(consignmentFact.ParentForwardingShipment.Fact);
			AssertNull(consignmentFact.ParentWarehouseOrder.Fact);
		}

		public void Test_LoadFacts_ShouldCreateWarehouseFact_WhenParentOrderIsWarehouseOrder()
		{
			var warehouseOrder = Factory.NewWithValidTestData<WhsOrder>();
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.ConsolidationSingleJob.KB_ParentID = warehouseOrder.PK;
			transportBooking.ConsolidationSingleJob.KB_ParentTableCode = WarehouseJobType;

			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);
			AssertNull(consignmentFact.ParentForwardingShipment.Fact);
			AssertNotNull(consignmentFact.ParentWarehouseOrder.Fact);
		}

		public void Test_LoadFacts_ShouldNotCreateShipmentFactOrWarehouseFact_WhenParentOrderDoesNotExist()
		{
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);
			AssertNull(consignmentFact.ParentForwardingShipment.Fact);
			AssertNull(consignmentFact.ParentWarehouseOrder.Fact);
		}

		public void Test_LoadFact_ShouldNotCreateShipmentFactOrWarehouseFact_WhenParentOrderIsNotWarehouseOrderOrShipment()
		{
			var shipment = Factory.NewWithValidTestData<WhsReceive>();
			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "WD";
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.KM_KB_Booking = bookingConsolidation.PK;
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);
			AssertNull(consignmentFact.ParentForwardingShipment.Fact);
			AssertNull(consignmentFact.ParentWarehouseOrder.Fact);
		}

		public void Test_LoadFacts_ShouldLoadCorrectBranchAndDeptAndCompany()
		{
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);
			AssertEquals(BranchCode, consignmentFact.CurrentBranch.Fact.Code);
			AssertEquals(DepartmentCode, consignmentFact.CurrentDepartment.Fact.Code);
			AssertEquals(CompanyCountryCode, consignmentFact.CurrentCompanyCountry);
			AssertNull(consignmentFact.LocalClient.Fact);
			AssertNull(consignmentFact.ParentForwardingShipment.Fact);
			AssertNull(consignmentFact.ParentWarehouseOrder.Fact);
		}

		public void Test_LoadFacts_ShouldLoadCorrectLocalClient()
		{
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;
			job.JH_ParentID = consignment.PK;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);

			var localClient = consignmentFact.LocalClient.Fact;
			AssertNotNull(localClient);
			AssertEquals(orgHeader.PK.ToGuid(), localClient.PK);
			AssertEquals(orgHeader.OH_Code, localClient.Code);

			var localClientAddress = localClient.MainAddress.Fact;
			AssertNotNull(localClientAddress);
			AssertEquals(orgHeader.MainAddress.PK.ToGuid(), localClientAddress.PK);

			var localClientCountry = localClientAddress.Country.Fact;
			AssertNotNull(localClientCountry);
			AssertEquals(country.Code, localClientCountry.Code);
			AssertEquals(country.RN_EconomicGrouping, localClientCountry.EconomicGrouping);
		}

		public void Test_LoadFacts_ShouldLoadCorrectWarehouse()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var warehouseOrder = Factory.NewWithValidTestData<WhsOrder>();
			warehouseOrder.WD_WW_Whs = warehouse.PK;
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.ConsolidationSingleJob.KB_ParentID = warehouseOrder.PK;
			transportBooking.ConsolidationSingleJob.KB_ParentTableCode = WarehouseJobType;
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);

			var warehouseWithClientFact = consignmentFact.ParentWarehouseOrder.Fact;
			AssertNotNull(warehouseWithClientFact);

			var warehouseFact = warehouseWithClientFact.Warehouse.Fact;
			AssertNotNull(warehouseFact);
			AssertEquals(warehouse.PK.ToGuid(), warehouseFact.PK);
			AssertEquals(warehouse.WW_WarehouseCode, warehouseFact.Code);
		}

		public void Test_LoadFacts_ShouldLoadShipmentFact()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = ShipmentJobType;

			var origin = Factory.NewWithValidTestData<RefUNLOCO>();
			origin.RL_RN_NKCountryCode = country.Code;

			var destination = Factory.NewWithValidTestData<RefUNLOCO>();
			destination.RL_RN_NKCountryCode = country.Code;
			shipment.JS_RL_NKOrigin = origin.RL_Code;
			shipment.JS_RL_NKDestination = destination.RL_Code;

			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
			transportBooking.ConsolidationSingleJob.KB_ParentTableCode = ShipmentJobType;

			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;

			var consignmentFact = GetConsignmentFact(consignment);

			AssertNotNull(consignmentFact);

			var shipmentFact = consignmentFact.ParentForwardingShipment.Fact;
			AssertNotNull(shipmentFact);
			AssertEquals(shipment.TransportMode, shipmentFact.TransportMode);

			var originFact = shipmentFact.Origin.Fact;
			AssertNotNull(originFact);
			AssertEquals(origin.PK.ToGuid(), originFact.PK);
			AssertEquals(origin.RL_Code, originFact.UNLOCO);

			var originCountryFact = originFact.Country.Fact;
			AssertNotNull(originCountryFact);
			AssertEquals(country.Code, originCountryFact.Code);
			AssertEquals(country.RN_EconomicGrouping, originCountryFact.EconomicGrouping);

			var destinationFact = shipmentFact.Destination.Fact;
			AssertNotNull(destinationFact);
			AssertEquals(destination.PK.ToGuid(), destinationFact.PK);
			AssertEquals(destination.RL_Code, destinationFact.UNLOCO);

			var destinationCountryFact = destinationFact.Country.Fact;
			AssertNotNull(destinationCountryFact);
			AssertEquals(country.RN_Code, destinationCountryFact.Code);
			AssertEquals(country.RN_EconomicGrouping, destinationCountryFact.EconomicGrouping);
		}

		IConsignmentFactLoader GetFactLoader()
		{
			return new ConsignmentFactLoader();
		}

		IConsignmentFact GetConsignmentFact(DtbConsignment consignment)
		{
			var factLoader = GetFactLoader();
			var facts = factLoader.GetFacts(consignment, loginCompany.Object, loginBranch.Object, loginDepartment.Object);

			return (IConsignmentFact)facts.FirstOrDefault();
		}

		protected IEnumerable<IInputFact> GetNestedFacts(IEnumerable<IInputFact> topLevelFacts)
		{
			var finder = new NestedFactsFinder();
			var queue = new Queue<IInputFact>(topLevelFacts);
			var result = new HashSet<IInputFact>();
			while (queue.Count > 0)
			{
				var fact = queue.Dequeue();
				var nestedFacts = finder.GetDirectNestedInputFacts(fact);
				foreach (var nestedFact in nestedFacts)
				{
					if (result.Add(nestedFact.Fact))
					{
						queue.Enqueue(nestedFact.Fact);
					}
				}
			}

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var countryMock = new Mock<ICountry>();
			countryMock.Setup(x => x.Code).Returns(CompanyCountryCode);

			loginBranch = new Mock<IBranch>();
			loginBranch.Setup(b => b.Code).Returns(BranchCode);

			loginCompany = new Mock<ICompany>();
			loginCompany.SetupGet(x => x.Country).Returns(countryMock.Object);
			loginCompany.Setup(x => x.Code).Returns(CompanyCode);

			loginDepartment = new Mock<IDepartment>();
			loginDepartment.Setup(b => b.Code).Returns(DepartmentCode);
		}

		Mock<ICompany> loginCompany;
		Mock<IBranch> loginBranch;
		Mock<IDepartment> loginDepartment;

		const string BranchCode = "BRN";
		const string DepartmentCode = "DEP";
		const string CompanyCode = "COP";
		const string CompanyCountryCode = "AU";
		const string WarehouseJobType = "WD";
		const string ShipmentJobType = "JS";
	}
}
