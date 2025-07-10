namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsSalesChannelValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWSH_Code

		public void TestCheckWSH_Code()
		{
			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "";
			AssertHasError(salesChannel.WSH_CodeInfo, "Please enter a Code.");

			salesChannel.WSH_Code = "TST";
			AssertNoErrors("Non empty code should have no issue.", salesChannel.WSH_CodeInfo);
		}

		public void TestCheckWSH_Code_IsUnique()
		{
			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "AAA";
			AssertNoErrors(salesChannel.WSH_CodeInfo);

			var newSalesChannel = Factory.New<WhsSalesChannel>();
			newSalesChannel.WSH_Code = "AAA";
			AssertHasError(newSalesChannel.WSH_CodeInfo, "Sales Channel Code 'AAA' already exists.");
		}

		#endregion

		#region TestCheckWSH_Description

		public void TestCheckWSH_Description()
		{
			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Description = "";
			AssertHasError(salesChannel.WSH_DescriptionInfo, "Please enter a Description.");

			salesChannel.WSH_Description = "TST";
			AssertNoErrors("Non empty description should have no issue.", salesChannel.WSH_DescriptionInfo);
		}

		public void TestCheckWSH_Description_IsUnique()
		{
			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Description = "Anti-Air Artillery";
			AssertNoErrors(salesChannel.WSH_DescriptionInfo);

			var newSalesChannel = Factory.New<WhsSalesChannel>();
			newSalesChannel.WSH_Description = "Anti-Air Artillery";
			AssertHasError(newSalesChannel.WSH_DescriptionInfo, "Sales Channel Description 'Anti-Air Artillery' already exists.");
		}

		#endregion
	}
}
