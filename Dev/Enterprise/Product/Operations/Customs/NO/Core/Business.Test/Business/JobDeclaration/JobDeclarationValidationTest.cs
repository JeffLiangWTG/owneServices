using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobDeclarationValidation))]
sealed class JobDeclarationValidationTest : JobDeclarationValidationAbstractTest<JobDeclarationValidation>
{
	public void TestCheckJE_RN_NKTransportNationality_MandatoryAndListValidation()
	{
		var targetInfo = declaration.JE_RN_NKTransportNationalityInfo;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			foreach (var transportMode in transportModesRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(targetInfo, "XX", "NO");
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			foreach (var transportMode in transportModesRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(targetInfo, "XX", "NO");
			}
		});
	}

	public void TestCheckJE_RN_NKTransportNationality_NotMandatory()
	{
		var targetInfo = declaration.JE_RN_NKTransportNationalityInfo;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			foreach (var transportMode in transportModesNotRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			foreach (var transportMode in transportModesNotRequireTransportNationality)
			{
				declaration.JE_TransportMode = transportMode;
				ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			}
		});
	}

	public void TestCheckJE_TotalWeight()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TotalWeightInfo);
	}

	public void TestCheckJE_TotalWeightUnit()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_TotalWeightUnitInfo, "XX", Core.Constants.Weight.Kilograms);
	}

	public void TestCheckJE_TotalNoOfPieces()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TotalNoOfPiecesInfo);
	}

	public void TestCheckJE_GS_NKCusAgent_InvalidOrEmptyCode()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GS_NKCusAgentInfo, "XX", Env.CurrentUser.Initials);
	}

	public void TestCheckJE_GoodsNumber_WesternEuropeanChar()
	{
		var dec = Factory.New<JobDeclaration>();

		var info = dec.JE_GoodsNumberInfo;
		var expectedError = EnglishCharactersValidation.GetNotificationMessage(info);

		dec.JE_GoodsNumber = "□□□";
		AssertHasError(info, expectedError);

		dec.JE_GoodsNumber = "TEST012345";
		AssertNoError(info, expectedError);
	}

	public void TestCheckJE_OA_DeclarantHasAuthorization()
	{
		const string message = "The Declarant must have an Authorization of Type 'IDE'";

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;

		CombineAssertions(() =>
		{
			AssertHasMessageError("No authorization", declaration.JE_OA_DeclarantAddressInfo, message);

			var authorisation = CreateAuthorisationRecord(declarantOrgHeader, CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration, "NUMBER1");
			declaration.Validation.ValidateAll();
			AssertHasMessageError("No correct authorization", declaration.JE_OA_DeclarantAddressInfo, message);

			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration;
			declaration.Validation.ValidateAll();
			AssertNoMessageError("Has authorization", declaration.JE_OA_DeclarantAddressInfo, message);
		});
	}

	public void TestCheckJE_OA_DeclarantHasVATNumber()
	{
		const string message = "Declarant is missing MVA Number in Registration Numbers / Codes.";

		var organizationHasVAT = Factory.New<OrgHeader>().AsMVARegistered();
		var organizationNoVAT = Factory.New<OrgHeader>();

		var declarantAddressHasVAT = organizationHasVAT.MainAddress.PK;
		var declarantAddressNoVAT = organizationNoVAT.MainAddress.PK;

		CombineAssertions(() =>
		{
			declaration.JE_OA_DeclarantAddress = declarantAddressNoVAT;
			AssertHasMessageError("No VAT", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = declarantAddressHasVAT;
			AssertNoMessageError("Has VAT", declaration.JE_OA_DeclarantAddressInfo, message);
		});
	}

	public void TestCheckJE_OA_DeclarantAddressEmpty()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OA_DeclarantAddressInfo);
	}

	public void TestCheckJE_MessageSubType_ShouldBeMandatory()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_MessageSubTypeInfo, "X", "EU");
	}

	public void TestCheckNO_CustomsTransportMode_ShouldBeMandatory()
	{
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsTransportModeInfo, "X", "10");
	}

	public void TestCheckJE_OH_Supplier_IfNotEntered()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_SupplierInfo);
	}

	public void TestCheckJE_OH_Importer_IfNotEntered()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_ImporterInfo);
	}

	public void TestCheckJE_MergeBy()
	{
		const string expectedWarning = "The merge key should be NON on a Preliminary declaration entry (Customs Message Type FO).";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.P;

		CombineAssertions(() =>
		{
			declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
			declaration.Validation.ValidateJE_MergeBy();
			AssertHasWarning("When JE_MergeBy: MAX, EntryInstruction.CEI_SubStyle: P", declaration.JE_MergeByInfo, expectedWarning);

			declaration.JE_MergeBy = MergeInvoiceLinesConstants.None;
			declaration.Validation.ValidateJE_MergeBy();
			AssertNoWarning("When JE_MergeBy: NON, EntryInstruction.CEI_SubStyle: P", declaration.JE_MergeByInfo, expectedWarning);

			entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.N;
			declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
			declaration.Validation.ValidateJE_MergeBy();
			AssertNoWarning("When JE_MergeBy: MAX, EntryInstruction.CEI_SubStyle: N", declaration.JE_MergeByInfo, expectedWarning);
		});
	}

	public void TestCheckJE_LocationOfGoods()
	{
		const string expectedMessageError = "Goods location A8 is not allowed on a Preliminary declaration entry (Customs Message Type FO).";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.P;

		CombineAssertions(() =>
		{
			declaration.JE_LocationOfGoods = ImportGoodsLocationCodeList.Codes._10DayWarehouseConsignee;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageError("When JE_LocationOfGoods: A8, EntryInstruction.CEI_SubStyle: P", declaration.JE_LocationOfGoodsInfo, expectedMessageError);

			declaration.JE_LocationOfGoods = ImportGoodsLocationCodeList.Codes.CustomsWarehouseD;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageError("When JE_LocationOfGoods: D, EntryInstruction.CEI_SubStyle: P", declaration.JE_LocationOfGoodsInfo, expectedMessageError);

			entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.N;
			declaration.JE_LocationOfGoods = ImportGoodsLocationCodeList.Codes._10DayWarehouseConsignee;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageError("When JE_LocationOfGoods: A8, EntryInstruction.CEI_SubStyle: N", declaration.JE_LocationOfGoodsInfo, expectedMessageError);
		});
	}

	HashSet<string> transportModesRequireTransportNationality => new HashSet<string> { TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Road, TransportTypeList.Codes.Air, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.Sea };

	HashSet<string> transportModesNotRequireTransportNationality => new HashSet<string> { TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Rail };

	#region Helpers

	CusAuthorisationHeader CreateAuthorisationRecord(OrgHeader orgHeader, ZString type, ZString number, string countryCode = Core.Constants.CountryCodes.Norway)
	{
		var result = orgHeader.Factory.New<CusAuthorisationHeader>();
		result.CPH_OH_PermitHolder = orgHeader.PK;
		result.CPH_Type = type;
		result.CPH_RN_NKCountryCode = countryCode;
		result.CPH_StartDate = ZDate.Today.AddDays(-1);
		result.CPH_EndDate = ZDate.Today.AddDays(1);
		result.CPH_Number = number;
		return result;
	}

	#endregion

	#region Impl

	protected override string MessageType => JobMessageTypeList.Codes.MiscellaneousCustoms;

	protected override JobDeclarationValidation GetValidation() => new JobDeclarationValidation(declaration);

	#endregion
}
