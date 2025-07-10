using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class SendingMessageValidationHelperTest : TestCaseWithFactory
	{
		public void TestShouldValidation()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill1.PK);
			var container1 = moveDetail1.Containers.AddNew();
			container1.ShouldSend = true;
			var container2 = moveDetail1.Containers.AddNew();
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, true);
			header.ValidationModes = ValidationModes.BillOfLadingLevelArrival;
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, false);
			header.ValidationModes = ValidationModes.InBondLevelArrival;
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, false);
			header.ValidationModes = ValidationModes.ContainerLevelArrival;
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, false);
			header.ValidationModes = ValidationModes.InBondLevelExportation;
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, false);
			header.ValidationModes = ValidationModes.BillOfLadingLevelExportation;
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, false);
			header.ValidationModes = ValidationModes.ContainerLevelExportation;
			AssertShouldValidationByChangingValidationModes(header, container1, true, container2, false);
		}

		void AssertShouldValidationByChangingValidationModes(CusInBondHeader header, CusInBondContainer container1, bool container1Result, CusInBondContainer container2, bool container2Result)
		{
			var moveHeaderHelper = new SendingMessageValidationHelper(header, container1, container1.ShouldSend);
			AssertEquals(container1Result, moveHeaderHelper.ShouldValidation);
			moveHeaderHelper = new SendingMessageValidationHelper(header, container2, container2.ShouldSend);
			AssertEquals(container2Result, moveHeaderHelper.ShouldValidation);
		}
	}
}
