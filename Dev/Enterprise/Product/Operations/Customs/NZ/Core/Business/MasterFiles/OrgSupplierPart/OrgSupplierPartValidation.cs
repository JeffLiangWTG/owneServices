namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class OrgSupplierPartValidation : Customs.Business.OrgSupplierPartValidation
	{
		public OrgSupplierPartValidation(OrgSupplierPart part)
			: base(part)
		{
		}

		public new OrgSupplierPart Parent
		{
			get { return base.Parent as OrgSupplierPart; }
		}
	}
}
