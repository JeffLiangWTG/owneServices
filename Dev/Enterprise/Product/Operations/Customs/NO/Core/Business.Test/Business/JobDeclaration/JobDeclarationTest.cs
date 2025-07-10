using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
{
	public override void TestIsDeclarationWithEntryInstruction()
	{
		AssertEquals(expected: true, declaration.CustomsEntryInstructions.IsLoaded);
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		AssertEquals(expected: false, declaration.AreMultipleEntryInstructionsAllowed);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Norway, declaration.LocalCurrencyCode);
	}

	protected override void CreatePartAndClassification(string partNum, string tariff, OrgHeader importer)
	{
		var part = Factory.New<OrgSupplierPart>();
		part.OP_PartNum = partNum;
		part.RelatedOrganisations.AddOwner(importer);
		var classification = Factory.New<BaseCusClassification>();
		classification.CC_ClassificationType = Common.ClassificationType.Both;
		classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		classification.CC_TariffNum = tariff;
		var pivot = Factory.New<CusClassPartPivot>();
		pivot.CI_CC = classification.PK;
		pivot.CI_OP = part.PK;
	}

	public void TestHouseBillsCollectionIsOfRightType()
	{
		AssertType<BillCollection<Bill, JobDeclaration>>(declaration.Bills);
	}

	public void TestLookups_Cached()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationLookups>("Export lookups should not be cached", declaration.Lookups);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<JobDeclarationLookups>("Other lookups should not be cached", declaration.Lookups);
	}

	public void TestMergeManager()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<MergeManager>(declaration.MergeManager);
	}

	public void TestEntryCreationStrategy()
	{
		AssertType<EntryCreationStrategy>(declaration.CreateEntryCreationStrategy());
	}

	public void TestValidationOfGoodsnumberAndPosition()
	{
		var declaration = Factory.New<JobDeclarationForTest>();
		declaration.JE_GoodsNumber = "111";
		declaration.JE_Position = "222";
		AssertEquals(declaration.GNOCusEntryNumberForTest.EntryNumber, "111;222");
	}

	public void TestValidationOfGoodsnumberAndPositionEmptyStrings()
	{
		var declaration = Factory.New<JobDeclarationForTest>();
		declaration.JE_GoodsNumber = ZString.Empty;
		declaration.JE_Position = ZString.Empty;
		Assert(!declaration.GNOCusEntryNumberForTest.ExistsCusEntryNumber);
	}

	public void TestJE_OA_DeclarantAddress_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_OA_DeclarantAddressInfo);
		AssertEquals("Declarant", resourceStringData.Caption);
	}

	public void TestJE_EntryStatusDescription_Caption()
	{
		_ = AssertEntity<JobDeclaration>()
				.HasProperty(h => h.JE_EntryStatusDescription)
				.WithCaption("Entry Status");
	}

	public void TestCopyStatus_Attributes() => CombineAssertions(() =>
		AssertEntity<JobDeclaration>()
			.HasProperty(x => x.JE_CopyStatus)
			.WithMaxLength(3, because: "CopyStatus is dependent on CEI_SubStyle"));

	public void TestCopyStatus_ShouldReferenceCEISubStyle() => CombineAssertions(() =>
	{
		var instruction1 = declaration.CustomsEntryInstructions.AddNew();
		instruction1.CEI_SubStyle = NODeclarationCopyStatus.Codes.FinalImport;
		AssertEquals("Get CopyStatus when CEI_SubStyle is 'FIN'", "FIN", declaration.JE_CopyStatus);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
		AssertEquals("Get CEI_SubStyle after setting CopyStatus = 'REX' (single instruction)", "REX", instruction1.CEI_SubStyle);

		instruction1.Delete();
		var instruction2 = declaration.CustomsEntryInstructions.AddNew();
		AssertNoExceptionThrown("When setting CopyStatus while one instruction is inactive", () =>
		{
			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
			AssertEquals("Get CEI_SubStyle after setting CopyStatus = 'REC' (active instruction)", "REC", instruction2.CEI_SubStyle);
		});
	});

	public void TestCopyStatus_ShouldCreateInstructionIfNeeded() => CombineAssertions(() =>
	{
		AssertEquals("instruction count before", 0, declaration.CustomsEntryInstructions.Count);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
		AssertEquals("instruction count after", 1, declaration.CustomsEntryInstructions.Count);
	});

	public void TestMessageStatus_Caption()
	{
		_ = AssertEntity<JobDeclaration>()
				.HasProperty(h => h.MessageStatus)
				.WithCaption("Message Status");
	}

	public void TestMessageStatus()
	{
		AssertEquals("Precondition: No status yet", ZString.Empty, declaration.MessageStatus);

		var header0 = declaration.ActiveEntryHeaders.AddNew();
		header0.CH_Status = "ACC";

		CombineAssertions(() =>
		{
			AssertEquals("Acknowledged Change", declaration.MessageStatus);

			var header1 = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("Multiple - See Entries", declaration.MessageStatus);

			header1.CH_Status = ZString.Empty;
			AssertEquals("Multiple - See Entries", declaration.MessageStatus);

			header1.CH_Status = "ACC";
			AssertEquals("Acknowledged Change", declaration.MessageStatus);

			header1.CH_Status = "AWR";
			AssertEquals("Multiple - See Entries", declaration.MessageStatus);
		});
	}

	public void TestGoodsOriginDefaultsToFirstEnteredPortOfOrigin()
	{
		CombineAssertions(() =>
		{
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Denmark;
			AssertEquals("GoodsOrigin should default to origin port", Core.Constants.CountryCodes.Denmark, declaration.JE_GoodsOrigin);

			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Norway;
			AssertEquals("If origin port is changed, GoodsOrigin should also change", Core.Constants.CountryCodes.Norway, declaration.JE_GoodsOrigin);
		});
	}

	public void TestGoodsDestinationDefaultsToFirstEnteredPortOfOrigin()
	{
		CombineAssertions(() =>
		{
			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Denmark;
			AssertEquals("GoodsDestination should default to destination port", Core.Constants.CountryCodes.Denmark, declaration.JE_GoodsDestination);

			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Norway;
			AssertEquals("If destination port is changed, GoodsDestination should also change", Core.Constants.CountryCodes.Norway, declaration.JE_GoodsDestination);
		});
	}

	public void TestContainerModeDefaultValue()
	{
		AssertEquals("Default value should be containerized", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
	}

	public void TestJE_GS_NKCusAgentDefaultOnCreating()
	{
		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			AssertEquals("Default value should be Current User Initials, if it's not System Account", Env.CurrentUser.Initials, declaration1.JE_GS_NKCusAgent);

			GlbStaff.CurrentUser.GS_IsSystemAccount = true;
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			AssertEquals("Default value should be Empty, if it's System Account", ZString.Empty, declaration2.JE_GS_NKCusAgent);
		});
	}

	public void TestJE_GS_NKCusAgentDefaultWhenEmptyOnSaving()
	{
		GlbStaff.CurrentUser.GS_IsSystemAccount = false;
		GlbStaff.CurrentUser.GS_Code = "XZX";
		Factory.Save();
		AssertEquals("Default value should be Current User Initials, if it's not System Account", "XZX", declaration.JE_GS_NKCusAgent);
	}

	public void TestJE_GS_NKCusAgentDefaultWhenNotEmptyOnSaving()
	{
		declaration.JE_GS_NKCusAgent = "ABC";
		GlbStaff.CurrentUser.GS_IsSystemAccount = false;
		GlbStaff.CurrentUser.GS_Code = "XZX";
		Factory.Save();
		AssertEquals("Value should not be changed to Current User Initials", "ABC", declaration.JE_GS_NKCusAgent);
	}

	public void TestJE_GS_NKCusAgent()
	{
		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_Code = "XZX";
			GlbStaff.CurrentUser.GS_IsSystemAccount = true;
			declaration.JE_GS_NKCusAgent = string.Empty;
			AssertNotEquals("Value should not be Current User Initials, when CurrentUser is a System account and Value is set to Empty", "XZX", declaration.JE_GS_NKCusAgent);

			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			declaration.JE_GS_NKCusAgent = string.Empty;
			AssertEquals("Value should be Current User Initials, when CurrentUser is Not a System account and Value is set to Empty", "XZX", declaration.JE_GS_NKCusAgent);

			declaration.JE_GS_NKCusAgent = "AAA";
			AssertEquals("JE_GS_NKCusAgent should be set to the assigned value", "AAA", declaration.JE_GS_NKCusAgent);
		});
	}

	public override void TestBrokerName()
	{
		GlbStaff staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
		staffMember1.GS_Code = "JS";
		staffMember1.GS_FullName = "John Smith";
		GlbStaff staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
		staffMember2.GS_Code = "JC";
		staffMember2.GS_FullName = "Jinlee Chow";

		GlbStaff.CurrentUser.GS_IsSystemAccount = false;
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			AssertEquals("Broker Name", Env.CurrentUser.FullName, declaration.BrokerName);

			declaration.JE_GS_NKCusAgent = staffMember1.GS_Code;
			AssertEquals("Broker Name", "John Smith", declaration.BrokerName);

			declaration.JE_GS_NKCusAgent = staffMember2.GS_Code;
			AssertEquals("Broker Name", "Jinlee Chow", declaration.BrokerName);
		});
	}

	public void TestJE_OA_DeclarantAddressDefaultValue()
	{
		GlbBranch.CurrentBranch.OrgProxy.OH_FullName = ZString.Empty;
		GlbBranch.CurrentBranch.OrgProxy.OH_RL_NKClosestPort = ZString.Empty;
		GlbCompany.CurrentCompany.OrgProxy.OH_FullName = "Random Company Org Proxy";
		GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "RND-P";

		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Should be set to Company OrgProxy", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);

			GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "Random Branch Org Proxy";
			GlbBranch.CurrentBranch.OrgProxy.OH_RL_NKClosestPort = "RND-P";
			declaration = Factory.New<JobDeclaration>();
			AssertEquals("Should be set to Branch OrgProxy", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		});
	}

	public void TestCheckAutosuggestDeclarationTypeOnExport()
	{
		SetupTradeGroups();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Precondition: declaration.IsExport", expected: true, declaration.IsExport);

			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_RL_NKFinalDestination = "SESTO";
			AssertEquals("Should be set to 'EU' for EU countries", "EU", declaration.JE_MessageSubType);

			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_RL_NKFinalDestination = "TRIST";
			AssertEquals("Should be set to 'EX' for non-EU countries", "EX", declaration.JE_MessageSubType);
		});
	}

	public void TestCheckAutosuggestDeclarationTypeOnImport()
	{
		SetupTradeGroups();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Precondition: declaration.IsImport", expected: true, declaration.IsImport);

			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_RL_NKOrigin = "SESTO";
			AssertEquals("Should be set to 'EU' for EU countries", "EU", declaration.JE_MessageSubType);

			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_RL_NKOrigin = "TRIST";
			AssertEquals("Should be set to 'IM' for non-EU countries", "IM", declaration.JE_MessageSubType);
		});
	}

	void SetupTradeGroups()
	{
		var startDate = ZDateTime.Now.AddMonths(-1);
		var endDate = ZDateTime.Now.AddMonths(1);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tradeGroupEU = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Norway, "TEF");
		var tradeGroupNonEU = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Norway, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);

		helper.AddCountry(tradeGroupEU, Core.Constants.CountryCodes.Sweden, startDate.Date, endDate.Date);
		helper.AddCountry(tradeGroupNonEU, Core.Constants.CountryCodes.Taiwan, startDate.Date, endDate.Date);
		Factory.Save();
	}

	public void TestJE_TransportMode_ContainerModeDefault()
	{
		AssertEquals("Default should be 'Not Containerized'", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
	}

	public void TestJE_TransportMode_ContainermodeForFix()
	{
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
		declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
		AssertEquals("FixedTransport should be 'Not Containerized'", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
	}

	public void TestJE_CustomsTransportMode()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Default CustomsTransportMode - Sea", "10", declaration.JE_CustomsTransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Default CustomsTransportMode - Road", "30", declaration.JE_CustomsTransportMode);
		});
	}

	public void TestJE_CustomsTransportModeCaptions() => CombineAssertions(() =>
		AssertEntity<JobDeclaration>()
			.HasProperty(x => x.JE_CustomsTransportMode)
			.WithCaption("Customs Transport Mode")
			.WithShortCaption("Cus. Transp. Mode")
			.WithFullDescription("Transport mode at border passing for the EDI to Norwegian customs."));

	public void TestFilteredInvoiceLines()
	{
		AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
	}

	public void TestGetTemplateCopyStrategy()
	{
		var declaration = Factory.New<JobDeclarationForTest>();
		var strategy = declaration.GetTemplateCopyStrategyExposed(declaration.Factory, CloneType.TemplateCopy);
		AssertType<JobDeclarationDeepCloneStrategy>(strategy);
	}

	public void TestJE_TransportMode_ClearsJE_RN_NKTransportNationality()
	{
		CombineAssertions(() =>
		{
			foreach (var transportMode in transportModesNotRequireTransportNationality)
			{
				declaration.JE_RN_NKTransportNationality = "NO";
				declaration.JE_TransportMode = transportMode;
				AssertEquals($"TransportMode: {transportMode}", ZString.Empty, declaration.JE_RN_NKTransportNationality);
			}
		});
	}

	public void TestJE_TransportMode_NotClearsJE_RN_NKTransportNationality()
	{
		CombineAssertions(() =>
		{
			declaration.JE_RN_NKTransportNationality = "NO";
			foreach (var transportMode in transportModesRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				AssertEquals($"TransportMode: {transportMode}", "NO", declaration.JE_RN_NKTransportNationality);
			}
		});
	}

	public void TestTransportModeRequiresTransportNationality_False()
	{
		CombineAssertions(() =>
		{
			foreach (var transportMode in transportModesNotRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				AssertEquals($"TransportMode: {transportMode}", expected: false, declaration.TransportModeRequiresTransportNationality);
			}
		});
	}

	public void TestTransportModeRequiresTransportNationality_True()
	{
		CombineAssertions(() =>
		{
			foreach (var transportMode in transportModesRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				AssertEquals($"TransportMode: {transportMode}", expected: true, declaration.TransportModeRequiresTransportNationality);
			}
		});
	}

	public void TestGetCustomsEntryHeaders()
	{
		var jobDeclaration = base.Factory.New<JobDeclaration>();
		AssertType<CusEntryHeaderCollection<CusEntryHeader>>(jobDeclaration.CustomsEntryHeaders);
	}

	public void TestGetCustomsEntryInstructionProvider()
	{
		var jobDeclaration = base.Factory.New<JobDeclaration>();
		AssertType<EntryInstructionProvider>(jobDeclaration.CustomsEntryInstructionProvider);
	}

	public override void TestSetDefaultPackagesType()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Default value for JE_TotalNoOfPacksPackType", ZString.Empty, declaration.JE_TotalNoOfPacksPackType);
	}

	public void TestOrganizationDeclarant_ShouldSetAddressToMainOfficeAddress_WhenDeclarantIsUpdated()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		var office1 = declarant.Addresses.AddNew(OrgAddressType.Office, isDefault: true);
		var office2 = declarant.Addresses.AddNew(OrgAddressType.Office, isDefault: false);
		office2.Address1 = "office2 has no default value for Address1";
		Factory.Save();
		CombineAssertions(() =>
		{
			declaration.SetDeclarant(declarant);
			AssertEquals("(main-office1) DeclarantAddress should be office1", office1.PK, declaration.JE_OA_DeclarantAddress);

			declaration.ClearDeclarant();
			AssertEquals("(cleared) DeclarantAddress should be empty", ZGuid.Empty, declaration.JE_OA_DeclarantAddress);

			declarant.Addresses.SwapMainAddress(office1, office2);
			var office3 = declarant.MainAddress; // SwapMainAddress creates a new OrgAddress instead of reusing office2
			declaration.SetDeclarant(declarant);
			AssertEquals("(main-office3) DeclarantAddress should be office3", office3.PK, declaration.JE_OA_DeclarantAddress);
		});
	}

	public override void TestDisableResultApportionmentWithRealInvoice2()
	{
		using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3581.21m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			var invFIFT = invoice.Charges.AddNew();
			PrepareCharge(invFIFT);
			invFIFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			invFIFT.J7_Amount = 55.85m;
			invFIFT.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			invFIFT.J7_IsIncludedInITOT = false;

			var invDED = invoice.Charges.AddNew();
			PrepareCharge(invDED);
			invDED.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			invDED.J7_Percentage = 0.75m;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 360;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 2880m;

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 312m;
			var line3DED = line3.Charges.AddNew();
			PrepareCharge(line3DED);
			line3DED.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			line3DED.J7_Amount = 10.34m;

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals("Line1 FIFT", 5.66m, line1.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount);
				AssertEquals("Line2 FIFT", 45.28m, line2.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount);
				AssertEquals("Line3 FIFT", 4.91m, line3.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount);

				AssertEquals("Line1 Apportioned DIS", 2.70m, line1.ApportionedCharges.GetCharge(invDED.ChargeKey).Amount);
				AssertEquals("Line2 Apportioned DIS", 21.60m, line2.ApportionedCharges.GetCharge(invDED.ChargeKey).Amount);
				AssertEquals("Line3 Apportioned DIS", 2.34m, line3.ApportionedCharges.GetCharge(invDED.ChargeKey).Amount);

				AssertEquals("Invoice DIS", 26.64m, invDED.J7_Amount);
			});
		}
	}

	public void TestHasDigitollGoodsNumber() => CombineAssertions(() =>
	{
		declaration.JE_GoodsNumber = ZString.Empty;
		AssertEquals("When GoodsNumber is empty", expected: false, declaration.HasDigitollGoodsNumber);

		declaration.JE_GoodsNumber = "202501DT";
		AssertEquals("When GoodsNumber char 7-8 is 'DT' and 9-15 is empty", expected: true, declaration.HasDigitollGoodsNumber);
		declaration.JE_GoodsNumber = "202501DT1";
		AssertEquals("When GoodsNumber char 7-8 is 'DT' and 9-15 is NOT empty", expected: false, declaration.HasDigitollGoodsNumber);

		declaration.JE_GoodsNumber = "202412D";
		AssertEquals("When GoodsNumber char 7 is 'D' and 8-15 is empty", expected: true, declaration.HasDigitollGoodsNumber);
		declaration.JE_GoodsNumber = "202412D1";
		AssertEquals("When GoodsNumber char 7 is 'D' and 8-15 is NOT empty", expected: false, declaration.HasDigitollGoodsNumber);

		declaration.JE_GoodsNumber = "202401321022001";
		AssertEquals("When GoodsNumber is not a Digitoll goods number", expected: false, declaration.HasDigitollGoodsNumber);
	});

	public void TestHasImporterWithDeferredCustomsPaymentAccount()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without an importer", expected: false, declaration.HasImporterWithDeferredCustomsPaymentAccount);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("With importer that does not have a deferred customs payment account", expected: false, declaration.HasImporterWithDeferredCustomsPaymentAccount);

			importer.AsDeferredDutiesAccount();
			AssertEquals("With importer that have a deferred customs payment account", expected: true, declaration.HasImporterWithDeferredCustomsPaymentAccount);
		});
	}

	public void TestHasImporterWithMVARegistration()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without an importer", expected: false, declaration.HasImporterWithMVARegistration);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("With importer that is not MVA-registered", expected: false, declaration.HasImporterWithMVARegistration);

			importer.AsMVARegistered();
			AssertEquals("With importer that is MVA-registered", expected: true, declaration.HasImporterWithMVARegistration);
		});
	}

	public void TestHasImporterWithSocialSecurityNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without an importer", expected: false, declaration.HasImporterWithSocialSecurityNumber);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_Category = "NAT";
			AssertEquals("With importer that has no SSN", expected: false, declaration.HasImporterWithSocialSecurityNumber);

			var customsCode = importer.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
			customsCode.OK_CodeType = "SSN";
			customsCode.OK_CustomsRegNo = "08052621187";

			AssertEquals("With importer that has SSN", expected: true, declaration.HasImporterWithSocialSecurityNumber);
		});
	}

	public void TestHasImporterWithOrganizationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without an importer", expected: false, declaration.HasImporterWithOrganizationNumber);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_Category = "BUS";
			AssertEquals("With importer that has no OrganizationNumber", expected: false, declaration.HasImporterWithOrganizationNumber);

			var customsCode = importer.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
			customsCode.OK_CodeType = "ORG";
			customsCode.OK_CustomsRegNo = "123456789";

			AssertEquals("With importer that has OrganizationNumber", expected: true, declaration.HasImporterWithOrganizationNumber);
		});
	}

	public void TestHasRelatedRecalculations()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "5040";
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("PreReq: No related recalculations", expected: false, declaration.HasRelatedRecalculations);

			var clonedDeclaration = CopyHelper.CreateRecalculationCopyOf(declaration);
			CopyHelper.LinkDeclarationCopyToParent(declaration, clonedDeclaration.Copy);
			AssertEquals("Should have related recalculation", expected: true, declaration.HasRelatedRecalculations);
		});
	}

	public void TestValidation_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationValidation>(declaration.Validation);
	}

	public void TestValidation_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationValidation>(declaration.Validation);
	}

	public void TestMergeByDefaultValue()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Default value should be based on the Registry Setting", Env.Registry.CommercialInvoiceLineMergeMethod, declaration.JE_MergeBy);
	}

	public void TestGetFetchStrategyCoreInstance()
	{
		AssertType<JobDeclarationFetchStrategy>(declaration.FetchStrategy);
	}

	public void TestGoodsOriginName() => CombineAssertions(() =>
	{
		AssertEquals("GoodsOriginName when JE_GoodsOrigin is empty", ZString.Empty, declaration.GoodsOriginName);

		declaration.JE_GoodsOrigin = "DE";
		AssertEquals("GoodsOriginName when JE_GoodsOrigin is DE", "Germany", declaration.GoodsOriginName);
	});

	public void TestDeclarant() => CombineAssertions(() =>
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNull("When JE_OA_DeclarantAddress is not specified (empty)", declaration.Declarant);

		var declarant = Factory.New<OrgHeader>();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		AssertSame("When JE_OA_DeclarantAddress is specified", declarant, declaration.Declarant);
	});

	public void TestCustomsOfficeName() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals("CustomsOfficeName when JE_CustomsOffice is empty", ZString.Empty, declaration.CustomsOfficeName);

		declaration.JE_CustomsOffice = "3775";
		AssertEquals("CustomsOfficeName when JE_CustomsOffice is 3775", "Halden", declaration.CustomsOfficeName);
	});

	public void TestLocationOfGoodsDescription() => CombineAssertions(() =>
	{
		AssertEquals("LocationOfGoodsDescription when JE_LocationOfGoods is empty", ZString.Empty, declaration.LocationOfGoodsDescription);

		declaration.JE_LocationOfGoods = "A";
		AssertEquals("LocationOfGoodsDescription when JE_LocationOfGoods is A", "Customs Warehouse A", declaration.LocationOfGoodsDescription);
	});

	string GetMergeType() => Env.Registry.CommercialInvoiceLineMergeMethod;

	protected override string DefaultMergeType => GetMergeType();

	protected override string MergeType1 => GetMergeType();

	protected override string MergeType2 => GetMergeType();

	public override void TestPhaseStatusDescription_Caption()
	{
		Assertion.AssertEquals("Caption", "Phase Status", base.Factory.New<BaseJobDeclaration>().PhaseStatusInfo.Description);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = (JobDeclaration)GetNewBusinessObject();
	}
	JobDeclaration declaration;

	HashSet<string> transportModesRequireTransportNationality => new HashSet<string> { TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Road, TransportTypeList.Codes.Air, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.Sea };

	HashSet<string> transportModesNotRequireTransportNationality => new HashSet<string> { TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Rail };
}

class JobDeclarationForTest : JobDeclaration
{
	public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategyExposed(BusinessObjectFactory alternateFactory, CloneType cloneType) => base.GetTemplateCopyStrategy(alternateFactory, cloneType);

	public CusEntryNumberWrapper GNOCusEntryNumberForTest => GoodsNumberAndPosition;
}

static class JobDeclarationHelper
{
	public static void ClearDeclarant(this JobDeclaration je) => je.SetDeclarant(null);
	public static void SetDeclarant(this JobDeclaration je, IOrgHeader orgHeader)
	{
		je.JE_OA_DeclarantAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgHeader?.PK ?? ZGuid.Empty);
		var address = je.JE_OA_DeclarantAddress_ZAddress.GetDefaultAddress(orgHeader);
		je.JE_OA_DeclarantAddress = address;
	}

	public static void SetupGetHasDigitollGoodsNumber(this Mock<JobDeclaration> mock, bool hasDigitollGoodsNumber)
	{
		mock.Protected().Setup<ZBool>("GetHasDigitollGoodsNumber").Returns(hasDigitollGoodsNumber);
		mock.Object.Factory.InvalidateCachedProperties();
	}
}

