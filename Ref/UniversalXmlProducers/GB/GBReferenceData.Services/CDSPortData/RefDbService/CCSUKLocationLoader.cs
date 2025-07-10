using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService
{
	public class CCSUKLocationLoader
	{
		public CCSUKLocationLoader(IRefDataLoader refDataLoader)
		{
			RefDataLoader = refDataLoader;
		}

		protected IRefDataLoader RefDataLoader { get; private set; }

		public Task<IEnumerable<CCSUKLocation>> GetLocations()
		{
			return Task.Factory.StartNew(() => RefDataLoader.LoadData<RefCusCodeList>(CreateUrlQuery().OriginalString).Result.Select(x => new CCSUKLocation { Code = x.ZZD_Code }));
		}

		protected static Uri CreateUrlQuery()
		{
			return new Uri($"RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq '{GBDataGrouping}' and ZZD_ZZK_NKCodeType eq '{GBLocationCodeType}'", UriKind.Relative);
		}

		const string GBDataGrouping = "GB";
		const string GBLocationCodeType = "FAC";
	}
}
