using System;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.CountryCompliance.Countries.Colombia
{
	sealed class  AccComplianceSequenceColombiaValidationTest : AccComplianceSequenceValidationTest
	{
		public void TestValidateXD_PrintingAuthorizationNumber_DependOnEInvoicingFunctionality()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Colombia))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				Factory.Save();

				sequence.XD_PrintingAuthorizationNumber = "12";
				AssertNoWarnings(sequence.XD_PrintingAuthorizationNumberInfo);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				sequence.XD_PrintingAuthorizationNumber = "14";
				AssertHasWarnings(sequence.XD_PrintingAuthorizationNumberInfo);
			}
		}

		public void TestValidateXD_PrintingAuthorizationNumber_RegexIsMatch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Colombia))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				Factory.Save();

				sequence.XD_PrintingAuthorizationNumber = "1239 - 0000";
				AssertNoWarnings(sequence.XD_PrintingAuthorizationNumberInfo);

				sequence.XD_PrintingAuthorizationNumber = "1239-0000";
				AssertNoWarnings(sequence.XD_PrintingAuthorizationNumberInfo);
			}
		}

		public void TestValidateXD_PrintingAuthorizationNumber_RegexIsNotMatch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Colombia))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var expectedMessage = "Format is invalid. The 'Resolution Number' and the 'Technical Key' assigned by DIAN must be separated by a hyphen.";
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				Factory.Save();

				sequence.XD_PrintingAuthorizationNumber = "";
				AssertHasWarning("PrintingAuthorizationNumber must have warning message", sequence.XD_PrintingAuthorizationNumberInfo, expectedMessage);

				sequence.XD_PrintingAuthorizationNumber = "-";
				AssertHasWarning("PrintingAuthorizationNumber must have warning message", sequence.XD_PrintingAuthorizationNumberInfo, expectedMessage);

				sequence.XD_PrintingAuthorizationNumber = "A-B-C";
				AssertHasWarning("PrintingAuthorizationNumber must have warning message", sequence.XD_PrintingAuthorizationNumberInfo, expectedMessage);

				sequence.XD_PrintingAuthorizationNumber = "1239";
				AssertHasWarning("PrintingAuthorizationNumber must have warning message", sequence.XD_PrintingAuthorizationNumberInfo, expectedMessage);

				sequence.XD_PrintingAuthorizationNumber = "1239 -";
				AssertHasWarning("PrintingAuthorizationNumber must have warning message", sequence.XD_PrintingAuthorizationNumberInfo, expectedMessage);

				sequence.XD_PrintingAuthorizationNumber = "- 1239";
				AssertHasWarning("PrintingAuthorizationNumber must have warning message", sequence.XD_PrintingAuthorizationNumberInfo, expectedMessage);
			}
		}
	}
}
