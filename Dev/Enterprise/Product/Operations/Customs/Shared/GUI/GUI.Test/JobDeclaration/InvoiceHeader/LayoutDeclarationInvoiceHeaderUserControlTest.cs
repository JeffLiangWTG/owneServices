using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(LayoutDeclarationInvoiceHeaderUserControl))]
	sealed class LayoutDeclarationInvoiceHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<LayoutDeclarationInvoiceHeaderUserControl, BaseJobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestInvoiceChargesGridColumnLayoutApplied() => CombineAssertions(() =>
		{
			AssertChargesGridColumns(c => c.InvoiceChargesGrid, DefaultInvoiceChargesGridColumns);
			AssertChargesGridColumns(c => c.InvoiceChargesGrid, TestingLayoutGridColumns, invoiceChargesGridColumnLayout: new InvoiceChargesGridColumnLayoutForTesting());
		});

		public void TestGroupChargesGridColumnLayoutApplied() => CombineAssertions(() =>
		{
			AssertChargesGridColumns(c => c.BaseGroupChargesGrid, DefaultGroupChargesGridColumns);
			AssertChargesGridColumns(c => c.BaseGroupChargesGrid, TestingLayoutGridColumns, groupChargesGridColumnLayout: new InvoiceChargesGridColumnLayoutForTesting());
		});

		public void TestApportionedChargesGridColumnLayoutApplied() => CombineAssertions(() =>
		{
			AssertChargesGridColumns(c => c.ApportionedChargesGrid, DefaultApportionedChargesGridColumns);
			AssertChargesGridColumns(c => c.ApportionedChargesGrid, TestingLayoutGridColumns, apportionedChargesGridColumnLayout: new InvoiceChargesGridColumnLayoutForTesting());
		});

		void AssertChargesGridColumns(Func<LayoutCustomsSupplierHeaderUserControl, ZGrid> getGrid, string[] expectedColumns, IGridColumnLayoutProvider invoiceChargesGridColumnLayout = null, IGridColumnLayoutProvider groupChargesGridColumnLayout = null, IGridColumnLayoutProvider apportionedChargesGridColumnLayout = null, [CallerLineNumber] int line = 0)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.Charges.AddNew();
			invoice.GroupCharges.AddNew();
			declaration.TopGroupInvoice.Charges.AddNew();

			var providerMock = new Mock<IDeclarationFormLayoutProvider>();
			providerMock.Setup(x => x.GetInvoiceChargesGridColumnLayout(It.IsAny<BaseJobDeclaration>())).Returns(invoiceChargesGridColumnLayout);
			providerMock.Setup(x => x.GetGroupChargesGridColumnLayout(It.IsAny<BaseJobDeclaration>())).Returns(groupChargesGridColumnLayout);
			providerMock.Setup(x => x.GetApportionedInvoiceChargesGridColumnLayout(It.IsAny<BaseJobDeclaration>())).Returns(apportionedChargesGridColumnLayout);
			var objectHandle = Mock.Of<ObjectHandle>(x => x.GetObject() == providerMock.Object);
			var hashtable = new Hashtable { { "Default", objectHandle } };

			using (ObjectFactory.Substitute("DeclarationFormLayoutProviders", hashtable))
			using (var control = new LayoutDeclarationInvoiceHeaderUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				AssertContainsExactElementsInExactOrder($"[{line}]", expectedColumns, getGrid(control).ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		string[] DefaultInvoiceChargesGridColumns => [nameof(JobComInvCharge.J7_ChargeType), nameof(JobComInvCharge.ChargeCodeDescription), nameof(JobComInvCharge.J7_Amount), nameof(JobComInvCharge.J7_RX_NKCurrency), nameof(JobComInvCharge.J7_IsIncludedInITOT), nameof(JobComInvCharge.J7_IsDutiable), nameof(JobComInvCharge.J7_IsGSTApplicable), nameof(JobComInvCharge.J7_Calc_IsIncludedInInvoiceAmount), nameof(JobComInvCharge.J7_PrepaidCollect), nameof(JobComInvCharge.J7_Percentage), nameof(JobComInvCharge.J7_DistributeBy)];
		string[] DefaultGroupChargesGridColumns => [nameof(JobComInvCharge.J7_ChargeType), nameof(JobComInvCharge.ChargeCodeDescription), nameof(JobComInvCharge.J7_Amount), nameof(JobComInvCharge.J7_RX_NKCurrency), nameof(JobComInvCharge.J7_IsDutiable), nameof(JobComInvCharge.J7_IsGSTApplicable), nameof(JobComInvCharge.J7_Percentage), nameof(JobComInvCharge.J7_PrepaidCollect), nameof(JobComInvCharge.J7_DistributeBy), nameof(JobComInvCharge.J7_FullOrPartialApportionment), nameof(BaseGroupInvoiceCharge.J7_Calc_IsIncludedInITOT)];
		string[] DefaultApportionedChargesGridColumns => [nameof(JobComInvCharge.J7_ChargeType), nameof(JobComInvCharge.ChargeCodeDescription), nameof(JobComInvCharge.J7_Amount), nameof(JobComInvCharge.J7_RX_NKCurrency), nameof(JobComInvCharge.J7_IsDutiable), nameof(JobComInvCharge.J7_IsGSTApplicable), nameof(JobComInvCharge.J7_IsIncludedInITOT), nameof(JobComInvCharge.J7_Calc_IsIncludedInInvoiceAmount), nameof(JobComInvCharge.J7_FullOrPartialApportionment)];
		string[] TestingLayoutGridColumns => [nameof(JobComInvCharge.J7_ChargeType)];

		class InvoiceChargesGridColumnLayoutForTesting : IGridColumnLayoutProvider
		{
			public IGridColumnLayout Layout => layout ??= CreateLayout();
			IGridColumnLayout layout;

			IGridColumnLayout CreateLayout()
			{
				var builder = GridColumnLayoutBuilder.Create();
				builder.AddColumn(new GridColumnReference<ZDropEditColumnStyleInfo>(nameof(JobComInvCharge.J7_ChargeType), 40));
				return builder.Build();
			}
		}
	}
}
