using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.Web.Auth;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public class StagingAuthorizationHelper : AuthorizationHelper
	{
		public StagingAuthorizationHelper(ISafeRepository safeRepository)
		{
			Argument.NotNull(safeRepository, nameof(safeRepository));
			this.safeRepository = safeRepository;
		}

		readonly ISafeRepository safeRepository;

		protected override IEnumerable<RefUserAuthorization> GetAuthorizationData(string user, Type type)
		{
			var userAuthorizations = safeRepository.GetLatest<UserAuthorization>().Where(x => x.UA_User.Equals(user, StringComparison.OrdinalIgnoreCase)
				&& (type.Name.Equals(x.UA_TableName, StringComparison.OrdinalIgnoreCase)
					|| "*".Equals(x.UA_TableName, StringComparison.OrdinalIgnoreCase))).ToArray();
			return userAuthorizations.Select(u => new RefUserAuthorization(u.UA_User, u.UA_DataSetName, u.UA_TableName, u.UA_ColumnName, u.UA_ColumnValue));
		}
	}
}
