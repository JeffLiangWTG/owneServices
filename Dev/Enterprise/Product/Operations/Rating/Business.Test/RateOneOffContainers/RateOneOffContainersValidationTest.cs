using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateOneOffContainersValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTC_RC()
		{
			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();
			var container = oneOffShipment.Containers.AddNew();
			container.Validation.ValidateTC_RC();
			AssertHasError(container.TC_RCInfo, "Please enter a " + container.TC_RCInfo.Description + ".");
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertNoErrors(container.TC_RCInfo);

			var looseCargo = oneOffShipment.LooseCargo.AddNew();
			looseCargo.Validation.ValidateTPL_RC_RefContainer();
			AssertNoErrors(looseCargo.TPL_RC_RefContainerInfo);
		}
	}
}
