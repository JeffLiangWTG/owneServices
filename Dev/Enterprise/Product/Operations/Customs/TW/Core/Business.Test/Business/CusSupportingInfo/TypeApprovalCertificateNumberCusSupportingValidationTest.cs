using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TypeApprovalCertificateNumberCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			NUnit.Framework.Assert.That(TypeApprovalCertificateNumberCusSupporting.Validation.Parent, NUnit.Framework.Is.TypeOf(typeof(TypeApprovalCertificateNumberCusSupporting)));
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber = "XXXXXXXXXXXXX1";
			AssertNoMessageErrors(TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumberInfo);
			TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber = "XXX";
			AssertHasMessageError(TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumberInfo, ValidationConstants.TypeApprovalCertificateNumber.LengthForCertificateNo);
			TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber = "!XXXXXXXXXXXXX";
			AssertHasMessageError(TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumberInfo, ValidationConstants.TypeApprovalCertificateNumber.AlphanumericCharactersOnlyForCertificateNo);
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber2 = "XXXXXXXXXXXXX1";
			AssertNoMessageErrors(TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber2Info);
			TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber2 = "!!!@@33333";
			AssertHasMessageError(TypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber2Info, ValidationConstants.TypeApprovalCertificateNumber.AlphanumericCharactersOnlyForAuthorizedParty);
		}

		public void TestCheckCSI_Description()
		{
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(InvoiceLine.EntryInstruction.ControllingMessageHeaders, new string[] { "20" });
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(InvoiceLine, "20", true);
			TypeApprovalCertificateNumberCusSupporting.CSI_Description = "XXX";
			AssertHasMessageErrorContaining(TypeApprovalCertificateNumberCusSupporting.CSI_DescriptionInfo, ListValidation.InvalidCodeMessageError);
			TypeApprovalCertificateNumberCusSupporting.CSI_Description = PartyIdentifierCodeList.Codes._174;
			AssertNoMessageErrorContaining(TypeApprovalCertificateNumberCusSupporting.CSI_DescriptionInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_Code()
		{
			TypeApprovalCertificateNumberCusSupporting.CSI_Code = "X";
			AssertHasMessageErrorContaining(TypeApprovalCertificateNumberCusSupporting.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			TypeApprovalCertificateNumberCusSupporting.CSI_Code = ExemptionCodeList.Codes.A;
			AssertNoMessageErrorContaining(TypeApprovalCertificateNumberCusSupporting.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_DescriptionWithReadOnly()
		{
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(InvoiceLine.EntryInstruction.ControllingMessageHeaders, new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" });
			AssertCSI_DescriptionInfoHasMessageErrorContainingByControllingAgency("20", MandatoryValidation.YouHaveNotEntered);
			AssertCSI_DescriptionInfoHasMessageErrorContainingByControllingAgency("CI", MandatoryValidation.YouHaveNotEntered);
			AssertCSI_DescriptionInfoHasMessageErrorContainingByControllingAgency("2Q", MandatoryValidation.YouHaveNotEntered);
		}

		void AssertCSI_DescriptionInfoHasMessageErrorContainingByControllingAgency(ZString controllingAgency, ZString message)
		{
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(InvoiceLine, controllingAgency, false);
			AssertNoMessageErrorContaining(InvoiceLine.TypeApprovalCertificateNumbers.CSI_DescriptionInfo, message);
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(InvoiceLine, controllingAgency, true);
			InvoiceLine.TypeApprovalPartyIdentifier = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.TypeApprovalCertificateNumbers.CSI_DescriptionInfo, message);
			InvoiceLine.TypeApprovalPartyIdentifier = "1";
			AssertNoMessageErrorContaining(InvoiceLine.TypeApprovalCertificateNumbers.CSI_DescriptionInfo, message);
		}

		public void TestCheckCSI_ReferenceNumber2ReadOnly()
		{
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(InvoiceLine.EntryInstruction.ControllingMessageHeaders, new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" });
			AssertCSI_ReferenceNumber2InfoHasMessageErrorContainingByControllingAgency("20", MandatoryValidation.YouHaveNotEntered);
			AssertCSI_ReferenceNumber2InfoHasMessageErrorContainingByControllingAgency("CI", MandatoryValidation.YouHaveNotEntered);
			AssertCSI_ReferenceNumber2InfoHasMessageErrorContainingByControllingAgency("2Q", MandatoryValidation.YouHaveNotEntered);
		}

		void AssertCSI_ReferenceNumber2InfoHasMessageErrorContainingByControllingAgency(ZString controllingAgency, ZString message)
		{
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(InvoiceLine, controllingAgency, false);
			AssertNoMessageErrorContaining(InvoiceLine.TypeApprovalCertificateNumbers.CSI_ReferenceNumber2Info, message);
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(InvoiceLine, controllingAgency, true);
			InvoiceLine.TypeApprovalAuthorizedParty = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.TypeApprovalCertificateNumbers.CSI_ReferenceNumber2Info, message);
			InvoiceLine.TypeApprovalAuthorizedParty = "1";
			AssertNoMessageErrorContaining(InvoiceLine.TypeApprovalCertificateNumbers.CSI_ReferenceNumber2Info, message);
		}

		TypeApprovalCertificateNumberCusSupporting TypeApprovalCertificateNumberCusSupporting => InvoiceLine.TypeApprovalCertificateNumbers;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var jobDeclaration = Factory.New<JobDeclaration>();
					var entryInstruction = jobDeclaration.CusEntryInstruction;
					invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
					invoiceLine.JI_CEI = entryInstruction.PK;
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
