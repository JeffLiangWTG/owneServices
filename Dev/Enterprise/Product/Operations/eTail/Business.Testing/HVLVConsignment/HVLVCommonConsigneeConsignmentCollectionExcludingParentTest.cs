using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVCommonConsigneeConsignmentCollectionExcludingParent))]
	public class HVLVCommonConsigneeConsignmentCollectionExcludingParentTest : BusinessObjectCollectionTestCase
	{
		public void TestExcludeParent()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsigneeName = "Test Consignee";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment1.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment1.HVC_ConsigneePostcode = "1234";
			consignment1.HVC_ConsigneeCity = "Sydney";
			consignment1.HVC_ConsigneeState = "NSW";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsigneeName = "Test Consignee";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment2.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment2.HVC_ConsigneePostcode = "1234";
			consignment2.HVC_ConsigneeCity = "Sydney";
			consignment2.HVC_ConsigneeState = "NSW";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsigneeName = "Test Different Consignee";
			consignment3.HVC_ConsigneeAddress1 = "Test Address 3";
			consignment3.HVC_ConsigneeAddress2 = "Test Address 4";
			consignment3.HVC_ConsigneePostcode = "4321";
			consignment3.HVC_ConsigneeCity = "Aukland";
			consignment3.HVC_ConsigneeState = "AKL";
			consignment3.HVC_RN_NKConsigneeCountryCode = "NZ";

			var collection = new HVLVCommonConsigneeConsignmentCollectionExcludingParent(consignment1);
			collection.Load();

			AssertEquals("Should contain 1 consignment", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2.PK }, collection.Select(x => x.PK));
		}

		public void TestExcludeConsignmentsWithDeclaration()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsigneeName = "Test Consignee";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment1.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment1.HVC_ConsigneePostcode = "1234";
			consignment1.HVC_ConsigneeCity = "Sydney";
			consignment1.HVC_ConsigneeState = "NSW";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.HVC_JE_ImportDeclaration = default;
			consignment1.HVC_JE_ExportDeclaration = default;

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsigneeName = "Test Consignee";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment2.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment2.HVC_ConsigneePostcode = "1234";
			consignment2.HVC_ConsigneeCity = "Sydney";
			consignment2.HVC_ConsigneeState = "NSW";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.HVC_JE_ImportDeclaration = default;
			consignment2.HVC_JE_ExportDeclaration = default;

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsigneeName = "Test Consignee";
			consignment3.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment3.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment3.HVC_ConsigneePostcode = "1234";
			consignment3.HVC_ConsigneeCity = "Sydney";
			consignment3.HVC_ConsigneeState = "NSW";
			consignment3.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment3.HVC_JE_ImportDeclaration = default;
			consignment3.HVC_JE_ExportDeclaration = ZGuid.BrettsGuid;

			var consignment4 = bookingHeader.Consignments.AddNew();
			consignment4.HVC_ConsigneeName = "Test Consignee";
			consignment4.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment4.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment4.HVC_ConsigneePostcode = "1234";
			consignment4.HVC_ConsigneeCity = "Sydney";
			consignment4.HVC_ConsigneeState = "NSW";
			consignment4.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment4.HVC_JE_ImportDeclaration = ZGuid.BrettsGuid;
			consignment4.HVC_JE_ExportDeclaration = default;

			var collection = new HVLVCommonConsigneeConsignmentCollectionExcludingParent(consignment1);
			collection.Load();

			AssertEquals("Should contain 1 consignment", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2.PK }, collection.Select(x => x.PK));
		}

		#region Implementations

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = "NZ";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			return new HVLVCommonConsigneeConsignmentCollectionExcludingParent(consignment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = "NZ";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			return consignment;
		}

		#endregion
	}
}
