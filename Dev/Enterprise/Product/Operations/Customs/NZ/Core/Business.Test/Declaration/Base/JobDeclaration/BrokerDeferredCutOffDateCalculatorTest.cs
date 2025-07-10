using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class BrokerDeferredCutOffDateCalculatorTest : TestCaseWithFactory
	{
		public void TestGetWarningMessageIfWarningRequired_Default()
		{
			var jobDec = Factory.New<JobDeclaration>();
			var cutOffDateCalc = new BrokerDeferredCutOffDateCalculator(jobDec);

			Assert("Should default to empty because the registry is disabled by default", cutOffDateCalc.GetWarningMessageIfWarningRequired().IsEmpty);
		}

		[TestDate(2021, 02, 01)]
		public void TestGetWarningMessageIfWarningRequired_BeforeTheNextCutoffDate()
		{
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDeclarationIsBeingPaidFromBrokersAccount();

				var declaration = Factory.New<JobDeclaration>();
				var cutOffCalc = new BrokerDeferredCutOffDateCalculator(declaration);

				AssertEquals("Should be empty as it's not close to the next cutoff date in this month - 2021-02-20.", string.Empty, cutOffCalc.GetWarningMessageIfWarningRequired());
			}
		}

		[TestDate(2021, 02, 18)]
		public void TestGetWarningMessageIfWarningRequired_NearTheNextCutoffDate()
		{
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDeclarationIsBeingPaidFromBrokersAccount();

				var declaration = Factory.New<JobDeclaration>();
				var cutOffCalc = new BrokerDeferredCutOffDateCalculator(declaration);

				var expectedOutput = @"This entry is of a high value and the next cutoff date for your Broker Deferred account is close.

Total Amount Payable: 0
Next Cutoff Date: 20-Feb-21

Are you sure you still want to submit this entry?";

				AssertEquals("Should not be empty as it's close to the next cutoff date - 2021-02-20.", expectedOutput, cutOffCalc.GetWarningMessageIfWarningRequired());
			}
		}

		[TestDate(2021, 12, 25)]
		public void TestGetWarningMessageIfWarningRequired_AfterTheNextCutoffDate()
		{
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDeclarationIsBeingPaidFromBrokersAccount();

				var declaration = Factory.New<JobDeclaration>();
				var cutOffCalc = new BrokerDeferredCutOffDateCalculator(declaration);

				AssertEquals("Should be empty as it's not close to the next cutoff date in next month - 2022-01-20.", string.Empty, cutOffCalc.GetWarningMessageIfWarningRequired());
			}
		}

		[TestDate(2021, 02, 21)]
		public void TestBrokerDeferredCutoffDateDaysBeforeWarning()
		{
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDeclarationIsBeingPaidFromBrokersAccount();

				var declaration = Factory.New<JobDeclaration>();
				var cutOffCalc = new BrokerDeferredCutOffDateCalculator(declaration);

				AssertEquals("Should be empty as it's not close to the next cutoff date in next month - 2021-03-20.", string.Empty, cutOffCalc.GetWarningMessageIfWarningRequired());

				using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateDaysBeforeWarning.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 30))
				{
					var expectedOutput = @"This entry is of a high value and the next cutoff date for your Broker Deferred account is close.

Total Amount Payable: 0
Next Cutoff Date: 20-Mar-21

Are you sure you still want to submit this entry?";

					AssertEquals("Should not be empty as it's close to the next cutoff date - 2021-02-20.", expectedOutput, cutOffCalc.GetWarningMessageIfWarningRequired());
				}
			}
		}

		[TestDate(2021, 02, 18)]
		public void TestDeferredCutoffDateMinimumDutyWarning()
		{
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateDaysBeforeWarning.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateMinimumDutyWarning.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 400m))
			{
				var jobDec = Factory.New<JobDeclaration>();
				var cutOffDateCalc = new BrokerDeferredCutOffDateCalculator(jobDec);

				Assert("Should default to empty because the registry is disabled by default", cutOffDateCalc.GetWarningMessageIfWarningRequired().IsEmpty);

				SetupDeclarationIsBeingPaidFromBrokersAccount();

				var entryLine = jobDec.CusEntryHeader.MergedLines.AddNew();
				entryLine.DutyAmount = 300m;

				Assert("Not over the minimum duty amount", cutOffDateCalc.GetWarningMessageIfWarningRequired().IsEmpty);

				var expectedOutput = @"This entry is of a high value and the next cutoff date for your Broker Deferred account is close.

Total Amount Payable: 500
Next Cutoff Date: 20-Feb-21

Are you sure you still want to submit this entry?";

				entryLine.DutyAmount = 500m;

				AssertMultilineASCIIEquals("Over the duty amount", expectedOutput, cutOffDateCalc.GetWarningMessageIfWarningRequired());
			}
		}

		void SetupDeclarationIsBeingPaidFromBrokersAccount()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var usersPin = new CurrentUsersPin(Factory, true);

			var wrapper = staff.GetNZWrapper();
			_ = wrapper.NZBPassword.GP_CurrentPassword;

			wrapper.NZBPassword.GP_UserID = "CUCKOOSQUEAKER";
			usersPin.DecryptedPinCode = "OMGPIN";
		}
	}
}
