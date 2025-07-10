using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsControlBag))]
	sealed class CompanyCredentialsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(CompanyCredentialsControlBag.CompanyCredentialsDetailsUserControl));
			}
		}

		protected override ControlBag GetControlBagForTesting() => CompanyCredentialsControlBag.Instance;
	}
}
