using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RefCusCodeListLoader
	{
		public RefCusCodeListLoader(IRefDataLoader refDataLoader)
		{
			RefDataLoader = refDataLoader;
		}

		internal readonly IRefDataLoader RefDataLoader;

		protected IRefDataLoader GetRefDataLoader => RefDataLoader;

		public Task<IEnumerable<RefCusCodeList>> GetEuUomCodeList()
		{
			return Task.Factory.StartNew(() => RefDataLoader.LoadData<RefCusCodeList>(CreateFilterForQueryForEuUom()).Result);
		}

		protected static string CreateFilterForQueryForEuUom()
		{
			return "RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq 'EUN' and ZZD_ZZK_NKCodeType eq 'CUSUQ'";
		}


		public Task<IEnumerable<RefCusCodeList>> GetRateCodeUsageCodeList(string rateCodes)
		{
			return Task.Factory.StartNew(() => RefDataLoader.LoadData<RefCusCodeList>(CreateFilterQueryForRateCodeUsage(rateCodes)).Result);
		}

		protected static  string CreateFilterQueryForRateCodeUsage(string rateCodes)
		{
			while (rateCodes.Contains(" "))
			{
				rateCodes = rateCodes.Replace(" ", "");
			}
			rateCodes = rateCodes.Replace(",", "', '");

			return $"RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq 'FR' and ZZD_ZZK_NKCodeType in ('{rateCodes}')";
		}
	}
}
