using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocSGRefundInfo : DocumentWrapper
	{
		protected DocSGRefundInfo(RefundInfo refund, BusinessObjectFactory factoryToWrap)
			: base(refund, factoryToWrap)
		{
		}
		public static DocSGRefundInfo New(RefundInfo refund, BusinessObjectFactory factoryToWrap)
		{
			return refund != null ? new DocSGRefundInfo(refund, factoryToWrap) : null;
		}

		protected IRefundInfo inRefund
		{
			get { return ((RefundInfo)WrappedObject).Refund; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Refund Information"; }
		}

		public override string ToString()
		{
			return HumanReadableName;
		}

		#region IRefundInfo Members

		public ZString DateOfApproval
		{
			get { return inRefund.DateOfApproval.ToString("dd/MM/yyyy"); }
		}

		public ZString DeclarantCode
		{
			get { return inRefund.DeclarantCode; }
		}

		public ZString DeclarantName
		{
			get { return inRefund.DeclarantName.ToUpperInvariant(); }
		}

		public ZString EntityIdentifier
		{
			get { return inRefund.EntityIdentifier.ToUpperInvariant(); }
		}

		public ZString NameOfCompany
		{
			get { return inRefund.NameOfCompany.ToUpperInvariant(); }
		}

		public ZString RefundNumber
		{
			get { return inRefund.PermitNumber; }
		}

		public ZString TotalGoodsAndServicesTaxRefundAmount => ConvertToPayableValue(inRefund.TotalGoodsAndServicesTaxRefundAmount);

		public ZString TotalExciseDutyRefundAmount => ConvertToPayableValue(inRefund.TotalExciseDutyRefundAmount);

		public ZString TotalCustomsDutyRefundAmount => ConvertToPayableValue(inRefund.TotalCustomsDutyRefundAmount);

		public ZString TotalOtherTaxRefundAmount => ConvertToPayableValue(inRefund.TotalOtherTaxRefundAmount);

		public DocPrintPermitConditionsCollection ReasonForRefund
		{
			get
			{
				var result = new DocPrintPermitConditionsCollection(Factory);

				if (inRefund.ReasonForRefund != null)
				{
					foreach (var printConditions in inRefund.ReasonForRefund)
					{
						var boPrintPermitConditions = new PrintPermitConditions(printConditions, Factory);
						var docPrintPermitConsignment = DocPrintPermitConditions.New(boPrintPermitConditions, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public DocPrintPermitConditionsCollection RefundMessage
		{
			get
			{
				var result = new DocPrintPermitConditionsCollection(Factory);

				if (inRefund.RefundMessage != null)
				{
					foreach (var printConditions in inRefund.RefundMessage)
					{
						var boPrintPermitConditions = new PrintPermitConditions(printConditions, Factory);
						var docPrintPermitConsignment = DocPrintPermitConditions.New(boPrintPermitConditions, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public ZString ReplacementNumber
		{
			get { return inRefund.ReplacementNumber; }
		}

		public ZString TelNo
		{
			get { return inRefund.TelNo; }
		}

		public ZString UniqueRef
		{
			get { return inRefund.UniqueRef; }
		}

		public ZString RefundNumberBarcode
		{
			get { return RefundNumber.Length > 0 ? "*" + RefundNumber + "*" : ""; }
		}

		public DocRefundInfoConsignmentDetailsCollection ConsignmentDetails
		{
			get
			{
				var result = new DocRefundInfoConsignmentDetailsCollection(Factory);

				foreach (var printConsignment in inRefund.ConsignmentDetails)
				{
					var boPrintPermitConsignment = new RefundInfoConsignmentDetails(printConsignment, Factory);
					var docPrintPermitConsignment = DocRefundInfoConsignmentDetails.New(boPrintPermitConsignment, Factory);
					result.Add(docPrintPermitConsignment);
				}
				return result;
			}
		}

		ZString ConvertToPayableValue(ZDecimal value)
		{
			return value.ToString("0.00").PadLeft(16, ' ');
		}

		#endregion
	}
}
