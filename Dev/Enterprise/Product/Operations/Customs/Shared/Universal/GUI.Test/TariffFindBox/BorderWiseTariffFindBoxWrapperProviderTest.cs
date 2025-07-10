using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class BorderWiseTariffFindBoxWrapperProviderTest : TestCaseWithFactory
	{
		public void TestGetFindBoxWrapper()
		{
			var wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(null, Core.Constants.CountryCodes.Australia, "", ZDateTime.Now, "testDataGrouping");

			AssertNull(wrapper);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var dummyBo = Factory.New<DummyBusinessObject>();
				wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Australia, "", ZDateTime.Now, "testDataGrouping");

				AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
				AssertEquals("E", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);
			}
		}

		public void TestGetFindBoxWrapperShouldUseRefCusMapToGetParameterForBorderWise()
		{
			var dummyBo = Factory.New<DummyBusinessObject>();
			var wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Australia, "NoRecord", ZDateTime.Now, "testDataGrouping");

			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
			AssertEquals("E", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.BORDERWISE, MapDirectionList.Codes.BTH, "borderwise", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.BORDERWISE, "cw1Value", "I", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "CDS");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.UnitedStates, "cw1Value", ZDateTime.Now, "CDS");
			}

			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
			AssertEquals("I", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);
		}

		public void TestGetFindBoxWrapperShouldMapDefaultBorderWiseTariffTypeWhenNoRefMapFound()
		{
			var dummyBo = Factory.New<DummyBusinessObject>();
			var wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Australia, "IMP", ZDateTime.Now, "testDataGrouping");

			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
			AssertEquals("I", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);

			wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Australia, "EXP", ZDateTime.Now, "testDataGrouping");

			AssertEquals("E", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);

			wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Australia, "ABC", ZDateTime.Now, "testDataGrouping");

			AssertEquals("E", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);
		}

		public void TestGetFindBoxWrapperShouldReturnNullWhenBorderWiseLauncherIsNull()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Egypt))
			{
				var dummyBo = Factory.New<DummyBusinessObject>();
				var wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Egypt, "", ZDateTime.Now, "testDataGrouping");

				AssertNull(wrapper);
				ErrorReporter.Clear();
			}
		}

		public void TestGetFindBoxWrapperShouldReturnNullWhenDataGroupingIsWCO()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Egypt))
			{
				var dummyBo = Factory.New<DummyBusinessObject>();
				var wrapper = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dummyBo, Core.Constants.CountryCodes.Australia, "cw1Value", ZDateTime.Now, "wco");

				AssertNull(wrapper);
			}
		}

		public void TestGetAdditionalDataForBorderWise_ShouldSetCountryCodeOverrideWhenDataGroupingIsXI()
		{
			var dummyBo = Factory.New<DummyBusinessObject>();
			var additionalData = BorderWiseTariffFindBoxWrapperProvider.GetAdditionalDataForBorderWise(dummyBo, Core.Constants.CountryCodes.Australia, "cw1Value", ZDateTime.Now, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes);

			AssertEquals("XI", additionalData.CountryCodeOverride);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
		}

		protected override void TearDown()
		{
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			base.TearDown();
		}
	}
}
