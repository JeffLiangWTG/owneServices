using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsControlBag))]
	sealed class CompanyCredentialsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(CompanyCredentialsControlBag.ForwarderCertificateUserControl));
				yield return (nameof(CompanyCredentialsControlBag.LicensingCertificateUserControl));
			}
		}

		protected override ControlBag GetControlBagForTesting() => CompanyCredentialsControlBag.Instance;
	}
}
