//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFCCAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSFCCAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USFCCAddInfoValidation : AutoUSFCCAddInfoValidation
	{
		public USFCCAddInfoValidation(AutoUSFCCAddInfo parent)
			: base(parent)
		{
		}

		FCC FCC
		{
			get { return ((FCCAddInfo)Parent).Parent; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return FCC != null ? FCC.InvoiceLine : null; }
		}

		bool FromInvoiceLine
		{
			get { return InvoiceLine != null; }
		}

		protected override void CheckUS_FCCImpCondNo()
		{
			base.CheckUS_FCCImpCondNo();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FCCImpCondNoInfo, Parent.Lookups.FCCImportConditionNumbers);

			if (Parent.US_FCCImpCondNo.IsEmpty && FromInvoiceLine)
			{
				Parent.US_FCCImpCondNoInfo.AddMessageError(ImportConditionNumRequired);
			}
		}
		internal const string ImportConditionNumRequired = "Import Condition Number is Mandatory.";

		protected override void CheckUS_FCCImpCondNoQtyAppr()
		{
			base.CheckUS_FCCImpCondNoQtyAppr();

			if (Parent.US_FCCImpCondNo == FCCImportConditionNumberList.Codes._03)
			{
				if (Parent.US_FCCQty <= 200)
				{
					if (Parent.US_FCCImpCondNoQtyAppr)
					{
						Parent.US_FCCImpCondNoQtyApprInfo.AddMessageError(ApprovalNotRequiredForQuantity);
					}
				}
				else
				{
					if (!Parent.US_FCCImpCondNoQtyAppr && FromInvoiceLine)
					{
						Parent.US_FCCImpCondNoQtyApprInfo.AddMessageError(ApprovalRequiredForQuantity);
					}
				}
			}
			else if (!Parent.US_FCCImpCondNo.IsEmpty)
			{
				if (Parent.US_FCCImpCondNoQtyAppr)
				{
					Parent.US_FCCImpCondNoQtyApprInfo.AddMessageError(ApprovalNotRequiredMessage);
				}
			}
		}
		internal const string ApprovalNotRequiredMessage = "Prior approval is not required for the selected condition.";
		internal const string ApprovalNotRequiredForQuantity = "Prior approval is not required where the quantity is 200 units or less.";
		internal const string ApprovalRequiredForQuantity = "Prior approval must be arranged for where the quantity is greater than 200 units.";

		protected override void CheckUS_FCCID()
		{
			base.CheckUS_FCCID();

			if (Parent.US_FCCID.IsEmpty)
			{
				if (Parent.US_FCCImpCondNo == FCCImportConditionNumberList.Codes._01 && FromInvoiceLine)
				{
					Parent.US_FCCIDInfo.AddMessageError(IDRequired);
				}
			}
			else
			{
				if (Parent.US_FCCID.Length < 4)
				{
					Parent.US_FCCIDInfo.AddMessageError(IDLengthIsIncorrect);
				}
			}
		}
		internal const string IDLengthIsIncorrect = "An ID with A minimum of four characters is required (remember to include hypens/dashes).";
		internal const string IDRequired = "FCC Identifier is required when Import Condition Number is '01'.";

		protected override void CheckUS_FCCTradeName()
		{
			base.CheckUS_FCCTradeName();
			if (Parent.US_FCCTradeName.IsEmpty && FromInvoiceLine)
			{
				Parent.US_FCCTradeNameInfo.AddMessageError(TradeNameRequired);
			}
		}
		internal const string TradeNameRequired = "Trade Name is required.";

		protected override void CheckUS_FCCModel()
		{
			base.CheckUS_FCCModel();
			if (Parent.US_FCCModel.IsEmpty && FromInvoiceLine)
			{
				Parent.US_FCCModelInfo.AddMessageError(ModelRequired);
			}
		}
		internal const string ModelRequired = "Model is required.";

		protected override void CheckUS_FCCQty()
		{
			base.CheckUS_FCCQty();

			if (InvoiceLine != null && Parent.US_FCCQty <= 0)
			{
				Parent.US_FCCQtyInfo.AddMessageError(QuantityRequired);
			}
		}
		internal const string QuantityRequired = "Quantity is required.";

		protected override void CheckUS_FCCCommercialDesc()
		{
			base.CheckUS_FCCCommercialDesc();

			if (Parent.US_FCCCommercialDesc.IsEmpty && FromInvoiceLine)
			{
				Parent.US_FCCCommercialDescInfo.AddMessageError(CommercialDescriptionIsMandatory);
			}
		}
		internal const string CommercialDescriptionIsMandatory = "Commercial Description is mandatory.";
	}
}
