using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ContractManagement.Module
{
	sealed class SoftModuleNkFilter : ModuleNkFilter
	{
		public SoftModuleNkFilter(ZString description, GetNkQueryWithOperator getNkQueryWithOperator, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, getNkQueryWithOperator, id, list)
		{
		}

		#region Validation

		public new SoftModuleNkFilterValidation Validation => (SoftModuleNkFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new SoftModuleNkFilterValidation(this);

		#endregion
	}

	sealed class SoftModuleNkFilterValidation : ModuleNkFilterValidation
	{
		public SoftModuleNkFilterValidation(SoftModuleNkFilter parent)
			: base(parent)
		{
		}

		#region ValidateProperty

		protected override void CheckProperty()
		{
			ListValidation.WarnIfInvalidCode(Parent.PropertyInfo, Parent.List);
		}

		#endregion

		new SoftModuleNkFilter Parent => (SoftModuleNkFilter)base.Parent;
	}
}
