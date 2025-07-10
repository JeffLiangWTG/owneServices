//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesProductValidation
//
//    This class should be used for overriding validation in AutoOrgSalesProductValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesProductValidation : AutoOrgSalesProductValidation
	{
		public OrgSalesProductValidation(AutoOrgSalesProduct parent)
			: base(parent)
		{
		}

		protected override void CheckMP_Code()
		{
			base.CheckMP_Code();

			if (!Parent.MP_Code.IsEmpty)
			{
				var otherProductWithSameCodeQuery = new ZQuery(OrgSalesProductSchema.MP_Code, Parent.MP_Code);
				otherProductWithSameCodeQuery.AddToFilter(OrgSalesProductSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.LoadTop1<OrgSalesProduct>(otherProductWithSameCodeQuery) != null)
				{
					Parent.MP_CodeInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(Parent.MP_CodeInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckMP_Name()
		{
			base.CheckMP_Name();
			MandatoryValidation.CheckEntered(Parent.MP_NameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.MP_NameInfo);
		}

		protected override void CheckMP_FormLayoutDataIsNotEmpty()
		{
			return; // Do not check is not empty. It will be populated on saving
		}
	}
}
