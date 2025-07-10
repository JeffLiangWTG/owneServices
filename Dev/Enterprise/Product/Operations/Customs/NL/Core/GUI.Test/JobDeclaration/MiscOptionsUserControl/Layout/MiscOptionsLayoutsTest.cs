using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(MiscOptionsLayouts))]
sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
{
	public void TestPaymentSeparatorUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));
		});
	}

	public void TestPaymentMethodDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, declaration));
		});
	}

	public void TestPaymentPartyEORINumberTextBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentPartyEORINumberTextBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentPartyEORINumberTextBox, declaration));
		});
	}

	public void TestVATPartyTaxNumberTextBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.VATPartyTaxNumberTextBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.VATPartyTaxNumberTextBox, declaration));
		});
	}

	public void TestSupportingInformationUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.SupportingInformationUserControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.SupportingInformationUserControl, declaration));
		});
	}

	public void TestItineraryCountriesSeparatorUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Not Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
		});
	}

	public void TestItineraryCountriesUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Not Visible", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Visible", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
		});
	}

	protected override int ControlBagCount => 3;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Auto);
			yield return (MiscOptionsControlBag.Instance.CustomsAccountTextBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
			yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
			yield return (MiscOptionsControlBag.Instance.PaymentPartyEORINumberTextBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.VATPartyTaxNumberTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);
			yield return (MiscOptionsControlBag.Instance.SupportingInformationUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<BaseJobDeclaration>();
	}
	BaseJobDeclaration declaration;

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();

	PanelLayout layout;
}
