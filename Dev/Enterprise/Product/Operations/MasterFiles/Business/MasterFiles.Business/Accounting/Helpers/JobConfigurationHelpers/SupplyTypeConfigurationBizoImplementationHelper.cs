using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	class SupplyTypeConfigurationBizoImplementationHelper : IJobConfigurationHelper
	{
		public SupplyTypeConfigurationBizoImplementationHelper(ISupplyTypeConfiguration parent)
		{
			Parent = parent;
		}
		ISupplyTypeConfiguration Parent { get; }

		CodeDescriptionPairList IJobConfigurationHelper.GetLookupList()
		{
			return SupplyTypeClassificationCodesListRegistry.Value.GetCodeDescriptionPairList();
		}

		void IJobConfigurationHelper.Validate()
		{
			MandatoryValidation.CheckEntered(Parent.SupplyTypeCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SupplyTypeCodeInfo);

			if (!Parent.SupplyTypeCode.IsEmpty && !Parent.SupplyTypeCodeInfo.HasErrors())
			{
				if (!SupplyTypeClassificationCodesListRegistry.Value.GetBoolFromCode(Parent.SupplyTypeCode))
				{
					Parent.SupplyTypeCodeInfo.AddWarning(Res.GetString("20014EF9-EB6F-48DD-8134-094904B5A1AB", "This Supply Type Code is inactive. This rule will not be applied. You can review the list of Supply Type codes in the '{0}' Registry.", SupplyTypeClassificationCodesListRegistry.HumanReadableRegistryPath()));
				}
			}
		}

		bool IJobConfigurationHelper.GetReadOnlyStatus() => false;

		CodeDescriptionBoolDisallowNewRegistryItem SupplyTypeClassificationCodesListRegistry => AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList;
	}
}
