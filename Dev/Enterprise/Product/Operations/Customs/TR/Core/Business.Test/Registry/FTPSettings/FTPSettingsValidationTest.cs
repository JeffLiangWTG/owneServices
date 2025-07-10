using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class FTPSettingsValidationTest : BusinessObjectValidationTestCase
	{
		readonly FallbackLevel fallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
		readonly string englishCharactersErrorMessage = " only accepts Western European languages characters.";
		readonly string pathStartsWithSlashErrorMessage = "The text must start with a forward slash (/).";

		FTPSettings CreateFTPSettings() => new FTPSettings(fallbackLevel, Factory);

		public void TestCheckEnglishCharacters()
		{
			var fTPSettings = CreateFTPSettings();

			fTPSettings.ExportUnionUserCode = "yiğit";
			fTPSettings.ExportUnionUserPassword = "erşan";
			fTPSettings.ExportUnionPaymentPassword = "yiğiterşan";

			AssertHasErrorContaining(fTPSettings.ExportUnionUserCodeInfo, "Export Union User Code" + englishCharactersErrorMessage);
			AssertHasErrorContaining(fTPSettings.ExportUnionUserPasswordInfo, "Export Union User Password" + englishCharactersErrorMessage);
			AssertHasErrorContaining(fTPSettings.ExportUnionPaymentPasswordInfo, "Export Union Payment Password" + englishCharactersErrorMessage);
		}

		public void TestCheckExportUnionUserPassword()
		{
			var fTPSettings = CreateFTPSettings();
			var fTPSettingsValidation = new FTPSettingsValidation(fTPSettings);

			fTPSettings.ExportUnionUserCode = "TEST";
			fTPSettings.ExportUnionUserPassword = "";

			fTPSettingsValidation.CheckExportUnionUserPassword();

			AssertHasErrorContaining(fTPSettings.ExportUnionUserPasswordInfo, MandatoryValidation.MustBeEntered);

			fTPSettings.ExportUnionUserCode = "";
			AssertNoErrorContaining(fTPSettings.ExportUnionUserPasswordInfo, MandatoryValidation.MustBeEntered);

			fTPSettings.ExportUnionUserPassword = "password";
			AssertNoErrorContaining(fTPSettings.ExportUnionUserPasswordInfo, MandatoryValidation.MustBeEntered);

			fTPSettings.ExportUnionUserCode = "TEST";
			fTPSettings.ExportUnionUserPassword = "password";
			AssertNoErrorContaining(fTPSettings.ExportUnionUserPasswordInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckPathStartsWithSlash()
		{
			var fTPSettings = CreateFTPSettings();
			var fTPSettingsValidation = new FTPSettingsValidation(fTPSettings);
			void AssertPathValidation(ZString text, ZPropertyInfo propertyInfo, bool shouldHaveError)
			{
				fTPSettings.SetPropertyValue(propertyInfo.Name, text);
				fTPSettingsValidation.ValidateAll();
				if (shouldHaveError)
				{
					AssertHasMessageErrorContaining(propertyInfo, pathStartsWithSlashErrorMessage);
				}
				else
				{
					AssertNoMessageErrorContaining(propertyInfo, pathStartsWithSlashErrorMessage);
				}
			}

			CombineAssertions("Inbox & Outbox Path Validations", () =>
			{
				AssertPathValidation("test", fTPSettings.InboxInfo, true);
				AssertPathValidation("/test", fTPSettings.InboxInfo, false);
				AssertPathValidation("", fTPSettings.InboxInfo, false);
				AssertPathValidation(null, fTPSettings.InboxInfo, false);

				AssertPathValidation("test", fTPSettings.OutboxInfo, true);
				AssertPathValidation("/test", fTPSettings.OutboxInfo, false);
				AssertPathValidation("", fTPSettings.OutboxInfo, false);
				AssertPathValidation(null, fTPSettings.OutboxInfo, false);
			});
		}
	}
}
