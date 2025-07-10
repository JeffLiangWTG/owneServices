using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(TrackingOrderFilterBusinessObject_Old_ForWeb))]
	sealed class TrackingOrderFilterBusinessObject_Old_ForWebTest : OrderFilterBusinessObject_Old_ForWebTest
	{
		#region Setup

		new TrackingOrderFilterBusinessObject_Old_ForWeb FilterBO
		{
			get
			{
				return base.FilterBO as TrackingOrderFilterBusinessObject_Old_ForWeb;
			}
		}

		protected override OrdersFilterBusinessObject_Old_ForWeb GetNewFilterBusinessObject()
		{
			TrackingOrderFilterBusinessObject_Old_ForWeb result = FilterFactory.New<TrackingOrderFilterBusinessObject_Old_ForWeb>();
			result.CurrentOrg = Buyer.PK;
			return result;
		}

		new BusinessObjectFactory Factory
		{
			get
			{
				return base.Factory;
			}
		}

		protected override Order GetNewOrder()
		{
			TrackingOrder trackingOrder = Factory.NewWithValidTestData<TrackingOrder>();

			trackingOrder.BuyerPK = Buyer.PK;
			trackingOrder.SupplierPK = Supplier.PK;

			return trackingOrder;
		}

		protected override Type GetExpectedOrderType()
		{
			return typeof(TrackingOrder);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestInitialiseFilterBusinessObjectFromShipment()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.ConsignorPK = ZGuid.NewZGuid();
			shipment.ConsigneePK = ZGuid.NewZGuid();

			TrackingOrderFilterBusinessObject_Old_ForWeb filter = FilterBusinessObjectFactory.New<TrackingOrderFilterBusinessObject_Old_ForWeb>();
			filter.SetExternalDefaults(shipment.PossibleOrdersForAttachment_List.FilterBusinessObjectDefaults);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("should not search for cancelled orders", ZBool.False, FilterBO.JD_IsCancelled);
		}

		#region Helpers

		ZString BuyerCode
		{
			get
			{
				return Buyer.OH_Code;
			}
		}

		#endregion
		public void TestNoDBHitIfOrgIsInvalid()
		{
			WebEnv.AppInstance.SiteUser.Logout();
			FilterBO.CurrentOrg = ZGuid.Empty;
			FilterBO.JD_OH_Org1 = ZGuid.Empty;
			FilterBO.JD_OH_Org2 = ZGuid.Empty;
			Assert("Should NOT return results", FilterBO.Filter.IsNoResultQuery);

			WebEnv.AppInstance.SiteUser.Login(Helper.TestContact.Header.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			FilterBO.CurrentOrg = Buyer.PK;
			FilterBO.JD_OH_Org1 = ZGuid.Empty;
			FilterBO.JD_OH_Org2 = ZGuid.Empty;
			Assert("Should return results", !FilterBO.Filter.IsNoResultQuery);

			FilterBO.JD_OH_Org1 = Buyer.PK;
			FilterBO.JD_OH_Org2 = Supplier.PK;
			Assert("Should return results", !FilterBO.Filter.IsNoResultQuery);

			FilterBO.JD_OH_Org1 = ZGuid.Invalid;
			FilterBO.JD_OH_Org2 = ZGuid.Empty;
			Assert("Should NOT return results", FilterBO.Filter.IsNoResultQuery);

			FilterBO.JD_OH_Org1 = ZGuid.Empty;
			FilterBO.JD_OH_Org2 = ZGuid.Invalid;
			Assert("Should NOT return results", FilterBO.Filter.IsNoResultQuery);
		}

		public void TestOrg1CodeUpdatesJS_OH_Org1()
		{
			AssertEquals(BuyerCode, Buyer.OH_Code);
			AssertEquals(ZGuid.Empty, FilterBO.JD_OH_Org1);

			Factory.Save();
			FilterBO.Org1Code = BuyerCode;
			AssertEquals(Buyer.PK, FilterBO.JD_OH_Org1);

			FilterBO.Org1Code = "some";
			AssertEquals(ZGuid.Invalid, FilterBO.JD_OH_Org1);

			FilterBO.Org1Code = ZString.Empty;
			AssertEquals(ZGuid.Empty, FilterBO.JD_OH_Org1);
		}

		public void TestOrg2CodeUpdatesJS_OH_Org2()
		{
			AssertEquals(BuyerCode, Buyer.OH_Code);
			AssertEquals(ZGuid.Empty, FilterBO.JD_OH_Org2);

			Factory.Save();
			FilterBO.Org2Code = BuyerCode;
			AssertEquals(Buyer.PK, FilterBO.JD_OH_Org2);

			FilterBO.Org2Code = "some";
			AssertEquals(ZGuid.Invalid, FilterBO.JD_OH_Org2);

			FilterBO.Org2Code = ZString.Empty;
			AssertEquals(ZGuid.Empty, FilterBO.JD_OH_Org2);
		}

		public void TestJS_OH_Org1UpdatesOrg1Code()
		{
			FilterBO.Org1Code = "some value";
			FilterBO.JD_OH_Org1 = Buyer.PK;

			Factory.Save();
			AssertEquals(BuyerCode, FilterBO.Org1Code);

			FilterBO.Org1Code = "some value";
			FilterBO.JD_OH_Org1 = ZGuid.Empty;

			Factory.Save();
			AssertEquals(ZString.Empty, FilterBO.Org1Code);

			FilterBO.Org1Code = "some value";
			FilterBO.JD_OH_Org1 = ZGuid.Invalid;

			Factory.Save();
			AssertEquals(ZString.Empty, FilterBO.Org1Code);
		}

		public void TestJS_OH_Org2UpdatesOrg2Code()
		{
			FilterBO.Org2Code = "some value";
			FilterBO.JD_OH_Org2 = Buyer.PK;

			Factory.Save();
			AssertEquals(BuyerCode, FilterBO.Org2Code);

			FilterBO.Org2Code = "some value";
			FilterBO.JD_OH_Org2 = ZGuid.Empty;

			Factory.Save();
			AssertEquals(ZString.Empty, FilterBO.Org2Code);

			FilterBO.Org2Code = "some value";
			FilterBO.JD_OH_Org2 = ZGuid.Invalid;

			Factory.Save();
			AssertEquals(ZString.Empty, FilterBO.Org2Code);
		}

		public void TestOrgCodesCanBeSavedAndReloaded()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "Code1";
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = "CODE2";
			Factory.Save();

			FilterBO.Org1Code = "Code1";
			FilterBO.Org2Code = "CODE2";
			FilterBusinessObjectFactory.Save(FilterBO);

			var extraAllowedTypes = new[] { typeof(SQLComparisonOperator), typeof(StartsWithComparisonOperator) };

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain.CurrentDomain.SetData("System.Data.DataSetDefaultAllowedTypes", extraAllowedTypes);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

			var newFilterFactory = new WebFilterBusinessObjectFactory(new BusinessObjectFactory());
			var newFilterBO = newFilterFactory.Load<TrackingOrderFilterBusinessObject_Old_ForWeb>();
			AssertEquals("Should be retrieved from the DB", "Code1", newFilterBO.Org1Code);
			AssertEquals("Should be retrieved from the DB", "CODE2", newFilterBO.Org2Code);
		}

		public void TestNoneForDropDownLists()
		{
			AssertEquals("Shouldn't contain NONE code", false, FilterBO.JD_NumberFilterType_List.ContainsCode("None"));
			AssertEquals("Shouldn't contain NONE code", false, FilterBO.JD_DateFilterType_List.ContainsCode("None"));
			AssertEquals("Shouldn't contain NONE code", false, FilterBO.JD_OrgFilterType_List.ContainsCode("None"));
			AssertEquals("Shouldn't contain NONE code", false, FilterBO.JD_PortFilterType_List.ContainsCode("None"));
		}

		public void TestDefaultsForDropDowns()
		{
			AssertEquals(OrdersConstants.NumberFilterTypes.MostCommon, FilterBO.JD_NumberFilterType);
			AssertEquals(OrdersConstants.DateFilterTypes.MostCommon, FilterBO.JD_DateFilterType);
			AssertEquals(OrdersConstants.OrgFilterTypes.All, FilterBO.JD_OrgFilterType);
			AssertEquals(OrdersConstants.PortFilterTypes.All, FilterBO.JD_PortFilterType);
		}

		public void TestFilterByUndelivered()
		{
			FilterBO.JD_OrderStatus = "UND";

			TrackingLegacyOrderCollection collection1 = new TrackingLegacyOrderCollection(Factory);
			collection1.Load(FilterBO.Filter);

			AssertEquals("There should be no undelivered orders.", 0, collection1.Count);

			TrackingOrder order1 = (TrackingOrder)GetNewOrder();
			order1.JD_OrderStatus = Core.Constants.OrderStatus.Delivered;

			TrackingOrder order2 = (TrackingOrder)GetNewOrder();
			order2.JD_OrderStatus = Core.Constants.OrderStatus.Confirmed;

			TrackingOrder order3 = (TrackingOrder)GetNewOrder();
			order3.JD_OrderStatus = Core.Constants.OrderStatus.Incomplete;

			TrackingOrder order4 = (TrackingOrder)GetNewOrder();
			order4.JD_OrderStatus = Core.Constants.OrderStatus.PartDelivered;

			TrackingOrder order5 = (TrackingOrder)GetNewOrder();
			order5.JD_OrderStatus = Core.Constants.OrderStatus.Open;

			TrackingOrder order6 = (TrackingOrder)GetNewOrder();
			order6.JD_OrderStatus = Core.Constants.OrderStatus.Shipped;

			Factory.Save();

			TrackingLegacyOrderCollection collection2 = new TrackingLegacyOrderCollection(Factory);
			collection2.Load(FilterBO.Filter);

			AssertEquals("Should be 5 undelivered orders", 5, collection2.Count);
		}

		public void TestFilterByAll()
		{
			FilterBO.JD_OrderStatus = "ALL";

			TrackingLegacyOrderCollection collection1 = new TrackingLegacyOrderCollection(Factory);
			collection1.Load(FilterBO.Filter);

			AssertEquals("There should be no orders.", 0, collection1.Count);

			TrackingOrder order1 = (TrackingOrder)GetNewOrder();
			order1.JD_OrderStatus = Core.Constants.OrderStatus.Delivered;

			TrackingOrder order2 = (TrackingOrder)GetNewOrder();
			order2.JD_OrderStatus = Core.Constants.OrderStatus.Confirmed;

			TrackingOrder order3 = (TrackingOrder)GetNewOrder();
			order3.JD_OrderStatus = Core.Constants.OrderStatus.Incomplete;

			TrackingOrder order4 = (TrackingOrder)GetNewOrder();
			order4.JD_OrderStatus = Core.Constants.OrderStatus.PartDelivered;

			TrackingOrder order5 = (TrackingOrder)GetNewOrder();
			order5.JD_OrderStatus = Core.Constants.OrderStatus.Open;

			TrackingOrder order6 = (TrackingOrder)GetNewOrder();
			order6.JD_OrderStatus = Core.Constants.OrderStatus.Shipped;

			Factory.Save();

			TrackingLegacyOrderCollection collection2 = new TrackingLegacyOrderCollection(Factory);
			collection2.Load(FilterBO.Filter);

			AssertEquals("Should be 6 orders", 6, collection2.Count);
		}

		public void TestCustomAttributesInNumberFilter()
		{
			Buyer.CustomLabels.Load();
			AssertEquals("Should be no CustomLabels", 0, Buyer.CustomLabels.Count);
			AssertEquals("Should be 10 items in Number Filter dropdown list", 10, FilterBO.JD_NumberFilterType_List.Count);

			OrgCustomLabels label1 = Buyer.CustomLabels.AddNew();
			OrgCustomLabels label2 = Buyer.CustomLabels.AddNew();

			label1.OT_FieldName = "OrderHeader.CustomAttrib1";
			label1.OT_Caption = "Custom1";

			label2.OT_FieldName = "SomeOtherHeader.CustomAttrib2";
			label2.OT_Caption = "Custom2";

			Factory.Save();

			AssertEquals("Should be two CustomLabels", 2, Buyer.CustomLabels.Count);
			AssertEquals("Should be 11 items in Number Filter dropdown list", 11, FilterBO.JD_NumberFilterType_List.Count);

			AssertEquals("The eleventh should be Custom1", "OrderHeader.CustomAttrib1", FilterBO.JD_NumberFilterType_List[10].Code);
			AssertEquals("The eleventh should be Custom1", "Custom1", FilterBO.JD_NumberFilterType_List[10].Description);

			FilterBO.JD_NumberFilterType = "OrderHeader.CustomAttrib1";
			FilterBO.JD_Number = "12345";

			AssertContains("Should contain JD_CustomAttrib1", JobOrderHeaderSchema.JD_CustomAttrib1.Name, FilterBO.Filter.GetAsWhereClause(true));
			AssertContains("Should contain value for JD_CustomAttrib1", "'%12345%'", FilterBO.Filter.GetAsWhereClause(true));
		}

		#region Org and Location Captions

		public void TestOrgCaptions()
		{
			AssertOrgCaption(OrdersConstants.OrgFilterTypes.BuyerSupplier, "Buyer: ", "Supplier: ");
			AssertOrgCaption(OrdersConstants.OrgFilterTypes.SendingRecvAgent, "Send. Agent: ", "Recv. Agent: ");
			AssertOrgCaption(OrdersConstants.OrgFilterTypes.All, "Org. 1" + ": ",  "Org. 2" + ": ");
		}

		public void TestLocationCaptions()
		{
			AssertLocationCaption(OrdersConstants.PortFilterTypes.LoadDischargeCode, "Load: ", "Discharge: ");
			AssertLocationCaption(OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode, "Origin: ", "Destination: ");
		}

		void AssertOrgCaption(ZString orgFilterType, ZString expectedOrg1Caption, ZString expectedOrg2Caption)
		{
			FilterBO.JD_OrgFilterType = orgFilterType;
			AssertEquals(expectedOrg1Caption, FilterBO.Org1Caption);
			AssertEquals(expectedOrg2Caption, FilterBO.Org2Caption);
		}

		void AssertLocationCaption(ZString locationFilterType, ZString expectedLocation1Caption, ZString expectedLocation2Caption)
		{
			FilterBO.JD_PortFilterType = locationFilterType;
			AssertEquals(expectedLocation1Caption, FilterBO.Location1Caption);
			AssertEquals(expectedLocation2Caption, FilterBO.Location2Caption);
		}

		#endregion
	}
}
