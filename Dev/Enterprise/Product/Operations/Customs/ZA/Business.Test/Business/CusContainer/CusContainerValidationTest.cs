namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestContainerNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = dec.CusContainers.AddNew();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container.PK;
			container.CO_ContainerNumber = "CRXU1234561";
			AssertHasWarningContaining(container.CO_ContainerNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be");
			AssertHasWarningContaining(container.CO_ContainerNumberInfo, ValidationConstants.Containers.ContainerNotISO);
			container.CO_ContainerNumber = "ABC12345DE";
			AssertHasWarning(container.CO_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertHasWarningContaining(container.CO_ContainerNumberInfo, ValidationConstants.Containers.ContainerNotISO);
			container.CO_ContainerNumber = "APLU1234564";
			AssertNoWarnings(container.CO_ContainerNumberInfo);
			AssertNoWarningContaining(container.CO_ContainerNumberInfo, ValidationConstants.Containers.ContainerNotISO);
			container.CO_ContainerNumber = "PLTT7500126";
			AssertHasWarningContaining(container.CO_ContainerNumberInfo, ValidationConstants.Containers.ContainerNotISO);
		}

		public void TestExportCO_FCL_LCL_AIRListValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "XXX";
			AssertHasWarnings(container.CO_FCL_LCL_AIRInfo);
			container.CO_FCL_LCL_AIR = "LCL";
			AssertNoNotifications(container.CO_FCL_LCL_AIRInfo);
		}

		public void TestCO_FCL_LCL_AIR_ModesNeedToMatchOrBeEMPWhenJobDeclarationContainerModeIsCNT()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertNoNotifications(container1.CO_FCL_LCL_AIRInfo);
			AssertNoNotifications(container2.CO_FCL_LCL_AIRInfo);
			AssertHasError(container3.CO_FCL_LCL_AIRInfo, "Modes for all containers need to match, or be EMP");
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertNoNotifications(container1.CO_FCL_LCL_AIRInfo);
			AssertNoNotifications(container2.CO_FCL_LCL_AIRInfo);
			AssertNoNotifications(container3.CO_FCL_LCL_AIRInfo);
		}

		public void TestImportCO_FCL_LCL_AIRListValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "XXX";
			AssertHasMessageErrors(container.CO_FCL_LCL_AIRInfo);
			container.CO_FCL_LCL_AIR = "LCL";
			AssertNoNotifications(container.CO_FCL_LCL_AIRInfo);
		}

		public void TestIsValidContainerNumberForZA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = dec.CusContainers.AddNew();
			container.CO_ContainerNumber = "PLTT7500126";
			AssertEquals(false, CusContainerValidation.IsValidContainerNumberForZA(container.CO_ContainerNumber));
			container.CO_ContainerNumber = "PLTU7500123";
			AssertEquals(true, CusContainerValidation.IsValidContainerNumberForZA(container.CO_ContainerNumber));
		}
	}
}
