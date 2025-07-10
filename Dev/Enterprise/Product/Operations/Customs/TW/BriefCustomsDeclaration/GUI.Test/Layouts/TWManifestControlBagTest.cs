using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWManifestControlBag))]
	sealed class TWManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TWManifestControlBag.CustomsAgentCodeFindBox);
				yield return nameof(TWManifestControlBag.PersonGuidFindBox);
				yield return nameof(TWManifestControlBag.ImporterAddressUserControl);
				yield return nameof(TWManifestControlBag.ExporterAddressUserControl);
				yield return nameof(TWManifestControlBag.DeclarationDateEdit);
				yield return nameof(TWManifestControlBag.EntryNumberUserControl);
				yield return nameof(TWManifestControlBag.BagNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TWManifestControlBag.Instance;
	}
}
