using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo
{
	public class RefundInfo : NonPersistentBusinessObject
	{
		public RefundInfo(IRefundInfo inRefundInfo, BusinessObjectFactory factory)
			: base(factory)
		{
			this.inRefundInfo = inRefundInfo;
		}

		protected IRefundInfo inRefundInfo;

		public IRefundInfo Refund
		{
			get { return inRefundInfo; }
		}
	}

	public class RefundInfoConsignmentDetails : NonPersistentBusinessObject
	{
		public RefundInfoConsignmentDetails(IRefundInfoConsignment inConsignment, BusinessObjectFactory factory)
			: base(factory)
		{
			this.inConsignment = inConsignment;
		}

		protected IRefundInfoConsignment inConsignment;

		public IRefundInfoConsignment Consignment
		{
			get { return inConsignment; }
		}

		public ZString SerialNb
		{
			get { return inConsignment.SerialNb; }
		}

		public ZString HSCode
		{
			get { return inConsignment.HSCode; }
		}

		public ZDecimal DutyAmountPayable
		{
			get { return inConsignment.DutyAmountPayable; }
		}

		public ZDecimal ExciseAmountPayable
		{
			get { return inConsignment.ExciseAmountPayable; }
		}

		public ZDecimal GstAmount
		{
			get { return inConsignment.GstAmount; }
		}
	}
}
