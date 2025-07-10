using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.ZA;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(ZACustomsRegistry))]
	sealed class ZACustomsRegistryTest : RegistryItemSetTestCaseWithFactory<ZACustomsRegistry>
	{
		public void TestWarehouseOperatorTransactionsModuleEnabled()
		{
			TestRegistryItem((BooleanRegistryItem)ItemSet.WarehouseOperatorTransactionsModuleEnabled,
				expectedName: "WarehouseOperatorTransactionsModuleEnabled",
				expectedCategory: ZACustomsRegistry.Categories.Customs_SouthAfrica,
				expectedCaption: "Warehouse Operator Transactions Module enabled",
				expectedHint: "Override the Default value to enable the Warehouse Operator Transactions Module.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: false);
		}

		public void TestDefaultBranchFroManifestSubmission()
		{
			TestGenericRegistryItem(ItemSet.DefaultBranchForManifestSubmission,
				"DefaultBranchForManifestSubmission",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Default Branch for Manifest Submission",
				"Insert the ZA Branch Code that has a ZA Customs Manifest EDI Profile.\r\nSetting this registry item will allow ZA Manifest Messages to be submitted to ZA Customs from a Non ZA Company using the ZA Customs Manifest EDI Profile of the selected branch.",
				RegistryStorageFlags.Company);

			var company1 = GlbCompany.CurrentCompany;

			AssertEquals(2, ItemSet.DefaultBranchForManifestSubmission.CountryFilterPKs.Count());
			AssertCollectionNotContains(company1.PK, ItemSet.DefaultBranchForManifestSubmission.CountryFilterPKs);
			AssertEquals(0, ZACustomsRegistry.ZaBranchCodesForRegistry(Factory).Count);

			var proxy1 = Factory.New<OrgHeader>();
			proxy1.OH_Code = "Proxy1";
			proxy1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "JNB";
			branch1.GB_BranchName = "Joburg Corporate Office";
			branch1.GB_OH_OrgProxy = proxy1.PK;

			var proxy2 = Factory.New<OrgHeader>();
			proxy2.OH_Code = "Proxy2";
			proxy2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "67890", Core.Constants.CountryCodes.SouthAfrica);
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company1.PK;
			branch2.GB_Code = "CPT";
			branch2.GB_BranchName = "Capetown operations";
			branch2.GB_OH_OrgProxy = proxy2.PK;
			Factory.Save();

			var list = ZACustomsRegistry.ZaBranchCodesForRegistry(Factory);
			AssertEquals(2, list.Count);
			AssertEquals("JNB, CPT", list.CodesAsString);
			AssertEquals("Joburg Corporate Office(12345, EDI)", list.GetDescriptionFromCode("JNB"));
			AssertEquals("Capetown operations(67890, EDI)", list.GetDescriptionFromCode("CPT"));
		}

		public void TestZAOutturnGateInOutJobNumberCustomization()
		{
			TestGenericRegistryItem(
				ItemSet.ZAOutturnGateInOutJobNumberCustomization,
				"ZAOutturnGateInOutJobNumberCustomization",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Outturn & Gate In/Out Job number Customization",
				"Override this value to customize how Outturn & Gate In/Out Job Numbers are formatted",
				RegistryStorageFlags.All);

			var dataType = (BillCustomisationRegistryDataType)ItemSet.ZAOutturnGateInOutJobNumberCustomization.DataType;
			AssertEquals("GeneratedNumberName", "Outturn & Gate In/Out Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, dataType.MaxLength);
			AssertEquals("7", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode]);
		}

		public void TestFinancialAccountNumberPortMaps()
		{
			TestGenericRegistryItem(ItemSet.FinancialAccountNumberPortMaps,
				"ZAFinancialAccountNumberPortMaps",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Financial Account Number Mapping",
				"Enter the organizations that will have EDI profiles that will be used for submitting entries to Customs. Financial Account Numbers (FANs) must be entered for all organizations, Creditor Codes must be entered for all Customs offices where the account is to be paid by the agent. If the account is to be paid by the importer then the Importer Pays box must be checked and the Creditor Code cannot be entered.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestCPCAcquitByDate()
		{
			TestGenericRegistryItem(ItemSet.CPCAcquitByDate,
				"ZACPCAcquitByDate",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"CPC Acquit By Date",
				@"This Registry setting is used to define the default Acquit By Date for all types of ZA Brokerage Jobs 

This date will output to the Acquit by Date: field on the Entries > Messages > Entry Details sub tab of ZA Brokerage jobs

The Acquit By Date: field remains editable in the Brokerage job so that default dates can be overwritten when necessary. Eg Extension of deadlines granted to bonded goods in Customs Warehouses",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestCusCarHabSendPciMarks()
		{
			TestGenericRegistryItem(ItemSet.CusCarHabSendPciMarks,
				"CusCarHabSendPciMarks",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Send package marks in PCI segment of HAB CUSCAR?",
				"Send marks, or suppress them as per specs? Dev only.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestCustomsOfficeCode()
		{
			TestRegistryItem(ItemSet.CustomsOfficeCode,
				"ZACustomsOfficeCode",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Customs Office Code",
				"The default Customs Office Code for a branch",
				RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				RegistryFindBoxCollection.ZACustomsOffice,
				Guid.Empty);

			AssertType<ZACustomsOfficeDataType>(ZString.Format("Customs Office Code should not be longer than {0}.", JobDeclaration.Schema.JE_CustomsOfficeMaxLength), ZACustomsRegistry.Instance.CustomsOfficeCode.Inner.DataType);
		}

		public void TestFallbackNotificationGroup()
		{
			TestRegistryItem(ItemSet.FallbackNotificationGroup,
							 "ZAFallbackNotificationGroup",
							 ZACustomsRegistry.Categories.Customs_SouthAfrica,
							 "Fallback Notification Group",
							 "The group of users who will be receiving notifications about South Africa Responses.",
							RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
							 RegistryFindBoxCollection.GlbGroup, RegistryConstants.GroupPKs.Notification);
		}

		public void TestMessageSendingInterval()
		{
			TestRegistryItem(ItemSet.MessageSendingInterval,
							 "ZAMessageSendingInterval",
							 ZACustomsRegistry.Categories.Customs_SouthAfrica,
							 "Message Sending Interval",
							 @"The minimum time (in Minutes) between sending two messages for a single Entry.
If '0' is specified, then there will be no check between message sending.",
							 RegistryStorageFlags.Company, 5);
		}

		public void TestAllowAutomaticDeferredSelection()
		{
			TestGenericRegistryItem(ItemSet.AutomaticDeferredSelection,
				"ZAAutomaticDeferredSelection",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Automatic Deferred Selection",
				"Automatic Deferred Selection",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestFANList()
		{
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			orgheader.CompanyData.OB_IsCreditor = true;
			orgheader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AST11", Core.Constants.CountryCodes.SouthAfrica);
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("ABC");
			testHelper.CreateCustomsOfficeCusCodeEntry("CDE", "Office CDE");
			testHelper.CreateCustomsOfficeCusCodeEntry("XYZ", "Office XYZ");
			testHelper.CreateCustomsOfficeCusCodeEntry("ZZZ", "Office ZZZ");
			Factory.Save();

			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(0, ItemSet.FinancialAccountNumberPortMaps.Value.Count);

			var fansForTesting = new string[][]
			{
				new string[] { "0000000001", "ABC", "ABC"        },
				new string[] { "0000000002", "CDE", "Office CDE" },
				new string[] { "0000000003", "XYZ", "Office XYZ" },
				new string[] { "0000000004", "ZZZ", "Office ZZZ" }
			};

			for (int mappingIndex = 0; mappingIndex < fansForTesting.Length; mappingIndex++)
			{
				var mapping = maps.AddNew();
				mapping.AccountStartDay = 1;
				mapping.ImporterPays = false;
				mapping.OrganizationPK = orgheader.PK;
				mapping.CreditorPK = orgheader.PK;
				mapping.CustomsOfficeCode = fansForTesting[mappingIndex][1];
				mapping.FinancialAccountNumber = fansForTesting[mappingIndex][0];
			}

			using (ItemSet.FinancialAccountNumberPortMaps.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps))
			{
				AssertEquals(fansForTesting.Length, ItemSet.FinancialAccountNumberPortMaps.Value.Count);

				var results = ((IZACustomsRegistry)ItemSet).FANList.Cast<CodeDescriptionPair>().ToArray();
				foreach (var fanCodeArray in fansForTesting)
				{
					var codeDescPair = results.FirstOrDefault(x => x.Code == fanCodeArray[0]);
					AssertNotNull("Expected FAN " + fanCodeArray[0], codeDescPair);
					AssertEquals("Office Name", fanCodeArray[2], codeDescPair.Description);
				}
				AssertEquals(4, results.Length);
			}
		}

		public void TestGetFANsForOrgs()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CP1";
			company1.GC_RN_NKCountryCode = "ZA";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "BR1";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CP2";
			company2.GC_RN_NKCountryCode = "AU";
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "BR2";

			Factory.Save();

			using (Environment.DisposableEnvironment.ForCompany(company1.GC_Code))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "ORG1";
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "ORG2";
				org2.CompanyData.OB_IsCreditor = true;
				org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AST11", Core.Constants.CountryCodes.SouthAfrica);
				new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false).CreateCustomsOfficeCusCodeEntry("ABC");
				Factory.Save();

				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				var mapping = maps.AddNew();
				mapping.AccountStartDay = 1;
				mapping.ImporterPays = false;
				mapping.OrganizationPK = org2.PK;
				mapping.CreditorPK = org2.PK;
				mapping.CustomsOfficeCode = "ABC";
				mapping.FinancialAccountNumber = "0000000001";

				using (ItemSet.FinancialAccountNumberPortMaps.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps))
				{
					var portFan = ItemSet.FinancialAccountNumberPortMaps.Value.GetFinancialAccountNumberFor(org2.PK, "ABC");
					AssertEquals("precondition: FAN exists", "0000000001", portFan);

					var orgArray = new ZGuid[] { org1.PK, org2.PK };
					var orgFansCo1 = ((IZACustomsRegistry)ItemSet).GetFANsForOrgs(orgArray);
					AssertEquals("0000000001", orgFansCo1.First().FAN);

					using (Environment.DisposableEnvironment.ForCompany(company2.GC_Code))
					{
						portFan = ItemSet.FinancialAccountNumberPortMaps.Value.GetFinancialAccountNumberFor(org2.PK, "ABC");
						AssertEquals("precondition: FAN not visible in other country/company context", "", portFan);

						var orgFansCo2 = ((IZACustomsRegistry)ItemSet).GetFANsForOrgs(orgArray);
						AssertEquals("0000000001", orgFansCo2.First().FAN);
					}
				}
			}
		}

		public void TestZADutyFreeGoodsIntoBondedWarehouse()
		{
			TestGenericRegistryItem(ItemSet.DutyFreeGoodsIntoBondedWarehouse,
				"ZADutyFreeGoodsIntoBondedWarehouse",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Duty Free Goods into Bonded Warehouse",
				"The default is that a Warning will be displayed if a Duty Free Tariff is captured on an invoice line when doing an 'Into Warehouse' entry on a Brokerage Job.\nSelect the Override to change the Warning to an Error message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestSADDocumentWatermarks()
		{
			TestGenericRegistryItem(ItemSet.SADDocumentPackWatermarks,
				"ZASADDocumentWatermarks",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"SAD Document Pack Watermark",
				"Activate this Registry Setting to print a Watermark on the SAD500 Document for the status codes selected in the Status Code Grid.\r\nIf the Text Box is blank, the Status Code Description will be used as the Watermark text",
				RegistryStorageFlags.Branch);
		}

		public void TestAllowAutomaticSplitEntriesByBondAmount()
		{
			TestRegistryItem(ItemSet.AllowAutomaticSplitEntriesByBondAmount,
				"ZAAllowAutomaticSplitEntriesByBondAmount",
				ZACustomsRegistry.Categories.Customs_SouthAfrica_AutomaticSplitEntriesByBondAmount,
				"Allow automatic split of entries by bond amount",
				"On merging this will enable the automatic splitting of entries based on the Bond Holder's bond amount.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestPercentageOfBondAmountUsedBeforeSplit()
		{
			TestRegistryItem(ItemSet.PercentageOfBondAmountUsedBeforeSplit,
				"ZAPercentageOfBondAmountUsedBeforeSplit",
				ZACustomsRegistry.Categories.Customs_SouthAfrica_AutomaticSplitEntriesByBondAmount,
				"% of bond amount to be used",
				"This value will determine what % of the bond amount will be used before we split an entry into a new one.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				90,
				50,
				100);
		}

		public void TestExbondMaxNumberEntryLines()
		{
			TestRegistryItem(ItemSet.ExbondMaxNumberEntryLines,
				expectedName: "ExbondMaxNumberEntryLines",
				expectedCategory: ZACustomsRegistry.Categories.Customs_SouthAfrica,
				expectedCaption: "Ex-bond Max Number of Entry Lines",
				expectedHint: "Override the Default to set the 'Max Entry Lines' allowed per 'Ex-Bond Entry/Entry Instruction'. When the value is '0', then the current maximum of 9 999 lines will apply.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: 0,
				expectedMinValue: 0,
				expectedMaxValue: 9999);
		}

		public void TestExbondMaxNumberJobInvoiceLines()
		{
			TestRegistryItem(ItemSet.ExbondMaxNumberJobInvoiceLines,
				expectedName: "ExbondMaxNumberJobInvoiceLines",
				expectedCategory: ZACustomsRegistry.Categories.Customs_SouthAfrica,
				expectedCaption: "Ex-bond Max Number of Job Invoice Lines",
				expectedHint: "Override the Default to set the 'Max Invoice Lines' allowed per 'Ex-Bond' job. When the value is '0', then no maximum will apply.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: 0,
				expectedMinValue: 0,
				expectedMaxValue: int.MaxValue);
		}

		public void TestMaxNumberOfLinesForHomeConsumptionFile()
		{
			TestRegistryItem(ItemSet.MaxNumberLinesPerHomeConsumptionFile,
				expectedName: "MaxNumberLinesPerHomeConsumptionFile",
				expectedCategory: ZACustomsRegistry.Categories.Customs_SouthAfrica,
				expectedCaption: "Max No of Lines for WOT Home Consumption file",
				expectedHint: "Override the Default to set the 'Max number of Lines' allowed per WOT Home Consumption file. When the value is '0', then no maximum will apply.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: 0,
				expectedMinValue: 0,
				expectedMaxValue: 9999);
		}

		public void TestWOTReceiptsMatchingPeriod()
		{
			TestRegistryItem(ItemSet.WOTReceiptsMatchingPeriod,
				expectedName: "WOTReceiptsMatchingPeriod",
				expectedCategory: ZACustomsRegistry.Categories.Customs_SouthAfrica,
				expectedCaption: "Number of days to look back for matching receipts",
				expectedHint: "Override the Default to set the 'Number of days' to look back for matching receipts. When the value is '0', there will be no restriction.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: 120,
				expectedMinValue: 0,
				expectedMaxValue: 730);
		}

		public void TestAllowReversalOfCusWarehouseBatch()
		{
			TestRegistryItem(ItemSet.AllowReversalOfCusWarehouseBatch,
				"AllowReversalOfCusWarehouseBatch",
				ZACustomsRegistry.Categories.Customs_SouthAfrica,
				"Allow Reversal of Cus Warehouse Batch",
				"Allow reversal of Cus Warehouse Batch due to being added in error.",
				RegistryStorageFlags.Company,
				false);
		}
	}
}
