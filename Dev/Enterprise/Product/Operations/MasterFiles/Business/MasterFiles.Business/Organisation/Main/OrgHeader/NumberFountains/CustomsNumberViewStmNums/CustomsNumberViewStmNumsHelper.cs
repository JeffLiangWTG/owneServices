using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class CustomsNumberViewStmNumsHelper
	{
		public static CustomsNumberViewStmNums NewStmNums(BusinessObjectFactory factory, CustomsNumberViewStmNumsBusinessProvider provider, ZGuid ownerPk)
		{
			var stmNums = factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = provider;
			stmNums.SN_Owner = ownerPk;
			return stmNums;
		}

		public static CustomsNumberViewStmNums LoadTop1StmNums(BusinessObjectFactory factory, ZQuery query, CustomsNumberViewStmNumsBusinessProvider defaultProvider = null)
		{
			var stmNums = factory.LoadTop1<CustomsNumberViewStmNums>(query);
			SetProviderWhenNecessary(stmNums, defaultProvider);
			return stmNums;
		}

		public static CustomsNumberViewStmNums[] LoadStmNums(BusinessObjectFactory factory, ZQuery query)
		{
			var result = factory.Load<CustomsNumberViewStmNums>(query);
			result.ForEach(x => SetProviderWhenNecessary(x));
			return result;
		}

		public static CustomsNumberStmNumberRange NewStmNumRange(BusinessObjectFactory factory, CustomsNumberViewStmNumsBusinessProvider provider, ZGuid ownerPk)
		{
			var stmNumRange = factory.New<CustomsNumberStmNumberRange>();
			stmNumRange.Provider = provider;
			stmNumRange.SNR_Owner = ownerPk;
			return stmNumRange;
		}

		public static CustomsNumberStmNumberRange[] LoadStmNumRanges(BusinessObjectFactory factory, ZQuery query)
		{
			var result = factory.Load<CustomsNumberStmNumberRange>(query);
			result.ForEach(SetProviderWhenNecessary);
			return result;
		}

		static void SetProviderWhenNecessary(CustomsNumberViewStmNums stmNums, CustomsNumberViewStmNumsBusinessProvider defaultProvider = null)
		{
			if (stmNums.Provider == null)
			{
				var provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(stmNums.Factory, FindProviderKeyFromName(stmNums.SN_Name), stmNums.SN_Owner);
				stmNums.Provider = provider ?? defaultProvider;
			}
		}

		static void SetProviderWhenNecessary(CustomsNumberStmNumberRange stmNumsRange)
		{
			if (stmNumsRange.Provider == null)
			{
				var provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(stmNumsRange.Factory, FindProviderKeyFromName(stmNumsRange.SNR_Name), stmNumsRange.SNR_Owner);
				stmNumsRange.Provider = provider;
			}
		}

		static string FindProviderKeyFromName(ZString name)
		{
			return name.Substring(CustomsNumberViewStmNums.Schema.SN_NamePrefix.Length, CustomsNumberViewStmNums.Schema.ProviderKeyLength);
		}
	}
}
