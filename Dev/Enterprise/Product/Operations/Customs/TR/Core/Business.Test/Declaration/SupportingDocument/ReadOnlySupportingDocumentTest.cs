using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ReadOnlySupportingDocument))]
	public class ReadOnlySupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Supporting Document", () => new ReadOnlySupportingDocument(null));
		}

		public void CSI_StatusDescription()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Status = "A";
			var readOnlySupDoc = new ReadOnlySupportingDocument(supportingDocument);

			AssertEquals("CSI_CodeDescription is empty when CSI_Status is not in SupportingDocumentAvailabilityList", "", readOnlySupDoc.CSI_CodeDescription);

			supportingDocument.CSI_Status = SupportingDocumentAvailabilityList.Codes.V;
			readOnlySupDoc = new ReadOnlySupportingDocument(supportingDocument);

			AssertEquals("CSI_CodeDescription should return the respective Description of CSI_Status code", SupportingDocumentAvailabilityList.Descriptions.V, readOnlySupDoc.CSI_CodeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var supportingDocument = GetSupportingDoc(1, "REF111");

			return new ReadOnlySupportingDocument(supportingDocument);
		}

		protected SupportingDocument GetSupportingDoc(int i, ZString refNumber, decimal qty3 = 10.0m)
		{
			var supDoc = Factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_SubType = "A";

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Quantity3 = qty3;

			supDoc.CSI_Description = "Testing";
			supDoc.CSI_ReferenceNumber2 = "REFNUM2";
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_CustomsOffice = "ABC";
			supDoc.CSI_Procedure = "X";
			supDoc.CSI_RN_NKCountryCode = "GB";
			supDoc.CSI_Status = "V";
			supDoc.CSI_Tariff = "12345";
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_UnitOfQuantity3 = "U3";

			return supDoc;
		}
	}
}
