using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	class HVLVCustomsStatusService : IService
	{
		public static HVLVCustomsStatusService GetService(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<HVLVCustomsStatusService>()
				   ?? factory.ServiceContainer.AddService(new HVLVCustomsStatusService(factory));
		}

		HVLVCustomsStatusService(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public string GetReleaseStatus(string customsStatusCode, bool isImport, BaseJobDeclaration standaloneDeclaration, ZString? countryCode = null)
		{
			return GetCustomsStatusStoreForCountry(countryCode).GetReleaseStatus(customsStatusCode, isImport, standaloneDeclaration);
		}

		public string GetCustomsStatusDescription(string customsStatusCode, bool isImport, BaseJobDeclaration standaloneDeclaration, ZString? countryCode = null)
		{
			return GetCustomsStatusStoreForCountry(countryCode).GetCustomStatusDescription(customsStatusCode, isImport, standaloneDeclaration);
		}

		public CodeDescriptionPairList GetAllRefCusCodeList(bool isImport, ZString? countryCode = null)
		{
			return GetCustomsStatusStoreForCountry(countryCode).GetAllRefCusCodeList(isImport);
		}

		DefaultCustomsStatusStore GetCustomsStatusStoreForCountry(ZString? countryCode)
		{
			var countryCodeValue = countryCode.GetValueOrDefault();
			if (countryCodeValue.IsEmpty)
			{
				countryCodeValue = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}

			if (!CustomsStatusStoreDictionary.TryGetValue(countryCodeValue, out var result))
			{
				result = DefaultCustomsStatusStore.CreateCustomsStatusStore(countryCodeValue, factory);
				CustomsStatusStoreDictionary.Add(countryCodeValue, result);
			}

			return result;
		}

		Dictionary<ZString, DefaultCustomsStatusStore> CustomsStatusStoreDictionary => customsStatusStoreDictionary ??= new Dictionary<ZString, DefaultCustomsStatusStore>();
		Dictionary<ZString, DefaultCustomsStatusStore> customsStatusStoreDictionary;
	}
}
