using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConditionCalcDataForInvoiceLineTest : TestCaseWithFactory
	{
		public void TestUnitOfMeasureValueList()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_IncoTerm = "FOB";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 150m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 2000;
			invoiceLine.JI_CustomsSecondUnitQty = "MTQ";
			invoiceLine.JI_CustomsSecondQuantity = 100;
			invoiceLine.JI_CustomsThirdUnitQty = "PKG";
			invoiceLine.JI_CustomsThirdQuantity = 10;
			var calcData = new ConditionCalcDataForInvoiceLineForTest(invoiceLine);

			CombineAssertions(() =>
			{
				AssertEquals("CustomsValue", 0m, calcData.CustomsValue);
				AssertEquals("ValueForDuty", 0m, calcData.ValueForDuty);
				AssertEquals("[KG]", 2000m, calcData.UnitOfMeasureValueList.GetValueSafe("KG"));
				AssertEquals("[KGM]", 2000m, calcData.UnitOfMeasureValueList.GetValueSafe("KGM"));
				AssertEquals("[TNE]", 2m, calcData.UnitOfMeasureValueList.GetValueSafe("TNE"));
				AssertEquals("[DTN]", 20m, calcData.UnitOfMeasureValueList.GetValueSafe("DTN"));
				AssertEquals("[L]", 100000m, calcData.UnitOfMeasureValueList.GetValueSafe("L"));
				AssertEquals("[MTQ]", 100m, calcData.UnitOfMeasureValueList.GetValueSafe("MTQ"));
				AssertEquals("[PKG]", 10m, calcData.UnitOfMeasureValueList.GetValueSafe("PKG"));
				AssertEquals("[PRC]", 150m, calcData.CountrySpecificValueList.GetValueSafe("PRC"));
			});
		}

		#region ConditionCalcDataForInvoiceLineForTest

		class ConditionCalcDataForInvoiceLineForTest : ConditionCalcDataForInvoiceLine
		{
			public ConditionCalcDataForInvoiceLineForTest(BaseJobComInvoiceLine invoiceLine) : base(invoiceLine)
			{
			}

			protected override void AddCountrySpecificValues(IDictionary<string, decimal> countrySpecificValueList)
			{
				AddToDictionaryIfNotExists(countrySpecificValueList, "PRC", InvoiceLine.JI_LinePrice);
			}

			protected override IEnumerable<ZString> CustomsUQList => new ZString[] { "KGM", "TNE", "DTN", "L", "HLT" };

			readonly Dictionary<ZString, ZString> weightUQs = new Dictionary<ZString, ZString> {
				{ "KGM", Core.Constants.Weight.Kilograms },
				{ "TNE", Core.Constants.Weight.Tonnes },
				{ "DTN", Core.Constants.Weight.Decitons }
			 };
			readonly Dictionary<ZString, ZString> volumnUQs = new Dictionary<ZString, ZString> {
				{ "L", Core.Constants.Volume.Litre },
				{ "MTQ", Core.Constants.Volume.CubicMetres }
			 };

			protected override bool CustomsUnitConvertible(ZString fromUQ, ZString toUQ)
			{
				return (Core.Constants.Weight.ContainsCode(fromUQ) || Enumerable.Contains(weightUQs.Keys, fromUQ)) && Enumerable.Contains(weightUQs.Keys, toUQ)
					|| (Core.Constants.Volume.ContainsCode(fromUQ) || Enumerable.Contains(volumnUQs.Keys, fromUQ)) && Enumerable.Contains(volumnUQs.Keys, toUQ);
			}

			protected override ZDecimal CustomsUnitConvert(ZDecimal qty, ZString fromUQ, ZString toUQ)
			{
				if ((Core.Constants.Weight.ContainsCode(fromUQ) || Enumerable.Contains(weightUQs.Keys, fromUQ)) && Enumerable.Contains(weightUQs.Keys, toUQ))
				{
					if (Enumerable.Contains(weightUQs.Keys, fromUQ))
					{
						fromUQ = weightUQs[fromUQ];
					}
					if (Enumerable.Contains(weightUQs.Keys, toUQ))
					{
						toUQ = weightUQs[toUQ];
					}
					return Core.Constants.Weight.Convert(qty, fromUQ, toUQ);
				}

				if ((Core.Constants.Volume.ContainsCode(fromUQ) || Enumerable.Contains(volumnUQs.Keys, fromUQ)) && Enumerable.Contains(volumnUQs.Keys, toUQ))
				{
					if (Enumerable.Contains(volumnUQs.Keys, fromUQ))
					{
						fromUQ = volumnUQs[fromUQ];
					}
					if (Enumerable.Contains(volumnUQs.Keys, toUQ))
					{
						toUQ = volumnUQs[toUQ];
					}
					return Core.Constants.Volume.Convert(qty, fromUQ, toUQ);
				}

				return 0m;
			}
		}

		#endregion
	}
}
