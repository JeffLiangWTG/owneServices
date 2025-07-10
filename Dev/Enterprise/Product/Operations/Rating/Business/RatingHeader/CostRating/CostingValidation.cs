using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class CostingValidation : RatingHeaderValidation
	{
		public CostingValidation(Costing cost)
			: base(cost)
		{
		}

		public new Costing Parent
		{
			get { return (Costing)base.Parent; }
		}

		#region TH_OH

		protected override void CheckTH_OH()
		{
			if (!Parent.TH_OH.IsEmpty)
			{
				if (Parent.IsAnyOtherHeaderWithSameOrgAndType())
				{
					if (Parent.IsGlobal() && Parent.IsCosting())
					{
						Parent.TH_OHInfo.AddError(ErrorMessages.GlobalCostForThisSupplierAlreadyExists);
					}
					else
					{
						Parent.TH_OHInfo.AddError(ErrorMessages.CostForThisSupplierAlreadyExists);
					}
				}
				ListValidation.ErrorIfInvalidPK(Parent.TH_OHInfo, Parent.Lookups.Clients, ErrorMessages.InvalidCostingHeader);
			}

			var isGlobal = Parent.IsGlobal();
			if (Parent.IsStandardCostRate() && StandardCostingExists(isGlobal))
			{
				var errorMessage = isGlobal
					? ErrorMessages.StandardGlobalCostAlreadyExists
					: ErrorMessages.StandardCostAlreadyExists;

				Parent.TH_OHInfo.AddError(errorMessage);
			}
		}

		bool StandardCostingExists(bool isGlobal)
		{
			var query = new ZQuery(RatingHeaderSchema.TH_OH, null);
			if (isGlobal)
			{
				query.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}
			else
			{
				query.AddToFilter(RatingHeaderSchema.TH_GC, Env.CurrentCompany.PK);
			}

			query.AddToFilter(RatingHeaderSchema.TH_RateType, Parent.TH_RateType);
			query.AddToFilter(RatingHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			return Parent.Factory.LoadTop1<Costing>(query) != null;
		}

		#endregion

		#region TH_GlobalRateDescription

		protected override void CheckTH_GlobalRateDescription()
		{
			base.CheckTH_GlobalRateDescription();
			if (Parent.IsStandardCostRate())
			{
				MandatoryValidation.CheckEntered(Parent.TH_GlobalRateDescriptionInfo);
			}
		}

		#endregion
	}
}

