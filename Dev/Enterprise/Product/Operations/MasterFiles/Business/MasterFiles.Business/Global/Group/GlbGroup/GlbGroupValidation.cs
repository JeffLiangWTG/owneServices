using System;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupValidation : AutoGlbGroupValidation
	{
		public GlbGroupValidation(AutoGlbGroup parent)
			: base(parent)
		{
		}

		new GlbGroup Parent
		{
			get { return (GlbGroup)base.Parent; }
		}

		#region GG_Type

		protected override void CheckGG_Type()
		{
			base.CheckGG_Type();
			MandatoryValidation.CheckEntered(Parent.GG_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GG_TypeInfo);
		}

		#endregion

		#region GG_Code

		protected override void CheckGG_Code()
		{
			base.CheckGG_Code();
			MandatoryValidation.CheckEntered(Parent.GG_CodeInfo);
			if (!Parent.GG_Code.IsEmpty)
			{
				if (!IsUniqueOnGroup(GlbGroupSchema.GG_Code, Parent.GG_Code))
				{
					DuplicateCodeError();
				}
			}
		}

		bool IsUniqueOnGroup(SchemaColumn column, object fieldValue)
		{
			ZQuery query = new ZQuery(column, fieldValue);
			query.AddToFilter(JoinCondition.And, GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return !Parent.Factory.Exists(typeof(GlbGroup), query);
		}

		protected virtual void DuplicateCodeError()
		{
			Parent.GG_CodeInfo.AddError(Res.GetString("8cb41df7-980d-439a-a89b-f76e2112e7f0", "Group Code must be unique in the system."));
		}

		#endregion

		#region GG_Desc

		protected override void CheckGG_Desc()
		{
			base.CheckGG_Desc();
			MandatoryValidation.CheckEntered(Parent.GG_DescInfo);
			var registry = ObjectFactory.Get<IADRegistry>();

			if (registry.IsIntegrationEnabled && !Parent.GG_IsSystemDefined)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.GG_DescInfo, Parent.Factory.Load(Parent.GetType(), new ZQuery(GlbGroupSchema.GG_Desc, Parent.GG_Desc)));

				var expectedPrefix = ObjectFactory.Get<IADRegistry>().GroupNamePrefix;

				if (!string.IsNullOrEmpty(expectedPrefix) && !Parent.GG_Desc.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
				{
					Parent.GG_DescInfo.AddError(Res.GetString("5a5f3554-c95c-44ec-bdbb-fdd0a6672bb7", "Description must begin with '{0}'.", expectedPrefix));
				}

				if (registry.EntitiesToSync == EntitiesToSync.UsersAndGroups)
				{
					if (IsGroupDescInConflictWithAnyLinkedADObject())
					{
						Parent.GG_DescInfo.AddError(Res.GetString("D298C90E-B420-494B-875F-3DBC5DB52D4C", "Description already used in Active Directory and linked to another object."));
					}
				}
			}
		}

		bool IsGroupDescInConflictWithAnyLinkedADObject()
		{
			try
			{
				var domainCredentials = ObjectFactory.Get<IADEntityProvider>().GetADGroup(Parent).DomainCredentials;
				var directorySearcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(domainCredentials, false);
				var adGroup = directorySearcher.FindGroup(Parent.GG_Desc, domainCredentials?.GroupOrganisationalUnit);
				if (adGroup != null)
				{
					if (adGroup.Guid != Parent.GG_ActiveDirectoryObjectGuid)
					{
						return Parent.Factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, adGroup.Guid)) != null;
					}
				}
				else
				{
					// To ensure no user has the same login name as the group in Group's OU
					return directorySearcher.FindUser(Parent.GG_Desc, domainCredentials?.GroupOrganisationalUnit) != null;
				}
			}
			catch (Exception ex) when (ex is DirectoryServicesException || ex is InvalidOUException || ex is COMException)
			{
				Parent.GG_DescInfo.AddError(Res.GetString("34F97F12-743C-43A0-8CE6-5E38384F4848", @"Cannot connect to Active Directory: {0}.
Please contact your system administrator or try again later.", ex.Message));
			}
			return false;
		}

		#endregion

		#region GG_IsActive

		protected override void CheckGG_IsActive()
		{
			base.CheckGG_IsActive();
			if (!Parent.GG_IsActive)
			{
				if (Parent.GG_IsSystemDefined)
				{
					Parent.GG_IsActiveInfo.AddError(Res.GetString("7b494fbf-65eb-4539-a30d-a31eb01ddac5", "The '{0}' group may not be deactivated", Parent.GG_Code));
				}
				else
				{
					if (Parent.Staff.Count > 0)
					{
						Parent.GG_IsActiveInfo.AddError(Res.GetString("5B04F421-943E-4625-B184-16B15668DE60", "Members need to be Detached from the Group."));
					}

					if (Parent.IsADLinked)
					{
						Parent.GG_IsActiveInfo.AddError(Res.GetString("63093cff-c1f7-4165-8f91-6f175aba3635", "Groups linked to Active Directory cannot be deactivated. The group should be disconnected from Active Directory first."));
					}
				}
			}
		}

		#endregion

		#region GG_ActiveDirectoryObjectGuidIsValidZGuid

		protected override void CheckGG_ActiveDirectoryObjectGuidIsValidZGuid()
		{
			// We use ZGuid.Invalid to indicate a new group record needs to be created in AD
		}

		#endregion

		#region GG_IsSecurityEnabled / Non-Security

		protected override void CheckGG_IsSecurityEnabled()
		{
			base.CheckGG_IsSecurityEnabled();
			if (Parent.GG_IsSecurityEnabled)
			{
				if (Parent.GG_IsSecurityEnabledInfo.HasChanges)
				{
					Parent.GG_IsSecurityEnabledInfo.AddWarning(Res.GetString("31b4c548-39f0-491f-8bee-4ae8007aeae5", "Please save and re-open this Group to edit security rights."));
				}
				if (!Parent.GG_GG_ParentGroup.IsEmpty)
				{
					Parent.GG_IsSecurityEnabledInfo.AddError(SecurityGroupsCannotHaveParentErrorMessage);
				}
			}
		}

		#endregion

		#region GG_GC

		protected override void CheckGG_GC()
		{
			base.CheckGG_GC();

			if (Parent.GG_IsSales && !Parent.IsGlobal)
			{
				MandatoryValidation.CheckEntered(Parent.GG_GCInfo);
			}
		}

		#endregion

		#region  GlbStaff

		protected void Check_GlbStaff()
		{
			string glbStaffErrorMessage = Res.GetString("16C945F4-2563-4227-98EF-198F3A8C2D6C", "Group members must have at least one valid email address because group is assigned to recipient of scheduled reports.");

			this.Parent.RemoveRowError(glbStaffErrorMessage);
			if (Parent.IsInDatabase && !HasValidateEmailAddressToScheduleReportRecipient(Parent.Staff, Parent.PK))
			{
				this.Parent.AddRowError(glbStaffErrorMessage);
			}
		}

		public bool HasValidateEmailAddressToScheduleReportRecipient(GlbStaffManyToManyCollection staffs, ZGuid pk)
		{
			var query = new ZQuery();
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryMethod, Constants.ContactNotifyModes.Email);
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryToType, ScheduledReportDeliveryRecipientConstants.RecipientType.Group);
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_GG, pk);

			var countForRecipient = Parent.Factory.Load(ObjectFactory.GetType<IStmScheduleTaskRecipient>(), query).Length;

			if (countForRecipient > 0)
			{
				return staffs.Cast<GlbStaff>().Any(s => !s.IsDeleted && !string.IsNullOrEmpty(s.GS_EmailAddress));
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region GG_DomainName

		protected override void CheckGG_DomainName()
		{
			base.CheckGG_DomainName();
			if (!Parent.GG_IsSystemDefined)
			{
				if (ObjectFactory.Get<IADRegistry>().DomainCredentialsCollection.Any())
				{
					ListValidation.ErrorIfInvalidCode(Parent.GG_DomainNameInfo);
				}

				if (Parent.GG_DomainName.IsEmpty && (Parent.IsADIntegrationEnabled || ObjectFactory.Get<IADRegistry>().HasMultipleDomains))
				{
					Parent.GG_DomainNameInfo.AddWarning(Res.GetString("0fdfb7a7-75fe-4135-af16-23a27d9011dc", "The default domain will be used during the next synchronization with Active Directory."));
				}
			}
		}

		#endregion

		#region GG_Category

		protected override void CheckGG_Category()
		{
			base.CheckGG_Category();
			ListValidation.ErrorIfInvalidCode(Parent.GG_CategoryInfo);
		}

		#endregion

		#region GG_GG_ParentGroup

		protected override void CheckGG_GG_ParentGroup()
		{
			base.CheckGG_GG_ParentGroup();
			ListValidation.ErrorIfInvalidPK(Parent.GG_GG_ParentGroupInfo);
			if (!Parent.GG_GG_ParentGroup.IsEmpty && Parent.GG_IsSecurityEnabled)
			{
				Parent.GG_GG_ParentGroupInfo.AddError(SecurityGroupsCannotHaveParentErrorMessage);
			}
		}

		static string SecurityGroupsCannotHaveParentErrorMessage => Res.GetString("479dba09-b250-4fa4-b4cb-329b963249d5", "Security groups must not have a Parent Group specified.");

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();

			Check_GlbStaff();
		}
	}
}
