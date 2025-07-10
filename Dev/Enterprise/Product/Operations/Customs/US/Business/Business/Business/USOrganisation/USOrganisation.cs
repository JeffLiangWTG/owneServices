using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USOrganisation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public delegate bool IsSynchronizationEnableInvoker(ZBool isAddressOverride);

		public USOrganisation(ZPropertyInfo organisationPKInfo, ZPropertyInfo addressPKInfo, ZPropertyInfo contactInfo, ZPropertyInfo phoneInfo, ZPropertyInfo addressOverrideInfo, ContactType docGroup, IsSynchronizationEnableInvoker synchronizationInvoker, BusinessObjectFactory factory, USOrganisationDocAddress usOrganisationDocAddress = null)
			: this(organisationPKInfo, addressPKInfo, contactInfo, phoneInfo, addressOverrideInfo, docGroup.Code, synchronizationInvoker, factory, usOrganisationDocAddress)
		{
		}

		public USOrganisation(ZPropertyInfo organisationPKInfo, ZPropertyInfo addressPKInfo, ZPropertyInfo contactInfo, ZPropertyInfo phoneInfo, ZPropertyInfo addressOverrideInfo, ZString docGroup, IsSynchronizationEnableInvoker synchronizationInvoker, BusinessObjectFactory factory, USOrganisationDocAddress usOrganisationDocAddress = null)
			: base(factory)
		{
			Argument.NotNull(organisationPKInfo, nameof(organisationPKInfo));
			this.organisationPKInfo = organisationPKInfo;
			this.organisationPKInfo.ValueChanged += new EventHandler(organisationPKInfo_ValueChanged);

			Argument.NotNull(addressPKInfo, nameof(addressPKInfo));
			this.addressPKInfo = addressPKInfo;
			this.addressPKInfo.ValueChanged += new EventHandler(addressPKInfo_ValueChanged);

			Argument.NotNull(contactInfo, nameof(contactInfo));
			this.contactInfo = contactInfo;
			this.contactInfo.ValueChanged += new EventHandler(contactInfo_ValueChanged);

			Argument.NotNull(phoneInfo, nameof(phoneInfo));
			this.phoneInfo = phoneInfo;
			this.phoneInfo.ValueChanged += new EventHandler(phoneInfo_ValueChanged);

			Argument.NotNullOrEmpty(docGroup, nameof(docGroup));
			this.DocGroup = docGroup;

			Argument.NotNull(synchronizationInvoker, nameof(synchronizationInvoker));
			synchronizationEnableInvoker = synchronizationInvoker;

			Argument.NotNull(addressOverrideInfo, nameof(addressOverrideInfo));
			this.addressOverrideInfo = addressOverrideInfo;

			this.usOrganisationDocAddress = usOrganisationDocAddress;

			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers));
		}

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper => phoneNumberPropertyHelperThunk.Value;

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		ZString DefaultCountryCodeForPhoneNumbers => string.IsNullOrEmpty(Organisation?.CountryCode) ? ZString.Empty : Organisation.CountryCode;

		#region Schema

		public static class Schema
		{
			public const string TablePrefix = "ZO_";
			public const string ZO_OH_Organisation = "ZO_OH_Organisation";
			public const string ZO_OA_Address = "ZO_OA_Address";
			public const string ZO_Contact = "ZO_Contact";
			public const string ZO_Phone = "ZO_Phone";
			public const string ZO_Phone_Formatted = "ZO_Phone_Formatted";
		}

		#endregion

		#region Properties

		#region OganisationPK
		[RelatedBusinessObject("Organisation")]
		public virtual ZGuid ZO_OH_Organisation
		{
			get { return (ZGuid)organisationPKInfo.Value; }
			set { organisationPKInfo.Value = value; }
		}

		public virtual ZPropertyInfo ZO_OH_OrganisationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZO_OH_Organisation, x => organisationPKInfo); }
		}

		public OrgHeader Organisation
		{
			get { return Factory.Load<OrgHeader>(ZO_OH_Organisation); }
		}
		#endregion

		#region AddressPK
		[RelatedBusinessObject("Address")]
		public virtual ZGuid ZO_OA_Address
		{
			get { return (ZGuid)addressPKInfo.Value; }
			set { addressPKInfo.Value = value; }
		}

		public virtual ZPropertyInfo ZO_OA_AddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZO_OA_Address, x => addressPKInfo); }
		}

		public OrgAddress Address
		{
			get { return Factory.Load<OrgAddress>(ZO_OA_Address); }
		}
		#endregion

		#region Contact
		public virtual ZString ZO_Contact
		{
			get { return contactInfo.BizObj.IsDeleted ? ZString.Empty : (ZString)contactInfo.Value; }
			set { contactInfo.Value = value; }
		}

		public virtual ZPropertyInfo ZO_ContactInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZO_Contact, x => contactInfo); }
		}
		#endregion

		#region Phone
		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public virtual ZString ZO_Phone
		{
			get { return (ZString)phoneInfo.Value; }
			set { phoneInfo.Value = value; }
		}

		public virtual ZPropertyInfo ZO_PhoneInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZO_Phone, x => phoneInfo); }
		}
		#endregion

		#region Phone_Formatted

		[BusinessObjectTestExclude]
		public virtual ZString ZO_Phone_Formatted
		{
			get
			{
				return PhoneNumberPropertyHelper.GetPhoneNumber(ZO_PhoneInfo);
			}
		}

		public virtual ZPropertyInfo ZO_Phone_FormattedInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.ZO_Phone_Formatted, x => phoneInfo);
			}
		}
		#endregion

		#region FirstName
		public ZString FirstName
		{
			get
			{
				ZString[] list = ContactBreakDown;
				return (list.Length > 1) ? list[0] : ZString.Empty;
			}
		}
		#endregion

		#region MiddleName
		public ZString MiddleName
		{
			get
			{
				ZString result = ZString.Empty;
				ZString[] list = ContactBreakDown;
				if (list.Length > 2)
				{
					ZStringBuilder builder = new ZStringBuilder();
					for (int i = 1; i < list.Length - 1; i++)
					{
						builder.Append(list[i]);
					}
					result = builder.ToStringWithDelimiterBetweenAppends(" ");
				}
				return result;
			}
		}
		#endregion

		#region LastName
		public ZString LastName
		{
			get
			{
				ZString[] list = ContactBreakDown;
				return (list.Length >= 1) ? list[list.Length - 1] : ZString.Empty;
			}
		}
		#endregion

		public USOrganisationDocAddress USOrganisationDocAddress
		{
			get { return usOrganisationDocAddress; }
		}

		#endregion

		public RefUNLOCO OrgClosestPort
		{
			get
			{
				RefUNLOCO result = null;
				if (IsValid)
				{
					result = Address.EffectiveRelatedPortCode;
				}
				return result;
			}
		}

		public bool IsValid
		{
			get
			{
				return
					!IsDeleted &&
					!organisationPKInfo.BizObj.IsDeleted &&
					!addressPKInfo.BizObj.IsDeleted &&
					!contactInfo.BizObj.IsDeleted &&
					Organisation != null &&
					Address != null;
			}
		}

		public void CopyValueFrom(USOrganisation source)
		{
			((IBusinessObjectInternals)this).IsCopying = true;
			try
			{
				var isAddressOverride = new ZBool(!(source.USOrganisationDocAddress == null || source.USOrganisationDocAddress.IsDeleted || !source.USOrganisationDocAddress.E2_AddressOverride));
				addressOverrideInfo.Value = isAddressOverride;

				if (!isAddressOverride)
				{
					ZO_OH_Organisation = source.ZO_OH_Organisation;
					ZO_OA_Address = source.ZO_OA_Address;
					ZO_Contact = source.ZO_Contact;
					ZO_Phone = source.ZO_Phone;
				}
			}
			finally
			{
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		public delegate ZGuid OrganisationAddressPKDefaulter(OrgHeader org);
		public OrganisationAddressPKDefaulter GetDefaultAddressPK;

		public delegate OrgContact OrganisationContactDefaulter(OrgHeader org);
		public OrganisationContactDefaulter GetDefaultContact;

		#region Implementation

		internal void ReSynchronize(ZPropertyInfo orgPKInfo, ZPropertyInfo orgAddressPKInfo)
		{
			ZGuid orgPK = (ZGuid)orgPKInfo.Value;
			ZGuid orgAddressPK = (ZGuid)orgAddressPKInfo.Value;
			if (!orgAddressPK.IsEmpty)
			{
				addressPKInfo_ValueChanged(this, EventArgs.Empty);
				if (orgPKInfo.Value.Equals(orgPK))
				{
					SynchronizeContactData();
				}
			}
			else if (!orgPK.IsEmpty)
			{
				organisationPKInfo_ValueChanged(this, EventArgs.Empty);
			}
		}

		void phoneInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ShouldSynchronizeData(sender))
			{
				ZO_PhoneInfo.RefreshBinding();
			}
		}

		void contactInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ShouldSynchronizeData(sender))
			{
				ZO_ContactInfo.RefreshBinding();
			}
		}

		void addressPKInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ShouldSynchronizeData(sender))
			{
				if (!isSynchronizationInProgress)
				{
					SynchronizeOrgHeaderData();
				}
				ZO_OA_AddressInfo.RefreshBinding();
			}
		}

		void organisationPKInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ShouldSynchronizeData(sender))
			{
				if (!isSynchronizationInProgress)
				{
					SynchronizeOrgAddressData();
				}
				SynchronizeContactData();
				ZO_OH_OrganisationInfo.RefreshBinding();
			}
		}

		void SynchronizeContactData()
		{
			ZO_Contact = "";
			ZO_Phone = "";
			if (Organisation != null)
			{
				var contact = GetDefaultContact?.Invoke(Organisation) ?? new DefaultContactFinder(Organisation, false).DefaultContact(DocGroup);
				if (contact != null)
				{
					var address = Address;
					var relatedCountry = address?.RelatedCountry;
					var countryCode = relatedCountry?.Code ?? ZString.Empty;
					ZO_Contact = contact.OC_ContactName;
					ZO_Phone = PhoneNumberCalculator.GetUnformattedPhoneNumber(contact.PhoneFallbackToOrganisation, countryCode != Core.Constants.CountryCodes.UnitedStates);
				}
			}
		}

		void SynchronizeOrgAddressData()
		{
			try
			{
				isSynchronizationInProgress = true;
				ZO_OA_Address = (Organisation == null) ? ZGuid.Empty : (GetDefaultAddressPK == null ? Organisation.MainAddress.PK : GetDefaultAddressPK(Organisation));
			}
			finally
			{
				isSynchronizationInProgress = false;
			}
		}

		void SynchronizeOrgHeaderData()
		{
			try
			{
				isSynchronizationInProgress = true;
				ZO_OH_Organisation = (Address == null) ? ZGuid.Empty : Address.OA_OH;
			}
			finally
			{
				isSynchronizationInProgress = false;
			}
		}

		bool ShouldSynchronizeData(object caller)
		{
			bool result = synchronizationEnableInvoker((ZBool)addressOverrideInfo.Value);
			if (result && !IsCopying && !IsDeleted)
			{
				BusinessObject @object = caller as BusinessObject;
				result = @object != null && !@object.IsDeleted && !((IBusinessObjectInternals)@object).IsCopying;
			}
			return result;
		}
		bool isSynchronizationInProgress;

		ZString[] ContactBreakDown
		{
			get { return ZO_Contact.Split(' '); }
		}

		public readonly ZString DocGroup;
		protected readonly ZPropertyInfo organisationPKInfo;
		protected readonly ZPropertyInfo addressPKInfo;
		protected readonly ZPropertyInfo contactInfo;
		protected readonly ZPropertyInfo phoneInfo;
		protected readonly ZPropertyInfo addressOverrideInfo;
		protected IsSynchronizationEnableInvoker synchronizationEnableInvoker;
		protected USOrganisationDocAddress usOrganisationDocAddress;

		#endregion
	}
}
