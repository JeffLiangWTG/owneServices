using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class ReferenceNumberFilterValidation : ModuleTextFilterValidation
	{
		public ReferenceNumberFilterValidation(ReferenceNumberFilter parent)
			: base(parent) { }

		public void ValidateCountry()
		{
			ValidateCalculatedProperty(Parent.CountryInfo);
		}
		protected virtual void CheckCountry()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CountryInfo, Parent.Countries);
		}

		public void ValidateType()
		{
			ValidateCalculatedProperty(Parent.TypeInfo);
		}
		protected virtual void CheckType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TypeInfo, Parent.Types);
		}

		#region Implementation

		new ReferenceNumberFilter Parent
		{
			get { return (ReferenceNumberFilter)base.Parent; }
		}

		#endregion
	}
}
