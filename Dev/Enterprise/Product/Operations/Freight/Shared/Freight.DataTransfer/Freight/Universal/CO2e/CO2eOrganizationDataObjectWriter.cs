using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CO2eOrganizationDataObjectWriter : DataObjectWriter<OrgAddress, OrganizationAddress>
	{
		readonly string addressType;

		public CO2eOrganizationDataObjectWriter(IDataWritingManager writeManager, string addressType)
			: base(writeManager)
		{
			this.addressType = addressType;
		}

		protected override OrganizationAddress PopulateDataObject(OrgAddress addressBO)
		{
			if (addressBO == null || addressBO.Header == null)
			{
				return null;
			}

			var addressData = new OrganizationAddress(writeManager.WriterStrategy);

			addressData.AddressType = addressType;

			PopulateRegistrationNumbers(addressBO.Header, addressData, addressBO);

			return addressData;
		}

		void PopulateRegistrationNumbers(OrgHeader organizationBO, OrganizationAddress addressData, OrgAddress addressBO)
		{
			var carrierAlphaCusCode = organizationBO.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode);
			var govRegNumBO = organizationBO.PrimaryRegistrationNumber.CusCode;
			if (carrierAlphaCusCode != null && carrierAlphaCusCode != govRegNumBO && (carrierAlphaCusCode.OK_OA_PremisesAddress == ZGuid.Empty || carrierAlphaCusCode.OK_OA_PremisesAddress == addressBO.PK))
			{
				if (addressData.RegistrationNumberCollection != null || addressData.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()))
				{
					addressData.RegistrationNumberCollection.Add(new RegistrationNumber()
					{
						Type = ListHelper.GetWithDescription<RegistrationNumberType>(carrierAlphaCusCode.OK_CodeType, carrierAlphaCusCode.Lookups.OK_CodeType_List),
						CountryOfIssue = Country.New(carrierAlphaCusCode.CodeCountry),
						Value = carrierAlphaCusCode.OK_CustomsRegNo,
					});
				}
			}
		}
	}
}
