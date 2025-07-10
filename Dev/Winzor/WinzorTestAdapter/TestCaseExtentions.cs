using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace WinzorTestAdapter
{
	internal static class TestCaseExtensions
	{
		public static bool IsExplicit(this TestCase testCase) => testCase.Traits.Any(t => t.Name == "Explicit");
	}
}
