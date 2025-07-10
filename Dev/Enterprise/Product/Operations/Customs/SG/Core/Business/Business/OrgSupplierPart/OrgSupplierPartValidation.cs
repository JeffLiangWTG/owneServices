namespace Enterprise.Customs.SG.V4.Business
{
	public class OrgSupplierPartValidation : Customs.Business.OrgSupplierPartValidation
	{
		public OrgSupplierPartValidation(OrgSupplierPart part)
			: base(part)
		{
		}

		protected new OrgSupplierPart Parent
		{
			get { return (OrgSupplierPart)base.Parent; }
		}
	}
}
