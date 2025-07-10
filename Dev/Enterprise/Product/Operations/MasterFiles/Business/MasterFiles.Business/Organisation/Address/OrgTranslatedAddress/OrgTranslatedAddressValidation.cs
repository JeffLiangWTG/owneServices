//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTranslatedAddressValidation
//
//    This class should be used for overriding validation in AutoOrgTranslatedAddressValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTranslatedAddressValidation : AutoOrgTranslatedAddressValidation
	{
		public OrgTranslatedAddressValidation(AutoOrgTranslatedAddress parent)
			: base(parent)
		{
		}

		public new OrgTranslatedAddress Parent
		{
			get { return (OrgTranslatedAddress)base.Parent; }
		}

		readonly AddressValidation addressValidation = new AddressValidation();

		#region ValidateAll

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			if (Parent.HasErrors)
			{
				Parent.AddRowError(Res.GetString("9CC4E909-4503-4134-A498-2BC08437916D", "Please access the faulty translation via the Translated Address drop down menu to rectify the error(s)."));
			}
		}

		#endregion

		#region OTA_Language

		protected override void CheckOTA_Language()
		{
			base.CheckOTA_Language();
			var info = Parent.OTA_LanguageInfo;
			CheckNoDuplicateLocalAddress();
			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);

				if (!Parent.IsEnglishOnlyOrEmpty && Parent.IsEnglish)
				{
					info.AddError(Res.GetString("F0EDA501-D52F-4D55-8D26-B6F433070E64", "Non-English Characters detected in this address. Please select the proper language for this address."));
				}
			}
		}

		void CheckNoDuplicateLocalAddress()
		{
			Parent.ParentAddress.CheckNoDuplicateLocalAddress(Parent, Parent.OTA_LanguageInfo);
		}

		#endregion

		#region OTA_CompanyName

		protected override void CheckOTA_CompanyName()
		{
			base.CheckOTA_CompanyName();
			CheckEnglishCharactersForEnglishAddresses(Parent.OTA_CompanyNameInfo);
		}

		#endregion

		#region OA_Address1

		protected override void CheckOTA_Address1()
		{
			base.CheckOTA_Address1();
			var info = Parent.OTA_Address1Info;
			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
			}

			CheckEnglishCharactersForEnglishAddresses(info);
		}

		#endregion

		#region OA_Address2

		protected override void CheckOTA_Address2()
		{
			base.CheckOTA_Address2();
			var info = Parent.OTA_Address2Info;
			var header = Parent.ParentAddress.Header;
			if (header != null && header.RequiredFieldsForOrg.RequireAddress2)
			{
				MandatoryValidation.CheckEntered(info);
			}

			CheckEnglishCharactersForEnglishAddresses(info);
		}

		#endregion

		#region OA_City

		protected override void CheckOTA_City()
		{
			base.CheckOTA_City();
			var info = Parent.OTA_CityInfo;
			CheckEnglishCharactersForEnglishAddresses(info);
		}

		#endregion

		#region OA_State

		protected override void CheckOTA_State()
		{
			base.CheckOTA_State();
			var info = Parent.OTA_StateInfo;
			if (!info.HasErrors())
			{
				if (Parent.ParentAddress != null)
				{
					var country = Parent.ParentAddress.Country;

					if (country != null && !OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country.PK.ToGuid(), Parent.ValidationSection))
					{
						addressValidation.CheckState(info, country, Parent.ValidationSection, Parent.ValidationStatus);
					}
				}
			}

			CheckEnglishCharactersForEnglishAddresses(info);
		}

		#endregion

		#region OTA_ValidationStatus

		protected override void CheckOTA_ValidationStatus()
		{
			base.CheckOTA_ValidationStatus();

			if (Parent.ParentAddress != null && Parent.ParentAddress.OA_IsActive)
			{
				var country = Parent.ParentAddress.Country;

				if (country != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country.PK.ToGuid(), Parent.ValidationSection))
				{
					if (Parent.OTA_ValidationStatus == AddressValidationStatus.Invalid)
					{
						Parent.OTA_ValidationStatusInfo.AddError(Res.GetString("5A05EE1F-F24E-454A-BEAB-58CE43521A95", "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address."));
					}
				}
			}
		}

		#endregion

		void CheckEnglishCharactersForEnglishAddresses(ZPropertyInfo info)
		{
			if (Parent.IsEnglish)
			{
				EnglishStrictCharactersValidation.ErrorIfNotEnglish(info);
			}
		}
	}
}
