using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using INZSupplierProvider = Enterprise.Integration.Customs.NZ.INZSupplierProvider;

namespace Enterprise.Customs.NZ.Business
{
	public class NZSupplierProvider : INZSupplierProvider
	{
		BusinessObjectCollection INZSupplierProvider.GetSupplierList(BusinessObjectFactory factory, IBusiness parent)
		{
			var dataGroupingCode = (ZString)Core.Constants.CountryCodes.NewZealand;
			var codeTypes = new[] { (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZSupplierListType };
			var date = ZDateTime.Today;
			var headerPk = (parent as OrgCusCode)?.OK_OH ?? ZGuid.Empty;

			var key = string.Join("_", "NZ", "ZZRefCusCodeListCombinedCollection", dataGroupingCode, codeTypes, date.Date, headerPk, true);
			return factory.GetCachedValue(key,
				() =>
				{
					var fullName = (parent as OrgCusCode)?.Header?.OH_FullName ?? string.Empty;

					var collection = new ZZRefCusCodeListCombinedCollection(factory);
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.Description, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Contains));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.Description, "Property", fullName));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", codeTypes[0], false));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "DefaultProperty", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", date));
					return collection;
				});
		}
	}
}
