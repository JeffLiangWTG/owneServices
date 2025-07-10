using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class SecondaryNotifyPartyValidation : Customs.Business.CusCodeDataValidation
	{
		public SecondaryNotifyPartyValidation(SecondaryNotifyParty parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Order()
		{
			base.CheckCY_Order();
			ValidateCY_Data();
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CY_DataInfo);
				if (Parent.CY_Order == 1 && Parent.CY_Data.IsEmpty && IsNVOCCBill)
				{
					Parent.CY_DataInfo.AddMessageError(ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
				}
			}
		}

		protected new SecondaryNotifyParty Parent
		{
			get { return (SecondaryNotifyParty)base.Parent; }
		}

		bool IsNVOCCBill
		{
			get
			{
				var bill = Parent.Bill;
				return bill != null && bill.IsNVOCCBill;
			}
		}

		bool IsInventoryRecordValidationMode
		{
			get { return Parent.IsInventoryRecordValidationMode; }
		}
	}
}
