using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportManifestControlBag))]
	sealed class USExportManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(USExportManifestControlBag.DeparturePortUNLOCOCodeFindBox);
				yield return nameof(USExportManifestControlBag.ScheduleDCodeFindBox);
				yield return nameof(USExportManifestControlBag.IssuerSCACUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting()
		{
			return USExportManifestControlBag.Instance;
		}
	}
}
