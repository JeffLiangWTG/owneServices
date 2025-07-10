using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USITNumberAddInfoValidation : AutoUSITNumberAddInfoValidation
	{
		public USITNumberAddInfoValidation(AutoUSITNumberAddInfo parent) : base(parent)
		{
		}

		Bill Bill
		{
			get { return bill ?? (bill = ((ITAndSplitDetails)Parent.Parent).Bill); }
		}
		Bill bill;

		protected override void CheckUS_ITNumber()
		{
			base.CheckUS_ITNumber();
			ITNumberValidator.ValidateITNumber(Parent.US_ITNumberInfo, Bill);
		}

		protected override void CheckUS_NoOfPacks()
		{
			base.CheckUS_NoOfPacks();

			var bill = Bill;
			if (bill != null)
			{
				if (!Parent.US_NoOfPacks.IsEmpty && !bill.CU_NoOfPacks.IsEmpty && Parent.US_NoOfPacks > bill.CU_NoOfPacks)
				{
					Parent.US_NoOfPacksInfo.AddMessageError(Qty);
				}

				var declaration = bill.Declaration;
				if (Parent.US_NoOfPacks == ZInt.Zero)
				{
					if (bill.CU_NoOfPacks > 0)
					{
						Parent.US_NoOfPacksInfo.AddMessageError(NoOfPacksShouldNotBe0);
					}
					if (declaration.IsEntrySummaryValidationMode && !Parent.US_ITNumber.IsEmpty || bill.US_SESplitShip)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NoOfPacksInfo);
					}
				}
				else
				{
					if (declaration.IsACECargoReleaseValidationMode && !declaration.IsEntrySummaryValidationMode && !bill.IsBillQtySentAsManifestQtyInACE3461 && !bill.US_SESplitShip)
					{
						Parent.US_NoOfPacksInfo.AddWarning(CusDecHouseBillValidation.ACECargoReleaseQTY);
					}
				}
				bill.Validation.ValidateCU_NoOfPacks();
			}
		}
		internal const string NoOfPacksShouldNotBe0 = "Manifest Qty should not be zero if Bill Manifest Qty is entered.";
		internal const string Qty = "Manifest Qty should not be greater than Bill Manifest Qty.";
		internal const string QtyRequired = "Manifest Qty is mandatory for split shipments";

		protected override void CheckUS_ArrivalDate()
		{
			base.CheckUS_ArrivalDate();
			if (Bill != null && Bill.US_SESplitShip)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ArrivalDateInfo);
			}
		}

		protected override void CheckUS_CarrierCode()
		{
			base.CheckUS_CarrierCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CarrierCodeInfo, Parent.Lookups.USCarrierList);

			if (Bill != null && Bill.US_SESplitShip)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CarrierCodeInfo);
			}
		}

		protected override void CheckUS_FlightNumber()
		{
			base.CheckUS_FlightNumber();
			if (Bill != null && Bill.US_SESplitShip)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FlightNumberInfo);
			}
		}
	}
}
