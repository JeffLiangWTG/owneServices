using System;

namespace Enterprise.Customs.Business
{
	public interface ILandedCostOnlyConfiguration
	{
		Func<CusEntryLine, string, bool> GetIsLandedCostingOnlyFuncForEntryLine();
		Func<CusEntryHeader, string, bool> GetIsLandedCostingOnlyFuncForEntryHeader();
	}

	public static class LandedCostOnlyConfigurationExtensionClass
	{
		public static bool IsLandedCostingOnly(this CusEntryLine entryLine, string feeType, ILandedCostOnlyConfigurationProvider provider)
		{
			var declaration = entryLine.Declaration;
			var configuration = provider.GetLandedCostOnlyConfiguration(declaration);

			var isLandedCostingOnly = configuration?.GetIsLandedCostingOnlyFuncForEntryLine();
			return isLandedCostingOnly != null && isLandedCostingOnly(entryLine, feeType);
		}

		public static bool IsLandedCostingOnly(this CusEntryHeader entryHeader, string feeType, ILandedCostOnlyConfigurationProvider provider)
		{
			var declaration = entryHeader.Declaration;
			var configuration = provider.GetLandedCostOnlyConfiguration(declaration);

			var isLandedCostingOnly = configuration?.GetIsLandedCostingOnlyFuncForEntryHeader();
			return isLandedCostingOnly != null && isLandedCostingOnly(entryHeader, feeType);
		}
	}
}
