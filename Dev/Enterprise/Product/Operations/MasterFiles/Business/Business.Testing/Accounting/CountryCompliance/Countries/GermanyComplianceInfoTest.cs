using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(GermanyComplianceInfo))]
	sealed class GermanyComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Germany;

		protected override ZDate ExpectedEInvoicingComplianceDate => new ZDate(2020, 11, 27);

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		CodeDescriptionPairList ExpectedFiscalTaxCodesONT =>
			new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT21, FiscalOutputNetCodeList.Descriptions.ONT21),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT35, FiscalOutputNetCodeList.Descriptions.ONT35),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT41, FiscalOutputNetCodeList.Descriptions.ONT41),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT42, FiscalOutputNetCodeList.Descriptions.ONT42),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT43, FiscalOutputNetCodeList.Descriptions.ONT43),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT44, FiscalOutputNetCodeList.Descriptions.ONT44),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT45, FiscalOutputNetCodeList.Descriptions.ONT45),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT46, FiscalOutputNetCodeList.Descriptions.ONT46),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT48, FiscalOutputNetCodeList.Descriptions.ONT48),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT49, FiscalOutputNetCodeList.Descriptions.ONT49),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT60, FiscalOutputNetCodeList.Descriptions.ONT60),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT73, FiscalOutputNetCodeList.Descriptions.ONT73),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT76, FiscalOutputNetCodeList.Descriptions.ONT76),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT77, FiscalOutputNetCodeList.Descriptions.ONT77),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT81, FiscalOutputNetCodeList.Descriptions.ONT81),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT84, FiscalOutputNetCodeList.Descriptions.ONT84),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT86, FiscalOutputNetCodeList.Descriptions.ONT86),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT89, FiscalOutputNetCodeList.Descriptions.ONT89),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT91, FiscalOutputNetCodeList.Descriptions.ONT91),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT93, FiscalOutputNetCodeList.Descriptions.ONT93),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT94, FiscalOutputNetCodeList.Descriptions.ONT94),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONT95, FiscalOutputNetCodeList.Descriptions.ONT95),
				new CodeDescriptionPair(FiscalOutputNetCodeList.Codes.ONTNA, FiscalOutputNetCodeList.Descriptions.ONTNA)
			};

		CodeDescriptionPairList ExpectedFiscalTaxCodesOTX =>
			new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX36, FiscalOutputTaxCodeList.Descriptions.OTX36),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX80, FiscalOutputTaxCodeList.Descriptions.OTX80),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX98, FiscalOutputTaxCodeList.Descriptions.OTX98),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX96, FiscalOutputTaxCodeList.Descriptions.OTX96),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX47, FiscalOutputTaxCodeList.Descriptions.OTX47),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX74, FiscalOutputTaxCodeList.Descriptions.OTX74),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTX85, FiscalOutputTaxCodeList.Descriptions.OTX85),
				new CodeDescriptionPair(FiscalOutputTaxCodeList.Codes.OTXNA, FiscalOutputTaxCodeList.Descriptions.OTXNA)
			};

		CodeDescriptionPairList ExpectedFiscalTaxCodesITX =>
			new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX66, FiscalInputTaxCodeList.Descriptions.ITX66),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX61, FiscalInputTaxCodeList.Descriptions.ITX61),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX62, FiscalInputTaxCodeList.Descriptions.ITX62),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX67, FiscalInputTaxCodeList.Descriptions.ITX67),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX63, FiscalInputTaxCodeList.Descriptions.ITX63),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX59, FiscalInputTaxCodeList.Descriptions.ITX59),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX64, FiscalInputTaxCodeList.Descriptions.ITX64),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX65, FiscalInputTaxCodeList.Descriptions.ITX65),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITX69, FiscalInputTaxCodeList.Descriptions.ITX69),
				new CodeDescriptionPair(FiscalInputTaxCodeList.Codes.ITXNA, FiscalInputTaxCodeList.Descriptions.ITXNA)
			};

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "UST";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "MST";

			AssertEquals(expected, result);
		}

		public void TestGetIsGSTRegistered()
		{
			var result = Country.GetIsGSTRegistered(CountryCode);
			var expected = true;

			AssertEquals(expected, result);
		}

		public void TestIsCashBasisVAT()
		{
			var result = Country.IsGSTCashBasis(CountryCode);
			var expected = false;

			AssertEquals(expected, result);
		}

		public void TestFiscalTaxCodesOutputNet()
		{
			var fiscalTaxCodeProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(CountryCode);
			if (fiscalTaxCodeProvider != null)
			{
				var actualFiscalTaxCodesONT = fiscalTaxCodeProvider.GetFiscalTaxCodeForOutputNetAmount();

				CombineAssertions(() =>
				{
					foreach (CodeDescriptionPair ont in actualFiscalTaxCodesONT)
					{
						Assert(GetMessage(ont, "Not Expected"), ExpectedFiscalTaxCodesONT.IndexOf(ont) >= 0);
					}
					foreach (CodeDescriptionPair ecdp in ExpectedFiscalTaxCodesONT)
					{
						Assert(GetMessage(ecdp, "Expected"), actualFiscalTaxCodesONT.IndexOf(ecdp) >= 0);
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		public void TestFiscalTaxCodesOutputTax()
		{
			var fiscalTaxCodeProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(CountryCode);
			if (fiscalTaxCodeProvider != null)
			{
				var actualFiscalTaxCodesOTX = fiscalTaxCodeProvider.GetFiscalTaxCodeForOutputTaxAmount();

				CombineAssertions(() =>
				{
					foreach (CodeDescriptionPair otx in actualFiscalTaxCodesOTX)
					{
						Assert(GetMessage(otx, "Not Expected"), ExpectedFiscalTaxCodesOTX.IndexOf(otx) >= 0);
					}
					foreach (CodeDescriptionPair ecdp in ExpectedFiscalTaxCodesOTX)
					{
						Assert(GetMessage(ecdp, "Expected"), actualFiscalTaxCodesOTX.IndexOf(ecdp) >= 0);
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		public void TestFiscalTaxCodesInputTax()
		{
			var fiscalTaxCodeProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(CountryCode);
			if (fiscalTaxCodeProvider != null)
			{
				var actualFiscalTaxCodesITX = fiscalTaxCodeProvider.GetFiscalTaxCodeForInputTaxAmount();

				CombineAssertions(() =>
				{
					foreach (CodeDescriptionPair itx in actualFiscalTaxCodesITX)
					{
						Assert(GetMessage(itx, "Not Expected"), ExpectedFiscalTaxCodesITX.IndexOf(itx) >= 0);
					}
					foreach (CodeDescriptionPair ecdp in ExpectedFiscalTaxCodesITX)
					{
						Assert(GetMessage(ecdp, "Expected"), actualFiscalTaxCodesITX.IndexOf(ecdp) >= 0);
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		string GetMessage(CodeDescriptionPair item, string prefix)
		{
			return FormattableString.Invariant($"{prefix} - code: {item.Code}, description: {item.Description}");
		}
	}
}
