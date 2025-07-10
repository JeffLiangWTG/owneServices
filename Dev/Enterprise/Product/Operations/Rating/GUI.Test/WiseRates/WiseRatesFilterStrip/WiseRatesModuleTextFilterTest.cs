using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(WiseRatesModuleTextFilter))]
	public class WiseRatesModuleTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber, RateEntrySchema.TI_ContractNumber);
		}
	}
}
