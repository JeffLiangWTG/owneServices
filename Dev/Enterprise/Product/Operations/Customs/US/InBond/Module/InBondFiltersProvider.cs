using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusInBondMoveHeader = Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader;

namespace Enterprise.Customs.US.InBond.Module
{
	public class InBondFiltersProvider : Integration.Customs.US.InBond.IInBondFiltersProvider
	{
		public void AddFilters(IModuleFilterCollection filters, Type type)
		{
			var filterCollection = (ModuleFilterCollection)filters;
			var closedDate = filterCollection.AddDateFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondClosedDate, GetClosedDateQuery);
			closedDate.Category = FilterCategories.Dates;
			var itTypeFilter = filterCollection.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.ITType, GetInBondEntryTypeQuery, InbondCommonTypeList);
			itTypeFilter.Category = FilterCategories.ModesAndTypes;
			itTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			itTypeFilter.MaxLength = CusInBondMoveHeaderSchema.BM_InBondEntryType.MaxLength;
			parentType = type;
		}

		ZQuery GetClosedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(parentType);
			var inbondQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.BH_ParentID);
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(new DateQueryBuilder().CreateDateRange(comparisonOperator, CusInBondMoveHeaderSchema.BM_InBondClosedDate, date1.Date, date2.Date), JoinCondition.And);
			inbondQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			result.AddSubQuery(inbondQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetInBondEntryTypeQuery(SQLComparisonOperator comparisonOperator, ZString entryType)
		{
			var result = new ZDBOnlyQuery(parentType);
			var inbondQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.BH_ParentID);
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_InBondEntryType, comparisonOperator, entryType);
			inbondQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			result.AddSubQuery(inbondQuery, JoinCondition.And);
			return result;
		}

		InbondCommonTypeList InbondCommonTypeList
		{
			get { return inbondCommonTypeList ?? (inbondCommonTypeList = new InbondCommonTypeList()); }
		}
		InbondCommonTypeList inbondCommonTypeList;

		Type parentType;
	}
}
