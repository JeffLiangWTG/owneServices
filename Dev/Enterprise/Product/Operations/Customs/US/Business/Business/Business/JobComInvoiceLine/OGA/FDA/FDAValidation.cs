namespace Enterprise.Customs.US.Business
{
	public class FDAValidation : Customs.Business.MultiLineAddInfos.CusAddInfoValidation
	{
		public FDAValidation(FDA fda)
			: base(fda)
		{
		}

		new FDA Parent
		{
			get { return (FDA)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFDAValueInvCurrRunningTotal();
		}

		public void ValidateFDAValueInvCurrRunningTotal()
		{
			ValidateCalculatedProperty(Parent.FDAValueInvCurrRunningTotalInfo);
		}

		protected void CheckFDAValueInvCurrRunningTotal()
		{
			var invoiceLine = Parent.InvoiceLine;
			if (invoiceLine != null && invoiceLine.Declaration.IsImport && invoiceLine.IsOGAValueUpToDate)
			{
				if (invoiceLine.FDAValueInvCurrRunningTotal > invoiceLine.JI_LinePrice)
				{
					Parent.FDAValueInvCurrRunningTotalInfo.AddMessageError(TotalInvCurrFDAValueGreaterThanLinePrice);
				}
			}
		}
		internal const string TotalInvCurrFDAValueGreaterThanLinePrice = "The total Inv. Curr. FDA Value entered is greater than line price.";
	}
}
