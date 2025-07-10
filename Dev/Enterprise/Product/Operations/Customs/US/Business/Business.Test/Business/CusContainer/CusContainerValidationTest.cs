using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestFlags()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusContainer container = declaration.CusContainers.AddNew();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals(true, container.Validation.IsEntrySummaryValidationMode);
			declaration.US_EnableINB = true;
			AssertEquals(true, container.Validation.IsEntrySummaryValidationMode);
		}

		public void TestCheckCO_RCForAPHIS()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_EnableENS = true;
			declaration1.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration1.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var invoice = declaration1.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_APHISInd = "D";
			var container1 = invoiceLine.Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1234562";
			var npContainer1 = invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1);
			npContainer1.IsForInvoiceLine = true;
			invoiceLine.APHISHeaders.AddNew();
			const string noContainerTypeMessage = "Container Type is required for APHIS.";
			const string noContainerLengthMessage = "Container type must include a length for APHIS.";
			container1.Validation.ValidateCO_RC();
			AssertHasMessageError(container1.CO_RCInfo, noContainerTypeMessage);
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(USContainerCodeList.Codes.RR, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container1.CO_RC = refContainer.PK;
			container1.Validation.ValidateCO_RC();
			AssertNoMessageError(container1.CO_RCInfo, noContainerTypeMessage);
			AssertHasMessageError(container1.CO_RCInfo, noContainerLengthMessage);
			refContainer.RC_Length = 23.43m;
			container1.Validation.ValidateCO_RC();
			AssertNoMessageError(container1.CO_RCInfo, noContainerLengthMessage);
		}

		public void TestCheckCO_RCForAMS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_AMSInd = "D";
			// not linked to invoice lines
			var container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "CRUX0000000";
			// linked to invoice line without AMS
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX0000001";
			// linked to invoice line with AMS 
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX0000002";
			var npContainer1 = invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1);
			npContainer1.IsForInvoiceLine = true;
			var npContainer2 = invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2);
			npContainer2.IsForInvoiceLine = true;
			invoiceLine2.AMSLines.AddNew();
			const string noContainerTypeMessage = "Container Type is required for AMS.";
			const string noContainerLengthMessage = "Container type must include a length for AMS.";
			// no container type
			container0.Validation.ValidateCO_RC();
			AssertNoMessageError(container0.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container0.CO_RCInfo, noContainerLengthMessage);
			container1.Validation.ValidateCO_RC();
			AssertNoMessageError(container1.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container1.CO_RCInfo, noContainerLengthMessage);
			container2.Validation.ValidateCO_RC();
			AssertHasMessageError(container2.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container2.CO_RCInfo, noContainerLengthMessage);
			// container type without length
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(USContainerCodeList.Codes.RR, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container0.CO_RC = refContainer.PK;
			container1.CO_RC = refContainer.PK;
			container2.CO_RC = refContainer.PK;
			container0.Validation.ValidateCO_RC();
			AssertNoMessageError(container0.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container0.CO_RCInfo, noContainerLengthMessage);
			container1.Validation.ValidateCO_RC();
			AssertNoMessageError(container1.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container1.CO_RCInfo, noContainerLengthMessage);
			container2.Validation.ValidateCO_RC();
			AssertNoMessageError(container2.CO_RCInfo, noContainerTypeMessage);
			AssertHasMessageError(container2.CO_RCInfo, noContainerLengthMessage);
			// container type with length
			refContainer.RC_Length = 23m;
			container0.Validation.ValidateCO_RC();
			AssertNoMessageError(container0.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container0.CO_RCInfo, noContainerLengthMessage);
			container1.Validation.ValidateCO_RC();
			AssertNoMessageError(container1.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container1.CO_RCInfo, noContainerLengthMessage);
			container2.Validation.ValidateCO_RC();
			AssertNoMessageError(container2.CO_RCInfo, noContainerTypeMessage);
			AssertNoMessageError(container2.CO_RCInfo, noContainerLengthMessage);
		}

		public override void TestContainersRequirePackagesValidation()
		{
			Assert(true);
		}

		public void TestParent()
		{
			AssertEquals(container.Validation.Parent, container);
		}

		public void TestCheckCO_Seal()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "TEST";
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRXU1234568";
			container1.CO_Seal = "SEAL1";
			AssertNoMessageError(container1.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRXU1234569";
			container2.CO_Seal = "SEAL2";
			AssertNoMessageError(container1.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container2.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			CusContainer container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CRXU1234558";
			container3.CO_Seal = "";
			AssertNoMessageError(container1.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container2.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container3.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			CusContainer container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CRXU1234559";
			container4.CO_Seal = "";
			AssertNoMessageError(container1.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container2.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container3.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container4.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			//Add one more container row with same Seal ID
			CusContainer container5 = declaration.CusContainers.AddNew();
			container5.CO_ContainerNumber = "CRXU1234567";
			container5.CO_Seal = "SEAL1";
			AssertNoMessageError(container1.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container2.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container3.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container4.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertHasMessageError(container5.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusContainer container6 = declaration.CusContainers.AddNew();
			container6.CO_ContainerNumber = "CRXU1234537";
			container5.CO_Seal = "SEAL2";
			container6.CO_Seal = "SEAL1";
			AssertNoMessageError(container1.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container2.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container3.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container4.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container5.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
			AssertNoMessageError(container6.CO_SealInfo, CusContainerValidation.DuplicateSealNumberErrorMessage);
		}

		public void TestCheckCO_FCL_LCL_AIR()
		{
			container.CO_FCL_LCL_AIR = "~";
			AssertHasMessageError(container.CO_FCL_LCL_AIRInfo, CusContainerValidation.FCL_LCL_AIRShouldBeInList);
			container.CO_FCL_LCL_AIR = container.Lookups.CO_FCL_LCL_NCT_List[0].Code;
			AssertNoMessageError(container.CO_FCL_LCL_AIRInfo, CusContainerValidation.FCL_LCL_AIRShouldBeInList);
		}

		public void TestCheckCO_WeightUQ()
		{
			container.CO_WeightUQ = "~";
			AssertHasMessageError(container.CO_WeightUQInfo, CusContainerValidation.WeightUQShouldBeInList);
			container.CO_WeightUQ = container.Lookups.WeightUnits[0].Code;
			AssertNoMessageError(container.CO_WeightUQInfo, CusContainerValidation.WeightUQShouldBeInList);
		}

		public void TestCheckCO_ContainerNumber()
		{
			bool autoAllocate = CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			try
			{
				CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				declaration.US_EntryType = "";
				declaration.US_EnableINB = true;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "wer8";
				declaration.PrimaryMasterBill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				JobComInvoiceLine nonContainerisedInvoiceLine = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLine containerisedInvoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				declaration.US_EnableINB = false;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				container.CO_ContainerNumber = "CRUX1234562";
				RefContainer refContainer = Factory.NewWithValidTestData<RefContainer>();
				refContainer.SetCountrySpecificContainerCode(USContainerCodeList.Codes.RR, Enterprise.Core.Constants.CountryCodes.UnitedStates);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				container.CO_ContainerNumber = "AAL3048";
				AssertNoWarning(container.CO_ContainerNumberInfo, string.Format(CusContainerValidation.TreatedAsRailCarNumber, USContainerCodeList.Codes.RR, CusContainer.RailCarCodes.GetDescriptionFromCode(USContainerCodeList.Codes.RR)));
				container.CO_RC = refContainer.PK;
				AssertHasWarning(container.CO_ContainerNumberInfo, string.Format(CusContainerValidation.TreatedAsRailCarNumber, USContainerCodeList.Codes.RR, CusContainer.RailCarCodes.GetDescriptionFromCode(USContainerCodeList.Codes.RR)));
				AssertNoMessageError(container.CO_ContainerNumberInfo, string.Format(CusContainerValidation.InvalidUseOfRailCarNumber, USContainerCodeList.Codes.RR, CusContainer.RailCarCodes.GetDescriptionFromCode(USContainerCodeList.Codes.RR)));
				AssertNoMessageError(container.CO_ContainerNumberInfo, string.Format(CusContainerValidation.InvalidRailCarNumber, USContainerCodeList.Codes.RR, CusContainer.RailCarCodes.GetDescriptionFromCode(USContainerCodeList.Codes.RR)));
				container.CO_ContainerNumber = "AAL304838949007";
				AssertHasMessageError(container.CO_ContainerNumberInfo, string.Format(CusContainerValidation.InvalidRailCarNumber, USContainerCodeList.Codes.RR, CusContainer.RailCarCodes.GetDescriptionFromCode(USContainerCodeList.Codes.RR)));
				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				container.CO_ContainerNumber = "RR3048";
				AssertHasMessageError(container.CO_ContainerNumberInfo, string.Format(CusContainerValidation.InvalidUseOfRailCarNumber, USContainerCodeList.Codes.RR, CusContainer.RailCarCodes.GetDescriptionFromCode(USContainerCodeList.Codes.RR)));
			}
			finally
			{
				CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, autoAllocate);
			}
		}

		JobDeclaration declaration;
		CusContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			container = declaration.CusContainers.AddNew();
		}
	}
}
