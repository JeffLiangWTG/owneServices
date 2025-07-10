using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public static class UniversalExtensions
	{
		public static IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing(this BusinessObjectFactory factory)
		{
			IEnumerable<string> result = null;
			if (factory == null)
			{
				result = Enumerable.Empty<string>();
			}
			else
			{
				result = factory.GetCachedValue<IEnumerable<string>>("ZAInvoiceLineAddInfosApplicableForInwardWarehousing", () =>
				{
					return new string[]
					{
						JobComInvoiceLine.Schema.JI_EngineNumber.Substring(3),
						JobComInvoiceLine.Schema.JI_ROOCert.Substring(3),
						JobComInvoiceLine.Schema.JI_VIN.Substring(3),
						JobComInvoiceLine.Schema.JI_NewUsed.Substring(3)
					}.OrderBy(o => o);
				});
			}
			return result;
		}
	}
}
