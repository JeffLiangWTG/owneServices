using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
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
			builder.AddControlBag(euBag);
			var trBag = MiscOptionsControlBag.Instance;
			builder.AddControlBag(trBag);

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
			builder.Add(trBag.ManifestToOpenUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(trBag.ExportersUnionInfoUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(trBag.GuaranteeInfoOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(trBag.GuaranteeGuidFindBox, ControlWidthClass.Long);
			builder.Add(trBag.BondTypeDropEdit, ControlWidthClass.Long);
			builder.Add(trBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(trBag.GuaranteeDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(trBag.DedicatedAmountCalcEdit, ControlWidthClass.Medium);
			builder.Add(trBag.RatioCalcEdit, ControlWidthClass.Medium);
			builder.Add(trBag.AmountCalcEdit, ControlWidthClass.Medium);
			builder.Add(trBag.TotalAmountsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(trBag.InvoiceCountCalcEdit, ControlWidthClass.Long);
			builder.Add(trBag.TotalInvoiceAmountLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(trBag.TotalFreeOnBoardLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(trBag.TotalFreightLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(trBag.TotalInsuranceLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(trBag.TotalOverseasLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(trBag.LocalTotalChargesLocalCurrencyControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(euBag.RelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(trBag.SupportingInformationUserControl, ControlWidthClass.LongControl);

			builder.SetVisibility(trBag.ManifestToOpenUserControl, d => d.IsActualImport, x => x.IsActualImportInfo);
			builder.SetVisibility(trBag.ExportersUnionInfoUserControl, d => d.IsExport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.GuaranteeInfoOptionsSeparatorUserControl, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.GuaranteeGuidFindBox, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.BondTypeDropEdit, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.DedicatedAmountCalcEdit, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.RatioCalcEdit, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.ReferenceNumberTextBox, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.AmountCalcEdit, d => d.IsImport, x => x.JE_MessageTypeInfo);
			builder.SetVisibility(trBag.GuaranteeDescriptionTextBox, d => d.IsImport, x => x.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
