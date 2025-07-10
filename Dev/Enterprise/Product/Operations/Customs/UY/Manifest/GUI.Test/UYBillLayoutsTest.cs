using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.GUI.Testing
{
	[TestedType(typeof(UYBillLayouts))]
	sealed class UYBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestUYSpecificFiedsVisibilty()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			var bill = manifest.Bills.AddNew();

			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");

				var transshipmentCheckBox = asycudaBillUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "TransshipmentCheckBox");
				var messageStatus = asycudaBillUserControl.FindSingle<ZTextBox>(nameof(CommonBillControlBag.MessageStatusTextBox));
				var customsEntryNumber = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(nameof(CommonBillControlBag.CustomsEntryNumberTextBox));
				var billStatus = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(nameof(CommonBillControlBag.BillStatusDropEdit));

				AssertEquals("TransshipmentCheckBox", true, transshipmentCheckBox.Visible);
				AssertEquals("MessageStatus", true, messageStatus.Visible);
				AssertEquals("CustomsEntryNumber", true, customsEntryNumber.Visible);
				AssertEquals("BillStatus", true, billStatus.Visible);

				AssertEquals("MessageStatus", true, messageStatus.ReadOnly);
				AssertEquals("CustomsEntryNumber", true, customsEntryNumber.ReadOnly);
				AssertEquals("BillStatus", true, billStatus.ReadOnly);
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
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (UYBillControlBag.Instance.TransshipmentCheckBox, ControlWidthClass.Auto);
			}
		}
	}
}
