using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class OrganizationAddressReaderHelper<TBusinessObject> where TBusinessObject : BusinessObject
	{
		public OrganizationAddressReaderHelper(IXmlImportLogger logger, UniversalObjectFactory factory, TBusinessObject targetBO, DocAddressType docAddressType)
		{
			this.factory = factory;
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
			this.targetBO = targetBO;
			this.docAddressType = docAddressType;
		}
		public readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly TBusinessObject targetBO;
		readonly DocAddressType docAddressType;

		public ZGuid GetOrCreateDocAddress(List<OrganizationAddress> xmlAddresses, IEnumerable<JobDocAddress> parentAddresses, Func<TBusinessObject, DocAddressType, OrganizationAddress, JobDocAddress> createAddress, bool compareMobile = true)
		{
			if (xmlAddresses == null)
			{
				return ZGuid.Empty;
			}

			var docTypeAddress = xmlAddresses.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == docAddressType.ToString());
			if (docTypeAddress == null)
			{
				return ZGuid.Empty;
			}

			var orgAddressPK = ZGuid.Empty;
			if (!docTypeAddress.AddressOverride.GetValueOrDefault())
			{
				var reader = new OrganisationDataObjectReader(docTypeAddress, logger, factory);
				var orgAddress = reader.GetMatched();
				orgAddressPK = orgAddress == null ? ZGuid.Empty : orgAddress.PK;
			}

			var jobDocAddress = parentAddresses?.FirstOrDefault(docAddress =>
				docAddress.DocAddressType == docAddressType &&
				IsAddressMatched(docTypeAddress, docAddress, orgAddressPK, compareMobile))
					?? createAddress(targetBO, docAddressType, docTypeAddress);

			return jobDocAddress?.PK ?? ZGuid.Empty;
		}

		ZBool IsAddressMatched(OrganizationAddress orgAddress, JobDocAddress docAddress, ZGuid orgAddressPK, bool compareMobile)
		{
			var addressOverride = orgAddressPK.IsEmpty || orgAddress.AddressOverride.GetValueOrDefault();
			var isMatched = addressOverride == docAddress.E2_AddressOverride && orgAddress.Contact.GetValueOrDefault() == docAddress.E2_Contact;
			if (!addressOverride && isMatched)
			{
				isMatched = orgAddressPK == docAddress.E2_OA_Address;
			}

			if (addressOverride && isMatched)
			{
				isMatched = orgAddress.CompanyName.GetValueOrDefault() == docAddress.E2_CompanyName && orgAddress.Address1.GetValueOrDefault() == docAddress.E2_Address1
							&& orgAddress.Address2.GetValueOrDefault() == docAddress.E2_Address2 && orgAddress.Country.GetCodeAsUpperCase() == docAddress.E2_RN_NKCountryCode
							&& ((ZString?)orgAddress.State).GetValueOrDefault() == docAddress.E2_State && orgAddress.City.GetValueOrDefault() == docAddress.E2_City
							&& orgAddress.Postcode.GetValueOrDefault() == docAddress.E2_Postcode && (!compareMobile || orgAddress.Mobile.GetValueOrDefault() == docAddress.E2_Mobile)
							&& orgAddress.Phone.GetValueOrDefault() == docAddress.E2_Phone && orgAddress.Fax.GetValueOrDefault() == docAddress.E2_Fax
							&& orgAddress.Email.GetValueOrDefault() == docAddress.E2_Email;
				if (isMatched && !orgAddress.GovRegNum.GetValueOrDefault().IsEmpty)
				{
					isMatched = orgAddress.GovRegNumType.GetCodeAsUpperCase() == docAddress.E2_GovRegNumType
							&& orgAddress.GovRegNum.GetValueOrDefault() == docAddress.E2_GovRegNum;
				}
			}
			return isMatched;
		}
	}
}
