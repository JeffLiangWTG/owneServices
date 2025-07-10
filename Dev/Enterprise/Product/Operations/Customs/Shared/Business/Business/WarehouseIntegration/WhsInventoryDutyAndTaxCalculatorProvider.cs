using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	class WhsInventoryDutyAndTaxCalculatorProvider : Integration.Customs.IWhsInventoryDutyAndTaxCalculatorProvider
	{
		public Integration.Customs.IWhsInventoryDutyAndTaxCalculator GetProviderFor(BusinessObjectFactory factory, ZString countryCode)
		{
			return factory.GetCachedValue(countryCode, () =>
			{
				var builders = ObjectFactory.Get<Hashtable>("WhsInventoryDutyAndTaxCalculators");
				var objectHandle = (ObjectHandle)builders[countryCode.ToString()];
				var provider = (WhsInventoryDutyAndTaxCalculator)objectHandle?.GetObject(factory)
					?? throw new NotImplementedException(string.Format(Culture.Invariant, "{0} is not currently implemented for '{1}'.", "WhsInventoryDutyAndTaxCalculator", countryCode));

				return provider;
			});
		}
	}
}
