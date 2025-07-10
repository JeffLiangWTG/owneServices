using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartCategory))]
	sealed class OrgPartCategoryTest : EnterpriseBusinessObjectTestCase
	{
		#region TestOPC_CategoryDescription_Translatable

		public void TestOPC_CategoryDescription_Translatable()
		{
			var category = Factory.New<OrgPartCategory>();
			category.OPC_CategoryDescription = "Boom";
			var resKey = category.OPC_CategoryDescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(category, "Boom").ResourceKey;
			AssertEquals("Boom", category.OPC_CategoryDescriptionMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "咚"));
				AssertEquals("咚", category.OPC_CategoryDescriptionMultilingual);
			}
		}

		#endregion
	}
}
