using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.SafeDataClient.Default;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.OData.Client;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public class SafeRepository : ISafeRepository
	{
		public SafeRepository(Uri updateServiceUri, bool reportIssue = true, IAccessTokenProvider accessTokenProvider = null)
			: this(GetContainer(updateServiceUri, reportIssue, accessTokenProvider))
		{
			Argument.Argument.NotNull(updateServiceUri, nameof(updateServiceUri));
			AffectedRecords = 0;
		}

		static IContainer GetContainer(Uri updateServiceUri, bool reportIssue, IAccessTokenProvider accessTokenProvider)
		{
			var result = new Container(updateServiceUri);
			result.EntityParameterSendOption = EntityParameterSendOption.SendOnlySetProperties;
			result.Configurations?.ResponsePipeline?.OnEntityMaterialized(EnableTrackingHasChanges);
			result.BuildingRequest += (sender, e) => OnBuildingRequest(e, accessTokenProvider);
			if (!reportIssue)
			{
				result.BuildingRequest += SetNotReportIssueOnBuildingRequest;
			}
			return result;
		}

		static void OnBuildingRequest(BuildingRequestEventArgs e, IAccessTokenProvider accessTokenProvider)
		{
			if (accessTokenProvider != null)
			{
				var accessToken = accessTokenProvider.GetAccessToken();
				if (!string.IsNullOrEmpty(accessToken))
				{
					e.Headers?.Add("Authorization", $"Bearer {accessToken}");
				}
			}
		}

		static void SetNotReportIssueOnBuildingRequest(object sender, BuildingRequestEventArgs e)
		{
			e.Headers?.Add("ReportIssue", "false");
		}

		static void EnableTrackingHasChanges(MaterializedEntityArgs e)
		{
			var tracking = e?.Entity as IHasChangesTracker;
			tracking?.EnableTrackingHasChanges();
		}

		public SafeRepository(IContainer container)
		{
			Argument.Argument.NotNull(container, nameof(container));
			this.container = container;
			addOrUpdateNonPersist = new HashSet<INonPersistentBusinessObjectFlatten>();
			allFlattenObjects = new List<INonPersistentBusinessObjectFlatten>();
			unlinkedObjects = new List<object>();
		}
		List<INonPersistentBusinessObjectFlatten> allFlattenObjects;
		HashSet<INonPersistentBusinessObjectFlatten> addOrUpdateNonPersist;
		List<object> unlinkedObjects;

		public int AffectedRecords { get; private set; }

		public void Add<T>(T data)
		{
			Argument.Argument.NotNull(data, nameof(data));
			if (data is INonPersistentBusinessObject nonPersist)
			{
				addOrUpdateNonPersist.UnionWith(nonPersist.TopLevelNonPersistentObjects);
				return;
			}

			var entity = ConvertToRateOrConditionIfNeeded(data);
			container.AddObject(GetEntitySetName<T>(), entity);
		}

		public IQueryable<T> Get<T>() where T : class
		{
			var result = container.CreateQuery<T>(GetEntitySetName<T>());
			return result;
		}

		public IQueryable<T> GetLatest<T>() where T : class
		{
			var result = container.CreateQueryWithNoTracking<T>(GetEntitySetName<T>());
			return result;
		}

		public async Task<bool> SaveChangesAysnc()
		{
			AffectedRecords += ResetNonUpdate();
			return await container.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);
		}

		int ResetNonUpdate()
		{
			var savedRecords = 0;
			foreach (var entityState in container.EntityStates)
			{
				if (entityState.Item2 == EntityStates.Modified)
				{
					var hasChangesTracker = entityState.Item1 as IHasChangesTracker;
					if (hasChangesTracker != null && !hasChangesTracker.GetHasChanges())
					{
						container.ChangeState(hasChangesTracker, EntityStates.Unchanged);
					}
					else
					{
						savedRecords++;
					}
				}
			}
			return savedRecords;
		}

		public void Update<T>(T data)
		{
			Argument.Argument.NotNull(data, nameof(data));
			if (data is INonPersistentBusinessObject nonPersist)
			{
				addOrUpdateNonPersist.UnionWith(nonPersist.TopLevelNonPersistentObjects);
				return;
			}

			var entity = ConvertToRateOrConditionIfNeeded(data);
			container.UpdateObject(entity);
		}

		public void Delete<T>(T data)
		{
			if (data is INonPersistentBusinessObject nonPersist)
			{
				unlinkedObjects.AddRange(nonPersist.Unlink());
				return;
			}

			var entity = ConvertToRateOrConditionIfNeeded(data);
			container.DeleteObject(entity);
		}

		static string GetEntitySetName<T>()
		{
			var typeName = typeof(T).Name;
			if (typeName == nameof(RefCusRateWithoutApplicability))
			{
				typeName = nameof(RefCusRate);
			}
			else if (typeName == nameof(RefCusConditionWithoutApplicability))
			{
				typeName = nameof(RefCusCondition);
			}
			return typeName + "Update";
		}

		public IQueryable<T> GetCreatedBetween<T>(DateTimeOffset? afterCreatedTimeUTC, DateTimeOffset beforeOrEqualCreatedTimeUTC) where T : class
		{
			return ((DataServiceQuery<T>)Get<T>()).CreateFunctionQuery<T>("Default.GetCreatedBetween", false,
				new UriOperationParameter("afterCreatedTimeUTC", afterCreatedTimeUTC),
				new UriOperationParameter("beforeOrEqualCreatedTimeUTC", beforeOrEqualCreatedTimeUTC));
		}

		public IQueryable<T> GetWithOptimizedExpand<T>() where T : class
		{
			var result = ((DataServiceQuery<T>)Get<T>()).CreateFunctionQuery<T>("Default.GetWithOptimizedExpand", false);
			return result;
		}

		public async Task<DateTimeOffset> GetLastestCreatedTimeUTCAsync<T>() where T : class
		{
			return await ((DataServiceQuery<T>)Get<T>()).CreateFunctionQuerySingle<DateTimeOffset>("Default.GetLastestCreatedTimeUTC", false).GetValueAsync();
		}

		public async Task<DateTimeOffset> GetLatestUpdatedTimeUTCAsync(string nameOrPrefix)
		{
			Argument.Argument.NotNullOrEmpty(nameOrPrefix, nameof(nameOrPrefix));
			var type = GetType()
				.Assembly
				.ExportedTypes
				.FirstOrDefault(x => x.GetProperty($"{nameOrPrefix}_PK") != null || x.Name == nameOrPrefix);

			return type == null ? DateTimeOffset.MinValue : await (Task<DateTimeOffset>)GetType().GetMethod(nameof(GetLatestUpdatedTimeUTCCoreAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, new object[0]);
		}

		async Task<DateTimeOffset> GetLatestUpdatedTimeUTCCoreAsync<T>() where T : class
		{
			return await ((DataServiceQuery<T>)Get<T>()).CreateFunctionQuerySingle<DateTimeOffset>("Default.GetLatestUpdatedTimeUTC", false).GetValueAsync();
		}

		public async Task<bool> BatchExpire<T>(IEnumerable<Guid> ids, DateTimeOffset expiredTime) where T : class
		{
			var source = ((DataServiceQuery<T>)Get<T>());
			var result = await new DataServiceActionQuery(source.Context, source.AppendRequestUri("Default.BatchExpire"),
				new BodyOperationParameter("IDs", ids.ToList()), new BodyOperationParameter("expiredTime", expiredTime)).ExecuteAsync();
			return result.StatusCode == (int)HttpStatusCode.OK || result.StatusCode == (int)HttpStatusCode.NoContent;
		}

		public async Task<bool> BatchInActive<T>(IEnumerable<Guid> ids) where T : class
		{
			var source = ((DataServiceQuery<T>)Get<T>());
			var result = await new DataServiceActionQuery(source.Context, source.AppendRequestUri("Default.BatchInActive"),
				new BodyOperationParameter("IDs", ids.ToList())).ExecuteAsync();
			return result.StatusCode == (int)HttpStatusCode.OK || result.StatusCode == (int)HttpStatusCode.NoContent;
		}

		public async Task<bool> BatchDelete<T>(IEnumerable<Guid> ids) where T : class
		{
			var source = ((DataServiceQuery<T>)Get<T>());
			var result = await new DataServiceActionQuery(source.Context, source.AppendRequestUri("Default.BatchDelete"),
				new BodyOperationParameter("IDs", ids.ToList())).ExecuteAsync();
			return result.StatusCode == (int)HttpStatusCode.OK || result.StatusCode == (int)HttpStatusCode.NoContent;
		}

		public async Task<IEnumerable<CloneProcessResult>> CloneExistingRecordChildrenIntoNewRecord<T>(IEnumerable<CloneProcessObject> cloneProcessObjects) where T : class
		{
			var source = (DataServiceQuery<T>)Get<T>();
			var cloneProcessObjectsJson = JsonConvert.SerializeObject(cloneProcessObjects);
			var resultJson = await new DataServiceActionQuerySingle<string>(source.Context, source.AppendRequestUri("Default.CloneExistingRecordChildrenIntoNewRecord"),
				new BodyOperationParameter("cloneProcessObjectsJson", cloneProcessObjectsJson)).GetValueAsync();
			if (!string.IsNullOrEmpty(resultJson))
			{
				var cloneResults = JsonConvert.DeserializeObject<IEnumerable<CloneProcessResult>>(resultJson);
				return cloneResults;
			}
			return Enumerable.Empty<CloneProcessResult>();
		}

		public async Task<bool> ForceDelete<T>(IEnumerable<Guid> ids) where T : class
		{
			var source = (DataServiceQuery<T>)Get<T>();
			var result = await new DataServiceActionQuery(source.Context, source.AppendRequestUri("Default.ForceDelete"),
				new BodyOperationParameter("IDs", ids.ToList())).ExecuteAsync();

			return result.StatusCode == (int)HttpStatusCode.OK || result.StatusCode == (int)HttpStatusCode.NoContent;
		}

		static object ConvertToRateOrConditionIfNeeded(object data)
		{
			if (data is RefCusRateWithoutApplicability rateWithoutApplicability)
			{
				return rateWithoutApplicability.ConvertToRefCusRate();
			}
			if (data is RefCusConditionWithoutApplicability conditionWithoutApplicability)
			{
				return conditionWithoutApplicability.ConvertToRefCusCondition();
			}
			return data;
		}

		public void SavePersistentObjects()
		{
			var result = (new List<object>(), new List<object>(), new List<object>());
			var unlinkResult = new List<object>(unlinkedObjects).AsEnumerable();
			var addedOrUpdatedRateApps = addOrUpdateNonPersist.OfType<RefCusRateApplicability>();
			var addedOrUpdatedCondApps = addOrUpdateNonPersist.OfType<RefCusConditionApplicability>();

			var tariffResult_RateApp = GetPersistentObjects<RefCusTariff>(x => x.TariffPK,
				x => x.ZZ1_PK,
				x => x.RefCusRates,
				x => x.RefCusRateApplicabilities,
				addedOrUpdatedRateApps);
			result.Item1.AddRange(tariffResult_RateApp.AddedObjects);
			result.Item2.AddRange(tariffResult_RateApp.UpdatedObjects);
			unlinkResult = unlinkResult.Union(tariffResult_RateApp.UnlinedObjects);

			var nationalCodeResult = GetPersistentObjects<RefCusTariffNationalCode>(x => x.TariffNationalCodePK,
				x => x.ZZW_PK,
				x => x.RefCusRates,
				x => x.RefCusRateApplicabilities,
				addedOrUpdatedRateApps);
			result.Item1.AddRange(nationalCodeResult.AddedObjects);
			result.Item2.AddRange(nationalCodeResult.UpdatedObjects);
			unlinkResult = unlinkResult.Union(nationalCodeResult.UnlinedObjects);

			var tariffResult_CondApp = GetPersistentObjects<RefCusTariff>(x => x.TariffPK,
				x => x.ZZ1_PK,
				x => x.RefCusConditions,
				x => x.RefCusConditionApplicabilities,
				addedOrUpdatedCondApps);
			result.Item1.AddRange(tariffResult_CondApp.AddedObjects);
			result.Item2.AddRange(tariffResult_CondApp.UpdatedObjects);
			unlinkResult = unlinkResult.Union(tariffResult_CondApp.UnlinedObjects);

			var nomenclatureGroupResult_CondApp = GetPersistentObjects<RefCusNomenclatureGroup>(x => x.NomenclatureGroupPK,
				x => x.ZZ5_PK,
				x => x.RefCusConditions,
				x => x.RefCusConditionApplicabilities,
				addedOrUpdatedCondApps);
			result.Item1.AddRange(nomenclatureGroupResult_CondApp.AddedObjects);
			result.Item2.AddRange(nomenclatureGroupResult_CondApp.UpdatedObjects);
			unlinkResult = unlinkResult.Union(nomenclatureGroupResult_CondApp.UnlinedObjects);

			result.Item3.AddRange(NonPersistentObjectHelper.GetDeleteObjects(unlinkResult.Where(x => x != null).Distinct()));
			PopulatePersistentObjects(result.Item1, result.Item2, result.Item3);
		}

		(IEnumerable<object> AddedObjects, IEnumerable<object> UpdatedObjects, IEnumerable<object> UnlinedObjects) GetPersistentObjects<T>(
			Func<INonPersistentBusinessObjectFlatten, Guid?> getParentId,
			Func<T, Guid> getPk,
			Func<T, IEnumerable<INonPersistentBusinessObjectParent>> getParents,
			Func<T, IEnumerable<INonPersistentBusinessObjectFlatten>> getFlattenObjects,
			IEnumerable<INonPersistentBusinessObjectFlatten> changedFlattenObjects) where T : class
		{
			var result = (new List<object>(), new List<object>(), new List<object>());

			var parents = allFlattenObjects
						.Where(x => getParentId(x) != null)
						.Join(container.EntityStates.Select(x => x.Item1), x => getParentId(x), y => y.GetPKValue(), (x, y) => y as T)
						.Where(x => x != null).Distinct();

			foreach (var parent in parents)
			{
				var pResult = NonPersistentObjectHelper.Merge(getParents(parent), getFlattenObjects(parent), changedFlattenObjects.Where(x => getParentId(x) == getPk(parent)), NonPersistentObjecComparison);
				result.Item1.AddRange(pResult.AddedObjects);
				result.Item2.AddRange(pResult.UpdatedObjects);
				result.Item3.AddRange(pResult.UnlinkedObjects);
			}
			return result;
		}

		void PopulatePersistentObjects(IEnumerable<object> addedObjects, IEnumerable<object> updatedObjects, IEnumerable<object> deletedObjects)
		{
			Argument.Argument.NotNull(addedObjects, nameof(addedObjects));
			Argument.Argument.NotNull(updatedObjects, nameof(updatedObjects));
			Argument.Argument.NotNull(deletedObjects, nameof(deletedObjects));

			var intersec = addedObjects.Intersect(deletedObjects);

			foreach (var item in addedObjects.Except(intersec))
			{
				container.AddObject(item.GetType().Name + "Update", item);
			}
			foreach (var item in updatedObjects)
			{
				Update(item);
			}
			foreach (var item in deletedObjects.Except(intersec))
			{
				Delete(item);
			}
		}

		public void TrackNonPersistentFlattenObject(INonPersistentBusinessObjectFlatten flattenObject)
		{
			allFlattenObjects.Add(flattenObject);
		}

		public IEnumerable<object> GetAllPersistentObjects() => allFlattenObjects.SelectMany(x => x.LinkedObjects).Where(x => x != null).Distinct();

		public INonPersistentBusinessObjectComparison NonPersistentObjecComparison { get; set; } = new NonPersistentBusinessObjectComparison();

		readonly IContainer container;
	}
}
