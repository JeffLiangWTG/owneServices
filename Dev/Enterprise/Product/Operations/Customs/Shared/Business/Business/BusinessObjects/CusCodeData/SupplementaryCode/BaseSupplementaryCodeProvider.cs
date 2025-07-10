using System.Collections;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseSupplementaryCodeProvider : CusCodeDataWithOrderProvider
	{
		protected internal BaseSupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode)
		{
		}

		protected internal BaseSupplementaryCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		public BaseSupplementaryCodeValidation GetNewValidation(BaseSupplementaryCode supplementaryCode) => GetNewValidationCore(supplementaryCode);
		protected virtual BaseSupplementaryCodeValidation GetNewValidationCore(BaseSupplementaryCode supplementaryCode) => new BaseSupplementaryCodeValidation(supplementaryCode);

		public BaseSupplementaryCodeLookups GetNewLookups(BaseSupplementaryCode supplementaryCode) => GetNewLookupsCore(supplementaryCode);
		protected virtual BaseSupplementaryCodeLookups GetNewLookupsCore(BaseSupplementaryCode supplementaryCode) => new BaseSupplementaryCodeLookups(supplementaryCode);

		public BaseSupplementaryCodePropertyChangedNotifier GetNewSupplementaryCodePropertyChangedNotifier(BaseSupplementaryCode supplementaryCode) => GetNewSupplementaryCodePropertyChangedNotifierCore(supplementaryCode);

		protected virtual BaseSupplementaryCodePropertyChangedNotifier GetNewSupplementaryCodePropertyChangedNotifierCore(BaseSupplementaryCode supplementaryCode) => new (supplementaryCode);

		public static BaseSupplementaryCodeProvider GetBySupplementaryCodeSupporter(ISupplementaryCodeSupporter supplementaryCodeSupporter)
		{
			BaseSupplementaryCodeProvider result = null;
			var countryCode = supplementaryCodeSupporter?.GetCountryCodeForCodeProvider() ?? ZString.Empty;
			if (!countryCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("SupplementaryCodeProviders");
				var objectHandle = (ObjectHandle)types[countryCode.ToString()];
				result = (BaseSupplementaryCodeProvider)objectHandle?.GetObject(countryCode, supplementaryCodeSupporter) ?? new BaseSupplementaryCodeProvider(countryCode, supplementaryCodeSupporter);
			}
			return result;
		}

		public static BaseSupplementaryCodeProvider GetByCountryCode(ZString countryCode)
			=> SupplementaryCodeProviderFactory.GetByCountryCodeOrDefault(countryCode, () => new BaseSupplementaryCodeProvider(countryCode));
	}
}
