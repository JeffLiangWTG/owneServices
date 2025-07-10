namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingPackLineManyToManyCollection : AgencyShipmentPackLineManyToManyCollection
	{
		public BillOfLadingPackLineManyToManyCollection(BillOfLadingContainer container)
			: base(container) { }

		public new BillOfLadingPackLine AddNew()
		{
			return (BillOfLadingPackLine)base.AddNew();
		}

		public new BillOfLadingPackLine this[int index]
		{
			get { return (BillOfLadingPackLine)base[index]; }
		}
	}
}


