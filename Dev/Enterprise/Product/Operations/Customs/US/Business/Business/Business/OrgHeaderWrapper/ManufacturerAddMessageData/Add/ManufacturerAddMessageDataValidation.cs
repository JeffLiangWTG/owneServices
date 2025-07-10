using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ManufacturerAddMessageDataValidation : AutoManufacturerAddMessageDataValidation
	{
		public ManufacturerAddMessageDataValidation(AutoManufacturerAddMessageData parent)
			: base(parent) { }

		#region Implementation

		public new ManufacturerAddMessageData Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ManufacturerAddMessageData)base.Parent; }
		}

		#endregion

		protected override void CheckUS_Country()
		{
			base.CheckUS_Country();

			if (Parent.US_Country.IsEmpty)
			{
				Parent.US_CountryInfo.AddMessageError(CountryCodeMandatory);
			}
			else
			{
				if (Parent.US_Country == Core.Constants.CountryCodes.Canada)
				{
					Parent.US_CountryInfo.AddMessageError(InvalidCACountryCode);
				}
				CheckAlphanumeric(Parent.US_CountryInfo, Parent.US_Country);
			}

			ValidateUS_Zip();
		}

		internal const string CountryCodeMandatory = "A country code is mandatory. Please enter a valid UNLOCO in 'Details' tab.";
		internal const string InvalidCACountryCode = "For Canada, you should enter a valid Canadian Province/Territory code. Please check 'State' field in 'Details'. It should be a valid two-letter code.";

		protected override void CheckUS_City()
		{
			base.CheckUS_City();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CityInfo, "city");

			if (!Parent.US_City.IsEmpty)
			{
				CheckAlphanumeric(Parent.US_CityInfo, Parent.US_City);
			}
		}

		protected override void CheckUS_FirmName()
		{
			base.CheckUS_FirmName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FirmNameInfo, "firm name");

			if (!Parent.US_FirmName.IsEmpty)
			{
				CheckAlphanumeric(Parent.US_FirmNameInfo, Parent.US_FirmName);
			}
		}

		protected override void CheckUS_Zip()
		{
			base.CheckUS_Zip();
			var parent = Parent;

			var zipValidation = new ZipCodeValidation();
			zipValidation.ValidatePostalCodeForChinaManufacturer(parent.US_ZipInfo, parent.US_Country, parent.US_Zip);

			if (parent.US_Zip.IsEmpty)
			{
				if (parent.IsUS)
				{
					parent.US_ZipInfo.AddMessageError(ZipIsMandatoryForUSManufacturer);
				}
			}
			else
			{
				var zipCodeFormatMessage = ZString.Empty;

				if (parent.IsCA)
				{
					ZString postCodeToValidate = parent.US_Zip.SubstringSafe(3, 1) == " " ? parent.US_Zip : parent.US_Zip.InsertSafe(3, " ");
					zipCodeFormatMessage = zipValidation.ValidateCAZipCodeAndGetMessage(postCodeToValidate);
				}
				else if (parent.IsMX)
				{
					zipCodeFormatMessage = zipValidation.ValidateMXZipCodeAndGetMessage(parent.US_Zip);
				}

				if (zipCodeFormatMessage != "")
				{
					parent.US_ZipInfo.AddMessageError(zipCodeFormatMessage);
				}

				CheckAlphanumeric(parent.US_ZipInfo, parent.US_Zip);
			}
		}

		internal const string ZipIsMandatoryForUSManufacturer = "A zip code is required when adding an US/PR manufacturer.";

		protected override void CheckUS_MID()
		{
			base.CheckUS_MID();
			if (!Parent.US_MID.IsEmpty)
			{
				OrgCusCodeValidation.ValidateManufacturerID(
					(ZPropertyInfoString)Parent.US_MIDInfo,
					Parent.Factory,
					Parent,
					checkDuplicates: false);

				CheckAlphanumeric(Parent.US_MIDInfo, Parent.US_MID);
			}
		}

		protected override void CheckUS_Street()
		{
			base.CheckUS_Street();
			
			if (!Parent.US_Street.IsEmpty)
			{
				CheckAlphanumeric(Parent.US_StreetInfo, Parent.US_Street);
			}
		}

		void CheckAlphanumeric(ZPropertyInfo info, ZString str)
		{
			if (!regex.IsMatch(str))
			{
				info.AddMessageError(info.HumanReadableName + AlphanumericMessage);
			}
		}

		readonly Regex regex = new Regex(@"^[A-Z0-9 ]*$");
		internal const string AlphanumericMessage = " should be alphanumeric characters, and space.";
	}
}
