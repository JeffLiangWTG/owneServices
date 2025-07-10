using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class AdditionalInfosUserControlLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }
	public AdditionalInfosUserControlLayout()
	{
		Layout = CreateAdditionalInfoUserControlLayout();
	}

	static PanelLayout CreateAdditionalInfoUserControlLayout()
	{
		var builder = new AdditionalInfosUserControlLayoutBuilder();

		var euBag = AdditionalInformationDetailsControlBag.Instance;

		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.KindDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.FullTypeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DescriptionTextBox, ControlWidthClass.Auto);
			
		return builder.Build();
	}
}
