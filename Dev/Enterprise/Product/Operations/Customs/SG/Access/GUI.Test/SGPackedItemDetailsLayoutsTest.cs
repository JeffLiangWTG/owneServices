using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(SGPackedItemDetailsLayouts))]
	sealed class SGPackedItemDetailsLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();
			var previousValue = Env.Registry.ExternalBorderComplianceTool;
			using (var form = new ManifestForm(manifest))
			using (new DisposableAction(() => Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None, () => Env.Registry.ExternalBorderComplianceTool = previousValue))
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var asycudaPackedItemsUserControl = asycudaPackUserControl.FindSingle<AsycudaPackedItemsUserControl>(c => c.Name == "AsycudaPackedItemsUserControl");
				var tariffFindBox = asycudaPackedItemsUserControl.Controls.Find("tariffFindBox", true).First();
				var goodsOriginCodeFindBox = asycudaPackedItemsUserControl.Controls.Find("GoodsOriginCodeFindBox", true).First();
				var goodsTypeDropEdit = asycudaPackedItemsUserControl.Controls.Find("GoodsTypeDropEdit", true).First();
				var goodsDescriptionTextBox = asycudaPackedItemsUserControl.Controls.Find("GoodsDescriptionTextBox", true).First();
				var customsValueCalcEdit = asycudaPackedItemsUserControl.Controls.Find("CustomsValueCalcEdit", true).First();
				var taxAmountCalcEdit = asycudaPackedItemsUserControl.Controls.Find("TaxAmountCalcEdit", true).First();
				var dutyAmountCalcEdit = asycudaPackedItemsUserControl.Controls.Find("DutyAmountCalcEdit", true).First();
				var customEntriesSeparatorUserControl = asycudaPackedItemsUserControl.Controls.Find("CustomEntriesSeparatorUserControl", true).First();
				var customEntriesGrid = asycudaPackedItemsUserControl.Controls.Find("CustomEntriesGrid", true).First();
				var gstPaidDropEdit = asycudaPackedItemsUserControl.Controls.Find("GSTPaidDropEdit", true).First();
				AssertEquals(true, tariffFindBox.Visible);
				AssertEquals("GSTPaidDropEdit", true, gstPaidDropEdit.Visible);
				AssertEquals(true, asycudaPackedItemsUserControl.FindSingle<ZCalcDropEdit>(nameof(CommonPackedItemDetailsControlBag.CustomsQtyCalcDropEdit)).Visible);
				AssertEquals(true, goodsOriginCodeFindBox.Visible);
				AssertEquals(true, goodsTypeDropEdit.Visible);
				AssertEquals(true, goodsDescriptionTextBox.Visible);
				AssertEquals(true, customsValueCalcEdit.Visible);
				AssertEquals(true, taxAmountCalcEdit.Visible);
				AssertEquals(true, dutyAmountCalcEdit.Visible);
				AssertEquals(true, asycudaPackedItemsUserControl.FindSingle<ZTextBox>(nameof(CommonPackedItemDetailsControlBag.MessageStatusTextBox)).Visible);
				AssertEquals(true, asycudaPackedItemsUserControl.FindSingle<ZTextBox>(nameof(CommonPackedItemDetailsControlBag.PackStatusTextBox)).Visible);
				AssertEquals(true, customEntriesSeparatorUserControl.Visible);
				AssertEquals(true, customEntriesGrid.Visible);
				manifest.AMA_ManifestType = Constants.ManifestType.Export;
				AssertEquals("GSTPaidDropEdit", false, gstPaidDropEdit.Visible);
			}
		}

		[TestDate(2022, 12, 31)]
		public void TestVisibility_OVRInactive()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();
			var previousValue = Env.Registry.ExternalBorderComplianceTool;
			using (var form = new ManifestForm(manifest))
			using (new DisposableAction(() => Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None, () => Env.Registry.ExternalBorderComplianceTool = previousValue))
			using (SGAccessRegistry.Instance.OVRLiveEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var asycudaPackedItemsUserControl = asycudaPackUserControl.FindSingle<AsycudaPackedItemsUserControl>(c => c.Name == "AsycudaPackedItemsUserControl");
				var gstPaidDropEdit = asycudaPackedItemsUserControl.Controls.Find("GSTPaidDropEdit", true).First();
				AssertEquals("GSTPaidDropEdit", true, gstPaidDropEdit.Visible);
				manifest.AMA_ManifestType = Constants.ManifestType.Export;
				AssertEquals("GSTPaidDropEdit", false, gstPaidDropEdit.Visible);
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PackedItemDetailsLayoutBuilder<AsycudaPack>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Medium);
				yield return (SGPackedItemDetailsControlBag.Instance.SGEdiTariffFindBox, ControlWidthClass.Auto);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Long);
				yield return (SGPackedItemDetailsControlBag.Instance.GoodsTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (SGPackedItemDetailsControlBag.Instance.GSTPaidDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsValueCalcEdit, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.TaxAmountCalcEdit, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.DutyAmountCalcEdit, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Auto);
				yield return (CommonPackedItemDetailsControlBag.Instance.PackStatusTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomEntriesSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomEntriesGrid, ControlWidthClass.Long);
			}
		}
	}
}
