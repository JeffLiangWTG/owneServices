using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	sealed class ComplianceDocumentNumberAllocation_ReceivablesRegistryValidatorsTest : TestCase
	{
		#region CheckCannotChangeDefaultValue()

		public void TestCheckCannotChangeDefaultValue_WhenProposedIsSameAsDefault()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue("ABC", "ABC", "00");
			AssertNullOrEmpty(actual);
		}

		public void TestCheckCannotChangeDefaultValue_WhenProposedIsDifferentToDefault()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue("ABC", "DEF", "00");
			AssertEquals("Cannot change default value for country/region '00'.", actual);
		}

		public void TestCheckCannotChangeDefaultValue_WhenProposedIsDifferentToDefault_AndDifferentCountryCode()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue("ABC", "DEF", "11");
			AssertEquals("Cannot change default value for country/region '11'.", actual);
		}

		public void TestCheckCannotChangeDefaultValue_WhenProposedIsDifferentToDefault_AndAlternateErrorMessage()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue("ABC", "DEF", "11", alternateErrorMessage: "Some other error message, don't forget to translate me!");
			AssertEquals("Some other error message, don't forget to translate me!", actual);
		}

		#endregion

		#region CheckCannotBeGovernmentNumberAllocate()

		public void TestCheckCannotBeGovernmentNumberAllocate_WhenProposedIsNotGovernment()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate("ABC", "00");
			AssertNullOrEmpty(actual);
		}

		public void TestCheckCannotBeGovernmentNumberAllocate_WhenProposedIsGovernment()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate("GVT", "00");
			AssertEquals("'GVT' is not valid for country/region '00'.", actual);
		}

		public void TestCheckCannotBeGovernmentNumberAllocate_WhenProposedIsGovernment_AndDifferentCountryCode()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate("GVT", "11");
			AssertEquals("'GVT' is not valid for country/region '11'.", actual);
		}

		public void TestCheckCannotBeGovernmentNumberAllocate_WhenProposedIsGovernment_AndAlternateErrorMessage()
		{
			var actual = ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate("GVT", "11", alternateErrorMessage: "Some other error message, don't forget to translate me!");
			AssertEquals("Some other error message, don't forget to translate me!", actual);
		}
		#endregion
	}
}
