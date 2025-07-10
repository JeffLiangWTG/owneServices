using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.Serialization;

namespace Enterprise.Rating.Business
{
	public static class RatingTypeSerializer
	{
		public static string Serialize(object value)
		{
			var key = GetSerializerKey(value.GetType());
			return (key != null ? serializersDictionary[key] : DefaultRatingXmlSerializer.Serialize)(value);
		}

		public static Type GetSerializerKey(Type objectToSerializeType)
		{
			return serializersDictionary.Keys.FirstOrDefault(x => x.IsAssignableFrom(objectToSerializeType));
		}

		static readonly ConcurrentDictionary<Type, Func<object, string>> serializersDictionary = new Dictionary<Type, Func<object, string>>
		{
			{ typeof(SpotRateInfo), SpotRateInfoRatingXmlSerializer.Serialize },
			{ typeof(IJobDatesProvider), JobDateProviderRatingXmlSerializer.Serialize },
			{ typeof(OrgWithSource), OrgWithSourceSerializer.Serialize },
			{ typeof(MoneyType), MoneyTypeSerializer.Serialize },
			{ typeof(PaymentTermInfos), PaymentTermInfosSerializer.Serialize },
			{ typeof(ServiceLevelRatingInformation), ServiceLevelRatingInformationSerializer.Serialize },
			{ typeof(PackageInformation), PackageInformationSerializer.Serialize },
			{ typeof(RateLineConditionsSupporter), RateLineConditionsSupporterSerializer.Serialize }
		}
			.ToConcurrent();
	}
}

