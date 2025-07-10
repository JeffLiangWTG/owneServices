using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public abstract class GenPivotDualDirectionRelatedEntityFilter : ModuleGuidPivotFilter
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected GenPivotDualDirectionRelatedEntityFilter(ZString description, GetList listDelegate, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, otherModuleID, GenPivotSchema.XX_Relation2ID, GenPivotSchema.XX_Relation1ID, listDelegate, parentBusinessObjectType, typeof(GenPivot), CreateFilter(parentTablePrefix, relatedTablePrefix))
		{
			MultilingualDescription = GetMultilingualDescription();
			otherPivotDirectionFilter = CreateFilterForOtherPivotDirection(description, listDelegate, GenPivotSchema.XX_Relation1ID, GenPivotSchema.XX_Relation2ID, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}

		protected GenPivotDualDirectionRelatedEntityFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, otherModuleID, fromColumn, toColumn, listDelegate, parentBusinessObjectType, typeof(GenPivot), CreateFilter(relatedTablePrefix, parentTablePrefix))
		{
		}

		protected GenPivotDualDirectionRelatedEntityFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		readonly GenPivotDualDirectionRelatedEntityFilter otherPivotDirectionFilter;

		static ZQuery CreateFilter(string relation1Prefix, string relation2Prefix)
		{
			return new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.ProcessManagement)
				.AddToFilter(GenPivotSchema.XX_Relation1TableCode, relation1Prefix)
				.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2Prefix);
		}

		bool NeedsAdditionOfOppositeDirectionFilter => otherPivotDirectionFilter != null;

		protected abstract MultilingualString GetMultilingualDescription();
		protected abstract GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID);

		protected abstract SchemaGuidColumn ParentPKColumn { get; }

		protected sealed override string ParentPkColumnNameForAllMatch => ParentPKColumn.Name;

		protected override void AddAdditionalFiltersIfRequired(ZDBOnlySubQuery pivotSubQuery, FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			base.AddAdditionalFiltersIfRequired(pivotSubQuery, filterBusinessObject, subModuleFilter);

			if (NeedsAdditionOfOppositeDirectionFilter)
			{
				var otherPivotDirectionQuery = otherPivotDirectionFilter.GetPivotSubQuery(filterBusinessObject, subModuleFilter);
				pivotSubQuery.AddAsUnionQuery(otherPivotDirectionQuery, addAsUnionAll: true);
			}
		}

		protected override string CreateAllMatchSql(ZDBOnlyQuery query, FilterStripBusinessObject filterBusinessObject, ZSqlParameterCollection collectionToAddRenamedParameters)
		{
			var sequenceHelper = GetParameterSequenceHelper() ?? new ParameterSequenceHelper();
			SetParameterSequenceHelper(sequenceHelper);

			var baseSql = base.CreateAllMatchSql(query, filterBusinessObject, collectionToAddRenamedParameters);

			if (NeedsAdditionOfOppositeDirectionFilter)
			{
				otherPivotDirectionFilter.SetParameterSequenceHelper(sequenceHelper);
				var otherPivotDirectionSql = otherPivotDirectionFilter.CreateAllMatchSql(query, filterBusinessObject, collectionToAddRenamedParameters);

				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"
		{0}
		UNION ALL
		{1}
", baseSql, otherPivotDirectionSql); // part of SQL statement
			}
			else
			{
				return baseSql;
			}
		}
	}
}
