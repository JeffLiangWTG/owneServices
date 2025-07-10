using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassDocsAndCartage))]
	public class GatePassDocsAndCartageBusinessObjectTestCase : JobDocsAndCartageBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			return shipment.DocsAndCartage;
		}

		public override void TestJobServices()
		{
			GatePassDocsAndCartage docsAndCartage = Factory.New<GatePassDocsAndCartage>();
			AssertNotNull("DocsAndCartage should contain a job services collection", docsAndCartage.Services);
		}

		public override void TestJobServicesGetsLoadedProperly()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GatePassShipment loadedShipment = newFactory.Load<GatePassShipment>(shipment.PK);
			Assert("Shipment's job services did not load correctly", loadedShipment.DocsAndCartage.Services.Count > 0);
		}

		public override void TestJobServicesIsRegisteredEditableChild()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			AssertEquals("Precondition - GatePassServiceDependentCollection is read only by default", true, shipment.DocsAndCartage.Services.ReadOnly);
			shipment.DocsAndCartage.SetReadOnlyIncludingChildren(false);
			AssertEquals("Job services should be read only (registered editable child of JobDocsAndCartage)", false, shipment.DocsAndCartage.Services.ReadOnly);
		}

		public void TestReasonForContingencyReleaseBecomesReadOnly()
		{
			DocsAndCartage.JP_IsContingencyRelease = true;
			AssertEquals("Reason field should be editable", false, DocsAndCartage.ReasonForContingencyReleaseInfo.ReadOnly);

			DocsAndCartage.JP_IsContingencyRelease = false;
			AssertEquals("Reason field should be read only", true, DocsAndCartage.ReasonForContingencyReleaseInfo.ReadOnly);
		}

		public void TestContingencyReleaseIsLogged()
		{
			DocsAndCartage.JP_IsContingencyRelease = true;
			DocsAndCartage.ReasonForContingencyRelease = "a reason";
			Factory.Save();

			StmALog log = Shipment.Logs.MostRecentLogByEventTime(Events.CustomsContingencyRelease);
			AssertNotNull(log);
			AssertEquals("a reason", log.SL_Reference);
		}

		#region Implementation

		GatePassShipment Shipment;
		GatePassDocsAndCartage DocsAndCartage;

		protected override void SetUp()
		{
			Shipment = Factory.New<GatePassShipment>();
			DocsAndCartage = Shipment.DocsAndCartage;
		}

		#endregion

	}
}
