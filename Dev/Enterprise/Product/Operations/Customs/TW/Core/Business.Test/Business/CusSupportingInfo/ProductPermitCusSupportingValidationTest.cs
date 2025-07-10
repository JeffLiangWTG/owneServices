using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ProductPermitCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			var parent = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew().ProductPermitCusSupportingCollection.AddNew();
			NUnit.Framework.Assert.That(parent.Validation.Parent, NUnit.Framework.Is.EqualTo(parent), "Parent");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var warningMessage = "Permit Number must consist 14 alphanumeric characters.";
			var duplicatedMessage = "Permit Number and Permit Item Number combinations have been duplicated and must be unique.";
			var productPermitCusSupportingCollection = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew().ProductPermitCusSupportingCollection;
			var productPermitCusSupporting = productPermitCusSupportingCollection.AddNew();
			productPermitCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(productPermitCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			productPermitCusSupporting.CSI_LineNo = 1;
			productPermitCusSupporting.CSI_ReferenceNumber = "1234567890";
			AssertHasWarning(productPermitCusSupporting.CSI_ReferenceNumberInfo, warningMessage);
			productPermitCusSupporting.CSI_LineNo = 1;
			productPermitCusSupporting.CSI_ReferenceNumber = "12345678901234";
			AssertNoMessageErrors(productPermitCusSupporting.CSI_ReferenceNumberInfo);
			var productPermitCusSupporting2 = productPermitCusSupportingCollection.AddNew();
			productPermitCusSupporting2.CSI_LineNo = 1;
			productPermitCusSupporting2.CSI_ReferenceNumber = "12345678901234";
			AssertHasMessageError(productPermitCusSupporting2.CSI_ReferenceNumberInfo, duplicatedMessage);
		}

		public void TestCheckCSI_LineNo()
		{
			var duplicatedMessage = "Permit Number and Permit Item Number combinations have been duplicated and must be unique.";
			var productPermitCusSupportingCollection = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew().ProductPermitCusSupportingCollection;
			var productPermitCusSupporting = productPermitCusSupportingCollection.AddNew();
			productPermitCusSupporting.CSI_LineNo = 1;
			productPermitCusSupporting.CSI_ReferenceNumber = "12345678901234";
			var productPermitCusSupporting2 = productPermitCusSupportingCollection.AddNew();
			productPermitCusSupporting2.CSI_LineNo = 1;
			productPermitCusSupporting2.CSI_ReferenceNumber = "12345678901234";
			AssertHasMessageError(productPermitCusSupporting2.CSI_ReferenceNumberInfo, duplicatedMessage);
		}
	}
}
