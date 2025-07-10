using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	[TestedType(typeof(TRManifestControlBag))]
	sealed class TRManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TRManifestControlBag.TransportTypeDropEdit);
				yield return nameof(TRManifestControlBag.ManifestDescriptionTextBox);
				yield return nameof(TRManifestControlBag.TIRNumberTextBox);
				yield return nameof(TRManifestControlBag.InspectionClerkTextBox);
				yield return nameof(TRManifestControlBag.InternalInspectionNoTextBox);
				yield return nameof(TRManifestControlBag.TempStorageStartDateEdit);
				yield return nameof(TRManifestControlBag.TempStorageDueDateEdit);
				yield return nameof(TRManifestControlBag.PresentationCustomsOfficeDropEdit);
				yield return nameof(TRManifestControlBag.GlobalManifestStampDutyValueCalcEdit);
				yield return nameof(TRManifestControlBag.MasterBillStampDutyValueCalcEdit);
				yield return nameof(TRManifestControlBag.TotalStampDutyValueCalcEdit);
				yield return nameof(TRManifestControlBag.RegistrationDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TRManifestControlBag.Instance;
	}
}
