using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseJobComInvoiceHeader))]
	public abstract class BaseJobComInvoiceHeaderAbstractTest<TInvoiceHeader, TInvoiceLine> : EnterpriseBusinessObjectTestCase
			where TInvoiceHeader : BaseJobComInvoiceHeader
			where TInvoiceLine : BaseJobComInvoiceLine
	{
		public void TestJZ_Calc_LinesEntered_Performance()
		{
			var declaration = (BaseJobDeclaration)Factory.New(GetDeclarationType());
			var header = Factory.New<TInvoiceHeader>();
			declaration.Invoices.Add(header);
			var invoiceLineMock = Factory.NewMoq<TInvoiceLine>();
			invoiceLineMock.Setup(m => m.LinePriceForBalanceCalc).Returns(ZDecimal.Zero);
			var invoiceLine = (BaseJobComInvoiceLine)invoiceLineMock.Object;
			invoiceLine.JI_JZ = header.PK;
			header.InvoiceLines.Add(invoiceLine);

			AssertNoExceptionThrown(() =>
			{
				foreach (int num in Enumerable.Range(0, 10))
				{
					_ = header.JZ_Calc_LinesEntered;
					Factory.InvalidateCachedProperties();
				}
			});
		}

		public void TestJZ_Calc_LinesEntered_RecalculateWhenInvoiceLinesRemoved()
		{
			var declaration = (BaseJobDeclaration)Factory.New(GetDeclarationType());
			var invoice = Factory.New<TInvoiceHeader>();
			declaration.Invoices.Add(invoice);
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100;
			var invoiceLine2 = (BaseJobComInvoiceLine)Factory.New(invoiceLine1.GetType());
			invoiceLine2.JI_LinePrice = 200;
			invoiceLine2.JI_JZ = invoice.PK;
			invoice.JobComInvoiceLines.Add(invoiceLine2);
			AssertEquals("invoice1.JZ_Calc_LinesEntered", 300m, invoice.JZ_Calc_LinesEntered);

			invoice.JobComInvoiceLines.RemoveAndDelete(invoiceLine2);
			AssertEquals("invoice1.JZ_Calc_LinesEntered", 100m, invoice.JZ_Calc_LinesEntered);
		}

		public void TestJZ_Calc_LinesEntered_RecalculateWhenInvoiceLinesAdded()
		{
			var declaration = (BaseJobDeclaration)Factory.New(GetDeclarationType());
			var invoice1 = Factory.New<TInvoiceHeader>();
			declaration.Invoices.Add(invoice1);
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100;
			AssertEquals("invoice1.JZ_Calc_LinesEntered", 100m, invoice1.JZ_Calc_LinesEntered);

			var invoiceLine2 = (BaseJobComInvoiceLine)Factory.New(invoiceLine1.GetType());
			invoiceLine2.JI_LinePrice = 150;
			invoiceLine2.JI_JZ = invoice1.PK;
			invoice1.JobComInvoiceLines.Add(invoiceLine2);
			AssertEquals("invoice1.JZ_Calc_LinesEntered", 250m, invoice1.JZ_Calc_LinesEntered);

			var invoice2 = Factory.New<TInvoiceHeader>();
			declaration.Invoices.Add(invoice2);
			AssertEquals("invoice1.JZ_Calc_LinesEntered", 250m, invoice1.JZ_Calc_LinesEntered);
			AssertEquals("invoice2.JZ_Calc_LinesEntered", 0m, invoice2.JZ_Calc_LinesEntered);
			invoiceLine2.JI_JZ = invoice2.PK;
			AssertEquals("invoice1.JZ_Calc_LinesEntered", 100m, invoice1.JZ_Calc_LinesEntered);
			AssertEquals("invoice2.JZ_Calc_LinesEntered", 150m, invoice2.JZ_Calc_LinesEntered);
		}

		public void TestJZ_Calc_LinesEntered_RecalculateWhenDeclarationIsChanged()
		{
			var declaration = (BaseJobDeclaration)Factory.New(GetDeclarationType());
			var header = Factory.New<TInvoiceHeader>();
			var invoiceLine1 = (BaseJobComInvoiceLine)Factory.New(GetInvoiceLineType(header));
			header.JobComInvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_LinePrice = 100;
			AssertEquals(0m, header.JZ_Calc_LinesEntered);

			declaration.Invoices.Add(header);
			AssertEquals(100m, header.JZ_Calc_LinesEntered);
		}

		public void TestJZ_Calc_LinesEntered_RecalculateWhenRelatedPropertiesChange()
		{
			var declaration = (BaseJobDeclaration)Factory.New(GetDeclarationType());
			var header = Factory.New<TInvoiceHeader>();
			declaration.Invoices.Add(header);
			var invoiceLineMock = Factory.NewMoq<TInvoiceLine>();
			var cnt = 0;
			invoiceLineMock.Setup(m => m.LinePriceForBalanceCalc).Returns(ZDecimal.Zero).Callback(() => cnt++);
			var invoiceLine = (BaseJobComInvoiceLine)invoiceLineMock.Object;
			invoiceLine.JI_JZ = header.PK;
			header.InvoiceLines.Add(invoiceLine);

			var expectedCnt = 1;
			foreach (var func in GetJZ_Calc_LinesEnteredRelatedProperties())
			{
				var (propertyInfo, value) = func(declaration, header, invoiceLine);
				propertyInfo.Value = value;
				_ = header.JZ_Calc_LinesEntered;
				AssertEquals($"Changing the value of property {propertyInfo.Name} should invalidate the cache.", expectedCnt++, cnt);

				propertyInfo.Value = propertyInfo.DefaultValue;
				propertyInfo.Value = value;

				_ = header.JZ_Calc_LinesEntered;
				AssertEquals($"Changing the value of property {propertyInfo.Name} should invalidate the cache.", expectedCnt++, cnt);
			}
		}

		Type GetDeclarationType() => typeof(TInvoiceHeader).GetProperty(nameof(BaseJobComInvoiceHeader.JobDeclaration), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.PropertyType
										?? typeof(BaseJobDeclaration);

		Type GetInvoiceLineType(BaseJobComInvoiceHeader header) => (header as IInvoiceLineTypeSupporter)?.InvoiceLineType ?? typeof(BaseJobComInvoiceLine);

		protected virtual IEnumerable<Func<BaseJobDeclaration, TInvoiceHeader, BaseJobComInvoiceLine, (ZPropertyInfo, IZType)>> GetJZ_Calc_LinesEnteredRelatedProperties()
		{
			yield return (declaration, invoiceHeader, invoiceLine) => (invoiceLine.JI_LinePriceInfo, new ZDecimal(100m));
		}

		public void TestIsReciprocalRates()
		{
			foreach (var countryCode in CountryCodesForIsReciprocalRatesTest)
			{
				AssertIsReciprocalRates(countryCode);
			}
		}

		protected virtual List<string> CountryCodesForIsReciprocalRatesTest => new List<string> { GlbCompany.CurrentCompany.GC_RN_NKCountryCode };

		protected override Type ExpectedMetadataType => typeof(TInvoiceHeader);

		void AssertIsReciprocalRates(string countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var testDec = Factory.New<BaseJobDeclaration>();
				var invoice = testDec.Invoices.AddNew();
				AssertEquals("Invoice IsReciprocalRates for country:" + countryCode, testDec.IsReciprocalRates, invoice.IsReciprocalRates);

				var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
				AssertEquals("IsReciprocalRates default to GlbCompany.CurrentCompany.GC_IsReciprocal when JobDeclaration is null, for country:" + countryCode, GlbCompany.CurrentCompany.GC_IsReciprocal, invoice1.IsReciprocalRates);
			}
		}
	}
}
