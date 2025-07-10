using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class DeclarationDetailsLayouts : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => declarationDetails.Value;

	readonly Lazy<PanelLayout> declarationDetails = new (CreateDeclarationDetailsLayout);

	static PanelLayout CreateDeclarationDetailsLayout()
	{
		var builder = new DeclarationDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var noBag = DeclarationDetailsControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(commonBag.DeclarationNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.StatusTextBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(noBag.PhaseStatusTextBox, ControlWidthClass.Long);
		builder.Add(noBag.MessageStatusTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
