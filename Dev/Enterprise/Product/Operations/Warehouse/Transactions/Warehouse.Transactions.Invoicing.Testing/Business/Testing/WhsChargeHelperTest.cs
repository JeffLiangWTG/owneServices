namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Warehouse.Transactions.Business;
	using Enterprise.Warehouse.Transactions.Business.Testing;

	public class WhsChargeHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetDocket

		public void TestGetDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "External Reference";
			Helper.CreateAccountingDataWithCharge(receive);

			var job = new Job.Loader(receive).Load(true);
			var charge1 = job.Charges[0];
			var attrib1 = charge1.JobChargeAttributes.AddNew();
			attrib1.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib1.EC_Value = "External Reference";

			AssertEquals(receive, WhsChargeHelper.GetDocket(charge1));

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			Helper.CreateAccountingDataWithCharge(invoice);

			var charge2 = invoice.JobHeader.Charges[0];
			AssertEquals(null, WhsChargeHelper.GetDocket(charge2));
			Factory.ClearCachedValue<WhsDocket>(string.Format("DocketFromCharge-{0}", charge2.PK));

			var attrib2 = charge2.JobChargeAttributes.AddNew();
			attrib2.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib2.EC_Value = "External Reference";
			AssertEquals(receive, WhsChargeHelper.GetDocket(charge2));
		}

		#endregion
	}
}
