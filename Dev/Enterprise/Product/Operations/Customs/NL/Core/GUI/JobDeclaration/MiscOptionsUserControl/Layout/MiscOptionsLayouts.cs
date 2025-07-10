using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class MiscOptionsLayouts : IPanelLayoutProvider
{
	PanelLayout MiscOptions { get; }

	PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

	public MiscOptionsLayouts()
	{
		MiscOptions = CreateMiscOptionsLayouts();
	}

	PanelLayout CreateMiscOptionsLayouts()
	{
		var builder = new MiscOptionsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.MiscOptionsControlBag.Instance;
		var nlBag = MiscOptionsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(nlBag);

		builder.AddColumn();
		builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.LCPInspectDateEdit, ControlWidthClass.Medium);
		builder.Add(euBag.LCPDepartDateEdit, ControlWidthClass.Medium);
		builder.Add(euBag.ShipmentTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.RepresentationDropEdit, ControlWidthClass.Auto);
		builder.Add(nlBag.CustomsAccountTextBox, ControlWidthClass.Long);
		builder.Add(nlBag.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.PaymentMethodDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
		builder.Add(nlBag.PaymentPartyEORINumberTextBox, ControlWidthClass.Long);
		builder.Add(nlBag.VATPartyTaxNumberTextBox, ControlWidthClass.Long);
		builder.Add(euBag.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(nlBag.SupportingInformationUserControl, ControlWidthClass.LongNoCaption);

		builder.SetVisibility(euBag.PaymentMethodDropEdit, h => !h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(nlBag.PaymentPartyEORINumberTextBox, h => !h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(nlBag.PaymentSeparatorUserControl, h => !h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(nlBag.VATPartyTaxNumberTextBox, h => !h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(nlBag.SupportingInformationUserControl, h => !h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.ItineraryCountriesSeparatorUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.ItineraryCountriesUserControl, d => d.IsExport, d => d.JE_MessageTypeInfo);

		return builder.Build();
	}
}
