using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses
{
	internal class SARSFormulaHelperForTest
	{
		public static bool PopulateFormulaData(TariffData tariff, Rate rate)
		{
			if (Regex.IsMatch(rate.Description, @"^\(C([0-9]{4})"))
			{
				var firstSpace = rate.Description.IndexOf(' ');
				rate.Description = rate.Description.Substring(firstSpace + 1, rate.Description.Length - firstSpace - 1);
			}

			return SARSFormulaHelper.PopulateFormulaData(tariff, rate);
		}

		public static List<(int regCode, string regEx, string methodName)> GetAllFormulas()
		{
			var result = new List<(int regCode, string regEx, string methodName)>();

			result.AddRange(SARSFormulaHelper.Actions1001().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1001))));
			result.AddRange(SARSFormulaHelper.Actions1216().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1216))));
			result.AddRange(SARSFormulaHelper.Actions1302().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1302))));
			result.AddRange(SARSFormulaHelper.Actions1352().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1352))));
			result.AddRange(SARSFormulaHelper.Actions1354().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1354))));
			result.AddRange(SARSFormulaHelper.Actions1556().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1556))));
			result.AddRange(SARSFormulaHelper.Actions1564().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1564))));
			result.AddRange(SARSFormulaHelper.Actions1600().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions1600))));
			result.AddRange(SARSFormulaHelper.Actions3408().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions3408))));
			result.AddRange(SARSFormulaHelper.Actions3410().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions3410))));
			result.AddRange(SARSFormulaHelper.Actions3423().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions3423))));
			result.AddRange(SARSFormulaHelper.Actions3425().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions3425))));
			result.AddRange(SARSFormulaHelper.Actions3436().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions3436))));
			result.AddRange(SARSFormulaHelper.Actions3440().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions3440))));
			result.AddRange(SARSFormulaHelper.Actions4555().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions4555))));
			result.AddRange(SARSFormulaHelper.Actions4558().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions4558))));
			result.AddRange(SARSFormulaHelper.Actions4559().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions4559))));
			result.AddRange(SARSFormulaHelper.Actions4560().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions4560))));
			result.AddRange(SARSFormulaHelper.Actions4567().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions4567))));
			result.AddRange(SARSFormulaHelper.Actions4570().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions4570))));
			result.AddRange(SARSFormulaHelper.Actions9999().Select(x => (x.Item1, x.Item2, nameof(SARSFormulaHelper.Actions9999))));

			return result;
		}
	}
}
