using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR1513()
	{
		var errorMessage = "(R1513) – An additional info code 0PL12 is required for each invoice line (it is recommended to add the additional information code 0PL12 in the invoice header or in Declaration/Misc).";
		var invoice1 = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoice2 = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		var addInfo = invoice1.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._0PL12;
		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateGoodsLocationCustomsOffice();
			AssertNoMessageError("GoodsLocationCustomsOffice empty", jobDeclaration.GoodsLocationCustomsOfficeInfo, errorMessage);
			jobDeclaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.CUS;
			jobDeclaration.GoodsLocationCustomsOffice = "PL372020";
			jobDeclaration.JE_CustomsOffice = "PL374000";
			jobDeclaration.Validation.ValidateGoodsLocationCustomsOffice();
			AssertNoMessageError("GoodsLocationCustomsOffice starts same as JE_CustomsOffice", jobDeclaration.GoodsLocationCustomsOfficeInfo, errorMessage);
			jobDeclaration.JE_CustomsOffice = "PL2";
			jobDeclaration.Validation.ValidateGoodsLocationCustomsOffice();
			AssertHasMessageError("GoodsLocationCustomsOffice starts differently as JE_CustomsOffice", jobDeclaration.GoodsLocationCustomsOfficeInfo, errorMessage);
			addInfo = invoice2.AdditionalInfos.AddNew();
			addInfo.CSI_Code = AdditionalInfoCodes._0PL12;
			jobDeclaration.Validation.ValidateGoodsLocationCustomsOffice();
			AssertNoMessageError("All invoices have 0PL12 add info", jobDeclaration.GoodsLocationCustomsOfficeInfo, errorMessage);
		});
	}

	public void TestCheckJE_OA_DeclarantAddress_Empty()
	{
		var messageError = "(R293) You have not entered a Declarant.";
		var orgAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgAddress.OA_OH = orgHeader.PK;
		var invoice = jobDeclaration.Invoices.AddNew();
		var line = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError("Declarant empty, representative empty, no 00500 addinfo", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
			var addInfo = jobDeclaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = AdditionalInfoCodes._00500;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("Declarant empty, representative empty, declaration 00500 addinfo", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
			jobDeclaration.AdditionalInfos.RemoveAll();
			addInfo = invoice.AdditionalInfos.AddNew();
			addInfo.CSI_Code = AdditionalInfoCodes._00500;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("Declarant empty, representative empty, invoice 00500 addinfo", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
			invoice.AdditionalInfos.RemoveAll();
			addInfo = line.AdditionalInfos.AddNew();
			addInfo.CSI_Code = AdditionalInfoCodes._00500;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("Declarant empty, representative empty, line 00500 addinfo", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
			invoice.AdditionalInfos.RemoveAll();
			jobDeclaration.JE_OA_Representative = orgAddress.PK;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("Declarant empty, representative not empty, no 00500 addinfo", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
			jobDeclaration.JE_OA_Representative = ZGuid.Empty;
			jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("Declarant not empty, representative empty, no 00500 addinfo", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
		});
	}

	public void TestCheckRuleR489_JE_VesselName_FeasibleSubStyle()
	{
		const string messageError = "(R489) Transport ID [21] or Transport ID Inland [18] is missing.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;

		var subStyleList = new List<ZString> { "A", "B", "C", "X", "Y" };
		CombineAssertions(() =>
		{
			foreach (var subStyle in subStyleList)
			{
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = "70";
				line.JI_Procedure = "7000000";
				foreach (var transportMode in new[] { TransportTypeList.Codes.Mail, TransportTypeList.Codes.FixedTransportInstallations })
				{
					declaration.JE_TransportMode = transportMode;
					declaration.Validation.ValidateJE_VesselName();
					AssertNoMessageError($"{subStyle}|ProcedureCodeBase!=71|{transportMode}", declaration.JE_VesselNameInfo, messageError);
				}

				foreach (var transportMode in new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea })
				{
					entryInstruction.CEI_Procedure = "70";
					line.JI_Procedure = "7000000";
					declaration.JE_TransportMode = transportMode;
					declaration.ZG_BorderTransportMeans = "11";
					declaration.Validation.ValidateJE_VesselName();
					AssertHasMessageError($"{subStyle}|ProcedureCodeBase!=71|{transportMode}", declaration.JE_VesselNameInfo, messageError);

					entryInstruction.CEI_Procedure = "71";
					declaration.Validation.ValidateJE_VesselName();
					AssertNoMessageError($"{subStyle}|ProcedureCodeBase=71|{transportMode}", declaration.JE_VesselNameInfo, messageError);

					entryInstruction.CEI_Procedure = "00";
					line.JI_Procedure = "0071000";
					declaration.Validation.ValidateJE_VesselName();
					AssertNoMessageError($"{subStyle}|PreviousProcedureCode=71|{transportMode}", declaration.JE_VesselNameInfo, messageError);
				}

				entryInstruction.CEI_Procedure = "70";
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.ZG_BorderTransportMeans = ImportEUBorderTransportMeansList.Codes.Ship;
				declaration.Validation.ValidateJE_VesselName();
				AssertNoMessageError($"{subStyle}|PreviousProcedureCode=71|Sea|ImoNumber", declaration.JE_VesselNameInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR489_JE_VesselName_UnfeasibleSubStyle()
	{
		const string messageError = "(R489) Transport ID [21] or Transport ID Inland [18] is missing.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_SubStyle = "Z";
		entryInstruction.CEI_Procedure = "70";
		CombineAssertions(() =>
		{
			foreach (var transportMode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road })
			{
				declaration.JE_TransportMode = transportMode;
				declaration.Validation.ValidateJE_VesselName();
				AssertEquals(transportMode, false, declaration.JE_VesselNameInfo.HasMessageError(messageError));
			}
		});
	}

	public void TestCheckRuleR489_JE_VoyageFlightNo_FeasibleSubStyle()
	{
		const string messageError = "(R489) Transport ID [21] or Transport ID Inland [18] is missing.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;

		var subStyleList = new List<ZString> { "A", "B", "C", "X", "Y" };
		CombineAssertions(() =>
		{
			foreach (var subStyle in subStyleList)
			{
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = "70";
				line.JI_Procedure = "7000000";
				foreach (var transportMode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportModeCodeList.Codes.Mail })
				{
					declaration.JE_TransportMode = transportMode;
					declaration.Validation.ValidateJE_VoyageFlightNo();
					AssertNoMessageError($"{subStyle}|ProcedureCodeBase!=71|{transportMode}", declaration.JE_VoyageFlightNoInfo, messageError);
				}

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageError($"{subStyle}|ProcedureCodeBase!=71|IsAir", declaration.JE_VoyageFlightNoInfo, messageError);

				entryInstruction.CEI_Procedure = "71";
				declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError($"{subStyle}|ProcedureCodeBase=71|IsAir", declaration.JE_VoyageFlightNoInfo, messageError);

				entryInstruction.CEI_Procedure = "00";
				line.JI_Procedure = "0071000";
				declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError($"{subStyle}|PreviousProcedureCode=71|IsAir", declaration.JE_VoyageFlightNoInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR489_JE_VoyageFlightNo_UnfeasibleSubStyle()
	{
		const string messageError = "(R489) Transport ID [21] or Transport ID Inland [18] is missing.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_SubStyle = "Z";
		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		declaration.Validation.ValidateJE_VoyageFlightNo();
		AssertEquals(false, declaration.JE_VoyageFlightNoInfo.HasMessageError(messageError));
	}

	public void TestCheckRuleR489_JE_LloydsIMO_FeasibleSubStyle()
	{
		const string messageError = "(R489) Transport ID [21] or Transport ID Inland [18] is missing.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;

		var subStyleList = new List<ZString> { "A", "B", "C", "X", "Y" };
		var nonSeaTransports = new (ZString Mode, ZString BorderTransportMeans)[]
		{
			(TransportTypeList.Codes.Air, ImportEUBorderTransportMeansList.Codes.Plane),
			(TransportTypeList.Codes.Road, ImportEUBorderTransportMeansList.Codes.Truck),
			(TransportTypeList.Codes.Mail, ImportEUBorderTransportMeansList.Codes.Other),
		};
		CombineAssertions(() =>
		{
			foreach (var subStyle in subStyleList)
			{
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = "70";
				line.JI_Procedure = "7000000";
				foreach (var transport in nonSeaTransports)
				{
					declaration.JE_TransportMode = transport.Mode;
					declaration.ZG_BorderTransportMeans = transport.BorderTransportMeans;
					declaration.Validation.ValidateJE_LloydsIMO();
					AssertNoMessageError($"{subStyle}|ProcedureCodeBase!=71|{transport.Mode}", declaration.JE_LloydsIMOInfo, messageError);
				}

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.ZG_BorderTransportMeans = ImportEUBorderTransportMeansList.Codes.Ship;
				declaration.Validation.ValidateJE_LloydsIMO();
				AssertHasMessageError($"{subStyle}|ProcedureCodeBase!=71|IsSea|ImoNumber", declaration.JE_LloydsIMOInfo, messageError);

				declaration.ZG_BorderTransportMeans = ImportEUBorderTransportMeansList.Codes.Excluded;
				declaration.Validation.ValidateJE_LloydsIMO();
				AssertNoMessageError($"{subStyle}|ProcedureCodeBase!=71|IsSea|NotImoNumber", declaration.JE_LloydsIMOInfo, messageError);

				declaration.ZG_BorderTransportMeans = ImportEUBorderTransportMeansList.Codes.Ship;
				entryInstruction.CEI_Procedure = "71";
				declaration.Validation.ValidateJE_LloydsIMO();
				AssertNoMessageError($"{subStyle}|ProcedureCodeBase=71|IsSea", declaration.JE_LloydsIMOInfo, messageError);

				entryInstruction.CEI_Procedure = "00";
				line.JI_Procedure = "0071000";
				declaration.Validation.ValidateJE_LloydsIMO();
				AssertNoMessageError($"{subStyle}|PreviousProcedureCode=71|IsSea", declaration.JE_LloydsIMOInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR489_JE_LloydsIMO_UnfeasibleSubStyle()
	{
		const string messageError = "(R489) Transport ID [21] or Transport ID Inland [18] is missing.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_SubStyle = "Z";
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		declaration.ZG_BorderTransportMeans = ImportEUBorderTransportMeansList.Codes.Ship;

		declaration.Validation.ValidateJE_LloydsIMO();
		AssertEquals(false, declaration.JE_LloydsIMOInfo.HasMessageError(messageError));
	}

	public void TestCheckRuleR233_JE_VesselName()
	{
		const string errorMessage = "(R233) Transport ID contains invalid characters (only capital letters A through Z, digits 0 to 9 and the / character are allowed).";
		var declaration = GetImportDeclaration();
		declaration.JE_TransportMode = TransportTypeList.Codes.Road;

		ZPropertyInfo setVesselNameThenValidate(string value)
		{
			declaration.JE_VesselName = value;
			declaration.Validation.ValidateJE_VesselName();
			return declaration.JE_VesselNameInfo;
		}

		AssertPropertyCanHaveOnlyUpperOrDigitOrSlash(setVesselNameThenValidate, errorMessage);
	}

	public void TestCheckRuleR208_JE_Trailer1RegNo()
	{
		const string errorMessage = "(R208) Transport ID contains invalid characters (only capital letters A through Z, digits 0 to 9 and the / character are allowed).";
		var declaration = GetImportDeclaration();
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
		AssertPropertyCanHaveOnlyUpperOrDigitOrSlash(
			(value) =>
			{
				declaration.JE_Trailer1RegNo = value;
				declaration.Validation.ValidateJE_Trailer1RegNo();
				return declaration.JE_Trailer1RegNoInfo;
			},
			errorMessage);
	}

	public void TestCheckRuleR208_JE_Trailer2RegNo()
	{
		const string errorMessage = "(R208) Transport ID contains invalid characters (only capital letters A through Z, digits 0 to 9 and the / character are allowed).";
		var declaration = GetImportDeclaration();
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
		AssertPropertyCanHaveOnlyUpperOrDigitOrSlash(
			(value) =>
			{
				declaration.JE_Trailer2RegNo = value;
				declaration.Validation.ValidateJE_Trailer2RegNo();
				return declaration.JE_Trailer2RegNoInfo;
			},
			errorMessage);
	}

	public void TestCheckRuleR208_JE_TransportIDInland()
	{
		const string errorMessage = "(R208) Transport ID contains invalid characters (only capital letters A through Z, digits 0 to 9 and the / character are allowed).";
		var declaration = GetImportDeclaration();
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
		AssertPropertyCanHaveOnlyUpperOrDigitOrSlash(
			(value) =>
			{
				declaration.JE_TransportIDInland = value;
				declaration.Validation.ValidateJE_TransportIDInland();
				return declaration.JE_TransportIDInlandInfo;
			},
			errorMessage);
	}

	public void TestCheckForRuleR254()
	{
		const string messageError = "(R254) – For the selected Entry Style code, the country of Export/Goods Origin must be other than PL.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = "CO";
			declaration.JE_GoodsOrigin = CountryCodes.Poland;
			AssertNoMessageError("No message error when entry style is CO", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_EntryStyle = "AA";
			declaration.JE_GoodsOrigin = CountryCodes.UnitedKingdom;
			AssertNoMessageError("No message error when goods origin is not Poland", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_GoodsOrigin = CountryCodes.Poland;
			AssertHasMessageError("Has message error", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("No message error when declaration is export", declaration.JE_GoodsOriginInfo, messageError);
		});
	}

	public void TestCheckForRuleR431()
	{
		const string messageError = "(R431) – For the selected Entry Style code and previous procedure code, the country of Export/Goods Origin must be other than EU.";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDate(2019, 1, 1), new ZDate(2060, 12, 31));
		helper.AddCountry(tradeGroup, CountryCodes.Poland, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invLine = invoiceHeader.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = "CO";
			declaration.JE_GoodsOrigin = CountryCodes.Poland;
			AssertNoMessageError("No message error when entry style is CO", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_EntryStyle = "AA";
			var previousProcedureCodes = new[] { "51", "53", "71" };
			foreach (var code in previousProcedureCodes)
			{
				invLine.PreviousProcedureCode = code;
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageError("No message error when not exist any invoice line with previous procedure code other than 71 or 51 or 53", declaration.JE_GoodsOriginInfo, messageError);
			}

			invLine.PreviousProcedureCode = "11";
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertHasMessageError("Has message error", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("No message error when declaration is export", declaration.JE_GoodsOriginInfo, messageError);
		});
	}

	public void TestCheckForRuleR261()
	{
		const string messageError = "(R261) – For the selected Entry Style code, if the country of Export/Goods Origin must be other than CH, NO, IS, LI, TR, MK, XS, RS.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_EntryStyle = "AA";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("No message error when entry style is empty", declaration.JE_GoodsOriginInfo, messageError);

			var invalidCountryCodes = new[] { "CH", "NO", "IS", "LI", "TR", "MK", "XS", "RS" };
			foreach (var countryCode in invalidCountryCodes)
			{
				declaration.JE_GoodsOrigin = countryCode;
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasMessageError($"declaration - JE_GoodsOrigin = {countryCode}", declaration.JE_GoodsOriginInfo, messageError);

				declaration.JE_GoodsOrigin = ZString.Empty;
				invoiceLine.ZG_CountryOfSupply = countryCode;
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasMessageError($"declaration - ZG_CountryOfSupply = {countryCode}", declaration.JE_GoodsOriginInfo, messageError);
			}

			declaration.JE_GoodsOrigin = CountryCodes.Poland;
			invoiceLine.ZG_CountryOfSupply = ZString.Empty;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("declaration - JE_GoodsOrigin = Poland", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_GoodsOrigin = ZString.Empty;
			invoiceLine.ZG_CountryOfSupply = CountryCodes.Poland;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("declaration - ZG_CountryOfSupply = Poland", declaration.JE_GoodsOriginInfo, messageError);
		});
	}

	public void TestCheckForRuleR433()
	{
		const string messageError = "(R433) – Country of export code is required.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("No message error when No invoice line", declaration.JE_GoodsOriginInfo, messageError);

			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_GoodsOrigin = ZString.Empty;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertHasMessageError("declaration - Empty JE_GoodsOrigin", declaration.JE_GoodsOriginInfo, messageError);

			declaration.JE_GoodsOrigin = CountryCodes.Poland;
			AssertNoMessageError("declaration - Not Empty JE_GoodsOrigin", declaration.JE_GoodsOriginInfo, messageError);
		});
	}

	public void TestCheckRuleR1509()
	{
		const string messageError = "(R1509) - Selected Representation Type is not allowed.";
		var (declaration, entryInstruction, line) = GetImportDeclarationAndInstructionAndLine();
		line.JI_CEI = entryInstruction.PK;
		var supportingDocument = line.SupportingDocuments.AddNew();

		var representativeAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		representativeAddress.OA_OH = orgHeader.PK;
		orgHeader.OH_Category = Constants.OrgHeaderCategory.BUS;

		declaration.JE_OA_Representative = representativeAddress.PK;

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty Declarant Type", declaration.JE_DeclarantTypeInfo, messageError);
			declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._5Indirect;
			AssertNoMessageError("Declarant Type 5 - Indirect Representation and orgHeader.OH_Category is BUS", declaration.JE_DeclarantTypeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError("Cei Procedure 51", declaration.JE_DeclarantTypeInfo, messageError);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError("Cei Procedure 53", declaration.JE_DeclarantTypeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError("Cei Procedure 40 without Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N990;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError("Cei Procedure 40 with Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError("Cei Procedure 42 with Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._45;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError("Cei Procedure 45 with Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError("Cei Procedure 71 with Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Procedure = Constants.ProcedureCodes._40;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError("Cei Procedure 71 with Supporting Document N990 and Cei Procedure 40 without SupportingDocument N990", declaration.JE_DeclarantTypeInfo, messageError);

			var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
			declarationSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N990;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError("Cei Procedure 40 with Declaration Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);

			entryInstruction2.CEI_Procedure = Constants.ProcedureCodes._63;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError("Cei Procedure 63 & 71 with Declaration Supporting Document N990", declaration.JE_DeclarantTypeInfo, messageError);

			orgHeader.OH_Category = "ABC";
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			entryInstruction2.CEI_Procedure = Constants.ProcedureCodes._40;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError("orgHeader.OH_Category is not BUS", declaration.JE_DeclarantTypeInfo, messageError);
		});
	}

	public void TestCheckJE_OA_SellerAddress()
	{
		var declaration = GetImportDeclaration();
		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(declaration.JE_OA_SellerAddressInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty, orgAddress);
	}

	public void TestCheckJE_OA_DeclarantAddress()
	{
		var declaration = GetImportDeclaration();
		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(declaration.JE_OA_DeclarantAddressInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty, orgAddress);
	}

	public void TestCheckJE_OA_Representative()
	{
		var declaration = GetImportDeclaration();
		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(declaration.JE_OA_RepresentativeInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty, orgAddress);
	}

	public void TestCheckJE_LocationOfGoods()
	{
		var declaration = GetImportDeclaration();
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.OTH;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining("OTH - empty JE_LocationOfGoods", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_LocationOfGoods = "1234567890qwertyuiopasdfghjklzxcvb1234567890qwertyuiopasdfghjklzxcvb";
			AssertNoNotifications("OTH - not empty JE_LocationOfGoods", declaration.JE_LocationOfGoodsInfo);

			declaration.JE_LocationQualifier = GoodsLocationTypeList.Codes.GLC;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining("GLC - empty JE_LocationOfGoods", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_LocationOfGoods = "1234567890qwertyuiopasdfghjklzxcvb";
			AssertNoNotifications("GLC - not empty JE_LocationOfGoods", declaration.JE_LocationOfGoodsInfo);
		});
	}

	public void TestCheckJE_PaymentMethod()
	{
		var declaration = GetImportDeclaration();
		CombineAssertions(() =>
		{
			declaration.JE_PaymentMethod = ZString.Empty;
			AssertHasMessageErrorContaining("Empty Payment Method", declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_PaymentMethod = "1";
			AssertHasMessageErrorContaining("Invalid Payment Method", declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError.ToString());

			declaration.JE_PaymentMethod = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageErrorContaining("Valid Payment Method", declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Valid Payment Method", declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}

	public void TestCheckJE_TransportModeInland()
	{
		var declaration = GetImportDeclaration();
		CombineAssertions(() =>
		{
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageErrorContaining("Empty ZG_Box18TransportID", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_Box18TransportID = "1";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasMessageErrorContaining("Empty JE_TransportModeInland", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = "A";
			AssertNoMessageErrorContaining("Not Empty JE_TransportModeInlandInfo", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
	}

	JobDeclaration jobDeclaration;

	string MessageType => MessageTypeList.Codes.Import;

	JobDeclaration GetImportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		return declaration;
	}

	(JobDeclaration, CusEntryInstruction, JobComInvoiceLine) GetImportDeclarationAndInstructionAndLine()
	{
		var declaration = GetImportDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		return (declaration, entryInstruction, line);
	}

	void AssertPropertyCanHaveOnlyUpperOrDigitOrSlash(Func<string, ZPropertyInfo> setPropertyThenValidate, string errorMessage)
	{
		CombineAssertions(() =>
		{
			foreach (var validValue in new[] { "", "ABC", "123", "AB1/CD2" })
			{
				var infoToCheck = setPropertyThenValidate(validValue);
				AssertNoMessageError(validValue, infoToCheck, errorMessage);
			}

			foreach (var invalidValue in new[] { "AbC", "1-23", "AB1\\CD2" })
			{
				var infoToCheck = setPropertyThenValidate(invalidValue);
				AssertHasMessageError(invalidValue, infoToCheck, errorMessage);
			}
		});
	}
}
