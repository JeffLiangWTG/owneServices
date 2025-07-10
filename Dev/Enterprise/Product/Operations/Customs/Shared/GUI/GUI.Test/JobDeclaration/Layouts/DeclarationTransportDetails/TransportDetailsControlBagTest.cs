using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(TransportDetailsControlBag))]
	sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDetailsControlBag.Instance.OverrideValuesCheckBox);
				yield return nameof(TransportDetailsControlBag.Instance.PortOfLoadingUserControl);
				yield return nameof(TransportDetailsControlBag.PortOfFirstArrivalUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.PortOfDischargeUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.MasterBillTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.FlightUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.IATALoadPortCodeFindBox);
				yield return nameof(TransportDetailsControlBag.Instance.VoyageNumberTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.VesselCodeFindBox);
				yield return nameof(TransportDetailsControlBag.Instance.OceanBillTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.FlightAndNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.InlandModeOfTransportDropEdit);
				yield return nameof(TransportDetailsControlBag.VoyageAndNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.TransportDetailsPortOfLoadingWithIATAUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandSeparatorUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandRoadUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandModeAndTypeOfIdUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandIDAndNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandAirUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandInlandWaterwaysUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandOwnPropulsionUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandRailUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandSeaUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.UCRTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.SubLocationOfGoodsTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.GoodsDestinationCountryCodeFindBox);
				yield return nameof(TransportDetailsControlBag.Instance.TransportMeansDropEdit);
				yield return nameof(TransportDetailsControlBag.Instance.CustomsOfficeCodeFindBox);
				yield return nameof(TransportDetailsControlBag.Instance.CustomsLoadPortCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
	}
}
