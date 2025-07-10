
namespace Enterprise.Freight.CFS.Business
{
	public class TallyInnerPackLineCollection : CFSInnerPackLineCollection
	{
		public TallyInnerPackLineCollection(PackUnpackShipment master) : base(master)
		{
		}

		public new TallyPackLine AddNew()
		{
			return (TallyPackLine)base.AddNew();
		}

		public new TallyPackLine this[int index]
		{
			get { return (TallyPackLine)Elements[index]; }
		}
	}
}
