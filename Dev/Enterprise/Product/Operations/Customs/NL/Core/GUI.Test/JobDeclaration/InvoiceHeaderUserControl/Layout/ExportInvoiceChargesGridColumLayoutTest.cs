using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ExportInvoiceChargesGridColumLayout))]
sealed class ExportInvoiceChargesGridColumLayoutTest : GridColumnLayoutProviderAbstractTest<ExportInvoiceChargesGridColumLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(nameof(IEUCommonInvoiceCharge.J7_ChargeType), typeof(ZDropEditColumnStyleInfo), 40),
		(nameof(IEUCommonInvoiceCharge.J7_Amount), typeof(ZCalcEditColumnStyleInfo), 80),
		(nameof(IEUCommonInvoiceCharge.J7_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyleInfo), 40),
		(nameof(IEUCommonInvoiceCharge.J7_ExchangeRate), typeof(ZCalcEditColumnStyleInfo), 80),
		(nameof(IEUCommonInvoiceCharge.AmountInLocalCurrency), typeof(ZCalcEditColumnStyleInfo), 80),
		(nameof(IEUCommonInvoiceCharge.J7_Percentage), typeof(ZCalcEditColumnStyleInfo), 74),
		(nameof(IEUCommonInvoiceCharge.J7_DistributeBy), typeof(ZDropEditColumnStyleInfo), 75),
		(nameof(IEUCommonInvoiceCharge.J7_IsIncludedInITOT), typeof(ZCheckBoxColumnStyleInfo), 85),
		(nameof(IEUCommonInvoiceCharge.J7_FullOrPartialApportionment), typeof(ZDropEditColumnStyleInfo), 90),
		(nameof(IEUCommonInvoiceCharge.J7_IsStatisticalValueApplicable), typeof(ZCheckBoxColumnStyleInfo), 100),
		(nameof(IEUCommonInvoiceCharge.AmountCorrection), typeof(ZCalcEditColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(IEUCommonInvoiceCharge);
}
