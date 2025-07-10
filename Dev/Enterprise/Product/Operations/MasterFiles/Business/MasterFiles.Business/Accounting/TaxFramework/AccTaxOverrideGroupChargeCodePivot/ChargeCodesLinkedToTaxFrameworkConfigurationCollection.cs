using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChargeCode)]
	public class ChargeCodesLinkedToTaxFrameworkConfigurationCollection : ActiveBusinessObjectCollection<AccChargeCode>
	{
		public ChargeCodesLinkedToTaxFrameworkConfigurationCollection(AccTaxOverrideGroup overrideGroup)
			: base(overrideGroup.Factory,
				new ManyToManyRelationship(overrideGroup,
					typeof(AccChargeCode),
					typeof(AccTaxOverrideGroupChargeCodePivot),
					new ZQuery(),
					AccTaxOverrideGroupChargeCodePivotSchema.ACP_AX_TaxOverrideGroup,
					AccTaxOverrideGroupChargeCodePivotSchema.ACP_AC_ChargeCode))
		{
		}

		protected override bool RunPreSaveValidationCore() => true;
	}
}
