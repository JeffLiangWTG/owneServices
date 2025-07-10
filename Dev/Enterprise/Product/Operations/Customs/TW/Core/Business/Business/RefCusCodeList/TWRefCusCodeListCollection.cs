using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class TWRefCusCodeListCollection : Universal.ZZRefCusCodeListCombinedCollection, Integration.Customs.TW.ITWRefCusCodeListCollection
	{
		public TWRefCusCodeListCollection(BusinessObjectFactory factory, ZString codeType, ZDateTime date)
			: base(factory, Core.Constants.CountryCodes.Taiwan, codeType, date)
		{
			InitialiseFilterDefaults();
		}

		protected void InitialiseFilterDefaults()
		{
			FilterBusinessObjectDefaults.RemoveAll();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.CountryCodes.Taiwan));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", CodeTypes.First()));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Description, "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Code, "Property", ZString.Empty));
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Description;
			return result;
		}
	}
}
