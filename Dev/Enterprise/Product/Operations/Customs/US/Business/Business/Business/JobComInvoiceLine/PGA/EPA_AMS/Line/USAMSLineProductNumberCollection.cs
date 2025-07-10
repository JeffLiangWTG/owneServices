using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class USAMSLineProductNumberCollection : ZZRefCusCodeListCombinedCollection
	{
		public USAMSLineProductNumberCollection(BusinessObjectFactory factory, ZString productType, bool isList)
			: base(factory)
		{
			this.productType = productType;
			this.isList = isList;
			InitialiseFilterDefaults();
		}
		readonly ZString productType;
		readonly bool isList;

		protected override ZQuery CreateRelationshipFilter()
		{
			if (uSAMSLineProductNumberRelationshipFilter == null)
			{
				uSAMSLineProductNumberRelationshipFilter = new ZDBOnlyQuery(typeof(RefCusCodeList));
				uSAMSLineProductNumberRelationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
				uSAMSLineProductNumberRelationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes);
				uSAMSLineProductNumberRelationshipFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;
				if (isList && !productType.IsEmpty)
				{
					var codeListAttributeSubQuery = new ZDBOnlySubQuery(typeof(RefCusCodeListAttribute), RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, ZZRefCusCodeListCombinedSchema.PK);
					codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram);
					codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_Value, productType);
					uSAMSLineProductNumberRelationshipFilter.AddSubQuery(codeListAttributeSubQuery, JoinCondition.And);
				}
			}

			return uSAMSLineProductNumberRelationshipFilter;
		}
		ZDBOnlyQuery uSAMSLineProductNumberRelationshipFilter;

		void InitialiseFilterDefaults()
		{
			if (!isList && !productType.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", productType == All ? ZString.Empty : productType));
			}
		}

		public const string All = "All";
	}
}
