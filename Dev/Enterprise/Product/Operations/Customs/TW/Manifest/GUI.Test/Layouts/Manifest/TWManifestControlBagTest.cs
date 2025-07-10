using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWManifestControlBag))]
	sealed class TWManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TWManifestControlBag.GoodsLocationCodeFindBox);
				yield return nameof(TWManifestControlBag.DeconsolidateVATTextBox);
				yield return nameof(TWManifestControlBag.LoginCompanyGuidFindBox);
				yield return nameof(TWManifestControlBag.MailBoxTextBox);
				yield return nameof(TWManifestControlBag.MessageStatusDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TWManifestControlBag.Instance;
	}
}
