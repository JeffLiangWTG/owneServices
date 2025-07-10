using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocRefundInfoConsignmentDetails : DocumentWrapper
	{
		protected DocRefundInfoConsignmentDetails(RefundInfoConsignmentDetails consignmentDetails, BusinessObjectFactory factoryToWrap)
			: base(consignmentDetails, factoryToWrap)
		{
		}

		public static DocRefundInfoConsignmentDetails New(RefundInfoConsignmentDetails consignmentDetails, BusinessObjectFactory factoryToWrap)
		{
			return consignmentDetails != null ? new DocRefundInfoConsignmentDetails(consignmentDetails, factoryToWrap) : null;
		}

		protected IRefundInfoConsignment inConsignment
		{
			get { return ((RefundInfoConsignmentDetails)WrappedObject).Consignment; }
		}

		#region IRefundInfoConsignment Members

		public ZString CustomsDutyPayable
		{
			get { return inConsignment.DutyAmountPayable.ToString(2); }
		}

		public ZString ExciseDutyPayable
		{
			get { return inConsignment.ExciseAmountPayable.ToString(2); }
		}

		public ZString GstAmount
		{
			get { return inConsignment.GstAmount.ToString(2); }
		}

		public ZString HSCode
		{
			get { return inConsignment.HSCode; }
		}

		public ZString SerialNb
		{
			get { return inConsignment.SerialNb; }
		}

		#endregion
	}

	public class DocRefundInfoConsignmentDetailsCollection : DocumentWrapperCollection
	{
		public DocRefundInfoConsignmentDetailsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocRefundInfoConsignmentDetails this[int index]
		{
			get { return (DocRefundInfoConsignmentDetails)base[index]; }
		}
	}
}
