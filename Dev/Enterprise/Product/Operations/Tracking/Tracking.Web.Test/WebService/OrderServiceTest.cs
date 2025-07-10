using System.Linq;
using System.Xml.Serialization;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.WebService;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class OrderServiceTest : BaseWebServiceTest<OrderService>
	{
		public void TestGetOrderListInternalEmpty()
		{
			Factory.Save();
			Xsd.WebOrders result = WebService.GetOrderListInternal(CurrentOrg, null);
			AssertNotNull("even though there are no orders, it should return something", result);
			AssertEquals("empty content", 0, result.WebOrder.Count);
		}

		public void TestGetOrderListInternalFull()
		{
			TestHelper testHelper = new TestHelper(Factory);
			TrackingOrder order = testHelper.CreateOrder();

			CurrentOrg.OH_IsConsignee = true;
			order.BuyerPK = testHelper.TestContact.Header.PK;

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login(testHelper.TestContact.Header.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			Xsd.WebOrders result = WebService.GetOrderListInternal(testHelper.TestContact.Header, null);
			AssertNotNull("there are orders, it should return something", result);
			AssertEquals("Should contain one order", 1, result.WebOrder.Count);
			AssertEquals("Total row count", "1", result.TotalRows);
			AssertEquals("returned row count", "1", result.ReturnedRows);
		}

		public void TestGetFilterBusinessObjectEmpty()
		{
			TrackingOrderFilterBusinessObject_Old_ForWeb filterBizO = WebService.GetFilterBusinessObject(CurrentOrg, null);
			AssertNotNull("Filter Business Object must be created", filterBizO);
		}

		public void TestGetFilterBusinessObjectFull()
		{
			Xsd.WebOrderFilter filter = new Xsd.WebOrderFilter();
			filter.Number = new Xsd.WebOrderFilterNumber();
			filter.Number.NumberSearchField = Xsd.OrderNumberFieldsList.ALL;
			filter.Number.NumberValue = "1234";
			TrackingOrderFilterBusinessObject_Old_ForWeb filterBizO = WebService.GetFilterBusinessObject(CurrentOrg, filter);
			AssertEquals("number", "1234", filterBizO.JD_Number);
		}

		public void TestSoapBodyElementName()
		{
			// Callers expect to provide XML bodies beginning with capital letters.
			var getOrderListParameters = typeof(OrderService).GetMethod(nameof(OrderService.GetOrderList)).GetParameters();
			var xmlAttribute = getOrderListParameters[0].GetCustomAttributes(true).OfType<XmlElementAttribute>().FirstOrDefault();

			AssertEquals("Filter", xmlAttribute.ElementName);
		}
	}
}
