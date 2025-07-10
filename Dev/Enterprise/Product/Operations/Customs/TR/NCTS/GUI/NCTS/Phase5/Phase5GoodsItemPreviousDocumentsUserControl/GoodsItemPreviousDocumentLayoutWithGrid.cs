using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public GoodsItemPreviousDocumentLayoutWithGrid()
		{
			Layout = CreateGoodsItemPreviousDocumentLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateGoodsItemPreviousDocumentLayout()
		{
			var builder = new EU.NCTS.GUI.PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();
			var commonBag = builder.CommonBag;
			var trBag = PreviousDocumentsControlBag.Instance;
			builder.AddControlBag(trBag);

			builder.AddColumn();
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.QuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NumOfPackagesDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Auto);
			builder.Add(trBag.AmountCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(trBag.CountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(trBag.PrevDocsTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(trBag.PaymentTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(trBag.NatureOfBussinessDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(EU.NCTS.GUI.GoodsItemPreviousDocumentsGridUserControl);
	}
}
