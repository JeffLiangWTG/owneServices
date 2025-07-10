using CargoWise.Customs.TR.MessageContracts.Interfaces.ExportUnion;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public class ExportUnionCompaniesProvider : IExportUnionCompanies
	{
		public ExportUnionCompaniesProvider(OrgAddress orgAddress, ZString addressType, ZDateTime effectiveDate)
		{
			OrgAddress = orgAddress;
			Header = OrgAddress?.Header;
			AddressType = addressType;
			EffectiveDate = effectiveDate;
		}
		OrgAddress OrgAddress { get; }
		OrgHeader Header { get; }
		ZString AddressType { get; }
		ZDateTime EffectiveDate { get; }

		public string CompanyType
		{
			get
			{
				switch (AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						return CusEntryMessageConstants.ExportUnionConstants.CompanyTypes.Exporter;
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
						return CusEntryMessageConstants.ExportUnionConstants.CompanyTypes.Importer;
					case DocAddressTypes.Codes.Declarant:
						return CusEntryMessageConstants.ExportUnionConstants.CompanyTypes.DeclarationOwner;
					case DocAddressTypes.Codes.Representative:
						return CusEntryMessageConstants.ExportUnionConstants.CompanyTypes.FinancialConsultant;
					case DocAddressTypes.Codes.COLSResponsibleParty:
						return CusEntryMessageConstants.ExportUnionConstants.CompanyTypes.Responsible;
					case DocAddressTypes.Codes.Manufacturer:
						return CusEntryMessageConstants.ExportUnionConstants.CompanyTypes.Producer;
					default:
						return string.Empty;
				}
			}
		}

		public string TaxRegistrationCode => Header != null ? Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) : ZString.Empty;
		public string TaxOffice => Header != null ? Header.CustomsCodes.GetCustomsRegNo(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, Core.Constants.CountryCodes.Turkey) : ZString.Empty;
		public string RegiteredName => Header != null ? Header.OH_FullName : ZString.Empty;
		public string Address1 => OrgAddress != null ? OrgAddress.Address1 : ZString.Empty;
		public string Address2 => OrgAddress != null ? OrgAddress.Address2 : ZString.Empty;
		public string Town => ZString.Empty;
		public string Province => OrgAddress != null ? OrgAddress.City : ZString.Empty;
		public string Zipcode => OrgAddress != null ? OrgAddress.Postcode : ZString.Empty;
		public string Country => OrgAddress != null ? UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(OrgAddress.Factory, OrgAddress.Country.Code, EffectiveDate) : ZString.Empty;
		public string TelephoneNumber => OrgAddress != null ? OrgAddress.PhoneNumber.ToString() : ZString.Empty;
		public string FaxNumber => OrgAddress != null ? OrgAddress.FaxNumber.ToString() : ZString.Empty;
	}
}
