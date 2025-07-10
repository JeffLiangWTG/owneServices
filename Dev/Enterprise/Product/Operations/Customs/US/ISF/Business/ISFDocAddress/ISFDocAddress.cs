using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using static Enterprise.Integration.Customs.US.ISF;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	[CodeProperty(JobDocAddress.Schema.E2_CompanyName), DescriptionProperty(ISFDocAddress.Schema.E2_ShortAddress)]
	public class ISFDocAddress : JobDocAddress, IWebISFDocAddress, IUSISFDocAddress
	{
		#region Schema

		public new class Schema : JobDocAddress.Schema
		{
			public const string E2_SocialSecurityNumberDetails = "E2_SocialSecurityNumberDetails";
			public const string E2_SocialSecurityNumberDateOfBirth = "E2_SocialSecurityNumberDateOfBirth";
			public const string E2_SocialSecurityNumber = "E2_SocialSecurityNumber";
			public const string E2_SocialSecurityNumberOrGovRegNum = "E2_SocialSecurityNumberOrGovRegNum";
			public const string E2_ShortAddress = "E2_ShortAddress";

			public const int E2_SocialSecurityNumberMaxLength = 11;
		}

		#endregion

		public ISFDocAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Registration Type/Number for Document

		public ZString RegTypeAndNumForDocument => E2_GovRegNumType == OrgCusCode.USACodeTypes.SocialSecurityNumber ? ZString.Empty : ZString.Format("{0}: {1}", E2_GovRegNumType, E2_GovRegNum);

		#endregion

		#region SocialSecurityNumber Data

		public ZBool IsSocialSecurityNumberGovRegNumType
		{
			get { return E2_GovRegNumType == OrgCusCode.USACodeTypes.SocialSecurityNumber; }
		}

		#region E2_SocialSecurityNumberDetails

		public ZString E2_SocialSecurityNumberDetails
		{
			get
			{
				var ssnForDisplay = E2_SocialSecurityNumber;
				if (IsSocialSecurityNumberGovRegNumType && !OrgDetailsViewPersonalInformationIsAllow)
				{
					ssnForDisplay = SocialSecurityNumberValidator.SSNWithMask;
				}
				return Res.GetString("A1B78B9C-136B-41E1-8C90-937DB16589FE", "{0} DOB:{1}", ssnForDisplay.IsEmpty ? "NOT SPECIFIED" : ssnForDisplay.ToString(), E2_SocialSecurityNumberDateOfBirth.ToString(SocialSecurityNumberDateOfBirthFormat).ToUpper());
			}
		}

		public ZPropertyInfo E2_SocialSecurityNumberDetailsInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_SocialSecurityNumberDetails);
				GetInfoWithUpdatedHumanReadableName(info);
				return info;
			}
		}

		#endregion

		#region ShortAddress

		public ZString E2_ShortAddress
		{
			get
			{
				var orgAddress = this.Address;
				return orgAddress != null && !orgAddress.OA_Code.IsEmpty ? orgAddress.OA_Code : E2_Address1;
			}
		}

		#endregion

		#region E2_SocialSecurityNumberOrGovRegNum

		[BusinessObjectTestExclude]
		public ZString E2_SocialSecurityNumberOrGovRegNum
		{
			get { return IsSocialSecurityNumberGovRegNumType ? E2_SocialSecurityNumberDetails : E2_GovRegNum; }
		}

		public ZPropertyInfo E2_SocialSecurityNumberOrGovRegNumInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_SocialSecurityNumberOrGovRegNum);
				GetInfoWithUpdatedHumanReadableName(info);
				return info;
			}
		}

		#endregion

		#region E2_SocialSecurityNumber

		// NNN-NN-NNNN
		[ReadOnlyMember(nameof(SocialSecurityNumberDataNotOverridable))]
		[BusinessObjectTestExclude]
		[MaxLength(Schema.E2_SocialSecurityNumberMaxLength)]
		public ZString E2_SocialSecurityNumber
		{
			get { return (ZString)GetSocialSecurityNumberData(SocialSecurityNumberDataFieldType.SocialSecurityNumber); }
			set
			{
				ZString oldValue = E2_SocialSecurityNumber;
				if (IsSocialSecurityNumberGovRegNumType)
				{
					value = value.TrimEnd(' ');
					CheckMaximumLength(E2_SocialSecurityNumberInfo, value);
					SetSocialSecurityNumberData(value, E2_SocialSecurityNumberDateOfBirth);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_SocialSecurityNumber();
				}
				E2_SocialSecurityNumberInfo.RefreshBinding(oldValue);
				E2_SocialSecurityNumberDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo E2_SocialSecurityNumberInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_SocialSecurityNumber);
				GetInfoWithUpdatedHumanReadableName(info);
				return info;
			}
		}

		#endregion

		#region SocialSecurityNumberForDisplay

		[ReadOnlyMember(nameof(SocialSecurityNumberForDisplay_ReadOnly))]
		public ZString SocialSecurityNumberForDisplay
		{
			get
			{
				if (IsSocialSecurityNumberGovRegNumType && !OrgDetailsViewPersonalInformationIsAllow)
				{
					return SocialSecurityNumberValidator.SSNWithMask;
				}

				return E2_SocialSecurityNumber;
			}
			set => E2_SocialSecurityNumber = value;
		}

		bool SocialSecurityNumberForDisplay_ReadOnly => !IsSocialSecurityNumberDataOverridable || !OrgDetailsViewPersonalInformationIsAllow;

		public ZWrappedPropertyInfo SocialSecurityNumberForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SocialSecurityNumberForDisplay), x => E2_SocialSecurityNumberInfo); }
		}
		#endregion

		#region E2_SocialSecurityNumberDateOfBirth

		[ReadOnlyMember(nameof(SocialSecurityNumberDataNotOverridable))]
		[BusinessObjectTestExclude]
		public ZDateTime E2_SocialSecurityNumberDateOfBirth
		{
			get { return (ZDateTime)GetSocialSecurityNumberData(SocialSecurityNumberDataFieldType.DateOfBirth); }
			set
			{
				ZDateTime oldValue = E2_SocialSecurityNumberDateOfBirth;
				if (IsSocialSecurityNumberGovRegNumType)
				{
					SetSocialSecurityNumberData(E2_SocialSecurityNumber, value);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_SocialSecurityNumberDateOfBirth();
				}
				E2_SocialSecurityNumberDateOfBirthInfo.RefreshBinding(oldValue);
				E2_SocialSecurityNumberDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo E2_SocialSecurityNumberDateOfBirthInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_SocialSecurityNumberDateOfBirth);
				GetInfoWithUpdatedHumanReadableName(info);
				return info;
			}
		}

		#endregion

		bool SocialSecurityNumberDataNotOverridable
		{
			get { return !IsSocialSecurityNumberDataOverridable; }
		}

		public bool IsSocialSecurityNumberDataOverridable
		{
			get { return IsSocialSecurityNumberGovRegNumType && E2_AddressOverride; }
		}

		enum SocialSecurityNumberDataFieldType { SocialSecurityNumber, DateOfBirth }

		IZType GetSocialSecurityNumberData(SocialSecurityNumberDataFieldType fieldType)
		{
			switch (fieldType)
			{
				case SocialSecurityNumberDataFieldType.SocialSecurityNumber:
					return IsSocialSecurityNumberGovRegNumType ? E2_GovRegNum.SubstringSafe(0, Schema.E2_SocialSecurityNumberMaxLength).TrimEnd() : ZString.Empty;
				default:
					ZDateTime result = ZDateTime.Empty;
					if (IsSocialSecurityNumberGovRegNumType)
					{
						ZDateTime.TryParseExact(E2_GovRegNum.SubstringSafe(Schema.E2_SocialSecurityNumberMaxLength, SocialSecurityNumberDateOfBirthFormat.Length), out result, SocialSecurityNumberDateOfBirthFormat);
					}
					return result;
			}
		}

		bool OrgDetailsViewPersonalInformationIsAllow => Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;

		bool isSocialSecurityNumberDataUpdateInProgress;
		void SetSocialSecurityNumberData(ZString socialSecurityNumberID, ZDateTime dateOfBirth)
		{
			if (!isSocialSecurityNumberDataUpdateInProgress)
			{
				try
				{
					isSocialSecurityNumberDataUpdateInProgress = true;
					E2_GovRegNum = GetSocialSecurityNumberDataFormat(socialSecurityNumberID, dateOfBirth);
				}
				finally
				{
					isSocialSecurityNumberDataUpdateInProgress = false;
				}
			}
		}
		const string SocialSecurityNumberDateOfBirthFormat = "ddMMMyyyy";

		internal static string GetSocialSecurityNumberDataFormat(ZString socialSecurityNumberID, ZDateTime dateOfBirth)
		{
			return socialSecurityNumberID.PadRight(Schema.E2_SocialSecurityNumberMaxLength).Left(Schema.E2_SocialSecurityNumberMaxLength) +
						dateOfBirth.ToString(SocialSecurityNumberDateOfBirthFormat);
		}

		#endregion

		#region Override

		public override ZString E2_GovRegNumType
		{
			get { return base.E2_GovRegNumType; }
			set
			{
				var oldValue = E2_GovRegNumType;
				base.E2_GovRegNumType = value;
				if (oldValue != E2_GovRegNumType)
				{
					E2_GovRegNum = ZString.Empty;
				}
			}
		}

		#endregion

		public ISFDocAddressRequirement ISFRequirement
		{
			get { return Requirement as ISFDocAddressRequirement; }
		}

		public new ISFDocAddressValidation Validation
		{
			get { return (ISFDocAddressValidation)base.Validation; }
		}

		protected override JobDocAddressValidation GetNewValidation()
		{
			return new ISFDocAddressValidation(this);
		}

		public ISFDocAddressDependentCollection ManufacturerAddresses()
		{
			var header = this.Parent as CusISFHeader;
			return header != null ? header.DocAddresses : null;
		}

		#region Lookups

		public new ISFDocAddressLookup Lookups
		{
			get { return (ISFDocAddressLookup)base.Lookups; }
		}

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new ISFDocAddressLookup(this);
		}

		#endregion
	}
}
