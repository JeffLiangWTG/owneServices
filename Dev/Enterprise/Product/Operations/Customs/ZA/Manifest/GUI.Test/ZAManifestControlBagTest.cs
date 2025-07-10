using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	[TestedType(typeof(ZAManifestControlBag))]
	sealed class ZAManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ZAManifestControlBag.PlaceOfEntryDropEdit);
				yield return nameof(ZAManifestControlBag.PlaceOfExitDropEdit);
				yield return nameof(ZAManifestControlBag.DateAtCustomsOfficeDateEdit);
				yield return nameof(ZAManifestControlBag.VesselCodeFindBox);
				yield return nameof(ZAManifestControlBag.RadioCallSignCodeFindBox);
				yield return nameof(ZAManifestControlBag.EstLoadDateEdit);
				yield return nameof(ZAManifestControlBag.MasterCarrierCodeTextBox);
				yield return nameof(ZAManifestControlBag.SeparatorTextUserControl);
				yield return nameof(ZAManifestControlBag.VoyageFlightTextBox);
				yield return nameof(ZAManifestControlBag.TssVesselCodeFindBox);
				yield return nameof(ZAManifestControlBag.TssRadioCallSignCodeFindBox);
				yield return nameof(ZAManifestControlBag.TssCargoCarrierCodeCodeFindBox);
				yield return nameof(ZAManifestControlBag.DateOfDepartureDateEdit);
				yield return nameof(ZAManifestControlBag.CaseNumberManifestHeaderGroupBox);
				yield return nameof(ZAManifestControlBag.CallPurposeCodeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ZAManifestControlBag.Instance;
	}
}
