using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using CusGoodsLocation = Enterprise.Customs.PL.NCTS.Business.CusGoodsLocation;

namespace Enterprise.Customs.PL.NCTS.GUI;

public sealed class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
{
	PanelLayout Layout { get; } = CreateCusGoodsLocationLayout();

	#region IPanelLayoutProviderWithExtensions

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
	{
		new CusGoodsLocationWebAddressValidationExtension()
	};

	#endregion

	static PanelLayout CreateCusGoodsLocationLayout()
	{
		var builder = new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.EoriNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.AdditionalIdentifierTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.CityTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PostcodeTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnlocoCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ContactTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PhoneTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.EmailTextBox, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.TypeDropEdit, l => !IsParentEnRouteIncident(l));
		builder.SetVisibility(commonBag.ContactTextBox, IsContactDetailsVisible, l => l.CGL_QualifierInfo);
		builder.SetVisibility(commonBag.PhoneTextBox, IsContactDetailsVisible, l => l.CGL_QualifierInfo);
		builder.SetVisibility(commonBag.EmailTextBox, IsContactDetailsVisible, l => l.CGL_QualifierInfo);

		return builder.Build();
	}

	static bool IsContactDetailsVisible(CusGoodsLocation location)
	{
		return location.ContactPersonDataVisible
			&& location.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
			&& !(IsParentMovementHeaderAndCustomsOffice(location) || IsParentEnRouteIncident(location));
	}

	static bool IsParentMovementHeaderAndCustomsOffice(CusGoodsLocation location)
	{
		return location.CGL_Qualifier.IsEmpty
			|| (location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
				&& IsParentMovementHeader());

		bool IsParentMovementHeader()
		{
			var parent = location.Parent;
			return parent is NctsDepartureMovementHeader || parent is NctsArrivalMovementHeader;
		}
	}

	static bool IsParentEnRouteIncident(CusGoodsLocation location) => location.Parent is EnRouteIncident;
}
