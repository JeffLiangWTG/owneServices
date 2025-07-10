using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CusUnderbondDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestNillOutturnPerformerReturnsSomeSortOfMessage()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			CusUnderbondUserControlTest.TestHelperCusUnderbond underbond1 = (CusUnderbondUserControlTest.TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(CusUnderbondUserControlTest.TestHelperCusUnderbond));
			underbond1.CanDoOutturnResult = true;
			DummyParent dummy = Factory.New<DummyParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.OutturnDisabled = false;
				control.SetUnderbondParent(dummy);
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				control.DisableVisibilityCheck();
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.DetailsUserControl.NilOutturnButton_Click(null, null);
			}

			AssertEquals("Cuckoo Squeakers are great", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestNillOutturnWhenNoUnderbondDoesNotCauseException()
		{
			DummyParent dummy = Factory.New<DummyParent>();
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.OutturnDisabled = false;
				control.SetUnderbondParent(dummy);
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				control.DisableVisibilityCheck();
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.DetailsUserControl.NilOutturnButton_Click(null, null);
			}

			AssertEquals("There is no Underbond to outturn.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestVesselVoyageVisibilityAndDischargeVisibility()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			CusUnderbondUserControlTest.TestHelperCusUnderbond underbond1 = (CusUnderbondUserControlTest.TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(CusUnderbondUserControlTest.TestHelperCusUnderbond));
			underbond1.C4_ModeOfMovement = "XXX";
			CusUnderbondUserControlTest.TestHelperCusUnderbond underbond2 = (CusUnderbondUserControlTest.TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(CusUnderbondUserControlTest.TestHelperCusUnderbond));
			underbond2.C4_ModeOfMovement = ZString.Empty;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.DisableVisibilityCheck();
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				form.Show();
				control.UnderbondsGrid.CurrentRowIndex = 0;
				AssertVesselVoyageVisibility(control, true);
				control.UnderbondsGrid.CurrentRowIndex = 1;
				AssertVesselVoyageVisibility(control, false);
				underbond2.C4_ModeOfMovement = "XXX";
				AssertVesselVoyageVisibility(control, true);
				control.DetailsUserControl.DetailsTabControl.SelectedTab = control.DetailsUserControl.EstablishmentsTabPage;
				control.DetailsUserControl.IsMoveFromDischargeCheckBox.Checked = true;
				AssertDischargeVisibility(control, false);
				control.DetailsUserControl.IsMoveFromDischargeCheckBox.Checked = false;
				AssertDischargeVisibility(control, true);
			}
		}

		public void TestTranshipmentVisibilityForUsingTransPort()
		{
			DummyBizoWithUnderbondCollection dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			dummyWithUnderbonds.UsesTranshipmentPortOnUnderbond = true;
			CheckTranshipmentVisibility(dummyWithUnderbonds, true);
		}

		public void TestTranshipmentVisibilityForNotUsingTransPort()
		{
			DummyBizoWithUnderbondCollection dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			dummyWithUnderbonds.UsesTranshipmentPortOnUnderbond = false;
			CheckTranshipmentVisibility(dummyWithUnderbonds, false);
		}

		public void TestSetBindPrepend()
		{
			using (CusUnderbondDetailsUserControl control = new CusUnderbondDetailsUserControl())
			{
				control.InitializeComponent();
				control.SetBindPrepend("XXX.");
				Assert("OriginAddressControl.BindToAddress prepended", control.OriginAddressControl.BindToAddress.StartsWith("XXX."));
				Assert("OriginAddressControl.BindToOrgList prepended", control.OriginAddressControl.BindToOrgList.StartsWith("XXX."));
				Assert("DestinationAddressControl.BindToAddress prepended", control.DestinationAddressControl.BindToAddress.StartsWith("XXX."));
				Assert("DestinationAddressControl.BindToOrgList prepended", control.DestinationAddressControl.BindToOrgList.StartsWith("XXX."));
				Assert("DischargeAddressControl.BindToAddress prepended", control.DischargeAddressControl.BindToAddress.StartsWith("XXX."));
				Assert("DischargeAddressControl.BindToOrgList prepended", control.DischargeAddressControl.BindToOrgList.StartsWith("XXX."));
				Assert("OriginIDTextBox.BindTo prepended", control.OriginIDTextBox.BindTo.StartsWith("XXX."));
				Assert("DestinationIDTextBox.BindTo prepended", control.DestinationIDTextBox.BindTo.StartsWith("XXX."));
				Assert("DischargeIDTextBox.BindTo prepended", control.DischargeIDTextBox.BindTo.StartsWith("XXX."));
				Assert("SendersReferenceTextBox.BindTo prepended", control.SendersReferenceTextBox.BindTo.StartsWith("XXX."));
				Assert("VoyageTextBox.BindTo prepended", control.VoyageTextBox.BindTo.StartsWith("XXX."));
				Assert("MovementModeDropEdit.BindTo prepended", control.MovementModeDropEdit.BindTo.StartsWith("XXX."));
				Assert("MovementModeDropEdit.BindToList prepended", control.MovementModeDropEdit.BindToList.StartsWith("XXX."));
				Assert("IsMoveFromDischargeCheckBox.BindTo prepended", control.IsMoveFromDischargeCheckBox.BindTo.StartsWith("XXX."));
				Assert("UnderbondForDropEdit.BindTo prepended", control.UnderbondForDropEdit.BindTo.StartsWith("XXX."));
				Assert("UnderbondForDropEdit.BindToList prepended", control.UnderbondForDropEdit.BindToList.StartsWith("XXX."));
				Assert("VesselFindBox.BindTo prepended", control.VesselFindBox.BindTo.StartsWith("XXX."));
				Assert("VesselFindBox.BindToList prepended", control.VesselFindBox.BindToList.StartsWith("XXX."));
				Assert("RequestReasonDropEdit.BindTo prepended", control.RequestReasonDropEdit.BindTo.StartsWith("XXX."));
				Assert("RequestReasonDropEdit.BindToList prepended", control.RequestReasonDropEdit.BindToList.StartsWith("XXX."));
				Assert("MessageStatusTextBox.BindTo prepended", control.MessageStatusTextBox.BindTo.StartsWith("XXX."));
				Assert("CustomsStatusTextBox.BindTo prepended", control.CustomsStatusTextBox.BindTo.StartsWith("XXX."));
				Assert("MessageTextTextBox.BindTo prepended", control.MessageUserControl.MessageTextTextBox.BindTo.StartsWith("XXX."));
				Assert("MessageTextTextBox.BindTo prepended", control.MessageUserControl.MessagesGrid.BindTo.StartsWith("XXX."));
				Assert("TranshipmentCodeFindBox.BindTo prepended", control.TranshipmentCodeFindBox.BindTo.StartsWith("XXX."));
				Assert("TranshipmentCodeFindBox.BindToList prepended", control.TranshipmentCodeFindBox.BindToList.StartsWith("XXX."));
			}
		}

		void CheckTranshipmentVisibility(ICusUnderbondDependentCollectionParent dummyWithUnderbonds, bool visible)
		{
			CusUnderbondUserControlTest.TestHelperCusUnderbond underbond1 = (CusUnderbondUserControlTest.TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(CusUnderbondUserControlTest.TestHelperCusUnderbond));
			underbond1.C4_MovementReason = "FOO";
			AssertEquals("Precondition: Linked object", dummyWithUnderbonds, underbond1.LinkedObject);
			CusUnderbondUserControlTest.TestHelperCusUnderbond underbond2 = (CusUnderbondUserControlTest.TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(CusUnderbondUserControlTest.TestHelperCusUnderbond));
			underbond2.C4_MovementReason = ZString.Empty;
			AssertEquals("Precondition: Linked object", dummyWithUnderbonds, underbond2.LinkedObject);
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);
			using (ZForm form = new ZForm(dummy))
			{
				CusUnderbondUserControl control = new CusUnderbondUserControl();
				control.DisableVisibilityCheck();
				control.Parent = form;
				control.SetBindPrepend("");
				control.SetDataBinding(collection, "");
				form.Show();
				control.UnderbondsGrid.CurrentRowIndex = 0;
				AssertTranshipmentVisibility(control, visible);
				control.UnderbondsGrid.CurrentRowIndex = 1;
				AssertTranshipmentVisibility(control, false);
				underbond2.C4_MovementReason = "XXX";
				AssertTranshipmentVisibility(control, false);
				underbond2.C4_MovementReason = "FOO";
				AssertTranshipmentVisibility(control, visible);
			}
		}

		void AssertTranshipmentVisibility(CusUnderbondUserControl control, bool visible)
		{
			AssertEquals("TranshipmentLabel.Visible", visible, control.DetailsUserControl.TranshipmentLabel.Visible);
			AssertEquals("TranshipmentCodeFindBox.Visible", visible, control.DetailsUserControl.TranshipmentCodeFindBox.Visible);
		}

		void AssertDischargeVisibility(CusUnderbondUserControl control, bool visible)
		{
			AssertEquals("DischargeLabel.Visible", visible, control.DetailsUserControl.DischargeLabel.Visible);
			AssertEquals("DischargeAddressControl.Visible", visible, control.DetailsUserControl.DischargeAddressControl.Visible);
			AssertEquals("DischargeIDTextBox.Visible", visible, control.DetailsUserControl.DischargeIDTextBox.Visible);
		}

		void AssertVesselVoyageVisibility(CusUnderbondUserControl control, bool visible)
		{
			AssertEquals("UnderbondBySeaTabPage visible", visible, control.DetailsUserControl.DetailsTabControl.TabPages.Contains(control.DetailsUserControl.BySeaTabPage));
		}

		class DummyParent : DummyCusUnderbondUnionCollectionParent, ICusUnderbondNilUnderbondPerformer
		{
			public DummyParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZString ICusUnderbondNilUnderbondPerformer.PerformNilUnderbond(CusUnderbond underbond) => "Cuckoo Squeakers are great";
		}
	}
}
