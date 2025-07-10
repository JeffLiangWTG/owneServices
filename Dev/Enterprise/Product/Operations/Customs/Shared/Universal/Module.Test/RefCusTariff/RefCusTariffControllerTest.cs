using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefCusTariffController))]
	public class RefCusTariffControllerTest : ZControllerBasherTest
	{
		public void TestLoadBusinessEntityDefaultFilterData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffViewFilterDataMock = new Mock<ITariffViewFilterData>();
			tariffViewFilterDataMock.Setup(m => m.RatesApplyToCountry).Returns(Core.Constants.CountryCodes.UnitedStates);
			var controllerForTest = new RefCusTariffControllerForTest();
			using (var parentModule = new RefCusTariffModule())
			{
				controllerForTest.ParentModule = parentModule;
				parentModule.TariffViewFilterData = tariffViewFilterDataMock.Object;
				var loadedTariffView = (TariffView)controllerForTest.LoadBusinessEntityExposed(Factory, tariff.PK);
				AssertEquals("Defaulted", tariffViewFilterDataMock.Object.RatesApplyToCountry, loadedTariffView.Wrapper.RatesApplyToCountry);
			}
			tariffViewFilterDataMock.VerifyAll();
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return CusTariff;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Universal.RefCusTariff;
		}

		public override Type ControllerToBashType => typeof(RefCusTariffController);
		protected override void SetUp()
		{
			base.SetUp();
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
