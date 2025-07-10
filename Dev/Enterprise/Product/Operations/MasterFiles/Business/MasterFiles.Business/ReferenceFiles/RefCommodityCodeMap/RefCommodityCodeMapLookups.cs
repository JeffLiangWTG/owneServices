using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCodeMapLookups : AutoRefCommodityCodeMapLookups
	{
		public RefCommodityCodeMapLookups(AutoRefCommodityCodeMap parent) : base(parent)
		{
		}

		new RefCommodityCodeMap Parent => (RefCommodityCodeMap)base.Parent;

		public CodeDescriptionPairList LocalProviders
			=> ProvidersByCountryLookup.TryGetValue(Parent.LC_RN_NKCountry, out var providers)
				? providers
				: new CodeDescriptionPairList();

		public CodeDescriptionPairList AllProviders
		{
			get
			{
				var result = new CodeDescriptionPairList();
				ProvidersByCountryLookup.Values.Select(x => x).ForEach(y => result.AddRange(y));
				return result;
			}
		}

		const string NoCountryCode = "";

		readonly ReadOnlyDictionary<ZString, CodeDescriptionPairList> ProvidersByCountryLookup =
			new ReadOnlyDictionary<ZString, CodeDescriptionPairList>(
				new Dictionary<ZString, CodeDescriptionPairList>
				{
					{ NoCountryCode, new GlobalCommodityCodeProviderList() },
					{ CountryCodes.Germany, new DELocalCommodityCodeProviderList() }
				});
	}
}
