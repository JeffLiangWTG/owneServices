using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class EntryInstructionDetailsBasicUserControlLayout : IPanelLayoutProvider
{
	public PanelLayout Layout => layout ?? (layout = CreateEntryInstructionDetailsBasicUserControlLayout());
	PanelLayout layout;

	static PanelLayout CreateEntryInstructionDetailsBasicUserControlLayout()
	{
		var builder = new EntryInstructionDetailsBasicUserControlLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = builder.EUBag;
		var nlBag = builder.NLBag;

		builder.AddControlBag(euBag);
		builder.AddControlBag(nlBag);
		builder.AddColumn();

		builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.CPCDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(euBag.LocationOfGoodsUserControl, widthClass: ControlWidthClass.Long);
		builder.Add(nlBag.IsHighValueOvrdCheckBox, widthClass: ControlWidthClass.Medium);
		builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.NewOwnerOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(commonBag.SubStyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionTextBox, alignToControl: commonBag.CPCDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(nlBag.TransNatureDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.RemoverOrganisationControl, alignToControl: commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

		return builder.Build();
	}
}
