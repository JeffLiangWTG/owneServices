using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(OrganisationsControlBag))]
sealed class OrganisationsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(OrganisationsControlBag.IntracomReceiverAddressControl);
			yield return nameof(OrganisationsControlBag.DefermentPartyDocAddressControl);
			yield return nameof(OrganisationsControlBag.ExporterDocAddressControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => OrganisationsControlBag.Instance;
}
