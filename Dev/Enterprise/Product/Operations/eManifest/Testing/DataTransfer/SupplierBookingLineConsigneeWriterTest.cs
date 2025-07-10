using CargoWise.EntityFramework.Testing;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.eManifest.Testing.DataTransfer.Testing
{
	public class SupplierBookingLineConsigneeWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var dataWritingManager = new Mock<IDataWritingManager>();
			var writer = new SupplierBookingLineConsigneeAddressWriter(dataWritingManager.Object);
			var supplierBookingLine = Factory.New<SupplierBookingLine>();

			const string consigneeName = "company";
			const string consigneeAddress1 = "address1";
			const string consigneeAddress2 = "address2";
			const string consigneeCity = "city";
			const string consigneeState = "state";
			const string consigneePostCode = "postcode";
			const string consigneeContact = "contact";
			const string consigneePhone = "phone";
			const string consigneeMobile = "mobile";
			const string consigneeFax = "fax";
			const string consigneeCountryCode = "AU";

			supplierBookingLine.DL_ConsigneeName = consigneeName;
			supplierBookingLine.DL_ConsigneeAddress1 = consigneeAddress1;
			supplierBookingLine.DL_ConsigneeAddress2 = consigneeAddress2;
			supplierBookingLine.DL_ConsigneeCity = consigneeCity;
			supplierBookingLine.DL_ConsigneeState = consigneeState;
			supplierBookingLine.DL_ConsigneePostCode = consigneePostCode;
			supplierBookingLine.DL_RN_NKConsigneeCountryCode = consigneeCountryCode;
			supplierBookingLine.DL_ConsigneeContact = consigneeContact;
			supplierBookingLine.DL_ConsigneePhone = consigneePhone;
			supplierBookingLine.DL_ConsigneeMobile = consigneeMobile;
			supplierBookingLine.DL_ConsigneeFax = consigneeFax;

			var dataObject = writer.GetDataObject(supplierBookingLine);

			AssertEquals(consigneeName, dataObject.CompanyName);
			AssertEquals(consigneeAddress1, dataObject.Address1);
			AssertEquals(consigneeAddress2, dataObject.Address2);
			AssertEquals(consigneeCity, dataObject.City);
			AssertEquals(consigneeState, dataObject.State);
			AssertEquals(consigneePostCode, dataObject.Postcode);
			AssertNotNull(dataObject.Country);
			AssertEquals(consigneeCountryCode, dataObject.Country.Code);
			AssertEquals(consigneeContact, dataObject.Contact);
			AssertEquals(consigneePhone, dataObject.Phone);
			AssertEquals(consigneeMobile, dataObject.Mobile);
			AssertEquals(consigneeFax, dataObject.Fax);
		}

		public void TestPopulateDataObjectWhenSupplierBookingLineIsNull()
		{
			var dataWritingManager = new Mock<IDataWritingManager>();
			var writer = new SupplierBookingLineConsigneeAddressWriter(dataWritingManager.Object);

			AssertNull(writer.GetDataObject(null));
		}

		public void TestPopulateDataObjectWhenSupplierBookingLineAddressIsEmpty()
		{
			var dataWritingManager = new Mock<IDataWritingManager>();
			var writer = new SupplierBookingLineConsigneeAddressWriter(dataWritingManager.Object);
			var supplierBookingLine = Factory.New<SupplierBookingLine>();

			supplierBookingLine.DL_ConsigneeName = "";
			supplierBookingLine.DL_ConsigneeAddress1 = "";
			supplierBookingLine.DL_ConsigneeAddress2 = "";
			supplierBookingLine.DL_ConsigneeCity = "";
			supplierBookingLine.DL_ConsigneeState = "";
			supplierBookingLine.DL_ConsigneePostCode = "";
			supplierBookingLine.DL_RN_NKConsigneeCountryCode = "";
			supplierBookingLine.DL_ConsigneeContact = "";
			supplierBookingLine.DL_ConsigneePhone = "";
			supplierBookingLine.DL_ConsigneeMobile = "";
			supplierBookingLine.DL_ConsigneeFax = "";

			AssertNull(writer.GetDataObject(null));
		}
	}
}
