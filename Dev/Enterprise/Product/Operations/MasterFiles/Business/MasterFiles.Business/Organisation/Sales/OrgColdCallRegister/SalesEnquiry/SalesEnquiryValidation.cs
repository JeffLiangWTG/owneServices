using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryValidation : OrgColdCallRegisterValidation
	{
		public SalesEnquiryValidation(SalesEnquiry parent)
			: base(parent)
		{
		}

		SalesEnquiry ParentEnquiry
		{
			get { return (SalesEnquiry)Parent; }
		}

		#region O1_LeadSource

		protected override void CheckO1_LeadSource()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_LeadSource();
				ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ParentEnquiry.O1_LeadSourceInfo, null, ParentEnquiry.Lookups.Source_List, ParentEnquiry.Lookups.Source_ActiveList);
			}
		}

		#endregion

		#region Org Name

		protected override void CheckO1_CompanyName()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_CompanyName();
				MandatoryValidation.CheckEntered(ParentEnquiry.O1_CompanyNameInfo);
			}
		}

		#endregion

		#region O1_OA_LinkedAddress

		protected override void CheckO1_OA_LinkedAddress()
		{
			base.CheckO1_OA_LinkedAddress();
			if (ParentEnquiry.OrgPk.IsValid && !ParentEnquiry.ReadOnly)
			{
				MandatoryValidation.CheckEntered(ParentEnquiry.O1_OA_LinkedAddressInfo);
			}
		}

		#endregion

		#region Contact Name

		protected override void CheckO1_ContactName()
		{
			base.CheckO1_ContactName();
			if (!ParentEnquiry.ReadOnly)
			{
				MandatoryValidation.CheckEntered(ParentEnquiry.O1_ContactNameInfo);
				var org = ParentEnquiry.Header;
				if (org != null)
				{
					if (!ParentEnquiry.O1_OC_LinkedContact.IsEmpty)
					{
						OrgContact contact = (OrgContact)org.Contacts.FindByPK(ParentEnquiry.O1_OC_LinkedContact);
						if (contact == null || !contact.OC_IsActive)
						{
							var message = Res.GetString("6dd6dedb-aebf-4c93-8c3f-e15a871ef5be", "Select an active contact belonging to the organization.");
							if (ParentEnquiry.IsInDatabase && !ParentEnquiry.O1_OC_LinkedContactInfo.HasChanges)
							{
								ParentEnquiry.O1_ContactNameInfo.AddWarning(message);
							}
							else
							{
								ParentEnquiry.O1_ContactNameInfo.AddError(message);
							}
						}
					}
					else
					{
						CheckSecurityForContact(org, ParentEnquiry.O1_ContactName, ParentEnquiry.O1_ContactNameInfo);
					}
				}
			}
		}

		#endregion

		#region Phone / Mobile

		void CheckPhoneNumber(ZPropertyInfo numberToCheck, bool allowWorkExtension)
		{
			var validChars = allowWorkExtension ? ValidPhoneNumberWithWorkExtensionChars : ValidPhoneNumberWithoutWorkExtensionChars;
			var number = new ZString(numberToCheck.Value.ToString());
			if (number.ExcludeChars(validChars).Length > 0)
			{
				numberToCheck.AddError(allowWorkExtension ? NumbersWithWorkExtensionErrorMessage : NumbersWithoutWorkExtensionErrorMessage);
			}
		}

		protected override void CheckO1_Phone()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_Phone();
				CheckPhoneNumber(ParentEnquiry.O1_PhoneInfo, true);
			}
		}

		protected override void CheckO1_Mobile()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_Mobile();
				CheckPhoneNumber(ParentEnquiry.O1_MobileInfo, false);
			}
		}

		protected override void CheckO1_Fax()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_Fax();
				CheckPhoneNumber(ParentEnquiry.O1_FaxInfo, false);
			}
		}

		#endregion

		#region O1_JobCategory

		protected override void CheckO1_JobCategory()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_JobCategory();

				ListValidation.WarnIfInvalidCode(ParentEnquiry.O1_JobCategoryInfo, ParentEnquiry.Lookups.JobCategory_List, (IMultilingualString)ResString.GetMultilingualString("24ea4ff6-338c-4cf0-acfc-6c2f42a6b337", "Chosen Job Category is not defined in the Registry. Please define it or use the an Operation Action to change all Enquiries with this category to valid one."));
			}
		}

		#endregion

		#region O1_EnquiryType

		protected override void CheckO1_EnquiryType()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_EnquiryType();
				MandatoryValidation.CheckEntered(ParentEnquiry.O1_EnquiryTypeInfo);
				ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ParentEnquiry.O1_EnquiryTypeInfo, null, ParentEnquiry.Lookups.AllEnquiryTypes, ParentEnquiry.Lookups.ActiveEnquiryTypes);
			}
		}

		#endregion

		#region O1_CloseReason

		protected override void CheckO1_CloseReason()
		{
			if (ParentEnquiry.ReadOnly)
			{
				base.CheckO1_CloseReason();

				if (ParentEnquiry.O1_LeadStatus != SalesEnquiryStatusCodeList.Codes.Converted)
				{
					if (OrganisationsDataRegistry.Instance.SalesEnquiryFieldsMandatory.Value.GetBoolFromCode(OrgColdCallRegisterSchema.Constants.O1_CloseReason))
					{
						MandatoryValidation.CheckEntered(ParentEnquiry.O1_CloseReasonInfo);
					}

					ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ParentEnquiry.O1_CloseReasonInfo, null, ParentEnquiry.Lookups.EnquiryCloseReasonList, ParentEnquiry.Lookups.CloseReason_ActiveList);
				}
			}
		}

		#endregion

		#region O1_GS_NKRepAssigned

		protected override void CheckO1_GS_NKRepAssigned()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.O1_GS_NKRepAssignedInfo);
			}
		}

		#endregion

		#region O1_Email

		protected override void CheckO1_Email()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				if (!Parent.O1_Email.IsEmpty && !(new Regex(@"^.+@.+\..+$").Match(Parent.O1_Email).Success))
				{
					Parent.O1_EmailInfo.AddError(Res.GetString("d1edada5-f8b3-4d7e-b16c-8876289c442c", "An invalid Email address has been entered"));
				}
			}
		}

		#endregion

		#region O1_InterestLevel/Lead Interest

		protected override void CheckO1_InterestLevel()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_InterestLevel();
				ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ParentEnquiry.O1_InterestLevelInfo, null, ParentEnquiry.Lookups.LeadInterest_List, ParentEnquiry.Lookups.LeadInterest_ActiveList);
			}
		}

		#endregion

		#region O1_PortOrCountry

		protected override void CheckO1_PortOrCountry()
		{
			base.CheckO1_PortOrCountry();

			if (!ParentEnquiry.O1_PortOrCountry.IsEmpty)
			{
				var location = LocationHelper.GetLocationFromString(ParentEnquiry.O1_PortOrCountry, ParentEnquiry.Factory);
				if (location != null && !location.IsActive)
				{
					ListValidation.ErrorIfCancelled(ParentEnquiry.O1_PortOrCountryInfo, true);
				}
			}
		}

		#endregion

		#region Referral Organisations and Contacts

		protected override void CheckO1_OH_SourceOfLead()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_OH_SourceOfLead();
				ListValidation.ErrorIfInvalidPK(ParentEnquiry.O1_OH_SourceOfLeadInfo);
			}
		}

		protected override void CheckO1_OH_ReferTo()
		{
			if (!ParentEnquiry.ReadOnly)
			{
				base.CheckO1_OH_ReferTo();
				ListValidation.ErrorIfInvalidPK(ParentEnquiry.O1_OH_ReferToInfo);
			}
		}

		public void ValidateReferringContactName()
		{
			ValidateCalculatedProperty(ParentEnquiry.ReferringContactNameInfo);
		}

		public void ValidateReferToContactName()
		{
			ValidateCalculatedProperty(ParentEnquiry.ReferToContactNameInfo);
		}

		protected void CheckReferringContactName()
		{
			CheckReferralContactName(
				ParentEnquiry.ReferringOrg,
				ParentEnquiry.ReferringContactName,
				ParentEnquiry.ReferringContactNameInfo,
				ParentEnquiry.O1_OC_ReferringContact,
				ParentEnquiry.O1_OC_ReferringContactInfo,
				Res.GetString("5BF4F3E6-B368-47D8-B414-C7260D6F3949", "Select the Referring Organization before selecting the Referring Contact."),
				Res.GetString("74820446-9978-4C5B-B948-43958A22831D", "Select an active contact belonging to the Referring Organization."));
		}

		protected void CheckReferToContactName()
		{
			CheckReferralContactName(
				ParentEnquiry.ReferTo,
				ParentEnquiry.ReferToContactName,
				ParentEnquiry.ReferToContactNameInfo,
				ParentEnquiry.O1_OC_ReferToContact,
				ParentEnquiry.O1_OC_ReferToContactInfo,
				Res.GetString("81CB545C-846A-4964-AD41-8B6D52A04E5C", "Select the Refer To Organization before selecting the Refer To Contact."),
				Res.GetString("D7348B16-FEFE-466B-A1A8-2C0B111A3D89", "Select an active contact belonging to the Refer To Organization."));
		}

		void CheckReferralContactName(OrgHeader org, ZString contactName, ZPropertyInfo contactNameProp, ZGuid contactId, ZPropertyInfo contactIdProp, string selectReferralOrgMsg, string selectReferralContactMsg)
		{
			if (!ParentEnquiry.ReadOnly)
			{
				if (org == null)
				{
					if (!contactName.IsEmpty || !contactId.IsEmpty)
					{
						contactNameProp.AddError(selectReferralOrgMsg);
					}
				}
				else
				{
					if (!contactId.IsEmpty)
					{
						OrgContact contact = (OrgContact)org.Contacts.FindByPK(contactId);
						if (contact == null || !contact.OC_IsActive)
						{
							if (ParentEnquiry.IsInDatabase && !contactIdProp.HasChanges)
							{
								contactNameProp.AddWarning(selectReferralContactMsg);
							}
							else
							{
								contactNameProp.AddError(selectReferralContactMsg);
							}
						}
					}
					else
					{
						CheckSecurityForContact(org, contactName, contactNameProp);
					}
				}
			}
		}

		void CheckSecurityForContact(OrgHeader org, ZString contactName, ZPropertyInfo contactNameProp)
		{
			if (!contactName.IsEmpty)
			{
				var missingSecurity = ParentEnquiry.MissingSecurityForLinkToOrganizationByLinkingToAddress(org.PK, contactName);
				if (missingSecurity != null)
				{
					var message = ResString.GetMultilingualString("11cc1296-4372-400b-a0ad-789572c80437", @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
{0}", missingSecurity.DisplayTextPathToSecurityRight);
					contactNameProp.AddError(message);
				}
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info == Parent.O1_OC_ReferringContactInfo || info == Parent.O1_OC_ReferToContactInfo || info == Parent.O1_OC_LinkedContactInfo)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReferringContactName();
			ValidateReferToContactName();
		}

		public const string ValidPhoneNumberWithWorkExtensionChars = "0123456789+() -<>";
		public const string ValidPhoneNumberWithoutWorkExtensionChars = "0123456789+() -";

		public string NumbersWithWorkExtensionErrorMessage
		{
			get { return Res.GetString("3c118f55-1a17-4d2e-a1cc-b0c3a3d70efc", "Number can only contain number characters, +, -, (, ), <, > and spaces."); }
		}

		public string NumbersWithoutWorkExtensionErrorMessage
		{
			get { return Res.GetString("f0705021-e91f-487b-a4d4-489b5e8aec45", "Number can only contain number characters, +, -, (, ) and spaces."); }
		}

		/// <summary>
		/// Error If Invalid Code But Just Warn If Existing Inactive Code
		/// </summary>
		/// <param name="linkedInfo">The ZPropertyInfo of a property that's linked to parameter 'info', e.g. Contact.JobCategory can be linked to Enquiry.JobCategory</param>
		void ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ZPropertyInfo info, ZPropertyInfo linkedInfo, ICodeDescriptionPairList allCodes, ICodeDescriptionPairList activeCodes)
		{
			if (!info.BizObj.IsInDatabase || (linkedInfo != null ? linkedInfo.HasChanges : info.HasChanges) || !allCodes.ContainsCode(info.Value))
			{
				ListValidation.ErrorIfInvalidCode(info, activeCodes);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, activeCodes, ListValidation.InactiveCodeMessage);
			}
		}
	}
}
