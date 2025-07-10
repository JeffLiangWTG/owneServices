using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentDocManagerInfo))]
	sealed class DtbConsignmentDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region GetPopulatedParentBusinessObject

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var consignment = Helper.CreateConsignment();

			return consignment;
		}

		#endregion

		#region TestGetRelatedeDocs

		public void TestGetRelatedeDocs()
		{
			string docType = "MSC", consignmentFile = "ConsignmentFile";

			var consignment = Helper.CreateConsignment();

			var consignmentDocManagerInfo = ((IDocManagerSupport)consignment).DocManagerInfo;
			consignmentDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, consignmentFile, docType, true);

			AssertDocumentTypeAndFileName(consignmentDocManagerInfo, 1, docType, consignmentFile);
		}

		void AssertDocumentTypeAndFileName(DocManagerInfo docManagerInfo, int totalDoc, string docType, string fileName)
		{
			AssertEquals("Total file count should be: " + totalDoc.ToString(), totalDoc, docManagerInfo.AllEDocs.Count);
			AssertEquals("DocType should be: " + docType, docType, docManagerInfo.AllEDocs[0].DocType);
			AssertEquals("FileName should be: " + fileName, fileName, docManagerInfo.AllEDocs[0].FileName);
		}

		public void TestRelatedObjects()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAction = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp,
				ActionTypes.Codes.PickUp).PickupAction;
			var deliveryAction = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery,
				ActionTypes.Codes.Delivery).DeliveryAction;

			var docManagerInfo = consignment.DocManagerInfo();
			var relatedObjects = docManagerInfo.RelatedObjects;

			AssertContainsExactElementsInAnyOrder("The action should be included in the consignment's doc manager's related items because we need to see the action's edocs on the consignment's edocs form.",
				new[] { pickupAction, deliveryAction }, relatedObjects);
		}

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<DtbConsignment>();
		}

		#endregion
	}
}
