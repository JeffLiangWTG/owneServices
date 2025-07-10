using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyConfigurationToSender
	{
		public static void RegisterForSending(BusinessObjectFactory factory, ZString configurationName, GlbCompany company, string interchangeType = EDIInterchangeTypeList.Codes.Configuration)
		{
			if (factory != null && !configurationName.IsEmpty)
			{
				if (!Senders.TryGetValue(factory, out var sender))
				{
					sender = new GlbCompanyConfigurationToSender(interchangeType);
					factory.Saved += sender.Factory_Saved;
					Senders.Add(factory, sender);
				}

				sender.ConfigurationsToBeSend.Add(new SendingRegistration(configurationName, company));
			}
		}

		protected static Dictionary<BusinessObjectFactory, GlbCompanyConfigurationToSender> Senders => senders ??= new Dictionary<BusinessObjectFactory, GlbCompanyConfigurationToSender>();
		[ThreadStatic]
		static Dictionary<BusinessObjectFactory, GlbCompanyConfigurationToSender> senders;

		internal List<SendingRegistration> ConfigurationsToBeSend;

		internal class SendingRegistration : IEquatable<SendingRegistration>
		{
			public SendingRegistration(ZString configurationName, GlbCompany company)
			{
				ConfigurationName = configurationName;
				CompanyCode = company?.GC_Code ?? ZString.Empty;
				CompanyPK = company?.PK ?? ZGuid.Empty;
			}
			public ZString ConfigurationName { get; }
			public ZString CompanyCode { get; }
			public ZGuid CompanyPK { get; }

			public bool Equals(SendingRegistration other) => ConfigurationName == other.ConfigurationName && CompanyCode == other.CompanyCode && CompanyPK == other.CompanyPK;

			internal bool IsValid() => !this.ConfigurationName.IsEmpty && !CompanyPK.IsEmpty;

			internal ZQuery GetQueryFilter()
			{
				var result = new ZQuery();
				result.AddToFilter(GlbCompanySchema.PK, CompanyPK.IsValid ? (ZGuid?)CompanyPK : null);
				return result;
			}
		}

		protected GlbCompanyConfigurationToSender(ZString interchangeType)
		{
			this.interchangeType = interchangeType;
			ConfigurationsToBeSend = new List<SendingRegistration>();
		}
		readonly ZString interchangeType;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (factory != null)
			{
				factory.Saved -= Factory_Saved;
				if (savedSuccessfully)
				{
					if (CollectAndPopulateExternalDataToBeSend(factory))
					{
						SendUpdatedGlbCompany();
					}
					ConfigurationsToBeSend.Clear();
					ConfigurationsToBeSend = null;
				}
				Senders.Remove(factory);
				if (Senders.Count == 0)
				{
					senders = null;
				}
			}
		}

		bool CollectAndPopulateExternalDataToBeSend(BusinessObjectFactory factory)
		{
			var result = false;
			if (ConfigurationsToBeSend.Count > 0)
			{
				var distinctConfigurations = ConfigurationsToBeSend.Where(x => x.IsValid()).Distinct();
				foreach (var conf in distinctConfigurations)
				{
					Add(conf.ConfigurationName, conf.CompanyCode, factory.LoadTop1<GlbCompany>(conf.GetQueryFilter()));
					result = true;
				}
			}
			return result;
		}

		void Add(ZString configurationName, ZString companyCode, GlbCompany company)
		{
			if (!ConfigurationData.TryGetValue(configurationName, out var systemGroupData))
			{
				systemGroupData = new SystemGroupData();
				ConfigurationData.Add(configurationName, systemGroupData);
			}

			systemGroupData.AddOrUpdateCompanyGroupData(companyCode);
			systemGroupData.AddCustomData(company);
		}

		void SendUpdatedGlbCompany()
		{
			if (!configurationData.IsNullOrEmpty())
			{
				foreach (var configuration in configurationData)
				{
					var recipient = CredentialRecipient.DirectxT;
					var sender = new CredentialSender(configuration.Key, interchangeType, recipient);

					var systemGroupData = configuration.Value;
					if (systemGroupData != null)
					{
						sender.AddItems(systemGroupData.CreateCredentials().ToArray());
					}

					sender.SendCredential();
				}
			}
		}

		Dictionary<ZString, SystemGroupData> ConfigurationData => configurationData ??= new Dictionary<ZString, SystemGroupData>();
		Dictionary<ZString, SystemGroupData> configurationData;

		class SystemGroupData : CompanyGroupData
		{
			public CompanyGroupData AddOrUpdateCompanyGroupData(ZString companyCode)
			{
				if (!CompanyGroupData.TryGetValue(companyCode, out var groupData))
				{
					groupData = new CompanyGroupData();
					CompanyGroupData.Add(companyCode, groupData);
				}

				return groupData;
			}

			Dictionary<ZString, CompanyGroupData> CompanyGroupData => companyGroupData ??= new Dictionary<ZString, CompanyGroupData>();
			Dictionary<ZString, CompanyGroupData> companyGroupData;
		}

		class CompanyGroupData
		{
			public void AddCustomData(GlbCompany company)
			{
				if (company != null && !Companies.Contains(company))
				{
					Companies.Add(company);
				}
			}

			public IEnumerable<object> CreateCredentials()
			{
				if (!companies.IsNullOrEmpty())
				{
					foreach (var glbCompany in companies)
					{
						if (glbCompany != null)
						{
							yield return glbCompany.CredentialData.CreateCredential(glbCompany);
						}
					}
				}
			}

			List<GlbCompany> Companies => companies ??= new List<GlbCompany>();
			List<GlbCompany> companies;
		}
	}
}
