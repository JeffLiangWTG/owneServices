using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class PreviousDocumentsFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public PreviousDocumentsFieldsLayout()
	{
		Layout = CreatePreviousDocumentFieldsLayout();
	}

	static PanelLayout CreatePreviousDocumentFieldsLayout()
	{
		var builder = new PreviousDocumentsFieldsLayoutBuilder();

		var euBag = PreviousDocumentsFieldsControlBag.Instance;

		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.CodeDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.Quantity2CalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(euBag.SubTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.LineNoCalcEdit, ControlWidthClass.Auto);
		builder.Add(euBag.Reference2TextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
