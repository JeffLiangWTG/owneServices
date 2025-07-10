using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PermitCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			var parent = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PermitCusSupportingCollection.AddNew();
			NUnit.Framework.Assert.That(parent.Validation.Parent, NUnit.Framework.Is.EqualTo(parent), "Parent");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			var targetInfo = permitCusSupporting.CSI_ReferenceNumberInfo;
			var messageError = "Permit No. must consist 14 alphanumeric characters.";
			permitCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			permitCusSupporting.RunPreSaveValidation();
			AssertNoMessageError(targetInfo, messageError);
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ110100053F";
			AssertHasMessageError(targetInfo, messageError);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005387";
			AssertNoMessageError(targetInfo, messageError);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ1101000536";
			AssertHasMessageError(targetInfo, messageError);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005386";
			AssertNoMessageErrors(targetInfo);
			permitCusSupporting.CSI_ReferenceNumber = "!PJ11010005386";
			AssertHasMessageError(targetInfo, messageError);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ1101000538@";
			AssertHasMessageError(targetInfo, messageError);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005387";
			AssertNoMessageErrors(targetInfo);
			permitCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			permitCusSupporting.CSI_LineNo = 1;
			permitCusSupporting.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005387";
			permitCusSupporting.RunPreSaveValidation();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			CombineAssertions("CheckPermitNumbersPlusSpecialCodeNumber", () =>
			{
				var numbersExceedFiveMessage = "The sum of the number of Import/Export Permit plus the number of Special Code for Exemption of Controlling Agencies should not exceed 5.";
				invoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
				invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
				invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
				invoiceLine.PermitCusSupportingCollection.AddNew();
				invoiceLine.PermitCusSupportingCollection.AddNew();
				permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
				targetInfo = permitCusSupporting.CSI_ReferenceNumberInfo;
				permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005386";
				AssertNoMessageError(targetInfo, numbersExceedFiveMessage);
				permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
				targetInfo = permitCusSupporting.CSI_ReferenceNumberInfo;
				permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005387";
				AssertHasMessageError(targetInfo, numbersExceedFiveMessage);
			}

			);
		}

		public void TestCheckCSI_LineNo()
		{
			var permitCusSupporting = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PermitCusSupportingCollection.AddNew();
			var targetInfo = permitCusSupporting.CSI_LineNoInfo;
			permitCusSupporting.RunPreSaveValidation();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			permitCusSupporting.CSI_ReferenceNumber = "EPJ11010005387";
			permitCusSupporting.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			permitCusSupporting.CSI_LineNo = 1;
			permitCusSupporting.RunPreSaveValidation();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			permitCusSupporting.CSI_LineNo = 10000;
			AssertHasError(targetInfo, "Permit Item Number cannot enter value more than 4 length.");
			permitCusSupporting.CSI_LineNo = 9999;
			AssertNoError(targetInfo, "Permit Item Number cannot enter value more than 4 length.");
		}
	}
}
