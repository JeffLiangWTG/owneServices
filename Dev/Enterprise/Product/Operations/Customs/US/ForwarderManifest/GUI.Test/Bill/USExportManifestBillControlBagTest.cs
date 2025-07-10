using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportManifestBillControlBag))]
	internal class USExportManifestBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(USExportManifestBillControlBag.BoardedQuantityCalcEdit);
				yield return nameof(USExportManifestBillControlBag.BoardedWeightCalcDropEdit);
				yield return nameof(USExportManifestBillControlBag.PriorTransportationModeDropEdit);
				yield return nameof(USExportManifestBillControlBag.FinalDestinationPortUserControl);
				yield return nameof(USExportManifestBillControlBag.ArrivalPortUserControl);
				yield return nameof(USExportManifestBillControlBag.DeparturePortUserControl);
				yield return nameof(USExportManifestBillControlBag.LadingPortUserControl);
				yield return nameof(USExportManifestBillControlBag.UnladingPortUserControl);
				yield return nameof(USExportManifestBillControlBag.OriginPortUserControl);
				yield return nameof(USExportManifestBillControlBag.SpecialCargoCodesDropEdit);
				yield return nameof(USExportManifestBillControlBag.PlaceOfReceiptTextBox);
				yield return nameof(USExportManifestBillControlBag.AESExemptionCodeTextBox);
				yield return nameof(USExportManifestBillControlBag.AESITNNumbersUserControl);
				yield return nameof(USExportManifestBillControlBag.InBondNumbersUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting()
		{
			return USExportManifestBillControlBag.Instance;
		}
	}
}
