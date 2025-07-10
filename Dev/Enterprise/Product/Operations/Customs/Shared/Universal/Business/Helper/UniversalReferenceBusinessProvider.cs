using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class UniversalReferenceBusinessProvider
	{
		public UniversalReferenceBusinessProvider()
		{
		}

		public static UniversalReferenceBusinessProvider GetProvider(BusinessObjectFactory factory, ZString countryOrGrouping)
		{
			var cacheKey = string.Format(Culture.Invariant, "UniversalReferenceBusinessProvider_{0}", countryOrGrouping);
			return factory.GetCachedValue(cacheKey, () =>
			{
				UniversalReferenceBusinessProvider provider = null;
				var builders = ObjectFactory.Get<Hashtable>("UniversalReferenceBusinessProvider");

				if (!countryOrGrouping.IsEmpty)
				{
					var key = string.Format(Culture.Invariant, "{0}", countryOrGrouping);
					var objectHandle = (ObjectHandle)builders[key];
					provider = (UniversalReferenceBusinessProvider)objectHandle?.GetObject();
				}
				if (provider == null)
				{
					var objectHandle = (ObjectHandle)builders[Constants.UniversalReferenceBusinessProvider.Default];
					provider = (UniversalReferenceBusinessProvider)objectHandle?.GetObject();
				}

				return provider;
			});
		}

		#region TariffDateTimeFormat

		public ZDateTimePickerFormat TariffDateTimeFormat => GetTariffDateTimeFormat();

		protected virtual ZDateTimePickerFormat GetTariffDateTimeFormat()
		{
			return Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
		}

		#endregion
	}
}
