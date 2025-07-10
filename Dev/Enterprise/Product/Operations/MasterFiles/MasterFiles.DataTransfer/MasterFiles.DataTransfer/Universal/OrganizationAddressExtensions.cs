using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public static class OrganizationAddressExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Meassage builder")]
		public static void AddAddressErrorMessage(this OrganizationAddress addressDataObject, ZString addresstype, ZStringBuilder result)
		{
			if (addressDataObject != null)
			{
				result.Append(Res.GetString("880a7eda-f252-4baf-9993-b9a40a445ccf", "Unable to match {0} Address, please make sure the supplied {0} Address is valid. Details were:", addresstype));
				result.AppendIfNotEmpty("Address1: ", addressDataObject.Address1);
				result.AppendIfNotEmpty("Address2: ", addressDataObject.Address2);
				result.AppendIfNotEmpty("AddressShortCode: ", addressDataObject.AddressShortCode);
				result.AppendIfNotEmpty("City: ", addressDataObject.City);
				result.AppendIfNotEmpty("CompanyName: ", addressDataObject.CompanyName);
				result.AppendIfNotEmpty("Country: ", addressDataObject.Country.ToStringContents());
				result.AppendIfNotEmpty("Email: ", addressDataObject.Email);
				result.AppendIfNotEmpty("Fax: ", addressDataObject.Fax);
				result.AppendIfNotEmpty("GovRegNum: ", addressDataObject.GovRegNum);
				result.AppendIfNotEmpty("GovRegNumType: ", addressDataObject.GovRegNumType.ToStringContents());
				result.AppendIfNotEmpty("Mobile: ", addressDataObject.Mobile);
				result.AppendIfNotEmpty("OrganizationCode: ", addressDataObject.OrganizationCode);
				result.AppendIfNotEmpty("Phone: ", addressDataObject.Phone);
				result.AppendIfNotEmpty("Port: ", addressDataObject.Port.ToStringContents());
				result.AppendIfNotEmpty("Postcode: ", addressDataObject.Postcode);
				result.AppendIfNotEmpty("State: ", addressDataObject.State);
				result.AppendIfNotEmpty("UniversalNettingCode: ", addressDataObject.UniversalNettingCode);
				result.AppendIfNotEmpty("UniversalOfficeCode: ", addressDataObject.UniversalOfficeCode);
				AppendIfNotEmpty(result, addressDataObject.RegistrationNumberCollection);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Meassage builder")]
		static void AppendIfNotEmpty(ZStringBuilder result, List<RegistrationNumber> collection)
		{
			if (collection != null && collection.Count > 0)
			{
				int index = 1;

				foreach (var number in collection)
				{
					result.Append(string.Format("RegistrationNumber {0}:", index++));
					result.AppendIfNotEmpty("CountryOfIssue: ", number.CountryOfIssue.ToStringContents());
					result.AppendIfNotEmpty("Type: ", number.Type.ToStringContents());
					result.AppendIfNotEmpty("Value: ", number.Value);
				}
			}
		}
	}
}
