using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(MasterBillLayout))]
sealed class MasterBillLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
			yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfLoadingPanel, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfUnloadingPanel, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.PlaceOfDeliveryPanel, ControlWidthClass.Long);
			yield return (BillControlBag.Instance.EmailAddressControl, ControlWidthClass.LongNoCaption);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
}
