using System.Collections;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CusUnderbondUserControlTest : TestCaseWithFactory
	{
		public void TestVisibilityCheck()
		{
			using (CusUnderbondUserControl control = new CusUnderbondUserControl())
			{
				AssertEquals(false, control.visibilitySelected);
				control.RemoveColumnsForAir();
				AssertEquals(true, control.visibilitySelected);
				control.visibilitySelected = false;
				AssertEquals(false, control.visibilitySelected);
				control.RemoveColumnsForSea();
				AssertEquals(true, control.visibilitySelected);
				control.visibilitySelected = false;
				AssertEquals(false, control.visibilitySelected);
				control.DisableVisibilityCheck();
				AssertEquals(true, control.visibilitySelected);
			}
		}

		public void TestOutturnTabVisibility()
		{
			ZString ourPremiseID = "9914N";
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = ourPremiseID;
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.RemoveColumnsForAir();
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				form.Show();
				control.CreateNewUnderbondButtonInternal.PerformClick();
				AssertNotNull(control.DetailsUserControl.CurrentUnderbond);
			}
		}

		public void TestUnderbondButtonFocusChanged()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.CanDoOutturnResult = true;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.CanDoOutturnResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.OutturnDisabled = true;
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				control.DisableVisibilityCheck();
				form.Show();
				AssertEquals(false, control.DetailsUserControl.DestinationIDTextBox.Focused);
				control.CreateNewUnderbondButtonInternal.PerformClick();
				AssertEquals("PIRATES! ARR!", true, control.DetailsUserControl.DestinationIDTextBox.Focused);
			}
		}

		public void TestOutturnTabDisabled()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.CanDoOutturnResult = true;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.CanDoOutturnResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.OutturnDisabled = true;
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				control.DisableVisibilityCheck();
				form.Show();
				control.UnderbondsGrid.CurrentRowIndex = 0;
				//Control.zGrid1.CurrentCell = new DataGridCell(1, 0);
				int originalIndex = control.DetailsUserControl.MainTabControl.TabPages.IndexOf(control.DetailsUserControl.OutturnTabPage);
				AssertEquals("Visible", false, control.DetailsUserControl.MainTabControl.Contains(control.DetailsUserControl.OutturnTabPage));
				control.UnderbondsGrid.CurrentRowIndex = 1;
				AssertEquals("Visible", false, control.DetailsUserControl.MainTabControl.Contains(control.DetailsUserControl.OutturnTabPage));
				underbond2.CanDoOutturnResult = true;
				underbond2.FireCanDoOutturnChangedEventPublic();
				AssertEquals("Visible", false, control.DetailsUserControl.MainTabControl.Contains(control.DetailsUserControl.OutturnTabPage));
				AssertEquals("Index", originalIndex, control.DetailsUserControl.MainTabControl.TabPages.IndexOf(control.DetailsUserControl.OutturnTabPage));
			}
		}

		public void TestOutturnTabBindingContextIssue()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.CanDoOutturnResult = true;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.CanDoOutturnResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				control.DisableVisibilityCheck();
				form.Show();
				control.UnderbondsGrid.CurrentRowIndex = 0;
				control.UnderbondsGrid.CurrentRowIndex = 1;
				underbond2.CanDoOutturnResult = false;
				underbond2.FireCanDoOutturnChangedEventPublic();
				AssertBindingContextOnControls(control.DetailsUserControl.OutturnTabPage, control.BindingContext);
				control.UnderbondsGrid.CurrentRowIndex = 0;
			}
		}

		public void TestGetProviderToAddUnderbondTo()
		{
			using (TestCusUnderbondUserControl control = new TestCusUnderbondUserControl())
			{
				ICusUnderbondDependentCollectionParent uB = Factory.New<DummyBizoWithUnderbondCollection>();
				ICusUnderbondDependentCollectionParent uB2 = Factory.New<DummyBizoWithUnderbondCollection>();
				ArrayList testArrayList = new ArrayList();
				AssertEquals(null, control.GetProviderToAddUnderbondTo((ICusUnderbondDependentCollectionParent[])testArrayList.ToArray(typeof(ICusUnderbondDependentCollectionParent))));
				AssertEquals("An underbond movement cannot be created, as there are no valid items for which one can be created.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Dialog should not be shown for 0 items.", !control.DialogShown);
				testArrayList.Add(uB);
				AssertEquals(uB, control.GetProviderToAddUnderbondTo((ICusUnderbondDependentCollectionParent[])testArrayList.ToArray(typeof(ICusUnderbondDependentCollectionParent))));
				Assert("Dialog should not be shown for 1 item.", !control.DialogShown);
				testArrayList.Add(uB2);
				AssertEquals(uB, control.GetProviderToAddUnderbondTo((ICusUnderbondDependentCollectionParent[])testArrayList.ToArray(typeof(ICusUnderbondDependentCollectionParent))));
				Assert("Dialog should not be shown for more than 1 item.", control.DialogShown);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestBinding()
		{
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			TestBind(dummy);
		}

		public void TestSetBindPrepend()
		{
			using (CusUnderbondUserControl control = new CusUnderbondUserControl())
			{
				control.InitializeComponentInternal();
				control.SetBindPrepend("XXX.");
				Assert("UnderbondsGrid.BindTo prepended", control.UnderbondsGrid.BindTo.StartsWith("XXX."));
			}
		}

		public void TestAirVisibility()
		{
			AirObjectTestHelper airObject = Factory.New<AirObjectTestHelper>();
			using (ZForm form1 = new ZForm(airObject))
			{
				using (CusUnderbondUserControl control1 = new CusUnderbondUserControl())
				{
					form1.Controls.Add(control1);
					control1.SetBindPrepend("");
					control1.SetDataBinding(form1.BusinessEntity, "");
					control1.RemoveColumnsForAir();
					form1.Show();
					AssertNull("C4_DateOfArrivalIntoDestinationPremise should not be shown for Air", control1.UnderbondsGrid.Columns[CusUnderbondSchema.C4_DateOfArrivalIntoDestinationPremise.Name]);
					AssertNull("C4_UnderbondBySeaLloydsIMONum should not be shown for Air", control1.UnderbondsGrid.Columns[CusUnderbondSchema.C4_UnderbondBySeaLloydsIMONum.Name]);
					AssertNull("C4_UnderbondBySeaVoyage should not be shown for Air", control1.UnderbondsGrid.Columns[CusUnderbondSchema.C4_UnderbondBySeaVoyage.Name]);
					AssertNull("C4_UnderbondBySeaVessel should not be shown for Air", control1.UnderbondsGrid.Columns[CusUnderbondSchema.C4_UnderbondBySeaVessel.Name]);
					AssertEquals("Should Not be shown for Air", true, control1.DetailsUserControl.DetailsTabControl.TabPages.Contains(control1.DetailsUserControl.PartShipmentTabPage));
				}
			}

			DummyCusUnderbondUnionCollectionParent dummySea = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			using (ZForm form2 = new ZForm(dummySea))
			{
				using (CusUnderbondUserControl control2 = new CusUnderbondUserControl())
				{
					form2.Controls.Add(control2);
					control2.SetBindPrepend("");
					control2.SetDataBinding(form2.BusinessEntity, "");
					control2.RemoveColumnsForSea();
					form2.Show();
					AssertNotNull("C4_DateOfArrivalIntoDestinationPremise should be shown for Sea", control2.UnderbondsGrid.Columns[CusUnderbondSchema.C4_DateOfArrivalIntoDestinationPremise.Name]);
					AssertNotNull("C4_UnderbondBySeaLloydsIMONum should be shown for Sea", control2.UnderbondsGrid.Columns[CusUnderbondSchema.C4_UnderbondBySeaLloydsIMONum.Name]);
					AssertNotNull("C4_UnderbondBySeaVoyage should be shown for Sea", control2.UnderbondsGrid.Columns[CusUnderbondSchema.C4_UnderbondBySeaVoyage.Name]);
					AssertNotNull("C4_UnderbondBySeaVessel should be shown for Sea", control2.UnderbondsGrid.Columns[CusUnderbondSchema.C4_UnderbondBySeaVessel.Name]);
					AssertEquals("Should not be shown for sea", false, control2.DetailsUserControl.DetailsTabControl.TabPages.Contains(control2.DetailsUserControl.PartShipmentTabPage));
				}
			}
		}

		public void TestOutturnTabPageVisibility()
		{
			var dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.CanDoOutturnResult = true;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.CanDoOutturnResult = false;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				control.DisableVisibilityCheck();
				form.Show();
				control.UnderbondsGrid.CurrentRowIndex = 0;
				//Control.zGrid1.CurrentCell = new DataGridCell(1, 0);
				int originalIndex = control.DetailsUserControl.MainTabControl.TabPages.IndexOf(control.DetailsUserControl.OutturnTabPage);
				AssertEquals("Visible", true, control.DetailsUserControl.MainTabControl.Contains(control.DetailsUserControl.OutturnTabPage));
				control.DetailsUserControl.MainTabControl.SelectedTab = control.DetailsUserControl.OutturnTabPage;
				Assert("However covering label should be visible with explanation", !control.DetailsUserControl.CoveringLabel.Visible);
				control.UnderbondsGrid.CurrentRowIndex = 1;
				AssertEquals("Visible", true, control.DetailsUserControl.MainTabControl.Contains(control.DetailsUserControl.OutturnTabPage));
				Assert("However covering label should be visible with explanation", control.DetailsUserControl.CoveringLabel.Visible);
				underbond2.CanDoOutturnResult = true;
				underbond2.FireCanDoOutturnChangedEventPublic();
				AssertEquals("Visible", true, control.DetailsUserControl.MainTabControl.Contains(control.DetailsUserControl.OutturnTabPage));
				AssertEquals("Index", originalIndex, control.DetailsUserControl.MainTabControl.TabPages.IndexOf(control.DetailsUserControl.OutturnTabPage));
				Assert("covering label should NOT be visible with explanation", !control.DetailsUserControl.CoveringLabel.Visible);
			}
		}

		void TestBind(BusinessObject bizo)
		{
			using (ZForm form = new ZForm(bizo))
			{
				using (CusUnderbondUserControl control = new CusUnderbondUserControl())
				{
					control.DisableVisibilityCheck();
					control.Parent = form;
					control.SetBindPrepend("");
					form.Show();
				}
			}
		}

		void AssertBindingContextOnControls(Control parentControl, BindingContext expectedBindingContext)
		{
			foreach (Control controlToCheck in parentControl.Controls)
			{
				AssertEquals("binding context differs on " + controlToCheck.ToString(), expectedBindingContext, controlToCheck.BindingContext);
			}
		}

		sealed class TestCusUnderbondUserControl : CusUnderbondUserControl
		{
			protected override bool NewCusUnderbondDialogResultWithoutDispose(NewCusUnderbondDialog dialog)
			{
				dialog.Show();
				DialogShown = true;
				return true;
			}

			public bool DialogShown;
		}

		internal class TestHelperCusUnderbond : CusUnderbond
		{
			public TestHelperCusUnderbond(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool VoyageAndVesselDetailsVisible => !C4_ModeOfMovement.IsEmpty;

			protected override bool TranshipmentPortVisibleCore() => C4_MovementReason == "FOO";

			public void FireCanDoOutturnChangedEventPublic()
			{
				FireCanDoOutturnChangedEvent();
			}

			protected override bool GetCanDoOutturn() => CanDoOutturnResult;

			public bool CanDoOutturnResult;

			protected override TypeLoaderCollection GetParentLoaders()
			{
				var result = base.GetParentLoaders();
				result.Add(new TypeLoader(typeof(DummyBizoWithUnderbondCollection)));
				return result;
			}
		}

		sealed class AirObjectTestHelper : DummyCusUnderbondUnionCollectionParent
		{
			public AirObjectTestHelper(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsForAirCargoCore() => true;
		}
	}
}
