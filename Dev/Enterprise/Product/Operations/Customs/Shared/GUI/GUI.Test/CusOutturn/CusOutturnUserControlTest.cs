using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CusOutturnUserControlTest : TestCaseWithFactory
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestBinding()
		{
			var underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
			TestBind(underbond, ZString.Empty);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestBindingWithBindPrepend()
		{
			var dummy = Factory.New<DummyBizoWithUnderbondCollection>();
			TestBind(dummy, "Underbonds.");
		}

		public void TestGridId()
		{
			using (var control = new CusOutturnUserControl())
			{
				AssertEquals("GridLayoutDGu8fdRQgEtBCLuEeuYwuA==", control.OutturnsGrid.GridId);
			}
		}

		public void TestSetBindPrepend()
		{
			using (var control = new CusOutturnUserControl())
			{
				control.InitializeComponent();
				control.SetBindPrepend("XXX.");
				Assert("StatusTextBox.BindTo prepended", control.StatusTextBox.BindTo.StartsWith("XXX."));
				Assert("DateTimeOfOutturnDateEdit.BindTo prepended", control.DateTimeOfOutturnDateEdit.BindTo.StartsWith("XXX."));
				Assert("DateTimeOfUnloadDateEdit.BindTo prepended", control.DateTimeOfUnloadDateEdit.BindTo.StartsWith("XXX."));
				Assert("zGrid1.BindTo prepended", control.OutturnsGrid.BindTo.StartsWith("XXX."));
			}
		}

		public void TestSearchOfOutturns()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.GetIsUnderbondForSeaShipmentResult = false;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusOutturn outturnLine1 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine2 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine3 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine4 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine5 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine6 = underbond1.Outturns.AddNew();
			outturnLine1.C5_HouseBill = "Default";
			outturnLine2.C5_HouseBill = "CuckooSqueaker";
			outturnLine3.C5_HouseBill = "Default";
			outturnLine4.C5_HouseBill = "Default";
			outturnLine5.C5_HouseBill = "Gibbiceps";
			outturnLine6.C5_HouseBill = "Default";
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				using (CusUnderbondUserControl underbondControl = new CusUnderbondUserControl())
				{
					underbondControl.DisableVisibilityCheck();
					underbondControl.SetBindPrepend("");
					underbondControl.Parent = form;
					underbondControl.SetDataBinding(collection, "");
					form.Show();
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;
					underbondControl.DetailsUserControl.MainTabControl.SelectedTab = underbondControl.DetailsUserControl.OutturnTabPage;
					CusOutturnUserControl outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;
					outturnControl.OutturnSearchTextBox.Text = "Cuckoo";
					AssertEquals(1, outturnControl.OutturnsGrid.CurrentRowIndex);
					outturnControl.OutturnSearchTextBox.Text = "Gibb";
					AssertEquals(4, outturnControl.OutturnsGrid.CurrentRowIndex);
				}
			}
		}

		public void TestSeaOnlyControlsVisibility()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.GetIsUnderbondForSeaShipmentResult = false;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.GetIsUnderbondForSeaShipmentResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				using (CusUnderbondUserControl underbondControl = new CusUnderbondUserControl())
				{
					underbondControl.DisableVisibilityCheck();
					underbondControl.SetBindPrepend("");
					form.Controls.Add(underbondControl);
					underbondControl.SetDataBinding(collection, "");
					form.Show();
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;
					underbondControl.DetailsUserControl.MainTabControl.SelectedTab = underbondControl.DetailsUserControl.OutturnTabPage;
					CusOutturnUserControl outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;
					AssertSeaControlsVisibility(outturnControl, false);
					underbondControl.UnderbondsGrid.CurrentRowIndex = 1;
					outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;
					AssertSeaControlsVisibility(outturnControl, true);
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;
					AssertSeaControlsVisibility(outturnControl, false);
				}
			}
		}

		public void TestSeaColumns()
		{
			using (CusOutturnUserControl control = new CusOutturnUserControl())
			{
				List<string> seaColumns = new List<string>(control.SeaColumns);
				AssertEquals(10, seaColumns.Count);
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_SealIntactIndicator));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_ContainerNumber));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_HouseBill));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_MasterBill));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_OuterPackUnits));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_CargoReceiptDate));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_CargoUnpackDate));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_ReceiptOnlyIndicator));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_CargoType));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_PackagesUnits));
			}
		}

		public void TestRemoveColumnFromGrid()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.GetIsUnderbondForSeaShipmentResult = false;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.GetIsUnderbondForSeaShipmentResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				using (CusUnderbondUserControl underbondControl = new CusUnderbondUserControl())
				{
					underbondControl.DisableVisibilityCheck();
					underbondControl.SetBindPrepend("");
					underbondControl.Parent = form;
					underbondControl.SetDataBinding(collection, "");
					form.Show();
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;
					underbondControl.DetailsUserControl.MainTabControl.SelectedTab = underbondControl.DetailsUserControl.OutturnTabPage;
					CusOutturnUserControl outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;
					AssertNotNull(outturnControl.OutturnsGrid.Columns[CusOutturnSchema.Constants.C5_OuterPacks]);
					underbondControl.UnderbondsGrid.CurrentRowIndex = 1;
					outturnControl.RemoveColumnFromGrid(CusOutturnSchema.Constants.C5_OuterPacks);
					AssertNull(outturnControl.OutturnsGrid.Columns[CusOutturnSchema.Constants.C5_OuterPacks]);
				}
			}
		}

		void AssertSeaControlsVisibility(CusOutturnUserControl outturnControl, bool expectedVisibility)
		{
			AssertEquals("DateTimeOfUnloadDateEdit.Visible", expectedVisibility, outturnControl.DateTimeOfUnloadDateEdit.Visible);
			AssertEquals("DateTimeOfUnloadLabel.Visible", expectedVisibility, outturnControl.DateTimeOfUnloadLabel.Visible);
			foreach (string column in outturnControl.SeaColumns)
			{
				if (column != CusOutturnSchema.Constants.C5_ReceiptOnlyIndicator)
				{
					AssertEquals(column + " column", expectedVisibility, outturnControl.OutturnsGrid.Columns[column] != null);
				}
				else
				{
					AssertEquals(false, outturnControl.OutturnsGrid.Columns[column] != null);
				}
			}
		}

		void TestBind(BusinessObject bizo, ZString bindPrepend)
		{
			using (var form = new ZForm(bizo))
			using (var control = new CusOutturnUserControl())
			{
				control.Parent = form;
				if (!bindPrepend.IsEmpty)
				{
					control.SetBindPrepend(bindPrepend);
				}

				form.Show();
				control.SetDataBinding(bizo, "");
			}
		}

		sealed class TestHelperCusUnderbond : CusUnderbond
		{
			public TestHelperCusUnderbond(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool GetIsUnderbondForSeaShipmentResult;

			protected override bool GetIsUnderbondForSeaShipment() => GetIsUnderbondForSeaShipmentResult;

			protected override bool GetCanDoOutturn() => true;

			protected override TypeLoaderCollection GetParentLoaders()
			{
				var result = base.GetParentLoaders();
				result.Add(new TypeLoader(typeof(DummyBizoWithUnderbondCollection)));
				return result;
			}
		}
	}
}
