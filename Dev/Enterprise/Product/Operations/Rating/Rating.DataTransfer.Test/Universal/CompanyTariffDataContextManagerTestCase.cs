using CargoWise.Types;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(CompanyTariffDataContextManager))]
	public class CompanyTariffDataContextManagerTestCase : RatingHeaderDataContextManagerTestCase<CompanyTariffDataContextManager, CompanyTariff>
	{
		/// <summary>
		/// Company Tariff does not have TH_OH
		/// </summary>
		protected override ZGuid CreateAndSetOrgHeaderForTestData(RatingHeader bizO)
		{
			bizO.TH_OH = ZGuid.Empty;
			return ZGuid.Empty;
		}

		protected override CompanyTariff GetBusinessObjectForTesting() => Factory.NewWithValidTestData<CompanyTariff>();

		protected override RatingHeaderDataContextManager<CompanyTariff> GetNewContextManagerForTesting() => new CompanyTariffDataContextManager();
	}
}
