using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressValidation : ACEOceanManifestJobDocAddressValidation
	{
		public ISFDocAddressValidation(ISFDocAddress parent)
			: base(parent)
		{
		}

		public new ISFDocAddress Parent
		{
			get { return (ISFDocAddress)base.Parent; }
		}

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();
			ValidateE2_SocialSecurityNumberDetails();
			ValidateE2_Contact();
		}

		void AMSValidation(ZString dataValue, ZPropertyInfo propertyInfo)
		{
			if (!dataValue.IsEmpty)
			{
				AMSCharactersValidator.ValidateCharacters(propertyInfo);
			}
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (Parent.E2_CompanyName.Length > ISFSF30.EntityNameMaxLength)
			{
				Parent.E2_CompanyNameInfo.AddWarning(string.Format("Company Name is too long. Only the first {0} characters will be sent.", ISFSF30.EntityNameMaxLength));
			}

			AMSValidation(Parent.E2_CompanyName, Parent.E2_CompanyNameInfo);
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			if (Parent.E2_Contact.Length > ISFSF30.EntityNameMaxLength)
			{
				Parent.E2_ContactInfo.AddWarning(string.Format("Contact is too long. Only the first {0} characters will be sent.", ISFSF30.EntityNameMaxLength));
			}
			AMSValidation(Parent.E2_Contact, Parent.E2_ContactInfo);
		}

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			AMSValidation(Parent.E2_Address1, Parent.E2_Address1Info);
		}

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			AMSValidation(Parent.E2_Address2, Parent.E2_Address2Info);
		}

		protected override void CheckE2_AddressSequence()
		{
			base.CheckE2_AddressSequence();
			var addresses = Parent.ManufacturerAddresses();
			if (addresses != null)
			{
				var currentAddressTypeList = addresses.FindDocAddressesByType(Parent.DocAddressType);
				if (currentAddressTypeList.Length <= byte.MaxValue)
				{
					foreach (ISFDocAddress address in addresses)
					{
						if (Parent.E2_AddressType == address.E2_AddressType &&
							Parent.E2_AddressSequence == address.E2_AddressSequence &&
							Parent.PK != address.PK)
						{
							Parent.E2_AddressSequenceInfo.AddError(string.Format("Already exist duplicate AddressSequence with value: {0}", Parent.E2_AddressSequence.ToString()));
						}
					}
				}
			}
		}

		protected override bool ShouldSuppressError()
		{
			var status = ((CusISFHeader)Parent.Parent)?.BF_CustomsStatus;
			if (string.IsNullOrEmpty(status) || status.Equals(MessageStatusList.Codes.ErrorISFAdd) || status.Equals(MessageStatusList.Codes.NotSentISF))
			{
				return base.ShouldSuppressError();
			}
			return true;
		}

		#region E2_SocialSecurityNumberDetails

		public void ValidateE2_SocialSecurityNumberDetails()
		{
			ValidateCalculatedProperty(Parent.E2_SocialSecurityNumberDetailsInfo);
		}

		protected void CheckE2_SocialSecurityNumberDetails()
		{
			ValidateE2_SocialSecurityNumber();
			ValidateE2_SocialSecurityNumberDateOfBirth();
			if (Parent.IsSocialSecurityNumberGovRegNumType)
			{
				Parent.E2_SocialSecurityNumberDetailsInfo.AddAllNotificationsFrom(Parent.E2_SocialSecurityNumberInfo);
				Parent.E2_SocialSecurityNumberDetailsInfo.AddAllNotificationsFrom(Parent.E2_SocialSecurityNumberDateOfBirthInfo);
			}
		}

		#endregion

		#region E2_SocialSecurityNumber

		public void ValidateE2_SocialSecurityNumber()
		{
			ValidateCalculatedProperty(Parent.E2_SocialSecurityNumberInfo);
		}

		protected void CheckE2_SocialSecurityNumber()
		{
			if (Parent.IsSocialSecurityNumberDataOverridable)
			{
				var requirement = Parent.ISFRequirement;
				if (requirement != null && requirement.ValidateSocialSecurityNumber != null)
				{
					requirement.ValidateSocialSecurityNumber(this);
				}
				ValidateE2_SocialSecurityNumberDetails();
			}
		}

		#endregion

		#region E2_SocialSecurityNumberDateOfBirth

		public void ValidateE2_SocialSecurityNumberDateOfBirth()
		{
			ValidateCalculatedProperty(Parent.E2_SocialSecurityNumberDateOfBirthInfo);
		}

		protected void CheckE2_SocialSecurityNumberDateOfBirth()
		{
			if (Parent.IsSocialSecurityNumberDataOverridable)
			{
				var requirement = Parent.ISFRequirement;
				if (requirement != null && requirement.ValidateSocialSecurityNumberDateOfBirth != null)
				{
					requirement.ValidateSocialSecurityNumberDateOfBirth(this);
				}
				ValidateE2_SocialSecurityNumberDetails();
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateE2_SocialSecurityNumberDateOfBirth();
			ValidateE2_SocialSecurityNumber();
			ValidateE2_SocialSecurityNumberDetails();
		}

		#endregion
	}
}
