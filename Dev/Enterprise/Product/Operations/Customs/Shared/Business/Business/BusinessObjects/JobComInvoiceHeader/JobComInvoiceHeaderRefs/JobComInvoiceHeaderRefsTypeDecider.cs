using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceHeaderRefsTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(JobComInvoiceHeaderRefs);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			var type = (row != null) ? new ZString(row[JobComInvoiceHeaderRefs.Schema.J2_ReferenceType]) : ZString.Empty;
			var countryCode = GetCountryCode(row, factory);
			if (countryCode == Core.Constants.CountryCodes.Canada && type == JobComInvoiceHeaderRefs.Constants.CCN)
			{
				result = ObjectFactory.GetType<Integration.Customs.CA.ICAJobComInvoiceHeaderCCNs>();
			}
			if (countryCode == Core.Constants.CountryCodes.China && type == JobComInvoiceHeaderRefs.Constants.CTR)
			{
				result = ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceHeaderContract>();
			}
			return result ?? GetTypeForCountryCode(countryCode);
		}

		ZString GetCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var invoicePK = (row != null) ? new ZGuid(row[JobComInvoiceHeaderRefs.Schema.J2_JZ]) : ZGuid.Invalid;
			var invoiceHeader = invoicePK.IsValid ? factory.Load<BaseJobComInvoiceHeader>(invoicePK) : null;
			return invoiceHeader != null ? invoiceHeader.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
