using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class EntryInstructionDetailsBasicLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateLayout();
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
		var commonBag = builder.CommonBag;
		var noBag = EntryInstructionDetailsControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(commonBag.StyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.SubStyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CPCDropEdit, ControlWidthClass.Long);
		builder.Add(noBag.PackageCountCalcEdit, ControlWidthClass.Long);
		builder.Add(noBag.GoodsNumberUserControl, ControlWidthClass.Long);
		builder.Add(noBag.RequestProcessingDateDateEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(noBag.RelatedDeclarationSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(noBag.SelectedDeclTypeDropEdit, ControlWidthClass.Long);
		builder.Add(noBag.OriginalDeclarationDropEdit, ControlWidthClass.Long);
		builder.Add(noBag.CaseCodeDropEdit, ControlWidthClass.Long);
		builder.Add(noBag.ReasonTextBox, ControlWidthClass.Long);
		builder.Add(noBag.CustomsReplyMessageTextBox, ControlWidthClass.Long);

		builder.SetCaption(noBag.RelatedDeclarationSeparatorUserControl, GetSeparatorCaption);
		builder.SetVisibility(noBag.RelatedDeclarationSeparatorUserControl, IsRelatedDeclarationVisible, x => x.JobDeclaration?.JE_CopyStatusInfo);
		builder.SetVisibility(noBag.SelectedDeclTypeDropEdit, IsRelatedDeclarationVisible, x => x.JobDeclaration?.JE_CopyStatusInfo);
		builder.SetVisibility(noBag.OriginalDeclarationDropEdit, IsRelatedDeclarationVisible, x => x.JobDeclaration?.JE_CopyStatusInfo);
		builder.SetVisibility(noBag.CaseCodeDropEdit, x => x.JobDeclaration.JE_CopyStatus.Equals(NODeclarationCopyStatus.Codes.Recalculation), x => x.JobDeclaration?.JE_CopyStatusInfo);
		builder.SetVisibility(noBag.ReasonTextBox, IsRelatedDeclarationVisible, x => x.JobDeclaration?.JE_CopyStatusInfo);
		builder.SetVisibility(noBag.CustomsReplyMessageTextBox, IsRelatedDeclarationVisible, x => x.JobDeclaration?.JE_CopyStatusInfo);

		return builder.Build();
	}

	static bool IsRelatedDeclarationVisible(CusEntryInstruction entryInstruction)
	{
		return (entryInstruction.JobDeclaration?.JE_CopyStatus ?? ZString.Empty).In<ZString>(NODeclarationCopyStatus.Codes.Recalculation, NODeclarationCopyStatus.Codes.ReExport, NODeclarationCopyStatus.Codes.FinalImport);
	}

	static ResourceStringData GetSeparatorCaption(CusEntryInstruction entryInstruction)
	{
		switch (entryInstruction?.JobDeclaration?.JE_CopyStatus ?? ZString.Empty)
		{
			case NODeclarationCopyStatus.Codes.ReExport:
				return Enterprise.Customs.NO.GUI.Res.GetData("FD0DBC60-9182-D6A2-4789-DA8B6DDEEA6C", "Re-Export");
			case NODeclarationCopyStatus.Codes.FinalImport:
				return Enterprise.Customs.NO.GUI.Res.GetData("19F662BC-8BE3-E19C-4A51-FEC45804BC18", "Standard Import");
			default:
				return Enterprise.Customs.NO.GUI.Res.GetData("4DF4B7F1-C39F-9386-438B-17D74340922F", "Recalculation");
		}
	}
}
