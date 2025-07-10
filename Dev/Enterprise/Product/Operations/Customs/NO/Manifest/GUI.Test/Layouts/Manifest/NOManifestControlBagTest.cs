using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(NOManifestControlBag))]
sealed class NOManifestControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(NOManifestControlBag.DriverCommunicationIdTextBox);
			yield return nameof(NOManifestControlBag.DriverNameTextBox);
			yield return nameof(NOManifestControlBag.RepresentativeAddressControl);
			yield return nameof(NOManifestControlBag.ScheduledDateOfArrCustOfficeDateEdit);
			yield return nameof(NOManifestControlBag.TransportMeansCodeFindBox);
			yield return nameof(NOManifestControlBag.VehicleRegistrationAndNationalityUserControl);
			yield return nameof(NOManifestControlBag.MasterBillGroupBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => NOManifestControlBag.Instance;
}
