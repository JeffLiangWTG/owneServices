using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs._CustomsTemplate_.Business
{
	public partial class JobComInvoiceLine : Customs.Business.BaseJobComInvoiceLine, Integration.Customs._CustomsTemplate_.IJobComInvoiceLine
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		// GetTariffDescription - to be overridden once the Tariff is setup for a new country
		protected override ZString GetTariffDescription(ZString tariffCode) => "TARIFF_DESCRIPTION";
		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes._TemplateCountryName_;
		protected override System.Type TypeOfPartUsedCore => typeof(OrgSupplierPart);
	}
}
