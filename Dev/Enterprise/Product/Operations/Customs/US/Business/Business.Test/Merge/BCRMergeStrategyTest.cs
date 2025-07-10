using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BCRMergeStrategyTest : CargoReleaseMergeStrategyTest
	{
		public void TestLinesWithSupTariffDoNotGetMergedWithLinesWithoutSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.JE_MergeBy = "TRF";
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8407.90.9060";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8407.90.9060";
			invoiceLine2.US_SupTariff = "9802.00.5030";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(3, declaration.ActiveEntryHeaders.CargoReleaseEntry.MergedLines.Count);
		}

		public void TestMergeWhenMessageTypeChangesFromIMPToEXP()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader impEntry = declaration.ActiveEntryHeaders[0];
			AssertEquals("PreCondition", CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease, impEntry.CH_MessageType);

			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices[0].RunPreSaveValidation();
			AssertNoErrors("PreCondition", declaration);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Import entry is deleted while merging", true, impEntry.IsDeleted);

			AssertEquals("EXP entry should have been created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("EXP entry should have been created", CusEntryHeaderMessageTypeList.Codes.Export, declaration.ActiveEntryHeaders[0].CH_MessageType);
		}

		protected override void SetupDataForMerging()
		{
			helper.UpdateOrAddCustomsRegNo(helper.IntermediateConsignee, "45-1234487", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
			base.SetupDataForMerging();
			declaration.US_ITDate = ZDateTime.Empty;
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
		}

		protected override Common.US.ImportMessageStatusList.MessageType CargoReleaseEntryType => Common.US.ImportMessageStatusList.MessageType.BorderCargoRelease;

		protected override CargoReleaseMergeStrategy GetMergeStrategy(JobDeclaration declaration) => new BCRMergeStrategy(declaration);
	}
}
