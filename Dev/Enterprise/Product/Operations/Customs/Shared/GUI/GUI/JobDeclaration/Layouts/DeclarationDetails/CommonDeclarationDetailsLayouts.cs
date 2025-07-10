using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonDeclarationDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => declarationDetails.Value;

		readonly Lazy<PanelLayout> declarationDetails = new (CreateDeclarationDetailsLayout);

		static PanelLayout CreateDeclarationDetailsLayout()
		{
			var builder = new CommonDeclarationDetailsLayoutBuilder<BaseJobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DeclarationNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.StatusTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
