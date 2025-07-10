using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CrewMemberValidation : CusInBondPersonValidation
	{
		public CrewMemberValidation(CrewMember parent)
			: base(parent)
		{
		}

		new CrewMember Parent
		{
			get { return (CrewMember)base.Parent; }
		}

		#region CheckCP_DateOfBirth

		protected override void CheckCP_DateOfBirth()
		{
			base.CheckCP_DateOfBirth();
			if (!Parent.IsRegistered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CP_DateOfBirthInfo);
			}
		}

		#endregion

		#region CheckCP_FullName

		protected override void CheckCP_FullName()
		{
			base.CheckCP_FullName();
			if (!Parent.IsRegistered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CP_FullNameInfo);
			}

			if (!Parent.CP_FullNameInfo.HasNotifications() && Parent.CP_FullName.Split(' ').Length < 2)
			{
				Parent.CP_FullNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("First or Last name of the crew member/passenger"));
			}
		}

		#endregion

		#region CheckCP_Gender

		protected override void CheckCP_Gender()
		{
			base.CheckCP_Gender();
			if (Parent.IsRegistered)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CP_GenderInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CP_GenderInfo);
			}
		}

		#endregion

		#region CheckCP_RN_NKNationality

		protected override void CheckCP_RN_NKNationality()
		{
			base.CheckCP_RN_NKNationality();
			if (Parent.IsRegistered)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CP_RN_NKNationalityInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CP_RN_NKNationalityInfo);
			}
		}

		#endregion

		#region CheckCP_Type

		protected override void CheckCP_Type()
		{
			base.CheckCP_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CP_TypeInfo);
			if (!Parent.CP_TypeInfo.HasNotifications() && Parent.CP_Type == CrewTypes.Codes.ResponsibleParty)
			{
				var trip = Parent.Trip;
				if (trip != null && trip.CrewMembers.Any(crew => crew.CP_Type == Parent.CP_Type && crew.PK != Parent.PK))
				{
					Parent.CP_TypeInfo.AddMessageError("Only one responsible party may be reported per trip.");
				}
			}

			if (Parent.ValidateAllHasBeenRun)
			{
				ValidateAtLeastOneDocumentEntered(Parent.CP_TypeInfo);
				ValidateIfThereIsCrewMemberHoldingDriversLicense(Parent.CP_TypeInfo);
				ValidateIfThereIsDriverHoldingHazmatEndorsementIfRequired(Parent.CP_TypeInfo);
			}
		}

		void ValidateAtLeastOneDocumentEntered(ZPropertyInfo notificationInfo)
		{
			if (!notificationInfo.HasNotifications() && !(Parent.Certificates.Count > 0))
			{
				notificationInfo.AddMessageError("At least one travel document should be entered on the Travel Documents tab.");
			}
		}

		void ValidateIfThereIsCrewMemberHoldingDriversLicense(ZPropertyInfo notificationInfo)
		{
			if (!notificationInfo.HasNotifications() && Parent.CP_Type == CrewTypes.Codes.ResponsibleParty)
			{
				var trip = Parent.Trip;
				if (trip != null && trip.CrewMembers.All(c => c.DriversLicense.IsEmpty && !c.IsRegistered))
				{
					notificationInfo.AddMessageError("At least one crew member should have a driver license; please specify details on the Travel Documents tab.");
				}
			}
		}

		void ValidateIfThereIsDriverHoldingHazmatEndorsementIfRequired(ZPropertyInfo notificationInfo)
		{
			if (!notificationInfo.HasNotifications() && Parent.CP_Type == CrewTypes.Codes.ResponsibleParty)
			{
				var trip = Parent.Trip;
				if (trip != null && trip.HasHazmatShipments
					&& !trip.CrewMembers.Any(c => c.IsRegistered || (!c.DriversLicense.IsEmpty && !c.HazmatEndorsement.IsEmpty && c.HazmatEndorsement != "NO")))
				{
					notificationInfo.AddMessageError("At least one driver must have hasmat endorsement to transport hazmat shipments; please specify details on the Travel Documents tab.");
				}
			}
		}

		#endregion

		#region CheckUSAddress

		internal void ValidateUSAddressOrganisationPK(JobDocAddressValidation validation)
		{
			if (Parent.CP_Type == CrewTypes.Codes.CrewMember || Parent.CP_Type == CrewTypes.Codes.ResponsibleParty)
			{
				var address = validation.Parent;
				if (!address.OrganisationPKInfo.HasNotifications() && (!address.IsValidAddress || address.E2_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates))
				{
					address.OrganisationPKInfo.AddMessageError("Please specify a valid US Address for this crew member.");
				}
				OrgValidation.ValidateAddress(address);
			}
		}

		internal void ValidateUSAddressCountry(JobDocAddressValidation validation)
		{
			var address = validation.Parent;
			if (!address.E2_RN_NKCountryCode.IsEmpty && address.E2_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates)
			{
				address.E2_RN_NKCountryCodeInfo.AddError("Invalid country code. United States must be entered for this address.");
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			Parent.ValidateAllHasBeenRun = true;
			base.ValidateAll();
		}

		#endregion
	}
}
