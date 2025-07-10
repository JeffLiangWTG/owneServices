using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotValidation : AutoCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(AutoCusClassPartPivot parent) : base(parent)
		{
		}

		protected virtual ZQuery DuplicateQuery(BaseCusClassification classification)
		{
			return null;
		}

		public new BaseCusClassPartPivot Parent
		{
			get { return (BaseCusClassPartPivot)base.Parent; }
		}

		protected override void CheckCI_CC()
		{
			base.CheckCI_CC();
			var part = Parent.Part as OrgSupplierPart;
			var parentClass = Parent.Classification;
			if (part != null && parentClass != null && !Parent.IsInDatabase)
			{
				var duplicateQuery = DuplicateQuery(parentClass);
				if (duplicateQuery != null)
				{
					var query = new ZQuery(part.PivotsForBinding.CompleteFilter);
					query.AddToFilter(
						CusClassPartPivotSchema.PK,
						SQLComparisonOperator.NotEqual,
						Parent.PK);
					query.AddToFilter(duplicateQuery);
					query.ReLoadExistingRows = true;
					var duplicateClassPivot =
						Parent.Factory.LoadTop1<BaseCusClassPartPivot>(query);
					if (duplicateClassPivot != null)
					{
						Parent.CI_CCInfo.AddError(Res.GetString("b1e835fd-0fd4-4489-94c2-e919f63eac01", "Someone else has already classified this product to a Lookup code '{0}'. Please cancel your changes and open again to review.", duplicateClassPivot.Classification.CC_LookupCode));
					}
				}
			}
		}
	}
}
