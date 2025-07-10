using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderValidation))]
	sealed class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Validation.Parent));
		}

		public void TestCheckJE_DeclarationIncoterm()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var targetInfo = entryHeader.CH_DeclarationIncotermInfo;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, IncoTerms.FreeCarrier, IncoTerms.CostInsuranceAndFreight);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, IncoTerms.CarriagePaidTo, IncoTerms.CostAndFreight);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, IncoTerms.CarriageAndInsurancePaidTo, IncoTerms.FreeOnBoard);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, IncoTerms.DeliveredAtTerminal, IncoTerms.CostAndInsurance);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, IncoTerms.DeliveredAtPlace, IncoTerms.FreeAlongsideShip);
			ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, IncoTerms.DeliveredDutyPaid, IncoTerms.ExWorks);
		}

		public void TestCheckDeclarationIncotermWtihCharges()
		{
			var entryHeaderBO = Factory.New<CusEntryHeaderForTesting>();
			entryHeaderBO.IsImportCoreExposed = false;
			var warningMessage = "依據預報貨物通關報關手冊之規定，實際交易條件為EXW，僅為貨物出廠價格者，運費(17)、保險費(18)欄不得填列。";
			entryHeaderBO.OverseasFreightExposed = 0m;
			entryHeaderBO.OverseasInsuranceExposed = 0m;
			entryHeaderBO.CH_DeclarationIncoterm = "EXW";
			AssertNoWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.OverseasFreightExposed = 1m;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertHasWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.OverseasFreightExposed = 0m;
			entryHeaderBO.OverseasInsuranceExposed = 1m;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertHasWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.IsImportCoreExposed = true;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertNoWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			warningMessage = "依據預報貨物通關報關手冊之規定，實際交易條件為CIF，除貨物本身之離岸價格外，包含該筆交易中離岸後之保險費用及運輸費用者，運費(17)欄及保險費(18)欄應填報。";
			entryHeaderBO.IsImportCoreExposed = false;
			entryHeaderBO.OverseasFreightExposed = 1m;
			entryHeaderBO.OverseasInsuranceExposed = 1m;
			entryHeaderBO.CH_DeclarationIncoterm = "CIF";
			AssertNoWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.OverseasFreightExposed = 0m;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertHasWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.OverseasInsuranceExposed = 0m;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertHasWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.OverseasFreightExposed = 1m;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertHasWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
			entryHeaderBO.IsImportCoreExposed = true;
			entryHeaderBO.Validation.ValidateCH_DeclarationIncoterm();
			AssertNoWarning(entryHeaderBO.CH_DeclarationIncotermInfo, warningMessage);
		}

		public void TestCheckCH_TotalNetWeightInKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice.JZ_IncoTerm = "FOB";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = "8419.20.00.00-5";
			invoiceLine.JI_Procedure = "50";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = "PCE";
			invoiceLine.JI_EnteredUnitPrice = 7738.2m;
			invoiceLine.JI_NetWeight = 1.125m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			declaration.JE_TotalWeight = 1.000m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			entryHeader.Validation.ValidateCH_TotalNetWeightInKilograms();
			AssertHasMessageError(entryHeader.CH_TotalNetWeightInKilogramsInfo, ValidationConstants.CusEntryHeader.NetWeightNotBeGreaterThanGrossWeight);
			declaration.JE_TotalWeight = 1.125m;
			entryHeader.Validation.ValidateCH_TotalNetWeightInKilograms();
			AssertNoMessageError(entryHeader.CH_TotalNetWeightInKilogramsInfo, ValidationConstants.CusEntryHeader.NetWeightNotBeGreaterThanGrossWeight);
		}

		class CusEntryHeaderForTesting : CusEntryHeader
		{
			public CusEntryHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZDecimal overseasFreightExposed;
			ZDecimal overseasInsuranceExposed;
			public ZDecimal OverseasFreightExposed
			{
				get
				{
					return overseasFreightExposed;
				}

				set
				{
					var oldValue = overseasFreightExposed;
					if (oldValue != value)
					{
						overseasFreightExposed = value;
						Factory.InvalidateCachedProperties();
					}
				}
			}

			public ZDecimal OverseasInsuranceExposed
			{
				get
				{
					return overseasInsuranceExposed;
				}

				set
				{
					var oldValue = overseasInsuranceExposed;
					if (oldValue != value)
					{
						overseasInsuranceExposed = value;
						Factory.InvalidateCachedProperties();
					}
				}
			}

			public bool IsImportCoreExposed
			{
				get;
				set;
			}

			public override Money OverseasFreight => new Money(OverseasFreightExposed, GlbCompany.CurrentCompany.LocalCurrency);
			public override Money OverseasInsurance => new Money(OverseasInsuranceExposed, GlbCompany.CurrentCompany.LocalCurrency);
			protected override bool IsImportCore()
			{
				return IsImportCoreExposed;
			}

			protected override bool IsExportCore()
			{
				return !IsImportCoreExposed;
			}
		}
	}
}
