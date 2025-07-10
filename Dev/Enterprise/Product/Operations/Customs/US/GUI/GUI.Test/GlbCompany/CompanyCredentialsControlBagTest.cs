using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsControlBag))]
	sealed class CompanyCredentialsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(CompanyCredentialsControlBag.CredentialGroupBox));
			}
		}

		protected override ControlBag GetControlBagForTesting() => CompanyCredentialsControlBag.Instance;
	}
}
