#pragma warning disable CW1161 // This is a test file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay;
using NUnit.Framework;
using WTG.TestHelpers.SpecTesting;

namespace Enterprise.MasterFiles.Business.SpecTesting
{
	public class OrgCusCodeData
	{
		[TableColumnName]
		public string Code { get; set; }
		[TableColumnName]
		public string Description { get; set; }
		[TableColumnName("Is main")]
		public string IsMain { get; set; } // "y" or ""
		[TableColumnName("Is primary")]
		public string IsPrimary { get; set; } // "y" or ""

		public override bool Equals(object obj)
		{
			return obj is OrgCusCodeData data &&
				   Code == data.Code &&
				   Description == data.Description &&
				   IsMain == data.IsMain &&
				   IsPrimary == data.IsPrimary;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 1945373678;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IsMain);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IsPrimary);
				return hashCode;
			}
		}
	}

	class OrgCusCodeDiff
	{
		public OrgCusCodeData[] Added { get; set; }
		public OrgCusCodeData[] Removed { get; set; }
	}

	public class OrgCusCodeSpecGenerator : ISpecProvider
	{
		public string SpecFolderPath => "Enterprise/Product/Operations/MasterFiles/Business/MasterFiles.Business.SpecTesting/Spec/Countries/OrgCusCodeInfoStatic";
		public string SpecNamespacePrefix => "Enterprise.MasterFiles.Business.SpecTesting.Spec.Countries.OrgCusCodeInfoStatic";
		public string RegenExecutableName => "MasterFiles.Business.SpecTesting.Regenerate.exe";

		string[] GetSpecTestedCountries()
		{
			var collection = new RefCountryCollection(new BusinessObjectFactory());
			var result = collection.Select(rc => rc.Code.ToString()).OrderBy(c => c).ToList();
			result.Add("__");
			return result.ToArray();
		}

		OrgCusCodeData[] GetRefDbDependentCusCodesForCountry(string countryCode)
		{
			var codes = new List<OrgCusCodeData>();

			// "__" means null country, which internally is also ""
			var display = new CountryComplianceInfoDisplay(countryCode == "__" ? "" : countryCode);

			foreach (var code in display.OrgCusCodeTypes)
			{
				if (code is OrgCusCodeTypeDisplay orgCusCodeDisplay)
				{
					var codeData = new OrgCusCodeData()
					{
						Code = orgCusCodeDisplay.Code,
						Description = orgCusCodeDisplay.Description,
						IsMain = orgCusCodeDisplay.Main ? "Y" : "",
						IsPrimary = orgCusCodeDisplay.Primary ? "Y" : "",
					};

					codes.Add(codeData);
				}
				else
				{
					throw new Exception("Expected CountryComplianceInfoDisplay to have a list of OrgCusCodeTypeDisplay as of writing");
				}
			}

			return codes.OrderBy(code => code.Code).ToArray();
		}

		Dictionary<T, int> ArrayToCounts<T>(T[] array)
		{
			var counts = new Dictionary<T, int>();
			foreach (var item in array)
			{
				counts.TryGetValue(item, out var count);
				counts[item] = count + 1;
			}
			return counts;
		}

		OrgCusCodeDiff GetCusCodeDiffBetweenLists(OrgCusCodeData[] list1, OrgCusCodeData[] list2)
		{
			// Convert the arrays to a dictionary of counts
			var counts1 = ArrayToCounts(list1);
			var counts2 = ArrayToCounts(list2);

			// Get a dictionary difference of the counts
			var addedDict = counts2.ToDictionary(x => x.Key, x => x.Value - (counts1.TryGetValue(x.Key, out var count) ? count : 0));
			var removedDict = counts1.ToDictionary(x => x.Key, x => x.Value - (counts2.TryGetValue(x.Key, out var count) ? count : 0));

			// Convert the dictionaries to arrays, respecting the counts
			var added = new List<OrgCusCodeData>();
			var removed = new List<OrgCusCodeData>();

			foreach (var item in addedDict)
			{
				for (int i = 0; i < item.Value; i++)
				{
					added.Add(item.Key);
				}
			}

			foreach (var item in removedDict)
			{
				for (int i = 0; i < item.Value; i++)
				{
					removed.Add(item.Key);
				}
			}

			return new OrgCusCodeDiff
			{
				Added = added.OrderBy(code => code.Code).ToArray(),
				Removed = removed.OrderBy(code => code.Code).ToArray()
			};
		}

		string CommentLines(string text)
		{
			return string.Join("\n", text.Split('\n').Select(t => $"# {t}"));
		}

		// This function uses 2 choke points, because it is difficult to retrieve the main/primary booleans for codes without getting all refdb codes as well.
		// In the future, the choke point should be refactored to be more static, and this function can be improved.
		string GetCusCodesTableStringForCountry(string countryCode)
		{
			// Get all cus codes from the choke point, and then only keep the static ones
			var allCodes = GetRefDbDependentCusCodesForCountry(countryCode);

			// Only keep the codes contained within the static codes list
			var refCountry = new RefCountry.Loader(new BusinessObjectFactory()).LoadForCountry(countryCode);
			var staticCodes = new OrgCodeLists().CustomsCodes_StaticList(refCountry).GetAllCodes();
			var staticOnlyCodes = staticCodes.Select(code => allCodes.Where(c => c.Code == code).First()).OrderBy(code => code.Code).ToArray();

			var staticTable = SpecTableGen.ConvertArrayToTable(staticOnlyCodes);

			var diff = GetCusCodeDiffBetweenLists(staticOnlyCodes, allCodes);

			var resultingSpecString = @$"# Lines starting with # are comments and ignored in the spec

# Static OrgCusCodes for {countryCode}, that don't depend on RefDB:
{staticTable}
";

			if (diff.Added.Length > 0)
			{
				var addedTable = CommentLines(SpecTableGen.ConvertArrayToTable(diff.Added));
				resultingSpecString += $@"
# OrgCusCodes added by RefDB-dependent code:
{addedTable}
";
			}

			if (diff.Removed.Length > 0)
			{
				var removedTable = CommentLines(SpecTableGen.ConvertArrayToTable(diff.Removed));
				resultingSpecString += $@"
# OrgCusCodes removed by RefDB-dependent code:
{removedTable}
";
			}

			return resultingSpecString;
		}

		public IEnumerable<SpecFile> GetSpecFiles()
		{
			var allCountries = GetSpecTestedCountries();
			foreach (var country in allCountries)
			{
				var filename = $"{country}.txt";
				var countryData = GetCusCodesTableStringForCountry(country);
				yield return new SpecFile(filename, countryData);
			}
		}
	}

	public class OrgCusCodesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestStaticCountrySpecNotBroken()
		{
			SpecAssert.AssertAllSpecMatches(Assembly.GetExecutingAssembly(), new OrgCusCodeSpecGenerator());
		}
	}
}
