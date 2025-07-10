using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EnquiryOrgFinder : AutoEnquiryOrgFinder
	{
		public EnquiryOrgFinder(BusinessObjectFactory factory, OrgHeaderForEnquiryMatching orgForMatching, bool contactHasEmail, bool orgHasAddress1)
			: base(factory)
		{
			this.orgForMatching = orgForMatching;
			this.contactHasEmail = contactHasEmail;
			this.orgHasAddress1 = orgHasAddress1;
		}

		readonly OrgHeaderForEnquiryMatching orgForMatching;
		readonly bool contactHasEmail;
		readonly bool orgHasAddress1;

		public OrgHeaderForEnquiryMatching OrgForMatching
		{
			get { return orgForMatching; }
		}

		#region Default Value

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ShouldLinkToExistingClientIntelligence = false;
			ShouldLinkOrganizationAddressToInquiry = true;
			ShouldAddInquiryAddressToOrganization = false;

			ShouldCreateNewClientIntelligence = false;
			ShouldAllowWebAccess = false;
			ShouldNotAllowWebAccess = true;
		}

		#endregion

		#region LinkToExistingClientIntelligence

		#region ShouldLinkToExistingClientIntelligence

		public override ZBool ShouldLinkToExistingClientIntelligence
		{
			get { return base.ShouldLinkToExistingClientIntelligence; }
			set
			{
				base.ShouldLinkToExistingClientIntelligence = value;
				if (ShouldLinkToExistingClientIntelligence)
				{
					ShouldCreateNewClientIntelligence = false;
				}
			}
		}

		#endregion

		#region Single Org Pk

		[List("Lookups.Organisations")]
		public override ZGuid SingleOrgPk
		{
			get { return base.SingleOrgPk; }
			set
			{
				if (SingleOrgPk != value)
				{
					RefreshSingleOrgAddresses(value);
					base.SingleOrgPk = value;
				}
			}
		}

		#endregion

		#region Selected Address Pk

		public override ZGuid SelectedAddressPk
		{
			get { return base.SelectedAddressPk; }
			set
			{
				base.SelectedAddressPk = value;
				if (!IsValidationSuspended)
				{
					ValidateShouldLinkOrganizationAddressToInquiry();
					ValidateShouldAddInquiryAddressToOrganization();
				}
			}
		}

		public virtual OrgAddress SelectedAddress
		{
			get { return Factory.Load<OrgAddress>(SelectedAddressPk); }
		}

		#endregion

		#region SingleOrgAddresses

		public OrgAddressCollection SingleOrgAddresses
		{
			get
			{
				if (singleOrgAddresses == null)
				{
					singleOrgAddresses = new OrgAddressCollection(Factory);
					singleOrgAddresses.SetReadOnlyIncludingChildren(true);
				}
				return singleOrgAddresses;
			}
		}
		OrgAddressCollection singleOrgAddresses;

		void RefreshSingleOrgAddresses(ZGuid singleOrgPk)
		{
			if (singleOrgPk.IsValid)
			{
				SingleOrgAddresses.Load(new ZQuery(OrgAddressSchema.OA_OH, singleOrgPk));
			}
			else
			{
				SingleOrgAddresses.RemoveAll();
			}
		}

		#endregion

		#region SimilarOrgMatches

		public virtual OrgPatternMatchCollection SimilarOrgMatches
		{
			get
			{
				if (similarOrgMatches == null)
				{
					OrgForMatching.SimilarOrgFinder.FindSimilarOrganisations(false);
					similarOrgMatches = OrgForMatching.SimilarOrgMatches;
					similarOrgMatches.Sort(OrgPatternMatch.Schema.OS_Rank);
				}
				return similarOrgMatches;
			}
		}
		protected OrgPatternMatchCollection similarOrgMatches;

		public virtual void RefreshSimilarOrgMatches()
		{
			OrgForMatching.SimilarOrgFinder.FindSimilarOrganisations(false);
		}

		#endregion

		#endregion

		#region CreateNewClientIntelligence

		#region ShouldCreateNewClientIntelligence

		public override ZBool ShouldCreateNewClientIntelligence
		{
			get { return base.ShouldCreateNewClientIntelligence; }
			set
			{
				base.ShouldCreateNewClientIntelligence = value;
				if (ShouldCreateNewClientIntelligence)
				{
					ShouldLinkToExistingClientIntelligence = false;
				}
			}
		}

		#endregion

		#endregion

		#region HasChanges

		public override bool HasChanges
		{
			get { return false; }
			set { base.HasChanges = value; }
		}

		#endregion

		#region Lookups

		public OrgFinderLookups Lookups
		{
			get { return lookups ?? (lookups = new OrgFinderLookups(this)); }
		}
		OrgFinderLookups lookups;

		#endregion

		#region Validation

		public ZString NotificationMessagesAsText
		{
			get
			{
				return new ZNotificationCollector(this, false, false, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString();
			}
		}

		public override void ValidateSingleOrgPk()
		{
			base.ValidateSingleOrgPk();

			if (ShouldLinkToExistingClientIntelligence)
			{
				if (!SingleOrgPk.IsEmpty && SingleOrgPk.IsValid && !IsSingleOrgValid)
				{
					SingleOrgPkInfo.AddError(Res.GetString("44e76dbd-23a6-496f-a82e-b1e9a0076620", "The entered organization has either been deleted or not yet added to the database."));
				}
			}
		}

		public override void ValidateShouldLinkOrganizationAddressToInquiry()
		{
			base.ValidateShouldLinkOrganizationAddressToInquiry();

			if (ShouldLinkToExistingClientIntelligence && ShouldLinkOrganizationAddressToInquiry)
			{
				if (!IsSelectedAddressValid)
				{
					ShouldLinkOrganizationAddressToInquiryInfo.AddError(Res.GetString("55b50d76-5a4b-4591-ac4f-1249dc1e91f4", "Please select a valid organization address to link to inquiry."));
				}
			}
		}

		public override void ValidateShouldAddInquiryAddressToOrganization()
		{
			base.ValidateShouldAddInquiryAddressToOrganization();

			if (ShouldLinkToExistingClientIntelligence && ShouldAddInquiryAddressToOrganization)
			{
				if (!orgHasAddress1)
				{
					ShouldAddInquiryAddressToOrganizationInfo.AddError(Res.GetString("e005aed8-efd5-4a5c-9b3f-222565afbc46", "Cannot add inquiry address to an organization because Address is empty for inquiry."));
				}
				else if ((!SingleOrgPk.IsEmpty && !IsSingleOrgValid) || (SingleOrgPk.IsEmpty && !IsSelectedAddressValid))
				{
					ShouldAddInquiryAddressToOrganizationInfo.AddError(Res.GetString("2A5175F5-34CE-4CC1-AAD5-0243937A2C8D", "Please enter a valid organization or select a similarly matched organization to add inquiry address."));
				}
			}
		}

		public override void ValidateShouldAllowWebAccess()
		{
			base.ValidateShouldAllowWebAccess();

			if (ShouldCreateNewClientIntelligence)
			{
				if (ShouldAllowWebAccess && !contactHasEmail)
				{
					ShouldAllowWebAccessInfo.AddError(Res.GetString("ade0ffbb-578f-4d4c-bd02-295885b45122", "Cannot approve web access because contact email is empty."));
				}
			}
		}

		public bool IsSingleOrgValid
		{
			get
			{
				if (!SingleOrgPk.IsValid)
				{
					return false;
				}

				var org = Factory.Load<OrgHeader>(SingleOrgPk);
				return org != null && org.IsInDatabase && !org.IsDeleted;
			}
		}

		bool IsSelectedAddressValid
		{
			get
			{
				if (!SelectedAddressPk.IsValid)
				{
					return false;
				}

				var address = SelectedAddress;
				return address != null && address.IsInDatabase && !address.IsDeleted;
			}
		}

		#endregion
	}
}
