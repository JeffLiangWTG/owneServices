using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public static class InBondNumberGenerator
	{
		public static bool TryGetNextInBondNumber(GlbBranch entryBranch, out ZString inBondNumber)
		{
			inBondNumber = "";
			var inBondSetting = GetInBondNumberSetting(entryBranch);

			if (inBondSetting != null && inBondSetting.IsNumberFountainValid)
			{
				inBondSetting.EnsureCurrentNextNumberIsCorrect();

				if (inBondSetting.AvailableNumbers != ZInt.Zero)
				{
					do
					{
						var nextNum = inBondSetting.GenerateCurrentNextNumberWithCheckDigit();
						if (inBondSetting.GetCusEntryNumberMatching(nextNum, true) == null)
						{
							inBondNumber = nextNum;
							return true;
						}
					}
					while (inBondSetting.CurrentNextNumber <= inBondSetting.LastNumber);
				}
			}

			return false;
		}

		public static bool IsInBondNumberRangeByCompany(ZGuid companyPK)
		{
			var companyRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			return companyRange != null && companyRange.StartNumber > 0;
		}

		public static InBondNumberSetting GetInBondNumberSetting(GlbBranch branch)
		{
			return IsInBondNumberRangeByCompany(branch.GB_GC) ? InBondNumberSetting.New(branch.Company) : InBondNumberSetting.New(branch);
		}

		public static ZDecimal GetNextAvailableInBondNumberAndCheckReusable(InBondNumberSetting inBondNumberSetting, out ZString information)
		{
			information = ZString.Empty;
			var result = ZDecimal.Zero;
			if (inBondNumberSetting.IsNumberFountainValid)
			{
				inBondNumberSetting.EnsureCurrentNextNumberIsCorrect();
				if (inBondNumberSetting.AvailableNumbers != ZInt.Zero)
				{
					var factory = inBondNumberSetting.Factory;
					var currentNextNumber = inBondNumberSetting.CurrentNextNumber;
					var lastNumber = inBondNumberSetting.LastNumber;
					var utcNow = ZDateTime.UtcNow;
					while (result == ZDecimal.Zero && currentNextNumber <= lastNumber)
					{
						var currentNextNumberWithCheckDigit = inBondNumberSetting.GetNumberWithCheckDigit(currentNextNumber);
						var latestExisting = inBondNumberSetting.GetCusEntryNumberMatching(currentNextNumberWithCheckDigit);
						if (latestExisting == null)
						{
							result = currentNextNumber;
						}
						else
						{
							if (latestExisting.CE_SystemCreateTimeUtc.IsEmpty
								|| latestExisting.CE_SystemCreateTimeUtc.AddYears(InBondNumberSetting.ExpirationYear) < utcNow)
							{
								var tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(latestExisting.CE_ParentTable);
								var parentBO = factory.Load(tablePrefix, latestExisting.CE_ParentID);
								information = Res.GetString("E58F49A3-21D5-4F20-9523-754FFFA8143B", "In-bond number '{0}' was previously under on {1} but was re-issued by Customs.\r\nOK to proceed with re-using this number?", currentNextNumberWithCheckDigit, GetJobReference(parentBO));
								result = currentNextNumber;
							}
							else
							{
								currentNextNumber++;
							}
						}
					}

					if (result == ZDecimal.Zero)
					{
						information = Res.GetString("78C7A2E9-2803-46A3-8DC9-8B1982BD6E30", "All numbers in In-Bond number range have been allocated in past 3 year.");
					}
				}
				else
				{
					information = Res.GetString("FA4A9970-8274-4CE1-8812-8231A438FD09", "No available numbers in In-Bond number range.");
				}
			}
			else
			{
				information = Res.GetString("9CB5D020-1BFB-49B9-BCEA-9858D8C75ED5", "Invalid In-Bond number range.");
			}
			return result;
		}

		static ZString GetJobReference(BusinessObject bo)
		{
			var result = unknownObject;
			if (bo is IJobNumber jobNumber)
			{
				result = jobNumber.JobNumber;
			}
			return result;
		}

		const string unknownObject = "unknown object";
	}
}
