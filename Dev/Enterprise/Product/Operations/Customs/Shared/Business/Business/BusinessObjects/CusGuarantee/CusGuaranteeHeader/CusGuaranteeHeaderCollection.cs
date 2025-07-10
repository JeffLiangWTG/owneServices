using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusGuaranteeHeaderCollection : ActiveBusinessObjectCollection<BaseCusGuaranteeHeader>
	{
		public CusGuaranteeHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusGuaranteeHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CusGuaranteeHeaderCollection(BusinessObjectFactory factory, IReadOnlyList<ZString> countryCodesToLoad, IReadOnlyList<ZString> types)
			: base(factory, SetCollectionFilter(countryCodesToLoad, types))
		{
		}

		public CusGuaranteeHeaderCollection(BusinessObjectFactory factory, IReadOnlyList<ZString> countryCodesToLoad, IReadOnlyList<ZString> types, IReadOnlyList<ZString> references)
			: base(factory, SetCollectionFilterWithReferences(countryCodesToLoad, types, references))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
		public class FilterConstants
		{
			public const string GuaranteeNumber = "Guarantee Number";
			public const string GuaranteeHolder = "Guarantee Holder";
			public const string GuaranteeHolders = "Guarantee Holders";
			public const string GuaranteeSubType = "Guarantee Subtype";
			public const string GuaranteeType = "Guarantee Type";
			public const string EndDate = "End Date";
		}

		static ZQuery SetCollectionFilter(IReadOnlyList<ZString> countryCodes, IReadOnlyList<ZString> types, ZQuery query = null)
		{
			if (query == null)
			{
				query = new ZQuery();
			}

			if (types.Count > 0)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, types);
			}

			if (countryCodes.Count > 0)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCodes);
			}

			return query;
		}

		static ZQuery SetCollectionFilterWithReferences(IReadOnlyList<ZString> countryCodes, IReadOnlyList<ZString> types, IReadOnlyList<ZString> references)
		{
			if (references.Count > 0)
			{
				var query = new ZDBOnlyQuery(typeof(BaseCusGuaranteeHeader));

				var cusCodeDataInReferences = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
				cusCodeDataInReferences.AddToFilter(CusCodeDataSchema.CY_Code, references);
				cusCodeDataInReferences.AddToFilter(CusCodeDataSchema.CY_Type, GuaranteeCusCodeDataTypeList.Codes.GRN);
				cusCodeDataInReferences.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, CusPermitHeaderSchema.Constants.Prefix);
				query.AddSubQuery(CusPermitHeaderSchema.PK, CusCodeDataSchema.CY_ParentID, cusCodeDataInReferences, JoinCondition.And);
				var noCusCodeData = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID, true);
				noCusCodeData.AddToFilter(CusCodeDataSchema.CY_Type, GuaranteeCusCodeDataTypeList.Codes.GRN);
				noCusCodeData.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, CusPermitHeaderSchema.Constants.Prefix);
				query.AddSubQuery(CusPermitHeaderSchema.PK, CusCodeDataSchema.CY_ParentID, noCusCodeData, JoinCondition.Or);

				return (ZDBOnlyQuery)SetCollectionFilter(countryCodes, types, query);
			}

			return SetCollectionFilter(countryCodes, types);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			return query;
		}
	}
}
