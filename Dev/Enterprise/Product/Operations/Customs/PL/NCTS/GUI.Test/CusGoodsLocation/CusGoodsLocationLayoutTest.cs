using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusGoodsLocation = Enterprise.Customs.PL.NCTS.Business.CusGoodsLocation;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(CusGoodsLocationLayout))]
sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.EoriNumberTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

	public void TestAdditionalIdentifierTextBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
			CusGoodsLocationQualifierList.Codes.PostcodeAddress,
			CusGoodsLocationQualifierList.Codes.EoriNumber,
			CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
	}

	public void TestPostcodeTextBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.PostcodeTextBox,
			CusGoodsLocationQualifierList.Codes.PostcodeAddress,
			CusGoodsLocationQualifierList.Codes.Address);
	}

	public void TestCountryCodeFindBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.CountryCodeFindBox,
			CusGoodsLocationQualifierList.Codes.PostcodeAddress,
			CusGoodsLocationQualifierList.Codes.Address);
	}

	public void TestUnlocoCodeFindBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
	}

	public void TestCustomsOfficeCodeFindBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestGeoLocationLatitudeTextBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
	}

	public void TestGeoLocationLongitudeTextBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
	}

	public void TesOrganisationFindBoxisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.OrganisationFindBox,
			CusGoodsLocationQualifierList.Codes.EoriNumber,
			CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
	}

	public void TestEoriNumberTextBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.EoriNumberTextBox, CusGoodsLocationQualifierList.Codes.EoriNumber);
	}

	public void TestAuthorizationCodeFindBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
	}

	public void TestStreetAndNumberWithAddressValidationControlVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
	}

	public void TestCityTextBoxVisibility()
	{
		AssertControlIsVisibleForQualifiers(CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
	}

	public void TestContactTextBoxVisibility()
	{
		AssertControlIsVisibleForNotEmptyQualifier(CusGoodsLocationControlBag.Instance.ContactTextBox);
		AssertControlVisibilityForIncident(CusGoodsLocationControlBag.Instance.ContactTextBox);
		AssertControlVisibilityForMovementHeader(ArrivalMovementParent, CusGoodsLocationControlBag.Instance.ContactTextBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlVisibilityForMovementHeader(DepartureMovementParent, CusGoodsLocationControlBag.Instance.ContactTextBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestPhoneTextBoxVisibility()
	{
		AssertControlIsVisibleForNotEmptyQualifier(CusGoodsLocationControlBag.Instance.PhoneTextBox);
		AssertControlVisibilityForIncident(CusGoodsLocationControlBag.Instance.PhoneTextBox);
		AssertControlVisibilityForMovementHeader(ArrivalMovementParent, CusGoodsLocationControlBag.Instance.PhoneTextBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlVisibilityForMovementHeader(DepartureMovementParent, CusGoodsLocationControlBag.Instance.PhoneTextBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestEmailTextBoxVisibility()
	{
		AssertControlIsVisibleForNotEmptyQualifier(CusGoodsLocationControlBag.Instance.EmailTextBox);
		AssertControlVisibilityForIncident(CusGoodsLocationControlBag.Instance.EmailTextBox);
		AssertControlVisibilityForMovementHeader(ArrivalMovementParent, CusGoodsLocationControlBag.Instance.EmailTextBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlVisibilityForMovementHeader(DepartureMovementParent, CusGoodsLocationControlBag.Instance.EmailTextBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestTypeDropEditVisibility()
	{
		AssertControlVisibilityForIncident(CusGoodsLocationControlBag.Instance.TypeDropEdit);
	}

	public void TestAdditionalIdentifierTextBoxCaption()
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

		CombineAssertions(() =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out var resourceStringData);
			AssertEquals("CGL_Qualifier = 'T'", "House Number", resourceStringData.Caption);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
			AssertEquals("CGL_Qualifier = 'X'", "Additional Identifier", resourceStringData.Caption);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
			AssertEquals("CGL_Qualifier = 'Y'", "Additional Identifier", resourceStringData.Caption);
		});
	}

	void AssertControlIsVisibleForQualifiers(ControlReference control, params string[] visibleForQualifiers)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

		CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, control, location, visibleForQualifiers);
	}

	void AssertControlIsVisibleForNotEmptyQualifier(ControlReference control)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

		CusGoodsLocationLayoutAssertions.AssertControlInvisibleForCertainQualifiers(layout, control, location, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, string.Empty);
	}

	void AssertControlVisibilityForIncident(ControlReference control)
	{
		var location = GetLocationForParent(IncidentParent);
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;
		AssertEquals("Incident Location", false, layout.IsVisible(control, location));
	}

	void AssertControlVisibilityForMovementHeader(string locationParent, ControlReference control, string qualifierWhenInvisible)
	{
		var location = GetLocationForParent(locationParent);
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

		CombineAssertions(() =>
		{
			foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier != qualifierWhenInvisible, layout.IsVisible(control, location));
			}
		});
	}

	const string IncidentParent = "EnRouteIncident";
	const string ArrivalMovementParent = "ArrivalMovementHeader";
	const string DepartureMovementParent = "DepartureMovementHeader";

	CusGoodsLocation GetLocationForParent(string parent)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		switch (parent)
		{
			case IncidentParent:
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				return nctsHeader.EnRouteIncidents.AddNew().GoodsLocation as CusGoodsLocation;
			case ArrivalMovementParent:
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				return nctsHeader.ArrivalMovementHeader.GoodsLocation as CusGoodsLocation;
			case DepartureMovementParent:
			default:
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				return nctsHeader.MovementHeader.GoodsLocation as CusGoodsLocation;
		}
	}
}
