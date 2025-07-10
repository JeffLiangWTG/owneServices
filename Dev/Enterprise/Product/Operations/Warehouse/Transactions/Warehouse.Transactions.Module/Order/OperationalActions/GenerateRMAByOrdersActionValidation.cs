using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateRMAByOrdersActionValidation : ZValidation
	{
		public GenerateRMAByOrdersActionValidation(BusinessObject parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(GenerateRMAByOrdersActionValidation);

		#region ValidateWhsOverride

		public void ValidateWhsOverride() => ValidateCalculatedProperty(Applicator.WhsOverrideInfo);
		
		protected void CheckWhsOverride() => ListValidation.ErrorIfInvalidPK(Applicator.WhsOverrideInfo);

		#endregion

		#region Validate All

		public override void ValidateAll() => ValidateWhsOverride();

		#endregion

		#region Parent

		GenerateRMAByOrdersActionMethodApplicator Applicator => (GenerateRMAByOrdersActionMethodApplicator)base.ParentFilter;

		#endregion
	}
}
