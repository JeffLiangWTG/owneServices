using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI.Testing
{
	sealed class LandedCostingPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestName()
		{
			AssertEquals("Name", LandedCostingPlugIn.LCPlugInName, PlugIn.Name);
		}

		public void TestUserControl()
		{
			AssertNotNull("User Control", PlugIn.UserControl);
			AssertEquals("Type", typeof(LandCostInputUserControl), PlugIn.UserControl.GetType());
		}

		public void TestMessageShownWhenLCIsNotSupported()
		{
			LCHost.IsLCSupportedExposed = false;
			LCHost.MessageShownWhenLCIsNotSupportedExposed = "Ner ner nee ner ner.... Can't touch this....";
			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			ZString coveringLabel = PlugIn.PlugInNotDisplayedMessage;
			AssertEquals("Convering Label Got Right Message", LCHost.MessageShownWhenLCIsNotSupportedExposed, coveringLabel);

			LCHost.IsLCSupportedExposed = true;
			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			coveringLabel = PlugIn.PlugInNotDisplayedMessage;
			Assert("Convering Label Got Different Message", coveringLabel != LCHost.MessageShownWhenLCIsNotSupportedExposed);
		}

		public void TestSelectWhenLCIsNotSupported()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);
			LCHost.IsLCSupportedExposed = false;
			AssertNull("LC Header is not created yet", PlugIn.LCHeader);
			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			AssertNull("LC is not supported and LCHeader is not created yet", PlugIn.LCHeader);
			Assert("Should not consume a licence", !Env.Licence.LandedCosting.IsLoggedIn);
		}

		public void TestSelectWhenMutexIsLockedByOthers()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);

			ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.LandedCostingBeingCreatedForDeclarationOrOrder, LCHost.PK.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());
			mutex.Lock();

			try
			{
				AssertEquals("Mutex is locked before plugin attempts", true, mutex.HasLock);
				LCHost.IsLCSupportedExposed = true;
				PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				AssertNull("Mutex is locked by someone else and LC Header cannot be created", PlugIn.LCHeader);

				Assert("Should not consume a licence", !Env.Licence.LandedCosting.IsLoggedIn);
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestOnSelectWhenJobIsNotInRunnableStateAndUsersSayNo()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);
			LCHost.IsLCSupportedExposed = true;
			LCHost.IsJobInLCRunnableStateExposed = false;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			AssertNull("Should not create LCHeader", PlugIn.LCHeader);
			Assert("Should not consume a licence", !Env.Licence.LandedCosting.IsLoggedIn);
		}

		public void TestOnSelectWhenUsersDontWantToCreateLandedCostingNow()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);
			LCHost.IsLCSupportedExposed = true;
			LCHost.IsJobInLCRunnableStateExposed = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			AssertNull("User dont want to create", PlugIn.LCHeader);
			Assert("Should not consume a licence", !Env.Licence.LandedCosting.IsLoggedIn);
		}

		public void TestLandedCostingExRatesAreRefreshedOnSelect()
		{
			LCHost.IsLCSupportedExposed = true;
			LCHost.IsJobInLCRunnableStateExposed = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			DummyExchangeRateHolder holder1 = Factory.New<DummyExchangeRateHolder>();
			LCHost.ExchangeRateHoldersExposed = new DummyExchangeRateHolder[] { holder1 };
			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			AssertEquals("Exchange rates are loaded", 1, PlugIn.LCHeader.ExchangeRates.Count);

			DummyExchangeRateHolder holder2 = Factory.New<DummyExchangeRateHolder>();
			LCHost.ExchangeRateHoldersExposed = new DummyExchangeRateHolder[] { holder1, holder2 };

			AssertEquals("Exchange rates still have one item", 1, PlugIn.LCHeader.ExchangeRates.Count);
			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			AssertEquals("Exchange rates are re-loaded", 2, PlugIn.LCHeader.ExchangeRates.Count);
		}

		public void TestCreateAndSynchroniseLandedCostingObjects()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);
			LCHost.IsLCSupportedExposed = true;
			LCHost.IsJobInLCRunnableStateExposed = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			DummyLandCostInput charge1 = new DummyLandCostInput();
			charge1.AmountToDistributeExposed = new Money(500, GlbCompany.CurrentCompany.LocalCurrency);
			charge1.ChargeDescriptionExposed = "Charge1";
			charge1.ExchangeRateExposed = 1m;
			charge1.FKToChargeCodeExposed = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;

			DummyLandCostInput charge2 = new DummyLandCostInput();
			charge2.AmountToDistributeExposed = new Money(400, GlbCompany.CurrentCompany.LocalCurrency);
			charge2.ChargeDescriptionExposed = "Charge2";
			charge2.ExchangeRateExposed = 0.5m;
			charge2.FKToChargeCodeExposed = ZGuid.Empty;

			DummyLandCostChargeHolder chargeHolder = new DummyLandCostChargeHolder();
			chargeHolder.ChargesToImportForLandedCostingExposed = new IDefaultLandedCostInput[] { charge1, charge2 };
			LCHost.ChargeHoldersExposed = new ILandedCostChargeHolder[] { chargeHolder };

			DummyLandedCostDistributeTo distributeTo = Factory.New<DummyLandedCostDistributeTo>();
			LCHost.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distributeTo };

			PlugIn.TopLevelMenu.MenuItems[0].PerformSelect();
			AssertNull("LCHeader should not be created when 'selected'", PlugIn.LCHeader);
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);

			PlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
			AssertNotNull("LCHeader created", PlugIn.LCHeader);
			Assert("LC created and GUI shown properly", Env.Licence.LandedCosting.IsLoggedIn);
		}

		public void TestWhenLCJobExistsAndMenuIsClicked()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = LCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LCHost.IsLCSupportedExposed = true;
			PlugIn.TopLevelMenu.MenuItems[0].PerformClick();

			Assert(Env.Licence.LandedCosting.IsLoggedIn);
		}

		public void TestWhenLCDoesNotExistAndMenuIsClicked()
		{
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);

			LCHost.IsLCSupportedExposed = true;
			PlugIn.TopLevelMenu.MenuItems[0].PerformClick();
			AssertNull(PlugIn.LCHeader);

			AssertEquals("No Landed Costing job exists. Please click the tab and create a Landed Costing job first.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(!Env.Licence.LandedCosting.IsLoggedIn);
		}

		public void TestGetLCHeaderFromHost()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = LCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LCHost.IsLCSupportedExposed = true;
			PlugIn.TopLevelMenu.MenuItems[0].PerformClick();
			AssertEquals("LCHeader", lCHeader, PlugIn.LCHeader);
		}

		public void TestGetLCHeaderWhenThereAreMoreThanOne()
		{
			ErrorReporter.Clear();
			LCHost.IsLCSupportedExposed = true;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = LCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandedCostHeader lCHeader2 = Factory.New<LandedCostHeader>();
			lCHeader2.LT_ParentID = LCHost.PK;
			lCHeader2.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			try
			{
				PlugIn.TopLevelMenu.MenuItems[0].PerformClick();
				AssertEquals(true, ErrorReporter.LastMessageReported.Contains(LandedCostingPlugIn.MoreThanOneLCHeaderMessage));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetLCHeaderWhenMutexWasReleasedAndSomeelseCreated()
		{
			LCHost.TableCodeExposed = JobOrderHeaderSchema.Constants.Prefix;

			ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.LandedCostingBeingCreatedForDeclarationOrOrder, LCHost.PK.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());
			mutex.Lock();

			try
			{
				PlugIn.TopLevelMenu.MenuItems[0].PerformSelect();
				AssertNull("Mutex is locked and cannot create a LC header", PlugIn.LCHeader);

				var factory2 = new BusinessObjectFactory();
				var lCHeader = factory2.New<LandedCostHeader>();
				lCHeader.LT_ParentID = LCHost.PK;
				lCHeader.LT_ParentTableCode = LCHost.TableCode;
				factory2.Save();
				mutex.Unlock();

				AssertEquals("LCHeader loaded for plugin", lCHeader.PK, PlugIn.LCHeader.PK);
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestChangeEnabledWhenNotSupported()
		{
			LCHost.IsLCSupportedExposed = false;
			AssertEquals(false, PlugIn.Enabled);

			LCHost.IsLCSupportedExposed = true;
			LCHost.Z0_Bool = true;
			AssertEquals("Should have enabled the plugIn", true, PlugIn.Enabled);
		}

		TestLCPlugIn PlugIn;
		DummyLandedCostHeader LCHost;

		protected override void SetUp()
		{
			base.SetUp();
			LCHost = Factory.New<DummyLandedCostHeader>();
			LCHost.ChargeHoldersExposed = Array.Empty<ILandedCostChargeHolder>();
			LCHost.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			PlugIn = new TestLCPlugIn(LCHost);
		}

		protected override void TearDown()
		{
			base.TearDown();
			PlugIn?.Dispose();
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var jobDeclaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			_ = LandedCostHeader.New((ILandedCostHeader)jobDeclaration);
			return new LandedCostingPlugIn((ILandedCostHeader)jobDeclaration);
		}

		sealed class TestLCPlugIn : LandedCostingPlugIn
		{
			public TestLCPlugIn(ILandedCostHeader hostEntity)
				: base(hostEntity)
			{
			}

			public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
