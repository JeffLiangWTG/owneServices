using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Module.General
{
	public static class GlbStaffGroupHelper
	{
#if DEBUG
		[ThreadSafe]
		static internal BusinessObjectFactory securityFactory_ExposedForTest;
#endif

		public static string[] KeysForSecurityRightFilters(List<StaffSecurityModuleFilter> securityRightFilters)
		{
			var result = new HashSet<string>();
			foreach (var filter in securityRightFilters)
			{
				var lookupKey = filter.SecurityFilterContainer.LookupKey;
				ISecurityCheckpoint checkpoint = Env.Security.FindCheckPoint(lookupKey);
				var checkpointExists = checkpoint != null;

				ReloadSecurityCheckpoint(lookupKey, Env.Security, ref checkpointExists, ref checkpoint);
				while (checkpoint != null)
				{
					result.Add(checkpoint.Code);
					checkpoint = checkpoint.Parent;
				}
			}
			return result.ToArray();
		}

		public static void LoadCollectionWithCategory(IList<BusinessObject> collection, List<StaffSecurityModuleFilter> securityRightFilters, GlbSecurityCollection allGlbSecurity, HashSet<ZGuid> deniedItems)
		{
			try
			{
				foreach (var eachGroup in securityRightFilters.GroupBy(f => f.OrCategory))
				{
					if (eachGroup.Key == FilterOrCategory.None)
					{
						foreach (var securityModuleFilter in eachGroup)
						{
							PrepareGroupSecurity(collection, allGlbSecurity, securityModuleFilter, out var lookupKey, out var securityCore);
							AddDenied(collection, lookupKey, securityCore, deniedItems);
						}
					}
					else
					{
						var deniedForOrCategory = new List<ZGuid>();
						foreach (var securityModuleFilter in eachGroup)
						{
							var deniedItemsForThisFilter = new HashSet<ZGuid>();
							PrepareGroupSecurity(collection, allGlbSecurity, securityModuleFilter, out var lookupKey, out var securityCore);
							AddDenied(collection, lookupKey, securityCore, deniedItemsForThisFilter);

							if (eachGroup.First().Equals(securityModuleFilter) && deniedForOrCategory.Count == 0)
							{
								deniedForOrCategory.AddRange(deniedItemsForThisFilter);
							}
							else
							{
								deniedForOrCategory = deniedForOrCategory.Intersect(deniedItemsForThisFilter).ToList();
							}
						}
						deniedItems.UnionWith(deniedForOrCategory);
					}
				}
			}
			finally
			{
				User.DisposeFactory();
			}
		}

		static void PrepareGroupSecurity(IList<BusinessObject> collection, GlbSecurityCollection allGlbSecurity, StaffSecurityModuleFilter securityModuleFilter, out CheckpointLookupKey lookupKey, out SecurityCore securityCore)
		{
			lookupKey = securityModuleFilter.SecurityFilterContainer.LookupKey;
			var branchPK = (securityModuleFilter.Branch.IsValid ? securityModuleFilter.Branch : GlbBranch.CurrentBranch.PK).ToGuid();
			var departmentPK = (securityModuleFilter.Department.IsValid ? securityModuleFilter.Department : GlbDepartment.CurrentDepartment.PK).ToGuid();
			var companyPK = (securityModuleFilter.Branch.IsValid ? new BusinessObjectFactory().Load<GlbBranch>(securityModuleFilter.Branch)?.GB_GC ?? GlbCompany.CurrentCompany.PK : GlbCompany.CurrentCompany.PK).ToGuid();

			securityCore = new SecurityCore(allGlbSecurity, null, branchPK, departmentPK, companyPK);
			securityCore.CachingEnabled = false;

			if (collection.Count > 10)
			{
				securityCore.PrepareForSearching(); //load all GlbGroup and GlbGroupLink rows
			}
			new SecurityVector().Initialise(securityCore);
#if DEBUG
			if (Globals.IsTest)
			{
				var threadLocalFactory = (ThreadLocal<BusinessObjectFactory>)securityCore.SecurityInstance.GetType().GetField("ThreadLocalFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(securityCore.SecurityInstance);
				securityFactory_ExposedForTest = threadLocalFactory.Value;
			}
#endif
		}

		static void AddDenied(IList<BusinessObject> collection, CheckpointLookupKey lookupKey, SecurityCore securityCore, HashSet<ZGuid> deniedItems)
		{
			foreach (var group in collection.OfType<GlbGroup>())
			{
				securityCore.GroupPK = group.PK.ToGuid();
				ISecurityCheckpoint checkpoint = securityCore.FindCheckPoint(lookupKey);
				var checkpointExists = checkpoint != null;

				ReloadSecurityCheckpoint(lookupKey, securityCore, ref checkpointExists, ref checkpoint);

				if (checkpointExists && securityCore.SecurityInstance.IsGroupAllowed(checkpoint) == SecurityState.Denied)
				{
					deniedItems.Add(group.PK);
				}
			}

			foreach (var staff in collection.OfType<GlbStaff>())
			{
				securityCore.UserPK = staff.PK.ToGuid();
				ISecurityCheckpoint checkpoint = securityCore.FindCheckPoint(lookupKey);
				var checkpointExists = checkpoint != null;

				ReloadSecurityCheckpoint(lookupKey, securityCore, ref checkpointExists, ref checkpoint);

				if (checkpointExists && !checkpoint.IsAllowed)
				{
					deniedItems.Add(staff.PK);
				}
			}
		}

		static void ReloadSecurityCheckpoint(CheckpointLookupKey lookupKey, SecurityCore securityCore, ref bool checkpointExists, ref ISecurityCheckpoint checkpoint)
		{
			if (!checkpointExists)
			{
				securityCore.LoadRegistrySecurityCheckPoints();
				checkpoint = securityCore.FindCheckPoint(lookupKey);
				checkpointExists = checkpoint != null || securityCore.TryGetAutoGeneratedCheckpoint(lookupKey, out checkpoint);
			}
		}
	}
}
