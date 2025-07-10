namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USMLCategoryCodesTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		// Effective date: 2013-10-15
		public void TestGetCachedListBeforeEffectiveDate()
		{
			var list = USMLCategoryCodes.GetCachedPreECRList(Factory);
			Assert("should be cached", object.ReferenceEquals(list, USMLCategoryCodes.GetCachedPreECRList(Factory)));
			AssertEquals(21, list.Count);
		}

		public void TestGetCachedListAfterEffectiveDate()
		{
			var list = USMLCategoryCodes.GetCachedPostECRList(Factory);
			AssertEquals(22, list.Count);
			Assert("should be cached", object.ReferenceEquals(list, USMLCategoryCodes.GetCachedPostECRList(Factory)));
			AssertEquals(USMLCategoryCodes.Descriptions.GasTurbineEnginesAndAssociatedEquipment, list.GetDescriptionFromCode(USMLCategoryCodes.Codes.GasTurbineEnginesAndAssociatedEquipment));
		}
	}
}
