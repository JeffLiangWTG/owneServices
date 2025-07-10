//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenCustomAddOnRuleValidation
//
//    This class should be used for overriding validation in AutoGenCustomAddOnRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business.CustomValues
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class GenCustomAddOnRuleValidation : AutoGenCustomAddOnRuleValidation
	{
		public GenCustomAddOnRuleValidation(AutoGenCustomAddOnRule parent)
			: base(parent)
		{ }

		protected override void CheckXR_Code()
		{
			MandatoryValidation.CheckEntered(Parent.XR_CodeInfo);

			if (Parent.XR_Code.Length > 0)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(GenCustomAddOnRuleSchema.XR_Code, Parent.XR_Code);
				query.AddToFilter(GenCustomAddOnRuleSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.Load<GenCustomAddOnRule>(query).Length != 0)
				{
					Parent.XR_CodeInfo.AddError(Res.GetString("39c6e415-d027-457d-a3d5-ce8327de9f23", "The specified rule code '{0}' already exists. Please enter a unique value.", Parent.XR_Code));
				}
			}
		}
	}
}
