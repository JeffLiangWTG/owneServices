using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLineValidation_INP : AddInfoJobComInvoiceLineValidation_CUSDEC
	{
		public AddInfoJobComInvoiceLineValidation_INP(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckSG_LastSellingPrice()
		{
			base.CheckSG_LastSellingPrice();
			if (!Declaration.SG_SupplyIndicator.IsEmpty && Parent.SG_LastSellingPrice.IsEmpty)
			{
				Parent.SG_LastSellingPriceInfo.AddMessageError(LastSellingPriceRequired);
			}
		}

		protected override void CheckSG_EndUseDescription()
		{
			base.CheckSG_EndUseDescription();

			if (Tariff != null)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.SG_EndUseDescriptionInfo);
			}
		}

		protected override void CheckSG_TotalDutiableWGTVOLQTY()
		{
			base.CheckSG_TotalDutiableWGTVOLQTY();

			if (Parent.SG_TotalDutiableWGTVOLQTY.IsEmpty && IsBondedWarehouseAPSDeclaration)
			{
				Parent.SG_TotalDutiableWGTVOLQTYInfo.AddMessageError("For APS declarations with goods bonded into or released from a Bonded Warehouse, specify the Total Qty/Wgt/Vol");
			}
		}

		bool IsBondedWarehouseAPSDeclaration
		{
			get
			{
				bool result = false;
				if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.APS)
				{
					var placeOfRelease = Declaration.PlaceOfRelease;
					var placeOfReceipt = Declaration.PlaceOfReceipt;
					result = placeOfRelease.IsBondedWarehouse() || placeOfReceipt.IsBondedWarehouse();
				}

				return result;
			}
		}
	}
}
