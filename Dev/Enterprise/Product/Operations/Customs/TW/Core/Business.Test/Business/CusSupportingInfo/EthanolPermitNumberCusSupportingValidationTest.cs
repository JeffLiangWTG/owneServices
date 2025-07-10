using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EthanolPermitNumberCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var ethanolPermitNumber = messageHeader.EthanolPermitNumbers.AddNew();
			NUnit.Framework.Assert.That(ethanolPermitNumber.Validation.Parent, NUnit.Framework.Is.EqualTo(ethanolPermitNumber), "Parent");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var ethanolPermitNumber1 = messageHeader.EthanolPermitNumbers.AddNew();
			ethanolPermitNumber1.CSI_ReferenceNumber = "EPJ11010005387";
			var ethanolPermitNumber2 = messageHeader.EthanolPermitNumbers.AddNew();
			ethanolPermitNumber2.CSI_ReferenceNumber = "EPJ11010005387";
			var targetInfo = ethanolPermitNumber2.CSI_ReferenceNumberInfo;
			AssertHasMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.EthanolPermitNumberCannotBeDuplicated);
			ethanolPermitNumber2.CSI_ReferenceNumber = "EPJ11010005686";
			AssertNoMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.EthanolPermitNumberCannotBeDuplicated);
			ethanolPermitNumber2.CSI_ReferenceNumber = "ss123";
			AssertHasMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
			ethanolPermitNumber2.CSI_ReferenceNumber = "SS123";
			AssertNoMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
			ethanolPermitNumber2.CSI_ReferenceNumber = "!SS123";
			AssertHasMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
			ethanolPermitNumber2.CSI_ReferenceNumber = "AAA";
			AssertNoMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
			ethanolPermitNumber2.CSI_ReferenceNumber = "SS123@";
			AssertHasMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
			ethanolPermitNumber2.CSI_ReferenceNumber = "123";
			AssertNoMessageError(targetInfo, ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
		}

		public void TestCheckCSI_ReferenceNumberLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var ethanolPermitNumber = messageHeader.EthanolPermitNumbers.AddNew();
			var targetInfo = ethanolPermitNumber.CSI_ReferenceNumberInfo;
			var messageError = "Ethanol Permit Number must consist 14 alphanumeric characters.";
			ethanolPermitNumber.CSI_ReferenceNumber = ZString.Empty;
			ethanolPermitNumber.RunPreSaveValidation();
			AssertNoMessageError(targetInfo, messageError);
			ethanolPermitNumber.CSI_ReferenceNumber = "EPJ110100053F";
			AssertHasMessageError(targetInfo, messageError);
			ethanolPermitNumber.CSI_ReferenceNumber = "EPJ11010005387";
			AssertNoMessageError(targetInfo, messageError);
			ethanolPermitNumber.CSI_ReferenceNumber = "EPJ1101000536";
			AssertHasMessageError(targetInfo, messageError);
			ethanolPermitNumber.CSI_ReferenceNumber = "EPJ11010005386";
			AssertNoMessageErrors(targetInfo);
		}
	}
}
