using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(ACEManifestControlBag))]
	sealed class ACEManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ACEManifestControlBag.EstDateAtFirstArrivalDateEdit);
				yield return nameof(ACEManifestControlBag.BillStatusTextBox);
				yield return nameof(ACEManifestControlBag.BillStatusDescriptionTextBox);
				yield return nameof(ACEManifestControlBag.FIRMSTextBox);
				yield return nameof(ACEManifestControlBag.ExpressCourierCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ACEManifestControlBag.Instance;
	}
}
