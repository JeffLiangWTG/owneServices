using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	#region Helper

	public class RequiredTaxNumberHelper : IRequiredTaxNumberHelper
	{
		public RequiredTaxNumberHelper()
		{ }

		public ZString ExpandTaxTypeCodeIfNecessary(ZString taxTypeCode)
		{
			return RequiredTaxNumbers.ExpandTaxTypeCodeIfNecessary(taxTypeCode);
		}

		public IEnumerable<string> GetTaxCodeTypes(ZString docType, ZString countryCode)
		{
			RequiredTaxNumbers.DocumentType documentType;

			if (!Enum.TryParse(docType, out documentType))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Value of '{0}' is not valid. Valid values are {1}.", docType, string.Join(", ", Enum.GetNames(typeof(RequiredTaxNumbers.DocumentType)))));
			}
			return RequiredTaxNumbers.GetTaxCodeTypes(countryCode, documentType);
		}
	}

	#endregion

	public static class RequiredTaxNumbers
	{
		const string CUIT_4DIGITS = "CUIT";
		const string CNPJ_4DIGITS = "CNPJ";
		const string USCI_4DIGITS = "USCI";

		public static ZString GetRequiredTaxNumber(ZString importCountryCode, ZString exportCountryCode, IRegistrationNumberProvider org, TaxOrgType orgType, DocumentType docType)
		{
			return GetRequiredTaxDetail(importCountryCode, exportCountryCode, org, orgType, docType)?.Item2 ?? string.Empty;
		}

		public static IList<ZString> GetRequiredTaxNumbers(ZString importCountryCode, ZString exportCountryCode, IRegistrationNumberProvider org, TaxOrgType orgType, DocumentType docType)
		{
			return GetRequiredTaxDetails(importCountryCode, exportCountryCode, org, orgType, docType)?.Select(x => (ZString)x.Item2).ToList();
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IList<Tuple<string, string>> GetRequiredTaxDetails(ZString importCountryCode, ZString exportCountryCode, IRegistrationNumberProvider org, TaxOrgType orgType, DocumentType docType)
		{
			var countryCode = orgType == TaxOrgType.Shipper
				? exportCountryCode
				: importCountryCode;

			if (!IsApplicable(countryCode, orgType))
			{
				return new List<Tuple<string, string>>();
			}

			if (orgType == TaxOrgType.Shipper && IsEINRequired(exportCountryCode, importCountryCode))
			{
				return GetTaxDetailsWithFallbacks(countryCode, org, new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber }).ToList();
			}

			return GetTaxDetailsWithFallbacks(countryCode, org, GetTaxCodeTypes(countryCode, docType)).ToList();
		}

		public static Tuple<string, string> GetRequiredTaxDetail(ZString importCountryCode, ZString exportCountryCode, IRegistrationNumberProvider org, TaxOrgType orgType, DocumentType docType)
		{
			return GetRequiredTaxDetails(importCountryCode, exportCountryCode, org, orgType, docType).FirstOrDefault();
		}

		static bool IsApplicable(ZString countryCode, TaxOrgType orgType)
		{
			if (orgType != TaxOrgType.Shipper)
			{
				return true;
			}

			switch (countryCode)
			{
				case Core.Constants.CountryCodes.India:
				case Core.Constants.CountryCodes.VietNam:
					return false;

				default:
					return true;
			}
		}

		static IEnumerable<Tuple<string, string>> GetTaxDetailsWithFallbacks(ZString countryCode, IRegistrationNumberProvider org, IEnumerable<string> types)
		{
			foreach (var currentTaxType in types)
			{
				var taxNum = GetTaxNumber(countryCode, org, currentTaxType);
				if (!string.IsNullOrWhiteSpace(taxNum))
				{
					yield return new Tuple<string, string>(currentTaxType, taxNum);
				}
			}
		}

		public static Tuple<string, string> GetTaxDetailsWithFallback(ZString countryCode, IRegistrationNumberProvider org, IEnumerable<string> types)
		{
			return GetTaxDetailsWithFallbacks(countryCode, org, types).FirstOrDefault();
		}

		public static string GetRequiredTaxNumberWithType(ZString importCountryCode, ZString exportCountryCode, OrgHeader org, TaxOrgType orgType, DocumentType docType)
		{
			return GetRequiredTaxNumberWithType(importCountryCode, exportCountryCode, new OrgHeaderRegistrationNumberProvider(org), orgType, docType);
		}

		public static IEnumerable<string> GetRequiredTaxNumberWithTypes(ZString importCountryCode, ZString exportCountryCode, OrgHeader org, TaxOrgType orgType, DocumentType docType)
		{
			return GetRequiredTaxNumberWithTypes(importCountryCode, exportCountryCode, new OrgHeaderRegistrationNumberProvider(org), orgType, docType).ToList();
		}

		public static IEnumerable<string> GetRequiredTaxNumberWithTypes(ZString importCountryCode, ZString exportCountryCode, IRegistrationNumberProvider org, TaxOrgType orgType, DocumentType docType)
		{
			return GetRequiredTaxDetails(importCountryCode, exportCountryCode, org, orgType, docType).Select(detail => CombineTaxTypeAndNumber(detail.Item1, detail.Item2));
		}

		public static string GetRequiredTaxNumberWithType(ZString importCountryCode, ZString exportCountryCode, IRegistrationNumberProvider org, TaxOrgType orgType, DocumentType docType)
		{
			var details = GetRequiredTaxDetail(importCountryCode, exportCountryCode, org, orgType, docType);
			return details == null ? string.Empty : CombineTaxTypeAndNumber(details.Item1, details.Item2, importCountryCode);
		}

		public static string CombineTaxTypeAndNumber(ZString taxTypeCode, ZString taxNumber)
		{
			return CombineTaxTypeAndNumber(taxTypeCode, taxNumber, null);
		}

		public static string CombineTaxTypeAndNumber(ZString taxTypeCode, ZString taxNumber, ZString importCountryCode)
		{
			if (taxNumber.IsEmpty)
			{
				return string.Empty;
			}

			var traderType = ExpandTaxTypeCodeIfNecessary(taxTypeCode, importCountryCode);
			return !traderType.IsEmpty ? string.Concat(traderType, ": ", taxNumber) : (string)taxNumber;
		}

		public static ZString ExpandTaxTypeCodeIfNecessary(ZString taxTypeCode)
		{
			return ExpandTaxTypeCodeIfNecessary(taxTypeCode, null);
		}

		public static ZString ExpandTaxTypeCodeIfNecessary(ZString taxTypeCode, ZString importCountryCode)
		{
			switch (taxTypeCode)
			{
				case ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT:
					return CUIT_4DIGITS;

				case BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ:
					return CNPJ_4DIGITS;

				case OrgCusCode.ChinaCodeTypes.USC:
					return USCI_4DIGITS;
			}

			if (importCountryCode == Core.Constants.CountryCodes.Bangladesh && taxTypeCode == OrgCusCode.CodeTypes.VATCode)
			{
				return OrgCusCode.BangladeshCodeTypes.BIN;
			}

			return taxTypeCode;
		}

		public static IEnumerable<string> GetTaxCodeTypes(ZString countryCode, DocumentType docType)
		{
			if (countryCode == ZString.Empty)
			{
				return Array.Empty<string>();
			}

			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Algeria:
					return new string[] { AlgeriaOrgCusCodeInfo.OrgCusCodes.NIF };

				case Core.Constants.CountryCodes.Argentina:
					return new string[] { ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT };

				case Core.Constants.CountryCodes.Brazil:
					return new string[] { BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration };

				case Core.Constants.CountryCodes.Bangladesh:
					return new string[] { OrgCusCode.CodeTypes.VATCode };

				case Core.Constants.CountryCodes.Chile:
					return new string[] { ChileOrgCusCodeInfo.OrgCusCodes.RUT };

				case Core.Constants.CountryCodes.Mexico:
					return new string[] { MexicoOrgCusCodeInfo.OrgCusCodes.RFC };

				case Core.Constants.CountryCodes.Colombia:
					return new string[] { ColombiaOrgCusCodeInfo.OrgCusCodes.NIT };

				case Core.Constants.CountryCodes.Peru:
					return new string[] { OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode };

				case Core.Constants.CountryCodes.Ecuador:
					return new string[] { OrgCusCode.EcuadorCodeTypes.RUC };

				case Core.Constants.CountryCodes.Turkey:
					return new string[] { OrgCusCode.CodeTypes.VATCode };

				case Core.Constants.CountryCodes.Uruguay:
					return new string[] { UruguayOrgCusCodeInfo.OrgCusCodes.RUT };

				case Core.Constants.CountryCodes.Venezuela:
					return new string[] { OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal };

				case Core.Constants.CountryCodes.Paraguay:
					return new string[] { OrgCusCode.ParaguayCodeTypes.RUC };

				case Core.Constants.CountryCodes.Indonesia:
					return docType == DocumentType.General ? Array.Empty<string>() : new string[] { OrgCusCode.IndonesiaCodeTypes.PPN, OrgCusCode.CodeTypes.PassportID };

				case Core.Constants.CountryCodes.China:
					return docType == DocumentType.General ? Array.Empty<string>() : new string[] { OrgCusCode.ChinaCodeTypes.USC };

				case Core.Constants.CountryCodes.India:
					if (docType == DocumentType.General)
					{
						return new string[] { OrgCusCode.CodeTypes.GSTCode, IndiaOrgCusCodeInfo.OrgCusCodes.IEC };
					}

					return docType == DocumentType.ShippingInstruction ? new string[] { OrgCusCode.CodeTypes.GSTCode } : Array.Empty<string>();

				case Core.Constants.CountryCodes.Kenya:
					return docType == DocumentType.AirwayBill ? new string[] { OrgCusCode.KenyaCodeTypes.PIN } : Array.Empty<string>();

				case Core.Constants.CountryCodes.VietNam:
					return docType == DocumentType.General ? Array.Empty<string>() : new string[] { OrgCusCode.CodeTypes.VATCode };

				default:
					return Array.Empty<string>();
			}
		}

		static ZString GetTaxNumber(ZString countryCode, IRegistrationNumberProvider org, string typeCode)
		{
			return org != null ? org.GetTaxNumber(countryCode, typeCode) : ZString.Empty;
		}

		static bool IsEINRequired(ZString expCountryCode, ZString impCountryCode)
		{
			return Core.Constants.CountryCodes.UnitedStates.Equals(expCountryCode)
				&& (Core.Constants.CountryCodes.Uruguay.Equals(impCountryCode)
				|| Core.Constants.CountryCodes.Paraguay.Equals(impCountryCode));
		}

		public static List<TaxCodeInformation> GetTaxInfoFromRefTable(ZString countryCode, IRegistrationNumberProvider org, BusinessObjectFactory factory = null, string regulatingCountry = null, string issueCountry = null, string docCodeType = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			var filter = new ZQuery(RefDocOrgCusCodeSchema.DOC_RN_NKCodeCountry, countryCode);
			filter.AddToFilter(new ZQuery(RefDocOrgCusCodeSchema.DOC_RN_NKRegulatingCountry, regulatingCountry ?? countryCode));
			filter.OrderBy = RefDocOrgCusCodeSchema.Constants.DOC_Priority + OrderByClause.Ascending;

			if (docCodeType != null)
			{
				filter.AddToFilter(new ZQuery(RefDocOrgCusCodeSchema.DOC_CodeType, docCodeType));
			}

			var refDocOrgCusCodeBOs = factory.Load<RefDocOrgCusCode>(filter);
			var result = new List<TaxCodeInformation>();
			foreach (var refDocOrgCusCodeBO in refDocOrgCusCodeBOs)
			{
				var number = org?.GetTaxNumber(issueCountry ?? countryCode, refDocOrgCusCodeBO.DOC_CodeType) ?? ZString.Empty;
				result.Add(new TaxCodeInformation(
					code: refDocOrgCusCodeBO.DOC_CodeType,
					shortLabel: refDocOrgCusCodeBO.DOC_ShortLabel,
					longLabel: refDocOrgCusCodeBO.DOC_LongLabel,
					description: refDocOrgCusCodeBO.DOC_Description,
					number: number,
					priority: refDocOrgCusCodeBO.DOC_Priority,
					countryCode: issueCountry ?? countryCode,
					documentType: refDocOrgCusCodeBO.DOC_DocumentType,
					comment: refDocOrgCusCodeBO.DOC_Notes,
					regulatingCountryCode: refDocOrgCusCodeBO.RegulatingCountry?.Code ?? ZString.Empty,
					direction: refDocOrgCusCodeBO.DOC_Direction
				));
			}
			return result;
		}

		public enum TaxOrgType
		{
			Shipper,
			Consignee,
			AlsoNotify
		}

		public enum DocumentType
		{
			General,
			AirwayBill,
			ShippingInstruction
		}
	}
}
