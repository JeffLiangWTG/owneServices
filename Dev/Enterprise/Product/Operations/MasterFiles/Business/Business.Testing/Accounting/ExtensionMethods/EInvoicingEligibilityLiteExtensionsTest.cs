using System.Linq;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EInvoicingEligibilityLiteExtensionsTest : TestCase
	{
		public void TestIsDisbursementInvoice()
		{
			IEInvoicingEligibilityLiteTransaction nullTransaction = null;
			AssertEquals("Null transaction is not a disbursement", false, nullTransaction.IsDisbursementInvoice());

			var expectedDisbursementCategories = AccTransactionHeader.DisbursementInvoiceTypes;
			var transaction = new FakeEligibilityLiteTransaction();
			foreach (var category in new InvoiceTypesList().GetAllCodes())
			{
				transaction.TransactionCategory = category;
				var expectedIsDisbursement = expectedDisbursementCategories.Contains(category);

				AssertEquals($"TransactionCategory '{category}'", expectedIsDisbursement, transaction.IsDisbursementInvoice());
			}
		}

		public void TestHasAnyRegistrationCode()
		{
			var orgHeader = new FakeEligibilityLiteOrgHeader()
					.WithRegistrationCode(countryCode: "AU", codeType: "GST", registrationNumber: "AU1234GST")
					.WithRegistrationCode(countryCode: "ZZ", codeType: "999", registrationNumber: "ZZ1234999")
					.WithRegistrationCode(countryCode: "AU", codeType: "999", registrationNumber: "");

			Assert("Contains GST code for AU country", orgHeader.HasAnyRegistrationCode("GST", "AU"));
			Assert("Contains 999 code for ZZ country", orgHeader.HasAnyRegistrationCode("999", "ZZ"));
			Assert("Does not contain GST code for ZZ country", !orgHeader.HasAnyRegistrationCode("GST", "ZZ"));
			Assert("Does not contain VAT code for ZZ country", !orgHeader.HasAnyRegistrationCode("VAT", "ZZ"));
			Assert("Does not contain VAT code for NZ country", !orgHeader.HasAnyRegistrationCode("VAT", "NZ"));
			Assert("Does not contain 999 code for AU country", !orgHeader.HasAnyRegistrationCode("999", "AU"));
		}

		public void TestHasRegistrationCode()
		{
			var orgHeader = new FakeEligibilityLiteOrgHeader()
					.WithRegistrationCode(countryCode: "AU", codeType: "GST", registrationNumber: "AU1234GST")
					.WithRegistrationCode(countryCode: "ZZ", codeType: "999", registrationNumber: "ZZ1234999");

			Assert("Contains GST code with AU1234GST number for AU country", orgHeader.HasRegistrationCode("GST", "AU", "AU1234GST"));
			Assert("Contains 999 code with ZZ1234999 number for ZZ country", orgHeader.HasRegistrationCode("999", "ZZ", "ZZ1234999"));
			Assert("Does not contain GST code with ZZ1234999 number for AU country", !orgHeader.HasRegistrationCode("GST", "AU", "ZZ1234999"));
			Assert("Does not contain 999 code with AU1234GST number for ZZ country", !orgHeader.HasRegistrationCode("VAT", "ZZ", "AU1234GST"));
			Assert("Does not contain 999 code for AU country", !orgHeader.HasRegistrationCode("999", "AU", "AU1234GST"));
			Assert("Does not contain GST code for ZZ country", !orgHeader.HasRegistrationCode("GST", "ZZ", "ZZ1234999"));
			Assert("Does not contain GST code for NZ country", !orgHeader.HasRegistrationCode("VAT", "NZ", "NZ1234GST"));
		}

		public void TestHasEligibleComplianceSubType()
		{
			var indiaTransactionEligible = new FakeEligibilityLiteTransaction()
			{
				CountryCode = Constants.CountryCodes.India,
				ComplianceSubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI,
			};
			Assert(indiaTransactionEligible.HasEligibleComplianceSubType());

			var indiaTransactionIneligible = new FakeEligibilityLiteTransaction()
			{
				CountryCode = Constants.CountryCodes.India,
				ComplianceSubType = "___",
			};
			Assert(!indiaTransactionIneligible.HasEligibleComplianceSubType());

			var brazilTransactionEligible = new FakeEligibilityLiteTransaction()
			{
				CountryCode = Constants.CountryCodes.Brazil,
				ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS,
			};
			Assert(brazilTransactionEligible.HasEligibleComplianceSubType());

			var brazilTransactionIneligible = new FakeEligibilityLiteTransaction()
			{
				CountryCode = Constants.CountryCodes.Brazil,
				ComplianceSubType = "___",
			};
			Assert(!brazilTransactionIneligible.HasEligibleComplianceSubType());
		}
	}
}
