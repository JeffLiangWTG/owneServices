using System;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EntryCreationStrategy))]
sealed class EntryCreationStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
{
	public void TestGetKeyForLine_ForMergeByNotEqualToNoMerge()
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
		CombineAssertions(() =>
		{
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_ValuationCode, "ZZ");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_Procedure, "1234567890");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_Tariff, "333333333");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_CountryOfOrigin, "ZZ");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_StateOrRegionOfOrigin, "ZZZ");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_PrimaryPreference, "1234567890");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_ReducedCustomsFlag, "Z");
			AssertMergeKeyForLineContains<ZDecimal>(invoiceLine, x => x.JI_CustomsRateOverrideValue, 42.0m);
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_CustomsRateOverrideType, "ZZZZZ");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_ZZF_NKTaxType, "ZZZZ");
			AssertMergeKeyForLineContains<ZString>(invoiceLine, x => x.JI_MergeOverride, "1234567890");
		});
	}

	public void TestGetKeyForLine_SupportingDocumentsForMergeByNotEqualToNoMerge() => CombineAssertions(() =>
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;

		var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Code = "BB";
		supportingDocument1.CSI_ReferenceNumber = "123";

		var mergeKey = EntryCreationStrategy.GetKeyForLine(invoiceLine);

		AssertEquals("Supporting Document: CSI_Code should be part of MergeKey", expected: true, mergeKey.Contains((ZString)"BB"));
		AssertEquals("Supporting Document: CSI_ReferenceNumber should be part of MergeKey", expected: true, mergeKey.Contains((ZString)"123"));
	});

	public void TestGetKeyForLine_WithASVAsUoMAndMergeByNotEqualToNoMerge() => CombineAssertions(() =>
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;

		invoiceLine!.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength;
		invoiceLine!.JI_CustomsQuantity = 15;
		AssertEquals("JI_CustomsQuantity should be part of MergeKey", expected: true, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains((ZDecimal)15));

		ResetAllQuantitiesAndUnits();
		invoiceLine!.JI_CustomsSecondUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength;
		invoiceLine!.JI_CustomsSecondQuantity = 18;
		AssertEquals("JI_CustomsSecondQuantity should be part of MergeKey", expected: true, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains((ZDecimal)18));

		ResetAllQuantitiesAndUnits();
		invoiceLine!.JI_CustomsThirdUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength;
		invoiceLine!.JI_CustomsThirdQuantity = 22.5m;
		AssertEquals("JI_CustomsThirdQuantity should be part of MergeKey", expected: true, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains((ZDecimal)22.5));

		ResetAllQuantitiesAndUnits();
		invoiceLine!.JI_CustomsFourthUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength;
		invoiceLine!.JI_CustomsFourthQuantity = 12.5m;
		AssertEquals("JI_CustomsFourthQuantity should be part of MergeKey", expected: true, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains((ZDecimal)12.5));

		ResetAllQuantitiesAndUnits();
		invoiceLine!.JI_CustomsFifthUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength;
		invoiceLine!.JI_CustomsFifthQuantity = 1.5m;
		AssertEquals("JI_CustomsFifthQuantity should be part of MergeKey", expected: true, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains((ZDecimal)1.5));

		void ResetAllQuantitiesAndUnits()
		{
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;

			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsSecondQuantity = ZDecimal.Zero;

			invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;

			invoiceLine.JI_CustomsFourthUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsFourthQuantity = ZDecimal.Zero;

			invoiceLine.JI_CustomsFifthUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsFifthQuantity = ZDecimal.Zero;
		}
	});

	public void TestGetKeyForLine_WhenMergeByIsNotMerge() => CombineAssertions(() =>
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		var mergeKeys = EntryCreationStrategy.GetKeyForLine(invoiceLine);
		AssertEquals("Merge Keys Count", 5, mergeKeys.Keys.Count);
		AssertEquals("JI_PK should be part of MergeKey", expected: true, mergeKeys.Contains(invoiceLine!.PK));
	});

	void AssertMergeKeyForLineContains<TValue>(JobComInvoiceLine invoiceLine, Expression<Func<JobComInvoiceLine, TValue>> valueSelector, TValue validValue, TValue emptyValue = default)
		where TValue : IZType
	{
		var propInfo = (valueSelector.Body as MemberExpression).Member as PropertyInfo;
		var propName = propInfo.Name;
		var getter = () => (TValue)propInfo.GetValue(invoiceLine);
		var setter = (TValue value) => propInfo.SetValue(invoiceLine, value, null);
		var originalValue = getter();
		setter(validValue);
		AssertEquals($"{propName} should contain value when set", expected: true, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains(validValue));
		setter(emptyValue);
		AssertEquals($"{propName} should not contain value when empty", expected: false, EntryCreationStrategy.GetKeyForLine(invoiceLine).Contains(validValue));
		setter(originalValue);
	}

	public override void TestGetKeyForHeader()
	{
		var entryManager = new EntryManager(declaration, EntryCreationStrategy);
		var provider = declaration.CustomsEntryInstructionProvider;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_ExportDate = new ZDateTime(2023, 09, 29);
			var mergeKey = EntryCreationStrategy.GetKeyForHeader(invoiceLine);
			Assert("Merge Key should contain header's Valuation Date", mergeKey.Contains(declaration.JE_ExportDate));
			Assert("Merge Key should contain header's Valuation Method", mergeKey.Contains(invoice.JZ_ValuationMethod));

			var entryLine = entryManager.GetOrCreateEntryLine(invoiceLine);
			AssertEquals("Entry line Instruction empty if not provided", ZGuid.Empty, entryLine.Header.CH_CEI_Instruction);

			var entryInstr = provider.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstr.PK;
			entryLine = entryManager.GetOrCreateEntryLine(invoiceLine);
			AssertEquals("Entry line Instruction populated if provided", entryInstr.PK, entryLine.Header.CH_CEI_Instruction);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
	EntryCreationStrategy EntryCreationStrategy => declaration.CreateEntryCreationStrategy();
}
