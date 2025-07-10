using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressRequirement : ICloneable
	{
		public delegate void ValidationDelegate(JobDocAddressValidation validation);
		public delegate RegistrationNumberResult RegistrationNumberResultDelegate(JobDocAddress docAddress);
		public delegate CodeDescriptionPairList LookupsDelegate(JobDocAddressLookups lookups);

		#region Constructor

		public JobDocAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType, ContactType defaultContactType, bool saveEvenIfBlank, int defaultMax)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
			this.DefaultContactType = defaultContactType;
			this.DefaultAddressType = defaultAddressType;
			this.SaveEvenIfBlank = saveEvenIfBlank;
			this.DefaultMax = defaultMax;
		}

		public JobDocAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType, ContactType defaultContactType, bool saveEvenIfBlank)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
			this.DefaultContactType = defaultContactType;
			this.DefaultAddressType = defaultAddressType;
			this.SaveEvenIfBlank = saveEvenIfBlank;
		}

		public JobDocAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType, ContactType defaultContactType)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
			this.DefaultContactType = defaultContactType;
			this.DefaultAddressType = defaultAddressType;
		}

		public JobDocAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
			this.DefaultContactType = defaultContactType;
		}

		public JobDocAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
			this.DefaultAddressType = defaultAddressType;
		}

		public JobDocAddressRequirement(DocAddressType defaultDocAddressType)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
		}

		public JobDocAddressRequirement()
		{
		}

		#endregion

		#region Properties

		public DocAddressType DefaultDocAddressType = DocAddressType.None;
		public ContactType DefaultContactType = ContactType.NoContactType;
		public AddressType DefaultAddressType = AddressType.OFC;
		public bool SaveEvenIfBlank { get; set; }
		public int DefaultMax = 1;
		public bool IsMandatory { get; set; }

		public RegistrationNumberResultDelegate GetRegistrationNumberResult;

		/// <summary>
		/// By default address can be overridden
		/// </summary>
		public bool CanOverride
		{
			get { return canOverride; }
			set { canOverride = value; }
		}
		bool canOverride = true;

		#endregion

		#region Additional Requirements

		public void AddLinkedRequirement(DocAddressType type)
		{
			AddLinkedRequirement(new JobDocAddressRequirement(type));
		}

		public void AddLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (requirement.DefaultDocAddressType != DefaultDocAddressType)
			{
				AdditionalRequirements[requirement.DefaultDocAddressType] = requirement;
			}
		}

		public DocAddressType[] SupportedDocAddressTypes
		{
			get
			{
				DocAddressType[] result = new DocAddressType[AdditionalRequirements.Count];
				AdditionalRequirements.Keys.CopyTo(result, 0);
				return result;
			}
		}

		internal Dictionary<DocAddressType, JobDocAddressRequirement> AdditionalRequirements = new Dictionary<DocAddressType, JobDocAddressRequirement>();

		#endregion

		#region Applicable DocAddressType List

		public CodeDescriptionPairList GetApplicableCodeList(JobDocAddressDependentCollection existingDocAddresses)
		{
			DocAddressTypes allKnownDocAddressCodes = existingDocAddresses.Factory.GetCachedValue<DocAddressTypes>();
			CodeDescriptionPairList allApplicableDocAddressCodes = new CodeDescriptionPairList();

			if (DefaultDocAddressType != DocAddressType.None)
			{
				if (!existingDocAddresses.ContainsDocAddressType(DefaultDocAddressType))
				{
					allApplicableDocAddressCodes.Add(DocAddressTypes.GetPair(existingDocAddresses.Factory, DefaultDocAddressType));
				}
				else
				{
					foreach (JobDocAddressRequirement requirement in AdditionalRequirements.Values)
					{
						if (DefaultDocAddressType != DocAddressType.None && !existingDocAddresses.ContainsDocAddressType(requirement.DefaultDocAddressType))
						{
							allApplicableDocAddressCodes.Add(DocAddressTypes.GetPair(existingDocAddresses.Factory, requirement.DefaultDocAddressType));
						}
					}
				}
			}

			return allApplicableDocAddressCodes;
		}

		#endregion

		#region Loolups Delegates

		public LookupsDelegate LookupsGovRegNumTypes;

		#endregion

		#region Validation Delegates

		public ValidationDelegate ValidateCompanyName;
		public ValidationDelegate ValidateAddress1;
		public ValidationDelegate ValidateAddress2;
		public ValidationDelegate ValidateCity;
		public ValidationDelegate ValidateState;
		public ValidationDelegate ValidatePostCode;
		public ValidationDelegate ValidateOrganisationPK;
		public ValidationDelegate ValidateGovRegNo;
		public ValidationDelegate ValidateGovRegNumType;
		public ValidationDelegate ValidateCountry;
		public ValidationDelegate ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address;
		public ValidationDelegate ValidateDelegateToBeFiredUponValidationOf_E2_AddressOverride;
		public ValidationDelegate ValidateOrganisationPKUponMandatoryRequirement;
		public ValidationDelegate ValidatePassportID;
		public ValidationDelegate ValidatePassportCountryOfIssue;
		public ValidationDelegate ValidatePassportDateOfBirth;
		public ValidationDelegate ValidateContact;
		public ValidationDelegate ValidatePhoneFormatted;
		public ValidationDelegate ValidateAddressType;
		public ValidationDelegate ValidateEmail;
		public ValidationDelegate ValidateMobileFormatted;

		#endregion

		#region ICloneable Members

		public JobDocAddressRequirement Clone()
		{
			JobDocAddressRequirement clone = (JobDocAddressRequirement)MemberwiseClone();
			clone.AdditionalRequirements = new Dictionary<DocAddressType, JobDocAddressRequirement>();

			foreach (KeyValuePair<DocAddressType, JobDocAddressRequirement> pair in AdditionalRequirements)
			{
				clone.AdditionalRequirements.Add(pair.Key, pair.Value.Clone());
			}

			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
