namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondContainerValidationSailingSynchronisationTest : LinkedSailingBillsImportedTest
	{
		public void TestContainerIsNotLinkedToSailing()
		{
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "SCAC";
			bill.B0_MasterBillNumber = "AAA";
			var container = bill.MovementDetail.Containers.AddNew();
			container.BC_ContainerNum = "ABCD1112";
			AssertHasWarning(container.BC_ContainerNumInfo, ValidationConstants.SailingSynchronisation.ContainerMightBeIncorectlyAdded.ToString());
			container.BC_ContainerNum = "ABCD1111";
			AssertNoWarning(container.BC_ContainerNumInfo, ValidationConstants.SailingSynchronisation.ContainerMightBeIncorectlyAdded.ToString());

			container.BC_ContainerNum = "ABCD11112";
			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertNoWarning(container.BC_ContainerNumInfo, ValidationConstants.SailingSynchronisation.ContainerMightBeIncorectlyAdded.ToString());
		}
	}
}
