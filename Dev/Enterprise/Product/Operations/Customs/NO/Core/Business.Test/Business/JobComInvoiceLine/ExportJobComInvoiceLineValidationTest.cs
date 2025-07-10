using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest
	{
		public void TestCheckJI_PrimaryPreference()
		{
			var validCodes = new PrimaryPreferenceCodeList();
			validCodes.RemoveCode(PrimaryPreferenceCodeList.Codes.J);
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_PrimaryPreferenceInfo, new ZString[] { "XX" }, validCodes.GetAllCodesZString());
				ValidationTestHelper.AssertWarningIfNotEntered(invoiceLine.JI_PrimaryPreferenceInfo, "Preference code is required. A value will be suggested on save.");
			});
		}

		public void TestCheckJI_Procedure()
		{
			RefCusProcedureHelper.CreateRefCusProcedureList(Factory);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "1";
			invoiceLine.JI_Procedure = ZString.Empty;
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_ProcedureInfo,
					new ZString[] { "8011", "6011", "7001", "4030" },
					new ZString[] { "1000", "1010", "1110", "1111" });
				AssertNoMessageErrors(invoiceLine.JI_ProcedureInfo);
			});
		}

		public void TestCheckJI_StateOrRegionOfOrigin_Export()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Norway;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo, "XX", "03");

				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo, "XX", "91");

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Denmark;
				AssertNoMessageErrors(invoiceLine.JI_StateOrRegionOfOriginInfo);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Denmark;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo, "03", "91");
			});
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "QQ", Core.Constants.CountryCodes.Norway);
		}

		protected override string MessageType => JobMessageTypeList.Codes.Export;
	}
}

