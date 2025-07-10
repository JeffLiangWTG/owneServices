using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ViewRelatedActivityPivot : AutoViewRelatedActivityPivot, IRelatedActivityPivot
	{
		public ViewRelatedActivityPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Load / Create

		public static ViewRelatedActivityPivot Load(BusinessObjectFactory factory, IRelatableActivity parent, IRelatableActivity child)
		{
			var query = new ZQuery();
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, parent.TablePrefix);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, parent.PK);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, child.TablePrefix);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, child.PK);

			return factory.LoadTop1<ViewRelatedActivityPivot>(query);
		}

		public static ViewRelatedActivityPivot[] LoadPivotsWithChild(BusinessObjectFactory factory, IRelatableActivity child)
		{
			var query = new ZQuery();
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, child.TablePrefix);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, child.PK);

			return factory.Load<ViewRelatedActivityPivot>(query);
		}

		public static ViewRelatedActivityPivot[] LoadPivotsWithChild(BusinessObjectFactory factory, IRelatableActivity child, IEnumerable<ZString> parentTableCodes, bool fetchOnlyFromLocalCache)
		{
			return LoadPivotsWithChild(factory, child.PK, child.TablePrefix, parentTableCodes, fetchOnlyFromLocalCache);
		}

		public static ViewRelatedActivityPivot[] LoadPivotsWithChild(BusinessObjectFactory factory, ZGuid childId, ZString childTableCode, IEnumerable<ZString> parentTableCodes, bool fetchOnlyFromLocalCache = false)
		{
			var query = new ZQuery();
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, parentTableCodes);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, childTableCode);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, childId);
			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;

			return factory.Load<ViewRelatedActivityPivot>(query);
		}

		public static ViewRelatedActivityPivot[] LoadPivotsWithParent(BusinessObjectFactory factory, IRelatableActivity parent)
		{
			var query = new ZQuery();
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, parent.TablePrefix);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, parent.PK);

			return factory.Load<ViewRelatedActivityPivot>(query);
		}

		public static ViewRelatedActivityPivot[] LoadPivotsWithParent(BusinessObjectFactory factory, IRelatableActivity parent, IEnumerable<ZString> childTableCodes, bool fetchOnlyFromLocalCache)
		{
			return LoadPivotsWithParent(factory, parent.PK, parent.TablePrefix, childTableCodes, fetchOnlyFromLocalCache);
		}

		public static ViewRelatedActivityPivot[] LoadPivotsWithParent(BusinessObjectFactory factory, ZGuid parentId, ZString parentTableCode, IEnumerable<ZString> childTableCodes, bool fetchOnlyFromLocalCache = false)
		{
			var query = new ZQuery();
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, parentTableCode);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, parentId);
			query.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, childTableCodes);
			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;

			return factory.Load<ViewRelatedActivityPivot>(query);
		}

		public static ViewRelatedActivityPivot Create(BusinessObjectFactory factory, IRelatableActivity parent, IRelatableActivity child)
		{
			var result = factory.New<ViewRelatedActivityPivot>();
			using (result.GetValidationSuspender())
			{
				result.RAP_ParentActivityTableCode = parent.TablePrefix;
				result.RAP_ParentActivityID = parent.PK;
				result.RAP_ChildActivityTableCode = child.TablePrefix;
				result.RAP_ChildActivityID = child.PK;
			}

			return result;
		}

		#endregion

		public static void DeleteAllPivots(IRelatableActivity activity)
		{
			var pivotsWithChildQuery = new ZQuery();
			pivotsWithChildQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, activity.TablePrefix);
			pivotsWithChildQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, activity.PK);

			var pivotsWithParentQuery = new ZQuery();
			pivotsWithParentQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, activity.TablePrefix);
			pivotsWithParentQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, activity.PK);

			var pivots = activity.Factory.Load<ViewRelatedActivityPivot>(new ZQuery(pivotsWithChildQuery, JoinCondition.Or, pivotsWithParentQuery));

			foreach (var pivot in pivots)
			{
				pivot.Delete();
			}
		}

		public static void MoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete(ISubRelatableActivity activity)
		{
			var pivotsWithActivityAsParent = LoadPivotsWithParent(activity.Factory, activity);
			var pivotsWithActivityAsChild = LoadPivotsWithChild(activity.Factory, activity);

			var superActivity = activity.SuperActivity;
			if (superActivity != null && !((BusinessObject)superActivity).IsDeleted)
			{
				foreach (var pivot in pivotsWithActivityAsParent)
				{
					var existingPivot = Load(activity.Factory, superActivity, pivot.ChildActivity);
					if (existingPivot == null)
					{
						pivot.ParentActivity = superActivity;
					}
					else
					{
						pivot.Delete();
					}
				}

				foreach (var pivot in pivotsWithActivityAsChild)
				{
					var existingPivot = Load(activity.Factory, pivot.ParentActivity, superActivity);
					if (existingPivot == null)
					{
						pivot.ChildActivity = superActivity;
					}
					else
					{
						pivot.Delete();
					}
				}
			}
			else
			{
				pivotsWithActivityAsParent.DeleteAll();
				pivotsWithActivityAsChild.DeleteAll();
			}
		}

		public IRelatableActivity ParentActivity
		{
			get
			{
				if (TablePrefixesThatShouldBeLoadedWithSpecificElementType.TryGetValue(RAP_ParentActivityTableCode, out var elementTypeForLoad))
				{
					return Factory.Load(elementTypeForLoad, RAP_ParentActivityID) as IRelatableActivity;
				}

				return Factory.Load(RAP_ParentActivityTableCode, RAP_ParentActivityID) as IRelatableActivity;
			}
			set
			{
				RAP_ParentActivityTableCode = value.TablePrefix;
				RAP_ParentActivityID = value.PK;
			}
		}

		public IRelatableActivity ChildActivity
		{
			get
			{
				if (TablePrefixesThatShouldBeLoadedWithSpecificElementType.TryGetValue(RAP_ChildActivityTableCode, out var elementTypeForLoad))
				{
					return Factory.Load(elementTypeForLoad, RAP_ChildActivityID) as IRelatableActivity;
				}

				return Factory.Load(RAP_ChildActivityTableCode, RAP_ChildActivityID) as IRelatableActivity;
			}
			set
			{
				RAP_ChildActivityTableCode = ((BusinessObject)value).TablePrefix;
				RAP_ChildActivityID = value.PK;
			}
		}

		bool IRelatedActivityPivot.IsEditable => RAP_IsEditable;

		public IDictionary<string, Type> TablePrefixesThatShouldBeLoadedWithSpecificElementType
		{
			get
			{
				return tablePrefixesThatShouldBeLoadedWithSpecificElementType ?? (tablePrefixesThatShouldBeLoadedWithSpecificElementType = RelatedActivityLinkLookups.GetTablePrefixesThatShouldBeLoadedWithSpecificElementType(Factory));
			}
		}

		IDictionary<string, Type> tablePrefixesThatShouldBeLoadedWithSpecificElementType;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			salesRelationTreeIdUpdatedThisFactorySave = false;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsSalesRelationPivot && (HasChanges || !IsInDatabase) && !salesRelationTreeIdUpdatedThisFactorySave)
			{
				UpdateSalesRelationTreeIdForWholeTree();
			}
			ParentActivity?.OnRelatedActivitySaving(ChildActivity);
			ChildActivity?.OnRelatedActivitySaving(ParentActivity);
		}

		public override void Delete()
		{
			if (IsSalesRelationPivot && !IsMultipleParentsAllowed(RAP_ParentActivityTableCode))
			{
				var updatedPivots = new HashSet<ZGuid>();
				var childPivots = GetChildSalesRelationPivots();
				foreach (var childPivot in childPivots)
				{
					childPivot.UpdateSalesRelationTreeIdForAllDescendants_DuringDelete(childPivot.RAP_ParentActivityID, updatedPivots);
				}
			}
			base.Delete();
		}

		public bool IsSalesRelationPivot => SalesRelationActivityTablePrefixes.Contains(RAP_ParentActivityTableCode) && SalesRelationActivityTablePrefixes.Contains(RAP_ChildActivityTableCode);

		HashSet<ZString> SalesRelationActivityTablePrefixes
		{
			get { return GetSalesRelationActivityTablePrefixes(Factory); }
		}

		static HashSet<ZString> GetSalesRelationActivityTablePrefixes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("SalesRelationActivityTablePrefixes", () =>
			{
				var result = new HashSet<ZString>();
				var salesRelationTypes = SalesRelationTypeList.New().GetCodes();
				var definitions = RelatedActivityLinkLookups.GetRelatableActivityTypeDefinitions(factory);
				var subActivityTablePrefixes = RelatedActivityLinkLookups.GetTablePrefixesForSuperAndSubActivities(factory);

				foreach (var salesRelationType in salesRelationTypes)
				{
					var tablePrefix = definitions[salesRelationType].TablePrefix;
					result.Add(tablePrefix);

					if (subActivityTablePrefixes.TryGetValue(tablePrefix, out var subActivityTablePrefix))
					{
						result.Add(subActivityTablePrefix);
					}
				}

				return result;
			});
		}

		void UpdateSalesRelationTreeIdForWholeTree()
		{
			var topLevelSalesPivot = GetTopLevelSalesRelationPivot(new List<ZString>());
			if (!IsMultipleParentsAllowed(topLevelSalesPivot.RAP_ParentActivityTableCode))
			{
				var salesRelationTreeId = topLevelSalesPivot.RAP_ParentActivityID;
				var allTopLevelSalesPivotsForThisTree = GetChildSalesRelationPivots(topLevelSalesPivot.RAP_ParentActivityID, topLevelSalesPivot.RAP_ParentActivityTableCode, true);
				foreach (var otherTopLevelSalesPivot in allTopLevelSalesPivotsForThisTree)
				{
					otherTopLevelSalesPivot.UpdateSalesRelationTreeIdForAllDescendants_DuringSave(salesRelationTreeId);
				}
			}
			else
			{
				var salesRelationTreeId = topLevelSalesPivot.RAP_ChildActivityID;
				topLevelSalesPivot.UpdateSalesRelationTreeIdForAllDescendants_DuringSave(salesRelationTreeId);
			}
		}

		bool IsMultipleParentsAllowed(string tableCode)
		{
			return IsMultipleParentsAllowed(Factory, tableCode);
		}

		public static bool IsMultipleParentsAllowed(BusinessObjectFactory factory, string tableCode)
		{
			var salesRelationActivityTablePrefixes = GetSalesRelationActivityTablePrefixes(factory);
			return !salesRelationActivityTablePrefixes.Contains(tableCode) || tableCode == GlbCompanyCampaignSchema.Constants.Prefix || tableCode == OrgSalesCallSchema.Constants.Prefix;
		}

		IEnumerable<ViewRelatedActivityPivot> GetChildSalesRelationPivots()
		{
			return GetChildSalesRelationPivots(RAP_ChildActivityID, RAP_ChildActivityTableCode, false);
		}

		IEnumerable<ViewRelatedActivityPivot> GetChildSalesRelationPivots(ZGuid id, ZString tableCode, bool fetchOnlyFromLocalCache)
		{
			return IsMultipleParentsAllowed(tableCode) ? Enumerable.Empty<ViewRelatedActivityPivot>() : LoadPivotsWithParent(Factory, id, tableCode, SalesRelationActivityTablePrefixes, fetchOnlyFromLocalCache);
		}

		string GetChildActivityIDCodeName()
		{
			var name = ChildActivity?.HumanReadableName ?? "NULL";
			var childActivityIDCodeName = $"({RAP_ChildActivityID} - {RAP_ChildActivityTableCode} - {name})";

			return childActivityIDCodeName;
		}

		string GetSalesRelationLoopMessage(List<ZString> visitedActivities)
		{
			var stringBuilder = new StringBuilder();
			for (var i = 0; i < visitedActivities.Count; i++)
			{
				stringBuilder.Append($"{visitedActivities[i]} -> ");
			}

			stringBuilder.Append(GetChildActivityIDCodeName());
			return stringBuilder.ToString();
		}

		ViewRelatedActivityPivot GetTopLevelSalesRelationPivot(List<ZString> visitedActivities)
		{
			var visitedActivity = GetChildActivityIDCodeName();

			if (visitedActivities.Contains(visitedActivity))
			{
				ErrorReporter.ReportOnce("LoopDetectedInGetTopLevelSalesRelationPivot", "Loop in Sales Relation Tree: " + GetSalesRelationLoopMessage(visitedActivities));
				throw new ZCannotSaveException("Top level sales relation pivot cannot be found due to loop in the sales relation.", "Cannot save");
			}
			visitedActivities.Add(visitedActivity);
			var parentSalesRelationPivot = GetParentSalesRelationPivot();
			return parentSalesRelationPivot == null ? this : parentSalesRelationPivot.GetTopLevelSalesRelationPivot(visitedActivities);
		}

		ViewRelatedActivityPivot GetParentSalesRelationPivot()
		{
			return IsMultipleParentsAllowed(RAP_ParentActivityTableCode) ? null : LoadPivotsWithChild(Factory, RAP_ParentActivityID, RAP_ParentActivityTableCode, SalesRelationActivityTablePrefixes).FirstOrDefault();
		}

		void UpdateSalesRelationTreeIdForAllDescendants_DuringSave(ZGuid salesRelationTreeId)
		{
			if (!salesRelationTreeIdUpdatedThisFactorySave)
			{
				salesRelationTreeIdUpdatedThisFactorySave = true;
				UpdateSalesRelationTreeIdThenRunActionForDescendants(salesRelationTreeId,
					childPivot => childPivot.UpdateSalesRelationTreeIdForAllDescendants_DuringSave(salesRelationTreeId));
			}
		}

		void UpdateSalesRelationTreeIdForAllDescendants_DuringDelete(ZGuid salesRelationTreeId, HashSet<ZGuid> updatedPivots)
		{
			if (!updatedPivots.Contains(PK))
			{
				updatedPivots.Add(PK);
				UpdateSalesRelationTreeIdThenRunActionForDescendants(salesRelationTreeId,
					childPivot => childPivot.UpdateSalesRelationTreeIdForAllDescendants_DuringDelete(salesRelationTreeId, updatedPivots));
			}
		}

		void UpdateSalesRelationTreeIdThenRunActionForDescendants(ZGuid salesRelationTreeId, Action<ViewRelatedActivityPivot> actionForDescendants)
		{
			using (GetValidationSuspender())
			{
				RAP_SalesRelationTreeID = salesRelationTreeId;
			}

			var childPivots = GetChildSalesRelationPivots();
			foreach (var childPivot in childPivots)
			{
				actionForDescendants.Invoke(childPivot);
			}
		}

		bool salesRelationTreeIdUpdatedThisFactorySave;
	}
}
