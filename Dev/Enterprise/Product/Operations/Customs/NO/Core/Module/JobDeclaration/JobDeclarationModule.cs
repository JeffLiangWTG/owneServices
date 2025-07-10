using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.Customs.NO.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Module;

public class JobDeclarationModule : Customs.Module.JobDeclarationModule
{
	protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

	protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

	protected MultilingualString FullGeneralCopyText => ResString.GetMultilingualString("6933e4da-5778-4452-8512-3a77718d7b6d", "Full general copy");
	protected MultilingualString RecalculationCopyText => ResString.GetMultilingualString("df208646-9177-44b1-8968-cf8dbd62ce75", "Recalculation category EB/RE/SO");
	protected MultilingualString ReexportCopyText => ResString.GetMultilingualString("C6881AE5-81A0-41ED-AE80-5C074ABB5541", "Re-export, following import");
	protected MultilingualString FinalImportCopyText => ResString.GetMultilingualString("8679724E-C22F-4FD1-A016-ABCA2E2FDCC8", "Import, following temporary import");

	protected override void AddCopyMenuItem(List<MenuItem> menu)
	{
		base.AddCopyMenuItem(menu);
		CopyMenuItem.MenuItems.Add(new ZMenuItem(FullGeneralCopyText, HandleTemplateCopyClick));
		CopyMenuItem.MenuItems.Add(new ZMenuItem(RecalculationCopyText, HandleRecalculationCopyClick));
		CopyMenuItem.MenuItems.Add(new ZMenuItem(ReexportCopyText, HandleReexportCopyClick));
		CopyMenuItem.MenuItems.Add(new ZMenuItem(FinalImportCopyText, HandleFinalImportCopyClick));
	}

	void HandleReexportCopyClick(object sender, EventArgs e)
	{
		if (!HandleDeclarationCopy(CopyHelper.CreateReexportCopyOf, NODeclarationCopyStatus.Codes.ReExport, DisplayGrid.SelectedElements))
		{
			ShowNoSelectedMessage();
		}
	}

	void HandleFinalImportCopyClick(object sender, EventArgs e)
	{
		if (!HandleDeclarationCopy(CopyHelper.CreateFinalImportCopyOf, NODeclarationCopyStatus.Codes.FinalImport, DisplayGrid.SelectedElements))
		{
			ShowNoSelectedMessage();
		}
	}

	void HandleRecalculationCopyClick(object sender, EventArgs e)
	{
		if (!HandleDeclarationCopy(CopyHelper.CreateRecalculationCopyOf, NODeclarationCopyStatus.Codes.Recalculation, DisplayGrid.SelectedElements))
		{
			ShowNoSelectedMessage();
		}
	}

	ZBool HandleDeclarationCopy(Func<JobDeclaration, CopyHelper.TemplateCopyDeclarationResult> copyFunc, string copyStatus, params BusinessObject[] elements)
	{
		return elements.ForEachAndCount<JobDeclaration>(declaration => HandleDeclarationCopy(copyFunc, copyStatus, declaration)) > 0;
	}

	void HandleDeclarationCopy(Func<JobDeclaration, CopyHelper.TemplateCopyDeclarationResult> copyFunc, string copyStatus, JobDeclaration declaration)
	{
		var copyDeclarationResult = copyFunc(declaration);
		if (!copyDeclarationResult.IsValid)
		{
			Globals.Message.ShowError(copyDeclarationResult.ErrorMessage, copyDeclarationResult.CopyStatusCode);
			return;
		}
		var newForm = ShowNewFormCore(() => copyDeclarationResult.Copy) as JobDeclarationForm;
		newForm.Saved += OnSaved;

		void OnSaved(object sender, EventArgs e)
		{
			CopyHelper.LinkDeclarationCopyToParent(declaration, newForm.Declaration as JobDeclaration);
			newForm.Saved -= OnSaved;
		}
	}
}
