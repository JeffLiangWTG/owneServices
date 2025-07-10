using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccComplianceSequenceTaiwanValidationTest : AccComplianceSequenceValidationTest
	{
		public void TestTWComplianceSubTypesToBeReported()
		{
			var validation = new AccComplianceSequenceTaiwanValidation(newSequence);
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TSX));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TSD));
			Assert(validation.TWComplianceSubTypesToBeReported_ForTestOnly.ContainsCode(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXS));
		}

		public new void TestValidateXD_Prefix()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				newSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
				newSequence.XD_Prefix = "AA";
				Assert(!newSequence.XD_PrefixInfo.HasErrors());

				newSequence.XD_Prefix = "1A";
				Assert(newSequence.XD_PrefixInfo.HasError("Series prefix must be 2 alpha-characters in length for TW company for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS"));

				newSequence.XD_Prefix = "11";
				Assert(newSequence.XD_PrefixInfo.HasError("Series prefix must be 2 alpha-characters in length for TW company for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS"));

				newSequence.XD_Prefix = "AAA";
				Assert(newSequence.XD_PrefixInfo.HasError("Series prefix must be 2 alpha-characters in length for TW company for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS"));

				newSequence.XD_Prefix = "A";
				Assert(newSequence.XD_PrefixInfo.HasError("Series prefix must be 2 alpha-characters in length for TW company for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS"));
			}
		}

		public void TestValidateXD_MaximumNumberDigits()
		{
			AssertNotEquals(CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
			{
				newSequence.XD_SequenceClass = x;
				newSequence.XD_MaximumNumberDigits = 9;
				newSequence.Validation.ValidateXD_MaximumNumberDigits();
				Assert(!newSequence.XD_MaximumNumberDigitsInfo.HasErrors());
			});

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
				{
					newSequence.XD_SequenceClass = x;
					newSequence.XD_MaximumNumberDigits = 9;
					newSequence.Validation.ValidateXD_MaximumNumberDigits();
					Assert(newSequence.XD_MaximumNumberDigitsInfo.HasError("Max number digits must equal to 8 for TW company for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS"));
				});

				TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
				{
					newSequence.XD_SequenceClass = x;
					newSequence.XD_MaximumNumberDigits = 8;
					newSequence.Validation.ValidateXD_MaximumNumberDigits();
					Assert(!newSequence.XD_MaximumNumberDigitsInfo.HasErrors());
				});
			}
		}

		public void TestValidatXD_StartDateWithComplianceSubType()
		{
			string startDateCanNotBeEmpty = "Valid From can not be empty for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS";

			AssertNotEquals(CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			newSequence.XD_StartDate = ZDate.Empty;
			TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
			{
				newSequence.XD_SequenceClass = x;
				Assert(!newSequence.XD_StartDateInfo.HasErrors());
			});

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				AssertEquals(CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				newSequence.XD_StartDate = ZDate.Empty;
				TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
				{
					newSequence.XD_SequenceClass = x;
					Assert(newSequence.XD_StartDateInfo.HasError(startDateCanNotBeEmpty));
				});

				newSequence.XD_StartDate = ZDate.Today;
				TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
				{
					newSequence.XD_SequenceClass = x;
					Assert(!newSequence.XD_StartDateInfo.HasErrors());
				});
			}
		}

		public void TestValidatXD_ExpiryDateWithComplianceSubType()
		{
			string expiryDateCanNotBeEmpty = "Expiry Date can not be empty for these compliance sub types: TXI, TXP, TDI, TDC, TDP, TCR, TCE, TCD, TXE, TXC, TSX, TSD, TXS";

			AssertNotEquals(CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			newSequence.XD_ExpiryDate = ZDate.Empty;
			TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
			{
				newSequence.XD_SequenceClass = x;
				Assert(!newSequence.XD_ExpiryDateInfo.HasError(expiryDateCanNotBeEmpty));
			});

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				AssertEquals(CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				newSequence.XD_ExpiryDate = ZDate.Empty;
				TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
				{
					newSequence.XD_SequenceClass = x;
					var r = newSequence.XD_ExpiryDateInfo.GetErrors();
					Assert(newSequence.XD_ExpiryDateInfo.HasError(expiryDateCanNotBeEmpty));
				});

				newSequence.XD_ExpiryDate = ZDate.Today;
				TWComplianceSubTypesToBeReported.GetAllCodes().ForEach((x) =>
				{
					newSequence.XD_SequenceClass = x;
					Assert(!newSequence.XD_ExpiryDateInfo.HasError(expiryDateCanNotBeEmpty));
				});
			}
		}

		public void TestModifyAfterAllocated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				AssertEquals(CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				existedSequence.XD_Prefix = "AA";
				TestFactory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var sequence = newFactory.Load<AccComplianceSequence>(existedSequence.PK);
				sequence.XD_NextNumber = sequence.XD_StartNumber + 1;
				newFactory.Save();

				Assert(sequence.IsAllocated);
				Assert(!existedSequence.IsAllocated);

				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
				sequence.RunPreSaveValidation();
				Assert(sequence.RowErrors.Contains("Sub Type can not be modified after allocated"));

				sequence.XD_Prefix = "BB";
				sequence.RunPreSaveValidation();
				Assert(sequence.RowErrors.Contains("Sub Type can not be modified after allocated"));
				Assert(sequence.RowErrors.Contains("Series Prefix can not be modified after allocated"));

				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.RunPreSaveValidation();
				Assert(sequence.RowErrors.Contains("Series Prefix can not be modified after allocated"));

				sequence.XD_Prefix = "AA";
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
				sequence.RunPreSaveValidation();
				Assert(sequence.RowErrors.Contains("Sub Type can not be modified after allocated"));

				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.RunPreSaveValidation();
				Assert(!sequence.HasRowErrors);
			}
		}

		CodeDescriptionPairList TWComplianceSubTypesToBeReported => Validation.TWComplianceSubTypesToBeReported_ForTestOnly;

		AccComplianceSequenceTaiwanValidation Validation
		{
			get
			{
				if (validation == null)
				{
					validation = new AccComplianceSequenceTaiwanValidation(newSequence);
				}
				return validation;
			}
		}
		AccComplianceSequenceTaiwanValidation validation;
	}
}
