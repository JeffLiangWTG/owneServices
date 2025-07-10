using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.SingleLineEntry;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.PL.Business.Constants;
using JobDeclaration = Enterprise.Customs.PL.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.PL.Module;

[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
public class JobDeclarationModule : EU.Module.JobDeclarationModule
{
	protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();

	protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

	protected override EU.Business.Declaration.SingleLineEntryManager GetNewSingleLineEntryManager(EU.Business.Declaration.JobDeclaration declaration) =>
		declaration.IsImport
			? new ImportSingleLineEntryManager((JobDeclaration)declaration)
			: new Business.Declaration.SingleLineEntryManager((JobDeclaration)declaration);

	protected override SingleLineEntryForm GetSingleLineEntryForm(ISingleLineEntryManager manager) =>
		manager.Declaration.IsImport ? new GUI.ImportSingleLineEntryForm(manager) : new GUI.SingleLineEntryForm(manager);

	protected override MenuItem[] GetNewActionMenuItems()
	{
		var result = new List<MenuItem>(base.GetNewActionMenuItems());

		var index = createFromShipmentMenuItem.Index;
		result.Insert(index + 1, new ZMenuItem(Res.GetString("12D0545E-5CFE-4E21-BC63-AF5137596864", "Create Supplementary Declaration"), CreateSupplementaryDeclaration_Click));

		return result.ToArray();
	}

	protected void CreateSupplementaryDeclaration_Click(object sender, EventArgs e)
	{
		var declaration = CurrentBusinessObjectInGrid?.Cast<JobDeclaration>().FirstOrDefault();
		if (declaration != null && declaration.JE_MessageType == PLEntryStatusList.Codes.EXP)
		{
			if (declaration.CustomsEntryInstructions.Count > 0
				&& (declaration.CustomsEntryInstructions.All(entry => IsSubStyleCOrF(entry.CEI_SubStyle)) || declaration.CustomsEntryInstructions.All(entry => IsSubStyleBOrE(entry.CEI_SubStyle))))
			{
				if (declaration.CustomsEntryHeaders.Count == 0 || !declaration.CustomsEntryHeaders.All(entryHeader => IsEntryStatusValid(entryHeader.CH_EntryStatus)))
				{
					Globals.Message.ShowError(Res.GetString("D3583722-7C04-44C5-B009-87A6FBFAD989", "Status of the Simplified Declaration is not valid \r\n to create a Supplementary Declaration"));
					return;
				}

				CreateSupplementaryDeclaration(declaration);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("AD8ADC83-6E56-4370-AF74-CA971A8C4C4A", "Operation \"Create Supplementary Declaration\" is available only \r\n for Simplified Declaration (where Sub Style is one of B, C, E or F)"));
			}
		}
	}

	void CreateSupplementaryDeclaration(JobDeclaration declaration)
	{
		var supplementaryCloneStrategy = new JobDeclarationSupplementaryCloneStrategy(declaration, CloneType.DeepTemplateCopy, Factory);
		var supplementaryDeclaration = (JobDeclaration)supplementaryCloneStrategy.Clone();

		foreach (var (declarationEntryInstruction, supplementaryDeclarationEntryInstruction) in declaration.CustomsEntryInstructions.Zip(supplementaryDeclaration.CustomsEntryInstructions, (first, second) => (first, second)))
		{
			supplementaryDeclarationEntryInstruction.CEI_SubStyle = IsSubStyleBOrE(declarationEntryInstruction.CEI_SubStyle) ? SubStyleCodes.X : SubStyleCodes.Y;
			var previousDocuments = supplementaryDeclarationEntryInstruction.PreviousDocuments.AddNew();
			previousDocuments.CSI_Code = TemporaryStorageConstants.PreviousDocumentsCodeType.NMRN;
			previousDocuments.CSI_ReferenceNumber = declarationEntryInstruction?.EntryHeader?.MovementReferenceNumber ?? ZString.Empty;
		}
		var newForm = ShowNewFormCore(() => supplementaryDeclaration) as GUI.JobDeclarationForm;
		newForm.Saved += OnSaved;

		void OnSaved(object sender, EventArgs e)
		{
			declaration.RelatedDeclarations.Add(supplementaryDeclaration);
			newForm.Saved -= OnSaved;
		}
	}

	bool IsSubStyleCOrF(ZString subStyle) => subStyle == SubStyleCodes.C || subStyle == SubStyleCodes.F;

	bool IsSubStyleBOrE(ZString subStyle) => subStyle == SubStyleCodes.B || subStyle == SubStyleCodes.E;

	bool IsEntryStatusValid(ZString entryStatus) => entryStatus == PLEntryStatusList.Codes.ReleasedForExport
											|| entryStatus == ExportExitStatus.Codes.ReleasedForExit
											|| entryStatus == PLEntryStatusList.Codes.REQ
											|| entryStatus == PLEntryStatusList.Codes.EXP;
}
