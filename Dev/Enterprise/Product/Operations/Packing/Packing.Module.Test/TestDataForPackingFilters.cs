using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Module.Testing
{
	internal class TestDataForPackingFilters
	{
		public TestDataForPackingFilters(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public void CreateTestData()
		{
			// create 2 picked orders
			Order1 = Factory.New(ObjectFactory.GetType<IWhsOrder>());
			Order1.FillWithValidTestData();
			Order1[WhsDocketSchema.WD_ExternalReference] = "order1";

			Order2 = Factory.New(ObjectFactory.GetType<IWhsOrder>());
			Order2.FillWithValidTestData();
			Order2[WhsDocketSchema.WD_ExternalReference] = "order2";

			Factory.Save(); // Need to save the orders to ensure locations are created and in database first.

			Order1[WhsDocketSchema.WD_DocketStatus] = "DEP";
			Order2[WhsDocketSchema.WD_DocketStatus] = "PIC";

			Pick1 = Factory.New(ObjectFactory.GetType<IWhsPick>());
			Pick1.FillWithValidTestData();
			Order1[WhsDocketSchema.WD_WP] = Pick1.PK;
			Pick1[WhsPickSchema.WP_WW_Whs] = Order1[WhsDocketSchema.WD_WW_Whs];

			Pick2 = Factory.New(ObjectFactory.GetType<IWhsPick>());
			Pick2.FillWithValidTestData();
			Order2[WhsDocketSchema.WD_WP] = Pick2.PK;
			Pick2[WhsPickSchema.WP_WW_Whs] = Order2[WhsDocketSchema.WD_WW_Whs];

			// create receive w/transport booking
			Receive = Factory.New(ObjectFactory.GetType<IWhsReceive>());
			Receive.FillWithValidTestData();
			Receive[WhsDocketSchema.WD_ExternalReference] = "receive";

			TBWithReceive = Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			TBWithReceive[DtbBookingConsolidationSchema.KB_ParentID] = Receive.PK;
			TBWithReceive[DtbBookingConsolidationSchema.KB_ParentTableCode] = WhsDocketSchema.Constants.Prefix;

			// create a standalone transport booking
			TB = Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			TB.FillWithValidTestData();

			// create a shipment w/transport booking
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.FillWithValidTestData();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00000001";

			TBWithShipment = Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			TBWithShipment[DtbBookingConsolidationSchema.KB_ParentID] = shipment.PK;
			TBWithShipment[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobShipmentSchema.Constants.Prefix;

			// create a bill of lading w/transport booking
			var bol = Factory.New(ObjectFactory.GetType<Freight.Integration.Agency.IBillOfLading>());
			bol.FillWithValidTestData();

			TBWithBOL = Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			TBWithBOL[DtbBookingConsolidationSchema.KB_ParentID] = bol.PK;
			TBWithBOL[DtbBookingConsolidationSchema.KB_ParentTableCode] = JobShipmentSchema.Constants.Prefix;

			// create ignored HVLV booking
			var dodgeyBooking = Factory.New<IDtbBookingConsolidation>();
			dodgeyBooking.KB_JobType = "HLS";
			TBDodgey = (BusinessObject)dodgeyBooking;

			// attach packageJobs
			PackageJobOnOrder1 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)Order1);
			PackageJobOnOrder2 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)Order2);
			PackageJobOnTB = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)TB);
			PackageJobOnTBWithReceive = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)TBWithReceive);
			PackageJobOnTBWithShipment = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)TBWithShipment);
			PackageJobOnTBWithBOL = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)TBWithBOL);
			PackageJobOnTBDodgey = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)TBDodgey);

			Factory.Save();

			// WP_PickNo is clobbered by the first save, set then re-save
			Pick1[WhsPickSchema.WP_PickNo] = "Pick1";
			Pick2[WhsPickSchema.WP_PickNo] = "Pick2";
			Factory.Save();

			Asserter.AddToScope(PackageJobOnOrder1);
			Asserter.AddToScope(PackageJobOnOrder2);
			Asserter.AddToScope(PackageJobOnTB);
			Asserter.AddToScope(PackageJobOnTBWithReceive);
			Asserter.AddToScope(PackageJobOnTBWithShipment);
			Asserter.AddToScope(PackageJobOnTBWithBOL);
			Asserter.AddToScope(PackageJobOnTBDodgey);
		}

		public BusinessObject Order1 { get; private set; }
		public BusinessObject Order2 { get; private set; }
		public BusinessObject Receive { get; private set; }
		public BusinessObject TB { get; private set; }
		public BusinessObject TBWithReceive { get; private set; }
		public BusinessObject TBWithShipment { get; private set; }
		public BusinessObject TBWithBOL { get; private set; }
		public BusinessObject TBDodgey { get; private set; }
		public BusinessObject Consignment { get; private set; }

		public BusinessObject Pick1 { get; private set; }
		public BusinessObject Pick2 { get; private set; }

		public PkgPackageJob PackageJobOnOrder1 { get; private set; }
		public PkgPackageJob PackageJobOnOrder2 { get; private set; }
		public PkgPackageJob PackageJobOnTB { get; private set; }
		public PkgPackageJob PackageJobOnTBWithReceive { get; private set; }
		public PkgPackageJob PackageJobOnTBWithShipment { get; private set; }
		public PkgPackageJob PackageJobOnTBWithBOL { get; private set; }
		public PkgPackageJob PackageJobOnTBDodgey { get; private set; }

		public FilterStripAsserter<PkgPackageJob> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<PkgPackageJob>(Factory, (j) => j.IsDeleted ? "<JOB DELETED>" : (string)j.KJ_JobID)); }
		}

		FilterStripAsserter<PkgPackageJob> asserter;
	}
}
