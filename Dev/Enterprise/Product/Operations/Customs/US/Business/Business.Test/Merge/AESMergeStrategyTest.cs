using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESMergeStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
	{
		public void TestMergeWhenMessageTypeChangesFromEXPToIMP()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader expEntry = declaration.ActiveEntryHeaders[0];
			AssertNotNull(expEntry);
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.Export, expEntry.CH_MessageType);

			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices[0].RunPreSaveValidation();
			AssertNoErrors("PreCondition", declaration);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("EXP entry is deleted while merging", true, expEntry.IsDeleted);

			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull("ENS entry should have been created", ensEntry);

			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Same ENS Entry", ensEntry, declaration.ActiveEntryHeaders.EntrySummaryEntry);

			AssertNoErrors(declaration);
		}

		public void TestLinesWithSameDescriptionsMerge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "AIR";

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			line1.JI_Description = "Beer";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			line2.JI_Description = "Beer";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestUS_OH_IntermConsigneeIsMergeKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "AIR";

			var orgHeader = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OH_Consignee = orgHeader.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Consignee = orgHeader2.PK;

			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
		}

		public void TestLinesWithDifferentDescriptions()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "AIR";

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			line1.JI_Description = "Beer";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			line2.JI_Description = "Beer 2";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestLinesWithDifferentMarksAndNumbers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "AIR";

			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			line1.US_MarksAndNumbers = "1";
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			line2.US_MarksAndNumbers = "2";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestHeadersWithDifferentUltimateDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var header01 = declaration.Invoices.AddNew();
			var header02 = declaration.Invoices.AddNew();
			var line01 = header01.InvoiceLines.AddNew();
			var line02 = header02.InvoiceLines.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			orgHeader.OH_RL_NKClosestPort = "USTES";

			header01.JZ_OH_Buyer = orgHeader.PK;
			header02.JZ_OH_Buyer = orgHeader.PK;
			header02.US_UltimateDestinationCountry = "XX";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
		}

		public void TestHeaderWithDifferentAESOriginIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var header01 = declaration.Invoices.AddNew();
			var header02 = declaration.Invoices.AddNew();
			var line01 = header01.InvoiceLines.AddNew();
			var line02 = header02.InvoiceLines.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			orgHeader.OH_RL_NKClosestPort = "USTES";

			header01.JZ_OH_Buyer = orgHeader.PK;
			header02.JZ_OH_Buyer = orgHeader.PK;
			header01.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			header02.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
		}

		public void TestHeaderWithDifferentSupplierPickup()
		{
			CheckHeaderWithDifferentDocAddress("usPPI");
		}

		public void TestHeaderWithDifferentUSPPIDocAddress()
		{
			CheckHeaderWithDifferentDocAddress("pickup");
		}

		public void TestHeaderWithDifferentUltimateConsigneeDocAddress()
		{
			CheckHeaderWithDifferentDocAddress("ultimateConsignee");
		}

		public void TestHeaderWithSupplierPickupOverridden()
		{
			CheckHeaderWitDocAddressOverridden(true);
		}

		public void TestHeaderWithUltimateConsigneeDocAddressOverridden()
		{
			CheckHeaderWitDocAddressOverridden(false);
		}

		public override void TestGetKeyForHeader()
		{
			var mergeKey = strategy.GetKeyForHeader(invoiceLine);
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_OA_Address));
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_Address1));
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_Address2));
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_City));
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_State));
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_Postcode));
			Assert(mergeKey.Contains(invoice.SupplierPickupAddress.E2_RN_NKCountryCode));
			Assert(mergeKey.Contains(invoice.USPPIDocAddress.E2_OA_Address));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_OA_Address));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_Address1));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_Address2));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_City));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_State));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_Postcode));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_RN_NKCountryCode));
			Assert(mergeKey.Contains(invoice.UltimateConsigneeDocAddress.E2_CompanyName));
			Assert(mergeKey.Contains(invoice.US_TransactionsRelated));
			Assert(mergeKey.Contains(invoice.US_HazardousCargo));
			Assert(mergeKey.Contains(invoice.US_RoutedTransaction));
			Assert(mergeKey.Contains(invoice.US_ImportEntryNo));
			Assert(mergeKey.Contains(invoice.US_InbondType));
			Assert(mergeKey.Contains(invoiceLine.US_DateOfExport.Date));
			Assert(mergeKey.Contains(invoice.US_StateOfOrigin));
			Assert(mergeKey.Contains(invoice.US_ForeignTradeZone));
			Assert(mergeKey.Contains(invoice.US_UltimateConsigneeType));
			Assert(mergeKey.Contains(invoice.US_UltimateDestinationCountry));
		}

		public void TestGetKeyForLine()
		{
			var mergeKey = strategy.GetKeyForLine(invoiceLine);
			Assert(mergeKey.Contains(invoiceLine.US_ExportCode));
			Assert(mergeKey.Contains(invoiceLine.US_LicenseType));
			Assert(mergeKey.Contains(invoiceLine.US_LicenseNo));
			Assert(mergeKey.Contains(invoiceLine.JI_Tariff));
			Assert(mergeKey.Contains(invoiceLine.US_AESOriginIndicator));
			Assert(mergeKey.Contains(invoiceLine.US_ECCN));
			Assert(mergeKey.Contains(invoiceLine.US_DDTCITARExemptionNo));
			Assert(mergeKey.Contains(invoiceLine.US_DDTCMilitaryEquipmentIndicator));
			Assert(mergeKey.Contains(invoiceLine.US_DDTCPartyCertificationIndicator));
			Assert(mergeKey.Contains(invoiceLine.US_DDTCRegistrationNo));
			Assert(mergeKey.Contains(invoiceLine.US_DDTCUnit));
			Assert(mergeKey.Contains(invoiceLine.US_DDTCUSMLCategoryCode));
			Assert(mergeKey.Contains(invoiceLine.US_AMSInd));
			Assert(mergeKey.Contains(invoiceLine.US_PSTIndicator));
			Assert(mergeKey.Contains(invoiceLine.US_NMFSHMSInd));
			Assert(mergeKey.Contains(invoiceLine.US_ATFInd));
			Assert(mergeKey.Contains(invoiceLine.US_DEAInd));
			Assert(mergeKey.Contains(invoiceLine.US_FWSInd));
			Assert(mergeKey.Contains(invoiceLine.US_TTBInd));

			invoiceLine.US_IsUsedVehicle = false;
			mergeKey = strategy.GetKeyForLine(invoiceLine);
			Assert(!mergeKey.Contains(invoiceLine.PK));

			invoiceLine.US_IsUsedVehicle = true;
			mergeKey = strategy.GetKeyForLine(invoiceLine);
			Assert(mergeKey.Contains(invoiceLine.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = new DeclarationTestHelper(Factory).CreateSeaExportDeclaration();
			strategy = new AESMergeStrategy(declaration);
			invoice = declaration.Invoices[0];
			invoiceLine = invoice.JobComInvoiceLines[0];
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		AESMergeStrategy strategy;

		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var header1 = declaration.Invoices.AddNew();
			header1.InvoiceLines.AddNew();
			var header2 = declaration.Invoices.AddNew();
			header2.InvoiceLines.AddNew();
			return declaration;
		}

		void CheckHeaderWithDifferentDocAddress(string type)
		{
			var declaration = GetDeclaration();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = org.Addresses.AddNew();
			var orgAddress2 = org.Addresses.AddNew();

			var docAddress1 = type == "usPPI" ? declaration.Invoices[0].USPPIDocAddress : type == "ultimateConsignee" ? declaration.Invoices[0].UltimateConsigneeDocAddress : declaration.Invoices[0].SupplierPickupAddress;
			var docAddress2 = type == "usPPI" ? declaration.Invoices[1].USPPIDocAddress : type == "ultimateConsignee" ? declaration.Invoices[1].UltimateConsigneeDocAddress : declaration.Invoices[1].SupplierPickupAddress;

			docAddress1.E2_OA_Address = orgAddress1.PK;
			docAddress2.E2_OA_Address = orgAddress2.PK;
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_OA_Address = orgAddress1.PK;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
		}

		void CheckHeaderWitDocAddressOverridden(bool isPickup)
		{
			var declaration = GetDeclaration();
			var docAddress1 = isPickup ? declaration.Invoices[0].SupplierPickupAddress : declaration.Invoices[0].UltimateConsigneeDocAddress;
			var docAddress2 = isPickup ? declaration.Invoices[1].SupplierPickupAddress : declaration.Invoices[1].UltimateConsigneeDocAddress;
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_AddressOverride = true;

			docAddress1.E2_Address1 = "123 Merry St";
			docAddress2.E2_Address1 = "456 Merry St";
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_Address1 = "123 Merry St";
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			docAddress1.E2_Address2 = "Unit 1";
			docAddress2.E2_Address2 = "Unit 2";
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_Address2 = "Unit 1";
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			docAddress1.E2_City = "CLEVELAND";
			docAddress2.E2_City = "SEATTLE";
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_City = "CLEVELAND";
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			docAddress1.E2_State = "TN";
			docAddress2.E2_State = "SD";
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_State = "TN";
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			docAddress1.E2_RN_NKCountryCode = "US";
			docAddress2.E2_RN_NKCountryCode = "CA";
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_RN_NKCountryCode = "US";
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			docAddress1.E2_Postcode = "37300";
			docAddress2.E2_Postcode = "47312";
			declaration.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			docAddress2.E2_Postcode = "37300";
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
		}
	}
}
