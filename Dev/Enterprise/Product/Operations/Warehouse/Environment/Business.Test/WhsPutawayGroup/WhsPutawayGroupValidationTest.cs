namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsPutawayGroupValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWPG_Code

		public void TestCheckWPG_Code()
		{
			var group = Factory.New<WhsPutawayGroup>();
			group.WPG_Code = "";
			AssertHasErrors("It should be complaining because it's mandatory", group.WPG_CodeInfo);
			group.WPG_Code = "TST";
			AssertNoErrors("Not it's set, should not be problems.", group.WPG_CodeInfo);
		}

		public void TestCheckWPG_Code_IsUnique()
		{
			var group = Factory.New<WhsPutawayGroup>();
			group.WPG_Code = "AAA";
			AssertNoErrors(group.WPG_CodeInfo);

			var newGroup = Factory.New<WhsPutawayGroup>();
			newGroup.WPG_Code = "AAA";
			AssertHasError(newGroup.WPG_CodeInfo, "Putaway Group Code 'AAA' already exists.");
		}

		#endregion

		#region TestCheckWPG_Description

		public void TestCheckWPG_Description()
		{
			var group = Factory.New<WhsPutawayGroup>();
			group.WPG_Description = "";
			AssertHasErrors("It should be complaining because it's mandatory", group.WPG_DescriptionInfo);
			group.WPG_Description = "TST";
			AssertNoErrors("Not it's set, should not be problems.", group.WPG_DescriptionInfo);
		}

		public void TestCheckWPG_Description_IsUnique()
		{
			var group = Factory.New<WhsPutawayGroup>();
			group.WPG_Description = "Anti-Air Artillery";
			AssertNoErrors(group.WPG_DescriptionInfo);

			var newGroup = Factory.New<WhsPutawayGroup>();
			newGroup.WPG_Description = "Anti-Air Artillery";
			AssertHasError(newGroup.WPG_DescriptionInfo, "Putaway Group Description 'Anti-Air Artillery' already exists.");
		}

		#endregion
	}
}
