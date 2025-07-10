using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Norway;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

	public void TestCopyMenuItem_ShouldIncludeCopyOptions()
	{
		using var module = new JobDeclarationModule();
		using var popup = new EmbeddedModulePopup(module);
		popup.Show();
		CombineAssertions(() =>
		{
			var menuItems = module.CopyMenuItem.MenuItems;
			AssertEquals("Submenu of Copy should have count", 4, menuItems.Count);
			AssertEquals("Submenu of Copy should have item[0]", "Full general copy", menuItems[0].Text);
			AssertEquals("Submenu of Copy should have item[1]", "Recalculation category EB/RE/SO", menuItems[1].Text);
			AssertEquals("Submenu of Copy should have item[2]", "Re-export, following import", menuItems[2].Text);
			AssertEquals("Submenu of Copy should have item[3]", "Import, following temporary import", menuItems[3].Text);
		});
	}

	public void TestHandleRecalculationCopyClick_ShouldWarnWhenNoDeclarationIsSelected()
	{
		AssertMenuItemWarns_WhenNoDeclarationSelected("Recalculation category EB/RE/SO");
	}

	public void TestHandleReexportCopyClick_ShouldWarnWhenNoDeclarationIsSelected()
	{
		AssertMenuItemWarns_WhenNoDeclarationSelected("Re-export, following import");
	}

	public void TestHandleFinalImportCopyClick_ShouldWarnWhenNoDeclarationIsSelected()
	{
		AssertMenuItemWarns_WhenNoDeclarationSelected("Import, following temporary import");
	}

	void AssertMenuItemWarns_WhenNoDeclarationSelected(string menuItemText)
	{
		using var module = new JobDeclarationModule();
		using var popup = new EmbeddedModulePopup(module);
		popup.Show();
		CombineAssertions(() =>
		{
			var menu = module.CopyMenuItem.MenuItems.FindByText(menuItemText);
			menu.PerformClick();
			AssertEquals("(base-case): Should warn when no declaration is selected", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

			AddAndSelectNewDeclaration(module);
			UnitTestUserNotification.Instance.ClearMessages();
			menu.PerformClick();
			AssertNotEquals("(selected): Should NOT warn when declaration is selected", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestErrorWhen_CannotCopyDeclarationWithThisCode()
	{
		using var module = new JobDeclarationModule();
		using var popup = new EmbeddedModulePopup(module);
		popup.Show();

		var declaration = AddAndSelectNewDeclarationWithEntryInstruction(module, "6000");
		Factory.Save();
		foreach (var menuText in reExportAndFinalImp)
		{
			var menu = module.CopyMenuItem.MenuItems.FindByText(menuText);
			menu.PerformClick();
			AssertErrorMessageIsSet($"{menuText} when CEI_Code is '6000'", CopyHelper.CannotCopyDeclarationWithThisCode);
		}

		declaration = AddAndSelectNewDeclarationWithEntryInstruction(module, "5000");
		Factory.Save();
		foreach (var menuText in reExportAndFinalImp)
		{
			var menu = module.CopyMenuItem.MenuItems.FindByText(menuText);
			menu.PerformClick();
			AssertNoErrorMessageIsSet($"{menuText} when CEI_Code is '5000'");
		}
	}

	public void TestErrorWhen_CannotCopyCancelledDeclaration()
	{
		using var module = new JobDeclarationModule();
		using var popup = new EmbeddedModulePopup(module);
		popup.Show();
		var declaration = AddAndSelectNewDeclarationWithEntryInstruction(module, "5000");
		declaration.IsCancelled = true;
		Factory.Save();
		foreach (var menuText in allCopyOptions)
		{
			var menu = module.CopyMenuItem.MenuItems.FindByText(menuText);
			menu.PerformClick();
			AssertErrorMessageIsSet($"{menuText} when CEI_Code is '5000'", CopyHelper.CannotCopyCancelledDeclaration);
		}
	}

	public void TestErrorWhen_ParentDeclarationNotSaved()
	{
		using var module = new JobDeclarationModule();
		using var popup = new EmbeddedModulePopup(module);
		popup.Show();
		var declaration = AddAndSelectNewDeclarationWithEntryInstruction(module, "5000");
		foreach (var menuText in allCopyOptions)
		{
			var menu = module.CopyMenuItem.MenuItems.FindByText(menuText);
			menu.PerformClick();
			AssertErrorMessageIsSet($"{menuText} when CEI_Code is '5000'", CopyHelper.ParentDeclarationNotSaved);
		}
	}

	void AssertErrorMessageIsSet(string description, string expectedError)
	{
		CombineAssertions(() =>
		{
			AssertEquals($"{description} - Should have errortext", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals($"{description} - Message should be error", expected: true, UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessages();
		});
	}

	void AssertNoErrorMessageIsSet(string description)
	{
		CombineAssertions(() =>
		{
			AssertEquals($"{description} - Should not have errortext", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals($"{description} - Message should be error", expected: false, UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessages();
		});
	}

	readonly static ZString reExportMenuText = "Re-export, following import";
	readonly static ZString reCalulationMenuText = "Recalculation category EB/RE/SO";
	readonly static ZString importFollowingTempImp = "Import, following temporary import";
	readonly static ZString[] allCopyOptions = new ZString[] { reExportMenuText, reCalulationMenuText, importFollowingTempImp };
	readonly static ZString[] reExportAndFinalImp = new ZString[] { reExportMenuText, importFollowingTempImp };

	#region Helpers

	JobDeclaration AddAndSelectNewDeclarationWithEntryInstruction(JobDeclarationModule module, string cei_Code)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = cei_Code;
		var index = module.GridCollection.Add(declaration);
		module.DisplayGrid.Select(index);
		return declaration;
	}

	JobDeclaration AddAndSelectNewDeclaration(JobDeclarationModule module)
	{
		var declaration = Factory.New<JobDeclaration>();
		var index = module.GridCollection.Add(declaration);
		module.DisplayGrid.Select(index);
		return declaration;
	}

	#endregion

	protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
	{
		var declaration = base.CreateDeclarationForFetchHintTest(factory, messageType, i);

		foreach (var phaseStatus in new[] { "REM", "FIN", "REM", "REM", "FIN", "REM", "FIN" })
		{
			declaration.ActiveEntryHeaders.AddNew().CH_PhaseStatus = phaseStatus;
		}

		return declaration;
	}
}
