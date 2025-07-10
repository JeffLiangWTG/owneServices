//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsCartonGroupValidation
//
//    This class should be used for overriding validation in AutoWhsCartonGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Environment.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class WhsCartonGroupValidation : AutoWhsCartonGroupValidation
	{
		public WhsCartonGroupValidation(AutoWhsCartonGroup parent) : base(parent)
		{
		}

		#region CheckWCG_Code

		protected override void CheckWCG_Code()
		{
			base.CheckWCG_Code();
			MandatoryValidation.CheckEntered(Parent.WCG_CodeInfo);
			CheckCodeIsUnique();
		}

		void CheckCodeIsUnique()
		{
			if (!Parent.WCG_CodeInfo.HasErrors())
			{
				var query = new ZQuery(WhsCartonGroupSchema.WCG_Code, Parent.WCG_Code);
				query.AddToFilter(WhsCartonGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsCartonGroup>(query) != null)
				{
					Parent.WCG_CodeInfo.AddError(Res.GetString("36cb7f95-3b2e-4c36-ae72-27b050ea0302", "Code must be unique."));
				}
			}
		}

		#endregion

		#region CheckWCG_Description

		protected override void CheckWCG_Description()
		{
			base.CheckWCG_Description();
			MandatoryValidation.CheckEntered(Parent.WCG_DescriptionInfo);
		}

		#endregion

		#region ValidateOptimizationMode

		public void ValidateOptimizationMode()
		{
			ValidateCalculatedProperty(Parent.OptimizationModeInfo);
		}

		protected void CheckOptimizationMode()
		{
			MandatoryValidation.CheckEntered(Parent.OptimizationModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OptimizationModeInfo);
		}

		#endregion

		new WhsCartonGroup Parent => (WhsCartonGroup)base.Parent;
	}
}
