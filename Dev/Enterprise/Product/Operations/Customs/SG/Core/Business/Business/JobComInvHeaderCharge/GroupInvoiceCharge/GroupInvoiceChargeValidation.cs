using System.Linq;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public GroupInvoiceCharge GroupInvoiceCharge
		{
			get { return Parent; }
		}

		protected new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();

			var chargeType = Parent.J7_ChargeType;
			if (!chargeType.IsEmpty
							 && (chargeType == Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight || chargeType == Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance)
							 && IsInwardDecType
							 && IsCIFInvoice)
			{
				Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("80E99505-29D0-41DB-945C-88DCB6482548", "Freight and Insurance is not permitted when the INCO Term is CIF."));
			}
		}

		bool IsInwardDecType
		{
			get
			{
				var messageType = GroupInvoiceCharge?.Parent?.JobDeclaration?.JE_MessageType ?? string.Empty;
				return messageType == MessageTypeCodeList.Codes.IPT || messageType == MessageTypeCodeList.Codes.INP;
			}
		}

		bool IsCIFInvoice => Parent.GroupInvoice.AllJobComInvoiceHeaders.Any(c => c.JZ_IncoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight);
	}
}
