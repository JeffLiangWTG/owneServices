using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingProcessTaskCollection : AgencyShipmentProcessTaskCollection
	{
		public BillOfLadingProcessTaskCollection(BillOfLading bol)
			: base(bol)
		{
		}

		public new BillOfLadingProcessTask this[int index]
		{
			get { return (BillOfLadingProcessTask)Elements[index]; }
		}

		public new BillOfLadingProcessTask AddNew()
		{
			return (BillOfLadingProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new BillOfLadingProcessTaskCollection(Parent);
		}

		#region Implementation

		new BillOfLading Parent
		{
			get { return (BillOfLading)base.Parent; }
		}

		#endregion

	}
}


