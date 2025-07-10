using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class CustomsOfficeCodeCollectionHelper
	{
		public static CustomsOfficeCodeCollection GetEuropeanUnionECICSList(BusinessObjectFactory factory)
		{
			var europeanUnionECICSList = new CustomsOfficeCodeCollection(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, DocDataConstants.RefCusCodeListType.Code_ECICS);

			europeanUnionECICSList.FilterBusinessObjectDefaults.RemoveAll();
			europeanUnionECICSList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)DocDataConstants.RefCusCodeListType.Code_ECICS, false));
			europeanUnionECICSList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, false));
			europeanUnionECICSList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDate.Today));

			return europeanUnionECICSList;
		}
	}
}
