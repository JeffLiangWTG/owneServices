using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Module
{
	public class DGClassDGSubstanceFilterValidation : ModuleTextFilterValidation
	{
		public DGClassDGSubstanceFilterValidation(DGClassDGSubstanceFilter parent)
			: base(parent) { }

		public void ValidateDGClass()
		{
			ValidateCalculatedProperty(Parent.DGClassInfo);
		}

		protected virtual void CheckDGClass()
		{
			ListValidation.WarnIfInvalidCode(Parent.DGClassInfo, Parent.DGClasses);
		}

		public void ValidateDGSubstance()
		{
			ValidateCalculatedProperty(Parent.DGSubstanceInfo);
		}
		protected virtual void CheckType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.DGSubstanceInfo, Parent.DGSubstances);
		}

		#region Implementation

		new DGClassDGSubstanceFilter Parent
		{
			get { return (DGClassDGSubstanceFilter)base.Parent; }
		}

		#endregion
	}
}
