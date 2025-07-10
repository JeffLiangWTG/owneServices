using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DIS.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class DISDocumentLabelCollection : ZZRefCusCodeListCombinedCollection
	{
		public DISDocumentLabelCollection(BusinessObjectFactory factory, IEnumerable<ZString> formGroups)
			: base(factory)
		{
			this.formGroups = formGroups;
		}
		readonly IEnumerable<ZString> formGroups;

		protected override ZQuery CreateRelationshipFilter()
		{
			if (disDocumentRelationshipFilter == null)
			{
				disDocumentRelationshipFilter = new ZDBOnlyQuery(typeof(RefCusCodeList));
				disDocumentRelationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
				disDocumentRelationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList);
				disDocumentRelationshipFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;

				if (formGroups != null)
				{
					var codeListAttributeSubQuery = new ZDBOnlySubQuery(typeof(RefCusCodeListAttribute), RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, ZZRefCusCodeListCombinedSchema.PK);
					codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, RefCusCodeListAttributeTypes.Codes.USDISFormGroup);
					codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_Value, formGroups);
					disDocumentRelationshipFilter.AddSubQuery(codeListAttributeSubQuery, JoinCondition.And);
				}
			}

			return disDocumentRelationshipFilter;
		}
		ZDBOnlyQuery disDocumentRelationshipFilter;

		public ZZRefCusCodeListCombined FindBestMatch(ZString code)
		{
			var codeAsCaps = code.ToUpper();
			return codeAsCaps.IsEmpty ? null : CodesCollection.FirstOrDefault(c => c.ZZD_Code == codeAsCaps);
		}

		ZZRefCusCodeListCombined[] CodesCollection
		{
			get
			{
				if (codesCollection == null)
				{
					var filter = CreateRelationshipFilter();
					filter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code + ", " + ZZRefCusCodeListCombinedSchema.Constants.ZZD_EndDate + " " + OrderByClause.Descending;
					codesCollection = Factory.Load<ZZRefCusCodeListCombined>(filter);
				}
				return codesCollection;
			}
		}
		ZZRefCusCodeListCombined[] codesCollection;
	}
}
