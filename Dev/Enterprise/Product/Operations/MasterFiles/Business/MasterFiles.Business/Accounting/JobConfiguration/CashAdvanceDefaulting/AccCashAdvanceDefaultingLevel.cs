using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public enum AccCashAdvanceDefaultingLevel
	{
		Null = 0,
		Company = 1,
		Branch = 2,
		Debtor = 3,
		Creditor = 4
	}

	static class AccCashAdvanceDefaultingLevelExtensions
	{
		internal static string ToTablePrefix(this AccCashAdvanceDefaultingLevel level)
		{
			switch (level)
			{
				case AccCashAdvanceDefaultingLevel.Debtor:
				case AccCashAdvanceDefaultingLevel.Creditor:
					return OrgHeaderSchema.Constants.Prefix;

				case AccCashAdvanceDefaultingLevel.Branch:
					return GlbBranchSchema.Constants.Prefix;

				case AccCashAdvanceDefaultingLevel.Company:
				default:
					return string.Empty;
			}
		}

		internal static ZString GetLevelName(this AccCashAdvanceDefaultingLevel level)
		{
			switch (level)
			{
				case AccCashAdvanceDefaultingLevel.Company:
					return Res.GetString("20590a7f-a0b3-4512-aa84-b93d3ae0ea55", "Company");
				case AccCashAdvanceDefaultingLevel.Branch:
					return Res.GetString("b56e5c8c-743b-449b-813c-121acdbbcf8b", "Branch");
				case AccCashAdvanceDefaultingLevel.Debtor:
					return Res.GetString("ab7405c6-fed3-4f3b-9cf3-66eb303ddd65", "Debtor");
				case AccCashAdvanceDefaultingLevel.Creditor:
					return Res.GetString("71196435-51be-48ca-b313-3868f7a5143e", "Creditor");
				default:
					return string.Empty;
			}
		}
	}
}
