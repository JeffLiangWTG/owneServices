using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.Universal
{
	public static class RefCarrierHelper
	{
		public static ICodeDescriptionPairList GetTransportModesList(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "Factory");

			return factory.GetCachedValue<ICodeDescriptionPairList>("UniveralRefTransportModeList", () =>
			{
				return new RefCarrierTransportModeList();
			});
		}

		public static RefCarrierCodeAttribute[] GetAttributes(BusinessObjectFactory factory, ZString country)
		{
			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCarrierHelper_{0}", country), () =>
			{
				var carrierSubQuery = new ZDBOnlySubQuery(typeof(RefCarrierCode), RefCarrierCodeAttributeSchema.ZZG_ZZ4_CarrierCode);
				carrierSubQuery.AddToFilter(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, country);

				var query = new ZDBOnlyQuery(typeof(RefCarrierCodeAttribute));
				query.AddSubQuery(carrierSubQuery, JoinCondition.And);
				query.OrderBy = RefCarrierCodeAttribute.Schema.ZZG_Name;
				return factory.Load<RefCarrierCodeAttribute>(query);
			});
		}

		public static IRefCarrierConfig GetConfig(ZString countryCode)
		{
			var types = ObjectFactory.Get<Hashtable>("RefCarrierConfigurations");
			var objectHandle = (ObjectHandle)types[countryCode.ToString()];
			return (IRefCarrierConfig)objectHandle?.GetObject();
		}
	}
}
