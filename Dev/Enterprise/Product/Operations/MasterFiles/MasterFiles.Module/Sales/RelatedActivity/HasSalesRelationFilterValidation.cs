using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public abstract class HasSalesRelationFilterValidation : ModuleFilterValidation
	{
		protected HasSalesRelationFilterValidation(HasSalesRelationFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected HasSalesRelationFilter Parent;

		#region ValidateTypeProperty

		public void ValidateTypeProperty()
		{
			ValidateCalculatedProperty(Parent.TypePropertyInfo);
		}

		protected void CheckTypeProperty()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TypePropertyInfo);
		}

		#endregion
	}
}
