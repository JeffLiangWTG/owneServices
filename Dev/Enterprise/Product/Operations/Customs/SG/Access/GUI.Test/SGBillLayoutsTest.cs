using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(SGBillLayouts))]
	sealed class SGBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = Constants.ManifestType.Import;
			manifest.Bills.RemoveAndDeleteAll(); // Ensure that controls are visible even without a bill
			using (var form = new ManifestForm(manifest))
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingle<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");
				var sgPartyIDTextBox = asycudaBillUserControl.Controls.Find("SGPartyIDTextBox", true).First();
				var sgPartyStatusDropEdit = asycudaBillUserControl.Controls.Find("SGPartyStatusDropEdit", true).First();
				var sgPayeeIndicatorDropEdit = asycudaBillUserControl.Controls.Find("SGPayeeIndicatorDropEdit", true).First();
				var sgGstAmountCalcEdit = asycudaBillUserControl.Controls.Find("SGGstAmountCalcEdit", true).First();
				var sgDutyAmountCalcEdit = asycudaBillUserControl.Controls.Find("SGDutyAmountCalcEdit", true).First();
				var cycleNumberDropEditWithFixedWidthCycleNumber = asycudaBillUserControl.FindSingle<ZDropEditWithFixedWidth>(nameof(SGBillControlBag.CycleNumberDropEditWithFixedWidth));
				var cycleDateDateEdit = asycudaBillUserControl.Controls.Find("CycleDateDateEdit", true).First();
				var messageStatusTextBox = asycudaBillUserControl.Controls.Find("MessageStatusTextBox", true).First();
				var gstnReferenceNoTextBox = asycudaBillUserControl.Controls.Find("GSTNReferenceNoTextBox", true).First();
				AssertEquals("SGPartyIDTextBox", true, sgPartyIDTextBox.Visible);
				AssertEquals("SGPartyStatusDropEdit", true, sgPartyStatusDropEdit.Visible);
				AssertEquals("SGPayeeIndicatorDropEdit", true, sgPayeeIndicatorDropEdit.Visible);
				AssertEquals("SGGstAmountCalcEdit", true, sgGstAmountCalcEdit.Visible);
				AssertEquals("SGDutyAmountCalcEdit", true, sgDutyAmountCalcEdit.Visible);
				AssertEquals("CycleNumberDropEditWithFixedWidthCycleNumber", true, cycleNumberDropEditWithFixedWidthCycleNumber.Visible);
				AssertEquals("CycleDateDateEdit", true, cycleDateDateEdit.Visible);
				AssertEquals("MessageStatusTextBox", true, messageStatusTextBox.Visible);
				AssertEquals("GSTNReferenceNoTextBox", true, gstnReferenceNoTextBox.Visible);
				manifest.AMA_ManifestType = Constants.ManifestType.Export;
				AssertEquals("SGPartyIDTextBox", true, sgPartyIDTextBox.Visible);
				AssertEquals("SGPartyStatusDropEdit", false, sgPartyStatusDropEdit.Visible);
				AssertEquals("SGPayeeIndicatorDropEdit", false, sgPayeeIndicatorDropEdit.Visible);
				AssertEquals("SGGstAmountCalcEdit", false, sgGstAmountCalcEdit.Visible);
				AssertEquals("SGDutyAmountCalcEdit", false, sgDutyAmountCalcEdit.Visible);
				AssertEquals("CycleNumberDropEditWithFixedWidthCycleNumber", false, cycleNumberDropEditWithFixedWidthCycleNumber.Visible);
				AssertEquals("CycleDateDateEdit", false, cycleDateDateEdit.Visible);
				AssertEquals("MessageStatusTextBox", true, messageStatusTextBox.Visible);
				AssertEquals("GSTNReferenceNoTextBox", false, gstnReferenceNoTextBox.Visible);
			}
		}

		[TestDate(2022, 12, 31)]
		public void TestVisibility_OVRInactive()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = Constants.ManifestType.Import;
			using (var form = new ManifestForm(manifest))
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingle<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");
				var gstnReferenceNoTextBox = asycudaBillUserControl.Controls.Find("GSTNReferenceNoTextBox", true).First();
				AssertEquals("GSTNReferenceNoTextBox", false, gstnReferenceNoTextBox.Visible);
				manifest.AMA_ManifestType = Constants.ManifestType.Export;
				AssertEquals("GSTNReferenceNoTextBox", false, gstnReferenceNoTextBox.Visible);
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
				yield return (SGBillControlBag.Instance.MessageStatusDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CusJobNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (SGBillControlBag.Instance.SGPartyIDTextBox, ControlWidthClass.Long);
				yield return (SGBillControlBag.Instance.SGPartyStatusDropEdit, ControlWidthClass.Auto);
				yield return (SGBillControlBag.Instance.SGPayeeIndicatorDropEdit, ControlWidthClass.Auto);
				yield return (SGBillControlBag.Instance.SGGstAmountCalcEdit, ControlWidthClass.Auto);
				yield return (SGBillControlBag.Instance.SGDutyAmountCalcEdit, ControlWidthClass.Auto);
				yield return (SGBillControlBag.Instance.CycleDateDateEdit, ControlWidthClass.Auto);
				yield return (SGBillControlBag.Instance.CycleNumberDropEditWithFixedWidth, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (SGBillControlBag.Instance.GSTNReferenceNoTextBox, ControlWidthClass.Long);
			}
		}
	}
}
