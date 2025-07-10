using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class JobComInvLineRefsTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(JobComInvLineRefs);

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			Type type = null;
			if (row != null)
			{
				var invoiceLinePK = new ZGuid(row[JobComInvLineRefs.Schema.JG_JI]);
				var invoiceLine = invoiceLinePK.IsValid ? factory.Load<BaseJobComInvoiceLine>(invoiceLinePK) : null;
				if (invoiceLine != null)
				{
					var typeCode = new ZString(row[JobComInvLineRefsSchema.JG_ReferenceType.Name]);
					type = GetTypeForLoad(typeCode, invoiceLine);
					if (type == null)
					{
						var countryCode = GetCountryCode(invoiceLine);
						type = GetTypeForCountryCode(countryCode);
					}
				}
			}
			return type;
		}

		ZString GetCountryCode(BaseJobComInvoiceLine invoiceLine)
		{
			var invoiceHeader = invoiceLine.InvoiceHeader;
			return invoiceHeader != null ? invoiceHeader.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		Type GetTypeForLoad(ZString type, BaseJobComInvoiceLine invoiceLine)
		{
			Type result = null;

			if (!type.IsEmpty)
			{
				(invoiceLine as IJobComInvLineRefsTypeSupporter)?.GetJobComInvLineRefsTypes()?.TryGetValue(type, out result);
			}
			return result;
		}
	}
}
