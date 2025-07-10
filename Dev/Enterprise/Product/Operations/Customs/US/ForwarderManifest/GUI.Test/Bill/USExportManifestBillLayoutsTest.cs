using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportManifestBillLayouts))]
	class USExportManifestBillLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillIssuerCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.SpecialCargoCodesDropEdit, ControlWidthClass.Long);

				yield return (USExportManifestBillControlBag.Instance.AESITNNumbersUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.AESExemptionCodeTextBox, ControlWidthClass.Medium);
				yield return (USExportManifestBillControlBag.Instance.InBondNumbersUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.OriginPortUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.FinalDestinationPortUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.LadingPortUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.UnladingPortUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.DeparturePortUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.ArrivalPortUserControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.PlaceOfReceiptTextBox, ControlWidthClass.Long);

				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);

				yield return (USExportManifestBillControlBag.Instance.PriorTransportationModeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (USExportManifestBillControlBag.Instance.BoardedQuantityCalcEdit, ControlWidthClass.Medium);
				yield return (USExportManifestBillControlBag.Instance.BoardedWeightCalcDropEdit, ControlWidthClass.Medium);
			}
		}
		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new USExportManifestBillLayoutBuilder<USExportAsycudaBill>();
	}
}
