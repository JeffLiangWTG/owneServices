using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105GoodsShipment_CustomsValuation : ICustomsValuation
	{
		readonly CusEntryHeader entryHeader;

		public NX5105GoodsShipment_CustomsValuation(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
		}

		ZDecimal ICustomsValuation.ExitToEntryChargeAmount => entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency;

		ZDecimal ICustomsValuation.FreightChargeAmount => entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency;

		ZDecimal ICustomsValuation.OtherChargeDeductionAmount => entryHeader.BusinessTaxBaseAmount;

		ZString ICustomsValuation.PartyRelationshipCode
		{
			get
			{
				var invoiceHeader = entryHeader.InvoiceHeaders().FirstOrDefault();
				var result = ZString.Empty;
				if (invoiceHeader != null)
				{
					switch (invoiceHeader.JZ_RelatedIndicator)
					{
						case RelationshipIndicatorList.Codes.NoRelationship:
							result = MessageConstants.CustomsValuationPartyRelationshipCode._135;
							break;
						case RelationshipIndicatorList.Codes.RelationshipIndicator:
							result = MessageConstants.CustomsValuationPartyRelationshipCode._136;
							break;
						case RelationshipIndicatorList.Codes.RelationshipIndicatorNoEffect:
							result = MessageConstants.CustomsValuationPartyRelationshipCode._137;
							break;
						case RelationshipIndicatorList.Codes.RelationshipIndicator138:
							result = MessageConstants.CustomsValuationPartyRelationshipCode._138;
							break;
						default:
							break;
					}
				}
				return result;
			}
		}

		ZDecimal ICustomsValuation.OtherChargeAmount => entryHeader.CH_TotalAdditionsInInvoiceCurrency;

		ZDecimal ICustomsValuation.OtherDeductionAmount => entryHeader.CH_TotalDeductionsInInvoiceCurrency;

		ZDecimal ICustomsValuation.InvoiceAmount => ZDecimal.Zero;

		ZDecimal ICustomsValuation.TotalDutyTaxFeeAmount => ZDecimal.Zero;
	}
}
