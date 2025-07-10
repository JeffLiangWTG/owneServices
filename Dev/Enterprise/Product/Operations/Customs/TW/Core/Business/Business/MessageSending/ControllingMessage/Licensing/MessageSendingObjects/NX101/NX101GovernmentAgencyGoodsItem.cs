using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GovernmentAgencyGoodsItem : INX101GovernmentAgencyGoodsItem
	{
		readonly CusTWControllingMessageHeader header;

		public NX101GovernmentAgencyGoodsItem(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		public IEnumerable<IPartyDetails> Manufacturers
		{
			get
			{
				var localProcessorAddress = header.LocalProcessorAddress;
				yield return GetManufacturer(localProcessorAddress, YesNoList.Codes.Yes);

				var keys = new HashSet<(ZString ChineseName, ZString EnglishName, ZString ID)> { (localProcessorAddress.CompanyChineseName, localProcessorAddress.CompanyEnglishName, localProcessorAddress.IDCode) };
				foreach (ControllingMessageHeaderLinkInvoiceLine link in header.ControllingMessageHeaderLinkInvoiceLines)
				{
					if (link.Invoiceline is JobComInvoiceLine invoiceline)
					{
						var manufacturer = invoiceline.ManufacturerDocAddress;
						var chineseName = manufacturer.CompanyChineseName;
						var englishName = manufacturer.CompanyEnglishName;
						var idCode = manufacturer.IDCode;
						if (!(chineseName.IsEmpty && englishName.IsEmpty && idCode.IsEmpty) && !keys.Any(key => IsSameCompany(key, manufacturer)))
						{
							keys.Add((chineseName, englishName, idCode));
							yield return GetManufacturer(manufacturer, YesNoList.Codes.No);
						}
					}
				}
			}
		}

		bool IsSameCompany((ZString ChineseName, ZString EnglishName, ZString ID) key, TWJobDocAddress address)
		{
			if (key.ID == address.IDCode)
			{
				var keyChineseName = key.ChineseName;
				var addressChineseName = address.CompanyChineseName;
				if (keyChineseName.IsEmpty || addressChineseName.IsEmpty)
				{
					return key.EnglishName == address.CompanyEnglishName;
				}
				else
				{
					return keyChineseName == addressChineseName;
				}
			}
			else
			{
				return false;
			}
		}

		public IEnumerable<IOrigin> Origins
		{
			get
			{
				IEnumerable<IOrigin> result = null;

				if (CertificateTypes.Contains(header.TW1_CertificateType))
				{
					result = header.ControllingMessageHeaderLinkInvoiceLines.Where(x => !(x.Invoiceline?.JI_CountryOfOrigin.IsEmpty ?? true)).Select(x => x.Invoiceline.JI_CountryOfOrigin).Distinct().Select(x => new OriginWrapper(countryCode: x));
				}

				return result;
			}
		}

		public IEnumerable<IPreviousDocument> PreviousDocuments => header.PreviousDocumentNumbers.Where(x => !x.CSI_ReferenceNumber.IsEmpty).Select(x => new PreviousDocumentWrapper(x.CSI_ReferenceNumber));

		PartyWrapper GetManufacturer(TWJobDocAddress jobDocAddress, ZString mainManufacturer)
		{
			var address = new AddressWrapper(line: new AddressData(jobDocAddress, SharedHelper.GetEnglishLanguageCodes()).EnglishAddressFormat, chineseLine: new AddressData(jobDocAddress.E2_AddressOverride ? jobDocAddress.LocalAddress : jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional).ChineseTraditionalAddressFormat);
			var communications = GetCommunications(jobDocAddress);
			return new PartyWrapper(id: jobDocAddress.IDCode, name: jobDocAddress.CompanyEnglishName, chineseName: jobDocAddress.CompanyChineseName, mainManufacturer: mainManufacturer, typeCode: jobDocAddress.TypeCode, address: address, communications: communications);
		}

		IEnumerable<ICommunication> GetCommunications(TWJobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				var communications = new List<(ZString id, ZString typeID)>
				{
					(jobDocAddress.E2_Phone, MessageConstants.CommunicationTypeIDs.TE),
					(jobDocAddress.E2_Email, MessageConstants.CommunicationTypeIDs.MA),
					(jobDocAddress.E2_Fax, MessageConstants.CommunicationTypeIDs.FX),
				};

				foreach (var communication in communications)
				{
					if (!communication.id.IsEmpty)
					{
						yield return new CommunicationWrapper(communication.id, communication.typeID);
					}
				}
			}
		}

		HashSet<ZString> CertificateTypes => certificateTypes ??= [CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17];
		HashSet<ZString> certificateTypes;
	}
}
