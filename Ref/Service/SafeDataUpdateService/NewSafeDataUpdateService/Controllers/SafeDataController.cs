using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	[IsEditableActionFilter]
	public class SafeDataController<T> : ODataController, IUserAuthorizationHelperProvider where T : class
	{
		public SafeDataController(IAuthorizationHelper userAuthorizationHelper)
		{
			Argument.NotNull(userAuthorizationHelper, nameof(userAuthorizationHelper));

			this.userAuthorizationHelper = userAuthorizationHelper;
		}
		readonly IAuthorizationHelper userAuthorizationHelper;

		public IAuthorizationHelper UserAuthorizationHelper => userAuthorizationHelper;

		Tuple<IReferenceDataRepository, bool> SharedRepository
		{
			get
			{
				if (fSharedRepository == null)
				{
					fSharedRepository = HttpContext.GetContext();
				}
				return fSharedRepository;
			}
		}
		Tuple<IReferenceDataRepository, bool> fSharedRepository;

		protected IReferenceDataRepository Repository
		{
			get
			{
				return SharedRepository.Item1;
			}
		}

		protected virtual IQueryable<T> GetCore()
		{
			return Repository.Get<T>();
		}

		[HttpGet]
		[SystemVersion]
		[ODataEnableQuery(MaxExpansionDepth = 10)]
		public virtual IEnumerable<T> Get()
		{
			return GetCore();
		}

		[HttpGet]
		[SystemVersion]
		public virtual IEnumerable<T> GetWithOptimizedExpand(ODataQueryOptions<T> queryOptions)
		{
			return GetWithOptimizedExpand(GetCore(), queryOptions);
		}

		[HttpGet]
		[ODataEnableQuery(MaxExpansionDepth = 10)]
		public virtual IEnumerable<T> Get([FromODataUri] Guid key)
		{
			var param = Expression.Parameter(typeof(T));
			var propertyExp = Expression.Property(param, PKColumn);
			var equalExp = Expression.Equal(propertyExp, Expression.Constant(key, typeof(Guid)));
			var whereExp = Expression.Lambda<Func<T, bool>>(equalExp, param);
			return GetCore().Where(whereExp);
		}

		IEnumerable<T> GetWithOptimizedExpand(IQueryable<T> query, ODataQueryOptions<T> queryOptions)
		{
			var dataQuery = queryOptions.ApplyTo(query, AllowedQueryOptions.Expand | AllowedQueryOptions.Select)?.Cast<T>();
			var results = dataQuery.ToList();

			var expandClause = queryOptions.SelectExpand?.SelectExpandClause;
			if (expandClause != null && results != null)
			{
				Repository.Expand(results, new ExpandClauseWrapper(expandClause));
			}

			var oDataProperties = Request.ODataFeature();
			if (oDataProperties != null && expandClause != null)
			{
				oDataProperties.SelectExpandClause = expandClause;
			}
			return results;
		}

		protected PropertyInfo PKColumn
		{
			get
			{
				if (fPKColumn == null)
				{
					fPKColumn = typeof(T).GetPKPropertyInfo();
				}
				return fPKColumn;
			}
		}
		PropertyInfo fPKColumn;

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> Post([FromBody] T data)
		{
			if ((Guid)PKColumn.GetValue(data) == Guid.Empty)
			{
				PKColumn.SetValue(data, Guid.NewGuid());
			}
			if (!IsAuthorized(data))
			{
				return Unauthorized();
			}
			Repository.Add(data);
			await SaveChangesAysncIfNotSharedAsync();
			return Created<T>(data);
		}

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] T data)
		{
			if (!IsAuthorized(data))
			{
				return Unauthorized();
			}
			Repository.Update(data);
			await SaveChangesAysncIfNotSharedAsync();
			return Updated(data);
		}

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> Patch([FromODataUri] Guid key, [FromBody] T data)
		{
			if (!IsAuthorized(data))
			{
				return Unauthorized();
			}
			Repository.Update(data);
			await SaveChangesAysncIfNotSharedAsync();
			return Updated(data);
		}

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> Delete([FromODataUri] Guid key)
		{
			var data = Get(key).FirstOrDefault();

			if (!IsAuthorized(data))
			{
				return Unauthorized();
			}
			if (!IsDataHeader)
			{
				Repository.Delete(data);
			}
			else
			{
				MarkAsDeleted(key);
			}
			await SaveChangesAysncIfNotSharedAsync();
			return NoContent();
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> ForceDelete(ODataActionParameters parameters)
		{
			if (!UserAuthorizationHelper.IsAuthorized<T>(HttpContext.GetUserId()))
			{
				return Unauthorized();
			}
			var ids = parameters["IDs"] as IEnumerable<Guid>;
			if (ids != null)
			{
				var tblPrefix = typeof(T).GetTablePrefix();
				var pkProperty = typeof(T).GetPKPropertyInfo();
				var pkExp = ExpressionHelper.GetPKExpression<T>();
				var containsPKExpression = ExpressionHelper.ContainsStructPropertyExpression<T, Guid>(ids.ToList(), pkProperty);
				var data = Repository.Get<T>().Where(containsPKExpression);
				if (IsDataHeader)
				{
					var versionControlData = Repository.Get<RefDbVersionControl>().Where(x => ids.Contains(x.RVC_ParentPK));
					await versionControlData.DeleteAsync();
				}
				Console.WriteLine("Deleting {0}: {1}", typeof(T).Name, string.Join(", ", ids));
				await data.DeleteAsync();
				await SaveChangesAysncIfNotSharedAsync();
			}
			return NoContent();
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> BatchExpire(ODataActionParameters parameters)
		{
			if (!UserAuthorizationHelper.IsAuthorized<T>(HttpContext.GetUserId()))
			{
				return Unauthorized();
			}
			var data = GetApplicableData(parameters);
			if (!data.Any())
			{
				return NoContent();
			}

			var pkExp = ExpressionHelper.GetPKExpression<T>();
			var expiredTime = ((DateTimeOffset)parameters["expiredTime"]).DateTime;
			var tblPrefix = typeof(T).GetTablePrefix();
			var startDateProperty = typeof(T).GetProperty(tblPrefix + StartDateSuffix);
			var endDateProperty = typeof(T).GetProperty(tblPrefix + EndDateSuffix);
			if (!IsDataHeader)
			{
				var filterStartDateInFuture = ExpressionHelper.GetPropertyFiltersExpression<T>(startDateProperty, [expiredTime], Operations.GreaterThanOrEqual);
				var dataInFuture = data.Where(filterStartDateInFuture);
				if (dataInFuture.Any())
				{
					var pksToRecursiveDelete = dataInFuture.Select(pkExp).ToArray();
					Console.WriteLine("RecursiveDeleting {0}: {1}", typeof(T).Name, string.Join(", ", pksToRecursiveDelete));
					await Repository.RecursiveDeleteAsync(dataInFuture);
				}
			}
			var filterEndDate = ExpressionHelper.GetPropertyFiltersExpression<T>(endDateProperty, [expiredTime], Operations.GreaterThanOrEqual);
			var filterStartDate = ExpressionHelper.GetPropertyFiltersExpression<T>(startDateProperty, [expiredTime], Operations.LessThan);
			data = data.Where(filterStartDate);
			data = data.Where(filterEndDate);
			var newExp = Expression.New(typeof(T));
			var endDateBindExp = Expression.Bind(endDateProperty, Expression.Constant(expiredTime, typeof(DateTime)));
			var initExp = Expression.MemberInit(newExp, endDateBindExp);
			var updateExp = (Expression<Func<T, T>>)Expression.Lambda(initExp, Expression.Parameter(typeof(T)));
			await data.UpdateAsync(updateExp);

			return NoContent();
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> BatchInActive(ODataActionParameters parameters)
		{
			if (!UserAuthorizationHelper.IsAuthorized<T>(HttpContext.GetUserId()))
			{
				return Unauthorized();
			}
			var data = GetApplicableData(parameters);
			if (!data.Any())
			{
				return NoContent();
			}

			var tblPrefix = typeof(T).GetTablePrefix();
			var isActiveProperty = typeof(T).GetProperty(tblPrefix + IsActiveSuffix);
			var filterIsActive = ExpressionHelper.GetPropertyFiltersExpression<T>(isActiveProperty, [true], Operations.Equals);
			data = data.Where(filterIsActive);
			var newExp = Expression.New(typeof(T));
			var isActiveBindExp = Expression.Bind(isActiveProperty, Expression.Constant(false, typeof(bool)));
			var initExp = Expression.MemberInit(newExp, isActiveBindExp);
			var updateExp = (Expression<Func<T, T>>)Expression.Lambda(initExp, Expression.Parameter(typeof(T)));
			await data.UpdateAsync(updateExp);

			return NoContent();
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<IActionResult> BatchDelete(ODataActionParameters parameters)
		{
			if (!UserAuthorizationHelper.IsAuthorized<T>(HttpContext.GetUserId()))
			{
				return Unauthorized();
			}

			if (!IsDataHeader)
			{
				var data = GetApplicableData(parameters);
				await data.DeleteAsync();
			}
			else
			{
				var ids = parameters["IDs"] as IEnumerable<Guid>;
				if (ids != null && ids.Any())
				{
					foreach (var pk in ids)
					{
						MarkAsDeleted(pk);
					}
					await SaveChangesAysncIfNotSharedAsync();
				}
			}

			return NoContent();
		}

		IQueryable<T> GetApplicableData(ODataActionParameters parameters)
		{
			var ids = parameters["IDs"] as IEnumerable<Guid>;
			if (ids == null)
			{
				return Enumerable.Empty<T>().AsQueryable();
			}
			var existingSafeData = GetExistingSafeData(ids);
			var data = GetUserOverrideDisabledData(existingSafeData);
			return data;
		}

		IQueryable<T> GetExistingSafeData(IEnumerable<Guid> ids)
		{
			var data = Repository.Get<T>();
			var pkExp = ExpressionHelper.GetPKExpression<T>();
			if (IsDataHeader)
			{
				data = data.Join(Repository.Get<RefDbVersionControl>().Where(x => !x.RVC_Deleted),
					pkExp, x => x.RVC_ParentPK, (r, _) => r);
			}

			var pkProperty = typeof(T).GetPKPropertyInfo();
			var containsPKExpression = ExpressionHelper.ContainsStructPropertyExpression<T, Guid>(ids.ToList(), pkProperty);
			return data.Where(containsPKExpression);
		}

		static IQueryable<T> GetUserOverrideDisabledData(IQueryable<T> data)
		{
			var tblPrefix = typeof(T).GetTablePrefix();
			var userOverrideProperty = typeof(T).GetProperty(tblPrefix + UserOverrideSuffix);
			if (userOverrideProperty == null)
			{
				return data;
			}
			var filterUserOverride = ExpressionHelper.GetPropertyFiltersExpression<T>(userOverrideProperty, [false], Operations.Equals);
			return data.Where(filterUserOverride);
		}

		[HttpPost]
		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public string CloneExistingRecordChildrenIntoNewRecord(ODataActionParameters parameters)
		{
			if (!UserAuthorizationHelper.IsAuthorized<T>(HttpContext.GetUserId()))
			{
				throw new UnauthorizedAccessException();
			}

			var cloneProcessObjects = JsonConvert.DeserializeObject<IEnumerable<CloneProcessObject>>(parameters["cloneProcessObjectsJson"].ToString());

			var parentPkProperty = typeof(T).GetPKPropertyInfo();
			var allRecordsPKsToLookup = cloneProcessObjects.Select(x => x.NewRecordPk).Concat(cloneProcessObjects.Select(x => x.ExpiredRecordPk));
			var allRecordsContainsPKExpression = ExpressionHelper.ContainsStructPropertyExpression<T, Guid>(allRecordsPKsToLookup.ToList(), parentPkProperty);
			var genericCollectionDictionary = new Dictionary<Type, List<Type>>();
			var allRecordsFromDb = Repository.GetWithExpand<T>(genericCollectionDictionary).Where(allRecordsContainsPKExpression).ToArray();

			var result = new List<CloneProcessResult>();
			var cloneHelper = new CloneHelper();
			foreach (var cloneProcessObject in cloneProcessObjects)
			{
				var existingRecordPk = cloneProcessObject.ExpiredRecordPk;
				var newRecordPk = cloneProcessObject.NewRecordPk;
				var existingRecordPKExpression = ExpressionHelper.GetPropertyFiltersExpression<T>(parentPkProperty, new object[] { existingRecordPk }, Operations.Equals);
				var existingRecord = allRecordsFromDb.AsQueryable().FirstOrDefault(existingRecordPKExpression);

				var newRecordPKExpression = ExpressionHelper.GetPropertyFiltersExpression<T>(parentPkProperty, new object[] { newRecordPk }, Operations.Equals);
				var newRecord = allRecordsFromDb.AsQueryable().FirstOrDefault(newRecordPKExpression);
				var cloneResult = cloneHelper.Clone(existingRecord, newRecord, cloneProcessObject, genericCollectionDictionary);
				if (cloneResult.Any())
				{
					result.AddRange(cloneResult);
				}
			}

			var resultsJson = JsonConvert.SerializeObject(result);
			return resultsJson;
		}

		void MarkAsDeleted(Guid parentPK)
		{
			var versionControl = Repository.Get<RefDbVersionControl>()?.FirstOrDefault(x => x.RVC_ParentPK == parentPK);
			if (versionControl != null)
			{
				versionControl.RVC_Deleted = true;
				versionControl.RVC_IsPublished = false;
				Repository.Update(versionControl);
			}
		}

		protected virtual bool IsDataHeader => false;

		bool IsAuthorized(T data)
		{
			return UserAuthorizationHelper.IsAuthorized(data, HttpContext.GetUserId());
		}

		async Task<int> SaveChangesAysncIfNotSharedAsync()
		{
			var result = 0;
			if (!SharedRepository.Item2)
			{
				result = await Repository.SaveChangesAsync(HttpContext.GetUserId());
			}
			return result;
		}

		const string StartDateSuffix = "_StartDate";
		const string EndDateSuffix = "_EndDate";
		const string IsActiveSuffix = "_IsActive";
		const string UserOverrideSuffix = "_UserOverride";
	}
}
