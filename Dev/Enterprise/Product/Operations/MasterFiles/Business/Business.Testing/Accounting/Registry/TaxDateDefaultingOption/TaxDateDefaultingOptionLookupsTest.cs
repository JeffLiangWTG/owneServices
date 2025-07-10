using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class TaxDateDefaultingOptionLookupsTest : JobConfigurationSelectorLookupsTest
	{
		public new void TestJobTypeList()
		{
			base.TestJobTypeList();

			var taxDateDefaultingOptionLookups = new TaxDateDefaultingOptionLookups(BizObj);
			AssertEquals(53, taxDateDefaultingOptionLookups.JobTypeList.Count);
			Assert("The JobTypeList should contain 'FCN'", taxDateDefaultingOptionLookups.JobTypeList.ContainsCode(JobInvoicingConsumerTypes.ForwardingConsolCode));
		}

		public void TestLedgerList()
		{
			var taxDateDefaultingOptionLookups = new TaxDateDefaultingOptionLookups(BizObj);
			AssertEquals(3, taxDateDefaultingOptionLookups.LedgerList.Count);
			AssertEquals("ALL, AP, AR", BizObj.TaxDateDefaultingOptionLookups.LedgerList.CodesAsString);
			AssertEquals("Accounts Payable", taxDateDefaultingOptionLookups.LedgerList.GetDescriptionFromCode(LedgerTypes.AccountsPayable));
			AssertEquals("Accounts Receivable", taxDateDefaultingOptionLookups.LedgerList.GetDescriptionFromCode(LedgerTypes.AccountsReceivable));
		}

		public void TestTaxDateOptionList()
		{
			var taxDateDefaultingOptionLookups = new TaxDateDefaultingOptionLookups(BizObj);
			foreach (CodeDescriptionPair jobType in JobConfigurationSelectorLookups.GetBaseJobTypeList())
			{
				BizObj.JobType = jobType.Code;
				switch (BizObj.JobType)
				{
					case JobInvoicingConsumerTypes.ShipmentCode:
						AssertEquals("TaxDateOptionList.Count", 8, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV, ARV, DEP, EAD, EDD, PIC, DEL", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
					case JobInvoicingConsumerTypes.GatewayConsolCode:
						AssertEquals("TaxDateOptionList.Count", 6, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV, ARV, DEP, EAD, EDD", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
					case JobInvoicingConsumerTypes.BrokerageCode:
						AssertEquals("TaxDateOptionList.Count", 5, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV, CUS, PIC, DEL", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
					case JobInvoicingConsumerTypes.ForwardingConsolCode:
					case JobInvoicingConsumerTypes.TransportBookingCode:
					case JobInvoicingConsumerTypes.TransportBookingConsignmentCode:
					case JobInvoicingConsumerTypes.TransportConsignmentCode:
						AssertEquals("TaxDateOptionList.Count", 2, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
					case JobInvoicingConsumerTypes.AgencyBookingCode:
					case JobInvoicingConsumerTypes.AgencyBillOfLadingCode:
						AssertEquals("TaxDateOptionList.Count", 4, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV, VAD, VDD", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
					case JobInvoicingConsumerTypes.LocalCartageCode:
						AssertEquals("TaxDateOptionList.Count", 4, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV, PIC, DEL", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
					default:
						AssertEquals("TaxDateOptionList.Count", 2, BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.Count);
						AssertEquals("TDY, INV", BizObj.TaxDateDefaultingOptionLookups.TaxDateOptionList.CodesAsString);
						break;
				}
			}
		}

		#region Implementation

		protected new TaxDateDefaultingOption BizObj
		{
			get { return (TaxDateDefaultingOption)base.BizObj; }
			set { base.BizObj = value; }
		}

		#endregion
	}
}
