using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetProcessTaskCollection : ProcessTaskCollection
	{
		public DtbConsignmentRunSheetProcessTaskCollection(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
		}

		public new DtbConsignmentRunSheetProcessTask this[int index]
		{
			get { return (DtbConsignmentRunSheetProcessTask)Elements[index]; }
		}

		public new DtbConsignmentRunSheetProcessTask AddNew()
		{
			return (DtbConsignmentRunSheetProcessTask)base.AddNew();
		}
	}
}
