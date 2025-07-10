using System;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class SumARegisterDetailsHeaderLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => declarationDetails.Value;

	readonly Lazy<PanelLayout> declarationDetails = new(CreateDeclarationDetailsLayout);

	static PanelLayout CreateDeclarationDetailsLayout()
	{
		var builder = new DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>();
		var commonBag = builder.CommonBag;

		var noBag = SumARegisterDetailsHeaderControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(noBag.GoodsNumberUserControl, ControlWidthClass.Long);
		builder.Add(commonBag.PreviousReferenceTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.StatusDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.ArrvialDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PreviousReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(noBag.TrasportMeansUserControl, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(noBag.UnloadingRemarksLabel, ControlWidthClass.Long);
		builder.Add(noBag.UnloadingRemarksTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
