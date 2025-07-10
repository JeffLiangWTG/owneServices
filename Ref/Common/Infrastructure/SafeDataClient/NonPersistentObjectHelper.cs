using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public static class NonPersistentObjectHelper
	{
		public static IEnumerable<INonPersistentBusinessObjectFlatten> Build<T>(IEnumerable<(T, RefCusApplicability[])> parentAndApps, ISafeRepository repo)
			where T : INonPersistentBusinessObjectParent
		{
			return parentAndApps.SelectMany(x => x.Item2.Select(y => CreateNonPersistentFlattenObject(x.Item1, y, repo)));
		}

		static INonPersistentBusinessObjectFlatten CreateNonPersistentFlattenObject(INonPersistentBusinessObjectParent parent, RefCusApplicability app, ISafeRepository repo)
		{
			if (parent is RefCusRate rate)
			{
				return new RefCusRateApplicability(rate, app, repo);
			}
			else if (parent is RefCusCondition cond)
			{
				return new RefCusConditionApplicability(cond, app, repo);
			}
			return null;
		}

		static (IEnumerable<object>, IEnumerable<object>) Merge(
			IEnumerable<INonPersistentBusinessObjectParent> allParents,
			IEnumerable<INonPersistentBusinessObjectFlatten> allFlattenObjectsInParents,
			IEnumerable<INonPersistentBusinessObjectFlatten> linkedFlattenObjects,
			INonPersistentBusinessObjectComparison comp,
			List<INonPersistentBusinessObjectFlatten> unprocesssedAddObjects
		)
		{
			var unlinkedObjs = new List<object>();
			var addedObjs = new List<object>();
			addedObjs.AddRange(linkedFlattenObjects.SelectMany(x => x.Create()));
			var unprocessedUpdateObjects = new List<INonPersistentBusinessObjectFlatten>(linkedFlattenObjects);
			foreach (var linkedFlattenObject in linkedFlattenObjects)
			{
				var matchedParent = allParents.OrderBy(x => x.GetPKValue()).FirstOrDefault(x => comp.IsIdentical(linkedFlattenObject, x));
				if (matchedParent != null)
				{
					if (linkedFlattenObject.Parent != matchedParent)
					{
						linkedFlattenObject.Link(matchedParent, comp, out var linkedResult);
						unlinkedObjs.AddRange(linkedResult);
					}
					linkedFlattenObject.Update(false);
					unprocessedUpdateObjects.Remove(linkedFlattenObject);
				}
			}

			while (unprocessedUpdateObjects.Count > 0)
			{
				var updateObject = unprocessedUpdateObjects[0];
				unprocessedUpdateObjects.Remove(updateObject);
				var parent = updateObject.Parent;
				var flattenObjectsInParent = allFlattenObjectsInParents.Where(x => x.Parent == parent).ToArray();
				if (flattenObjectsInParent.Any(x => comp.IsIdentical(x, parent)))
				{
					unlinkedObjs.AddRange(updateObject.Unlink());
					unprocesssedAddObjects.Add(updateObject);
				}
				else
				{
					updateObject.Update(true);
					foreach (var flattenObject in flattenObjectsInParent)
					{
						if (flattenObject != updateObject && !comp.IsIdentical(flattenObject, parent))
						{
							unlinkedObjs.AddRange(flattenObject.Unlink());
							unprocessedUpdateObjects.Remove(flattenObject);
							unprocesssedAddObjects.Add(flattenObject);
						}
					}
					var matchedFlattenObjects = unprocessedUpdateObjects.Where(x => comp.IsIdentical(x, parent)).ToArray();
					foreach (var matchedFlattenObject in matchedFlattenObjects)
					{
						if (parent != matchedFlattenObject.Parent)
						{
							matchedFlattenObject.Link(parent, comp, out var linkedResult);
							unlinkedObjs.AddRange(linkedResult);
						}
						matchedFlattenObject.Update(false);
						unprocessedUpdateObjects.Remove(matchedFlattenObject);
					}
				}
			}

			return (addedObjs, unlinkedObjs);
		}

		public static IEnumerable<object> GetDeleteObjects(IEnumerable<object> unlinkedObjs)
		{
			var result = new List<object>();
			foreach (var unlinkedObj in unlinkedObjs)
			{
				if (unlinkedObj is RefCusRateUOM uom && uom.RefCusRateApplicabilityUOMs.Count == 0)
				{
					result.Add(uom);
					var unLinkedRate = unlinkedObjs.OfType<RefCusRate>().FirstOrDefault(x => x.RefCusRateUOMs.Contains(uom));
					unLinkedRate?.RefCusRateUOMs.Remove(uom);
				}
				else if (unlinkedObj is RefCusExcludedTradeGroup ex && ex.RefCusExcludedTradeGroupNews.Count == 0)
				{
					result.Add(ex);
					var unlikedApp = unlinkedObjs.OfType<RefCusApplicability>().FirstOrDefault(x => x.RefCusExcludedTradeGroups.Contains(ex));
					unlikedApp?.RefCusExcludedTradeGroups.Remove(ex);
				}
				else if (unlinkedObj is RefCusApplicability app && app.RefCusRateApplicabilities.Count + app.RefCusConditionApplicabilities.Count == 0)
				{
					result.Add(app);
					var unLinkedRate = unlinkedObjs.OfType<RefCusRate>().FirstOrDefault(x => x.RefCusApplicabilities.Contains(app));
					unLinkedRate?.RefCusApplicabilities.Remove(app);

					var unLinkedCond = unlinkedObjs.OfType<RefCusCondition>().FirstOrDefault(x => x.RefCusApplicabilities.Contains(app));
					unLinkedCond?.RefCusApplicabilities.Remove(app);
				}
				else if (unlinkedObj is RefCusConditionValue val && val.RefCusConditionApplicabilityValues.Count == 0)
				{
					result.Add(val);
					var unLinkedCond = unlinkedObjs.OfType<RefCusCondition>().FirstOrDefault(x => x.RefCusConditionValues.Contains(val));
					unLinkedCond?.RefCusConditionValues.Remove(val);
				}
				else if (unlinkedObj is RefCusConditionLanguage lang && lang.RefCusConditionApplicabilityLanguages.Count == 0)
				{
					result.Add(lang);
					var unLinkedCond = unlinkedObjs.OfType<RefCusCondition>().FirstOrDefault(x => x.RefCusConditionLanguages.Contains(lang));
					unLinkedCond?.RefCusConditionLanguages.Remove(lang);
				}
				else if (unlinkedObj is RefCusRate rate && rate.RefCusRateApplicabilities.Count == 0)
				{
					result.Add(rate);
				}
				else if (unlinkedObj is RefCusCondition cond && cond.RefCusConditionApplicabilities.Count == 0)
				{
					result.Add(cond);
				}
			}
			return result;
		}

		public static (IEnumerable<object> AddedObjects, IEnumerable<object> UpdatedObjects, IEnumerable<object>
			UnlinkedObjects) Merge(
				IEnumerable<INonPersistentBusinessObjectParent> allParents,
				IEnumerable<INonPersistentBusinessObjectFlatten> allFlattenObjectsFromParent,
				IEnumerable<INonPersistentBusinessObjectFlatten> changedFlattenObjects,
			INonPersistentBusinessObjectComparison comp)
		{
			var addedObjs = new List<object>();
			var updatedObjs = new List<object>();
			var unlinkedObjs = new List<object>();

			var unlinkObjects =
				new List<INonPersistentBusinessObjectFlatten>(changedFlattenObjects.Where(x => x.Parent == null));

			var updatedObjectsResult = Merge(allParents, allFlattenObjectsFromParent,
				changedFlattenObjects.Where(x => x.Parent != null), comp, unlinkObjects);
			addedObjs.AddRange(updatedObjectsResult.Item1);
			unlinkedObjs.AddRange(updatedObjectsResult.Item2);

			var allParentsList = allParents.OrderBy(x => x.GetPKValue()).ToList();
			while (unlinkObjects.Count > 0)
			{
				var obj = unlinkObjects[0];
				unlinkObjects.Remove(obj);
				var created = obj.Create();
				var matchedParent = allParentsList.FirstOrDefault(x => comp.IsIdentical(obj, x));
				if (matchedParent != null)
				{
					obj.Link(matchedParent, comp, out var linkResult);
					unlinkedObjs.AddRange(linkResult);
				}
				obj.Update(false);
				addedObjs.AddRange(created);
				if (matchedParent == null)
				{
					allParentsList.Add(created.OfType<INonPersistentBusinessObjectParent>().FirstOrDefault());
				}
			}

			foreach (INonPersistentBusinessObjectFlatten obj in changedFlattenObjects)
			{
				foreach (var linkedObj in obj.LinkedObjects)
				{
					if (!addedObjs.Contains(linkedObj))
					{
						updatedObjs.Add(linkedObj);
					}
				}
			}

			return (addedObjs.Where(x => x != null).Distinct(), updatedObjs.Where(x => x != null).Distinct(), unlinkedObjs);
		}
	}
}
