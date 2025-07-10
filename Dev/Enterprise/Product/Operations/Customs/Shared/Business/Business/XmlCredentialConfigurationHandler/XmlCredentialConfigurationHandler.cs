using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.XmlCredential
{
	public abstract class XmlCredentialConfigurationHandler : IXmlCredentialConfigurationHandler
	{
		protected XmlCredentialConfigurationHandler(LoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		protected readonly LoggingInformation logger;

		public bool Process(Configuration configuration)
		{
			bool processOK = false;
			try
			{
				logger.OnLogInfoAdded -= Logger_OnLogInfoAdded;
				logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
				Factory = new BusinessObjectFactory();
				hasErrorLog = false;

				if (configuration != null && configuration.IsSpecified && configuration.Group.IsSpecified)
				{
					processOK = true;
					foreach (var group in configuration.Group.OfType<Group>().Where(x => x.IsSpecified))
					{
						if (group.Type.EqualsIgnoringCase(Constants.GroupTypes.SystemType))
						{
							processOK &= ProcessSystemLevel(configuration, group, GetItemData(group));
						}
						else
						{
							logger.LogError(Res.GetString("{BDA7B7A9-F204-4FF6-B942-DA092FC9E238}", "Expected group type to be 'System' but '{0}' was found.", group.Type));
							processOK = false;
						}
					}
				}
				if (processOK && !hasErrorLog)
				{
					try
					{
						Factory.Save();
					}
					catch (ZSaveException ex)
					{
						processOK = false;
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
			finally
			{
				logger.OnLogInfoAdded -= Logger_OnLogInfoAdded;
				Factory = null;
			}
			return processOK && !hasErrorLog;
		}

		void Logger_OnLogInfoAdded(string log, Integration.LogType logType)
		{
			if (!hasErrorLog && logType == Integration.LogType.Error)
			{
				hasErrorLog = true;
			}
		}
		bool hasErrorLog;

		protected BusinessObjectFactory Factory { get; private set; }

		protected virtual bool ProcessSystemLevel(Configuration configuration, Group systemGroup, ItemData systemItems)
		{
			var result = systemItems != null;
			if (result)
			{
				foreach (var group in systemItems.Groups)
				{
					switch (group.Type)
					{
						case Constants.GroupTypes.CompanyType:
							result &= ProcessSystemCompanyLevel(configuration, systemGroup, systemItems, group, GetItemData(group));
							break;
						case Constants.GroupTypes.BranchType:
							result &= ProcessBranchLevel(configuration, systemGroup, systemItems, null, null, group, GetItemData(group));
							break;
						case Constants.GroupTypes.GroupType:
							result &= ProcessGroupLevel(configuration, systemGroup, systemItems, null, null, group, GetItemData(group));
							break;
						case Constants.GroupTypes.StaffType:
							result &= ProcessStaffLevel(configuration, systemGroup, systemItems, null, null, null, null, group, GetItemData(group));
							break;
						default:
							result &= ProcessSystemOtherLevel(configuration, systemGroup, systemItems, group, GetItemData(group));
							break;
					}
				}
			}
			return result;
		}

		protected abstract bool ProcessSystemOtherLevel(Configuration configuration, Group systemGroup, ItemData items, Group otherGroup, ItemData otherItems);

		protected virtual bool ProcessSystemCompanyLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems)
		{
			var result = companyItems != null;
			if (result)
			{
				foreach (var group in companyItems.Groups)
				{
					switch (group.Type)
					{
						case Constants.GroupTypes.BranchType:
							result &= ProcessBranchLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, group, GetItemData(group));
							break;
						case Constants.GroupTypes.GroupType:
							result &= ProcessGroupLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, group, GetItemData(group));
							break;
						case Constants.GroupTypes.StaffType:
							result &= ProcessStaffLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, null, null, group, GetItemData(group));
							break;
						default:
							result &= ProcessCompanyOtherLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, group, GetItemData(group));
							break;
					}
				}
			}
			return result;
		}

		protected abstract bool ProcessCompanyOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group otherGroup, ItemData otherItems);
		protected virtual bool ProcessBranchLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group branchGroup, ItemData branchItems)
		{
			var result = branchItems != null;
			if (result)
			{
				foreach (var group in branchItems.Groups)
				{
					result &= ProcessBranchOtherLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, branchGroup, branchItems, group, GetItemData(group));
				}
			}
			return result;
		}

		protected abstract bool ProcessBranchOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group branchGroup, ItemData branchItems, Group group, ItemData itemData);

		protected virtual bool ProcessGroupLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems)
		{
			var result = groupItems != null;
			if (result)
			{
				foreach (var group in groupItems.Groups)
				{
					switch (group.Type)
					{
						case Constants.GroupTypes.StaffType:
							result &= ProcessStaffLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, groupGroup, groupItems, group, GetItemData(group));
							break;
						default:
							result &= ProcessGroupOtherLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, groupGroup, groupItems, group, GetItemData(group));
							break;
					}
				}
			}
			return result;
		}

		protected virtual bool ProcessStaffLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems)
		{
			var result = staffItems != null;
			if (result)
			{
				foreach (var group in staffItems.Groups)
				{
					result &= ProcessStaffOtherLevel(configuration, systemGroup, systemItems, companyGroup, companyItems, groupGroup, groupItems, staffGroup, staffItems, group, GetItemData(group));
				}
			}
			return result;
		}

		protected abstract bool ProcessStaffOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems, Group otherGroup, ItemData otherItems);

		protected abstract bool ProcessGroupOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group otherGroup, ItemData otherItems);

		protected Credential GetCredential(ItemData itemData, ZString name)
		{
			return itemData?.Credentials?.FirstOrDefault(x => x.Name == name);
		}

		protected EmailDef CreateEmailNotification(ZString subject, ZString body)
		{
			var email = new EmailDef();
			email.Subject = subject;
			email.Body = body;
			return email;
		}

		protected ZString GetDecrypted(byte[] password)
		{
			return CargoWise.eServices.Encryption.Common.Constants.Converter.GetString(CargoWise.eServices.Encryption.Client.Decryptor.EhubClientDecryptor.DecryptBinary(password));
		}

		protected GlbCompany GetCompany(ZString code)
		{
			return GetBizObj<GlbCompany>(code, Constants.GroupTypes.CompanyType, GlbCompanySchema.GC_Code);
		}

		protected GlbGroup GetGroup(ZString code)
		{
			return GetBizObj<GlbGroup>(code, Constants.GroupTypes.GroupType, GlbGroupSchema.GG_Code);
		}

		protected GlbStaff GetStaff(ZString code)
		{
			return GetBizObj<GlbStaff>(code, Constants.GroupTypes.StaffType, GlbStaffSchema.GS_Code);
		}

		protected T GetBizObj<T>(ZString code, ZString bizObjType, SchemaStringColumn column)
			where T : BusinessObject
		{
			T result = null;
			var dictionary = Factory.GetCachedValue(string.Format(Culture.Invariant, (NoResString)"XmlCredential{0}Dictionary", bizObjType), () => new Dictionary<ZString, T>());
			if (!dictionary.TryGetValue(code, out result))
			{
				if (code.IsEmpty)
				{
					logger.LogError(Res.GetString("{C50B99B9-83EC-42C1-898F-B437E59A9832}", "{0} reference cannot be empty.", bizObjType));
				}
				else
				{
					result = Factory.LoadFromNaturalKey<T>(column, code);
					if (result == null)
					{
						logger.LogError(Res.GetString("{40D39840-FF81-472D-87D6-18FFA5490F8A}", "Could not find {0} with reference '{1}'.", bizObjType, code));
					}
					dictionary.Add(code, result);
				}
			}
			return result;
		}

		ItemData GetItemData(Group group)
		{
			ItemData result = null;
			if (group.Items != null)
			{
				result = new ItemData();
				foreach (var item in group.Items)
				{
					var groupData = item as Group;
					if (groupData == null)
					{
						var fileData = item as File;
						if (fileData == null)
						{
							var certificateData = item as Certificate;
							if (certificateData == null)
							{
								var credentialData = item as Credential;
								if (credentialData == null)
								{
									var ftpData = item as FTP;
									if (ftpData == null)
									{
										var itemData = item as Item;
										if (itemData == null)
										{
											logger.LogError(Res.GetString("{C88B2619-B685-4362-8E90-89F90DED3FF6}", "Unknown Group.Item type '{0}'.", item.GetType().FullName));
										}
										else if (itemData.IsSpecified)
										{
											result.Items.Add(itemData);
										}
									}
									else if (ftpData.IsSpecified)
									{
										result.FTPs.Add(ftpData);
									}
								}
								else if (credentialData.IsSpecified)
								{
									result.Credentials.Add(credentialData);
								}
							}
							else if (certificateData.IsSpecified)
							{
								result.Certificates.Add(certificateData);
							}
						}
						else if (fileData.IsSpecified)
						{
							result.Files.Add(fileData);
						}
					}
					else if (groupData.IsSpecified)
					{
						result.Groups.Add(groupData);
					}
				}
			}
			return result;
		}

		public class ItemData
		{
			public List<File> Files => files ?? (files = new List<File>());
			List<File> files;

			public List<Certificate> Certificates => certificates ?? (certificates = new List<Certificate>());
			List<Certificate> certificates;

			public List<Credential> Credentials => credentials ?? (credentials = new List<Credential>());
			List<Credential> credentials;

			public List<FTP> FTPs => ftps ?? (ftps = new List<FTP>());
			List<FTP> ftps;

			public List<Group> Groups => groups ?? (groups = new List<Group>());
			List<Group> groups;

			public List<Item> Items => items ?? (items = new List<Item>());
			List<Item> items;
		}
	}
}
