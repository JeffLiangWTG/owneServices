using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	class ClassificationTypeProvider : IClassificationTypeProvider
	{
		public static IClassificationTypeProvider GetProviderFor(string country)
		{
			IClassificationTypeProvider result;
			country = CountryCodes.GetCustomsCountryOfJurisdiction(country ?? string.Empty);
			if (!ClassificationTypeProviders.TryGetValue(country, out result))
			{
				var types = ObjectFactory.Get<Hashtable>("ClassificationTypeProviders");
				var objectHandle = (ObjectHandle)types[country];
				if (objectHandle == null && ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(country))
				{
					objectHandle = (ObjectHandle)types[Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN];
				}

				result = (IClassificationTypeProvider)objectHandle?.GetObject() ?? new ClassificationTypeProvider();
				ClassificationTypeProviders.Add(country, result);
			}

			return result;
		}

		static Dictionary<ZString, IClassificationTypeProvider> ClassificationTypeProviders => classificationTypeProviders ?? (classificationTypeProviders = new Dictionary<ZString, IClassificationTypeProvider>());

		[ThreadStatic]
		static Dictionary<ZString, IClassificationTypeProvider> classificationTypeProviders;

		ZString IClassificationTypeProvider.HTICode => ClassificationTypeList.Codes.HTI;

		ZString IClassificationTypeProvider.HTECode => ClassificationTypeList.Codes.HTE;

		ZString IClassificationTypeProvider.SHBCode => throw new NotSupportedException("If you implement the Pivot matching by type feature, you should override this property.");

		ZString IClassificationTypeProvider.HTBCode => ClassificationTypeList.Codes.HTB;
	}
}
