using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Schema;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Business
{
	public class GroupRepository : BaseRepository<GlbGroup>, IPersistanceRepository<ScimGroup>
	{
		public GroupRepository(ISCIMSchemaQueryRepository scimSchemaQueryRepository) : base(scimSchemaQueryRepository)
		{
		}

		#region Create

		public Task<ScimGroup> CreateSCIMResource(ScimGroup scimGroup)
		{
			var factory = new BusinessObjectFactory();
			var group = factory.New<GlbGroup>();
			group.UpdateFromScim(scimGroup);

			if (group.IsInDatabase)
			{
				scimGroup.Id = group.PK.ToGuid();
			}
			else
			{
				throw new SCIMNoTargetException("Couldn't save a group record");
			}

			return Task.FromResult(scimGroup);
		}

		#endregion

		#region Delete

		public Task DeleteSCIMResourceById(string id)
		{
			var group = GetBizOFromId(id, false);

			if (group != null)
			{
				group.RemoveStaffFlagsIfRequired();
				group.Delete();
				group.Factory.Save();
			}

			return Task.CompletedTask;
		}

		#endregion

		#region Find

		public Task<SearchScimResponse<ScimGroup>> FindSCIMResource(SearchSCIMRepresentationsParameter searchParameter)
		{
			var groups = Find(searchParameter.Filter, searchParameter.StartIndex, searchParameter.Count);

			var response = new SearchScimResponse<ScimGroup>(groups.total, groups.Item1.Select(g => (ScimGroup)g.ToScim(searchParameter.IncludedAttributes, searchParameter.ExcludedAttributes)));
			return Task.FromResult(response);
		}

		public Task<ScimGroup> FindSCIMByResourceId(string id)
		{
			var group = GetBizOFromId(id, false);

			if (group == null || group.GG_IsSystemDefined)
			{
				return Task.FromResult<ScimGroup>(null);
			}

			var result = group.ToScim();
			return Task.FromResult((ScimGroup)result);
		}

		#endregion

		#region Patch

		public Task<ScimGroup> PatchSCIMResourceById(string id, PatchRepresentationParameter representationParameter)
		{
			return Task.FromResult((ScimGroup)ApplyPatches(id, representationParameter));
		}

		#endregion

		#region Update

		public Task<ScimGroup> UpdateSCIMResourceById(string id, ScimGroup scimGroup)
		{
			return Task.FromResult((ScimGroup)Update(id, scimGroup));
		}

		#endregion

		#region Common

		protected override string OrderByColumn => GlbGroupSchema.PK.Name;

		protected override string TableName => GlbGroupSchema.Constants.TableName;

		protected override ZQuery GetIsActiveOrRecentlyEditedQuery(string id = "")
		{
			var query = new ZQuery(GlbGroupSchema.GG_IsActive, true);
			query.AddToFilter(JoinCondition.Or, GlbGroupSchema.GG_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(-30));

			if (!string.IsNullOrEmpty(id))
			{
				query.AddToFilter(GlbGroupSchema.PK, GetPkFromId(id));
			}

			return query;
		}

		#endregion
	}
}
