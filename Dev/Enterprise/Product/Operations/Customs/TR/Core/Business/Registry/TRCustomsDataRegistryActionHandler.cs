using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	sealed class TRCustomsDataRegistryActionHandler() : ITRCustomsDataRegistryActionHandler
	{
		public RegistryUpdateActionDelegate OnUpdateAction =>
			(companyPk, branchPk, departmentPk, currentValue) =>
			{
				if (currentValue is FTPSettings ftpSettings)
				{
					SendCredentials(companyPk, ftpSettings);
				}
			};

		void SendCredentials(Guid companyPK, FTPSettings credentials)
		{
			var company = (GlbCompany)Companies.FindByPK(companyPK);
			var credentialSender = new CredentialSender(configurationName, TRMessageTypes.Codes.EUT, CredentialRecipient.DirectxT);
			var ftp = CreateFTP(credentials);
			var groupStatus = (credentials.ExportUnionUserCode.IsEmpty || credentials.ExportUnionUserPassword.IsEmpty) ? Constants.CredentialStatusList.Invalid : Constants.CredentialStatusList.Valid;
			CreateGroup(credentialSender, company, ftp, groupStatus);
			credentialSender.SendCredential(Factory);
		}

		FTP CreateFTP(FTPSettings credentials)
		{
			if (credentials.ExportUnionUserCode.IsEmpty || credentials.ExportUnionUserPassword.IsEmpty)
			{
				return new FTP();
			}

			var ftpAddress = credentials.FTPAddressList.GetDescriptionFromCode(credentials.FTPAddress);
			var ftp = CredentialSender.CreateFTP(ftpAddress, new ZInt(credentials.Port), credentials.ExportUnionUserCode, credentials.ExportUnionUserPassword);
			ftp.SendFolder = credentials.Outbox;
			ftp.ReceiveFolder = credentials.Inbox;

			return ftp;
		}

		void CreateGroup(CredentialSender credentialSender, GlbCompany company, FTP ftp, ZString groupStatus)
		{
			var reference = company != null ? company.GC_Code : ZString.Empty;
			var companyGroup = CredentialSender.CreateGroup(companyType, reference, groupStatus);
			companyGroup.Items = new object[] { ftp };
			credentialSender.AddItems(companyGroup);
		}

		public Action OnAllValuesSaved => () => { Factory.Save(); Factory.CleanUp(); factory = null; };

		GlbCompanyCollection Companies => companies ??= new(Factory);
		GlbCompanyCollection companies;

		BusinessObjectFactory Factory => factory ??= new() { NameForDebugging = nameof(TRCustomsDataRegistry) };
		BusinessObjectFactory factory;

		readonly string companyType = (NoResString)"Company";
		const string configurationName = "TREUTUnion";
	}
}
