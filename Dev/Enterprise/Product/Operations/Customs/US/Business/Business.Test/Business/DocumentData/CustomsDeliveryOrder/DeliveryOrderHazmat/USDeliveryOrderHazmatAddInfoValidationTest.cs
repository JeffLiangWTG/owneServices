using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDeliveryOrderHazmatAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_UNNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var hazmat = header.DeliveryOrderHazmats.AddNew();
			MasterFiles.Business.UNDGSubstance substance = Factory.New<MasterFiles.Business.UNDGSubstance>();
			substance.DG_Code = "2643Z";
			substance.DG_Class = "1.3";
			substance.DG_PG = "I";
			substance.DG_PSN = "PROP DUMMY BLAH";
			hazmat.US_UNNumber = "~";
			AssertHasWarning(hazmat.US_UNNumberInfo, USDeliveryOrderHazmatAddInfoValidation.UNNumberShouldBeInList);
			hazmat.US_UNNumber = "2643Z";
			AssertNoWarning(hazmat.US_UNNumberInfo, USDeliveryOrderHazmatAddInfoValidation.UNNumberShouldBeInList);
			AssertNoErrorContaining(hazmat.US_UNNumberInfo, MandatoryValidation.MustBeEntered);
			hazmat.US_UNNumber = ZString.Empty;
			AssertHasErrorContaining(hazmat.US_UNNumberInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
