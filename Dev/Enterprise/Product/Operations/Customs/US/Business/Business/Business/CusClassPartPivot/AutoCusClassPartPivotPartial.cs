
namespace Enterprise.Customs.US.Business
{
	partial class AutoCusClassPartPivot
	{
		public new OrgSupplierPart Part
		{
			get { return (OrgSupplierPart)base.Part; }
		}

		protected bool CD_CottonCertificate_ReadOnly
		{
			get { return CD_CottonFeeExempt == YesNoDefaultList.Codes.Yes; }
		}
	}
}
