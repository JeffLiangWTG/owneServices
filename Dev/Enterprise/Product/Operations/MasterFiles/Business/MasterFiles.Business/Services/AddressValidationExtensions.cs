using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.Business
{
	public static class AddressValidationExtensions
	{
		const string BackgroundValidation = "BAV";
		const string ValidationService = "AVS";
		const string ManuallyVerified = "MAN";

		public static string GetFullAddressString(this ISupportWebAddressValidation address)
		{
			var addressString = new StringBuilder();

			if (!string.IsNullOrEmpty(address.Address1))
			{
				addressString.Append(address.Address1 + " ");
			}

			if (!string.IsNullOrEmpty(address.Address2))
			{
				addressString.Append(address.Address2 + " ");
			}

			if (!string.IsNullOrEmpty(address.City))
			{
				addressString.Append(address.City + " ");
			}

			if (!string.IsNullOrEmpty(address.State))
			{
				addressString.Append(address.State + " ");
			}

			if (!string.IsNullOrEmpty(address.Postcode))
			{
				addressString.Append(address.Postcode + " ");
			}

			if (!string.IsNullOrEmpty(address.CountryCodeISO2))
			{
				addressString.Append(address.CountryCodeISO2);
			}

			return addressString.ToString();
		}

		public static void AddAddressValidationEventLog(this ISupportWebAddressValidation address, string originalStatus, IFactory factory)
		{
			if ((originalStatus != AddressValidationStatus.Verified &&
				originalStatus != AddressValidationStatus.ManuallyVerified &&
				(address.ValidationStatus == AddressValidationStatus.Verified ||
				 address.ValidationStatus == AddressValidationStatus.ManuallyVerified)) || address.IsValidatedByBackgroundService)
			{
				if (address.IsJobDocAddress)
				{
					var jobDocAddress = factory.Load<JobDocAddress>(address.EntityPK);
					if (jobDocAddress != null)
					{
						AddEventLog(jobDocAddress.Parent as BusinessObject, address.ValidationStatus, jobDocAddress.E2_AddressType, address.IsValidatedByBackgroundService);
					}
				}
				else
				{
					var orgAddress = factory.Load<OrgAddress>(address.EntityPK);
					if (orgAddress != null)
					{
						AddEventLog(orgAddress.Header, address.ValidationStatus, DocAddressTypes.OrgAddressTypeCode, address.IsValidatedByBackgroundService);
					}
				}
			}
		}

		static void AddEventLog(BusinessObject parent, string validationStatus, string addressCode, bool backgroundValidation)
		{
			if (parent != null)
			{
				string statusCode;
				string validationType;
				if (backgroundValidation)
				{
					validationType = BackgroundValidation;
				}
				else
				{
					validationType = validationStatus == AddressValidationStatus.ManuallyVerified ? ManuallyVerified : ValidationService;
				}

				if (validationStatus == AddressValidationStatus.Verified || validationStatus == AddressValidationStatus.ManuallyVerified)
				{
					statusCode = AddressValidationStatus.Verified;
				}
				else
				{
					statusCode = AddressValidationStatus.Invalid;
				}

				var logParameters = new Dictionary<string, string>
				{
					[Params.Codes.Status] = statusCode,
					[Params.Codes.Type] = validationType,
					[Params.Codes.Department] = addressCode
				};

				parent.GetLogs().AddNew(AutoEvents.AddressValidationStatus, logParameters.ToArray());
			}
		}
	}
}
