using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class UNDGDataItemCollectionSynchroniserTest : SynchroniserTestCase
	{
		public virtual void TestFiledsReadOnlyWhenSynchronize()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "BOB";

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";

			var packLine1 = shipment.OuterPackLines.AddNew();

			var packLineUNDG = packLine1.UNDGs.AddNew();
			packLineUNDG.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3010", "A", "IMO").First().PK;
			packLineUNDG.DI_DGFlashPoint = 10m;
			packLineUNDG.DI_TechnicalName = "DATA1";
			packLineUNDG.DI_OC_DGContact = contact1.PK;

			BillContainer.BC_ContainerNum = "CONT1";

			var synchroniser = new UNDGDataItemCollectionSynchroniser(shipment, BillContainer);
			synchroniser.Synchronise(true);

			var syncUNDG = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLineUNDG));
			AssertNotNull("A UNDG matching packLineUNDG", syncUNDG);
			Assert("DI_DG field is ReadOnly.", syncUNDG.DI_DGInfo.ReadOnly);
			Assert("DI_DGFlashPoint field is ReadOnly.", syncUNDG.DI_DGFlashPointInfo.ReadOnly);
			Assert("DI_OC_DGContact field is ReadOnly.", syncUNDG.DI_OC_DGContactInfo.ReadOnly);
			Assert("DI_TechnicalName field is ReadOnly.", syncUNDG.DI_TechnicalNameInfo.ReadOnly);
		}

		public void TestSynchronisation()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "BOB";
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "JACK";

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "10.01 01.10 A";
			packLine1.SetContainer(Consol, container2);

			var packLine1UNDG1 = packLine1.UNDGs.AddNew();

			packLine1UNDG1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3010", "A", "IMO").First().PK;
			packLine1UNDG1.DI_DGFlashPoint = 10m;
			packLine1UNDG1.DI_TechnicalName = "DATA1";
			packLine1UNDG1.DI_OC_DGContact = contact1.PK;

			var packLine1UNDG2 = packLine1.UNDGs.AddNew();
			packLine1UNDG2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			packLine1UNDG2.DI_DGFlashPoint = 10m;
			packLine1UNDG2.DI_TechnicalName = "DATA1";
			packLine1UNDG2.DI_OC_DGContact = contact2.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "20.02 02.20 B";
			packLine2.SetContainer(Consol, container1);

			var undg1010A = UNDGSubstanceLoader.LoadSubstances(Factory, "1010", "A", "IMO").First();
			undg1010A.DG_FlashPoint = "10 c.c";
			var undg1010APK = undg1010A.PK;
			var packLine2UNDG1 = packLine2.UNDGs.AddNew();
			packLine2UNDG1.DI_DG = undg1010APK;
			packLine2UNDG1.DI_DGFlashPoint = 10m;
			packLine2UNDG1.DI_TechnicalName = "DATA1";
			packLine2UNDG1.DI_OC_DGContact = contact1.PK;

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "30.03 03.30 C";
			packLine3.SetContainer(Consol, container1);

			var packLine3UNDG1 = packLine3.UNDGs.AddNew();
			packLine3UNDG1.DI_DG = undg1010APK;
			packLine3UNDG1.DI_DGFlashPoint = 10m;
			packLine3UNDG1.DI_TechnicalName = "DATA1";
			packLine3UNDG1.DI_OC_DGContact = contact1.PK;

			var packLine3UNDG2 = packLine3.UNDGs.AddNew();
			packLine3UNDG2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1010", "B", "IMO").First().PK;
			packLine3UNDG2.DI_DGFlashPoint = 10m;
			packLine3UNDG2.DI_TechnicalName = "DATA1";
			packLine3UNDG2.DI_OC_DGContact = contact1.PK;

			var packLine3UNDG3 = packLine3.UNDGs.AddNew();
			packLine3UNDG3.DI_DG = undg1010APK;
			packLine3UNDG3.DI_DGFlashPoint = 11m;
			packLine3UNDG3.DI_TechnicalName = "DATA1";
			packLine3UNDG3.DI_OC_DGContact = contact1.PK;

			var packLine3UNDG4 = packLine3.UNDGs.AddNew();
			packLine3UNDG4.DI_DG = undg1010APK;
			packLine3UNDG4.DI_DGFlashPoint = 10m;
			packLine3UNDG4.DI_TechnicalName = "DATA2";
			packLine3UNDG4.DI_OC_DGContact = contact1.PK;

			var packLine3UNDG5 = packLine3.UNDGs.AddNew();
			packLine3UNDG5.DI_DG = undg1010APK;
			packLine3UNDG5.DI_DGFlashPoint = 10m;
			packLine3UNDG5.DI_TechnicalName = "DATA1";
			packLine3UNDG5.DI_OC_DGContact = contact2.PK;

			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_HarmonisedCode = "40.02.234B";
			packLine4.SetContainer(Consol, container1);

			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.JL_HarmonisedCode = "50.105.30";
			packLine5.SetContainer(Consol, container2);

			BillContainer.BC_ContainerNum = "CONT1";
			AssertEquals(0, BillContainer.UNDGs.Count);

			var synchroniser = new UNDGDataItemCollectionSynchroniser(shipment, BillContainer);
			synchroniser.Synchronise(true);
			AssertEquals(5, BillContainer.UNDGs.Count);
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG1", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG1));
			AssertNull("No UNDG should match packLine1UNDG1 as it belongs to different container", BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			var billContainerPackLine3UNDG1 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1));
			AssertNotNull("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1);
			var billContainerPackLine3UNDG2 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG2));
			AssertNotNull("A UNDG matching packLine3UNDG2", billContainerPackLine3UNDG2);
			var billContainerPackLine3UNDG3 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG3));
			AssertNotNull("A UNDG matching packLine3UNDG3", billContainerPackLine3UNDG3);
			var billContainerPackLine3UNDG4 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG4));
			AssertNotNull("A UNDG matching packLine3UNDG4", billContainerPackLine3UNDG4);
			var billContainerPackLine3UNDG5 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG5));
			AssertNotNull("A UNDG matching packLine3UNDG5", billContainerPackLine3UNDG5);

			packLine3UNDG4.DI_TechnicalName = "DATA1";
			AssertEquals("packLine3UNDG1 should have same data as packLine3UNDG4", true, synchroniser.HasUNDGGotSameData(packLine3UNDG1, packLine3UNDG4));
			AssertEquals(4, BillContainer.UNDGs.Count);
			AssertNull("No UNDG should match packLine1UNDG1 as it belongs to different container", BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG2", billContainerPackLine3UNDG2, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG2)));
			AssertEquals("A UNDG matching packLine3UNDG3", billContainerPackLine3UNDG3, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG3)));
			AssertEquals(true, billContainerPackLine3UNDG4.IsDeleted);
			AssertEquals("A UNDG matching packLine3UNDG5", billContainerPackLine3UNDG5, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG5)));

			packLine3UNDG3.DI_DGFlashPoint = 10m;
			AssertEquals("packLine3UNDG1 should have same data as packLine3UNDG3", true, synchroniser.HasUNDGGotSameData(packLine3UNDG1, packLine3UNDG3));
			AssertEquals(3, BillContainer.UNDGs.Count);
			AssertNull("No UNDG should match packLine1UNDG1 as it belongs to different container", BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG2", billContainerPackLine3UNDG2, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG2)));
			AssertEquals(true, billContainerPackLine3UNDG3.IsDeleted);
			AssertEquals("A UNDG matching packLine3UNDG5", billContainerPackLine3UNDG5, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG5)));

			packLine3UNDG2.DI_DG = undg1010APK;
			AssertEquals("packLine3UNDG1 should have same data as packLine3UNDG2", true, synchroniser.HasUNDGGotSameData(packLine3UNDG1, packLine3UNDG2));
			AssertEquals(2, BillContainer.UNDGs.Count);
			AssertNull("No UNDG should match packLine1UNDG1 as it belongs to different container", BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals(true, billContainerPackLine3UNDG2.IsDeleted);
			AssertEquals("A UNDG matching packLine3UNDG5", billContainerPackLine3UNDG5, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG5)));

			packLine3UNDG5.DI_OC_DGContact = contact1.PK;
			AssertEquals("packLine3UNDG1 should have same data as packLine3UNDG5", true, synchroniser.HasUNDGGotSameData(packLine3UNDG1, packLine3UNDG5));
			AssertEquals(1, BillContainer.UNDGs.Count);
			AssertNull("No UNDG should match packLine1UNDG1 as it belongs to different container", BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals(true, billContainerPackLine3UNDG5.IsDeleted);

			var packLine4UNDG1 = packLine4.UNDGs.AddNew();
			AssertEquals(2, BillContainer.UNDGs.Count);
			AssertNull("No UNDG should match packLine1UNDG1 as it belongs to different container", BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			var billContainerPackLine4UNDG1 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine4UNDG1));
			AssertNotNull("A UNDG matching packLine4UNDG1", billContainerPackLine4UNDG1);

			packLine1.SetContainer(Consol, container1);
			AssertEquals(4, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("A UNDG matching packLine4UNDG1", billContainerPackLine4UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine4UNDG1)));
			var billContainerPackLine1UNDG1 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1));
			AssertNotNull("A UNDG matching packLine1UNDG1", billContainerPackLine1UNDG1);
			var billContainerpackLine1UNDG2 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG2));
			AssertNotNull("A UNDG matching packLine1UNDG2", billContainerpackLine1UNDG2);

			packLine4UNDG1.Delete();
			AssertEquals(3, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("Should be deleted as the packLine4UNDG1 was deleted", true, billContainerPackLine4UNDG1.IsDeleted);
			AssertEquals("A UNDG matching packLine1UNDG1", billContainerPackLine1UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG1)));
			AssertEquals("A UNDG matching packLine1UNDG2", billContainerpackLine1UNDG2, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine1UNDG2)));

			packLine1.Delete();
			AssertEquals(1, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("Should be deleted as packLine1 was deleted", true, billContainerPackLine4UNDG1.IsDeleted);
			AssertEquals("Should be deleted as packLine1 was deleted", true, billContainerPackLine1UNDG1.IsDeleted);
			AssertEquals("Should be deleted as packLine1 was deleted", true, billContainerpackLine1UNDG2.IsDeleted);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			packLine3UNDG1.DI_DG = subs.PK;
			packLine3UNDG1.LinkDefault(subs);
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG1", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG1));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG2", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG2));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG3", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG3));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG4", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG4));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG5", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG5));
			AssertEquals(2, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine2UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine2UNDG1)));
			var billContainerNewPackLine3UNDG1 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1));
			AssertNotNull("A UNDG matching packLine3UNDG1", billContainerNewPackLine3UNDG1);

			packLine3UNDG2.DI_DGFlashPoint = 5.5m;
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG1", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG1));
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG2", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG2));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG3", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG3));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG4", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG4));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG5", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG5));
			AssertEquals(3, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine2UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine2UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerNewPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			var billContainerNewPackLine3UNDG2 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG2));
			AssertNotNull("A UNDG matching packLine3UNDG2", billContainerNewPackLine3UNDG2);

			packLine3UNDG3.DI_TechnicalName = "DATA5";
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG1", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG1));
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG2", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG2));
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG3", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG3));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG4", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG4));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG5", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG5));
			AssertEquals(4, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine2UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine2UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerNewPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG2", billContainerNewPackLine3UNDG2, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG2)));
			var billContainerNewPackLine3UNDG3 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG3));
			AssertNotNull("A UNDG matching packLine3UNDG3", billContainerNewPackLine3UNDG3);

			packLine3UNDG4.DI_OC_DGContact = contact2.PK;
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG1", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG1));
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG2", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG2));
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG3", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG3));
			AssertEquals("packLine2UNDG1 should not have same data as packLine3UNDG4", false, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG4));
			AssertEquals("packLine2UNDG1 should have same data as packLine3UNDG5", true, synchroniser.HasUNDGGotSameData(packLine2UNDG1, packLine3UNDG5));
			AssertEquals(5, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine2UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine2UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG1", billContainerNewPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG1)));
			AssertEquals("A UNDG matching packLine3UNDG2", billContainerNewPackLine3UNDG2, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG2)));
			AssertEquals("A UNDG matching packLine3UNDG3", billContainerNewPackLine3UNDG3, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG3)));
			var billContainerNewPackLine3UNDG4 = BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine3UNDG4));
			AssertNotNull("A UNDG matching packLine3UNDG4", billContainerNewPackLine3UNDG4);

			packLine3.SetContainer(Consol, container2);
			AssertEquals(1, BillContainer.UNDGs.Count);
			AssertEquals("A UNDG matching packLine2UNDG1", billContainerPackLine3UNDG1, BillContainer.UNDGs.FirstOrDefault(x => synchroniser.HasUNDGGotSameData(x, packLine2UNDG1)));
			AssertEquals("Should be deleted as packLine3 is not part of container1", true, billContainerNewPackLine3UNDG1.IsDeleted);
			AssertEquals("Should be deleted as packLine3 is not part of container1", true, billContainerNewPackLine3UNDG2.IsDeleted);
			AssertEquals("Should be deleted as packLine3 is not part of container1", true, billContainerNewPackLine3UNDG3.IsDeleted);
			AssertEquals("Should be deleted as packLine3 is not part of container1", true, billContainerNewPackLine3UNDG4.IsDeleted);

			container1.Delete();
			AssertEquals(0, BillContainer.UNDGs.Count);
			AssertEquals("Should be deleted as container1 was deleted", true, billContainerPackLine3UNDG1.IsDeleted);
		}

		public void TestSynchronisationViaDataRefreshBus()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";

			shipment.JS_HouseBill = "ABCDHB1";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(Consol, container);
			var packLineUNDG = packLine.UNDGs.AddNew();
			packLineUNDG.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1010", "A", "IMO").First().PK;

			BillContainer.BC_ContainerNum = "CONT1";
			var synchroniser = new UNDGDataItemCollectionSynchroniser(shipment, BillContainer);
			synchroniser.Synchronise(true);
			AssertEquals(1, BillContainer.UNDGs.Count);
			var undg1 = BillContainer.UNDGs[0];
			AssertEquals("1010a", undg1.UNDGSubstance.DG_Code);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packLineUNDGInOtherFactory = newFactory.Load<UNDGDataItem>(packLineUNDG.PK);
			AssertEquals("1010a", packLineUNDGInOtherFactory.UNDGSubstance.DG_Code);
			packLineUNDGInOtherFactory.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			newFactory.Save();
			AssertEquals("0004a", packLineUNDG.UNDGSubstance.DG_Code);
			AssertEquals(1, BillContainer.UNDGs.Count);
			AssertEquals(false, undg1.IsDeleted);
			AssertEquals(undg1, BillContainer.UNDGs[0]);
			AssertEquals("0004a", undg1.UNDGSubstance.DG_Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			shipment = Consol.Shipments.AddNew();

			header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			billContainer = (CusInBondContainer)moveDetail.Containers.AddNew();
		}
		ForwardingConsol consol;
		ForwardingShipment shipment;
		CusInBondHeader header;
		CusInBondContainer billContainer;

		protected ForwardingConsol Consol
		{
			get { return consol; }
		}

		protected virtual CusInBondContainer BillContainer
		{
			get { return billContainer; }
		}

		protected virtual CusInBondHeader Header
		{
			get { return header; }
		}

		#endregion
	}
}
