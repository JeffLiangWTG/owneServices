namespace Enterprise.Customs.SG.V4.Business
{
	public class OrgSupplierPartLookups : Customs.Business.OrgSupplierPartLookups
	{
		public OrgSupplierPartLookups(OrgSupplierPart part)
			: base(part)
		{
		}

		protected new OrgSupplierPart Parent => (OrgSupplierPart)base.Parent;

		public ClassificationCollection SG4Classifications => new ClassificationCollection(Factory);
	}
}
