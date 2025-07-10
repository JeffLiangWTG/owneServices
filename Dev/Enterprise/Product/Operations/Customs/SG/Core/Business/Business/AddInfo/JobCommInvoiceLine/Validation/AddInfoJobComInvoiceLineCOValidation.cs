using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLineCOValidation : AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineCOValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckSG_TextileQuotaQuantity()
		{
			base.CheckSG_TextileQuotaQuantity();
			if (Parent.InvoiceLine.Declaration.Certificate1Type.IsTextileDetailsRequired() && Parent.InvoiceLine.Declaration.SG_ApplicationProductType == ApplicationProductTypeCodeList.Codes.TX)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_TextileQuotaQuantityInfo, "Textile Quota Quantity");
			}
		}

		protected override void CheckSG_TextileQuotaQuantityUnit()
		{
			base.CheckSG_TextileQuotaQuantityUnit();
			if (Parent.InvoiceLine.Declaration.Certificate1Type.IsTextileDetailsRequired() && Parent.InvoiceLine.Declaration.SG_ApplicationProductType == ApplicationProductTypeCodeList.Codes.TX)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_TextileQuotaQuantityUnitInfo, "Textile Unit Code");
			}
		}

		protected override void CheckSG_TextileCatCode()
		{
			base.CheckSG_TextileCatCode();
			if (Parent.InvoiceLine.Declaration.Certificate1Type.IsTextileDetailsRequired() && Parent.InvoiceLine.Declaration.SG_ApplicationProductType == ApplicationProductTypeCodeList.Codes.TX)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_TextileCatCodeInfo, "Textile Category Code.");
			}
		}
	}
}
