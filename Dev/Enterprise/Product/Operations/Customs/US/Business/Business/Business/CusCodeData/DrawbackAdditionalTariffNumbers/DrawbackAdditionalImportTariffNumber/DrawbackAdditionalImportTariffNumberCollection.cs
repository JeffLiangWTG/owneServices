
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackAdditionalImportTariffNumberCollection : DependentCusAddInfoCollection<DrawbackAdditionalImportTariffNumber, JobComInvoiceLine>
	{
		public DrawbackAdditionalImportTariffNumberCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber)
		{
		}

		public bool ContainsTariffNumber(ZString tariffNumber)
		{
			return this.OfType<DrawbackAdditionalImportTariffNumber>().Any(element => element.US_FormattedTariff == tariffNumber || element.US_Tariff == tariffNumber);
		}
	}
}
