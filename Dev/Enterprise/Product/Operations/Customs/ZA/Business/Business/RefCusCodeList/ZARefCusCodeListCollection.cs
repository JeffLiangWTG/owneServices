using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class ZARefCusCodeListCollection : ZZRefCusCodeListCombinedCollection, Integration.Customs.ZA.IZARefCusCodeListCollection
	{
		public ZARefCusCodeListCollection(BusinessObjectFactory factory, ZString codeType, ZDateTime date)
			: base(factory, Core.Constants.CountryCodes.SouthAfrica, codeType, date)
		{
			InitialiseFilterDefaults();
		}

		protected void InitialiseFilterDefaults()
		{
			FilterBusinessObjectDefaults.RemoveAll();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.CountryCodes.SouthAfrica));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", CodeTypes.First()));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.Description, "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.Code, "Property", ZString.Empty));
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Description;
			return result;
		}
	}
}
