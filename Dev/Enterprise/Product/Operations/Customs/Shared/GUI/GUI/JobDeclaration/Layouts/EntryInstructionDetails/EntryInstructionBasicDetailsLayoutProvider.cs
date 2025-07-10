using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class EntryInstructionBasicDetailsLayoutProvider : IPanelLayoutProvider
	{
		#region IPanelLayoutProvider

		PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		PanelLayout layout;

		#endregion

		PanelLayout CreateLayout()
		{
			var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DetailsLabel, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Auto);
			builder.Add(commonBag.CPCDropEdit, widthClass: ControlWidthClass.Auto);
			builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.NewOwnerOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.RemoverOrganisationControl, alignToControl: commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.SubStyleDropEdit, widthClass: ControlWidthClass.Auto);
			builder.Add(commonBag.DescriptionTextBox, alignToControl: commonBag.CPCDropEdit, widthClass: ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
