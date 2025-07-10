using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public class USCForeignPortCollection : ZZRefCusCodeListCombinedCollection
	{
		public USCForeignPortCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public USCForeignPortCollection(BusinessObjectFactory factory, USCForeignPortWrapper.Type validForType)
			: base(factory)
		{
			foreignPortTypes = new List<ZString>() { ForeignPortTypeList.Codes.Common };
			if (validForType != USCForeignPortWrapper.Type.Common)
			{
				foreignPortTypes.Add(ForeignPortTypeList.GetCodeFromType(validForType));
			}
		}
		readonly List<ZString> foreignPortTypes;

		protected override ZQuery CreateRelationshipFilter()
		{
			var attributeFilters = new List<RefCusCodeListAttributeFilter>();
			if (foreignPortTypes != null && foreignPortTypes.Count > 0)
			{
				attributeFilters.Add(new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, foreignPortTypes.ToArray()));
			}
			return ZZRefCusCodeListCombined.Loader.GetFilter(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, attributeFilters, false);
		}
	}
}
