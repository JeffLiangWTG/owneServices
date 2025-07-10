using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InBondNumberAvailabilityCheckerTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestCheck()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
			string branchErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			AssertEquals(branchErrorMessage, InBondNumberAvailabilityChecker.Check(otherBranch));

			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(Env.CurrentBranch.PK);
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
			{
			}

			AssertEquals(InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForBranchForJob(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), InBondNumberAvailabilityChecker.Check(GlbBranch.CurrentBranch));
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			long numberToCheck = (long)(numberRange.LastNumber - 1);
			while (numberToCheck != branchNumberFountain.PeekPreliminary(Factory))
			{
				branchNumberFountain.GetNext(Factory);
			}

			AssertEquals("", InBondNumberAvailabilityChecker.Check(GlbBranch.CurrentBranch));
		}

		[UseSnapshotProtection]
		public void TestValidateInBondNumber()
		{
			var header = Factory.New<DummyBusinessObject>();
			using (header.SuspendValidationTesting())
			{
				var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				var otherBranch = currentCompany.Branches.AddNew();
				otherBranch.GB_Code = "~Z~";
				var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty);
				numberRange.StartNumber = 0;
				AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
				var branchErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
				header.Z0_Description = ZString.Empty;
				InBondNumberAvailabilityChecker.Check(header.Z0_DescriptionInfo, otherBranch);
				AssertHasMessageError(header.Z0_DescriptionInfo, branchErrorMessage);
				header.Z0_Description = "153598466";
				InBondNumberAvailabilityChecker.Check(header.Z0_DescriptionInfo, otherBranch);
				AssertNoMessageError(header.Z0_DescriptionInfo, branchErrorMessage);

				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(Env.CurrentBranch.PK);
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
				{
				}

				var warningMessage = InBondNumberSetting.BranchInBondNumberHasReachedLimitWarning(2, GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName);
				var messageError = InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForBranchForJob(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName);
				header.Z0_Description = ZString.Empty;
				InBondNumberAvailabilityChecker.Check(header.Z0_DescriptionInfo, GlbBranch.CurrentBranch);
				AssertNoMessageError(header.Z0_DescriptionInfo, branchErrorMessage);
				AssertHasMessageError(header.Z0_DescriptionInfo, messageError);
				header.Z0_Description = "153598466";
				InBondNumberAvailabilityChecker.Check(header.Z0_DescriptionInfo, GlbBranch.CurrentBranch);
				AssertNoMessageError(header.Z0_DescriptionInfo, messageError);
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				var numberToCheck = numberRange.LastNumber - 1;
				while (numberToCheck != branchNumberFountain.PeekPreliminary(Factory))
				{
					branchNumberFountain.GetNext(Factory);
				}

				header.Z0_Description = ZString.Empty;
				InBondNumberAvailabilityChecker.Check(header.Z0_DescriptionInfo, GlbBranch.CurrentBranch);
				AssertNoMessageError(header.Z0_DescriptionInfo, branchErrorMessage);
				AssertNoMessageError(header.Z0_DescriptionInfo, messageError);
				AssertHasWarning(header.Z0_DescriptionInfo, warningMessage);
				InBondNumberAvailabilityChecker.Check(header.Z0_DescriptionInfo, GlbBranch.CurrentBranch, true);
				AssertHasMessageError(header.Z0_DescriptionInfo, InBondNumberAvailabilityChecker.InBondNumberIsRequired);
			}
		}

		[UseSnapshotProtection]
		public void TestCheckWhenCompanyRange()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
			string rangeErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			AssertEquals(rangeErrorMessage, InBondNumberAvailabilityChecker.Check(otherBranch));

			DeclarationTestHelper.SetupCompanySpecificInBondNumberRange();
			numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var companyNumberFountain = Env.NumberFountains.USInBondNumberFountain(currentCompany.PK.ToGuid());
			companyNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			while (companyNumberFountain.GetNext(Factory) != numberRange.LastNumber)
			{
			}

			AssertEquals(InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForCompanyForJob(GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name), InBondNumberAvailabilityChecker.Check(otherBranch));
			companyNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			long numberToCheck = (long)(numberRange.LastNumber - 1);
			while (numberToCheck != companyNumberFountain.PeekPreliminary(Factory))
			{
				companyNumberFountain.GetNext(Factory);
			}

			AssertEquals("", InBondNumberAvailabilityChecker.Check(otherBranch));
		}

		[UseSnapshotProtection]
		[TestDate(2012, 4, 1)]
		public void TestReportLimitHasReachedIfNeededForBranch()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "bob@where.com";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "!2s";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "3$3";
			staff2.GS_LoginName = "32!";
			staff2.GS_EmailAddress = "joe@who.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "4$4";
			staff3.GS_LoginName = "95!";
			staff3.GS_EmailAddress = "jay@how.com";
			Factory.Save();
			USCustomsDataRegistry.Instance.NumberRangeLimitWarningGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK));

			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			var dateTime = ZDateTime.Now.AddHours(-1).ToDateTime();
			DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
			AssertEquals("Body", string.Format("The In-Bond Number Range set up for branch ('{0} - {1}') is running out.\r\nThere are only 102 numbers remaining.\r\nYou will need to prepare to allocate a new number range.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), email.Body);
			AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals("Email contains " + staff.GS_EmailAddress, true, email.Recipients.Contains(staff.GS_EmailAddress));
			AssertEquals("Email contains " + staff2.GS_EmailAddress, true, email.Recipients.Contains(staff2.GS_EmailAddress));
			while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
			{
			}

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals("Should not run as it's not the right time", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			staff.GS_EmailAddress = ZString.Empty;
			GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
			Factory.Save();
			DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
			AssertEquals("Body", string.Format("The In-Bond Number Range set up for branch ('{0} - {1}') has run out.\r\nPlease allocate a new number range.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), email.Body);
			AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("Email", staff2.GS_EmailAddress, email.Recipients[0].Email);
			staff2.GS_EmailAddress = ZString.Empty;
			Factory.Save();
			DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		[UseSnapshotProtection]
		[TestDate(2012, 4, 1)]
		public void TestReportLimitHasReachedIfNeededForCompany()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "bob@where.com";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "!2s";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "3$3";
			staff2.GS_LoginName = "32!";
			staff2.GS_EmailAddress = "joe@who.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "4$4";
			staff3.GS_LoginName = "95!";
			staff3.GS_EmailAddress = "jay@how.com";
			Factory.Save();
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			USCustomsDataRegistry.Instance.NumberRangeLimitWarningGroup.SetValue(companyPK, Guid.Empty, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK));

			DeclarationTestHelper.SetupCompanySpecificInBondNumberRange();
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			var companyNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbCompany.CurrentCompany.PK.ToGuid());
			companyNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			var dateTime = ZDateTime.Now.AddHours(-1).ToDateTime();
			DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(companyPK, Guid.Empty, Guid.Empty, dateTime);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
			AssertEquals("Body", string.Format("The In-Bond Number Range set up for company ('{0} - {1}') is running out.\r\nThere are only 102 numbers remaining.\r\nYou will need to prepare to allocate a new number range.", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name), email.Body);
			AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals("Email contains " + staff.GS_EmailAddress, true, email.Recipients.Contains(staff.GS_EmailAddress));
			AssertEquals("Email contains " + staff2.GS_EmailAddress, true, email.Recipients.Contains(staff2.GS_EmailAddress));
			while (companyNumberFountain.GetNext(Factory) != numberRange.LastNumber)
			{
			}

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals("Should not run as it's not the right time", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			staff.GS_EmailAddress = ZString.Empty;
			GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
			Factory.Save();
			DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(companyPK, Guid.Empty, Guid.Empty, dateTime);
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
			AssertEquals("Body", string.Format("The In-Bond Number Range set up for company ('{0} - {1}') has run out.\r\nPlease contact Customs for a new range.", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name), email.Body);
			AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("Email", staff2.GS_EmailAddress, email.Recipients[0].Email);
			staff2.GS_EmailAddress = ZString.Empty;
			Factory.Save();
			DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(companyPK, Guid.Empty, Guid.Empty, dateTime);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(GlbBranch.CurrentBranch);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}
	}
}
