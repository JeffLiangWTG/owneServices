namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CusEntryHeaderCharges EntryHeaderCharges
		{
			get { return Parent; }
		}

		protected new CusEntryHeaderCharges Parent
		{
			get { return (CusEntryHeaderCharges)base.Parent; }
		}

		protected override void CheckC1_ChargeType()
		{
			base.CheckC1_ChargeType();

			CusEntryHeader reconEntry = Parent.Parent;
			if (reconEntry != null && reconEntry.IsReconImportEntry && reconEntry.Charges.HasDuplicate(Parent.C1_ChargeType))
			{
				Parent.C1_ChargeTypeInfo.AddMessageError(CodeCannotBeDuplicated);
			}
		}
		internal const string CodeCannotBeDuplicated = "No Charge can be duplicated.";
	}
}
