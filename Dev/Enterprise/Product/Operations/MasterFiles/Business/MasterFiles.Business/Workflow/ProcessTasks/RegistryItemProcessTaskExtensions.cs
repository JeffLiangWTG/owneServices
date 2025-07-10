using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public static class RegistryItemProcessTaskExtensions
	{
		public static string Location(this IRegistryItem registryItem)
		{
			Argument.NotNull(registryItem, nameof(registryItem));
			var registryItemInternal = registryItem as IRegistryItemInternals;
			Argument.NotNull(registryItemInternal, nameof(registryItemInternal));

			return registryItemInternal.Location;
		}

		public static T GetValue<T>(this StronglyTypedRegistryItem<T, T> registryItem, IWorkflowItem workflowItem)
		{
			Argument.NotNull(registryItem, nameof(registryItem));
			Argument.NotNull(workflowItem, nameof(workflowItem));

			var branchPK = Guid.Empty;
			var companyPK = Guid.Empty;
			var departmentPK = Guid.Empty;

			var template = workflowItem.Factory.Load<ProcessTaskTemplate>(workflowItem.ParentID);

			if (template == null)
			{
				companyPK = workflowItem.CompanyPK.IsValid && IsCompanyStorage(registryItem) ? workflowItem.CompanyPK.ToGuid() : Guid.Empty;
			}
			else
			{
				if (template.P0_GE.IsValid && IsDepartmentStorage(registryItem))
				{
					departmentPK = template.P0_GE.ToGuid();
				}

				if (template.P0_GB.IsValid && IsBranchStorage(registryItem))
				{
					branchPK = template.P0_GB.ToGuid();
				}
				else if (template.P0_GC.IsValid && IsCompanyStorage(registryItem))
				{
					companyPK = template.P0_GC.ToGuid();
				}
			}

			return registryItem.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
		}

		static bool IsCompanyStorage(RegistryItemWrapper registryItem)
		{
			return (registryItem.Storage & RegistryStorageFlags.Company) == RegistryStorageFlags.Company;
		}

		static bool IsBranchStorage(RegistryItemWrapper registryItem)
		{
			return (registryItem.Storage & RegistryStorageFlags.Branch) == RegistryStorageFlags.Branch;
		}

		static bool IsDepartmentStorage(RegistryItemWrapper registryItem)
		{
			return ((registryItem.Storage & RegistryStorageFlags.SystemDepartment) == RegistryStorageFlags.SystemDepartment
				|| (registryItem.Storage & RegistryStorageFlags.CompanyDepartment) == RegistryStorageFlags.CompanyDepartment
				|| (registryItem.Storage & RegistryStorageFlags.BranchDepartment) == RegistryStorageFlags.BranchDepartment);
		}
	}
}
