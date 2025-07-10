using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentInvoicingSupportTest : BaseFreightTest
	{
		#region JobHeader

		public void TestJobHeader()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Initially no job.", null, shipment.ShipmentJobHeader);

			JobHeader job = AddJobToShipment(shipment);
			AssertEquals("JobHeader", job.PK, shipment.ShipmentJobHeader.PK);
		}

		public void TestEventsProxyFromJobHeader()
		{
			CommonShipment shipment = GetShipment();
			JobHeader job = AddJobToShipment(shipment);
			ArrayList relatedLogObjects = new ArrayList(shipment.BusinessObjectsWithRelatedEvents);
			AssertEquals("RelatedBusinessObjectsWithLogs should Container JobHeader", true, relatedLogObjects.Contains(shipment.ShipmentJobHeader));
		}

		public void TestJH_GS_NKRepSalesSecurity()
		{
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "AAA";
			var salesRep2 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep2.GS_Code = "BBB";
			var shipment = GetShipment();
			var job = AddJobToShipment(shipment);
			job.JH_GS_NKRepSales = salesRep.GS_Code;
			shipment.ShipmentJobHeader.Parent = shipment;
			Factory.Save();

			Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSalesRep).IsAllowed = false;
			shipment.ShipmentJobHeader.JH_GS_NKRepSales = salesRep2.GS_Code;
			AssertHasError("Precondition: security error message when setting the Sales Rep", shipment.ShipmentJobHeader.JH_GS_NKRepSalesInfo,
				"You do not have sufficient security rights to modify this field. You must reset the value to its previous value AAA");

			Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSalesRep).IsAllowed = true;
			shipment = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			shipment.ShipmentJobHeader.JH_GS_NKRepSales = salesRep2.GS_Code;
			AssertNoError(shipment.ShipmentJobHeader.JH_GS_NKRepSalesInfo, "You do not have sufficient security rights to modify this field. You must reset the value to its previous value AAA");
		}

		public void TestCreateShipmentJobHeaderWithMutex_ReloadsJobWhenJobCompanyIsNotCurrentCompany()
		{
			// Arrange
			var shipment = GetShipment();
			AssertEquals("Precondition: no job header loaded by default", null, shipment.ShipmentJobHeader);

			// Act
			shipment.CreateShipmentJobHeaderWithMutex();

			// Assert
			var shipmentJobHeader = shipment.ShipmentJobHeader;
			AssertNotNull("ShipmentJobHeader should now be created for current company", shipmentJobHeader);
			AssertEquals(Env.CurrentCompanyPK, shipmentJobHeader.JH_GC);

			// Arrange (Note: This *should* never happen but somehow we're getting a mismatch between Env.CurrentCompany.PK and shipmentJobHeader.JH_GC)
			var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			var anotherBranch = anotherCompany.ActiveBranches.FirstOrDefault();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				// Act
				shipment.CreateShipmentJobHeaderWithMutex();

				// Assert
				var recreatedShipmentJobHeader = shipment.ShipmentJobHeader;
				AssertNotNull("ShipmentJobHeader should now be created for current company", recreatedShipmentJobHeader);
				AssertEquals(anotherCompany.PK, recreatedShipmentJobHeader.JH_GC);
				AssertNotEquals(shipmentJobHeader.PK, recreatedShipmentJobHeader.JH_GC);

				shipmentJobHeader.Dispose();
				recreatedShipmentJobHeader.Dispose();
			}
		}

		#endregion

		#region IJobInvoicingPlugInAdditionalJobs

		public void TestIJobInvoicingPlugInAdditionalJobs_BuyersConsol()
			=> TestIJobInvoicingPlugInAdditionalJobs(packingMode: Constants.ContainerModes.BuyersConsol, invoicingStyleField: OrgCompanyData.Schema.OB_ARBuyersConsolInvoicingStyle);

		public void TestIJobInvoicingPlugInAdditionalJobs_ShippersConsol()
			=> TestIJobInvoicingPlugInAdditionalJobs(packingMode: Constants.ContainerModes.ShippersConsol, invoicingStyleField: OrgCompanyData.Schema.OB_ARShippersConsolInvoicingStyle);

		void TestIJobInvoicingPlugInAdditionalJobs(string packingMode, string invoicingStyleField)
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = packingMode;
			shipment.CoLoadShipments.AddNew();
			shipment.CoLoadShipments.AddNew();

			JobHeader job = AddJobToShipment(shipment);
			AssertEquals("No local client", 0, ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);

			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			job.LocalCharges.CompanyData[invoicingStyleField] = Constants.ConsolInvoicingStyles.Apportion;
			AssertEquals("Local client ConsolInvoicingStyle not MAS or MAB", 0, ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);

			job.LocalCharges.CompanyData[invoicingStyleField] = Constants.ConsolInvoicingStyles.Master;
			AssertEquals("Returns CoLoadShipments, MAS", 2, ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);

			job.LocalCharges.CompanyData[invoicingStyleField] = Constants.ConsolInvoicingStyles.ApportionInvoiceMaster;
			AssertEquals("Returns CoLoadShipments, MAB", 2, ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);

			shipment.JS_PackingMode = Constants.ContainerModes.AIR;
			job.LocalCharges.CompanyData[invoicingStyleField] = Constants.ConsolInvoicingStyles.ApportionInvoiceMaster;
			AssertEquals("Packing mode is not BuyersConsol", 0, ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);
		}

		#endregion

		#region Implementation

		CommonShipment GetShipment()
		{
			return Factory.NewWithValidTestData<CommonShipment>();
		}

		public JobHeader AddJobToShipment(CommonShipment shipment)
		{
			JobHeader result = Factory.NewJobForTesting<JobHeader>();
			result.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			result.JH_ParentID = shipment.PK;
			return result;
		}

		#endregion
	}
}
