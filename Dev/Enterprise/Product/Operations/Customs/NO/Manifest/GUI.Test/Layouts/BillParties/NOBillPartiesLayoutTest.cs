using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(NOBillPartiesLayout))]
sealed class NOBillPartiesLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBillPartiesControlBag.Instance.ShipperSeparatorUserControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConvertShipperToOrganizationButton, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperNameTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperStreet1TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperStreet2TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperCityTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ShipperStateDropEdit, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ShipperPhoneTextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.ShipperEmailTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeCityTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneePhoneTextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.ConsigneeEmailTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (NOBillPartiesControlBag.Instance.RepresentativeSeparatorUserControl, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeNameTextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeStreet1TextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeStreet2TextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeCityTextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeStateDropEdit, ControlWidthClass.Auto);
			yield return (NOBillPartiesControlBag.Instance.RepresentativePostCodeTextBox, ControlWidthClass.Auto);
			yield return (NOBillPartiesControlBag.Instance.RepresentativePhoneTextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeEmailTextBox, ControlWidthClass.Long);
			yield return (NOBillPartiesControlBag.Instance.RepresentativeRegNoTextBox, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<AsycudaBill>();
}
