using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public delegate NumberGeneratorContext NumberGeneratorContextProviderDelegate();

	public sealed class NumberGeneratorContext
	{
		public NumberGeneratorContext()
		{
			this.companyPK = GlbCompany.CurrentCompany.PK;
			this.branchPK = GlbBranch.CurrentBranch.PK;
			this.departmentPK = GlbDepartment.CurrentDepartment.PK;
		}

		public NumberGeneratorContext(ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK)
		{
			this.companyPK = companyPK;
			this.branchPK = branchPK;
			this.departmentPK = departmentPK;
		}

		public T CompanyValue<T>(BusinessObjectFactory factory, Converter<GlbCompany, T> valueGetter)
		{
			GlbCompany company = factory.Load<GlbCompany>(companyPK);
			return company == null ? default(T) : valueGetter(company);
		}

		public T BranchValue<T>(BusinessObjectFactory factory, Converter<GlbBranch, T> valueGetter)
		{
			GlbBranch branch = factory.Load<GlbBranch>(branchPK);
			return branch == null ? default(T) : valueGetter(branch);
		}

		public T DepartmentValue<T>(BusinessObjectFactory factory, Converter<GlbDepartment, T> valueGetter)
		{
			GlbDepartment department = factory.Load<GlbDepartment>(departmentPK);
			return department == null ? default(T) : valueGetter(department);
		}

		public BillOfLadingNumberCustomisation AccessRegistry(BillCustomisationRegistryItem item)
		{
			var val = item.GetFallBackValueAtAllLevels(
				companyPK.IsEmpty ? Guid.Empty : companyPK.ToGuid(),
				branchPK.IsEmpty ? Guid.Empty : branchPK.ToGuid(),
				departmentPK.IsEmpty ? Guid.Empty : departmentPK.ToGuid());
			return (BillOfLadingNumberCustomisation)item.DataType.CloneValue(val);
		}

		public BillOfLadingNumberCustomisationsByServiceLevel AccessRegistryByServiceLevel(BillCustomisationByServiceLevelRegistryItem item)
		{
			var val = item.GetFallBackValueAtAllLevels(
				companyPK.IsEmpty ? Guid.Empty : companyPK.ToGuid(),
				branchPK.IsEmpty ? Guid.Empty : branchPK.ToGuid(),
				departmentPK.IsEmpty ? Guid.Empty : departmentPK.ToGuid());
			return (BillOfLadingNumberCustomisationsByServiceLevel)item.DataType.CloneValue(val);
		}

		readonly ZGuid companyPK;
		readonly ZGuid branchPK;
		readonly ZGuid departmentPK;
	}
}
