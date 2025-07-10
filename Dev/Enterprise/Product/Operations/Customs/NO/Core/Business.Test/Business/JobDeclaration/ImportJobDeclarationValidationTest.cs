using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(ImportJobDeclarationValidation))]
	sealed class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ImportJobDeclarationValidation>
	{
		public void TestCheckJE_GoodsDestination()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_GoodsDestinationInfo, "XX", Core.Constants.CountryCodes.Norway);
		}

		public void TestCheckJE_GoodsNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_GoodsNumberInfo);
		}

		public void TestCheckJE_GoodsNumber_CEI_Procedure()
		{
			const string expectedMessageErrorText = "The Procedure code you have selected do not require the Goods Number.";
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new[] { "4050", "4051", "4052" })
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					declaration.Validation.ValidateJE_GoodsNumber();
					AssertNoMessageError($"(base-case-{procedureCode}): NO isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertHasMessageErrorContaining($"(base-case-{procedureCode}): HAS isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.JE_GoodsNumber = "A";
					AssertNoMessageError($"(goodsnr-set-{procedureCode}): NO isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(goodsnr-set-{procedureCode}): NO isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);

					var instruction = declaration.CustomsEntryInstructions.AddNew();
					instruction.CEI_Procedure = procedureCode;
					declaration.Validation.ValidateJE_GoodsNumber();
					AssertHasMessageError($"(instruction-added-{procedureCode}): HAS isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(instruction-added-{procedureCode}): NO isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.CustomsEntryInstructions.Delete(instruction);
					declaration.Validation.ValidateJE_GoodsNumber();
					AssertNoMessageError($"(instruction-deleted-{procedureCode}): NO isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(instruction-deleted-{procedureCode}): NO isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckJE_GoodsNumber_JI_Procedure()
		{
			const string expectedMessageErrorText = "The Procedure code you have selected do not require the Goods Number.";
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new[] { "4050", "4051", "4052" })
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoice = declaration.Invoices.AddNew();

					declaration.Validation.ValidateJE_GoodsNumber();
					AssertNoMessageError($"(base-case-{procedureCode}): NO isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertHasMessageErrorContaining($"(base-case-{procedureCode}): HAS isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.JE_GoodsNumber = "A";
					AssertNoMessageError($"(goodsnr-set-{procedureCode}): NO isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(goodsnr-set-{procedureCode}): NO isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);

					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Procedure = procedureCode;
					declaration.Validation.ValidateJE_GoodsNumber();
					AssertHasMessageError($"(invoiceline-added-{procedureCode}): HAS isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(invoiceline-added-{procedureCode}): NO isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);

					invoice.InvoiceLines.Remove(invoiceLine);
					declaration.Validation.ValidateJE_GoodsNumber();
					AssertNoMessageError($"(invoiceline-deleted-{procedureCode}): NO isNotRequired", declaration.JE_GoodsNumberInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(invoiceline-deleted-{procedureCode}): NO isMandatory", declaration.JE_GoodsNumberInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckJE_GoodsOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GoodsOriginInfo, "XX", Core.Constants.CountryCodes.Norway);
		}

		public void TestCheckJE_LocationOfGoods()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_LocationOfGoodsInfo, "XXX", ImportGoodsLocationCodeList.Codes.CustomsWarehouseA);
		}

		public void TestCheckJE_LocationOfGoods_CEI_Procedure()
		{
			const string expectedMessageErrorText = "The Procedure code you have selected do not require the Location Of Goods.";
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new[] { "4050", "4051", "4052" })
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertNoMessageError($"(base-case-{procedureCode}): NO isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertHasMessageErrorContaining($"(base-case-{procedureCode}): HAS isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.JE_LocationOfGoods = "A";
					AssertNoMessageError($"(location-set-{procedureCode}): NO isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(location-set-{procedureCode}): NO isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

					var instruction = declaration.CustomsEntryInstructions.AddNew();
					instruction.CEI_Procedure = procedureCode;
					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertHasMessageError($"(instruction-added-{procedureCode}): HAS isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(instruction-added-{procedureCode}): NO isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.CustomsEntryInstructions.Delete(instruction);
					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertNoMessageError($"(instruction-deleted-{procedureCode}): NO isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(instruction-deleted-{procedureCode}): NO isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckJE_LocationOfGoods_JI_Procedure()
		{
			const string expectedMessageErrorText = "The Procedure code you have selected do not require the Location Of Goods.";
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new[] { "4050", "4051", "4052" })
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoice = declaration.Invoices.AddNew();

					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertNoMessageError($"(base-case-{procedureCode}): NO isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertHasMessageErrorContaining($"(base-case-{procedureCode}): HAS isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.JE_LocationOfGoods = "A";
					AssertNoMessageError($"(location-set-{procedureCode}): NO isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(location-set-{procedureCode}): NO isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Procedure = procedureCode;
					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertHasMessageError($"(invoiceline-added-{procedureCode}): HAS isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(invoiceline-added-{procedureCode}): NO isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

					invoice.InvoiceLines.Remove(invoiceLine);
					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertNoMessageError($"(invoiceline-deleted-{procedureCode}): NO isNotRequired", declaration.JE_LocationOfGoodsInfo, expectedMessageErrorText);
					AssertNoMessageErrorContaining($"(invoiceline-deleted-{procedureCode}): NO isMandatory", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckJE_OH_Importer()
		{
			const string importerIsPersonNoSSNMessageText = "The importer is a natural person but has no social security number.";
			const string importerIsOrgNotMvaNotORGMessageText = "The importer is an organization but has no ORG number or MVA registered number.";
			const string importerIsPersonMessageText = "The importer is a natural person. That is: No deferred customs account and not MVA registered. Potential outlays.";
			const string importerIsOrgNotMvaNotDeferredMessageText = "The importer is not MVA registered and has no deferred customs account. Potential outlays.";
			const string importerIsOrgNotMvaHasDeferredMessageText = "The importer is not MVA registered. Potential outlays.";
			const string importerIsOrgHasMvaNotDeferredMessageText = "The importer has no deferred customs account. Potential outlays.";

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			CombineAssertions(() =>
			{
				importer.OH_Category = Constants.OrgHeaderType.NaturalPerson;
				AssertMessageErrors("The importer is a person without SSN.", importerIsPersonNoSSNMessageText, importerIsPersonMessageText,
					new[] { importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaNotDeferredMessageText, importerIsOrgNotMvaHasDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				var cusCode = importer.CustomsCodes.AddNew();
				cusCode.OK_CodeType = Constants.OrgCodeType.SocialSecurityNumber;
				AssertMessageErrors("The importer is a person but has an empty SSN CusCode.", importerIsPersonNoSSNMessageText, importerIsPersonMessageText,
					new[] { importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaNotDeferredMessageText, importerIsOrgNotMvaHasDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				cusCode.OK_CustomsRegNo = "200001011234";
				AssertMessageErrors("The importer is a person with a valid SSN CusCode.", null, importerIsPersonMessageText,
					new[] { importerIsPersonNoSSNMessageText, importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaNotDeferredMessageText, importerIsOrgNotMvaHasDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				importer.OH_Category = Constants.OrgHeaderType.Organization;
				AssertMessageErrors("The importer is an organization without MVA and DefermentApprovalNumber.", importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaNotDeferredMessageText,
					new[] { importerIsPersonMessageText, importerIsPersonNoSSNMessageText, importerIsOrgNotMvaHasDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				var deferredCode = importer.CustomsCodes.AddNew();
				deferredCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
				deferredCode.OK_CustomsRegNo = "1234567890";
				AssertMessageErrors("The importer is an organization without MVA, but with DefermentApprovalNumber.", importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaHasDeferredMessageText,
					new[] { importerIsPersonMessageText, importerIsPersonNoSSNMessageText, importerIsOrgNotMvaNotDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				cusCode.OK_CodeType = Constants.OrgCodeType.OrganizationNumber;
				cusCode.OK_CustomsRegNo = "1234567890";
				AssertMessageErrors("The importer is an organization with ORG and DefermentApprovalNumber, but without MVA.", null, importerIsOrgNotMvaHasDeferredMessageText,
					new[] { importerIsOrgNotMvaNotORGMessageText, importerIsPersonMessageText, importerIsPersonNoSSNMessageText, importerIsOrgNotMvaNotDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				cusCode.OK_CodeType = Constants.OrgCodeType.MVARegistrationNumber;
				AssertMessageErrors("The importer is an organization with both MVA and DefermentApprovalNumber.", null, null,
					new[] { importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaHasDeferredMessageText, importerIsPersonMessageText, importerIsPersonNoSSNMessageText, importerIsOrgNotMvaNotDeferredMessageText, importerIsOrgHasMvaNotDeferredMessageText }
				);

				deferredCode.OK_CustomsRegNo = ZString.Empty;
				AssertMessageErrors("The importer is an organization with MVA but without valid DefermentApprovalNumber.", null, importerIsOrgHasMvaNotDeferredMessageText,
					new[] { importerIsOrgNotMvaNotORGMessageText, importerIsOrgNotMvaHasDeferredMessageText, importerIsPersonMessageText, importerIsPersonNoSSNMessageText, importerIsOrgNotMvaNotDeferredMessageText }
				);
			});

			void AssertMessageErrors(string testDescription, string errorMessage, string warningMessage, string[] errosToAvoid)
			{
				declaration.Validation.ValidateJE_OH_Importer();
				var targetInfo = declaration.JE_OH_ImporterInfo;

				if (!errorMessage.IsNullOrEmpty())
				{
					AssertHasMessageError($"{testDescription} - {errorMessage}", targetInfo, errorMessage);
				}
				if (!warningMessage.IsNullOrEmpty())
				{
					AssertHasWarning($"{testDescription} - {warningMessage}", targetInfo, warningMessage);
				}
				foreach (var error in errosToAvoid)
				{
					AssertNoMessageError($"{testDescription} - {error}", targetInfo, error);
					AssertNoWarning($"{testDescription} - {error}", targetInfo, error);
				}
			}
		}

		public void TestImporterHasPOC()
		{
			var powerOfAttorneyWarningMessage = AuthorityToActValidator.GetNoPOADocumentForImporterString(new AuthorityToActValidator().CountrySpecificNameForPOA);
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var targetInfo = declaration.JE_OH_ImporterInfo;

			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasWarning("Importer has no POC", targetInfo, powerOfAttorneyWarningMessage);

				var poc = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoWarning("Importer has POC", targetInfo, powerOfAttorneyWarningMessage);
			});
		}

		#region Impl

		protected override string MessageType => JobMessageTypeList.Codes.Import;

		protected override ImportJobDeclarationValidation GetValidation() => new ImportJobDeclarationValidation(declaration);

		#endregion
	}
}
