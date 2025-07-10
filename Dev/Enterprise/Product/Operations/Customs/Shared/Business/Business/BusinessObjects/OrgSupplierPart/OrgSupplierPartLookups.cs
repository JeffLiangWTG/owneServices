namespace Enterprise.Customs.Business
{
	public class OrgSupplierPartLookups : MasterFiles.Business.OrgSupplierPartLookups
	{
		public OrgSupplierPartLookups(OrgSupplierPart parent)
			: base(parent)
		{
		}

		protected new OrgSupplierPart Parent
		{
			get { return (OrgSupplierPart)base.Parent; }
		}
	}
}
