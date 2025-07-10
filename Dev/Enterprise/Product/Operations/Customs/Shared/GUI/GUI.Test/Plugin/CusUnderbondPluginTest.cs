using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.PlugIn.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	sealed class CusUnderbondPluginTest : TestCaseWithFactory
	{
		public void TestTypeOfCurrentCannotBeNullInConstructor()
		{
			CusUnderbondPlugin plugin = null;
			bool foo = false;
			bool bar = false;
			try
			{
				plugin = new CusUnderbondPlugin(Host, ZString.Empty, "Foo", Host.GetType());
				plugin.Dispose();
				foo = true;
				plugin = new CusUnderbondPlugin(Host, ZString.Empty, "Bar", null);
				plugin.Dispose();
				bar = true;
			}
			catch (ArgumentNullException)
			{
				AssertEquals(true, foo);
				AssertEquals(false, bar);
			}
			finally
			{
				if (plugin != null)
				{
					plugin.Dispose();
				}
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestTypeOfElementsNotPassed()
		{
			dependentPlugin.fTypeOfCurrent = null;
			dependentPlugin.fIsCurrentDependent = true;
			dependentPlugin.fCurrent = null;
			Type typeOfCurrent = dependentPlugin.TypeOfCurrent;
		}

		public void TestRemoveColumnsForAirAndSea()
		{
			AirObjectTestHelper airObject = Factory.New<AirObjectTestHelper>();
			using (CusUnderbondPluginTestHelper plugin = new CusUnderbondPluginTestHelper(airObject, ZString.Empty, "Foo", airObject.GetType()))
			{
				using (Control control = plugin.UserControl)
				{
					Assert(plugin.RemoveColumnsForAirHasBeenHit);
					Assert(!plugin.RemoveColumnsForSeaHasBeenHit);
				}
			}

			SeaObjectTestHelper seaObject = Factory.New<SeaObjectTestHelper>();
			using (CusUnderbondPluginTestHelper pluginSea = new CusUnderbondPluginTestHelper(seaObject, ZString.Empty, "Foo", seaObject.GetType()))
			{
				using (Control control = pluginSea.UserControl)
				{
					Assert(!pluginSea.RemoveColumnsForAirHasBeenHit);
					Assert(pluginSea.RemoveColumnsForSeaHasBeenHit);
				}
			}
		}

		public void TestTypeOfCurrent()
		{
			dependentPlugin.fIsCurrentDependent = false;
			dependentPlugin.fTypeOfCurrent = typeof(CusUnderbond);
			dependentPlugin.fCurrent = Host;
			AssertEquals("uses current's type when not current dependent and when current exists", typeof(CusUnderbond), dependentPlugin.TypeOfCurrent);
			dependentPlugin.fIsCurrentDependent = true;
			AssertEquals("uses current's type when current dependent and when current exists", typeof(CusUnderbond), dependentPlugin.TypeOfCurrent);
			dependentPlugin.fTypeOfCurrent = null;
			AssertEquals("uses typeofcurrent when current is null", typeof(DummyCusUnderbondUnionCollectionParent), dependentPlugin.TypeOfCurrent);
		}

		public void TestTypeOfElementsInCollectionIfIsCurrentDependent()
		{
			AssertEquals("TypeOfElements on plugin's collection", typeof(ICusUnderbondUnionCollectionParent), plugin.Collection.TypeOfElements);
			AssertEquals("TypeOfElements on dependent plugin's collection", typeof(DummyCusUnderbondUnionCollectionParent), dependentPlugin.Collection.TypeOfElements);
		}

		public void TestUserControlEnablingDependingOnIsCurrentDependent()
		{
			dependentPlugin.fIsCurrentDependent = false;
			dependentPlugin.fCurrent = null;
			dependentPlugin.OnCurrentChangedInternal();
			AssertEquals("usercontrol when is not current dependent and current null", true, dependentPlugin.UserControl.Enabled);
			dependentPlugin.fIsCurrentDependent = true;
			dependentPlugin.fCurrent = Host;
			dependentPlugin.OnCurrentChangedInternal();
			AssertEquals("usercontrol when is current dependent and current not null", true, dependentPlugin.UserControl.Enabled);
			dependentPlugin.fCurrent = null;
			dependentPlugin.OnCurrentChangedInternal();
			AssertEquals("usercontrol when is current dependent and current null", false, dependentPlugin.UserControl.Enabled);
		}

		public void TestOutturnVisibility()
		{
			TestHelperCusUnderbond underbond = Factory.New<TestHelperCusUnderbond>();
			underbond.CanDoOutturnResult = true;
			dependentPlugin.UserControl.SetDataBinding(Host, "");
			dependentPlugin.OnCurrentChangedInternal();
			AssertEquals(true, dependentPlugin.UserControl.DetailsUserControl.MainTabControl.Contains(dependentPlugin.UserControl.DetailsUserControl.OutturnTabPage));
			dependentPlugin.UserControl.DetailsUserControl.MainTabControl.SelectedTab = dependentPlugin.UserControl.DetailsUserControl.OutturnTabPage;
			Assert("However covering label should NOT be visible with explanation", !dependentPlugin.UserControl.DetailsUserControl.CoveringLabel.Visible);
			dependentPlugin.fCurrent = Host;
			Host.AllUnderbonds.Add(underbond);
			AssertEquals(true, dependentPlugin.UserControl.DetailsUserControl.MainTabControl.Contains(dependentPlugin.UserControl.DetailsUserControl.OutturnTabPage));
			Assert("However covering label should be visible with explanation", !dependentPlugin.UserControl.DetailsUserControl.CoveringLabel.Visible);
			dependentPlugin.OnCurrentChangedInternal();
			AssertEquals(true, dependentPlugin.UserControl.DetailsUserControl.MainTabControl.Contains(dependentPlugin.UserControl.DetailsUserControl.OutturnTabPage));
		}

		public void TestPluginAlwaysAllowed()
		{
			AssertEquals("CheckPoint", Env.Licence.AlwaysAllow, ((IPlugInInternals)plugin).LicenceCheckPoint);
		}

		public void TestName()
		{
			AssertEquals("Name", "Customs Underbond Movement", plugin.Name);
		}

		public void TestOnGUIShownSetsBindPrepend()
		{
			using (TestHelperCusUnderbondPlugin plugin = new TestHelperCusUnderbondPlugin(Host))
			{
				plugin.OnGUIShown();
				AssertEquals("SetBindPrependCalled", true, ((TestHelperCusUnderbondUserControl)plugin.UserControl).SetBindPrependCalled);
				((TestHelperCusUnderbondUserControl)plugin.UserControl).SetBindPrependCalled = false;
				plugin.OnGUIShown();
				AssertEquals("SetBindPrepend should not be called twice", false, ((TestHelperCusUnderbondUserControl)plugin.UserControl).SetBindPrependCalled);
			}
		}

		[ExpectNoExceptions()]
		public void TestOptionalFirstDateOfArrival()
		{
			(new CusUnderbondPlugin(Host, ZString.Empty, null)).Dispose();
		}

		public void TestRegisterPlugInBusinessEntityAsEditable()
		{
			AssertEquals(false, plugin.RegisterPlugInBusinessEntityAsEditableInternal);
		}

		public void TestCollection()
		{
			AssertEquals(typeof(CusUnderbondUnionCollectionParentCollection), plugin.Collection.GetType());
		}

		public void TestOnCurrentChangesRefereshesCollection()
		{
			DummyCusUnderbondUnionCollectionParent host1 = (DummyCusUnderbondUnionCollectionParent)((CusUnderbondUnionCollectionParentCollection)(plugin.BusinessEntity))[0];
			DummyCusUnderbondUnionCollectionParent host2 = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			plugin.businessObject = host2;
			AssertEquals(host1, ((CusUnderbondUnionCollectionParentCollection)plugin.BusinessEntity)[0]);
			plugin.OnCurrentChangedInternal();
			AssertEquals(host2, ((CusUnderbondUnionCollectionParentCollection)plugin.BusinessEntity)[0]);
		}

		public void TestGetUserControl()
		{
			DummyCusUnderbondUnionCollectionParent parent = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			using (CusUnderbondPlugin plugin = new CusUnderbondPlugin(parent, ZString.Empty))
			{
				using (CusUnderbondUserControl control = plugin.UserControl)
				{
					AssertNull(control.UnderbondParentForTest);
				}
			}

			DummyWithNilOutturnPerformer parent2 = new DummyWithNilOutturnPerformer();
			using (CusUnderbondPlugin plugin = new CusUnderbondPlugin(parent2, ZString.Empty))
			{
				using (CusUnderbondUserControl control = plugin.UserControl)
				{
					AssertNotNull(control.UnderbondParentForTest);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			plugin = new CusUnderbondPlugin(Host, ZString.Empty);
			dependentPlugin = new TestHelperCusUnderbondPlugin(Host);
			dependentPlugin.fIsCurrentDependent = true;
			dependentPlugin.fCurrent = Host;
		}

		protected override void TearDown()
		{
			base.TearDown();
			plugin.Dispose();
			dependentPlugin.Dispose();
		}
		CusUnderbondPlugin plugin;
		TestHelperCusUnderbondPlugin dependentPlugin;

		DummyCusUnderbondUnionCollectionParent host;
		DummyCusUnderbondUnionCollectionParent Host => host ?? (host = Factory.New<DummyCusUnderbondUnionCollectionParent>());

		sealed class DummyWithNilOutturnPerformer : NonPersistentBusinessObject, ICusUnderbondNilUnderbondPerformer, ICusUnderbondUnionCollectionParent
		{
			public bool IsForAirCargo
			{
				get => isForAirCargo;

				set => isForAirCargo = value;
			}
			bool isForAirCargo;

			public bool UsesTranshipmentPortOnUnderbond
			{
				get => usesTranshipmentPortOnUnderbond;

				set => usesTranshipmentPortOnUnderbond = value;
			}
			bool usesTranshipmentPortOnUnderbond;

			public CusUnderbondUnionCollection AllUnderbonds => null;

			public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders() => null;

			ZString ICusUnderbondNilUnderbondPerformer.PerformNilUnderbond(CusUnderbond underbond) => ZString.Empty;
		}

		sealed class TestHelperCusUnderbondPlugin : CusUnderbondPlugin
		{
			public TestHelperCusUnderbondPlugin(ICusUnderbondUnionCollectionParent hostEntity) : base(hostEntity, ZString.Empty)
			{
			}

			protected override CusUnderbondUserControl CreateNewUserControl() => new TestHelperCusUnderbondUserControl();

			public override bool IsCurrentDependent => fIsCurrentDependent;

			public override BusinessObject Current => fIsCurrentDependent ? fCurrent : base.Current;

			public BusinessObject fCurrent;
			public bool fIsCurrentDependent;
		}

		sealed class TestHelperCusUnderbondUserControl : CusUnderbondUserControl
		{
			public override void SetBindPrepend(ZString bindPrepend)
			{
				base.SetBindPrepend(bindPrepend);
				SetBindPrependCalled = true;
			}

			public bool SetBindPrependCalled;
		}

		sealed class TestHelperCusUnderbond : CusUnderbond
		{
			public TestHelperCusUnderbond(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool VoyageAndVesselDetailsVisible => !C4_ModeOfMovement.IsEmpty;

			public void FireCanDoOutturnChangedEventPublic() => FireCanDoOutturnChangedEvent();

			protected override bool GetCanDoOutturn() => CanDoOutturnResult;

			public bool CanDoOutturnResult;
		}

		sealed class AirObjectTestHelper : DummyCusUnderbondUnionCollectionParent
		{
			public AirObjectTestHelper(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsForAirCargoCore() => true;
		}

		sealed class SeaObjectTestHelper : DummyCusUnderbondUnionCollectionParent
		{
			public SeaObjectTestHelper(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsForAirCargoCore() => false;
		}

		sealed class CusUnderbondPluginTestHelper : CusUnderbondPlugin
		{
			public CusUnderbondPluginTestHelper(ICusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend, string pluginName, Type typeOfCurrent) : base(hostEntity, bindPrepend, pluginName, typeOfCurrent)
			{
			}

			protected override void RemoveColumnsForAir(CusUnderbondUserControl control)
			{
				base.RemoveColumnsForAir(control);
				RemoveColumnsForAirHasBeenHit = true;
			}

			protected override void RemoveColumnsForSea(CusUnderbondUserControl control)
			{
				base.RemoveColumnsForSea(control);
				RemoveColumnsForSeaHasBeenHit = true;
			}

			public bool RemoveColumnsForAirHasBeenHit;
			public bool RemoveColumnsForSeaHasBeenHit;
		}
	}
}
