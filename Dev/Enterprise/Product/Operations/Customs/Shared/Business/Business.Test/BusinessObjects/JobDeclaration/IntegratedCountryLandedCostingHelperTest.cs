using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	sealed class IntegratedCountryLandedCostingHelperTest : TestCaseWithFactory
	{
		public void TestLandedCostCalculator()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					foreach (var countryCode in IntegratedCountryHelper.CustomsWareCountryCodes)
					{
						AssertLandedCostCalculate(countryCode, FeeTypeList.Codes.A00, "CustomsWareCountry");
					}

					foreach (var countryCode in IntegratedCountryHelper.HasBuiltInDeclarationCountryCodes)
					{
						if (!IntegratedCountryHelper.CustomsWareCountryCodes.Contains(countryCode))
						{
							var countryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
							if (ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryOfJurisdiction) || countryOfJurisdiction == Core.Constants.CountryCodes.Switzerland)
							{
								AssertLandedCostCalculate(countryOfJurisdiction, FeeTypeList.Codes.A00, "NonCustomsWareInstallationsCountry, IsInEuropeanCustomsUnionOrInheritsFromEU");
							}
							else
							{
								AssertLandedCostCalculate(countryOfJurisdiction, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, "NonCustomsWareInstallationsCountry, not IsInEuropeanCustomsUnionOrInheritsFromEU");
							}
						}
					}
				}
			});
		}

		void AssertLandedCostCalculate(string countryCode, string dutyCode, string assertionMessage)
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, countryCode, Universal.Constants.RateTypes.Duty, dutyCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				if (IntegratedCountryHelper.HasBuiltInDeclarationCountryCodes.Contains(countryCode) || IntegratedCountryHelper.CountryHasDeclarationInDevelopment(countryCode))
				{
					declaration.JE_ApplicationCode = "ITF";
				}

				if (countryCode == Core.Constants.CountryCodes.Singapore)
				{
					declaration.JE_MessageType = "IPT";
				}
				else if (countryCode == Core.Constants.CountryCodes.NewZealand)
				{
					declaration.JE_MessageType = "IMP";
				}

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine11 = entryHeader1.MergedLines.AddNew();
				entryLine11.CL_CustomsValue = 1000m;
				var entryLine12 = entryHeader1.MergedLines.AddNew();
				entryLine12.CL_CustomsValue = 1000m;

				var entryHeader2 = CountrySupportsMultipleEntryHeaders(countryCode) ? declaration.CustomsEntryHeaders.AddNew() : entryHeader1;
				var entryLine21 = entryHeader2.MergedLines.AddNew();
				entryLine21.CL_CustomsValue = 1000m;

				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 2000m;

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 2000m;

				if (countryCode == Core.Constants.CountryCodes.Australia)
				{
					invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
					invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				}

				var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine11.JI_CL = entryLine11.PK;
				invoiceLine11.JI_LinePrice = 1000m;

				var invoiceLine12 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine12.JI_CL = entryLine12.PK;
				invoiceLine12.JI_LinePrice = 1000m;

				var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine21.JI_CL = entryLine21.PK;
				invoiceLine21.JI_LinePrice = 1000m;

				var invoiceLine22 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine22.JI_CL = entryLine21.PK;
				invoiceLine22.JI_LinePrice = 1000m;

				entryLine11.Fees.GetOrAddFeeByFeeType(dutyCode).CF_ChargeAmount = 100m;
				entryLine12.Fees.GetOrAddFeeByFeeType(dutyCode).CF_ChargeAmount = 100m;
				entryLine21.Fees.GetOrAddFeeByFeeType(dutyCode).CF_ChargeAmount = 100m;

				AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Customs Value for invoice line11", 1000m, ((IUltimateDistributee)invoiceLine11).CustomsValue);
				AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Customs Value for invoice line12", 1000m, ((IUltimateDistributee)invoiceLine12).CustomsValue);
				if (countryCode == Core.Constants.CountryCodes.Australia || countryCode == Core.Constants.CountryCodes.Canada)
				{
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Customs Value for invoice line21, Because ShouldUseBackRoundedInvoiceLineCV is false", 1000m, ((IUltimateDistributee)invoiceLine21).CustomsValue);
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Customs Value for invoice line22, Because ShouldUseBackRoundedInvoiceLineCV is false", 1000m, ((IUltimateDistributee)invoiceLine22).CustomsValue);
				}
				else
				{
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Customs Value for invoice line21", 500m, ((IUltimateDistributee)invoiceLine21).CustomsValue);
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Customs Value for invoice line22", 500m, ((IUltimateDistributee)invoiceLine22).CustomsValue);
				}
				if (countryCode != Core.Constants.CountryCodes.UnitedStates)
				{
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Total Duty Amount for declaration", 300m, ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems["TDT"]);
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Duty Amount for invoice line11", 100m, ((IUltimateDistributee)invoiceLine11).LineDutyTaxEntryFeeItems["TDT"]);
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Duty Amount for invoice line12", 100m, ((IUltimateDistributee)invoiceLine12).LineDutyTaxEntryFeeItems["TDT"]);
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Duty Amount for invoice line21", 50m, ((IUltimateDistributee)invoiceLine21).LineDutyTaxEntryFeeItems["TDT"]);
					AssertEquals($"[{countryCode}] ({assertionMessage})-LandedCost Duty Amount for invoice line22", 50m, ((IUltimateDistributee)invoiceLine22).LineDutyTaxEntryFeeItems["TDT"]);
				}
			}
		}

		bool CountrySupportsMultipleEntryHeaders(string countryCode)
		{
			return countryCode != Core.Constants.CountryCodes.NewZealand;
		}

		public void TestAppointmentOfCusEntryLineValues_OnlyOneInvoiceLine()
		{
			using (TemporarilySetCountryToChinaAndInterfaced())
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine11 = entryHeader1.MergedLines.AddNew();
				entryLine11.CL_CustomsValue = 1000m;

				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 1000m;
				var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine11.JI_CL = entryLine11.PK;

				entryLine11.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100m;

				AssertEquals("LandedCost Customs Value for invoice line11", 1000m, ((IUltimateDistributee)invoiceLine11).CustomsValue);
				AssertEquals("LandedCost Duty Amount for invoice line11", 100m, ((IUltimateDistributee)invoiceLine11).LineDutyTaxEntryFeeItems["TDT"]);
			}
		}

		public void TestAppointmentOfCusEntryLineValues_DifferentCurrency()
		{
			using (TemporarilySetCountryToChinaAndInterfaced())
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine11 = entryHeader1.MergedLines.AddNew();
				entryLine11.CL_CustomsValue = 1000m;

				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_RX_NKInvoice_Currency = "XXX";
				invoice1.JZ_InvoiceAmount = 1000m;

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_RX_NKInvoice_Currency = "YYY";
				invoice2.JZ_InvoiceAmount = 2000m;

				var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine11.JI_CL = entryLine11.PK;
				invoiceLine11.JI_LinePrice = 1000m;

				var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine21.JI_CL = entryLine11.PK;
				invoiceLine21.JI_LinePrice = 1000m;

				entryLine11.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100m;

				AssertEquals("LandedCost Customs Value for invoice line11", 0m, ((IUltimateDistributee)invoiceLine11).CustomsValue);
				AssertEquals("LandedCost Customs Value for invoice line21", 0m, ((IUltimateDistributee)invoiceLine21).CustomsValue);

				AssertEquals("LandedCost Duty Amount for invoice line11", 0m, ((IUltimateDistributee)invoiceLine11).LineDutyTaxEntryFeeItems["TDT"]);
				AssertEquals("LandedCost Duty Amount for invoice line21", 0m, ((IUltimateDistributee)invoiceLine21).LineDutyTaxEntryFeeItems["TDT"]);
			}
		}

		public void TestAppointmentOfCusEntryLineValues_DifferentIncoTerm()
		{
			using (TemporarilySetCountryToChinaAndInterfaced())
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine11 = entryHeader1.MergedLines.AddNew();
				entryLine11.CL_CustomsValue = 1000m;

				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_IncoTerm = "";
				invoice1.JZ_InvoiceAmount = 1000m;

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_IncoTerm = "CIF";
				invoice2.JZ_InvoiceAmount = 1000m;

				var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine11.JI_CL = entryLine11.PK;
				invoiceLine11.JI_LinePrice = 1000m;

				var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine21.JI_CL = entryLine11.PK;
				invoiceLine21.JI_LinePrice = 1000m;

				entryLine11.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100m;

				AssertEquals("LandedCost Customs Value for invoice line11", 0m, ((IUltimateDistributee)invoiceLine11).CustomsValue);
				AssertEquals("LandedCost Customs Value for invoice line21", 0m, ((IUltimateDistributee)invoiceLine21).CustomsValue);

				AssertEquals("LandedCost Duty Amount for invoice line11", 0m, ((IUltimateDistributee)invoiceLine11).LineDutyTaxEntryFeeItems["TDT"]);
				AssertEquals("LandedCost Duty Amount for invoice line21", 0m, ((IUltimateDistributee)invoiceLine21).LineDutyTaxEntryFeeItems["TDT"]);
			}
		}

		public void TestAppointmentOfCusEntryLineValues_BackRounding()
		{
			using (TemporarilySetCountryToChinaAndInterfaced())
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine11 = entryHeader1.MergedLines.AddNew();
				entryLine11.CL_CustomsValue = 1000m;

				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 1000m;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 2000m;

				var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine11.JI_CL = entryLine11.PK;
				invoiceLine11.JI_LinePrice = 1000m;

				var invoiceLine12 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine12.JI_CL = entryLine11.PK;
				invoiceLine12.JI_LinePrice = 1000m;

				var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine21.JI_CL = entryLine11.PK;
				invoiceLine21.JI_LinePrice = 1000m;

				entryLine11.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100m;

				AssertEquals("LandedCost Customs Value for invoice line11", 333.34m, ((IUltimateDistributee)invoiceLine11).CustomsValue);
				AssertEquals("LandedCost Customs Value for invoice line12", 333.33m, ((IUltimateDistributee)invoiceLine12).CustomsValue);
				AssertEquals("LandedCost Customs Value for invoice line21", 333.33m, ((IUltimateDistributee)invoiceLine21).CustomsValue);

				AssertEquals("LandedCost Duty Amount for invoice line11", 33.334m, ((IUltimateDistributee)invoiceLine11).LineDutyTaxEntryFeeItems["TDT"]);
				AssertEquals("LandedCost Duty Amount for invoice line12", 33.333m, ((IUltimateDistributee)invoiceLine12).LineDutyTaxEntryFeeItems["TDT"]);
				AssertEquals("LandedCost Duty Amount for invoice line21", 33.333m, ((IUltimateDistributee)invoiceLine21).LineDutyTaxEntryFeeItems["TDT"]);
			}
		}

		static IDisposable TemporarilySetCountryToChinaAndInterfaced()
		{
			IDisposable temporarilySetCountryToChina = null;
			IDisposable setLocalCountryCustomsInterface = null;
			return new DisposableAction(() =>
			{
				temporarilySetCountryToChina = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China);
				var customsInterface = new LocalCountryCustomsInterface
				{
					RecipientID = "RecipientID",
					SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
				};
				setLocalCountryCustomsInterface = DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			}, () =>
			{
				setLocalCountryCustomsInterface.Dispose();
				temporarilySetCountryToChina.Dispose();
			});
		}
	}
}
