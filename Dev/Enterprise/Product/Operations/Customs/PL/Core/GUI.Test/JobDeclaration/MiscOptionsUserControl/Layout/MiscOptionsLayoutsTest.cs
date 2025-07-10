using System.Collections.Generic;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(MiscOptionsLayouts))]
sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
{
	public void TestPaymentMethodDropEditCaption_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		Layout.TryGetCaption(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, declaration, out var resourceStringData);
		AssertEquals("Import PaymentMethodDropEdit Caption", "MoP Duty", resourceStringData.Caption);
	}

	public void TestPaymentMethodDropEditCaption_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Layout.TryGetCaption(EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, declaration, out var resourceStringData);
		AssertEquals("Export PaymentMethodDropEdit Caption", "Payment Party", resourceStringData.Caption);
	}

	public void TestItineraryCountriesSeparatorUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("Visible in exit summary declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
	}

	public void TestItineraryCountriesUserControlVisibility()
	{
		var layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Visible in export declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("Not visible in import declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Not visible in miscellaneous declaration", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("Visible in exit summary declaration", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
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

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Medium);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Medium);
			yield return (CommonMiscOptionsControlBag.Instance.EntryAuthorisationDateEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.ExciseZDropEdit, ControlWidthClass.Auto);
			yield return (MiscOptionsControlBag.Instance.VATZDropEdit, ControlWidthClass.Auto);
			yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);
			yield return (MiscOptionsControlBag.Instance.SupportingInformationUserControl, ControlWidthClass.LongControl);
		}
	}

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();

	PanelLayout layout;
}
