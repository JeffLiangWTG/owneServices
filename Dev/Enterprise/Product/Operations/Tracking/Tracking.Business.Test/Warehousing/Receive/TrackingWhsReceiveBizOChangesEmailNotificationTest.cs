using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReceive))]
	[SetGlobalsIsWeb]
	sealed class TrackingWhsReceiveBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingWhsReceive>
	{
		protected override TrackingWhsReceive GetNewBizOForNotification()
		{
			var testReceive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			var receiveLine = testReceive.Lines.AddNew();
			receiveLine.FillWithValidTestData();

			var inventory = receiveLine.Inventory[0];
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			part.OP_Desc = "First Product";
			inventory.WI_OP = part.PK;
			receiveLine.WhsReceiveLine.WE_PackQuantity = 1;
			inventory.WI_F3_NKPackType = "UNT";
			inventory.WI_TotalUnits = 1;
			inventory.WI_PartAttrib1 = "one";
			inventory.WI_PartAttrib2 = "two";
			inventory.WI_PartAttrib3 = "three";
			testReceive.RunPreSaveValidation(); // Sync docket line and inventory;

			var container = testReceive.WhsReceive.Containers.AddNew();
			container.WC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container.WC_ContainerNum = "ABC123";
			container.WC_SealNum = "DEF456";
			container.WC_IsPalletised = true;
			container.WC_IsChargeable = true;
			container.WC_ItemCount = 3;
			container.WC_PalletCount = 3;
			return testReceive;
		}
	}
}
