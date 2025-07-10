using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class UNDGDataItemCollectionSynchroniserTest : Customs.Business.Testing.UNDGDataItemCollectionSynchroniserTest
	{
		public override void TestFiledsReadOnlyWhenSynchronize()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "BOB";

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var shipment = Consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();

			var packLineUNDG = packLine1.UNDGs.AddNew();
			packLineUNDG.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3010", "a", "IMO").First().PK;
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

		new CusInBondContainer BillContainer => (CusInBondContainer)base.BillContainer;
	}
}
