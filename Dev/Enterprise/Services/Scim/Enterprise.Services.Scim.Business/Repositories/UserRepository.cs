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
	public class UserRepository : BaseRepository<GlbStaff>, IPersistanceRepository<ScimUser>
	{
		public UserRepository(ISCIMSchemaQueryRepository scimSchemaQueryRepository) : base(scimSchemaQueryRepository)
		{
		}

		#region Create

		public Task<ScimUser> CreateSCIMResource(ScimUser scimUser)
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.UpdateFromScim(scimUser, CodeCalculator);

			if (staff.IsInDatabase)
			{
				scimUser.Id = staff.PK.ToGuid();
			}
			else
			{
				if (CodeCalculator.AvailableStaffCodes.Count == 0)
				{
					throw new SCIMNoTargetException("No more staff codes left");
				}
				else
				{
					throw new SCIMNoTargetException("Couldn't save a staff record");
				}
			}

			return Task.FromResult(scimUser);
		}

		#endregion

		#region Delete

		public Task DeleteSCIMResourceById(string id)
		{
			var staff = GetBizOFromId(id, false);

			if (staff != null)
			{
				staff.GS_IsActive = false;
				staff.Factory.Save();
			}

			return Task.CompletedTask;
		}

		#endregion

		#region Find

		public Task<SearchScimResponse<ScimUser>> FindSCIMResource(SearchSCIMRepresentationsParameter searchParameter)
		{
			var results = Find(searchParameter.Filter, searchParameter.StartIndex, searchParameter.Count);

			var response = new SearchScimResponse<ScimUser>(results.total, results.Item1.Select(s => (ScimUser)s.ToScim(searchParameter.IncludedAttributes, searchParameter.ExcludedAttributes)));
			return Task.FromResult(response);
		}

		public Task<ScimUser> FindSCIMByResourceId(string id)
		{
			var staff = GetBizOFromId(id, false);
			if (staff == null || staff.GS_IsSystemAccount)
			{
				return Task.FromResult<ScimUser>(null);
			}

			return Task.FromResult((ScimUser)staff.ToScim());
		}

		#endregion

		#region Patch

		public Task<ScimUser> PatchSCIMResourceById(string id, PatchRepresentationParameter representationParameter)
		{
			return Task.FromResult((ScimUser)ApplyPatches(id, representationParameter));
		}

		#endregion

		#region Update

		public Task<ScimUser> UpdateSCIMResourceById(string id, ScimUser scimUser)
		{
			return Task.FromResult((ScimUser)Update(id, scimUser));
		}

		#endregion

		#region Common

		readonly FastStaffCodeCalculator codeCalculator = new FastStaffCodeCalculator(new BusinessObjectFactory());

		FastStaffCodeCalculator CodeCalculator
		{
			get => codeCalculator;
		}

		protected override string OrderByColumn => GlbStaffSchema.PK.Name;

		protected override string TableName => GlbStaffSchema.Constants.TableName;

		protected override ZQuery GetIsActiveOrRecentlyEditedQuery(string id = "")
		{
			var query = new ZQuery(GlbStaffSchema.GS_IsActive, true);
			query.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(-30));

			if (!string.IsNullOrEmpty(id))
			{
				query.AddToFilter(GlbStaffSchema.PK, GetPkFromId(id));
			}

			return query;
		}

		#endregion
	}
}
