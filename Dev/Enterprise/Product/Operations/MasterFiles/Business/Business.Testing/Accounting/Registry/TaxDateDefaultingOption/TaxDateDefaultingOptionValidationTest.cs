namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class TaxDateDefaultingOptionValidationTest : JobConfigurationSelectorValidationTest
	{
		public void TestValidateLedgerForJobTypeFCN()
		{
			BizObj.JobType = "FCN";
			BizObj.Ledger = "AR";
			AssertHasError(BizObj.LedgerInfo, "Forwarding Consol can only have configuration for AP ledger.");
			BizObj.Ledger = "ALL";
			AssertHasError(BizObj.LedgerInfo, "Forwarding Consol can only have configuration for AP ledger.");
			BizObj.Ledger = "AP";
			AssertNoErrors("Expect Ledger should not have errors.", BizObj.LedgerInfo);
		}

		public void TestValidateTaxDateOption()
		{
			AssertNoErrors("Precondition: Tax Date Option should not have errors.", BizObj.TaxDateOptionInfo);
			BizObj.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			foreach (var code in BizObj.TaxDateOptionList.GetAllCodes())
			{
				BizObj.TaxDateOption = code;
				AssertNoErrors(BizObj.TaxDateOptionInfo);
			}

			BizObj.TaxDateOption = "XXX";
			AssertHasError(BizObj.TaxDateOptionInfo, "Enter a valid selection.");
		}

		#region Implementation

		protected override string ExpectedDuplicateJobParametersError
		{
			get { return "At least one more record already sets tax date option for the same Job parameters."; }
		}

		protected new TaxDateDefaultingOption BizObj
		{
			get { return (TaxDateDefaultingOption)base.BizObj; }
			set { base.BizObj = value; }
		}

		#endregion

	}
}
