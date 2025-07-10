//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCommodityCodeLookups
//
//    This class should be used for overriding collections in AutoRefCommodityCodeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.Application;
using CargoWise.Common.Cache;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Client;
using DotNetCache = System.Runtime.Caching;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCodeLookups : AutoRefCommodityCodeLookups
	{
		public RefCommodityCodeLookups(AutoRefCommodityCode parent) : base(parent)
		{
		}

		#region RH_UniversalCommodityGroup_List

		public UntranslatableCodeDescriptionPairList RH_UniversalCommodityGroup_List
		{
			get
			{
				var universalCommodityGroups = GetUniversalCommodityGroupMapUnfilteredList();

				foreach (var excludedUniversalCommodityGroup in excludedUniversalCommodityGroups)
				{
					universalCommodityGroups.RemoveCode(excludedUniversalCommodityGroup);
				}

				return universalCommodityGroups;
			}
		}

		public static UntranslatableCodeDescriptionPairList GetUniversalCommodityGroupMapUnfilteredList()
		{
			var policy = new DotNetCache.CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2) };
			return ObjectCacheExtensions.GetOrAdd(DotNetCache.MemoryCache.Default, "UniversalCommodityGroupMap_List", GetUniversalCommodityGroups, policy);
		}

		static UntranslatableCodeDescriptionPairList GetUniversalCommodityGroups()
		{
			var universalCodes = new UntranslatableCodeDescriptionPairList((NoResString)"Universal codes values cannot be translated");

			var clientFactory = ObjectFactory.Get<IWiseRatesClientFactory>();

			var requestID = WiseRatesClient.GenerateTraceID();

			var (client, failureMessage) = clientFactory.TryCreate(requestID);

			if (string.IsNullOrWhiteSpace(failureMessage))
			{
				var codes = client.GetCommodityGroups(requestID);

				if (codes != null)
				{
					foreach (var code in codes)
					{
						universalCodes.AddPair(code.Code, code.Description);
					}
				}
			}

			return universalCodes;
		}

		// commodity tick-boxes should be excluded from lookups
		readonly string[] excludedUniversalCommodityGroups = new[] { RefCommodityCode.HAZD, RefCommodityCode.PERS, RefCommodityCode.TIMB, RefCommodityCode.FLAM, RefCommodityCode.CNVT };

		#endregion

		#region Universal Commodity Codes

		public Rating.UniversalCommodityCodeBizoCollection UniversalCommodityCodeBizoList
		{
			get => new Rating.UniversalCommodityCodeBizoCollection(Factory);
		}

		#endregion

		#region IATA Commodity Codes

		public IATACommodityCodeCollection IATACommodityItemList
		{
			get => new IATACommodityCodeCollection(Factory);
		}

		#endregion
	}
}