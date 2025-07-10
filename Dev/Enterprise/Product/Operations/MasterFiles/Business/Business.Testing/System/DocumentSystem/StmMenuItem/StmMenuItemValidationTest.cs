using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmMenuItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSU_MenuName()
		{
			MenuItem.SU_MenuName = "Test Menu Name";
			Assert("No error expected", !MenuItem.SU_MenuNameInfo.HasErrors());

			MenuItem.SU_MenuName = "Test :Menu Name";
			Assert("Error expected", MenuItem.SU_MenuNameInfo.HasErrors());
		}

		public void TestValidateSU_BusinessContext()
		{
			MenuItem.SU_BusinessContext = "Business Context";
			Assert("No error expected", !MenuItem.SU_BusinessContextInfo.HasErrors());

			MenuItem.SU_BusinessContext = "Business :Context";
			Assert("Error expected", MenuItem.SU_BusinessContextInfo.HasErrors());
		}

		public void TestValidateSU_EmailSenderOverride_Warning_AllowEmailsToBeSentFromUsersAddress_True()
		{
			var warningMessage = String.Format("The address override will not work because the registry setting '{0}' is off", Env.Registry.RawRegistry.AllowEmailsToBeSentFromUsersAddress.Caption);

			MenuItem.SU_EmailSenderOverride = "a_valid@email.address";

			Env.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			AssertNoWarning(MenuItem.SU_EmailSenderOverrideInfo, warningMessage);

			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			MenuItem.SU_EmailSenderOverride = "a_valid@email.address";

			AssertHasWarning(MenuItem.SU_EmailSenderOverrideInfo, warningMessage);
		}

		public void TestValidateSU_EmailSenderOverride_WarningSMTP()
		{
			MenuItem.SU_EmailSenderOverride = "";
			Assert("No warning expected", !MenuItem.SU_EmailSenderOverrideInfo.HasWarnings());

			MenuItem.SU_EmailSenderOverride = "a_valid@email.address";
			Assert("Should have 'Relay Required' warning", MenuItem.SU_EmailSenderOverrideInfo.HasWarnings());
		}

		public void TestValidateSU_EmailSenderOverride_Invalid()
		{
			Env.Registry.SMTPServer = "aGoodProvider.withWierd.deeeep.domain";

			MenuItem.SU_EmailSenderOverride = "aGoodEmail@" + Env.Registry.SMTPServer;
			AssertNoError(MenuItem.SU_EmailSenderOverrideInfo, "That is not a valid email address");

			MenuItem.SU_EmailSenderOverride = "aBadEmail..@" + Env.Registry.SMTPServer;
			AssertHasError(MenuItem.SU_EmailSenderOverrideInfo, "That is not a valid email address");
		}

		public void TestValidateSU_MenuPath()
		{
			MenuItem.SU_MenuPath = "Test/Menu/Path";
			Assert("No error expected", !MenuItem.SU_MenuPathInfo.HasErrors());

			MenuItem.SU_MenuPath = "Test/:Menu/Name";
			Assert("Error expected", MenuItem.SU_MenuPathInfo.HasErrors());
		}

		public void TestValidateSU_DeliveryRestrictionMacro()
		{
			MenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			Assert("No Error expected", !MenuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());

			MenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			Assert("No Error expected", !MenuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());

			MenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			MenuItem.SU_DeliveryRestrictionMacro = string.Empty;
			AssertHasError(MenuItem.SU_DeliveryRestrictionMacroInfo, "User defined delivery restriction macro cannot be empty.");

			MenuItem.SU_DeliveryRestrictionMacro = "<Macor>";
			Assert("No Error expected", !MenuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());
		}

		public void TestValidateSU_MenuPathLeadingAndTrailingSlashes()
		{
			MenuItem.SU_MenuPath = "";
			AssertNoError(MenuItem.SU_MenuPathInfo, "Please remove all leading and trailing slashes from menu paths.");

			MenuItem.SU_MenuPath = "/Test/Menu/Name";
			AssertHasError(MenuItem.SU_MenuPathInfo, "Please remove all leading and trailing slashes from menu paths.");

			MenuItem.SU_MenuPath = "/Test/Menu/Name/";
			AssertHasError(MenuItem.SU_MenuPathInfo, "Please remove all leading and trailing slashes from menu paths.");

			MenuItem.SU_MenuPath = "Test/Menu/Name/";
			AssertHasError(MenuItem.SU_MenuPathInfo, "Please remove all leading and trailing slashes from menu paths.");

			MenuItem.SU_MenuPath = "Test/Menu/Name";
			AssertNoError(MenuItem.SU_MenuPathInfo, "Please remove all leading and trailing slashes from menu paths.");

			MenuItem.SU_MenuType = "ACT";
			MenuItem.SU_MenuPath = "/Test/Menu/Name/";
			AssertNoError(MenuItem.SU_MenuPathInfo, "Please remove all leading and trailing slashes from menu paths.");
		}

		public void TestShortcut()
		{
			MenuItem.SU_MenuShortcut = "CtrlShift1";
			Assert("No warning", !MenuItem.SU_MenuShortcutInfo.HasWarnings());
			MenuItem.SU_MenuShortcut = "CtrlShift0";
			Assert("Has warning", MenuItem.SU_MenuShortcutInfo.HasWarnings());
		}

		public void TestValidateSU_PrimaryDocPackItem_ValidPrimaryDoc_ShouldFindPivot()
		{
			var childMenu = Factory.New<StmMenuItem>();
			var menuPivot = Factory.New<StmMenuMenuPivot>();
			menuPivot.SF_SU_Inward = MenuItem.PK;
			menuPivot.SF_SU_Outward = childMenu.PK;

			var template = Factory.New<StmTemplate>();
			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = MenuItem.PK;
			templatePivot.SI_SO = template.PK;

			MenuItem.SU_PrimaryDocPackItemId = menuPivot.PK;
			AssertNoError(MenuItem.SU_PrimaryDocPackItemIdInfo, StmMenuItemValidation.SU_PrimaryDocPackItemError);

			MenuItem.SU_PrimaryDocPackItemId = templatePivot.PK;
			AssertNoError(MenuItem.SU_PrimaryDocPackItemIdInfo, StmMenuItemValidation.SU_PrimaryDocPackItemError);

			MenuItem.SU_PrimaryDocPackItemId = ZGuid.NewZGuid();
			AssertHasError(MenuItem.SU_PrimaryDocPackItemIdInfo, StmMenuItemValidation.SU_PrimaryDocPackItemError);
		}

		public void TestValidateSU_PrimaryDocPackItem_InvalidPrimaryDoc_ShouldHaveError()
		{
			var childMenu = Factory.New<StmMenuItem>();
			var menuPivot = Factory.New<StmMenuMenuPivot>();
			menuPivot.SF_SU_Inward = MenuItem.PK;
			menuPivot.SF_SU_Outward = childMenu.PK;

			var template = Factory.New<StmTemplate>();
			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = MenuItem.PK;
			templatePivot.SI_SO = template.PK;

			MenuItem.SU_PrimaryDocPackItemId = ZGuid.NewZGuid();

			AssertHasError(MenuItem.SU_PrimaryDocPackItemIdInfo, StmMenuItemValidation.SU_PrimaryDocPackItemError);
		}

		public void TestValidateSU_PrimaryDocPackItem_PrimaryDocMenuPivotFromAnotherMenuItem_ShouldHaveError()
		{
			var childMenu = Factory.New<StmMenuItem>();
			var menuPivot = Factory.New<StmMenuMenuPivot>();
			menuPivot.SF_SU_Inward = childMenu.PK;
			menuPivot.SF_SU_Outward = childMenu.PK;

			var template = Factory.New<StmTemplate>();
			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = MenuItem.PK;
			templatePivot.SI_SO = template.PK;

			MenuItem.SU_PrimaryDocPackItemId = childMenu.PK;

			AssertHasError(MenuItem.SU_PrimaryDocPackItemIdInfo, StmMenuItemValidation.SU_PrimaryDocPackItemError);
		}

		public void TestValidateSU_PrimaryDocPackItem_PrimaryDocTemplatePivotFromAnotherMenuItem_ShouldHaveError()
		{
			var childMenu = Factory.New<StmMenuItem>();
			var menuPivot = Factory.New<StmMenuMenuPivot>();
			menuPivot.SF_SU_Inward = MenuItem.PK;
			menuPivot.SF_SU_Outward = childMenu.PK;

			var template = Factory.New<StmTemplate>();
			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = childMenu.PK;
			templatePivot.SI_SO = template.PK;

			MenuItem.SU_PrimaryDocPackItemId = templatePivot.PK;

			AssertHasError(MenuItem.SU_PrimaryDocPackItemIdInfo, StmMenuItemValidation.SU_PrimaryDocPackItemError);
		}

		StmMenuItem MenuItem;
		protected override void SetUp()
		{
			base.SetUp();
			MenuItem = Factory.New<StmMenuItem>();
		}
	}
}
