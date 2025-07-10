namespace Enterprise.Freight.CFS.Business
{
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;

	public class CFSLoadListConsolRatingRoute : ConsolRatingRoute
	{
		public CFSLoadListConsolRatingRoute(CFSLoadListConsol parent)
			: base(parent)
		{ }

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new CFSLoadListConsolJobDatesProvider((CFSLoadListConsol)parent); }
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(OrgWithSource.NewFrom<OrgHeader>(Parent.ShippingLinePKInfo), OrgWithSource.NewFrom<OrgHeader>(((CFSLoadListConsol)Parent).CartageCoPKInfo)); }
		}
	}
}
