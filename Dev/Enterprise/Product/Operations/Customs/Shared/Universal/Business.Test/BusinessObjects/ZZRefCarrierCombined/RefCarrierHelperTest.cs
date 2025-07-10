using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.Universal.Testing
{
	public class RefCarrierHelperTest : TestCaseWithFactory
	{
		public void TestGetTransportModesList()
		{
			var list1 = RefCarrierHelper.GetTransportModesList(Factory);
			var list2 = RefCarrierHelper.GetTransportModesList(Factory);
			AssertEquals("UniveralRefTransportModeList should be cached", list1, list2);
		}

		public void TestGetAttributes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			helper.CreateNewOrGetExistingDataGrouping("FR", "France", parentGrouping);
			helper.CreateNewOrGetExistingDataGrouping("DE", "Germany", parentGrouping);
			Factory.Save();
			var carrier1 = Factory.New<RefCarrierCode>();
			carrier1.ZZ4_Code = "TestGetAttributes1";
			carrier1.ZZ4_ZZZ_NKDataGrouping = "IT";
			carrier1.ZZ4_Description = "One";
			var carrier2 = Factory.New<RefCarrierCode>();
			carrier2.ZZ4_Code = "TestGetAttributes2";
			carrier2.ZZ4_ZZZ_NKDataGrouping = "FR";
			carrier2.ZZ4_Description = "Two";
			var carrier3 = Factory.New<RefCarrierCode>();
			carrier3.ZZ4_Code = "TestGetAttributes3";
			carrier3.ZZ4_ZZZ_NKDataGrouping = "FR";
			carrier3.ZZ4_Description = "Three";
			var carrier4 = Factory.New<RefCarrierCode>();
			carrier4.ZZ4_Code = "TestGetAttributes4";
			carrier4.ZZ4_ZZZ_NKDataGrouping = "DE";
			carrier4.ZZ4_Description = "Four";
			var attrib1A = carrier1.Attributes.AddNew();
			attrib1A.ZZG_Name = "TestGetAttributesA";
			attrib1A.ZZG_Value = "aa";
			var attrib1B = carrier1.Attributes.AddNew();
			attrib1B.ZZG_Name = "TestGetAttributesB";
			attrib1B.ZZG_Value = "bb";
			var attrib2A = carrier2.Attributes.AddNew();
			attrib2A.ZZG_Name = "TestGetAttributesA";
			attrib2A.ZZG_Value = "aa";
			var attrib3A = carrier3.Attributes.AddNew();
			attrib3A.ZZG_Name = "TestGetAttributesC";
			attrib3A.ZZG_Value = "cc";
			Factory.Save();
			var attributes = RefCarrierHelper.GetAttributes(Factory, carrier1.ZZ4_ZZZ_NKDataGrouping);
			AssertEquals("Has 2 attributes for IT", 2, attributes.Length);
			attributes = RefCarrierHelper.GetAttributes(Factory, carrier2.ZZ4_ZZZ_NKDataGrouping);
			AssertEquals("Has 2 attributes for FR", 2, attributes.Length);
			attributes = RefCarrierHelper.GetAttributes(Factory, carrier4.ZZ4_ZZZ_NKDataGrouping);
			AssertEquals("Has 0 attributes for DE", 0, attributes.Length);
		}

		public void TestGetConfig()
		{
			SetupMockRefCarrierConfig();
			AssertNotNull(RefCarrierHelper.GetConfig(CountryCodes.SouthAfrica));
			AssertNull(RefCarrierHelper.GetConfig(CountryCodes.France));
		}

		public static void SetupMockRefCarrierConfig()
		{
			var mockConfig = new Mock<IRefCarrierConfig>();
			mockConfig.Setup(c => c.MandatoryAttributes).Returns([RefCarrierAttributeNames.MASTER]);
			mockConfig.Setup(c => c.IsSetDefaultCarrierType).Returns(true);
			ObjectFactory.Substitute("ZA.RefCarrierConfig", mockConfig.Object);

			var carrierConfigurations = new KeyObjectHandleDictionaryObject();
			carrierConfigurations.SourceDictionary = new Dictionary<string, string> { { CountryCodes.SouthAfrica, "ZA.RefCarrierConfig" } };
			ObjectFactory.Substitute("RefCarrierConfigurations", carrierConfigurations);
		}
	}
}
