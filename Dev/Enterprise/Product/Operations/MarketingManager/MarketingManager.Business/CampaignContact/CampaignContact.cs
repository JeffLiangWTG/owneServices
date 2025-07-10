using System;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignContact : AutoViewCampaignContact, IGlbCompanyCampaignItemRecipient, IScheduleItemsProvider
	{
		public CampaignContact(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new abstract class Schema : AutoViewCampaignContact.Schema
		{
			public const string IsNDR = "IsNDR";
			public const string ContactUrl = "ContactUrl";
		}

		public GlbCompanyCampaign Campaign { get; set; }

		public static string EditActionText
		{
			get { return Res.GetString("9df0cf88-8aa4-468f-8e76-9a7d2f9e9c19", "Edit"); }
		}
		public static string DeactivateActionText
		{
			get { return Res.GetString("02d6437d-673e-4690-96a6-0b97ff5c64c4", "Deactivate"); }
		}

		public override bool CanDelete
		{
			get
			{
				return false;
			}
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		#region Branch Address

		public OrgAddress BranchAddress
		{
			get { return Factory.Load<OrgAddress>(VCC_OA); }
		}

		#endregion

		#region Header

		public virtual OrgHeader Header
		{
			get { return Factory.Load<OrgHeader>(VCC_OH); }
		}

		public ZString FullNameTruncated
		{
			get
			{
				if (VCC_OrgFullName.Length > 50)
				{
					return VCC_OrgFullName.Substring(0, OrgHeader.Schema.OH_FullNameTruncatedLength);
				}

				return VCC_OrgFullName;
			}
		}

		#endregion

		#region RelatedPortCode

		public virtual RefUNLOCO RelatedPortCode
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, VCC_RelatedPortCode); }
		}

		#endregion

		#region Properties

		public ZString CurrentActionsAsText
		{
			get
			{
				if (IsEditing && IsDeactivating)
				{
					return EditActionText + " & " + DeactivateActionText;
				}
				else if (IsEditing)
				{
					return EditActionText;
				}
				else if (IsDeactivating)
				{
					return DeactivateActionText;
				}
				else
				{
					return "";
				}
			}
		}

		#region View Denied Message
		internal ZString ViewDeniedMessage
		{
			get { return Res.GetString("48B80C5A-7740-4A73-BB02-65EC7EE78E9F", "** View Denied due to Security Access **"); }
		}
		#endregion

		#region Protected Properties

		bool ViewAllowed
		{
			get
			{
				if (VCC_TableCode == "GS")
				{ return Env.Security.StaffViewOtherStaffDetails.IsAllowed; }
				if (VCC_TableCode == "HA")
				{ return Env.Security.HRJobApplicantView.IsAllowed; }
				return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		bool ViewNotAllowed
		{
			get { return !ViewAllowed; }
		}

		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public override ZString VCC_Address1
		{
			get { return ViewAllowed ? base.VCC_Address1 : ViewDeniedMessage; }
			set { base.VCC_Address1 = value; }
		}

		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public override ZString VCC_Address2
		{
			get { return ViewAllowed ? base.VCC_Address2 : ViewDeniedMessage; }
			set { base.VCC_Address2 = value; }
		}

		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public override ZString VCC_City
		{
			get { return ViewAllowed ? base.VCC_City : ViewDeniedMessage; }
			set { base.VCC_City = value; }
		}

		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public override ZString VCC_Phone
		{
			get { return ViewAllowed ? base.VCC_Phone : ViewDeniedMessage; }
			set { base.VCC_Phone = value; }
		}

		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public override ZString VCC_PostCode
		{
			get { return ViewAllowed ? base.VCC_PostCode : ViewDeniedMessage; }
			set { base.VCC_PostCode = value; }
		}

		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public override ZString VCC_State
		{
			get { return ViewAllowed ? base.VCC_State : ViewDeniedMessage; }
			set { base.VCC_State = value; }
		}

		#endregion

		#region Non-Delivery Report

		[BusinessObjectTestExclude()]
		public ZBool IsNDR
		{
			get
			{
				return EmailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			}
			set
			{
				EmailAddress.GI_DeliveryStatus = value ? (ZString)EmailDeliveryReportStatus.Codes.NonDeliveryReport : ZString.Empty;
				IsEditing = true;
				IsNDRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsNDRInfo
		{
			get { return GetZPropertyInfo(nameof(IsNDR)); }
		}

		protected bool IsNDR_ReadOnly
		{
			get { return IsNDR == ZBool.False; }
		}

		LazyGlbEmailAddress EmailAddress
		{
			get { return emailAddress ?? (emailAddress = new LazyGlbEmailAddress(Factory, () => VCC_Email)); }
		}
		LazyGlbEmailAddress emailAddress;

		#endregion

		#region Edit Action

		public ZBool IsEditing
		{
			get { return isEditing; }
			set
			{
				SetNonPersistentPropertyValue(IsEditingInfo, ref isEditing, value);
			}
		}
		ZBool isEditing;

		public ZPropertyInfo IsEditingInfo
		{
			get { return GetZPropertyInfo(nameof(IsEditing)); }
		}

		#endregion

		#region De-active Action

		public ZBool IsDeactivating
		{
			get { return isDeactivating; }
			set
			{
				SetNonPersistentPropertyValue(IsDeactivatingInfo, ref isDeactivating, value);
			}
		}
		ZBool isDeactivating;

		public ZPropertyInfo IsDeactivatingInfo
		{
			get { return GetZPropertyInfo(nameof(IsDeactivating)); }
		}

		#endregion

		public ZString ContactUrl
		{
			get
			{
				if (!contactUrl.HasValue)
				{
					IShowEditFormUrlCreator creator = ObjectFactory.Get<IShowEditFormUrlCreator>();
					string url = this.VCC_TableCode == OrgContactSchema.Constants.Prefix ? creator.Create(ControllerIDs.OrgContacts, this.PK.ToGuid()) : creator.Create(ControllerIDs.SalesEnquiry, this.PK.ToGuid());
					url += "&TextToShow=" + this.VCC_ContactName;

					contactUrl = url;
				}

				return contactUrl.Value;
			}
		}
		ZString? contactUrl;

		[EmailAddress]
		public override ZString VCC_Email
		{
			get
			{
				return base.VCC_Email;
			}
			set
			{
				if (VCC_Email != value)
				{
					IsEditing = true;
					if (IsNDR)
					{
						IsNDR = false;
					}
				}
				base.VCC_Email = value;

				VCC_EmailInfo.RefreshBinding();
			}
		}

		public ScheduleData ScheduleData
		{
			get { return scheduleDetails; }
			set { scheduleDetails = value; }
		}
		ScheduleData scheduleDetails;

		public ZString RelatedPortCodeForScheduling
		{
			get
			{
				if (!relatedPortCodeForScheduling.IsEmpty)
				{
					return relatedPortCodeForScheduling;
				}
				return VCC_RelatedPortCode;
			}
			set { relatedPortCodeForScheduling = value; }
		}
		ZString relatedPortCodeForScheduling;

		#endregion

		#region Related Properties

		public ZString PhoneFallbackToOrganisation
		{
			get { return VCC_Phone.IsEmpty && Header != null ? Header.MainAddress.OA_Phone : VCC_Phone; }
		}

		public ZPropertyInfo PhoneFallbackToOrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(PhoneFallbackToOrganisation)); }
		}

		public ZString EmailFallbackToOrganisation
		{
			get { return VCC_Email.IsEmpty && Header != null ? Header.MainAddress.OA_Email : VCC_Email; }
		}

		public ZPropertyInfo EmailFallbackToOrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(EmailFallbackToOrganisation)); }
		}

		#region RecipientTableCode

		public ZString RecipientTableCode
		{
			get
			{
				ZString result = VCC_TableCode;
				CampaignContactTypeCodeList list = new CampaignContactTypeCodeList();
				if (!result.IsEmpty)
				{
					result = list.GetDescriptionFromCode(result);
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CampaignContactFetchStrategy(this);
		}

		#endregion

		#region IGlbCompanyCampaignItemRecipient

		public string Fax
		{
			get { return VCC_Fax; }
		}

		public OrgHeader Organisation
		{
			get { return Factory.Load<OrgHeader>(VCC_OH); }
		}

		public string Phone
		{
			get { return VCC_Phone; }
		}

		public string Salutation
		{
			get { return VCC_Salutation; }
		}

		public string Title
		{
			get { return VCC_Title; }
		}

		public IContactable[] GetNestedContacts(string parentContactDescription)
		{
			return Array.Empty<IContactable>();
		}

		public string Mobile
		{
			get { return VCC_Mobile; }
		}

		public string Email
		{
			get { return VCC_Email; }
		}

		public string Name
		{
			get { return VCC_ContactName; }
		}

		public string RelatedDocName
		{
			get { return $"{ResString.GetMultilingualString("F4152FE5-662E-4C5A-9C30-D6AF327E3804", "Contact")} {Header.OH_FullName} ({Header.OH_Code})"; }
		}

		#region IContactable Members

		public bool IsActive
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region IScheduleCampaignItems

		public ZString ScheduleStatus
		{
			get { return TrackingStatusCodes.Codes.QUE; }
		}

		public ZString TableCode
		{
			get { return ViewCampaignContactSchema.Constants.Prefix; }
		}

		#endregion

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new CampaignContactTypeDecider();

		class CampaignContactTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(CampaignContact);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return typeof(CampaignContact);
			}

			public override Type GetTypeForNew()
			{
				return typeof(CampaignContact);
			}
		}

		#endregion
	}
}
