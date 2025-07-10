using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.TW.Business
{
	internal class TWNCATKClientCredencialSender
	{
		public static void SendSettingCreateOrUpdate(BusinessObjectFactory factory, Guid companyPK, ZString userName)
		{
			SendSetting(factory, companyPK, userName);
		}

		public static void SendSettingDelete(BusinessObjectFactory factory, Guid companyPK)
		{
			SendSetting(factory, companyPK, ZString.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Const string for xml.")]
		const string Company = "Company";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Const string for xml.")]
		const string Current = "Current";

		static void SendSetting(BusinessObjectFactory factory, Guid companyPk, ZString userName)
		{
			var credentialSender = new CredentialSender(Constants.TWNCATKClient.ConfigNameForeService);

			var company = factory.Load<GlbCompany>(companyPk);
			var groupCompany = new Group() { Type = Company, Reference = company.GC_Code };
			var credential = CredentialSender.CreateCredential(Current, userName, ZString.Empty);
			groupCompany.Items = new[] { credential };
			credentialSender.AddItems(groupCompany);

			credentialSender.SendCredential(factory);
		}
	}
}
