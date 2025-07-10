using CargoWise.EntityFramework.Testing;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.eManifest.Testing.DataTransfer.Testing
{
	public class SupplierBookingLineConsignorWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var dataWritingManager = new Mock<IDataWritingManager>();
			var writer = new SupplierBookingLineConsignorAddressWriter(dataWritingManager.Object);
			var supplierBookingLine = Factory.New<SupplierBookingLine>();

			const string consignorName = "company";
			const string consignorAddress1 = "address1";
			const string consignorAddress2 = "address2";
			const string consignorCity = "city";
			const string consignorState = "state";
			const string consignorPostCode = "postcode";
			const string consignorContact = "contact";
			const string consignorPhone = "phone";
			const string consignorMobile = "mobile";
			const string consignorFax = "fax";
			const string consignorCountryCode = "AU";

			supplierBookingLine.DL_ConsignorName = consignorName;
			supplierBookingLine.DL_ConsignorAddress1 = consignorAddress1;
			supplierBookingLine.DL_ConsignorAddress2 = consignorAddress2;
			supplierBookingLine.DL_ConsignorCity = consignorCity;
			supplierBookingLine.DL_ConsignorState = consignorState;
			supplierBookingLine.DL_ConsignorPostCode = consignorPostCode;
			supplierBookingLine.DL_RN_NKConsignorCountryCode = consignorCountryCode;
			supplierBookingLine.DL_ConsignorContact = consignorContact;
			supplierBookingLine.DL_ConsignorPhone = consignorPhone;
			supplierBookingLine.DL_ConsignorMobile = consignorMobile;
			supplierBookingLine.DL_ConsignorFax = consignorFax;

			var dataObject = writer.GetDataObject(supplierBookingLine);

			AssertEquals(consignorName, dataObject.CompanyName);
			AssertEquals(consignorAddress1, dataObject.Address1);
			AssertEquals(consignorAddress2, dataObject.Address2);
			AssertEquals(consignorCity, dataObject.City);
			AssertEquals(consignorState, dataObject.State);
			AssertEquals(consignorPostCode, dataObject.Postcode);
			AssertNotNull(dataObject.Country);
			AssertEquals(consignorCountryCode, dataObject.Country.Code);
			AssertEquals(consignorContact, dataObject.Contact);
			AssertEquals(consignorPhone, dataObject.Phone);
			AssertEquals(consignorMobile, dataObject.Mobile);
			AssertEquals(consignorFax, dataObject.Fax);
		}

		public void TestPopulateDataObjectWhenSupplierBookingLineIsNull()
		{
			var dataWritingManager = new Mock<IDataWritingManager>();
			var writer = new SupplierBookingLineConsignorAddressWriter(dataWritingManager.Object);

			AssertNull(writer.GetDataObject(null));
		}

		public void TestPopulateDataObjectWhenSupplierBookingLineAddressIsEmpty()
		{
			var dataWritingManager = new Mock<IDataWritingManager>();
			var writer = new SupplierBookingLineConsignorAddressWriter(dataWritingManager.Object);
			var supplierBookingLine = Factory.New<SupplierBookingLine>();

			supplierBookingLine.DL_ConsignorName = "";
			supplierBookingLine.DL_ConsignorAddress1 = "";
			supplierBookingLine.DL_ConsignorAddress2 = "";
			supplierBookingLine.DL_ConsignorCity = "";
			supplierBookingLine.DL_ConsignorState = "";
			supplierBookingLine.DL_ConsignorPostCode = "";
			supplierBookingLine.DL_RN_NKConsignorCountryCode = "";
			supplierBookingLine.DL_ConsignorContact = "";
			supplierBookingLine.DL_ConsignorPhone = "";
			supplierBookingLine.DL_ConsignorMobile = "";
			supplierBookingLine.DL_ConsignorFax = "";

			AssertNull(writer.GetDataObject(null));
		}
	}
}
