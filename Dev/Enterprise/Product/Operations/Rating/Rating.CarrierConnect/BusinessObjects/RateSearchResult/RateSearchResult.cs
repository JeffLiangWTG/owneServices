#nullable enable
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Rating.CarrierConnect;

public class RateSearchResult(BusinessObjectFactory factory, DataRow row) : AutoRateSearchResult(factory, row)
{
	public void StoreDto(object dto) => RR_JsonContent = ZBlob.FromUTF8(JsonConvert.SerializeObject(dto));

	public bool TryGetDto<T>(out T dto)
	{
		dto = JsonConvert.DeserializeObject<T>(RR_JsonContent.ToUTF8())!;
		return dto is not null;
	}

	public static void DeleteExpiredRecords(BusinessObjectFactory? factoryToUse = null)
	{
		const int recordExpiryMinutes = 60;
		var factory = factoryToUse ?? new BusinessObjectFactory();

		var query = new ZQuery(RateSearchResultSchema.RR_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcNow.AddMinutes(-recordExpiryMinutes));
		var orphans = factory.Load<RateSearchResult>(query);

		foreach (var rateSearchResult in orphans)
		{
			rateSearchResult.Delete();
		}

		factory.Save();
	}
}
