using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(VoyageAccountInvoicingSupporter))]
	internal class VoyageAccountInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			return voyageAccount;
		}

		public void TestVoyageVesselOrFlightDate()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "PLANET EXPRESS";
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_VoyageFlight = "001000";
			var voyage = (BusinessObject)Factory.New<Integration.Agency.IVoyageAccount>();
			voyage["NA_JV"] = jobVoyage.PK;
			var pluginData = (IJobInvoicingPlugIn)voyage;
			AssertEquals("001000/PLANET EXPRESS", pluginData.InvoicingSupporter.VoyageVesselOrFlightDate);
		}
	}
}
