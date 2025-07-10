using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class TWCustomsSubscribersXmlCredentialConfigurationHandler : GlbExternalPasswordConfigurationHandler
	{
		public TWCustomsSubscribersXmlCredentialConfigurationHandler(LoggingInformation logger)
		: base(logger)
		{ }

		protected override bool ProcessStaffOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems, Group otherGroup, ItemData otherItems)
		{
			var result = true;
			var staff = GetStaff(staffGroup.Reference);
			var group = groupGroup == null ? null : GetGroup(groupGroup.Reference);
			var company = companyGroup == null ? null : GetCompany(companyGroup.Reference);
			var externalPassword = (company == null || staff == null) ? null : GetTWExternalPassword(otherGroup, company, group, staff);
			if (externalPassword == null)
			{
				logger.LogWarning(Res.GetString("62F91E90-5ED4-410D-8160-9B48FC0FD7D4", "No password  data be found matching (Type='{0}', Company='{1}', Group='{2}', Staff='{3}').", GetPasswordType(otherGroup), company?.GC_Code ?? "NONE", group?.GG_Code ?? "NONE", staff?.GS_Code ?? "NONE"));
			}
			else if (configuration.TimestampSpecified && configuration.Timestamp.CompareTo(externalPassword.GP_SystemLastEditTimeUtc) < 0)
			{
				logger.LogWarning(Res.GetString("2CFF170F-0F32-4DF8-84DC-ABC7BA0ADD21", "Password (Type='{0}', Mail Box='{1}', Staff='{2}') has newer changes than XML data.", externalPassword.GP_PasswordType, externalPassword.GP_MailBoxID, staff?.GS_Code ?? "NONE"));
			}
			else
			{
				ProcessCredential(externalPassword, otherGroup, null, GetStatusReason(systemGroup, companyGroup, groupGroup, staffGroup, otherGroup));
			}
			return result;
		}

		ZString GetPasswordType(Group groupData) => groupData?.Items?.Cast<Item>()?.FirstOrDefault(x => x.Name == MasterFiles.Business.Customs.XmlCredential.Constants.ItemTypes.Platform)?.Value ?? ZString.Empty;

		MasterFiles.Business.GlbExternalPassword GetTWExternalPassword(Group groupData, GlbCompany company, GlbGroup group, GlbStaff staff)
		{
			MasterFiles.Business.GlbExternalPassword result = null;
			var passwordType = GetPasswordType(groupData);
			var mailBoxID = groupData.Reference;
			if (!passwordType.IsEmpty && !mailBoxID.IsEmpty)
			{
				var query = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, passwordType);
				query.AddToFilter(GlbExternalPasswordSchema.GP_MailBoxID, mailBoxID);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GC, company?.PK);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GG, group?.PK);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GS, staff?.PK);
				result = Factory.LoadTop1<MasterFiles.Business.GlbExternalPassword>(query);
				if (result != null)
				{
					result.DisableConfigurationToSender = true;
				}
			}
			return result;
		}
	}
}
