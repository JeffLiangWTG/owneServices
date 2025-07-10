using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Business.Testing
{
	public class TestHelper : ZWebTestHelper
	{
		public TestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Shipments

		public TrackingShipment CreateShipment()
		{
			var testShipment = Factory.New<TrackingShipment>();
			testShipment.JS_UniqueConsignRef = GetRandomString(testShipment.JS_UniqueConsignRefInfo.MaxLength);
			testShipment.JS_HouseBill = GetRandomString(testShipment.JS_HouseBillInfo.MaxLength);
			testShipment.JS_BookingReference = GetRandomString(testShipment.JS_BookingReferenceInfo.MaxLength);
			testShipment.ConsigneePK = TestOrg.PK;
			testShipment.ConsignorPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			testShipment.Consignor.OH_FullName = "Consignor";
			testShipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 1, 1);
			testShipment.JS_E_ARV = new ZDateTime(2005, 2, 2);
			testShipment.JS_E_DEP = new ZDateTime(2005, 3, 3);
			testShipment.JS_RL_NKOrigin = "AUSYD";
			testShipment.JS_RL_NKDestination = "MYPKG";

			return testShipment;
		}

		#endregion

		#region Order

		public TrackingOrder CreateOrder()
		{
			var order = Factory.New<TrackingOrder>();
			order.JD_OrderNumber = GetRandomString(order.JD_OrderNumberInfo.MaxLength);
			order.JD_OrderDate = ZDateTime.Today;
			order.BuyerPK = TestOrg.PK;
			return order;
		}

		#endregion

		#region WhsOrder

		public TrackingWhsOrder CreateWhsOrder()
		{
			TrackingWhsOrder order = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			order.SiteUser = TestSiteUser;
			return order;
		}

		#endregion

		#region WhsReceive

		public TrackingWhsReceive CreateWhsReceive()
		{
			var receive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			receive.SiteUser = TestSiteUser;
			return receive;
		}

		#endregion

		#region WhsInventory

		public TrackingWhsInventory CreateWhsInventory()
		{
			var receiveLine = CreateWhsReceive().Lines.AddNew();
			receiveLine.WhsReceiveLine.FillWithValidTestData();

			var inventory = receiveLine.Inventory[0];
			inventory.SiteUser = TestSiteUser;
			return inventory;
		}

		public TrackingWhsInventory CreateEmptyWhsInventory()
		{
			var receive = TrackingHelper.Get(Factory.New<WhsReceive>());
			var receiveLine = receive.Lines.AddNew();
			var inventory = receiveLine.Inventory[0];

			return inventory;
		}

		#endregion

		#region Declarations

		public BaseJobDeclaration CreateDeclaration()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Importer = TestOrg.PK;
			return dec;
		}
		#endregion

		#region Overrides

		public new TrackingSiteUser TestSiteUser
		{
			get { return base.TestSiteUser as TrackingSiteUser; }
		}

		protected override WebUser GetNewSiteUser()
		{
			var user = new TrackingSiteUser();
			if (TestContact.OC_Email.IsEmpty)
			{
				TestContact.OC_Email = "test@cargowise.com";
			}

			if (!TestContact.HasPassword)
			{
				TestContact.SetHashedPassword("test");
			}

			TestContact.OC_WebAccessEnabled = true;
			TestContact.Factory.Save();

			user.Login(TestOrg.OH_Code, TestContact.OC_Email, TestContact.PasswordForTesting);
			if (WebEnv.AppInstance is DummyHttpApplication dummyHttpApplication)
			{
				dummyHttpApplication.SetSiteUser(user);
			}

			return user;
		}
		#endregion

		public class ZTestDataGrid : ZDataGrid
		{
			public StateBag ViewState_Exposed
			{
				get { return base.ViewState; }
			}
		}
	}
}
