using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceSequencePresentationProviderTest : TestCaseWithFactory
	{
		public void TestCanReactivate_TryReactivateInactivedComplianceBook()
		{
			var expectedError = @"Compliance book TST cannot be activated due to registry setting.
This is controlled by this registry: Accounting -> Government Compliance Invoice Document -> Allow re-activation of inactive compliance books.";

			var povider = new ComplianceSequencePresentationProvider();
			var complianceSequance = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequance.XD_Code = "TST";
			AssertEquals("Precondition", ZDateTime.Empty, complianceSequance.XD_PermanentDisableTimeUtc);

			using (AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				complianceSequance.XD_IsActive = true;
				AssertNullOrEmpty(povider.CanReactivate(complianceSequance));

				complianceSequance.XD_IsActive = false;
				AssertNullOrEmpty(povider.CanReactivate(complianceSequance));
			}

			using (AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				complianceSequance.XD_IsActive = true;
				AssertEquals(expectedError, povider.CanReactivate(complianceSequance));

				complianceSequance.XD_IsActive = false;
				AssertEquals(expectedError, povider.CanReactivate(complianceSequance));
			}
		}

		public void TestCanReactivate_WithPermanentDisableTime()
		{
			var expectedError1 = "The database was restored recently. According to compliance policies, this Compliance Sequence Book was disabled. And it cannot be reactivated.";

			var expectedError2 = @"Compliance book TST cannot be activated due to registry setting.
This is controlled by this registry: Accounting -> Government Compliance Invoice Document -> Allow re-activation of inactive compliance books.";

			var povider = new ComplianceSequencePresentationProvider();
			var complianceSequance = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequance.XD_Code = "TST";

			complianceSequance.XD_PermanentDisableTimeUtc = ZDateTime.Now;
			AssertEquals(expectedError1, povider.CanReactivate(complianceSequance));

			complianceSequance.XD_PermanentDisableTimeUtc = ZDateTime.Empty;
			AssertNullOrEmpty(povider.CanReactivate(complianceSequance));

			using (AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				complianceSequance.XD_PermanentDisableTimeUtc = ZDateTime.Now;
				AssertEquals(expectedError1, povider.CanReactivate(complianceSequance));

				complianceSequance.XD_PermanentDisableTimeUtc = ZDateTime.Empty;
				AssertEquals(expectedError2, povider.CanReactivate(complianceSequance));
			}
		}
	}
}
