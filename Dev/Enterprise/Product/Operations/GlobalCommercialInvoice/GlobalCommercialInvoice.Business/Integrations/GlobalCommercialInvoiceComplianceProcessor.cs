using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;
using static Enterprise.GlobalCommercialInvoice.Integration.Constants.Compliance;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	public class GlobalCommercialInvoiceComplianceProcessor : IGlobalCommercialInvoiceComplianceProcessor
	{
		/// <summary>
		/// This facilitates compliance screening for parties collections
		/// </summary>
		internal static IEnumerable<IScreeningParty> GetParties(BusinessObject hostBusinessObject, GlobalCommercialInvoiceHeaderCollection invoiceHeaders)
		{
			if (!ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled)
			{
				return Enumerable.Empty<IScreeningParty>();
			}

			var orgHeaderPKs = invoiceHeaders.Select(invoice => invoice.GIH_OH_Importer)
				.Union(invoiceHeaders.Select(invoice => invoice.GIH_OH_Supplier))
				.Where(invoice => invoice.IsValid).ToArray();
			var orgHeaders = hostBusinessObject.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgHeaderPKs));

			var parties = new List<ScreeningParty>();
			parties.AddRange(orgHeaders
				.Where(orgHeader => invoiceHeaders.Any(header => orgHeader.PK == header.GIH_OH_Importer))
				.Select(orgHeader => new ScreeningParty(hostBusinessObject, Parties.ImporterDescription, orgHeader)));

			parties.AddRange(orgHeaders
				.Where(orgHeader => invoiceHeaders.Any(header => orgHeader.PK == header.GIH_OH_Supplier))
				.Select(orgHeader => new ScreeningParty(hostBusinessObject, Parties.SupplierDescription, orgHeader)));

			return parties;
		}

		/// <summary>
		/// This facilitates compliance screening for locations collections
		/// </summary>
		internal static IEnumerable<IComplianceLocation> GetLocations(BusinessObject hostBusinessObject, GlobalCommercialInvoiceHeaderCollection invoiceHeaders)
		{
			if (!ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled)
			{
				return Enumerable.Empty<IComplianceLocation>();
			}

			var refCountryCodes = invoiceHeaders.Select(invoice => invoice.GIH_RN_NKCountryExport)
				.Union(invoiceHeaders.Select(invoice => invoice.GIH_RN_NKCountryImport))
				.Union(invoiceHeaders.Select(invoice => invoice.GIH_RN_NKCountryOrigin))
				.Where(invoice => !invoice.IsEmpty).ToArray();
			var refCountries = hostBusinessObject.Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, refCountryCodes));

			var locations = new List<IComplianceLocation>();
			locations.AddRange(refCountries.Where(country => invoiceHeaders.Any(header => country.RN_Code == header.GIH_RN_NKCountryExport))
				.Select(country => new ScreeningParty(hostBusinessObject, Locations.CountryOfExportDescription, country)));

			locations.AddRange(refCountries.Where(country => invoiceHeaders.Any(header => country.RN_Code == header.GIH_RN_NKCountryImport))
				.Select(country => new ScreeningParty(hostBusinessObject, Locations.CountryOfImportDescription, country)));

			locations.AddRange(refCountries.Where(country => invoiceHeaders.Any(header => country.RN_Code == header.GIH_RN_NKCountryOrigin))
				.Select(country => new ScreeningParty(hostBusinessObject, Locations.GoodsCountryOfOriginDescription, country)));

			return locations;
		}

		/// <summary>
		/// This facilitates compliance tariff assessment for commodities collections
		/// </summary>
		internal static IEnumerable<IComplianceCommodity> GetCommodities(BusinessObject hostBusinessObject, GlobalCommercialInvoiceHeaderCollection invoiceHeaders, GlobalCommercialInvoiceLineIntegratedCollection invoiceLines)
		{
			if (!ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled)
			{
				return Enumerable.Empty<IComplianceCommodity>();
			}

			var jobNumber = ((IGlobalCommercialInvoiceJobProvider)hostBusinessObject).JobNumber;
			var parentID = ((IComplianceItemRiskStatusProvider)hostBusinessObject).ParentID;

			var tariffCodes = invoiceLines.Select(line => line.GIL_Tariff1)
				.Union(invoiceLines.Select(line => line.GIL_Tariff2))
				.Where(line => !line.IsEmpty).ToArray();

			var commodities = new List<IComplianceCommodity>();
			commodities.AddRange(invoiceLines.Where(line => tariffCodes.Any(tariff => tariff == line.GIL_Tariff1))
				.Select(line => new ComplianceCommodity(line.GIL_Tariff1, Codes.WorldCustomsOrganisationWCO, jobNumber, parentID,
					invoiceHeaders.Single(header => header.PK == line.GIL_GIH_Header).GIH_RN_NKCountryOrigin,
					Commodities.CommercialInvoiceDescription, line.GIL_Description)));

			commodities.AddRange(invoiceLines.Where(line => tariffCodes.Any(tariff => tariff == line.GIL_Tariff2))
				.Select(line => new ComplianceCommodity(line.GIL_Tariff2, Codes.WorldCustomsOrganisationWCO, jobNumber, parentID,
					invoiceHeaders.Single(header => header.PK == line.GIL_GIH_Header).GIH_RN_NKCountryOrigin,
					Commodities.CommercialInvoiceDescription, line.GIL_Description)));

			return commodities;
		}

		public IGlobalCommercialInvoiceComplianceProvider GetGlobalCommercialInvoiceBusinessObject(IBusiness hostBusinessEntity)
		{
			return new GlobalCommercialInvoiceBusinessObject(hostBusinessEntity);
		}
	}
}
