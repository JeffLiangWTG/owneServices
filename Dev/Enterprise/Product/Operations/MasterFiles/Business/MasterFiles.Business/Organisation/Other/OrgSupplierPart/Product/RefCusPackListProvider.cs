using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefCusPackListProvider : IRefCusPackListProvider
	{
		public virtual CodeDescriptionPairList GetCustomsPackList(BusinessObjectFactory factory, ZString type, ZString country)
		{
			CodeDescriptionPairList list;
			if (type == Customs.RPTypeList.Codes.AllAreas)
			{
				list = GetCustomsPackListForAllAreas(factory, country);
			}
			else if (type == Customs.RPTypeList.Codes.CommercialInvoice)
			{
				list = GetCIPCustomsPackList(factory, country);
			}
			else if (type == Customs.RPTypeList.Codes.GlobalManifestLine || type == Customs.RPTypeList.Codes.GlobalManifestBill)
			{
				list = new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits();
			}
			else if (type == Customs.RPTypeList.Codes.AMSManifest || type == Customs.RPTypeList.Codes.AFRManifest)
			{
				list = GetCusPackListForType(factory, type);
			}
			else if (type == Customs.RPTypeList.Codes.PackingDeclaration)
			{
				list = GetDeclarationPackTypeList(factory, country);
			}
			else
			{
				list = factory.GetCachedValue<BaseCusUQList>();
			}

			return list;
		}

		public virtual CodeDescriptionPairList GetCustomsPackListForAllAreas(BusinessObjectFactory factory, ZString country) => GetCIPCustomsPackList(factory, country);

		public virtual CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country)
		{
			var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
			var listForCountry = zzCusCodeListGetter.GetList(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, ZDateTime.Today, null);
			if (listForCountry.Count > 0)
			{
				return (CodeDescriptionPairList)listForCountry;
			}
			else
			{
				switch (country.ToString())
				{
					case Core.Constants.CountryCodes.UnitedKingdom:
					case Core.Constants.CountryCodes.SouthAfrica:
						return (CodeDescriptionPairList)zzCusCodeListGetter.GetList(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, new ZDateTime(2012, 1, 1));
					default:
						return factory.GetCachedValue<BaseCusUQList>();
				}
			}
		}

		public virtual CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory) => new CodeDescriptionPairList();

		static CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory, ZString country) => GetDeclarationPackTypeListProvider(factory, country).GetDeclarationPackTypeList(factory);

		internal static IRefCusPackListProvider GetDeclarationPackTypeListProvider(BusinessObjectFactory factory, ZString country)
		{
			var customsCountry = Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(country);
			var provider = Loader.GetRefCusPackListProvider(factory, customsCountry);
			if (provider == null && ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(customsCountry))
			{
				provider = Loader.GetRefCusPackListProvider(factory, "EU");
			}

			return provider ?? Loader.GetRefCusPackListProvider(factory, (NoResString)"General");
		}

		static CodeDescriptionPairList GetCusPackListForType(BusinessObjectFactory factory, ZString type)
		{
			var cachedKey = string.Format(CultureInfo.InvariantCulture, "GetCusPackList_{0}", type);
			return factory.GetCachedValue(cachedKey, () =>
			{
				var zzLists = ObjectFactory.Get<Hashtable>("RefCusPackListProviders");
				var objectHandle = (ObjectHandle)zzLists[type.ToString()];
				var list = (CodeDescriptionPairList)objectHandle?.GetObject() ?? new BaseCusUQList();
				return list;
			});
		}

		public virtual CodeDescriptionPairList GetCommercialPackList(BusinessObjectFactory factory, ZString type)
		{
			return new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits();
		}

		public virtual CodeDescriptionPairList GetPackConversionTypeList(BusinessObjectFactory factory) => null;

		public static class Loader
		{
			public static IRefCusPackListProvider GetRefCusPackListProvider(BusinessObjectFactory factory, ZString country)
			{
				var cachedKey = string.Format(CultureInfo.InvariantCulture, "RefCusPackListProvider_{0}", country);
				return factory.GetCachedValue(cachedKey, () =>
				{
					var zzProviders = ObjectFactory.Get<Hashtable>("RefCusPackListProviders");
					var objectHandle = (ObjectHandle)zzProviders[country.ToString()];
					var provider = (RefCusPackListProvider)objectHandle?.GetObject();
					return provider;
				});
			}
		}
	}
}
