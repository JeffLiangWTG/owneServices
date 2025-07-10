using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business
{
	public static class InBondNumberAvailabilityChecker
	{
		public static string NotEnoughAvailableInBondNumbersForBranchForJob(string branchCode, string branchName)
		{
			return ResString.GetMultilingualString("441EE53F-444A-4C89-AB91-E60656F0A53D", "The Inbond Number Range from which this Job will be allocated a number has run out.\r\nPlease allocate a new number range to branch: '{0} - {1}'.", branchCode, branchName);
		}

		public static string NotEnoughAvailableInBondNumbersForCompanyForJob(string companyCode, string companyName)
		{
			return ResString.GetMultilingualString("ECCFBA46-BA12-462F-89E8-86CAF24C4EBC", "The In-Bond Number Range set up for company ('{0} - {1}'), from which this Job will be allocated a number, has run out.\r\nPlease contact Customs for a new range.", companyCode, companyName);
		}

		public static string InBondNumberRangeNotSetup(string location)
		{
			return ResString.GetMultilingualString("6BBAFB8A-B69B-48c9-BB45-8C7912AB7335", "The Inbond Number Range from which this Job will be allocated a number has not been set up correctly.\r\nThe Inbond Number Range can be set up in the System Registry.\r\nMaintain -> System -> Registry -> {0}.", location);
		}

		public static string InBondNumberIsRequired
		{
			get { return ResString.GetMultilingualString("FDF17339-3EAF-4252-8BB5-3DFD18C4C8D7", "In-Bond Number is required. Please click the Allocate button and enter an In-Bond number its departure was declared under."); }
		}

		public static void Check(ZPropertyInfo inBondNumberInfo, GlbBranch branch, bool isPostDepartureMessageOnly = false)
		{
			if (branch != null && !isPostDepartureMessageOnly)
			{
				var isInBondNumberRangeByCompany = InBondNumberGenerator.IsInBondNumberRangeByCompany(branch.GB_GC);
				InBondNumberSetting setting = isInBondNumberRangeByCompany ? InBondNumberSetting.New(branch.Company) : InBondNumberSetting.New(branch);
				if (inBondNumberInfo.Value.IsEmpty)
				{
					var notification = Check(setting, isInBondNumberRangeByCompany);
					if (!string.IsNullOrEmpty(notification))
					{
						inBondNumberInfo.AddMessageError(notification);
					}
				}
				if (setting != null && setting.HasReachedLimit)
				{
					inBondNumberInfo.AddWarning(GetInBondNumberHasReachedLimitWarning(branch, isInBondNumberRangeByCompany, (long)setting.AvailableNumbers));
				}
			}
			else if (isPostDepartureMessageOnly && inBondNumberInfo.Value.IsEmpty)
			{
				inBondNumberInfo.AddMessageError(InBondNumberIsRequired);
			}
		}

		static string GetInBondNumberHasReachedLimitWarning(GlbBranch branch, bool isInBondNumberRangeByCompany, long availableNumbers)
		{
			if (isInBondNumberRangeByCompany)
			{
				var company = branch.Company;
				return InBondNumberSetting.CompanyInBondNumberHasReachedLimitWarning(availableNumbers, company.GC_Code, company.GC_Name);
			}
			else
			{
				return InBondNumberSetting.BranchInBondNumberHasReachedLimitWarning(availableNumbers, branch.GB_Code, branch.GB_BranchName);
			}
		}

		public static string Check(GlbBranch branch)
		{
			var isInBondNumberRangeByCompany = InBondNumberGenerator.IsInBondNumberRangeByCompany(branch.GB_GC);
			return Check(isInBondNumberRangeByCompany ? InBondNumberSetting.New(branch.Company) : InBondNumberSetting.New(branch), isInBondNumberRangeByCompany);
		}

		static string Check(InBondNumberSetting inbondSetting, bool isInBondNumberRangeByCompany)
		{
			string result = "";
			if (inbondSetting != null)
			{
				if (!inbondSetting.IsNumberFountainValid)
				{
					result = InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
				}
				else if (inbondSetting.AvailableNumbers == 0)
				{
					if (isInBondNumberRangeByCompany)
					{
						var company = inbondSetting.Company;
						result = NotEnoughAvailableInBondNumbersForCompanyForJob(company.GC_Code, company.GC_Name);
					}
					else
					{
						var branch = inbondSetting.Branch;
						result = NotEnoughAvailableInBondNumbersForBranchForJob(branch.GB_Code, branch.GB_BranchName);
					}
				}
			}
			return result;
		}

		public static string NotEnoughAvailableInBondNumbersForCompany(string companyCode, string companyName)
		{
			return ResString.GetMultilingualString("AFA9BECC-8B9D-4192-B9E4-84DFBDBE7AE8", "The In-Bond Number Range set up for company ('{0} - {1}') has run out.\r\nPlease contact Customs for a new range.", companyCode, companyName);
		}

		public static string NotEnoughAvailableInBondNumbersForBranch(string branchCode, string branchName)
		{
			return ResString.GetMultilingualString("C3E81AA1-EB7C-4885-963F-1778C46C9B2C", "The In-Bond Number Range set up for branch ('{0} - {1}') has run out.\r\nPlease allocate a new number range.", branchCode, branchName);
		}

		public static string InBondNumbersForCompanyIsRunning(long availableNumbers, string companyCode, string companyName)
		{
			return ResString.GetMultilingualString("1B64E9CC-785A-43EF-8297-6DA79C037B7F", "The In-Bond Number Range set up for company ('{1} - {2}') is running out.\r\nThere are only {0} numbers remaining.\r\nYou will need to prepare to allocate a new number range.", availableNumbers, companyCode, companyName);
		}

		public static string InBondNumbersForBranchIsRunning(long availableNumbers, string branchCode, string branchName)
		{
			return ResString.GetMultilingualString("45B0802C-A286-455B-831A-3ED24A884E79", "The In-Bond Number Range set up for branch ('{1} - {2}') is running out.\r\nThere are only {0} numbers remaining.\r\nYou will need to prepare to allocate a new number range.", availableNumbers, branchCode, branchName);
		}

		public static void ReportLimitHasReachedIfNeeded(GlbBranch branch)
		{
			if (branch != null)
			{
				var isInBondNumberRangeByCompany = InBondNumberGenerator.IsInBondNumberRangeByCompany(branch.GB_GC);
				var companyPK = Guid.Empty;
				var branchPK = Guid.Empty;
				if (isInBondNumberRangeByCompany)
				{
					companyPK = branch.GB_GC.ToGuid();
				}
				else
				{
					branchPK = branch.PK.ToGuid();
				}
				var nextReportRunDate = DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(companyPK, branchPK, Guid.Empty);
				if (nextReportRunDate == DateTime.MinValue || nextReportRunDate <= ZDateTime.Now)
				{
					InBondNumberSetting setting = isInBondNumberRangeByCompany ? InBondNumberSetting.New(branch.Company) : InBondNumberSetting.New(branch);
					if (setting != null && setting.HasReachedLimit)
					{
						var bodyText = ZString.Empty;
						if (setting.AvailableNumbers == 0)
						{
							if (isInBondNumberRangeByCompany)
							{
								var company = branch.Company;
								bodyText = NotEnoughAvailableInBondNumbersForCompany(company.GC_Code, company.GC_Name);
							}
							else
							{
								bodyText = NotEnoughAvailableInBondNumbersForBranch(branch.GB_Code, branch.GB_BranchName);
							}
						}
						else
						{
							if (isInBondNumberRangeByCompany)
							{
								var company = branch.Company;
								bodyText = InBondNumbersForCompanyIsRunning((long)setting.AvailableNumbers, company.GC_Code, company.GC_Name);
							}
							else
							{
								bodyText = InBondNumbersForBranchIsRunning((long)setting.AvailableNumbers, branch.GB_Code, branch.GB_BranchName);
							}
						}
						var email = new EmailDef() { Subject = "INBOND NUMBER RANGE LIMIT WARNING", Body = bodyText };

						var userToNotify = !Env.CurrentUser.IsBatchProcessor ? branch.Factory.Load<GlbStaff>(Env.CurrentUser.PK) : null;
						var warningGroupRegistry = USCustomsDataRegistry.Instance.NumberRangeLimitWarningGroup.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branchPK, Guid.Empty);
						var emailRecipientCalculator = new EmailRecipientCalculator(warningGroupRegistry.SendMode, warningGroupRegistry.SendGroupPK, userToNotify, ZGuid.Empty);
						emailRecipientCalculator.SendNotifications(branch.Factory, email, USCustomsDataRegistry.Instance.NumberRangeLimitWarningGroup);

						DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(companyPK, branchPK, Guid.Empty, ZDateTime.Now.AddDays(1).ToDateTime());
					}
				}
			}
		}
	}
}
