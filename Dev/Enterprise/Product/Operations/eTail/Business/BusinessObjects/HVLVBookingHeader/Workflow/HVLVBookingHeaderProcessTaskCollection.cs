using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public HVLVBookingHeaderProcessTaskCollection(HVLVBookingHeader bookingHeader)
			: base(bookingHeader)
		{ }

		new HVLVBookingHeader Parent
		{
			get { return (HVLVBookingHeader)base.Parent; }
		}

		public new HVLVBookingHeaderProcessTask this[int index]
		{
			get { return (HVLVBookingHeaderProcessTask)Elements[index]; }
		}

		public new HVLVBookingHeaderProcessTask AddNew()
		{
			return (HVLVBookingHeaderProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new HVLVBookingHeaderProcessTaskCollection(Parent);
		}
	}
}
