using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	static class AccCFXConfigurationExtensions
	{
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		internal static string ToTablePrefix(this AccCFXConfigurationLevelEnum level)
		{
			if (level == AccCFXConfigurationLevelEnum.Branch)
			{
				return GlbBranchSchema.Constants.Prefix;
			}
			if (level == AccCFXConfigurationLevelEnum.Organisation)
			{
				return OrgHeaderSchema.Constants.Prefix;
			}
			if (level == AccCFXConfigurationLevelEnum.Company)
			{
				return string.Empty;
			}

			throw new InvalidOperationException($"Unknown level value {level}");
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		internal static AccCFXConfigurationLevelEnum ToCFXLevel(this ZString tablePrefix)
		{
			if (tablePrefix == GlbBranchSchema.Constants.Prefix)
			{
				return AccCFXConfigurationLevelEnum.Branch;
			}
			if (tablePrefix == OrgHeaderSchema.Constants.Prefix)
			{
				return AccCFXConfigurationLevelEnum.Organisation;
			}
			if (string.IsNullOrEmpty(tablePrefix))
			{
				return AccCFXConfigurationLevelEnum.Company;
			}

			throw new InvalidOperationException($"Unknown table prefix value {tablePrefix}");
		}
	}
}
