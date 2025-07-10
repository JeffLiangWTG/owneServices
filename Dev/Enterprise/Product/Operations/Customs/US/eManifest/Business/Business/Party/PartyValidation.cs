using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class PartyValidation : JobDocAddressValidation
	{
		public PartyValidation(Party parent)
			: base(parent)
		{
		}

		new Party Parent
		{
			get { return (Party)base.Parent; }
		}

		#region CheckE2_AddressType

		protected override void CheckE2_AddressType()
		{
			var description = ResString.GetMultilingualString("d9a387a4-5cba-4185-94f7-253b8c8b6248", "Party Type");
			ListValidation.ErrorIfInvalidCode(Parent.E2_AddressTypeInfo, Parent.Factory.GetCachedValue<PartyTypes>(), description);
			MandatoryValidation.CheckEntered(Parent.E2_AddressTypeInfo, description.ToString());
		}

		#endregion

		#region CheckE2_GovRegNumType

		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();
			if (Parent.E2_AddressOverride)
			{
				ListValidation.ErrorIfInvalidCode(Parent.E2_GovRegNumTypeInfo);
			}
		}

		#endregion

		#region CheckE2_RN_NKCountryCode

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			if (Parent.E2_AddressOverride && !Parent.E2_RN_NKCountryCodeInfo.HasNotifications())
			{
				MandatoryValidation.CheckEntered(Parent.E2_RN_NKCountryCodeInfo);
			}
		}

		#endregion

		#region CheckOrganisationPK

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (!Parent.IsValidAddress)
			{
				if (IsMandatory && !Parent.OrganisationPKInfo.HasNotifications())
				{
					Parent.OrganisationPKInfo.AddMessageError(Res.GetString("1e3cf395-1c36-4b74-a5b0-8811bc29bd4b", "Please specify a valid {0} for this job.", Parent.AddressCaption));
				}
			}
			else
			{
				OrgValidation.ValidateOrganization(Parent);
			}
		}

		bool IsMandatory
		{
			get
			{
				var shipment = Parent.Shipment;
				return !shipment.IsSplit && shipment.B0_ReleaseStatus != ShipmentEntryStatusList.Codes.LodgedWithOtherTrip
					   || (Parent.E2_AddressType != PartyTypes.Codes.Consignee && Parent.E2_AddressType != PartyTypes.Codes.Shipper);
			}
		}

		#endregion

		#region CheckE2_CompanyName

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (!Parent.E2_CompanyName.IsEmpty && !Parent.OrganisationPKInfo.HasNotifications())
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_CompanyNameInfo);
			}
		}

		#endregion

		#region CheckE2_Address1

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			if (!Parent.E2_Address1.IsEmpty && !Parent.OrganisationPKInfo.HasNotifications())
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_Address1Info);
			}
		}

		#endregion

		#region CheckE2_Address2

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			if (!Parent.E2_Address2.IsEmpty && !Parent.OrganisationPKInfo.HasNotifications())
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_Address2Info);
			}
		}

		#endregion

		#region CheckE2_City

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			if (!Parent.E2_City.IsEmpty && !Parent.OrganisationPKInfo.HasNotifications())
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_CityInfo);
			}
		}

		#endregion

		#region CheckE2_Postcode

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_PostcodeInfo);
			if (!Parent.OrganisationPKInfo.HasNotifications())
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_PostcodeInfo);
			}
		}

		#endregion

		#region CheckE2_State

		protected override void CheckE2_State()
		{
			base.CheckE2_State();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_StateInfo);
			if (!Parent.OrganisationPKInfo.HasNotifications())
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_StateInfo);
			}
		}

		#endregion

		#region CheckE2_ValidationStatus

		protected override void CheckE2_ValidationStatus()
		{
			if (!Parent.E2_SuppressAddressValidationError)
			{
				base.CheckE2_ValidationStatus();
			}
		}

		#endregion
	}
}
