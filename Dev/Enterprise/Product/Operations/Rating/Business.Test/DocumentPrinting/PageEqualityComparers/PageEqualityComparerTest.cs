using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class PageEqualityComparerTest<T> : TestCaseWithFactory
		where T : PageEqualityComparer, new()
	{
		#region Implementation

		protected void AssertEqualityGrouping(RateEntry[][] arrayOfGroups)
		{
			var result = new Dictionary<string, IList<string>>();
			var errors = new List<string>();

			for (var i = 0; i < arrayOfGroups.Length; i++)
			{
				var groupedEntries1 = arrayOfGroups[i];

				for (var j = 0; j < groupedEntries1.Length; j++)
				{
					// Equality Checks

					var label1 = string.Format("arrayOfGroups[{0}][{1}]", i, j);

					for (var k = j; k < groupedEntries1.Length; k++) // we DO want to compare against self
					{
						var label2 = string.Format("arrayOfGroups[{0}][{1}]", i, k);

						CheckMatch(errors, label1, label2, groupedEntries1[j], groupedEntries1[k]);

						if (errors.Count > 0)
						{
							result.Add(label1 + " == " + label2, errors);
							errors = new List<string>();
						}
					}

					// Inequality Checks

					for (var l = i + 1; l < arrayOfGroups.Length; l++)
					{
						var groupedEntries2 = arrayOfGroups[l];

						for (var k = 0; k < groupedEntries2.Length; k++)
						{
							var label2 = string.Format("arrayOfGroups[{0}][{1}]", l, k);

							CheckNoMatch(errors, label1, label2, groupedEntries1[j], groupedEntries2[k]);

							if (errors.Count > 0)
							{
								result.Add(label1 + " != " + label2, errors);
								errors = new List<string>();
							}
						}
					}
				}
			}

			AssertGroupedErrorList(result);
		}

		void CheckMatch(List<string> errors, string label1, string label2, RateEntry entry1, RateEntry entry2)
		{
			if (!Comparer.Equals(entry1, entry2))
			{
				errors.Add(string.Format("Comparer.Equals({0}, {1}) should be true", label1, label2));
			}

			if (!Comparer.Equals(entry2, entry1))
			{
				errors.Add(string.Format("Comparer.Equals({0}, {1}) should be true", label2, label1));
			}

			if (Comparer.GetHashCode(entry1) != Comparer.GetHashCode(entry2))
			{
				errors.Add(string.Format("Comparer.GetHashCode({0}) should equal Comparer.GetHashCode({1})\r\n({2}, {3})", label1, label2, Comparer.GetHashCode(entry1), Comparer.GetHashCode(entry2)));
			}
		}

		void CheckNoMatch(List<string> errors, string label1, string label2, RateEntry entry1, RateEntry entry2)
		{
			if (Comparer.Equals(entry1, entry2))
			{
				errors.Add(string.Format("Comparer.Equals({0}, {1}) should be false", label1, label2));
			}

			if (Comparer.Equals(entry2, entry1))
			{
				errors.Add(string.Format("Comparer.Equals({0}, {1}) should be false", label2, label1));
			}

			if (Comparer.GetHashCode(entry1) == Comparer.GetHashCode(entry2))
			{
				errors.Add(string.Format("Comparer.GetHashCode({0}) not be the same as Comparer.GetHashCode({1})\r\n({2}, {3})", label1, label2, Comparer.GetHashCode(entry1), Comparer.GetHashCode(entry2)));
			}
		}

		protected T Comparer
		{
			get { return comparer ?? (comparer = new T()); }
		}
		T comparer;

		#endregion
	}
}
