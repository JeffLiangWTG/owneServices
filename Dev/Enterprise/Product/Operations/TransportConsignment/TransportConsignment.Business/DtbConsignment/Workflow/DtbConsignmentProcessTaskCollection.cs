using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	internal class DtbConsignmentProcessTaskCollection : ProcessTaskCollection
	{
		public DtbConsignmentProcessTaskCollection(DtbConsignment consignment)
			: base(consignment)
		{
		}

		public new DtbConsignmentProcessTask this[int index]
		{
			get { return (DtbConsignmentProcessTask)Elements[index]; }
		}

		public new DtbConsignmentProcessTask AddNew()
		{
			return (DtbConsignmentProcessTask)base.AddNew();
		}
	}
}
