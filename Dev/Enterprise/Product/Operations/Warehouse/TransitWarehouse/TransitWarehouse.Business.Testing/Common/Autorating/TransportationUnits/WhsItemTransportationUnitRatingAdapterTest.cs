using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class WhsItemTransportationUnitRatingAdapterTest<T> : WhsTransitTestCaseWithFactory
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitTransportationUnitForRating,
		ITransitJobInvoicingPlugIn
	{
		#region TestAdapterType

		public void TestAdapterType()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(true, true);
			var jobHeader = new JobHeader.Loader(transportationUnit).TryLoadOrCreate();
			var billingPartyOrg = new DebtorOrg(transportationUnit.JobHeader.LocalCharges, RatingDebtorOrgTypes.LC);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(AdapterType.TransitWarehouse, ratingAdapter.AdapterType);
			AssertEquals(RateType.TransitWarehouseTransportationUnit, ratingAdapter.RateTypeToUse);
			AssertEquals(MergeChargeOptions.WithinAdapter, ratingAdapter.MergeCharges);
			AssertEquals(transportationUnit.ConsumerType, ratingAdapter.ConsumerType);
			AssertContainsExactElementsInAnyOrder(new[] { transportationUnit.ChargeCodeGroup }, ratingAdapter.ChargeCodeGroups);

			AssertEquals(2, ratingAdapter.Creditors.AllOrgs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { transportationUnit.Warehouse.WarehouseAddress.Header, transportationUnit.TransportCompanyDocAddress.Organisation }, ratingAdapter.Creditors.AllOrgs);

			Assert("Should contains Billing Party", ratingAdapter.DebtorOrgs.Contains(billingPartyOrg));
		}

		public void TestCreditors_TransportCompanyIsOverridden()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(true, true, isTransportCompanyOverride: true);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);

			AssertEquals(1, ratingAdapter.Creditors.AllOrgs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { transportationUnit.Warehouse.WarehouseAddress.Header }, ratingAdapter.Creditors.AllOrgs);
		}

		#endregion

		#region TestFreightMode

		public void TestFreightMode_TransportationUnitWithoutRelatedJobs()
		{
			var transportationUnit = Factory.New<T>();
			AssertNull("Percondition", transportationUnit.FreightMode);

			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(FreightMode.UKN, ratingAdapter.FreightMode);
		}

		public void TestFreightMode_NoTransportMode()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(false, false);
			AssertNull("Percondition", transportationUnit.FreightMode);

			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(FreightMode.UKN, ratingAdapter.FreightMode);
		}

		public void TestFreightMode_WithTransportMode()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(false, true);
			AssertNotNull("Percondition", transportationUnit.FreightMode);

			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(FreightMode.AIR, ratingAdapter.FreightMode);
		}

		#endregion

		#region TestContainerMode

		public void TestContainerMode_TransportationUnitIsContainer()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(true, true);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(ContainerModes.FCL, ratingAdapter.ContainerMode);
		}

		public void TestContainerMode_TransportationUnitIsVehicle()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(false, true);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(ContainerModes.FTL, ratingAdapter.ContainerMode);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(false, true);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertEquals(transportationUnit.TransportCompanyDocAddress.Organisation, ratingAdapter.Carrier);
		}

		public void TestCarrier_TransportCompanyIsOverridden()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(false, true, isTransportCompanyOverride: true);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertNull(ratingAdapter.Carrier);
		}

		#endregion

		#region TestConditionsSupporter

		public void TestConditionsSupporter()
		{
			var transportationUnit = GetTransportationUnitWithValidTestData(true, true);
			var ratingAdapter = new WhsItemTransportationUnitRatingAdapter<T>(transportationUnit);
			AssertType(typeof(TransitConditionsSupporter<T>), ratingAdapter.ConditionsSupporter);
		}

		#endregion

		protected abstract T GetTransportationUnitWithValidTestData(bool isContainer, bool areRelatedJobsHaveTransportMode, bool isTransportCompanyOverride = false);

		protected abstract OrgHeader ExpectedCarrier { get; }
	}

	public class WhsItemReceiveTransportationUnitRatingAdapterTest : WhsItemTransportationUnitRatingAdapterTest<WhsItemReceiveTransportationUnit>
	{
		protected override WhsItemReceiveTransportationUnit GetTransportationUnitWithValidTestData(bool isContainer, bool areRelatedJobsHaveTransportMode, bool isTransportCompanyOverride = false)
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			if (areRelatedJobsHaveTransportMode)
			{
				rcn.WRC_TransportMode = TransportModes.Air;
				rcn2.WRC_TransportMode = TransportModes.AirSea;
			}

			WhsItemReceiveTransportationUnit rtu = null;
			if (isContainer)
			{
				rtu = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			}
			else
			{
				rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			}

			var transportCompany = Helper.CreateClient("TRC");
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			if (isTransportCompanyOverride)
			{
				rtu.TransportCompany.E2_AddressOverride = true;
				rtu.TransportCompany.E2_CompanyName = "Transport Co";
			}
			else
			{
				expectedCarrier = transportCompany;
			}

			var localClient = Helper.CreateClient("LCC1");
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			return rtu;
		}

		protected override OrgHeader ExpectedCarrier => expectedCarrier;
		OrgHeader expectedCarrier;
	}

	public class WhsItemDispatchTransportationUnitRatingAdapterTest : WhsItemTransportationUnitRatingAdapterTest<WhsItemDispatchTransportationUnit>
	{
		protected override WhsItemDispatchTransportationUnit GetTransportationUnitWithValidTestData(bool isContainer, bool areRelatedJobsHaveTransportMode, bool isTransportCompanyOverride = false)
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			if (areRelatedJobsHaveTransportMode)
			{
				dll.WDL_TransportMode = TransportModes.Air;
				dll2.WDL_TransportMode = TransportModes.Air;
			}

			WhsItemDispatchTransportationUnit dtu = null;
			if (isContainer)
			{
				dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			}
			else
			{
				dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			}

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			var transportCompany = Helper.CreateClient("TRC");
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			if (isTransportCompanyOverride)
			{
				dtu.TransportCompany.E2_AddressOverride = true;
				dtu.TransportCompany.E2_CompanyName = "Transport Co";
			}
			else
			{
				expectedCarrier = transportCompany;
			}

			var localClient = Helper.CreateClient("LCC1");
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			return dtu;
		}

		protected override OrgHeader ExpectedCarrier => expectedCarrier;
		OrgHeader expectedCarrier;
	}
}
