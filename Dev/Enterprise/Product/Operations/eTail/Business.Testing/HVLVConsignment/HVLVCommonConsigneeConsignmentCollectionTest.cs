using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVCommonConsigneeConsignmentCollection))]
	public class HVLVCommonConsigneeConsignmentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHVLVCommonConsigneeConsignmentCollectionContainsImportConsignmentsUnderSameConsignmentHeader()
		{
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			var importConsignment = consignmentHeader.Consignments.AddNew();
			var importSiblingConsignment = consignmentHeader.Consignments.AddNew();
			var domesticSiblingConsignment = consignmentHeader.Consignments.AddNew();

			importConsignment.HVC_ConsigneeName = "Test Consignee";
			importSiblingConsignment.HVC_ConsigneeName = "Test Consignee";
			domesticSiblingConsignment.HVC_ConsigneeName = "Test Consignee";
			importConsignment.HVC_ConsigneeAddress1 = "Test Address 1";
			importSiblingConsignment.HVC_ConsigneeAddress1 = "Test Address 1";
			domesticSiblingConsignment.HVC_ConsigneeAddress1 = "Test Address 1";
			importConsignment.HVC_ConsigneeAddress2 = "Test Address 2";
			importSiblingConsignment.HVC_ConsigneeAddress2 = "Test Address 2";
			domesticSiblingConsignment.HVC_ConsigneeAddress2 = "Test Address 2";
			importConsignment.HVC_ConsigneePostcode = "1234";
			importSiblingConsignment.HVC_ConsigneePostcode = "1234";
			domesticSiblingConsignment.HVC_ConsigneePostcode = "1234";
			importConsignment.HVC_ConsigneeCity = "Sydney";
			importSiblingConsignment.HVC_ConsigneeCity = "Sydney";
			domesticSiblingConsignment.HVC_ConsigneeCity = "Sydney";
			importConsignment.HVC_ConsigneeState = "NSW";
			importSiblingConsignment.HVC_ConsigneeState = "NSW";
			domesticSiblingConsignment.HVC_ConsigneeState = "NSW";
			importConsignment.HVC_RN_NKConsigneeCountryCode = "AU";
			importSiblingConsignment.HVC_RN_NKConsigneeCountryCode = "AU";
			domesticSiblingConsignment.HVC_RN_NKConsigneeCountryCode = "AU";
			importConsignment.HVC_IsActive = true;
			importSiblingConsignment.HVC_IsActive = true;
			domesticSiblingConsignment.HVC_IsActive = true;
			importConsignment.HVC_RN_NKShipperCountryCode = "NZ";
			importSiblingConsignment.HVC_RN_NKShipperCountryCode = "NZ";
			domesticSiblingConsignment.HVC_RN_NKShipperCountryCode = "AU";

			var collection = new HVLVCommonConsigneeConsignmentCollection(importConsignment);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new List<HVLVConsignment> { importConsignment, importSiblingConsignment }, collection.Cast<HVLVConsignment>());
		}

		public void TestHVLVCommonConsigneeConsignmentCollectionContainsExportConsignmentsUnderSameConsignmentHeader()
		{
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			var exportConsignment = consignmentHeader.Consignments.AddNew();
			var exportSiblingConsignment = consignmentHeader.Consignments.AddNew();
			var domesticSiblingConsignment = consignmentHeader.Consignments.AddNew();

			exportConsignment.HVC_ConsigneeName = "Test Consignee";
			exportSiblingConsignment.HVC_ConsigneeName = "Test Consignee";
			domesticSiblingConsignment.HVC_ConsigneeName = "Test Consignee";
			exportConsignment.HVC_ConsigneeAddress1 = "Test Address 1";
			exportSiblingConsignment.HVC_ConsigneeAddress1 = "Test Address 1";
			domesticSiblingConsignment.HVC_ConsigneeAddress1 = "Test Address 1";
			exportConsignment.HVC_ConsigneeAddress2 = "Test Address 2";
			exportSiblingConsignment.HVC_ConsigneeAddress2 = "Test Address 2";
			domesticSiblingConsignment.HVC_ConsigneeAddress2 = "Test Address 2";
			exportConsignment.HVC_ConsigneePostcode = "1234";
			exportSiblingConsignment.HVC_ConsigneePostcode = "1234";
			domesticSiblingConsignment.HVC_ConsigneePostcode = "1234";
			exportConsignment.HVC_ConsigneeCity = "Sydney";
			exportSiblingConsignment.HVC_ConsigneeCity = "Sydney";
			domesticSiblingConsignment.HVC_ConsigneeCity = "Sydney";
			exportConsignment.HVC_ConsigneeState = "NSW";
			exportSiblingConsignment.HVC_ConsigneeState = "NSW";
			domesticSiblingConsignment.HVC_ConsigneeState = "NSW";
			exportConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			exportSiblingConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			domesticSiblingConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			exportConsignment.HVC_IsActive = true;
			exportSiblingConsignment.HVC_IsActive = true;
			domesticSiblingConsignment.HVC_IsActive = true;
			exportConsignment.HVC_RN_NKShipperCountryCode = "AU";
			exportSiblingConsignment.HVC_RN_NKShipperCountryCode = "AU";
			domesticSiblingConsignment.HVC_RN_NKShipperCountryCode = "NZ";

			var collection = new HVLVCommonConsigneeConsignmentCollection(exportConsignment);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new List<HVLVConsignment> { exportConsignment, exportSiblingConsignment }, collection.Cast<HVLVConsignment>());
		}

		public void TestCollectionReturnsNoResultWhenConsignmentHasNoBookingHeader()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var collection = new HVLVCommonConsigneeConsignmentCollection(consignment);
			collection.Load();

			AssertEquals("Should return no result", 0, collection.Count);
		}

		public void TestCollectionContainsActiveConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsigneeName = "Test Consignee";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment1.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment1.HVC_ConsigneePostcode = "1234";
			consignment1.HVC_ConsigneeCity = "Sydney";
			consignment1.HVC_ConsigneeState = "NSW";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.HVC_IsActive = true;

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsigneeName = "Test Consignee";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment2.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment2.HVC_ConsigneePostcode = "1234";
			consignment2.HVC_ConsigneeCity = "Sydney";
			consignment2.HVC_ConsigneeState = "NSW";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.HVC_IsActive = true;

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsigneeName = "Test Consignee";
			consignment3.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment3.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment3.HVC_ConsigneePostcode = "1234";
			consignment3.HVC_ConsigneeCity = "Sydney";
			consignment3.HVC_ConsigneeState = "NSW";
			consignment3.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment3.HVC_IsActive = false;

			Factory.Save();

			var collection = new HVLVCommonConsigneeConsignmentCollection(consignment1);
			collection.Load();

			AssertEquals("Should contain 2 consignments", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, collection.Select(x => x.PK));
		}

		public void TestCollectionContainsConsignmentsOfSameBookingHeader()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsigneeName = "Test Consignee";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment1.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment1.HVC_ConsigneePostcode = "1234";
			consignment1.HVC_ConsigneeCity = "Sydney";
			consignment1.HVC_ConsigneeState = "NSW";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			var consignment2 = bookingHeader1.Consignments.AddNew();
			consignment2.HVC_ConsigneeName = "Test Consignee";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment2.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment2.HVC_ConsigneePostcode = "1234";
			consignment2.HVC_ConsigneeCity = "Sydney";
			consignment2.HVC_ConsigneeState = "NSW";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment3 = bookingHeader2.Consignments.AddNew();
			consignment3.HVC_ConsigneeName = "Test Consignee";
			consignment3.HVC_ConsigneeAddress1 = "Test Address 1";
			consignment3.HVC_ConsigneeAddress2 = "Test Address 2";
			consignment3.HVC_ConsigneePostcode = "1234";
			consignment3.HVC_ConsigneeCity = "Sydney";
			consignment3.HVC_ConsigneeState = "NSW";
			consignment3.HVC_RN_NKConsigneeCountryCode = "AU";

			Factory.Save();

			var collection = new HVLVCommonConsigneeConsignmentCollection(consignment1);
			collection.Load();

			AssertEquals("Should contain 2 consignments", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, collection.Select(x => x.PK));
		}

		public void TestCollectionContainsConsignmentsBelongToSameConsignee()
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

			var collection = new HVLVCommonConsigneeConsignmentCollection(consignment1);
			collection.Load();

			AssertEquals("Should contain 2 consignments", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, collection.Select(x => x.PK));
		}

		#region Implementations

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = "NZ";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			return new HVLVCommonConsigneeConsignmentCollection(consignment);
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
