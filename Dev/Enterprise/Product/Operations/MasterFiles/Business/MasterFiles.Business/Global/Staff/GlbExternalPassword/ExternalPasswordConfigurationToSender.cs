using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Customs.XmlCredential
{
	public class ExternalPasswordConfigurationToSender
	{
		protected ExternalPasswordConfigurationToSender(ZString interchangeType, Action actionOnSuccessfulSending = null)
		{
			this.interchangeType = interchangeType;
			ConfigurationsToBeSend = new List<SendingRegistration>();
			this.actionOnSuccessfulSending = actionOnSuccessfulSending;
		}

		#region Added Registration for Sending

		internal List<SendingRegistration> ConfigurationsToBeSend;

		readonly ZString interchangeType;

		readonly Action actionOnSuccessfulSending;

		internal class SendingRegistration : IEquatable<SendingRegistration>
		{
			public SendingRegistration(ZString configurationName, GlbCompany company, GlbGroup group, GlbStaff staff)
			{
				ConfigurationName = configurationName;
				CompanyCode = company?.GC_Code ?? ZString.Empty;
				GroupCode = group?.GG_Code ?? ZString.Empty;
				StaffCode = staff?.GS_Code ?? ZString.Empty;
				CompanyPK = company?.PK ?? ZGuid.Empty;
				GroupPK = group?.PK ?? ZGuid.Empty;
				StaffPK = staff?.PK ?? ZGuid.Empty;
			}

			internal ZString ConfigurationName;
			internal ZString CompanyCode;
			internal ZString GroupCode;
			internal ZString StaffCode;

			internal ZGuid CompanyPK;
			internal ZGuid GroupPK;
			internal ZGuid StaffPK;

			public bool Equals(SendingRegistration other)
			{
				return ConfigurationName == other.ConfigurationName
					&& CompanyCode == other.CompanyCode
					&& GroupCode == other.GroupCode
					&& StaffCode == other.StaffCode
					&& CompanyPK == other.CompanyPK
					&& GroupPK == other.GroupPK
					&& StaffPK == other.StaffPK;
			}

			internal bool IsValid()
			{
				return !this.ConfigurationName.IsEmpty;
			}

			internal ZQuery GetQueryFilter()
			{
				var result = new ZQuery();
				result.AddToFilter(GlbExternalPasswordSchema.GP_GC, CompanyPK.IsValid ? (ZGuid?)CompanyPK : null);
				result.AddToFilter(GlbExternalPasswordSchema.GP_GG, GroupPK.IsValid ? (ZGuid?)GroupPK : null);
				result.AddToFilter(GlbExternalPasswordSchema.GP_GS, StaffPK.IsValid ? (ZGuid?)StaffPK : null);
				return result;
			}
		}

		public static void RegisterForSending(BusinessObjectFactory factory,
			ZString configurationName,
			GlbCompany company,
			GlbGroup group,
			GlbStaff staff,
			string interchangeType = EDIInterchangeTypeList.Codes.Configuration,
			CredentialRecipient credentialRecipient = CredentialRecipient.eHub,
			Action actionOnSuccessfulSending = null)
		{
			if (factory != null && !configurationName.IsEmpty)
			{
				if (!Senders.TryGetValue(factory, out var sender))
				{
					sender = new ExternalPasswordConfigurationToSender(interchangeType, actionOnSuccessfulSending);
					factory.Saved += sender.Factory_Saved;
					Senders.Add(factory, sender);
				}

				sender.ConfigurationRecipients[configurationName] = credentialRecipient;
				sender.ConfigurationsToBeSend.Add(new SendingRegistration(configurationName, company, group, staff));
			}
		}

		#endregion

		void Add(GlbExternalPassword externalPassword, ZString configurationName, ZString companyCode, ZString groupCode, ZString staffCode)
		{
			if (!ConfigurationDatas.TryGetValue(configurationName, out var systemGroupData))
			{
				systemGroupData = new SystemGroupData();
				ConfigurationDatas.Add(configurationName, systemGroupData);
			}

			var companyGroupData = companyCode.IsEmpty ? systemGroupData : systemGroupData.AddOrUpdateCompanyGroupDatas(companyCode);
			var groupGroupData = groupCode.IsEmpty ? companyGroupData : companyGroupData.AddOrUpdateGroupGroupDatas(groupCode);
			var groupData = staffCode.IsEmpty ? groupGroupData : groupGroupData.AddOrUpdateStaffGroupDatas(staffCode);
			groupData.AddToExternalPassword(externalPassword);
		}

		bool CollectAndPopulateExternalDataToBeSend(BusinessObjectFactory factory)
		{
			var result = false;
			if (ConfigurationsToBeSend.Count > 0)
			{
				var distinctConfigurations = ConfigurationsToBeSend.Where(x => x.IsValid()).Distinct();
				foreach (var conf in distinctConfigurations)
				{
					var confName = conf.ConfigurationName;
					var companyCode = conf.CompanyCode;
					var groupCode = conf.GroupCode;
					var staffCode = conf.StaffCode;

					var loadedGlbExternalPasswords = factory.Load<GlbExternalPassword>(conf.GetQueryFilter())?.Where(x => x.ConfigurationName == confName);
					if (loadedGlbExternalPasswords != null && loadedGlbExternalPasswords.Any())
					{
						foreach (var password in loadedGlbExternalPasswords)
						{
							Add(password, confName, companyCode, groupCode, staffCode);
						}
					}
					else
					{
						Add(null, confName, companyCode, groupCode, staffCode);
					}
					result = true;
				}
			}
			return result;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (factory != null)
			{
				factory.Saved -= Factory_Saved;
				if (savedSuccessfully)
				{
					var needsSending = CollectAndPopulateExternalDataToBeSend(factory);
					if (needsSending)
					{
						SendUpdatedExternalPasswords(factory);
						actionOnSuccessfulSending?.Invoke();
					}
					ConfigurationsToBeSend.RemoveAll(x => true);
					ConfigurationsToBeSend = null;
				}
				Senders.Remove(factory);
				if (Senders.Count == 0)
				{
					senders = null;
				}
			}
		}

		void SendUpdatedExternalPasswords(BusinessObjectFactory factory)
		{
			if (!configurationDatas.IsNullOrEmpty())
			{
				foreach (var configurationData in configurationDatas)
				{
					var recipient = ConfigurationRecipients[configurationData.Key];
					var sender = new CredentialSender(configurationData.Key, interchangeType, recipient);

					var systemGroupData = configurationData.Value;
					if (systemGroupData != null)
					{
						sender.AddItems(systemGroupData.CreateCredential(factory).ToArray());
					}

					sender.SendCredential();
				}
			}
		}

		Dictionary<ZString, SystemGroupData> ConfigurationDatas => configurationDatas ?? (configurationDatas = new Dictionary<ZString, SystemGroupData>());
		Dictionary<ZString, SystemGroupData> configurationDatas;

		Dictionary<ZString, CredentialRecipient> ConfigurationRecipients => configurationRecipients ?? (configurationRecipients = new Dictionary<ZString, CredentialRecipient>());
		Dictionary<ZString, CredentialRecipient> configurationRecipients;

		class SystemGroupData : CompanyGroupData
		{
			public CompanyGroupData AddOrUpdateCompanyGroupDatas(ZString companyCode)
			{
				if (!CompanyGroupDatas.TryGetValue(companyCode, out var groupData))
				{
					groupData = new CompanyGroupData();
					CompanyGroupDatas.Add(companyCode, groupData);
				}
				return groupData;
			}

			public override IEnumerable<object> CreateCredential(BusinessObjectFactory factory)
			{
				return base.CreateCredential(factory).Concat(CreateCompanyCredentials(factory));
			}

			IEnumerable<object> CreateCompanyCredentials(BusinessObjectFactory factory)
			{
				return CreateCredentials(factory, Constants.GroupTypes.CompanyType, companyGroupDatas);
			}

			Dictionary<ZString, CompanyGroupData> CompanyGroupDatas => companyGroupDatas ?? (companyGroupDatas = new Dictionary<ZString, CompanyGroupData>());
			Dictionary<ZString, CompanyGroupData> companyGroupDatas;
		}

		class CompanyGroupData : GroupGroupData
		{
			public GroupGroupData AddOrUpdateGroupGroupDatas(ZString groupCode)
			{
				if (!GroupGroupDatas.TryGetValue(groupCode, out var groupData))
				{
					groupData = new GroupGroupData();
					GroupGroupDatas.Add(groupCode, groupData);
				}
				return groupData;
			}

			public override IEnumerable<object> CreateCredential(BusinessObjectFactory factory)
			{
				return base.CreateCredential(factory).Concat(CreateGroupCredentials(factory));
			}

			IEnumerable<object> CreateGroupCredentials(BusinessObjectFactory factory)
			{
				return CreateCredentials(factory, Constants.GroupTypes.GroupType, groupGroupDatas);
			}

			Dictionary<ZString, GroupGroupData> GroupGroupDatas => groupGroupDatas ?? (groupGroupDatas = new Dictionary<ZString, GroupGroupData>());
			Dictionary<ZString, GroupGroupData> groupGroupDatas;
		}

		class GroupGroupData : GroupData
		{
			public GroupData AddOrUpdateStaffGroupDatas(ZString staffCode)
			{
				if (!StaffGroupDatas.TryGetValue(staffCode, out var groupData))
				{
					groupData = new GroupData();
					StaffGroupDatas.Add(staffCode, groupData);
				}
				return groupData;
			}

			public override IEnumerable<object> CreateCredential(BusinessObjectFactory factory)
			{
				return base.CreateCredential(factory).Concat(CreateStaffCredentials(factory));
			}

			IEnumerable<object> CreateStaffCredentials(BusinessObjectFactory factory)
			{
				return CreateCredentials(factory, Constants.GroupTypes.StaffType, staffGroupDatas);
			}

			protected IEnumerable<object> CreateCredentials<T>(BusinessObjectFactory factory, ZString groupType, IEnumerable<KeyValuePair<ZString, T>> datas)
				where T : GroupData
			{
				if (!datas.IsNullOrEmpty())
				{
					foreach (var data in datas)
					{
						var credential = new Group() { Type = groupType, Reference = data.Key };
						var list = data.Value;
						if (data.Value != null)
						{
							credential.Items = data.Value.CreateCredential(factory).ToArray();
						}
						yield return credential;
					}
				}
			}

			Dictionary<ZString, GroupData> StaffGroupDatas => staffGroupDatas ?? (staffGroupDatas = new Dictionary<ZString, GroupData>());
			Dictionary<ZString, GroupData> staffGroupDatas;
		}

		class GroupData
		{
			public void AddToExternalPassword(GlbExternalPassword externalPassword)
			{
				if (!ExternalPasswords.Contains(externalPassword))
				{
					ExternalPasswords.Add(externalPassword);
				}
			}

			public virtual IEnumerable<object> CreateCredential(BusinessObjectFactory factory)
			{
				if (!externalPasswords.IsNullOrEmpty())
				{
					foreach (var glbExternalPassword in externalPasswords)
					{
						if (glbExternalPassword != null)
						{
							yield return glbExternalPassword.CreateCredentialData();
						}
					}
				}
			}

			List<GlbExternalPassword> ExternalPasswords => externalPasswords ?? (externalPasswords = new List<GlbExternalPassword>());
			List<GlbExternalPassword> externalPasswords;
		}

		protected static Dictionary<BusinessObjectFactory, ExternalPasswordConfigurationToSender> Senders => senders ?? (senders = new Dictionary<BusinessObjectFactory, ExternalPasswordConfigurationToSender>());
		[ThreadStatic]
		static Dictionary<BusinessObjectFactory, ExternalPasswordConfigurationToSender> senders;
	}
}
