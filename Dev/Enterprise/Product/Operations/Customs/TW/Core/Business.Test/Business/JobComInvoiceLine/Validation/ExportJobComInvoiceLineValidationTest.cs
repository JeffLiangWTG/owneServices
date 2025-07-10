using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineValidation))]
	sealed partial class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest<ExportJobComInvoiceLineValidation>
	{
		public void TestCheckJI_TariffHasImportRegulationOrExportRegulation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff_02089029204 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029204", minDate, maxDate);
			var tariff_02089029204_Attribute_EXP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_02089029204_Attribute_EXP_111.ZZ3_Value, "管制輸出。", minDate, maxDate);

			var tariff_02089029204_Attribute_EXP_F01 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "F01", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_02089029204_Attribute_EXP_F01.ZZ3_Value, "輸入商品應依照「食品及相關產品輸入查驗辦法」規定，向衛生福利部食品藥物管理署申請辦理輸入查驗。【註：相關規定應洽衛生福利部食品藥物管理署】。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_111.ZZ3_Value, "管制輸入。", minDate, maxDate);

			var tariff_02089029206 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029206", minDate, maxDate);
			var tariff_02089029206_Attribute_EXP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "112", tariff_02089029206);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_02089029206_Attribute_EXP_111.ZZ3_Value, "大陸物品不准輸入。", minDate, maxDate);

			var tariff_02089029208 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029208", minDate, maxDate);
			Factory.Save();

			CombineAssertions(() =>
			{
				InvoiceLine.JI_Tariff = tariff_02089029204.ZZ1_TariffCode;
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Export Regulation 111, F01. Permit Number might be required.");

				var permit1 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit1.CSI_ReferenceNumber = "ref1";
				permit1.CSI_LineNo = 1;
				var permit2 = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permit2.CSI_ReferenceNumber = "ref2";
				permit2.CSI_LineNo = 2;
				InvoiceLine.RunPreSaveValidation();
				AssertNoRowWarningContaining(InvoiceLine, "The selected Tariff has Export Regulation 111, F01. Permit Number might be required.");

				InvoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
				InvoiceLine.JI_Tariff = tariff_02089029206.ZZ1_TariffCode;
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Export Regulation 112. Permit Number might be required.");

				var exemptionOfControllingAgency = InvoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
				exemptionOfControllingAgency.CSI_ReferenceNumber = "ref1";
				exemptionOfControllingAgency.CSI_LineNo = 1;
				InvoiceLine.RunPreSaveValidation();
				AssertNoRowWarningContaining(InvoiceLine, "The selected Tariff has Export Regulation 112. Permit Number might be required.");

				var permitCusSupportingCollection = InvoiceLine.PermitCusSupportingCollection.AddNew();
				permitCusSupportingCollection.CSI_ReferenceNumber = "ref2";
				permitCusSupportingCollection.CSI_LineNo = 2;
				InvoiceLine.RunPreSaveValidation();
				AssertHasRowWarning(InvoiceLine, "The selected Tariff has Export Regulation 112. Permit Number might be required.");

				InvoiceLine.JI_Tariff = tariff_02089029208.ZZ1_TariffCode;
				AssertHasRowWarning(InvoiceLine, "The selected Tariff doesn't have Export Regulation. Permit Number might not be required.");

				InvoiceLine.ExemptionOfControllingAgenciesCusSupportings.RemoveAndDeleteAll();
				InvoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
				InvoiceLine.JI_Tariff = tariff_02089029208.ZZ1_TariffCode;
				AssertNoRowWarnings(InvoiceLine);
			});
		}

		public void TestCheckJI_CountryOfOrigin_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff_87120010109 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87120010109", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination, "AT;BE;BG;CY;CZ;DE;DK;EE;ES;FI;FR", tariff_87120010109);

			var tariff_87120010110 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87120010110", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination, "GR;HR;HU;IE;IT;LT;LU;LV;MT;NL;PL;PT;RO;SE;SI;SK;GB;US", tariff_87120010110);
			Factory.Save();

			var errorMessage = "When the Final Destination country for export from the Free Trade Zone is one of EU, US, and GB, the Goods Origin is required.";
			var decl = InvoiceLine.Declaration;
			var entryInstruction = decl.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			decl.JE_RL_NKFinalDestination = "USLAX";
			InvoiceLine.JI_Tariff = tariff_87120010109.ZZ1_TariffCode;
			InvoiceLine.RunPreSaveValidation();
			AssertNoMessageError(InvoiceLine.JI_CountryOfOriginInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariff_87120010110.ZZ1_TariffCode;
			InvoiceLine.RunPreSaveValidation();
			AssertHasMessageError(InvoiceLine.JI_CountryOfOriginInfo, errorMessage);

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			InvoiceLine.RunPreSaveValidation();
			AssertNoMessageError(InvoiceLine.JI_CountryOfOriginInfo, errorMessage);
		}

		public void TestCheckJI_CountryOfOriginInvalidCode()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var info = invoiceLine.JI_CountryOfOriginInfo;
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.Code = "AU";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageErrors(info);
			invoiceLine.JI_CountryOfOrigin = "XX";
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CountryOfOrigin = country.Code;
			AssertNoMessageErrors(info);
		}

		public void TestCheckJI_CustomsSecondQuantityAndUnit()
		{
			InvoiceLine.Declaration.JE_MessageType = "EXP";
			var messageError = "Please enter a 'Statistical Quantity' greater than 0.";
			var info = InvoiceLine.JI_CustomsSecondQuantityInfo;
			InvoiceLine.JI_CustomsSecondUnitQty = "ADT";
			InvoiceLine.JI_CustomsSecondQuantity = -1m;
			AssertHasMessageErrorContaining(info, messageError);
			InvoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			InvoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoMessageErrors(info);
			InvoiceLine.JI_CustomsSecondUnitQty = "ADT";
			InvoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertHasMessageErrorContaining(info, messageError);
			InvoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertNoMessageErrors(info);
		}

		public override void TestCheckJI_BrandName()
		{
			base.TestCheckJI_BrandName();

			var message = "System will automatically declare 'No Brand' when Brand Name is empty.";
			InvoiceLine.JI_BrandName = ZString.Empty;
			AssertHasWarning(InvoiceLine.JI_BrandNameInfo, message);

			InvoiceLine.JI_BrandName = "brand";
			AssertNoWarning(InvoiceLine.JI_BrandNameInfo, message);
		}

		public override void TestCheckJI_Procedure()
		{
			base.TestCheckJI_Procedure();

			InvoiceLine.JI_Procedure = ProcedureCodes._02;
			AssertNoRowMessageError(InvoiceLine, ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);

			var permit = InvoiceLine.PermitCusSupportingCollection.AddNew();
			permit.CSI_ItemNumber = 1;
			permit.CSI_ReferenceNumber = "test 1";
			InvoiceLine.JI_Procedure = ProcedureCodes._01;
			AssertNoRowMessageError(InvoiceLine, ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);

			InvoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
			InvoiceLine.Validation.ValidateJI_Procedure();
			AssertHasRowMessageError(InvoiceLine, ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);
		}

		public void TestCheckPermitCusSupportingNo1()
		{
			InvoiceLine.JI_Procedure = ProcedureCodes._01;
			InvoiceLine.Validation.ValidatePermitCusSupportingNo1();
			AssertHasMessageError(InvoiceLine.PermitCusSupportingNo1Info, ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);

			InvoiceLine.JI_Procedure = ProcedureCodes._02;
			InvoiceLine.Validation.ValidatePermitCusSupportingNo1();
			AssertNoMessageError(InvoiceLine.PermitCusSupportingNo1Info, ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);

			InvoiceLine.JI_Procedure = ProcedureCodes._01;
			InvoiceLine.PermitCusSupportingNo1 = "test 1";
			InvoiceLine.PermitCusSupportingLineNo1 = 1;
			InvoiceLine.Validation.ValidatePermitCusSupportingNo1();
			AssertNoMessageError(InvoiceLine.PermitCusSupportingNo1Info, ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);
		}

		protected override JobDeclaration CreateNewDeclaration()
		{
			var declaration = base.CreateNewDeclaration();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			return declaration;
		}
	}
}
