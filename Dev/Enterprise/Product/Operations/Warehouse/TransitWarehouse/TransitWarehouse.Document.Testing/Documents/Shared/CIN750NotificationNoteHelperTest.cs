using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750NotificationNoteHelperTest : TransitLogTableHelperTest<CIN750Notification, CIN750NotificationColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var (notification, _) = CreateTestBO(3);

			var column = tableHelper.GetColumn(new CIN750Notification[] { notification }, CIN750NotificationColumn.RefCode);
			AssertEquals("Ref Code", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "EDIDATRCN003" }, column.Values);
		}

		protected override (CIN750Notification BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var rcn = Helper.CreateReceiveConsignment("RCN" + string.Format("{0:000}", id), TestWarehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU" + id.ToString(), rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN" + id.ToString());
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST" + id.ToString());

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P" + id.ToString(), TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description " + id.ToString();
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			notification.Result = "Succeed";
			return (notification, "EDIDATRCN" + string.Format("{0:000}", id));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			TestWarehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(TestWarehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
		}

		protected override ZString GetDefaultTitle() => "Ref Code";

		protected override CIN750NotificationColumn GetDefaultColumn() => CIN750NotificationColumn.RefCode;

		protected override TransitLogTableHelper<CIN750Notification, CIN750NotificationColumn> GetTableHelper() => new CIN750NotificationNoteHelper();

		protected override int ExpectedMinimumColumnWidth { get; set; } = 6;

		WhsWarehouse TestWarehouse { get; set; }
	}
}
