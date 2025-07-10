using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbBookingConsignmentDocManagerInfo))]
	class DtbBookingConsignmentDocManagerInfoTest : DtbTransportDocManagerInfoTest<DtbBookingConsignment, DtbBookingConsignmentDocManagerInfo>
	{
		#region GetPopulatedParentBusinessObject

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var consignment = Helper.CreateBookingConsignment();
			var booking = Helper.CreateBooking("BOOKING", consignment);

			return consignment;
		}

		#endregion

		#region TestGetRelatedeDocs

		public void TestGetRelatedeDocs()
		{
			string docType = "MSC", bookingFile = "bookingFile", consignmentFile = "ConsignmentFile";

			var consignment = Helper.CreateBookingConsignment();
			var booking = Helper.CreateBooking("BOOKING", consignment);

			//booking
			var bookingDocManagerInfo = ((IDocManagerSupport)consignment.ConsolidationSingleJob.Parent).DocManagerInfo;
			bookingDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, bookingFile, docType, true);

			var consignmentDocManagerInfo = ((IDocManagerSupport)consignment).DocManagerInfo;
			consignmentDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, consignmentFile, docType, true);

			var relatedObjects = new DtbBookingConsignmentDocManagerInfo(consignment).RelatedObjects;
			AssertContainsExactElementsInAnyOrder(relatedObjects, new[] { booking });
			AssertDocumentTypeAndFileName(((IDocManagerSupport)relatedObjects[0]).DocManagerInfo, 1, docType, bookingFile);
			AssertDocumentTypeAndFileName(consignmentDocManagerInfo, 1, docType, consignmentFile);
		}

		void AssertDocumentTypeAndFileName(DocManagerInfo docManagerInfo, int totalDoc, string docType, string fileName)
		{
			AssertEquals("Total file count should be: " + totalDoc.ToString(), totalDoc, docManagerInfo.AllEDocs.Count);
			AssertEquals("DocType should be: " + docType, docType, docManagerInfo.AllEDocs[0].DocType);
			AssertEquals("FileName should be: " + fileName, fileName, docManagerInfo.AllEDocs[0].FileName);
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
