using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class TariffFindBoxTest : TestCaseWithFactory
	{
		public void TestNeedLoadNomenclatureWhenTariffNotFound()
		{
			using (var findBox = new TariffFindBox())
			{
				Assert("NeedLoadNomenclatureWhenTariffNotFound should be default as false.", !findBox.NeedLoadNomenclatureWhenTariffNotFound);
			}
		}

		public void TestDataGrouping_GetDataGroupingExists()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			using (var findBox = new TariffFindBox())
			{
				findBox.GetDataGrouping = () => "EUN";
				findBox.GetCountryCode = () => Core.Constants.CountryCodes.UnitedKingdom;
				CombineAssertions(() =>
				{
					AssertEquals("EffectiveDataGrouping", "EUN", findBox.EffectiveDataGrouping);
					AssertEquals("EffectiveTariffCountry", Core.Constants.CountryCodes.UnitedKingdom, findBox.EffectiveTariffCountry);
				});
			}
		}

		public void TestDataGrouping_GetDataGroupingNotExists()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			using (var findBox = new TariffFindBox())
			{
				findBox.GetCountryCode = () => Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("EffectiveDataGrouping", Core.Constants.CountryCodes.NewZealand, findBox.EffectiveDataGrouping);
				AssertEquals("EffectiveTariffCountry", Core.Constants.CountryCodes.UnitedKingdom, findBox.EffectiveTariffCountry);
			}
		}

		public void TestDataGrouping_DefaultCurrentCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			using (var findBox = new TariffFindBox())
			{
				AssertEquals(Core.Constants.CountryCodes.NewZealand, findBox.EffectiveDataGrouping);
			}
		}

		public void TestCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			using (var testControl = new TariffFindBoxForTest())
			{
				Assert(testControl.ListProviderTest.List.CompleteFilter.LiteralTextADO.Contains(TariffView.Schema.ZZ1_ZZZ_NKDataGrouping + " = 'NZ'"));
			}
		}

		[RequiresSTA]
		public void TestDescriptionUpdater()
		{
			ExceptionReporterTestListener.Instance.Clear();
			var dummy = Factory.New<DummyBusinessObject>();
			using (var tariffFindBox = new SlowTariffFindBoxForTest())
			{
				tariffFindBox.CreateControl();
				tariffFindBox.SetDataBinding(dummy, DummyBusinessObject.Schema.Z0_Code);
				tariffFindBox.SetDataBinding(dummy, DummyBusinessObject.Schema.Z0_Code);
				AssertEquals("The thread should do sleep.", 4, tariffFindBox.GetDescriptionCallTimes);
				AssertEquals("SlowWorkFlow Exception should be suppressed", false, ExceptionReporterTestListener.Instance.Any());
			}
		}

		public void TestGetNewPopupFormWhenBorderWiseWebIsEnabled()
		{
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;

			ArrangeAndAssertGetNewPopupForm(Core.Constants.CountryCodes.Australia, true);
		}

		public void TestGetNewPopupFormWhenBorderWiseWebIsNotEnabled()
		{
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;

			ArrangeAndAssertGetNewPopupForm(Core.Constants.CountryCodes.Australia, false);
		}

		[RequiresSTA]
		public void TestGetNewPopupFormWhenGetFindBoxWrapperReturnNull()
		{
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			using (var findBox = new TariffFindBoxForTest())
			{
				using (var popupForm = (ZForm)findBox.GetNewPopupFormForTest())
				{
					AssertNotEquals("FindBox.GetNewPopupForm().GetType()", typeof(FindBoxWrapperForBorderWise), popupForm.GetType());
				}
			}
		}

		[RequiresSTA]
		public void TestSelectFromPopupForm_ShouldRecreateTariffLookupToolControl_WhenDataGroupingHasDifferentValue()
		{
			const string someCountry = Core.Constants.CountryCodes.Australia;
			const string anotherCountry = Core.Constants.CountryCodes.Egypt;
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			DataRegistry.Instance.BorderWiseEnableWebSocketClient = false;
			BorderWiseLauncher.CountriesNotToUseBorderWise.Clear();
			BorderWiseLauncher.CountriesNotToUseBorderWise.TryAdd(anotherCountry, DateTime.UtcNow.AddHours(1));

			var dummyBo = Factory.New<DummyEnterpriseBusinessObject>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(someCountry))
			using (var form = new ZForm(dummyBo))
			using (var findBox = new TariffFindBoxForTest())
			{
				form.Controls.Add(findBox);
				findBox.CreateControl();
				findBox.SetDataBinding(dummyBo, DummyBusinessObject.Schema.Z0_Code);

				findBox.SelectFromPopupForm();

				AssertEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)findBox).PopupForm.GetType());

				GlbCompany.CurrentCompany.TemporarilySetCountry(anotherCountry);

				findBox.SelectFromPopupForm();

				AssertNotEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)findBox).PopupForm.GetType());
			}
		}

		[RequiresSTA]
		public void TestSelectFromPopupForm_ValidationForUnsupportedCountry()
		{
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			var dummyBo = Factory.New<DummyEnterpriseBusinessObject>();
			using (var form = new ZForm(dummyBo))
			using (var findBox = new TariffFindBoxForTest())
			{
				form.Controls.Add(findBox);
				findBox.CreateControl();
				findBox.SetDataBinding(dummyBo, DummyBusinessObject.Schema.Z0_Code);

				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("AAA", "Test Parent Data Grouping");
				helper.CreateNewOrGetExistingDataGrouping("BBB", "Test Data Grouping", parentGrouping);
				Factory.Save();
				findBox.GetDataGrouping = () => "BBB";
				findBox.TariffType = "TEST1";
				findBox.ErrorForUnsupportedCountry = "Tariff lookup is not supported for country/region AU. Enter the WCO Harmonized Code or enter the country/region specific HS code manually.";
				UnitTestUserNotification.Instance.ClearMessages();
				findBox.SelectFromPopupForm();
				AssertEquals("Should show error when no valid DataGrouping and TariffType exist", findBox.ErrorForUnsupportedCountry, UnitTestUserNotification.Instance.LastMessage.Text);

				helper.CreateNewOrGetExistingTariffType("BBB", "TEST1");
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				findBox.SelectFromPopupForm();
				AssertNull("Should not show error when valid DataGrouping and TariffType exist", UnitTestUserNotification.Instance.LastMessage.Text);

				helper.CreateNewOrGetExistingTariffType("AAA", "TEST2");
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				findBox.TariffType = "TEST2";
				findBox.SelectFromPopupForm();
				AssertNull("Should not show error when valid Parent DataGrouping and TariffType exist", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void ArrangeAndAssertGetNewPopupForm(string countryCode, bool shouldReturnBorderWiseFindBoxWrapper)
		{
			var dummyBo = Factory.New<DummyEnterpriseBusinessObject>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (var findBox = new TariffFindBoxForTest())
			{
				findBox.CreateControl();
				findBox.SetDataBinding(dummyBo, DummyBusinessObject.Schema.Z0_Code);
				using (var popupForm = findBox.GetNewPopupFormForTest())
				{
					if (shouldReturnBorderWiseFindBoxWrapper)
					{
						AssertEquals("FindBox.GetNewPopupForm().GetType()", typeof(FindBoxWrapperForBorderWise), popupForm.GetType());
					}
					else
					{
						AssertNotEquals("FindBox.GetNewPopupForm().GetType()", typeof(FindBoxWrapperForBorderWise), popupForm.GetType());
					}
				}
			}
		}

		class SlowTariffFindBoxForTest : TariffFindBox
		{
			public SlowTariffFindBoxForTest()
			{
				GetDescriptionCallTimes = 0;
			}

			protected override string GetDescription()
			{
				GetDescriptionCallTimes++;
				System.Threading.Thread.Sleep(6000);
				return base.GetDescription();
			}

			public int GetDescriptionCallTimes { get; private set; }
		}

		class TariffFindBoxForTest : TariffFindBox
		{
			public IFindBoxListProvider ListProviderTest => base.ListProvider;

			internal IFindBoxPopup GetNewPopupFormForTest() => GetNewPopupForm();
		}
	}
}
