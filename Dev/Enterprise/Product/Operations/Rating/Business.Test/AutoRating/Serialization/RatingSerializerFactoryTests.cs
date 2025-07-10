using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingSerializerFactoryTests : TestCaseWithFactory
	{
		public void TestAllRatingInterfacesTypesHaveSerializers()
		{
			foreach (var interfaceType in typeof(AutoRatingProxyBase).GetInterfaces())
			{
				foreach (var property in interfaceType.GetProperties())
				{
					//Condition have to be removed as soon as all serializers are done
					if (property.PropertyType == typeof(SpotRateInfo)
						|| property.PropertyType == typeof(IJobDatesProvider)
						|| property.PropertyType == typeof(MoneyType)
						|| property.PropertyType == typeof(OrgWithSource)
						|| property.PropertyType == typeof(RateLineConditionsSupporter)
						|| property.PropertyType == typeof(ServiceLevelRatingInformation)
						|| property.PropertyType == typeof(PackageInformation)
						|| property.PropertyType == typeof(PaymentTermInfos))
					{
						AssertNotNull(RatingTypeSerializer.GetSerializerKey(property.PropertyType));
					}
				}
			}
		}
	}
}
