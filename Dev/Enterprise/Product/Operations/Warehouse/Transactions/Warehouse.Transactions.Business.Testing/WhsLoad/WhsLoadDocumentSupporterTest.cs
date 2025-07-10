using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadDocumentSupporter))]
	public class WhsLoadDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<WhsLoad>();
	}

	internal class WhsLoadDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsLoad, DocSupporter.BusinessContext);
		}

		public override void TestGetDocBusinessObjects()
		{
			//remove this test when you want to implement printing of WhsLoad Documents.
			Assert(true);
		}

		public override void TestGetContactOrganisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var transportCompany = Helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var whsLoad = Helper.CreateWhsLoad(transportCompany, dockDoorLocation, "WL00000001", carrierServicelevel.PL_Code, truck);
			Factory.Save();

			var contact = whsLoad.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the transpor company", whsLoad.TransportCompany.PK, contact.OrgHeader.PK);
		}

		protected override bool GetExpectedShowReasonForNotPrinting() => false;

		public void TestGetEDocsProviderSupporter()
		{
			var vasOrder = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), vasOrder.GetEDocsProviderSupporter().GetType());
			AssertEquals("DocManagerInfo should be of type WLO", "WLO", vasOrder.DocManagerInfo.DocManagerCode);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		#region Implementation

		protected override Constants.DataContext DataContext => Constants.DataContext.GenericFreightJob;

		protected override BusinessObject GetNewBusinessObject() => Factory.New<WhsLoad>();

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsLoadCustomizeDocuments;

		#endregion
	}
}
