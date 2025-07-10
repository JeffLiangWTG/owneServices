using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously used in DE, can be used for other countries")]
	public sealed class CommonHeaderDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout CommonHeaderDetailsLayout { get; }

		PanelLayout IPanelLayoutProvider.Layout => CommonHeaderDetailsLayout;

		public CommonHeaderDetailsLayouts()
		{
			CommonHeaderDetailsLayout = CreateHeaderDetailsLayout();
		}

		PanelLayout CreateHeaderDetailsLayout()
		{
			var builder = new CommonHeaderDetailsLayoutBuilder<CusReconDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.EntryTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.EntryStatusTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.PeriodFromDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PeriodToDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.AuthorizationNumberGuidDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
