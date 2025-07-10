using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CBMATaxRate : ICodeDescription
	{
		public CBMATaxRate(string taxCode, string code, string description, decimal rate, decimal ttbConfirmationRate)
		{
			TaxCode = taxCode;
			Code = code;
			Description = description;
			Rate = rate;
			TTBConfirmationRate = ttbConfirmationRate;
		}
		public string TaxCode { get; private set; }
		public string Code { get; private set; }
		public string Description { get; private set; }
		public decimal Rate { get; private set; }
		public decimal TTBConfirmationRate { get; private set; }
		object ICodeDescription.PK => null;
	}

	public class CBMATaxRateList : CodeDescriptionPairList
	{
		public CBMATaxRateList()
		{
			Add(new CBMATaxRate("022", "B01010", "Imported Beer (Other Beer) First 6,000,000 BBLs", 0.1363469m, 16m));
			Add(new CBMATaxRate("017", "W01010", "Still Wine. 16% and under alcohol by volume (0.392g CO2/100mL or less) First 30,000 wine gallons", 0.01849200m, 0.07m));
			Add(new CBMATaxRate("017", "W01020", "Still Wine. 16% and under alcohol by volume (0.392g CO2/100mL or less) Over 30,000 up to 130,000 wine gallons", 0.04490920m, 0.17m));
			Add(new CBMATaxRate("017", "W01030", "Still Wine. 16% and under alcohol by volume (0.392g CO2/100mL or less) Over 130,000 up to 750,000 wine gallons", 0.14133200m, 0.535m));
			Add(new CBMATaxRate("017", "W02010", "Still Wine. Over 16 - 21% alcohol by volume (0.392g CO2/100mL or less) First 30,000 wine gallons", 0.15057810m, 0.57m));
			Add(new CBMATaxRate("017", "W02020", "Still Wine. Over 16 - 21% alcohol by volume (0.392g CO2/100mL or less) Over 30,000 up to 130,000 wine gallons", 0.17699530m, 0.67m));
			Add(new CBMATaxRate("017", "W02030", "Still Wine. Over 16 - 21% alcohol by volume (0.392g CO2/100mL or less) Over 130,000 up to 750,000 wine gallons", 0.27341800m, 1.035m));
			Add(new CBMATaxRate("017", "W03010", "Still Wine. Over 21 - 24% alcohol by volume (0.392g CO2/100mL or less) First 30,000 wine gallons", 0.56796990m, 2.15m));
			Add(new CBMATaxRate("017", "W03020", "Still Wine. Over 21 - 24% alcohol by volume (0.392g CO2/100mL or less) Over 30,000 up to 130,000 wine gallons", 0.59438710m, 2.25m));
			Add(new CBMATaxRate("017", "W03030", "Still Wine. Over 21 - 24% alcohol by volume (0.392g CO2/100mL or less) Over 130,000 up to 750,000 wine gallons", 0.69081000m, 2.615m));
			Add(new CBMATaxRate("017", "W04010", "Still Wine. Mead. No more than 0.64g CO2/100mL; derived solely from honey and water; containing no fruit product or fruit flavoring; and containing less than (not equal to) 8.5% alcohol by volume First 30,000 wine gallons", 0.01849200m, 0.07m));
			Add(new CBMATaxRate("017", "W04020", "Still Wine. Mead. No more than 0.64g CO2/100mL; derived solely from honey and water; containing no fruit product or fruit flavoring; and containing less than (not equal to) 8.5% alcohol by volume Over 30,000 up to 130,000 wine gallons", 0.04490920m, 0.17m));
			Add(new CBMATaxRate("017", "W04030", "Still Wine. Mead. No more than 0.64g CO2/100mL; derived solely from honey and water; containing no fruit product or fruit flavoring; and containing less than (not equal to) 8.5% alcohol by volume Over 130,000 up to 750,000 wine gallons", 0.14133200m, 0.535m));
			Add(new CBMATaxRate("017", "W05010", "Still Wine. Low alcohol by volume wine. No more than 0.64g CO2/100mL; derived primarily from grapes or from grape juice concentrate and water; containing no fruit product or fruit flavoring other than grape; and containing less than (not equal to) 8.5% alcohol by volume  First 30,000 wine gallons", 0.01849200m, 0.07m));
			Add(new CBMATaxRate("017", "W05020", "Still Wine. Low alcohol by volume wine. No more than 0.64g CO2/100mL; derived primarily from grapes or from grape juice concentrate and water; containing no fruit product or fruit flavoring other than grape; and containing less than (not equal to) 8.5% alcohol by volume  Over 30,000 up to 130,000 wine gallons", 0.04490920m, 0.17m));
			Add(new CBMATaxRate("017", "W05030", "Still Wine. Low alcohol by volume wine. No more than 0.64g CO2/100mL; derived primarily from grapes or from grape juice concentrate and water; containing no fruit product or fruit flavoring other than grape; and containing less than (not equal to) 8.5% alcohol by volume  Over 130,000 up to 750,000 wine gallons", 0.14133200m, 0.535m));
			Add(new CBMATaxRate("017", "W06010", "Artificially Carbonated Wine. Over 0.392g CO2/100mL - injected or otherwise added First 30,000 wine gallons", 0.60759570m, 2.3m));
			Add(new CBMATaxRate("017", "W06020", "Artificially Carbonated Wine. Over 0.392g CO2/100mL - injected or otherwise added Over 30,000 up to 130,000 wine gallons", 0.63401290m, 2.4m));
			Add(new CBMATaxRate("017", "W06030", "Artificially Carbonated Wine. Over 0.392g CO2/100mL - injected or otherwise added Over 130,000 up to 750,000 wine gallons", 0.73043570m, 2.765m));
			Add(new CBMATaxRate("017", "W07010", "Sparkling Wine. Over 0.392g CO2/100mL - naturally occurring First 30,000 wine gallons", 0.63401290m, 2.4m));
			Add(new CBMATaxRate("017", "W07020", "Sparkling Wine. Over 0.392g CO2/100mL - naturally occurring Over 30,000 up to 130,000 wine gallons", 0.66043010m, 2.5m));
			Add(new CBMATaxRate("017", "W07030", "Sparkling Wine. Over 0.392g CO2/100mL - naturally occurring Over 130,000 up to 750,000 wine gallons", 0.75685290m, 2.865m));
			Add(new CBMATaxRate("017", "W08010", "Hard Cider. No more than 0.64g CO2/100mL; derived primarily from apples/pears or apple/pear juice concentrate and water; containing no other fruit product or fruit flavoring other than apple/pear; and containing at least 0.5% and less than (not equal to) 8.5% alcohol by volume First 30,000 wine gallons", 0.04332420m, 0.164m));
			Add(new CBMATaxRate("017", "W08020", "Hard Cider. No more than 0.64g CO2/100mL; derived primarily from apples/pears or apple/pear juice concentrate and water; containing no other fruit product or fruit flavoring other than apple/pear; and containing at least 0.5% and less than (not equal to) 8.5% alcohol by volume Over 30,000 up to 130,000 wine gallons", 0.04490920m, 0.17m));
			Add(new CBMATaxRate("017", "W08030", "Hard Cider. No more than 0.64g CO2/100mL; derived primarily from apples/pears or apple/pear juice concentrate and water; containing no other fruit product or fruit flavoring other than apple/pear; and containing at least 0.5% and less than (not equal to) 8.5% alcohol by volume Over 130,000 up to 750,000 wine gallons", 0.05098520m, 0.193m));
			Add(new CBMATaxRate("016", "S01010", "Distilled spirits First 100,000 proof gallons", 0.71326450m, 2.7m));
			Add(new CBMATaxRate("016", "S01020", "Distilled spirits Over 100,000 up to 22,230,000 proof gallons", 3.52405520m, 13.34m));
		}

		public static CodeDescriptionPairList GetList(BusinessObjectFactory factory, string taxCode, string taxRate = "")
		{
			return factory.GetCachedValue(string.Join("_", "CBMATaxRateList", taxRate, taxCode), () => string.IsNullOrEmpty(taxRate) ? GeneratePairListByTaxCode(taxCode) : GeneratePairListByTaxRate(taxRate, taxCode));
		}

		static CodeDescriptionPairList GeneratePairListByTaxCode(string taxCode)
		{
			var list = new CBMATaxRateList();
			var result = new CodeDescriptionPairList();
			foreach (var record in list.Cast<CBMATaxRate>().Where(x => x.TaxCode.Equals(taxCode, StringComparison.OrdinalIgnoreCase)))
			{
				result.Add(record);
			}
			return result;
		}

		static CodeDescriptionPairList GeneratePairListByTaxRate(string taxRate, string taxCode)
		{
			CodeDescriptionPairList result;

			if (AppendixBTaxRateList.Codes.Wines_2 == taxRate)
			{
				result = SelectSpecificRateCode("W01010", "W01020", "W01030", "W04010", "W04020", "W04030", "W05010", "W05020", "W05030");
			}
			else if (AppendixBTaxRateList.Codes.Wines_3 == taxRate)
			{
				result = SelectSpecificRateCode("W02010", "W02020", "W02030");
			}
			else if (AppendixBTaxRateList.Codes.Wines_4 == taxRate)
			{
				result = SelectSpecificRateCode("W03010", "W03020", "W03030");
			}
			else if (AppendixBTaxRateList.Codes.Wines_6 == taxRate)
			{
				result = SelectSpecificRateCode("W06010", "W06020", "W06030");
			}
			else if (AppendixBTaxRateList.Codes.Wines_5 == taxRate)
			{
				result = SelectSpecificRateCode("W07010", "W07020", "W07030");
			}
			else if (AppendixBTaxRateList.Codes.Other_4 == taxRate)
			{
				result = SelectSpecificRateCode("S01010", "S01020");
			}
			else if (AppendixBTaxRateList.Codes.Other_3 == taxRate)
			{
				result = SelectSpecificRateCode("B01010");
			}
			else
			{
				result = GeneratePairListByTaxCode(taxCode);
			}
			return result;

			CodeDescriptionPairList SelectSpecificRateCode(params string[] taxRates)
			{
				var list = new CBMATaxRateList();
				var result = new CodeDescriptionPairList();
				foreach (var record in list.Cast<CBMATaxRate>().Where(x => taxRates.Contains(x.Code, StringComparer.OrdinalIgnoreCase)))
				{
					result.Add(record);
				}
				return result;
			}
		}
	}
}
