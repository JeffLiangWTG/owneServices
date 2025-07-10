using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartLocationValidation : AutoOrgPartLocationValidation
	{
		public OrgPartLocationValidation(AutoOrgPartLocation parent) : base(parent)
		{
		}

		protected override void CheckOR_StockTakeCount()
		{
			base.CheckOR_StockTakeCount();
			CompareValidation.CheckNumberNotNegative(Parent.OR_StockTakeCountInfo);
		}
	}
}
