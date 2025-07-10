using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public override void TestValidatePackage()
		{
			JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			CusEntryHeader header = declaration.CustomsEntryHeaders.AddNew();
			header.Validation.ValidatePackagesCount();
			AssertHasMessageErrors(header.PackagesCountInfo);
		}

		public void TestCheckCH_BGMReference()
		{
			var dec1 = (JobDeclaration)GetNewDeclaration();
			dec1.JE_DeclarationReference = "Dec1";
			var entry1 = dec1.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "0001";
			var dec2 = (JobDeclaration)GetNewDeclaration();
			dec2.JE_DeclarationReference = "Dec2";
			var entry2 = dec2.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "0002";
			AssertNoMessageErrorContaining(entry2.CH_BGMReferenceInfo, "is already in use");
			entry2.CH_BGMReference = "0001";
			AssertHasMessageErrorContaining(entry2.CH_BGMReferenceInfo, "is already in use");
			AssertHasMessageError(entry2.CH_BGMReferenceInfo, "LRN 0001 is already in use on Dec1");
			entry2.CH_BGMReference = "0002";
			AssertNoMessageErrorContaining(entry2.CH_BGMReferenceInfo, "is already in use");
		}
		public void TestCheckCH_BondValidToDate()
		{
			var registryValue = ZACustomsRegistry.Instance.CPCAcquitByDate.Value;
			registryValue.Quantity = 6;
			registryValue.Unit = "Month(s)";

			using (ZACustomsRegistry.Instance.CPCAcquitByDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ClusterKey = 2583639;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				CombineAssertions(() =>
				{
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 3, 21);
					AssertEquals("CH_BondValidToDate NOT changed", new ZDate(2022, 9, 21),
						entryHeader.CH_BondValidToDate);
					AssertHasWarning(entryHeader.CH_BondValidToDateInfo,
						@"The Date you have entered is not the same as the default date that is set in Registry
Acquit By Date is managed and and maintained in the following Registry Setting: Registry > Customs > South Africa > CPC Acquit By Date");
				});
			}
		}

		public void TestCheckCH_PaymentMethod()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.IsActive = true;
			var line = entryHeader.MergedLines.AddNew();
			line.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100);
			line.Fees.SetAmount("12A", 100);
			Factory.Save();
			AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			entryHeader.CH_PaymentMethod = "C";
			AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			entryHeader.CH_PaymentMethod = "D";
			AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			entryHeader.CH_PaymentMethod = "V";
			AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			entryHeader.CH_PaymentMethod = "F";
			AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			entryHeader.CH_PaymentMethod = "X";
			AssertHasMessageErrorContaining(entryHeader.CH_PaymentMethodInfo, "The code you have selected is not in the list.");
			entryHeader.CH_PaymentMethod = "";
			AssertHasMessageErrorContaining(entryHeader.CH_PaymentMethodInfo, "You have not entered a value.");
			AssertNoMessageError("FAN setup for JHB", entryHeader.CH_PaymentMethodInfo, ValidationConstants.EntryHeader.FinancialAccountNumberNotSetup(declaration.JE_CustomsOffice));
			declaration.AgentCode = "ZZZ";
			entryHeader.Validation.ValidateCH_PaymentMethod();
			AssertHasMessageError("FAN not setup for JHB", entryHeader.CH_PaymentMethodInfo, ValidationConstants.EntryHeader.FinancialAccountNumberNotSetup(declaration.JE_CustomsOffice));
			new LineMerger(declaration).DoMerge();
			entryHeader = declaration.ActiveEntryHeaders[0];
			CombineAssertions(() =>
			{
				AssertEquals("Default to F", PaymentMethodCodeList.Codes.Free, entryHeader.CH_PaymentMethod);
				AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			});
		}

		public void TestCheckCH_PaymentMethod_WhenEPPGreaterThanZero()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.IsActive = true;
			entryHeader.ProvisionalPaymentAmountAfter = 2m;
			entryHeader.PenaltyAmountAfter = 1m;
			Factory.Save();

			foreach (var messageType in new[] { ZAJobMessageTypeList.Codes.Export, ZAJobMessageTypeList.Codes.ImportByExternalBroker, ZAJobMessageTypeList.Codes.Miscellaneous })
			{
				declaration.JE_MessageType = messageType;
				CombineAssertionsForCH_PaymentMethod_WhenEPPGreaterThanZero(declaration, entryHeader, expectError: false);
			}

			foreach (var messageType in new[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.ExBond })
			{
				declaration.JE_MessageType = messageType;
				CombineAssertionsForCH_PaymentMethod_WhenEPPGreaterThanZero(declaration, entryHeader, expectError: true);
			}
		}

		void CombineAssertionsForCH_PaymentMethod_WhenEPPGreaterThanZero(JobDeclaration declaration, CusEntryHeader entryHeader, ZBool expectError)
		{
			string errorMessage = "Payment Code: should be C - CASH when ePP Amount Due to Customs is greater than zero";
			CombineAssertions(() =>
			{
				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				AssertNoMessageErrorContaining(entryHeader.CH_PaymentMethodInfo, errorMessage);

				var paymentMethods = new[] { PaymentMethodCodeList.Codes.Defer, PaymentMethodCodeList.Codes.VATOnly, PaymentMethodCodeList.Codes.Free };
				foreach (var paymentMethod in paymentMethods)
				{
					entryHeader.CH_PaymentMethod = paymentMethod;
					if (expectError)
					{
						AssertHasMessageErrorContaining(entryHeader.CH_PaymentMethodInfo, errorMessage);
					}
					else
					{
						AssertNoMessageErrorContaining(entryHeader.CH_PaymentMethodInfo, errorMessage);
					}
				}
			});
		}

		public void TestCheckCH_PaymentMethod_OverrideCustomsOffice()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			testHelper.CreateCustomsOfficeCusCodeEntry("CNT");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "CNT";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CustomsOfficeOverride = "CNT";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.IsActive = true;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var line = entryHeader.MergedLines.AddNew();
			line.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100);
			line.Fees.SetAmount("12A", 100);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoNotifications("UZ_PaymentMethod = F", entryHeader.CH_PaymentMethodInfo);
				entryHeader.CH_PaymentMethod = "C";
				AssertNoNotifications("UZ_PaymentMethod = C", entryHeader.CH_PaymentMethodInfo);
				entryHeader.CH_PaymentMethod = "D";
				AssertNoNotifications("UZ_PaymentMethod = D", entryHeader.CH_PaymentMethodInfo);
				entryHeader.CH_PaymentMethod = "V";
				AssertNoNotifications("UZ_PaymentMethod = V", entryHeader.CH_PaymentMethodInfo);
				entryHeader.CH_PaymentMethod = "F";
				AssertNoNotifications("UZ_PaymentMethod = F", entryHeader.CH_PaymentMethodInfo);
				entryHeader.CH_PaymentMethod = "X";
				AssertHasMessageErrorContaining("InvalidCode error", entryHeader.CH_PaymentMethodInfo, "The code you have selected is not in the list.");
				entryHeader.CH_PaymentMethod = "";
				AssertHasMessageErrorContaining("Empty error", entryHeader.CH_PaymentMethodInfo, "You have not entered a value.");
				AssertNoMessageError("FAN setup for CNT and AgentCode=ASBSD", entryHeader.CH_PaymentMethodInfo, ValidationConstants.EntryHeader.FinancialAccountNumberNotSetup(instruction.CEI_CustomsOfficeOverride));

				declaration.AgentCode = "ZZZ";
				entryHeader.Validation.ValidateCH_PaymentMethod();
				AssertHasMessageError("FAN not setup for CNT and AgentCode=ASBSD", entryHeader.CH_PaymentMethodInfo, ValidationConstants.EntryHeader.FinancialAccountNumberNotSetup(instruction.CEI_CustomsOfficeOverride));

				new LineMerger(declaration).DoMerge();
				entryHeader = declaration.ActiveEntryHeaders[0];
				AssertEquals("Default to F", PaymentMethodCodeList.Codes.Free, entryHeader.CH_PaymentMethod);
				AssertNoNotifications("No notifications", entryHeader.CH_PaymentMethodInfo);
			});
		}

		public void TestCheckCH_Packages()
		{
			var declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			entry.CH_Packages = -1;
			AssertHasMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeNegative);
			entry.CH_Packages = 0;
			AssertHasMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			entry.CH_Packages = 1;
			AssertNoMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			entry.CH_Packages = -1;
			AssertHasMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeNegative);
			entry.CH_Packages = 0;
			AssertHasMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			entry.CH_Packages = 1;
			AssertNoMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			entry.CH_Packages = -1;
			AssertNoMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeNegative);
			entry.CH_Packages = 0;
			AssertNoMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			entry.CH_Packages = 1;
			AssertNoMessageErrorContaining(entry.CH_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckCH_EntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			entry.CH_TotalEntries = 1;
			entry.CH_EntryNumber = -1;
			AssertHasMessageErrorContaining(entry.CH_EntryNumberInfo, MandatoryValidation.ValueCannotBeNegative);
			entry.CH_EntryNumber = 0;
			AssertHasMessageErrorContaining(entry.CH_EntryNumberInfo, MandatoryValidation.ValueCannotBeZero);
			entry.CH_EntryNumber = 1;
			AssertNoMessageErrorContaining(entry.CH_EntryNumberInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(entry.CH_EntryNumberInfo, "Entry Number should not be greater than Total Entries.");
			entry.CH_EntryNumber = 2;
			AssertHasMessageErrorContaining(entry.CH_EntryNumberInfo, "Entry Number should not be greater than Total Entries.");
		}

		public void TestCheckCH_TotalEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_TotalEntries = -1;
			AssertHasMessageErrorContaining(entry.CH_TotalEntriesInfo, MandatoryValidation.ValueCannotBeNegative);
			entry.CH_TotalEntries = 0;
			AssertHasMessageErrorContaining(entry.CH_TotalEntriesInfo, MandatoryValidation.ValueCannotBeZero);
			entry.CH_TotalEntries = 1;
			AssertNoMessageErrorContaining(entry.CH_TotalEntriesInfo, MandatoryValidation.ValueCannotBeZero);
			CusEntryHeader entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_TotalEntries = 1;
			AssertNoMessageErrorContaining(entry1.CH_TotalEntriesInfo, "Total Entries value is not equal to other entries.");
			entry1.CH_TotalEntries = 2;
			AssertHasMessageErrorContaining(entry1.CH_TotalEntriesInfo, "Total Entries value is not equal to other entries.");
		}

		public override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
