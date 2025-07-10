using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class BindToListsTranslatableTest : TransactionedTestCase
	{
		public void TestNewAllCartageJobTypes()
		{
			var factory = new BusinessObjectFactory();

			var cartageType1 = CreateCartageType(factory, "ESC1", "Test 1");
			var cartageType2 = CreateCartageType(factory, "IRL1", "Test 2");

			string resKey = cartageType1.E3_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(cartageType1, "Test 1").ResourceKey;
			AssertEquals("Test 1", cartageType1.E3_DescriptionMultilingual);
			factory.Save();

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "咚"));

				var bindToLists1 = BindToLists.GetCachedLists(factory);
				var newCartageJobTypes = bindToLists1.NewCartageJobTypes;
				var newAllCartageJobTypes = bindToLists1.NewAllCartageJobTypes;

				AssertEquals("咚", newCartageJobTypes.GetDescriptionFromCode("ESC1"));
				AssertEquals("Test 2", newCartageJobTypes.GetDescriptionFromCode("IRL1"));

				AssertEquals("咚", newAllCartageJobTypes.GetDescriptionFromCode("ESC1"));
				AssertEquals("Test 2", newAllCartageJobTypes.GetDescriptionFromCode("IRL1"));
			}
		}

		CommonCartageType CreateCartageType(BusinessObjectFactory factory, string jobType, string description)
		{
			var cartageType = factory.New<CommonCartageType>();
			cartageType.E3_GE = GlbDepartment.CurrentDepartment.PK;
			var cartageLegType = factory.New<CommonCartageLegType>();
			cartageLegType.E4_E3 = cartageType.PK;
			var cartageOrg = factory.New<CommonCartageOrg>();
			cartageOrg.E5_E3 = cartageType.PK;

			cartageType.E3_JobType = jobType;
			cartageType.E3_Description = description;
			return cartageType;
		}
	}
}
