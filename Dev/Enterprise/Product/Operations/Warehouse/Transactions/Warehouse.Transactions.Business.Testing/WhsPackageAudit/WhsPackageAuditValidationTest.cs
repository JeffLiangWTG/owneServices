namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsPackageAuditValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region CheckWPA_PackageID

		public void TestCheckWPA_PackageID()
		{
			var audit = Factory.New<WhsPackageAudit>();
			audit.WPA_PackageID = "";
			AssertHasErrors("It should be complaining because it's mandatory", audit.WPA_PackageIDInfo);
			audit.WPA_PackageID = "TST";
			AssertNoErrors("Not it's set, should not be problems.", audit.WPA_PackageIDInfo);
		}

		#endregion

		#region CheckWPA_GS_NKAuditor

		public void TestCheckWPA_GS_NKAuditor()
		{
			var audit = Factory.New<WhsPackageAudit>();
			audit.WPA_GS_NKAuditor = "";
			AssertHasErrors("It should be complaining because it's mandatory", audit.WPA_GS_NKAuditorInfo);
			audit.WPA_GS_NKAuditor = "TST";
			AssertNoErrors("Not it's set, should not be problems.", audit.WPA_GS_NKAuditorInfo);
		}

		#endregion
	}
}
