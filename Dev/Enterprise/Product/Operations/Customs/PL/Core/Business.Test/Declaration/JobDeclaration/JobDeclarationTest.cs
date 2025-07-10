using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
{
	public void TestGetNewValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>("Export Declaration", declaration.Validation);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationValidation>("Import Declaration", declaration.Validation);

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryJobDeclarationValidation>("Exit Summary Declaration", declaration.Validation);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>("Miscellaneous Declaration", declaration.Validation);
		});
	}

	public void TestGetNewLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>("Export Declaration", declaration.Lookups);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<JobDeclarationLookups>("Import Declaration", declaration.Lookups);

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExportJobDeclarationLookups>("Exit Summary Declaration", declaration.Lookups);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<ExportJobDeclarationLookups>("Miscellaneous Declaration", declaration.Lookups);
		});
	}

	public void TestAllAdditionalInfos()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.AdditionalInfos.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.AdditionalInfos.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.AdditionalInfos.AddNew();
		AssertEquals(3, declaration.AllAdditionalInfos.Count());
	}

	public void TestClearAdditionalProcedureCodesOnceNotImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.AdditionalProcedureCodes.AddNew();
		invoiceLine.AdditionalProcedureCodes.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Pre-condition", 2, invoiceLine.AdditionalProcedureCodes.Count);
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Cleared", 0, invoiceLine.AdditionalProcedureCodes.Count);
		});
	}

	public override void TestCustomsOfficeOfEntry()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		jobDeclaration.CustomsOffices.RemoveAndDeleteAll();
		jobDeclaration.JE_CustomsOffice = "LV001000";
		jobDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
		AssertEquals("LV002000", jobDeclaration.OfficeOfEntry);
	}

	public void TestDefaultValuesForBox_b14()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var localCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, jobDeclaration.Country.Code);
		var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, localCountry.Code));

		var foreignCompany = Factory.New<OrgHeader>();
		foreignCompany.OH_Code = "Sid";
		foreignCompany.OH_RL_NKClosestPort = "AUSYD";
		var foreignCompanyAddress = foreignCompany.Addresses.AddNewMainAddress();
		foreignCompanyAddress.OA_Code = "A";

		var localCompany = Factory.New<OrgHeader>();
		localCompany.OH_Code = "Loc";
		localCompany.OH_RL_NKClosestPort = localPort.Code;
		var localCompanyAddress = localCompany.Addresses.AddNewMainAddress();
		localCompanyAddress.OA_Code = "B";

		CombineAssertions(() =>
		{
			AssertEquals("Import, No EORI codes, repr still at default value", ZString.Empty, jobDeclaration.JE_DeclarantType);
			jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
			AssertEquals("Export, No EORI codes, repr still at default value", PLRepresentationTypeList.Codes._4Direct, jobDeclaration.JE_DeclarantType);

			localCompany.CustomsCodes.RemoveAndDeleteAll();
			foreignCompany.CustomsCodes.RemoveAndDeleteAll();
			jobDeclaration.JE_DeclarantType = "YYY";
			jobDeclaration.JE_OH_Importer = Guid.Empty;
			jobDeclaration.JE_OH_Supplier = Guid.Empty;
			jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = Guid.Empty;
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = Guid.Empty;
			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertEquals("No orgs at all, should not explode", "YYY", jobDeclaration.JE_DeclarantType);
		});
	}

	JobDeclarationForTesting GetJobDeclarationForTesting()
	{
		var dec = Factory.New<JobDeclarationForTesting>();
		dec.DisableDefaultPackingInformation = true;
		dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		return dec;
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Poland, GetJobDeclarationForTesting().LocalCurrencyCode);
	}

	public override void TestAreMultipleEntryInstructionsAllowed() => AssertEquals(true, Factory.New<JobDeclaration>().AreMultipleEntryInstructionsAllowed);

		public void TestEntryCreationStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportEntryCreationStrategy>("Should Be Enterprise.Customs.PL.Business.Declaration.EntryCreationStrategy", declaration.CreateEntryCreationStrategy());

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertType<ExportEntryCreationStrategy>("Should Be Enterprise.Customs.PL.Business.Declaration.ExportEntryCreationStrategy", declaration.CreateEntryCreationStrategy());
	}
	public void TestCustomsOffices() => AssertType<PlOfficeCodeCollection>(Factory.New<JobDeclaration>().CustomsOffices);

	public void TestCustomsOfficesForBinding() => AssertType<PlOfficeCodeCollectionForBinding>(Factory.New<JobDeclaration>().CustomsOfficesForBinding);

	public override void TestCustomsOfficeRequirementHelper() => AssertType<JobDeclarationCustomsOfficeRequirementHelper>(Factory.New<JobDeclaration>().CustomsOfficeRequirementHelper);

	public override void TestCustomsOfficeOfExit()
	{
		JobDeclaration decEcs = Factory.New<JobDeclaration>();
		decEcs.JE_MessageType = EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented;
		AssertEquals("", decEcs.OfficeOfExit);
		AssertEquals("", decEcs.JE_OfficeOfEntryExit);
		decEcs.JE_MessageType = EuOfficeCodesTypes.Codes.OfficeOfExport;
		AssertEquals("", decEcs.OfficeOfExit);
		AssertEquals("", decEcs.JE_OfficeOfEntryExit);
		Factory.Save();
		decEcs.JE_OfficeOfEntryExit = "PL335020";
		AssertEquals("PL335020", decEcs.JE_OfficeOfEntryExit);

		decEcs.JE_MessageType = "1";
		decEcs.JE_ApplicationCode = "EMC";
		AssertEquals(true, decEcs.IsEMCS);
	}

	public void TestIsLocationOfGoodsFromOfficeList()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		declaration.JE_LocationQualifier = string.Empty;
		AssertEquals(false, declaration.IsLocationOfGoodsFromOfficeList);

		declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.OTH;
		AssertEquals(false, declaration.IsLocationOfGoodsFromOfficeList);

		declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
		AssertEquals(false, declaration.IsLocationOfGoodsFromOfficeList);

		declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.CUS;
		AssertEquals(true, declaration.IsLocationOfGoodsFromOfficeList);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
		AssertEquals(true, declaration.IsLocationOfGoodsFromOfficeList);
	}

	public void TestIsLocationOfGoodsFreeText()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		declaration.JE_LocationQualifier = string.Empty;
		AssertEquals(false, declaration.IsLocationOfGoodsFreeText);

		declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.CUS;
		AssertEquals(false, declaration.IsLocationOfGoodsFreeText);

		declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.OTH;
		AssertEquals(true, declaration.IsLocationOfGoodsFreeText);

		declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
		AssertEquals(true, declaration.IsLocationOfGoodsFreeText);
	}

	public void TestGetExitControlHeader() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertNull("There is no ExitControlHeader", declaration.GetExitControlHeader());

		var exitControlHeader = Factory.New<CusExitControlHeader>();
		exitControlHeader.CEH_ParentID = declaration.PK;
		AssertEquals("ExitControlHeader created", exitControlHeader, declaration.GetExitControlHeader());
	});

	public override void TestGetCustomsEntryInstructionProviderCore() => AssertType<EntryInstructionProvider>(Factory.New<JobDeclaration>().CustomsEntryInstructionProvider);

	public void TestCustomsEntryInstructions() => AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(Factory.New<JobDeclaration>().CustomsEntryInstructions);

	public void TestSupportingDocuments() => AssertType<SupportingDocumentCollection>(Factory.New<JobDeclaration>().SupportingDocuments);

	public void TestPreviousDocuments() => AssertType<PreviousDocumentCollection>(Factory.New<JobDeclaration>().PreviousDocuments);

	public void TestDefaultValues() => AssertEquals(string.Empty, Factory.New<JobDeclaration>().JE_DeclarantType);

	public override void TestDefaultValuesForExport() => AssertEquals(string.Empty, Factory.New<JobDeclaration>().JE_DeclarantType);

	public void TestJE_UCR_MaxLength() => AssertEquals(35, Factory.New<JobDeclaration>().JE_UCRInfo.MaxLength);

	public void TestGetMergeManager() => AssertType<MergeManager>(Factory.New<JobDeclaration>().MergeManager);

	public void TestIsExportGoodsLocatedAtOfficeOfExit()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", false, declaration.IsExportGoodsLocatedAtOfficeOfExit);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
			declaration.JE_LocationOfGoods = "ABC";
			declaration.JE_OfficeOfEntryExit = "CBA";
			AssertEquals("Different JE_LocationOfGoods and JE_OfficeOfEntryExit", false, declaration.IsExportGoodsLocatedAtOfficeOfExit);

			declaration.JE_LocationOfGoods = "ABC";
			declaration.JE_OfficeOfEntryExit = "ABC";
			AssertEquals("Same JE_LocationOfGoods and JE_OfficeOfEntryExit without Customs Office Qualifier", false, declaration.IsExportGoodsLocatedAtOfficeOfExit);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
			AssertEquals("All IsExportGoodsLocatedAtOfficeOfExit requirements fulfilled", false, declaration.IsExportGoodsLocatedAtOfficeOfExit);
		});
	}

	public void TestDV1DetailsSupportIsEnabled() => AssertEquals(true, Factory.New<JobDeclaration>().DV1DetailsSupport);

	public override void TestGetCusCodeDataType()
	{
		AssertEquals(typeof(OfficeCode), ((ICusCodeDataTypeSupporter)Factory.New<JobDeclaration>()).GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.OfficeCode]);
	}

	public void TestCusEntryHeaderCollection() => AssertType<EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>>(Factory.New<JobDeclaration>().CustomsEntryHeaders);

	public void TestFilteredInvoiceLines() => AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(Factory.New<JobDeclaration>().FilteredInvoiceLines);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		JobDeclaration result = (JobDeclaration)base.GetNewBusinessObjectForDeleteTest(factory);
		result.CustomsOfficesForBinding.AddNew();
		return result;
	}

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	public void TestIsUNLocodeQualifier()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertEquals("JE_LocationQualifier is not UNLocode Qualifier", ZBool.False, declaration.IsLocationOfGoodsFromUNLocode);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.U;
			AssertEquals("JE_LocationQualifier is UNLocode Qualifier", ZBool.True, declaration.IsLocationOfGoodsFromUNLocode);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
			AssertEquals("JE_LocationQualifier is UNLocode Qualifier", ZBool.True, declaration.IsLocationOfGoodsFromUNLocode);
		});
	}

	public void TestIsAuthorisationNumberQualifier()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertEquals("JE_LocationQualifier is not Number Qualifier", ZBool.False, declaration.IsAuthorisationNumberQualifier);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.Y;
			AssertEquals("JE_LocationQualifier is Number Qualifier", ZBool.True, declaration.IsAuthorisationNumberQualifier);
		});
	}

	public void TestJE_SubLocationOfGoods_MaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(4, declaration.JE_SubLocationOfGoodsInfo.MaxLength);
	}

	public void TestJE_LocationOtherInformation_MaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(1, declaration.JE_LocationOtherInformationInfo.MaxLength);
	}

	public void TestJE_LocationOfGoods_MaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
			AssertEquals("JE_LocationOfGoods - V", 8, declaration.JE_LocationOfGoodsInfo.MaxLength);
			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.U;
			AssertEquals("JE_LocationOfGoods - U", 17, declaration.JE_LocationOfGoodsInfo.MaxLength);
			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.Y;
			AssertEquals("JE_LocationOfGoods - Y", 35, declaration.JE_LocationOfGoodsInfo.MaxLength);
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("JE_LocationOfGoods - Default", 100, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.CUS;
			AssertEquals("JE_LocationOfGoods - CUS", 8, declaration.JE_LocationOfGoodsInfo.MaxLength);
			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
			AssertEquals("JE_LocationOfGoods - GLC", 35, declaration.JE_LocationOfGoodsInfo.MaxLength);
			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.OTH;
			AssertEquals("JE_LocationOfGoods - OTH", 70, declaration.JE_LocationOfGoodsInfo.MaxLength);
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("JE_LocationOfGoods - Default", 100, declaration.JE_LocationOfGoodsInfo.MaxLength);
		});
	}

	public void TestGoodsLocationCustomsOffice_MaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(8, declaration.GoodsLocationCustomsOfficeInfo.MaxLength);
	}

	public void TestEntryInstructionSubStyle()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			var entryInstructionFirst = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Empty EntryInstructionSubStyle", ZString.Empty, declaration.EntryInstructionSubStyle);

			entryInstructionFirst.CEI_SubStyle = "A";
			AssertEquals("One A", "A", declaration.EntryInstructionSubStyle);

			var entryInstructionSecond = declaration.CustomsEntryInstructions.AddNew();
			entryInstructionSecond.CEI_SubStyle = "B";
			AssertEquals("A and B", "A,B", declaration.EntryInstructionSubStyle);

			var entryInstructionThird = declaration.CustomsEntryInstructions.AddNew();
			entryInstructionThird.CEI_SubStyle = "B";
			AssertEquals("A, B and B", "A,B", declaration.EntryInstructionSubStyle);

			entryInstructionThird.CEI_SubStyle = "A";
			AssertEquals("A, B, A", "A,B", declaration.EntryInstructionSubStyle);

			entryInstructionFirst.CEI_SubStyle = "C";
			AssertEquals("C, B, A", "A,B,C", declaration.EntryInstructionSubStyle);
		});
	}

	public void TestEntryInstructionSubStyleCaption() => AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.EntryInstructionSubStyle), false, attribute => attribute.Caption == "Entry Sub-style");

	public void TestGoodsLocationCustomsOffice()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Empty Declaration", ZString.Empty, declaration.GoodsLocationCustomsOffice);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
			declaration.GoodsLocationCustomsOffice = "asd";
			AssertEquals("Office qualifier", "asd", declaration.GoodsLocationCustomsOffice);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.Y;
			declaration.GoodsLocationCustomsOffice = "poi";
			AssertEquals("Invalid GoodsLocationCustomsOffice qualifier", ZString.Empty, declaration.GoodsLocationCustomsOffice);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Empty Declaration", ZString.Empty, declaration.GoodsLocationCustomsOffice);

			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.CUS;
			declaration.GoodsLocationCustomsOffice = "asd";
			AssertEquals("Office qualifier", "asd", declaration.GoodsLocationCustomsOffice);

			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
			declaration.GoodsLocationCustomsOffice = "poi";
			AssertEquals("Invalid GoodsLocationCustomsOffice qualifier", ZString.Empty, declaration.GoodsLocationCustomsOffice);
		});
	}

	public void TestGoodsLocationUNLocode_MaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(17, declaration.GoodsLocationUNLocodeInfo.MaxLength);
	}

	public void TestGoodsLocationUNLocode()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			AssertEquals("Empty Declaration", ZString.Empty, declaration.GoodsLocationUNLocode);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.U;
			declaration.GoodsLocationUNLocode = "asd";
			AssertEquals("UNLocode qualifier", "asd", declaration.GoodsLocationUNLocode);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
			declaration.GoodsLocationUNLocode = "qwe";
			AssertEquals("GNSS qualifier", "qwe", declaration.GoodsLocationUNLocode);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.Y;
			declaration.GoodsLocationUNLocode = "poi";
			AssertEquals("Invalid GoodsLocationUNLocode qualifier", ZString.Empty, declaration.GoodsLocationUNLocode);
		});
	}

	public void TestJE_MessageTypeChanged()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		invoice.ZG_AgreedPlaceCode = "abc";
		invoice2.ZG_AgreedPlaceCode = "cde";

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export invoice1", "abc", invoice.ZG_AgreedPlaceCode);
			AssertEquals("Export invoice2", "cde", invoice2.ZG_AgreedPlaceCode);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Not changed message type invoice1", "abc", invoice.ZG_AgreedPlaceCode);
			AssertEquals("Not changed message type invoice2", "cde", invoice2.ZG_AgreedPlaceCode);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Not changed message type invoice1", ZString.Empty, invoice.ZG_AgreedPlaceCode);
			AssertEquals("Not changed message type invoice2", ZString.Empty, invoice2.ZG_AgreedPlaceCode);
		});
	}

	public void TestZG_AgreedPlaceCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(String.Empty, DataBoundResourceStrings.GetDataForProperty(declaration.ZG_AgreedPlaceCodeInfo).FullDescription);
	}

	public void TestZG_AgreedPlaceCodeValidationSupport()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(false, declaration.ZG_AgreedPlaceCodeValidationSupport);
	}

	public void TestAgreedPlaceCodeSupport()
	{
		var declaration = Factory.New<JobDeclarationForTesting>();
		AssertEquals(false, declaration.AgreedPlaceCodeSupportCore_Exposed);
	}

	protected override Hashtable ExpectedDocAddressTypes
	{
		get
		{
			var result = base.ExpectedDocAddressTypes;
			result[DocAddressTypes.Codes.GoodsLocation] = DocAddressType.GoodsLocation;
			return result;
		}
	}

	public void TestGoodsLocationQualifierDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", " / ", declaration.GoodsLocationQualifierDescription);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.Y;
			AssertEquals("Empty JE_LocationOtherInformation & Not Empty JE_LocationQualifier"
				, $" / {QualifierOfTheIdentificationList.Descriptions.Y}"
				, declaration.GoodsLocationQualifierDescription);

			declaration.JE_LocationOtherInformation = TypeOfLocationList.Codes.B;
			AssertEquals("Not Empty JE_LocationOtherInformation & Not Empty JE_LocationQualifier"
				, $"{TypeOfLocationList.Descriptions.B} / {QualifierOfTheIdentificationList.Descriptions.Y}"
				, declaration.GoodsLocationQualifierDescription);
		});
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		const string testDataFirst = "PL1";
		const string testDataSecond = "PL2";
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertEquals("Empty Value", string.Empty, declaration.CustomsOfficeOfPresentationReferenceNumber());

			var customsOfficeFirst = declaration.CustomsOfficesForBinding.AddNew();
			customsOfficeFirst.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOfficeFirst.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			customsOfficeFirst.CY_Data = testDataFirst;
			AssertEquals("Matched with CY_Data", testDataFirst, declaration.CustomsOfficeOfPresentationReferenceNumber());

			customsOfficeFirst.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			AssertEquals("Return empty if not matched", string.Empty, declaration.CustomsOfficeOfPresentationReferenceNumber());

			var customsOfficeSecond = declaration.CustomsOfficesForBinding.AddNew();
			customsOfficeSecond.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOfficeSecond.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			customsOfficeSecond.CY_Data = testDataSecond;
			AssertEquals("Matched with CY_Data", testDataSecond, declaration.CustomsOfficeOfPresentationReferenceNumber());

			customsOfficeSecond.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			AssertEquals("Return empty if not matched", string.Empty, declaration.CustomsOfficeOfPresentationReferenceNumber());
		});
	}

	public void TestHasOfficeOfPresentationPL()
	{
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", false, declaration.HasOfficeOfPresentationPL);

			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			AssertEquals("Empty OfficeOfPresentation", false, declaration.HasOfficeOfPresentationPL);

			office.CY_Data = CountryCodes.Poland;
			AssertEquals("Office is PL", true, declaration.HasOfficeOfPresentationPL);

			office.CY_Data = "PL01";
			AssertEquals("office is PL01", true, declaration.HasOfficeOfPresentationPL);

			office.CY_Data = "DE01";
			AssertEquals("office is DE01", false, declaration.HasOfficeOfPresentationPL);
		});
	}

	public void TestPLCustomsVATTypeCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationForTesting = Factory.New<JobDeclarationForTesting>();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declarationForTesting.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("JobDeclaration Import PLCustomsVATTypeCaption", "PTU", declaration.CustomsVATTypeCaption);
			AssertEquals("JobDeclarationForTesting Import PLCustomsVATTypeCaption", "PTUForTest", declarationForTesting.CustomsVATTypeCaption);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declarationForTesting.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("JobDeclaration Export PLCustomsVATTypeCaption", "PTU", declaration.CustomsVATTypeCaption);
			AssertEquals("JobDeclarationForTesting Export PLCustomsVATTypeCaption", "PTU", declarationForTesting.CustomsVATTypeCaption);
		});
	}

	public void TestPiggyBackedDocAddressValidation()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDocAddressValidation>("Export", declaration.PiggyBackedDocAddressValidation(declaration.GoodsLocationAddress));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportJobDocAddressValidation>("Import", declaration.PiggyBackedDocAddressValidation(declaration.GoodsLocationAddress));

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryJobDocAddressValidation>("ExitSummary", declaration.PiggyBackedDocAddressValidation(declaration.GoodsLocationAddress));

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertNull("Miscellaneous", declaration.PiggyBackedDocAddressValidation(declaration.GoodsLocationAddress));
		});
	}

	public void TestJE_PaymentMethod()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(1, declaration.JE_PaymentMethodInfo.MaxLength);
	}

	public void TestJE_LloydsIMO_ResourceStringDataAttribute()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_LloydsIMO), false, attribute => attribute.Caption == "IMO No." && attribute.FullDescription == "Lloyds / IMO Number");
	}

	public void TestJE_VesselName_ResourceStringDataAttribute()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_VesselName), false, attribute => attribute.Caption == "Vessel" && attribute.FullDescription == "Vessel Name");
	}

	public void TestUseDeclarationContainersIfNoneFoundOnEntry()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(false, declaration.UseDeclarationContainersIfNoneFoundOnEntry);
	}

	public void TestUseDeclarationContainersForSingleContainerWhenMultipleEntriesExist()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(false, declaration.UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist);
	}

	protected override ZDecimal TestAutoRatingExpectedAmount => 85M;

	protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

	public void TestAttachmentMessages() => AssertType<EDIMessageCollection>(Factory.New<JobDeclaration>().AttachmentMessages);

	public void TestGetDiscardedMessagesFilter()
	{
		var declaration = Factory.New<JobDeclarationForTesting>();
		var expectedQuerry = new ZQuery(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Discarded);

		AssertEquals(expectedQuerry, declaration.GetDiscardedMessagesFilter_Exposed());
	}

	public void TestAttachmentMessageFilter()
	{
		var declaration = Factory.New<JobDeclarationForTesting>();
		var expectedQuerry = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, declaration.PK);
		expectedQuerry.AddToFilter(EDIMessageSchema.EM_MessageType, EdiMessageMessageType.Attachment);

		AssertEquals(expectedQuerry, declaration.AttachmentMessageFilter_Exposed());
	}

	public void TestPackingGroups() => AssertType<DeclarationLevelPackingGroupCollection>(Factory.New<JobDeclaration>().PackingGroups);

	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForImport => EUCommonConstants.TransportModeSource.InlandTransportMode;
	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForExport => EUCommonConstants.TransportModeSource.InlandTransportMode;
	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForMiscellaneousCustoms => EUCommonConstants.TransportModeSource.InlandTransportMode;

	public void TestVesselInland()
	{
		var vessel = RefVessel.New(Factory);
		vessel.RV_Code = "abc123";
		vessel.RV_LloydsNumber = "654321";

		var vessel2 = RefVessel.New(Factory);
		vessel2.RV_Code = "321cba";
		vessel2.RV_LloydsNumber = "123456";

		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertNull("Empty JE_TransportIDInland", declaration.VesselInland);

			declaration.JE_TransportIDInland = "asd";
			AssertNull("Vessel does not exist", declaration.VesselInland);

			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			AssertNull("Vessel Name transport means JE_TransportIDInland does not exist", declaration.VesselInland);

			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._10;
			AssertNull("LloydNumber transport means JE_TransportIDInland does not exist", declaration.VesselInland);

			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			declaration.JE_TransportIDInland = "abc123";
			AssertEquals("Vessel Name transport means JE_TransportIDInland exist - RV_Code", "abc123", declaration.VesselInland.RV_Code);
			AssertEquals("Vessel Name transport means JE_TransportIDInland exist - LloydNumber", "654321", declaration.VesselInland.RV_LloydsNumber);

			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._10;
			declaration.JE_TransportIDInland = "123456";
			AssertEquals("LloydNumber transport means JE_TransportIDInland exist - RV_Code", "321cba", declaration.VesselInland.RV_Code);
			AssertEquals("LloydNumber transport means JE_TransportIDInland exist - LloydNumber", "123456", declaration.VesselInland.RV_LloydsNumber);
		});
	}

	public void TestUpdateJE_RN_NKTransportNationalityInland_OnSeaVesselChange()
	{
		var vessel = RefVessel.New(Factory);
		vessel.RV_Code = "abc123";
		vessel.RV_LloydsNumber = "123456";
		vessel.RV_RN_NKCountryOfReg = CountryCodes.Germany;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RN_NKTransportNationalityInland = CountryCodes.Tuvalu;

		CombineAssertions(() =>
		{
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			declaration.JE_TransportIDInland = "abc123";
			AssertEquals("JE_TransportIDInland is vessel but not a sea inland transport mode", CountryCodes.Tuvalu, declaration.JE_RN_NKTransportNationalityInland);

			declaration.JE_TransportModeInland = TransportModes.Sea;
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._10;
			declaration.JE_TransportIDInland = "123456";
			AssertEquals("JE_TransportModeInland is sea with lloyd number", CountryCodes.Germany, declaration.JE_RN_NKTransportNationalityInland);

			declaration.JE_RN_NKTransportNationalityInland = CountryCodes.Tuvalu;
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			declaration.JE_TransportIDInland = "abc123";
			AssertEquals("JE_TransportModeInland is sea with vessel name", CountryCodes.Germany, declaration.JE_RN_NKTransportNationalityInland);
		});
	}

	public void TestZG_PresentationStartDate_Caption() => AssertEquals("Presentation Start Date", new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>()).ZG_PresentationStartDateInfo.HumanReadableName);

	public void TestIsExitSummary()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("JE_MessageType is ExitSummaryDeclaration", true, declaration.IsExitSummary);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageType is Export", false, declaration.IsExitSummary);
		});
	}

	sealed class PLJobDeclarationLightValidationTester : LightValidationTester
	{
		public PLJobDeclarationLightValidationTester(BusinessObject bo) : base(bo) { }

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			var ignoreJobDocAddressNumberProperties = new[] {
				JobDocAddress.Schema.E2_AddressOverride,
				JobDocAddress.Schema.E2_OA_Address,
				JobDocAddress.Schema.E2_ParentID,
				JobDocAddress.Schema.E2_ParentTableCode,
				JobDocAddress.Schema.E2_AddressType
			};
			return base.ShouldTestProperty(info)
					&& !ignoreJobDocAddressNumberProperties.Contains(info.Name);
		}
	}

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
	{
		return new PLJobDeclarationLightValidationTester(bizObjToTest);
	}
}

sealed class JobDeclarationForTesting : JobDeclaration
{
	public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override string PLCustomsVATTypeCaption => "PTUForTest";

	internal ZQuery GetDiscardedMessagesFilter_Exposed() => GetDiscardedMessagesFilter();

	internal ZQuery AttachmentMessageFilter_Exposed() => AttachmentMessageFilter;

	public bool AgreedPlaceCodeSupportCore_Exposed => base.AgreedPlaceCodeSupportCore;
}
