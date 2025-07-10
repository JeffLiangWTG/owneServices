using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Rating.Business
{
	internal sealed class PricingPageRateLineListTest : TestCaseWithFactory
	{
		public void TestGetSetContainerTypes()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry("FCL");
			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			var line2 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);

			var set = new PricingPageRateLineList();
			set.Add(line1);
			set.Add(line2);

			AssertEquals("", set.GetContainerType(line1));
			AssertEquals("", set.GetContainerType(line2));

			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20RE");

			AssertEquals("20GP", set.GetContainerType(line1));
			AssertEquals("20RE", set.GetContainerType(line2));

			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20GP");

			AssertEquals("20GP", set.GetContainerType(line1));
			AssertEquals("20RE, 20GP", set.GetContainerType(line2));
		}

		public void TestContainsSameLines()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry("FCL");
			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			var line2 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			var line3 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line1.TL_RateDesc = "Freight";
			line2.TL_RateDesc = "Freight";
			line3.TL_RateDesc = "Freight";

			var set1 = new PricingPageRateLineList();
			set1.Add(line1);
			set1.Add(line2);

			var set2 = new PricingPageRateLineList();
			set2.Add(line1);
			set2.Add(line2);

			var set3 = new PricingPageRateLineList();
			set3.Add(line1);
			set3.Add(line2);
			set3.Add(line3);

			var set4 = new PricingPageRateLineList();
			set4.Add(line1);
			set4.Add(line3);

			var sameSets = new PricingPageRateLineList[] { set1, set2 };
			var differentSets = new PricingPageRateLineList[] { set1, set3, set4 };

			for (var i = 0; i < sameSets.Length; i++)
			{
				AssertEquals(string.Format("sameSets[0].ContainsSameLines(sameSets[{0}])", i), true, sameSets[0].ContainsSameLinesAs(sameSets[i]));
				AssertEquals(string.Format("sameSets[{0}].ContainsSameLines(sameSets[0])", i), true, sameSets[i].ContainsSameLinesAs(sameSets[0]));
			}

			for (var i = 1; i < differentSets.Length; i++)
			{
				AssertEquals(string.Format("differentSets[0].ContainsSameLines(differentSets[{0}])", i), false, differentSets[0].ContainsSameLinesAs(differentSets[i]));
				AssertEquals(string.Format("differentSets[{0}].ContainsSameLines(differentSets[0])", i), false, differentSets[i].ContainsSameLinesAs(differentSets[0]));
			}
		}

		public void TestRemoveDuplicateSets()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "40GP");

			var line1a = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line1a.TL_RateDesc = "Freight1a";

			var line1b = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line1b.TL_RateDesc = "Freight1b";

			var line2a = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line2a.TL_RateDesc = "Freight2a";

			var line2b = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line2b.TL_RateDesc = "Freight2b";

			var list = new List<PricingPageRateLineList>()
			{
				new PricingPageRateLineList()
				{
					line1a,
					line1b,
				},
				new PricingPageRateLineList()
				{
					line2a,
					line2b,
				},
				new PricingPageRateLineList()
				{
					line1a,
					line2a,
				},
				new PricingPageRateLineList()
				{
					line1b,
					line2b,
				},
				new PricingPageRateLineList()
				{
					line1a,
				},
				new PricingPageRateLineList()
				{
					line1a,
					line1b,
				},
				new PricingPageRateLineList()
				{
					line1a,
					line1b,
					line2a,
					line2b,
				},
			};

			PricingPageRateLineList.RemoveDuplicateRateLineInSets(list);

			const string expected = @"
[ShowEquipmentType = False]
FCL|AU->NL|SEA||FRT|Freight1a|UNT,,,0,|20GP
FCL|AU->NL|SEA||FRT|Freight1b|UNT,,,0,|20GP

[ShowEquipmentType = False]
FCL|AU->NL|SEA||FRT|Freight2a|UNT,,,0,|40GP
FCL|AU->NL|SEA||FRT|Freight2b|UNT,,,0,|40GP
";

			AssertMultilineASCIIEquals("", expected, Render(list));
		}

		#region Implementation

		static string Render(IEnumerable<PricingPageRateLineList> list)
		{
			var builder = new StringBuilder();

			foreach (var set in list)
			{
				builder.AppendLine(Render(set));
			}

			return builder.ToString();
		}

		static string Render(PricingPageRateLineList set)
		{
			var builder = new StringBuilder();
			var lines = new List<string>();

			foreach (var line in set)
			{
				var entry = line.Parent;

				builder.Append(entry.TI_RateCategory);
				builder.Append("|");

				builder.Append(entry.TI_OriginLRC);
				builder.Append("->");

				if (!entry.TI_ViaLRC.IsEmpty)
				{
					builder.Append(entry.TI_ViaLRC);
					builder.Append("->");
				}

				builder.Append(entry.TI_DestinationLRC);
				builder.Append("|");
				builder.Append(entry.TI_Mode);
				builder.Append("|");
				builder.Append(entry.TI_RS_NKServiceLevel_NI);
				builder.Append("|");
				builder.Append(line.ChargeCode.AC_Code);
				builder.Append("|");
				builder.Append(line.TL_RateDesc);
				builder.Append("|");
				builder.Append(line.TL_RateCalculator);
				builder.Append(",");
				builder.Append(line.Calculator[FlatCalculator.Items.Operator.BAS]);
				builder.Append(",");
				builder.Append(line.Calculator[FlatCalculator.Items.Operator.MIN]);
				builder.Append(",");
				builder.Append(line.Calculator[FlatCalculator.Items.Operator.UNT]);
				builder.Append(",");
				builder.Append(line.Calculator[FlatCalculator.Items.Operator.MAX]);
				builder.Append("|");
				builder.Append(set.GetContainerType(line));

				if (!line.Parent.TI_ContractNumber.IsEmpty)
				{
					builder.Append(" <");
					builder.Append(line.Parent.TI_ContractNumber);
					builder.Append(">");
				}

				lines.Add(builder.ToString());
				builder.Length = 0;
			}

			lines.Sort();

			builder.AppendLine();
			builder.Append("[ShowEquipmentType = ");
			builder.Append(set.ShowEquipmentType.ToString());
			builder.Append("]");

			foreach (var line in lines)
			{
				builder.AppendLine();
				builder.Append(line);
			}

			return builder.ToString();
		}

		#endregion
	}
}
