using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class SupplyTypeConfigurationLookups : JobConfigurationSelectorLookups
	{
		public SupplyTypeConfigurationLookups(ISupplyTypeSelector parent)
			: base(parent)
		{
		}

		#region Incoterm List

		public CodeDescriptionPairList IncotermList
		{
			get
			{
				var incotermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
				incotermList.AddPair(INCOTermCodes.All, INCOTermDescriptions.All);

				return incotermList;
			}
		}

		#endregion

		#region Line Department

		public GlbDepartmentCollection DepartmentList
		{
			get
			{
				return new GlbDepartmentCollection(new ReadOnlyBusinessObjectFactory());
			}
		}

		#endregion

		#region Supply Type List

		public CodeDescriptionPairList SupplyTypeList
		{
			get
			{
				var supplyTypeConfiguration = Parent as SupplyTypeConfiguration;
				var companyPK = supplyTypeConfiguration?.CurrentFallbackLevel?.CompanyPK(false) ?? GlbCompany.CurrentCompany.PK.ToGuid();
				var branchPK = supplyTypeConfiguration?.CurrentFallbackLevel?.BranchPK ?? GlbBranch.CurrentBranch.PK.ToGuid();
				var departmentPK = supplyTypeConfiguration?.CurrentFallbackLevel?.DepartmentPK ?? GlbDepartment.CurrentDepartment.PK.ToGuid();

				var registryItem = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);

				return registryItem.GetActiveCodeDescriptionPairList();
			}
		}

		#endregion
	}
}
