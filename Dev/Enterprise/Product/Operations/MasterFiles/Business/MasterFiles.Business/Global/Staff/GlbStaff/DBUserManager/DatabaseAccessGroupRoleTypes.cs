using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class DatabaseAccessGroupRoleTypes
	{
		public static IReadOnlyDictionary<string, string> AllDbRolesWithDescriptions => new Dictionary<string, string>
		{
			{ "db_datawriter", (NoResString)"Create views in a user repository when customizing reports" },
			{ "cwRestrictedReaderRole", (NoResString)"Query tables and view in the main, eDocs and Reference databases" },
			{ "db_backupoperator", (NoResString)"Allow backing up the database through direct SQL" },
			{ "cwHRMStaffRole", (NoResString)"Grant data layer access to staff remuneration, classification and review for reporting purposes" },
		};
	}
}
