using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChargeCode)]
	public class ChargeCodesLinkedToTaxOverrideGroupCollection : ActiveBusinessObjectCollection<AccChargeCode>
	{
		public ChargeCodesLinkedToTaxOverrideGroupCollection(AccTaxOverrideGroup parent)
			: base(parent.Factory, parent, new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK), AccChargeCodeSchema.AC_AX_TaxOverrideGroup)
		{
		}

		protected override bool RunPreSaveValidationCore() => true;

		protected override bool AllowNew
		{
			get { return allowNew_innerValue; }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		bool allowNew_innerValue;

		public void SetAllowNew(bool value)
		{
			allowNew_innerValue = value;
		}
	}
}

