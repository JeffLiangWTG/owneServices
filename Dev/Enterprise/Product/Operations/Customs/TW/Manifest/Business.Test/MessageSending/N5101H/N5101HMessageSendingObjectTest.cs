using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(N5101HMessageSendingObject))]
	sealed class N5101HMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return n5101H;
		}

		public void TestHeader()
		{
			AssertEquals(header.PK, n5101H.Header.PK);
		}

		public void TestFunctionalReferenceID()
		{
			AssertEquals(n5101H.FunctionalReferenceID, MessageConstants.FunctionalReferenceIDPlaceHolder);
		}

		public void TestFunctionCode()
		{
			var newN5101H = GetN5101HMessageSendingObject(ActionCodeList.Codes.New);
			var deleteN5101H = GetN5101HMessageSendingObject(ActionCodeList.Codes.Delete);
			var replaceN5101H = GetN5101HMessageSendingObject(ActionCodeList.Codes.Replace);
			CombineAssertions(() =>
			{
				AssertEquals("The new action code should be 9.", "9", newN5101H.FunctionCode);
				AssertEquals("The delete action code should be 1.", "1", deleteN5101H.FunctionCode);
				AssertEquals("The replace action code should be 5.", "5", replaceN5101H.FunctionCode);
			});
		}

		public void TestBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var sendingObject1 = new MessageSendingObject(bill1);
			sendingObject1.Action = ActionCodeList.Codes.New;
			var bill2 = header.Bills.AddNew();
			var sendingObject2 = new MessageSendingObject(bill2);
			sendingObject2.Action = ActionCodeList.Codes.New;

			var collection1 = new MessageSendingObjectCollection(Factory);
			var collection2 = new MessageSendingObjectCollection(Factory);

			collection1.Add(sendingObject1);
			var sendingObjParent = new MessageSendingObjectParent(header);
			var msg1 = new N5101HMessageSendingObject(header, collection1, sendingObjParent);
			AssertContainsExactElementsInExactOrder(new List<AsycudaBill>() { bill1 }, msg1.Bills);

			collection2.Add(sendingObject1);
			collection2.Add(sendingObject2);
			var msg2 = new N5101HMessageSendingObject(header, collection2, sendingObjParent);
			AssertContainsExactElementsInExactOrder(new List<AsycudaBill>() { bill1, bill2 }, msg2.Bills);
		}

		N5101HMessageSendingObject GetN5101HMessageSendingObject(ZString action)
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = action;
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject);
			var sendingObjParent = new MessageSendingObjectParent(header);
			return new N5101HMessageSendingObject(header, collection, sendingObjParent);
		}

		public void TestStatusCode()
		{
			sendingObjectParent.IsFinalManifest = true;
			AssertEquals("Should be Y.", YesNoList.Codes.Yes, n5101H.StatusCode);

			sendingObjectParent.IsFinalManifest = false;
			AssertEquals("Should be N.", YesNoList.Codes.No, n5101H.StatusCode);
		}

		public void TestBorderTransportMeans()
		{
			AssertType<N5101HBorderTransportMeans>(n5101H.BorderTransportMeans);
		}

		public void TestCarrierId()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject);
			var sendingObjParent = new MessageSendingObjectParent(header);
			n5101H = new N5101HMessageSendingObject(header, collection, sendingObjParent);
			AssertEquals(header.AMA_CarrierCode, n5101H.CarrierId);
		}

		public void TestConsignments()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.BagNumber = "B1";
			bill2.BagNumber = "B2";
			var sendingObject1 = new MessageSendingObject(bill1);
			var sendingObject2 = new MessageSendingObject(bill2);
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject1);
			collection.Add(sendingObject2);
			var sendingObjParent = new MessageSendingObjectParent(header);
			var n5101HSendingObj = new N5101HByBagMessageSendingObject(header, collection, sendingObjParent);
			var consignments = n5101HSendingObj.Consignments;
			CombineAssertions(() =>
			{
				AssertEquals(2, consignments.Count());
				AssertEquals("B1", consignments.First().AssociatedTransportDocumentId);
				AssertEquals("B2", consignments.ElementAt(1).AssociatedTransportDocumentId);
			});
		}

		public void TestDeconsolidatorId()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var deconsolidateAddress = Factory.NewWithValidTestData<OrgAddress>();
			deconsolidateAddress.CustomsCodes.AddNew("VAT", "42521663");
			header.AMA_OA_DeconsolidateAddress = deconsolidateAddress.PK;
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject);
			var sendingObjParent = new MessageSendingObjectParent(header);
			n5101H = new N5101HMessageSendingObject(header, collection, sendingObjParent);
			AssertEquals("42521663", n5101H.DeconsolidatorId);
		}

		public void TestGoodsShipment()
		{
			AssertType<N5101HGoodsShipment>(n5101H.GoodsShipment);
		}

		public void TestUnloadingLocationArrivalDateTime()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var today = ZDate.Today;
			header.MasterBill.ABL_E_ARV = today;
			var collection = new MessageSendingObjectCollection(Factory);
			var sendingObjParent = new MessageSendingObjectParent(header);
			n5101H = new N5101HMessageSendingObject(header, collection, sendingObjParent);
			AssertEquals(today, n5101H.UnloadingLocationArrivalDateTime);
		}

		public void TestSerializeToMessageString()
		{
			var expected = new N5101HMessageBuilder().PopulateXml(n5101H);
			AssertEquals(expected, n5101H.SerializeToMessageString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendingObject);
			sendingObjectParent = new MessageSendingObjectParent(header);
			n5101H = new N5101HMessageSendingObject(header, collection, sendingObjectParent);
		}

		AsycudaManifestHeader header;
		N5101HMessageSendingObject n5101H;
		MessageSendingObjectParent sendingObjectParent;
	}
}
