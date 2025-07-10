using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Quota;

public static class PLQuotaExtractor
{
	public static IEnumerable<RefCusQuota> GeneratePLQuotaData(IsztarHistoryResponse isztarData, IDateTimeProvider dateTimeProvider)
	{
		var expirationYear = dateTimeProvider.CurrentLocalDateTime.Year - Constants.ExpiredYears;

		var result = new List<RefCusQuota>();

		var responseHistoryItems = isztarData.IsztarHistoryItem
			.Where(x => x.Item.ToString() == Taric4Constants.GroupNodes.FindQuotaDefinitionByDatesResponseHistory)
			.Select(x => (findQuotaDefinitionByDatesResponseHistory)x.Item);

		var quotaDefinitions = responseHistoryItems.Where(x => x.QuotaDefinition is not null).SelectMany(x => x.QuotaDefinition);

		foreach (var quotaDefinition in quotaDefinitions)
		{
			RefCusQuota quota = quotaDefinition.ToRefCusQuota();
			if (!string.IsNullOrEmpty(quota.ZXQ_OrderNumber)
				&& !string.IsNullOrEmpty(quota.ZXQ_UnitOfMeasure)
				&& (quota.ZXQ_EndDate.Year > expirationYear && quota.ZXQ_EndDate >= Business.Constants.LatestVatChangeDate))
			{
				result.Add(quota);
			}
		}

		return result.OrderBy(x => x.ZXQ_OrderNumber);
	}
}
