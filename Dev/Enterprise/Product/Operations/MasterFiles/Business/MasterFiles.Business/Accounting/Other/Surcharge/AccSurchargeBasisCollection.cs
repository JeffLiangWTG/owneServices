using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeBasisCollection : DependentBusinessObjectCollection<AccSurchargeBasis, AccSurchargeConfiguration>
	{
		public AccSurchargeBasisCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccSurchargeBasisCollection(AccSurchargeConfiguration parent) : base(parent)
		{
			this.Parent = parent;
		}

		readonly AccSurchargeConfiguration Parent;

		protected override string FkColumnName => AccSurchargeBasisSchema.ASB_ASC_SurchargeConfiguration.Name;

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (this.Count == 0 && Parent.ASC_BasisType != InvoiceTypeChargeInclusionTypeList.Codes.ALL)
			{
				Parent.Validation.ValidateASC_BasisType();
			}
		}
	}
}
