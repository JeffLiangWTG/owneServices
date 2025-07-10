using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public sealed class CachedCusCodeAttributeProvider : ICusCodeAttributeProvider
	{
		public CachedCusCodeAttributeProvider(StringBuilder errorBuilder, IHttpClientHelper httpClientHelper, DateTime publicationTime)
		{
			Argument.NotNull(errorBuilder, nameof(errorBuilder));
			Argument.NotNull(httpClientHelper, nameof(httpClientHelper));

			lazyCusCodeItems = new Lazy<HashSet<RefCusCodeList>>(() => LoadCusCodeItems(errorBuilder, httpClientHelper, publicationTime));
		}

		RefCusCodeListAttribute ICusCodeAttributeProvider.GetCusCodeAttribute(string code)
		{
			var cusCodeItem = lazyCusCodeItems.Value.FirstOrDefault(x => x.ZZD_Code == code);

			var attributeValue = cusCodeItem is null
				? Constants.AttributeValues.N
				: Constants.AttributeValues.Y;

			return CreateCL016Attribute(attributeValue);
		}

		static RefCusCodeListAttribute CreateCL016Attribute(string attributeValue) => new RefCusCodeListAttribute()
		{
			ZZE_Value = attributeValue,
			ZZE_ZXE_NKName = Constants.CUSNumbers.CusCodeAttributeName,
		};

		readonly Lazy<HashSet<RefCusCodeList>> lazyCusCodeItems;

		static HashSet<RefCusCodeList> LoadCusCodeItems(StringBuilder errorBuilder, IHttpClientHelper httpClientHelper, DateTime publicationTime)
		{
			var cusCodeTypeDownloader = new NctsCodeListDownloader(errorBuilder, httpClientHelper, new NctsCusCodeType());
			var downloadedItems = cusCodeTypeDownloader.DownloadAndConvertToRefCusCodeList().Result;
			return downloadedItems
				.SelectMany(x => x.ParsedXml)
				.Where(x => x.ZZD_StartDate <= publicationTime)
				.ToHashSet();
		}
	}
}
