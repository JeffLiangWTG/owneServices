namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		protected new Bill Bill
		{
			get { return (Bill)base.Parent; }
		}

		protected override bool NeedToValidateHouseBillAndPackages
		{
			get
			{
				JobDeclaration declaration = Bill.Declaration;
				return declaration != null && !declaration.IsECIWriteoff && !declaration.IsPeriodic;
			}
		}

		protected override bool ShouldAllowMultiMaster
		{
			get { return false; }
		}

		protected override void CheckCU_BillType()
		{
			base.CheckCU_BillType();
			if (Bill.IsHouseBill && Bill.Declaration != null && Bill.Declaration.IsECIWriteoff)
			{
				if (Bill != Bill.Declaration.PrimaryHouseBill)
				{
					Bill.CU_BillTypeInfo.AddMessageError(ErrorOneHouseBillOnlyOnECIWriteOff);
				}
			}
		}
		public const string ErrorOneHouseBillOnlyOnECIWriteOff = "You can only have one House Bill on an ECI Write-Off.";
	}
}
