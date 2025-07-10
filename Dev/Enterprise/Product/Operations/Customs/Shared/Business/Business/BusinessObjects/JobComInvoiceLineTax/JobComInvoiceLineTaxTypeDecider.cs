using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceLineTaxTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceLineTax>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceLineTax>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.IJobComInvoiceLineTax>(); }),
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(JobComInvoiceLineTax);

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IJobComInvoiceLineTax>();

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var invoiceLine = GetInvoiceLine(row, factory);
			var country = invoiceLine?.InvoiceHeader?.JobDeclaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return GetTypeForCountryCode(country);
		}

		BaseJobComInvoiceLine GetInvoiceLine(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var invoicelLinePk = (row != null) ? new ZGuid(row[JobComInvoiceLineTax.Schema.JLT_JI]) : ZGuid.Invalid;
			return (invoicelLinePk.IsValid) ? factory.Load<BaseJobComInvoiceLine>(invoicelLinePk) : null;
		}
	}
}
