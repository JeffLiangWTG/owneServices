using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ABLEntryNumValidation : ASYCUDA.Business.ABLEntryNumValidation
	{
		public ABLEntryNumValidation(ASYCUDA.Business.ABLEntryNum entryNumber)
		: base(entryNumber)
		{
		}

		protected override void CheckCE_EntryNum()
		{
			if (!Parent.CE_EntryNum.IsEmpty && (Parent.CE_EntryType == ZaLRNTypes.Codes.ABT || Parent.CE_EntryType == ZaLRNTypes.Codes.AFM))
			{
				AddErrorIfNumberUsedMoreThanOnce(checkOtherBills: false);
			}
			if (Parent.Bill is AsycudaBill bill)
			{
				bill.CustomsEntryNumberInfo.RefreshBinding();
				bill.Validation.ValidateCustomsEntryNumber();
			}
		}

		protected override void CheckCE_EntryType()
		{
			if (Parent.Bill is AsycudaBill bill && bill.Header.IsRoad)
			{
				base.CheckCE_EntryType();
				bill.CustomsEntryNumberTypeInfo.RefreshBinding();
				bill.Validation.ValidateCustomsEntryNumberType();
				bill.Validation.ValidateCustomsEntryNumber();
			}
		}
	}
}
