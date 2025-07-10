using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public enum GlobalBusinessIdentifierMessageType
	{
		None,
		Original,
		Update,
		Delete
	}

	public class GlobalBusinessIdentifierData : AutoGlobalBusinessIdentifierData, IMessageAttachee
	{
		public GlobalBusinessIdentifierData(OrgHeaderWrapper wrapper)
			: base(wrapper.Factory)
		{
			this.Wrapper = wrapper;
			SetDefaultValues(wrapper);
		}

		public GlobalBusinessIdentifierData(OrgHeaderWrapper wrapper, bool importFromMessage)
			: this(wrapper)
		{
			if (importFromMessage)
			{
				var lastOutgoingMessage = (MQEDIMessage)wrapper.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete);
				if (lastOutgoingMessage != null)
				{
					var messageImporter = new GlobalBusinessIdentifierDataMessageImporter(lastOutgoingMessage, this);
					messageImporter.Populate();
				}
			}
		}

		public new class Schema : AutoGlobalBusinessIdentifierData.Schema
		{
			public const string SubmissionStatus = "SubmissionStatus";
			public const int SubmissionStatusMaxLength = 3;
			public const string GBIStatus = "GBIStatus";
			public const int GBIStatusMaxLength = 3;
		}

		public OrgHeaderWrapper Wrapper { get; }

		public OrgHeader Organization => Wrapper.organisation;

		public OrgAddress AddressDetails => Factory.Load<OrgAddress>(US_OA_AddressDetails);

		[List(nameof(Lookups) + "." + nameof(GlobalBusinessIdentifierDataLookups.Addresses))]
		public override ZGuid US_OA_AddressDetails
		{
			get { return base.US_OA_AddressDetails; }
			set
			{
				ZGuid oldValue = US_OA_AddressDetails;
				base.US_OA_AddressDetails = value;
				if (!IsCopying && oldValue != US_OA_AddressDetails)
				{
					UpdateAddressDetails();
				}
			}
		}

		[ReadOnlyMember(nameof(US_DUNS_ReadOnly))]
		public override ZString US_DUNS
		{
			get => base.US_DUNS;
			set => base.US_DUNS = value;
		}

		ZBool US_DUNS_ReadOnly => !Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails).IsEmpty;

		[ReadOnlyMember(nameof(US_GLN_ReadOnly))]
		public override ZString US_GLN
		{
			get => base.US_GLN;
			set => base.US_GLN = value;
		}

		ZBool US_GLN_ReadOnly => !Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.GlobalLocationNumber, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails).IsEmpty;

		[ReadOnlyMember(nameof(US_LEI_ReadOnly))]
		public override ZString US_LEI
		{
			get => base.US_LEI;
			set => base.US_LEI = value;
		}

		ZBool US_LEI_ReadOnly => !Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.LegalEntityIdentifier, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails).IsEmpty;

		[List(nameof(Lookups) + "." + nameof(GlobalBusinessIdentifierDataLookups.SubmissionStatusList))]
		[MaxLength(Schema.SubmissionStatusMaxLength)]
		public ZString SubmissionStatus
		{
			get
			{
				var result = ZString.Empty;
				if (AddressDetails is OrgAddress address)
				{
					result = address.GetSystemDefinedValue<ZString>(Schema.SubmissionStatus);
				}

				return result;
			}
			set
			{
				if (AddressDetails is OrgAddress address)
				{
					var oldValue = SubmissionStatus;
					CheckMaximumLength(SubmissionStatusInfo, value);
					address.SetSystemDefinedValue(Schema.SubmissionStatus, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSubmissionStatus();
					}
					SubmissionStatusInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SubmissionStatusInfo => GetZPropertyInfo(Schema.SubmissionStatus);

		[MaxLength(Schema.GBIStatusMaxLength)]
		public ZString GBIStatus
		{
			get
			{
				var result = ZString.Empty;
				if (AddressDetails is OrgAddress address)
				{
					result = address.GetSystemDefinedValue<ZString>(Schema.GBIStatus);
				}

				return result;
			}
			set
			{
				if (AddressDetails is OrgAddress address)
				{
					var oldValue = GBIStatus;
					CheckMaximumLength(GBIStatusInfo, value);
					address.SetSystemDefinedValue(Schema.GBIStatus, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateGBIStatus();
					}
					GBIStatusInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo GBIStatusInfo => GetZPropertyInfo(Schema.GBIStatus);

		public ZString ManufacturerID => Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails);

		public ZString AuthorisedEconomicOperator => Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails);

		void SetDefaultValues(OrgHeaderWrapper wrapper)
		{
			US_IsShipper = Organization.OH_IsConsignor;
			US_WebsiteURL = Organization.MainWebURL?.PU_URL.Left(Schema.US_WebsiteURLMaxLength) ?? ZString.Empty;
			US_OA_AddressDetails = wrapper.MainAddress.PK;
		}

		void UpdateAddressDetails()
		{
			IAddressDetails addressDetails = AddressDetails;

			if (addressDetails != null)
			{
				US_FirmName = addressDetails.CompanyName.Left(Schema.US_FirmNameMaxLength);
				US_Address1 = addressDetails.AddressLine1.Left(Schema.US_Address1MaxLength);
				US_Address2 = addressDetails.AddressLine2.Left(Schema.US_Address2MaxLength);
				US_City = addressDetails.City.Left(Schema.US_CityMaxLength);
				US_PostCode = addressDetails.PostCode.Left(Schema.US_PostCodeMaxLength);
				US_State = addressDetails.State.Left(Schema.US_StateMaxLength);
				US_Phone = addressDetails.Phone.GetLocalPhoneNumber(Organization.CountryCode).Left(Schema.US_PhoneMaxLength);
				US_Country = addressDetails.Country.Left(Schema.US_CountryMaxLength);
			}
			else
			{
				US_FirmName = ZString.Empty;
				US_Address1 = ZString.Empty;
				US_Address2 = ZString.Empty;
				US_City = ZString.Empty;
				US_PostCode = ZString.Empty;
				US_State = ZString.Empty;
				US_Phone = ZString.Empty;
				US_Country = ZString.Empty;
			}

			US_IsManufacturer = !ManufacturerID.IsEmpty;
			US_DUNS = Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails);
			US_GLN = Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.GlobalLocationNumber, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails);
			US_LEI = Organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.LegalEntityIdentifier, Core.Constants.CountryCodes.UnitedStates, US_OA_AddressDetails);
		}

		public void SaveGlobalBusinessIdentifiers()
		{
			if (AddressDetails is OrgAddress address)
			{
				void MarkOrgCusCodeUnVerified(ZString registrationNum, ZString codeType)
				{
					if (!registrationNum.IsEmpty)
					{
						var orgCusCode = Organization.CustomsCodes.GetOrgCusCode(codeType, Core.Constants.CountryCodes.UnitedStates, address.PK) ?? address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(codeType, registrationNum, Core.Constants.CountryCodes.UnitedStates);

						if (orgCusCode != null)
						{
							orgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(false, ZString.Empty, ZString.Empty);
						}
					}
				}

				MarkOrgCusCodeUnVerified(US_DUNS, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
				MarkOrgCusCodeUnVerified(US_GLN, OrgCusCode.USACodeTypes.GlobalLocationNumber);
				MarkOrgCusCodeUnVerified(US_LEI, OrgCusCode.USACodeTypes.LegalEntityIdentifier);
			}
		}

		public GlobalBusinessIdentifierDataLookups Lookups
		{
			get { return lookups ?? (lookups = new GlobalBusinessIdentifierDataLookups(this)); }
		}
		GlobalBusinessIdentifierDataLookups lookups;

		#region IMessageAttachee

		ZString IMessageAttachee.MessageStatus
		{
			get => SubmissionStatus;
			set => SubmissionStatus = value;
		}

		CBPEDIMessageCollection IMessageAttachee.Messages => Wrapper.Messages;

		BusinessObjectFactory IMessageAttachee.Factory => Factory;

		GlbBranch IMessageAttachee.Branch => Organization.Branch;

		BusinessObject IMessageAttachee.TopLevelBusinessObject => Organization;

		string IMessageAttachee.TopLevelBizObjReferenceNumber => ZString.Empty;

		Logs IMessageAttachee.TopLevelBusinessObjectLogs => Organization.Logs;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Organisation;

		Guid IControllerIDProvider.BusinessObjectPK => Organization.PK.ToGuid();

		#endregion
	}
}
