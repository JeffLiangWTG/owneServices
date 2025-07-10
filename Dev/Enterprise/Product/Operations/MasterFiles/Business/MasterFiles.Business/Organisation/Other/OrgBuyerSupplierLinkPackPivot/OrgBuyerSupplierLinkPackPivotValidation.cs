//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgBuyerSupplierLinkPackPivotValidation
//
//    This class should be used for overriding validation in AutoOrgBuyerSupplierLinkPackPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgBuyerSupplierLinkPackPivotValidation : AutoOrgBuyerSupplierLinkPackPivotValidation
	{
		public OrgBuyerSupplierLinkPackPivotValidation(AutoOrgBuyerSupplierLinkPackPivot parent) : base(parent)
		{
		}

		protected override void CheckQ0_UnitOfDimension()
		{
			base.CheckQ0_UnitOfDimension();

			if (!Parent.Q0_Height.IsEmpty || !Parent.Q0_Width.IsEmpty || !Parent.Q0_Length.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.Q0_UnitOfDimensionInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.Q0_UnitOfDimensionInfo);
		}

		protected override void CheckQ0_UnitOfWeight()
		{
			base.CheckQ0_UnitOfWeight();

			if (!Parent.Q0_Weight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.Q0_UnitOfWeightInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.Q0_UnitOfWeightInfo);
		}

		protected override void CheckQ0_F3()
		{
			base.CheckQ0_F3();

			var query = new ZQuery(OrgBuyerSupplierLinkPackPivotSchema.Q0_F3, Parent.Q0_F3);
			query.AddToFilter(OrgBuyerSupplierLinkPackPivotSchema.Q0_OL, SQLComparisonOperator.Equal, Parent.Q0_OL);

			if (Parent.Factory.Load<OrgBuyerSupplierLinkPackPivot>(query).Length > 1)
			{
				Parent.Q0_F3Info.AddError(Res.GetString("24139690-3f0d-40b9-9f0c-17f5dd58d11c", "Defaults for this Package Type already exist."));
			}
		}
	}
}
