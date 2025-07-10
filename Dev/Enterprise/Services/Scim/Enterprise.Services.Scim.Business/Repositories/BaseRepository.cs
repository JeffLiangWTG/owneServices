using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Parser.Expressions;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Business
{
	public abstract class BaseRepository<T> where T : BusinessObject
	{
		readonly ISCIMSchemaQueryRepository scimSchemaQueryRepository;
		const int MaxRows = 200;

		protected BaseRepository(ISCIMSchemaQueryRepository scimSchemaQueryRepository)
		{
			this.scimSchemaQueryRepository = scimSchemaQueryRepository;
		}

		protected ScimBase ApplyPatches(string id, PatchRepresentationParameter representationParameter)
		{
			var bizO = GetBizOFromId(id, true);
			if (bizO == null)
			{
				return null;
			}

			var options = new SCIMHostOptions() { IgnoreUnsupportedCanonicalValues = true };
			var helper = new SCIMRepresentationHelper(options);
			var converter = new ScimToScimRepresentation(scimSchemaQueryRepository, helper);

			var patchHandler = new PatchRepresentationHandler(converter);
			var result = patchHandler.Handle(bizO.ToScim(), representationParameter).GetAwaiter().GetResult();

			if (!result.IsPatched)
			{
				return null;
			}

			var scim = converter.RepresentationToScim(result.SCIMRepresentation)
				?? throw new SCIMNoTargetException("Cannot convert representation to scim");
			scim.Id = bizO.PK.ToGuid();

			bizO.UpdateFromScim(scim, null);

			bizO.TrySave();

			return bizO.ToScim();
		}

		protected ScimBase Update(string id, ScimBase scim)
		{
			var bizO = GetBizOFromId(id, true);

			if (bizO == null)
			{
				return null;
			}

			bizO.UpdateFromScim(scim);

			bizO.TrySave();
			scim.Id = bizO.PK.ToGuid();

			return scim;
		}

		protected (IEnumerable<T>, int total) Find(SCIMExpression filter, int startIndex, int count)
		{
			var factory = new BusinessObjectFactory();
			var query = FilterToZQueryConverter.ConvertToZQuery<T>(filter);
			query.AddToFilter(GetIsActiveOrRecentlyEditedQuery());

			var total = factory.GetDatabaseCount(typeof(T), query);

			query.OrderBy = OrderByColumn;
			if (count > MaxRows)
			{
				count = MaxRows;
			}
			if (startIndex < 1)
			{
				startIndex = 1;
			}

			var sqlAndParams = query.GetAsCompleteSQLStatementWithParameters(TableName);
			var sql = sqlAndParams.sql;

			var result = new List<T>();
			if (count > 0) // if count is 0, return total only
			{
				sql += @$"
OFFSET {startIndex - 1} ROWS
FETCH NEXT {count} ROWS ONLY";

				var queue = new DbOnlyBusinessObjectQueue<T>(new ZNonPersistentDataQuery(sql, sqlAndParams.parameters));

				queue.ProcessBatch((records, e) =>
				{
					foreach (var obj in records)
					{
						result.Add(obj);
					}
				}, -1, CancellationToken.None);
			}

			return (result, total);
		}

		protected abstract ZQuery GetIsActiveOrRecentlyEditedQuery(string id = "");

		protected abstract string OrderByColumn { get; }
		protected abstract string TableName { get; }

		protected T GetBizOFromId(string id, bool throwIfNotFound)
		{
			var query = GetIsActiveOrRecentlyEditedQuery(id);

			var bizO = new BusinessObjectFactory().LoadTop1<T>(query);
			if (bizO == null && throwIfNotFound)
			{
				throw new SCIMNotFoundException($"{typeof(T).Name} with PK [{id}] was not found");
			}

			return bizO;
		}

		protected ZGuid GetPkFromId(string id)
		{
			var pk = ZGuid.Empty;

			var failedCoversion = false;

			try
			{
				pk = new ZGuid(id);
			}
			catch
			{
				failedCoversion = true;
			}

			if (failedCoversion || !pk.IsValid || pk.IsEmpty)
			{
				throw new SCIMAttributeException($"Bad id: {id}");
			}

			return pk;
		}

		readonly FilterToZQueryConverter filterToZQueryConverter = new FilterToZQueryConverter();
		protected FilterToZQueryConverter FilterToZQueryConverter { get => filterToZQueryConverter; }
	}
}
