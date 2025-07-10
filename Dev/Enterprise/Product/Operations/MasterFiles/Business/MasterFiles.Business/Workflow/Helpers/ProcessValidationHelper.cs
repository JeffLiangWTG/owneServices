using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class ProcessValidationHelper
	{
		#region Empty Groups/Capabilities

		public static bool IsGroupEmpty_WithoutLoadingGlbStaffRecords(BusinessObjectFactory factory, ZGuid groupPk)
		{
			return IsContainerObjectEmpty_WithoutLoadingGlbStaffRecords<GlbGroupLink>(factory, groupPk, GlbGroupLinkSchema.GK_GG);
		}

		public static bool IsCapabilityEmpty_WithoutLoadingGlbStaffRecords(BusinessObjectFactory factory, ZGuid capabilityPk)
		{
			return IsContainerObjectEmpty_WithoutLoadingGlbStaffRecords<GlbResourceCapabilityPivot>(factory, capabilityPk, GlbResourceCapabilityPivotSchema.G5_G4_Capability);
		}

		static bool IsContainerObjectEmpty_WithoutLoadingGlbStaffRecords<T>(BusinessObjectFactory factory, ZGuid pk, SchemaGuidColumn pivotForeignKeyColumn)
			where T : BusinessObject
		{
			var query = new ZQuery(pivotForeignKeyColumn, pk);
			return !factory.Exists(typeof(T), query);
		}

		#endregion

		#region Capability/Group Intersection

		public static bool DoesCapabilityDisallowTaskToBeAssignedToResource(ProcessTask task)
		{
			return DoesIntersectionOfCapabilityTaskGroupAndReleaseGroupDisallowTaskToBeAssignedToResource(task);
		}

		public static bool DoesTaskGroupDisallowTaskToBeAutoAssignedToResource(ProcessTask task)
		{
			return task.P9_GG_AssignedGroup.IsValid && DoesIntersectionOfCapabilityTaskGroupAndReleaseGroupDisallowTaskToBeAssignedToResource(task);
		}

		public static bool DoesWorkflowReleaseGroupDisallowTaskToBeAutoAssignedToResource(ProcessTask task)
		{
			return !task.P9_GG_AssignedGroup.IsValid && (task.ProcessHeader?.FH_GG_ReleaseGroup.IsValid ?? false) && DoesIntersectionOfCapabilityTaskGroupAndReleaseGroupDisallowTaskToBeAssignedToResource(task);
		}

		static bool DoesIntersectionOfCapabilityTaskGroupAndReleaseGroupDisallowTaskToBeAssignedToResource(ProcessTask task)
		{
			if (!string.IsNullOrEmpty(task.P9_GS_NKAssignedStaffMember) || IsTaskAssignedToCapabilityWithGlobalScope(task))
			{
				return false;
			}

			var groupPk = task.P9_GG_AssignedGroup.IsValid ? task.P9_GG_AssignedGroup : (task.ProcessHeader?.FH_GG_ReleaseGroup ?? ZGuid.Empty);
			var capabilityPk = task.P9_G4_RequiredCapability;

			if (!groupPk.IsValid || !capabilityPk.IsValid)
			{
				return false;
			}

			return !DoesIntersectionOfCapabilityAndGroupHaveMembers(task.Factory, groupPk, capabilityPk);
		}

		static bool DoesIntersectionOfCapabilityAndGroupHaveMembers(BusinessObjectFactory factory, ZGuid groupPk, ZGuid capabilityPk)
		{
			var cacheKey = FormattableString.Invariant($"GroupCapabilityIntersection_{groupPk}_{capabilityPk}");
			return factory.GetCachedValue(cacheKey, () => DoesIntersectionOfCapabilityAndGroupHaveMembersInDatabase(groupPk, capabilityPk));
		}

		static bool DoesIntersectionOfCapabilityAndGroupHaveMembersInDatabase(ZGuid groupPk, ZGuid capabilityPk)
		{
			const string sql = @"
SELECT	TOP 1 1
FROM	dbo.GlbGroupLink
JOIN	dbo.GlbResourceCapabilityPivot ON GK_GS = G5_GS_Resource
WHERE	1 = 1
AND		GK_GG = @groupPk
AND		G5_G4_Capability = @capabilityPk
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("groupPk", SqlDbType.UniqueIdentifier, groupPk.ToGuid());
				command.AddParameter("capabilityPk", SqlDbType.UniqueIdentifier, capabilityPk.ToGuid());

				var result = command.ExecuteScalar();

				return result != null && result != DBNull.Value && (int)result == 1;
			}
		}

		#endregion

		#region Task Capability Assignment

		public static bool IsTaskAssignedToCapabilityWithGroupScope(ProcessTask task)
		{
			return IsTaskAssignedToCapabilityWithSpecifiedScope(task, GlbCapabilityScopeList.Codes.GroupScope);
		}

		public static bool IsTaskAssignedToCapabilityWithGlobalScope(ProcessTask task)
		{
			return IsTaskAssignedToCapabilityWithSpecifiedScope(task, GlbCapabilityScopeList.Codes.GlobalScope);
		}

		static bool IsTaskAssignedToCapabilityWithSpecifiedScope(ProcessTask task, string scope)
		{
			var capability = task.RequiredCapability;

			return capability != null && capability.G4_CapacityScope == scope;
		}

		#endregion
	}
}
