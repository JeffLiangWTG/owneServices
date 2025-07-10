using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Registry;

sealed class FTPSettingsRegistryActionHandler : IFTPSettingsRegistryActionHandler
{
	RegistryUpdateActionDelegate IFTPSettingsRegistryActionHandler.UpdateFtpCustomsSettings => (companyPk, _, _, currentValue) =>
	{
		var ftpSettings = currentValue as FTPSettingsCustomsRegistry ?? FTPSettingsCustomsRegistry.DefaultValues;
		SendCredentials(companyPk, ftpSettings, (settings, ftp) =>
		{
			ftp.SendFolder = settings.SendToCustomFolder.IsEmpty ? null : settings.SendToCustomFolder;
			ftp.ReceiveFolder = settings.ReceiveFromCustomFolder.IsEmpty ? null : settings.ReceiveFromCustomFolder;
		});
	};

	Action IFTPSettingsRegistryActionHandler.Save => () => { Factory.Save(); Factory.CleanUp(); factory = null; };

	void SendCredentials<TFtpSettingsRegistry>(Guid companyPK, TFtpSettingsRegistry ftpSettings, Action<TFtpSettingsRegistry, FTP> fillFtpDetails = null)
		where TFtpSettingsRegistry : FTPSettingsRegistry
	{
		var company = (GlbCompany)Companies.FindByPK(companyPK);
		var credentialSender = new CredentialSender(ConfigurationName, ApplicationCodeList.Codes.NOCustoms, CredentialRecipient.DirectxT);
		var ftp = CredentialSender.CreateFTP(ftpSettings.Url, new ZInt(ftpSettings.Port), ftpSettings.Username, ftpSettings.Password);
		fillFtpDetails?.Invoke(ftpSettings, ftp);
		var reference = company?.GC_Code ?? ZString.Empty;
		var companyGroup = CredentialSender.CreateGroup(companyType, reference, GroupStatus.RegisterOrUpdateFtpSettings);
		companyGroup.Items = [ftp];

		credentialSender.AddItems(companyGroup);
		_ = credentialSender.SendCredential(Factory);
	}

	GlbCompanyCollection Companies => companies ??= new(Factory);
	GlbCompanyCollection companies;

	BusinessObjectFactory Factory => factory ??= new() { NameForDebugging = nameof(NOCustomsDataRegistry) };
	BusinessObjectFactory factory;

	readonly string companyType = (NoResString)"Company";
	const string ConfigurationName = "NOCTietoEvry";

	static class GroupStatus
	{ 
		public const string RegisterOrUpdateFtpSettings = "VAL";
	}
}
