using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class PackingSynchronisationTest : TestCaseWithFactory
	{
		public void TestGetShipmentToSync()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);

			var declaration = GetDeclarationPackingRelevant();
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.JE_JS = shipment.PK;

			AssertEquals("1 shipment to sync", 1, declaration.GetShipmentsToSync().Count());
			AssertEquals(shipment, declaration.GetShipmentsToSync().ElementAt(0));

			consol.JK_AgentType = "DRT";
			AssertEquals("1 shipment to sync", 1, declaration.GetShipmentsToSync().Count());
		}

		public void TestDefaultPackingForContainerWhenThereIsOnlyOneBill()
		{
			BaseJobDeclaration declaration = GetDeclarationPackingRelevant();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "1";
			AssertEquals("There is a packing group defaulted by the system", 1, bill.PackingGroups.Count);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OCLU1111110";
			AssertEquals("there is one pack group defaulted by the system", 1, container.PackingGroups.Count);

			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU2222223";
			BasePackingGroup packGroup2 = container2.PackingGroups[0];
			AssertEquals("For this container, the bill is linked as it is the only bill in the job", bill, packGroup2.Bill);
		}

		public void TestSynchronisePackingOnSaving()
		{
			declaration.Bills.AddNew();//to be able to save packing pivot
			AssertEquals("No packing groups yet", 0, declaration.PackingGroups.Count);
			declaration.ShipmentSynchroniser.Synchronise();

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			declaration.HasChanges = false;//pack sync called from OnFactorySaving()

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);

			AssertEquals("There should be one packinggroup created", 1, declarationLoaded.PackingGroups.Count);
			AssertEquals("Total number of packages", 10, declarationLoaded.PackingGroups[0].TotalPackageCount());
		}

		public void TestSynchronisePackingWhenChangesAreMadeInShipment()
		{
			declaration.ShipmentSynchroniser.Synchronise();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;

			AssertEquals("There should be one packinggroup created", 1, declaration.PackingGroups.Count);
		}

		public void TestDefaultTotalNoOfPacksToPackage()
		{
			BaseJobDeclaration declaration = GetDeclarationPackingRelevant();
			AssertEquals("No packing groups yet", 0, declaration.PackingGroups.Count);
			AssertEquals("No House Bills yet", 0, declaration.Bills.Count);
			declaration.JE_TotalNoOfPacks = 100;
			AssertEquals("No package should have been created", 0, declaration.PackingGroups.Count);
			declaration.JE_HouseBill = "HB1";

			AssertEquals("One package should have been created", 1, declaration.PackingGroups.Count);
			BasePackingGroup packGroup = declaration.PackingGroups[0];
			AssertEquals(100, packGroup.TotalPackageCount());
			AssertEquals("A house bill is created to have PackGroup linked", declaration.Bills[0], packGroup.Bill);
		}

		public void TestSynchroniseLoosePackingDetails()
		{
			shipment.JS_HouseBill = "Shipment1";
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			declaration.RunPreSaveValidation();

			AssertEquals("House bill is created for the shipment", 1, declaration.Bills.Count);
			AssertEquals("Loose Packing Group is created", 1, declaration.Bills[0].PackingGroups.Count);
			AssertEquals("Loose packing details", 1, declaration.Bills[0].PackingGroups[0].Packages.Count);
			AssertEquals("Loose packing details", 12, declaration.Bills[0].PackingGroups[0].Packages[0].CW_PackQty);
		}

		public void TestChangingJE_OverrideFreightDefaultToFalseSynchronisePackingDetails()
		{
			declaration.JE_OverrideFreightDefaults = true;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;

			AssertEquals("No packing group is created as Override is ticked", 0, declaration.PackingGroups.Count);

			declaration.JE_OverrideFreightDefaults = false;
			AssertEquals("Packing group should be created as synchronisation is forced", 1, declaration.PackingGroups.Count);
		}

		public void TestChangingOverrideFreightDefaultRefreshBindingOfPackingInformationCollection()
		{
			BaseJobDeclaration declaration = GetDeclarationPackingRelevant();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			bool packingCollectionListChangedCalled = false;
			((IBusiness)declaration.PackingInformationCollection).ListChanged += new System.ComponentModel.ListChangedEventHandler(delegate
			{ packingCollectionListChangedCalled = true; });

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, packingCollectionListChangedCalled);
			AssertEquals(false, ((BusinessObjectCollection)declaration.PackingInformationCollection).ReadOnly);

			declaration.JE_OverrideFreightDefaults = false;
			AssertEquals(true, ((BusinessObjectCollection)declaration.PackingInformationCollection).ReadOnly);
		}

		ForwardingShipment shipment;
		BaseJobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetDeclarationPackingRelevant();
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "H1";
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			AssertEquals(false, declaration.ShouldDefaultPackingInfoFromDeclarationToBills);
		}

		protected abstract BaseJobDeclaration GetDeclarationPackingRelevant();
	}
}
