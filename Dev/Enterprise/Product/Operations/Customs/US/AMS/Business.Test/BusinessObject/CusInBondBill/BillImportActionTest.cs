using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(BillImportAction))]
	class BillImportActionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BillImportAction(Header.Bills.AddNew());
		}

		CusInBondHeader Header
		{
			get
			{
				return fHeader ?? (fHeader = Factory.New<CusInBondHeader>());
			}
		}

		CusInBondHeader fHeader;
		public void TestActionCodeChangeWhenChangeSailing()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 4, 10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2006, 4, 20);
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			var fCLSailingBill = Factory.New<BillOfLading>();
			fCLSailingBill.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			fCLSailingBill.JS_NKLoadPort = "AUSYD";
			fCLSailingBill.JS_JX = sailing.PK;
			fCLSailingBill.JS_HouseBill = "SCACAAA";
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "123SD";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = new ZDateTime(2006, 4, 10);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			destination2.JB_E_ARV = new ZDateTime(2006, 4, 20);
			voyage2.GenerateSailings();
			var sailing2 = voyage2.Sailings[0];
			Header.ChangeSailing(sailing.PK);
			AssertEquals(0, Header.Bills.Count);
			Header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(Header.Bills));
			AssertEquals(1, Header.Bills.Count);
			var testImportCollection = new BillImportActionCollection(Header.Bills);
			AssertEquals(1, testImportCollection.Count);
			AssertEquals(ImportAction.Replace, testImportCollection[0].Action);
			Header.ChangeSailing(sailing2.PK);
			testImportCollection = new BillImportActionCollection(Header.Bills);
			AssertEquals(1, testImportCollection.Count);
			AssertEquals(ImportAction.Delete, testImportCollection[0].Action);
		}
	}
}
