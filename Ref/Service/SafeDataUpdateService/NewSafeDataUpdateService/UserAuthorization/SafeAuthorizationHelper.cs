using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class SafeAuthorizationHelper : AuthorizationHelper
	{
		public SafeAuthorizationHelper(IReferenceDataRepository repository)
		{
			Argument.NotNull(repository, nameof(repository));
			this.repository = repository;
		}

		readonly IReferenceDataRepository repository;

		protected override IEnumerable<RefUserAuthorization> GetAuthorizationData(string user, Type type)
		{
			var userAuthorizations = repository.Get<UserAuthorization>().Where(x => EF.Functions.Collate(x.UA_User, "SQL_Latin1_General_CP1_CI_AS") == user
			&& (EF.Functions.Collate(x.UA_TableName, "SQL_Latin1_General_CP1_CI_AS") == type.Name
			|| EF.Functions.Collate(x.UA_TableName, "SQL_Latin1_General_CP1_CI_AS") == "*")).ToArray();
			return userAuthorizations.Select(u => new RefUserAuthorization(u.UA_User, u.UA_DataSetName, u.UA_TableName, u.UA_ColumnName, u.UA_ColumnValue));
		}
	}
}
