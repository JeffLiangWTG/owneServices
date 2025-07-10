using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(BillLayout))]
sealed class BillLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (BillControlBag.Instance.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfAcceptancePanel, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfLoadingPanel, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfUnloadingPanel, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfDeliveryPanel, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (BillControlBag.Instance.ImportProcedureDropEdit, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.ExportProcedureDropEdit, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.ForwarderAddressControl, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.EmailAddressControl, ControlWidthClass.LongNoCaption);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

	public void TestForwarderAddressControlCaption()
	{
		var bill = Factory.New<AsycudaBill>();

		LayoutForTesting.TryGetCaption(CommonBillControlBag.Instance.ForwarderAddressControl, bill, out var resourceStringData);
		AssertEquals("ForwarderAddressControl caption", "Representative", resourceStringData.Caption);
	}
}
