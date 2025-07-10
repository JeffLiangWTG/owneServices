using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCusEntryNumberProxyCollection))]
	sealed class ForwardingShipmentCusEntryNumberProxyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ForwardingShipmentCusEntryNumberProxyCollection>
	{
		protected override ForwardingShipmentCusEntryNumberProxyCollection GetCollectionToTest()
		{
			return new ForwardingShipmentCusEntryNumberProxyCollection(Factory.New<ForwardingShipment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new ForwardingShipmentCusEntryNumberProxy(shipment, shipment.CusEntryNumbers.AddNew());
		}

		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ForwardingShipmentCusEntryNumberProxyCollection(null));
			AssertNoExceptionThrown(() => new ForwardingShipmentCusEntryNumberProxyCollection(Factory.New<ForwardingShipment>()));

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingShipmentCusEntryNumberProxyCollection cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(shipment);
			AssertEquals(0, cusEntryNumberProxyCollection.Count);

			CusEntryNumber cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();

			cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(shipment);
			AssertContainsExactElementsInAnyOrder(new[] { cusEntryNumber1 },
				cusEntryNumberProxyCollection.Cast<ForwardingShipmentCusEntryNumberProxy>().Select((proxy) => proxy.CusEntryNumber));

			CusEntryNumber cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();

			cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(shipment);
			AssertContainsExactElementsInAnyOrder(new[] { cusEntryNumber1, cusEntryNumber2 },
				cusEntryNumberProxyCollection.Cast<ForwardingShipmentCusEntryNumberProxy>().Select((proxy) => proxy.CusEntryNumber));
		}

		public void TestAllowAddAndRemove()
		{
			ForwardingShipmentCusEntryNumberProxyCollection cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(Factory.New<ForwardingShipment>());
			AssertEquals(true, cusEntryNumberProxyCollection.AllowNew);
			AssertEquals(true, cusEntryNumberProxyCollection.AllowRemove);
		}

		public void TestDeleteElement()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusEntryNumber cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			CusEntryNumber cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();

			ForwardingShipmentCusEntryNumberProxyCollection cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(shipment);
			AssertEquals("prerequisite", 2, cusEntryNumberProxyCollection.Count);

			ForwardingShipmentCusEntryNumberProxy cusEntryNumber1Proxy = cusEntryNumberProxyCollection[0];
			AssertEquals("prerequisite", cusEntryNumber1, cusEntryNumber1Proxy.CusEntryNumber);

			ForwardingShipmentCusEntryNumberProxy cusEntryNumber2Proxy = cusEntryNumberProxyCollection[1];
			AssertEquals("prerequisite", cusEntryNumber2, cusEntryNumber2Proxy.CusEntryNumber);

			((IBusinessObjectCollection)cusEntryNumberProxyCollection).Delete(cusEntryNumber1Proxy);

			AssertCollectionNotContains(cusEntryNumber1Proxy, cusEntryNumberProxyCollection);
			AssertCollectionNotContains(cusEntryNumber1, shipment.CusEntryNumbers);

			((IBusinessObjectCollection)cusEntryNumberProxyCollection).Delete(cusEntryNumber2Proxy);

			AssertCollectionNotContains(cusEntryNumber2Proxy, cusEntryNumberProxyCollection);
			AssertCollectionNotContains(cusEntryNumber2, shipment.CusEntryNumbers);
		}

		[TestDate(2023, 7, 17, 2, 3, 00)]
		public void TestDeleteElement_ResetCustomsEntryNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_EntryType = "AAA";
			cusEntryNumber1.CE_ExpiryDate = ZDateTime.Today.AddDays(1);
			cusEntryNumber1.CE_IssueDate = ZDateTime.Today;
			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber2.CE_EntryType = "BBB";
			cusEntryNumber2.CE_ExpiryDate = ZDateTime.Today.AddDays(2);
			cusEntryNumber2.CE_IssueDate = ZDateTime.Today.AddDays(1);

			AssertEquals("111, 222", shipment.CustomsEntryNumber);
			AssertEquals(ZString.Empty, shipment.CustomsEntryNumberType);
			AssertEquals(ZDateTime.Empty, shipment.CustomsEntryNumberExpiryDate);
			AssertEquals(ZDateTime.Empty, shipment.CustomsEntryNumberIssueDate);

			var cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(shipment);
			AssertEquals("prerequisite", 2, cusEntryNumberProxyCollection.Count);

			var cusEntryNumber1Proxy = cusEntryNumberProxyCollection[0];
			AssertEquals(cusEntryNumber1, cusEntryNumber1Proxy.CusEntryNumber);

			((IBusinessObjectCollection)cusEntryNumberProxyCollection).Delete(cusEntryNumber1Proxy);
			AssertEquals(cusEntryNumber2.CE_EntryNum, shipment.CustomsEntryNumber);
			AssertEquals(cusEntryNumber2.CE_EntryType, shipment.CustomsEntryNumberType);
			AssertEquals(cusEntryNumber2.CE_ExpiryDate, shipment.CustomsEntryNumberExpiryDate);
			AssertEquals(cusEntryNumber2.CE_IssueDate, shipment.CustomsEntryNumberIssueDate);

			var cusEntryNumber2Proxy = cusEntryNumberProxyCollection[0];
			AssertEquals(cusEntryNumber2, cusEntryNumber2Proxy.CusEntryNumber);

			((IBusinessObjectCollection)cusEntryNumberProxyCollection).Delete(cusEntryNumber2Proxy);
			AssertEquals(ZString.Empty, shipment.CustomsEntryNumber);
			AssertEquals("BBB", shipment.CustomsEntryNumberType);
			AssertEquals(ZDateTime.Empty, shipment.CustomsEntryNumberExpiryDate);
			AssertEquals(ZDateTime.Empty, shipment.CustomsEntryNumberIssueDate);
		}

		public void TestChangeElement_UpdateCustomsEntryNumberForBinding()
		{
			var shipment = Factory.New<ForwardingShipment>();

			AssertEquals(ZString.Empty, shipment.CustomsEntryNumberForBinding);

			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_EntryType = "AAA";

			AssertEquals("111", shipment.CustomsEntryNumberForBinding);

			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber2.CE_EntryType = "BBB";

			AssertEquals("Many", shipment.CustomsEntryNumberForBinding);
		}

		public void TestChangeElement_UpdateCustomsEntryNumberForBindingReadonly()
		{
			var shipment = Factory.New<ForwardingShipment>();

			AssertEquals(false, shipment.CustomsEntryNumberForBinding_ReadOnly);

			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_EntryType = "AAA";
			cusEntryNumber1.CE_EntryIsSystemGenerated = false;

			AssertEquals(false, shipment.CustomsEntryNumberForBinding_ReadOnly);

			cusEntryNumber1.CE_EntryIsSystemGenerated = true;

			AssertEquals(true, shipment.CustomsEntryNumberForBinding_ReadOnly);

			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber2.CE_EntryType = "BBB";

			AssertEquals(true, shipment.CustomsEntryNumberForBinding_ReadOnly);
		}

		public void TestTwoNumbersWithOneEmpty_ShouldShowEntryNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = ZString.Empty;
			cusEntryNumber1.CE_EntryType = "AAA";

			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "111";
			cusEntryNumber2.CE_EntryType = "AAA";

			AssertEquals("111", shipment.CustomsEntryNumber);
			AssertEquals("111", shipment.CustomsEntryNumberForBinding);
		}

		[TestDate(2023, 7, 17, 2, 3, 00)]
		[ExpectNoExceptions]
		public void TestCancelNewElement_DoesNotAccessDeletedObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = string.Empty;
			cusEntryNumber1.CE_EntryType = string.Empty;
			cusEntryNumber1.CE_EntryIsSystemGenerated = false;

			AssertEquals(ZString.Empty, shipment.CustomsEntryNumber);
			AssertEquals("CAN", shipment.CustomsEntryNumberType);
			AssertEquals(ZDateTime.Empty, shipment.CustomsEntryNumberExpiryDate);
			AssertEquals(ZDateTime.Empty, shipment.CustomsEntryNumberIssueDate);

			var cusEntryNumberProxyCollection = new ForwardingShipmentCusEntryNumberProxyCollection(shipment);
			AssertEquals("Precondition", 1, cusEntryNumberProxyCollection.Count);

			var newProxy = cusEntryNumberProxyCollection.AddNew();
			cusEntryNumberProxyCollection.RemoveAndDelete(newProxy);
		}
	}
}
